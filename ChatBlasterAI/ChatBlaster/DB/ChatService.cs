using ChatBlaster.Infrastructure;
using ChatBlaster.Models;
using Microsoft.EntityFrameworkCore;
using WebSocketSharp;
using ChatBlaster.Utilities;
using System.Security.Policy;
using System.Xml.Linq;
namespace ChatBlaster.DB
{
    public class ChatService : IDisposable
    {
        private readonly DbContextOptions<ChatContext> _options;
        public DbSet<Avatar> Avatars => _context.Avatars;
        public readonly ChatContext _context;
        public ChatService(DbContextOptions<ChatContext> options)
        {
            _options = options;
            _context = new ChatContext(_options);
        }
        /***************************************************************************/
        public ChatContext Ctx() => new ChatContext(_options);
        /***************************************************************************/
        private ChatContext NewCtx() => new ChatContext(_options);
        /***************************************************************************/
        public void EnsureDatabase()
        {
            using var ctx = NewCtx();
            ctx.Database.Migrate();
            var avatarsList = ctx.Avatars.ToList();
            ProxyManager.Initialize(avatarsList, this);
        }
        public void Dispose() => _context?.Dispose();
        /*********************************************************************************/
        /* ───────────────────────────────  Add / Create  ────────────────────────────── */
        /*********************************************************************************/
        public Company AddCompany(string name, string owner, string ownerPhone, string officePhone)
        {
            using (var db = Ctx())
            {
                var c = new Company
                {
                    Name = name,
                    OwnerName = owner,
                    OwnerPhone = ownerPhone,
                    OfficePhone = officePhone
                };
                db.Companies.Add(c);
                db.SaveChanges();
                return c;
            }
        }
        /***************************************************************************/
        public ServiceArea AddServiceArea(string city, string state, string zip_code, string s_cities, int radius, int companyId)
        {
            using (var db = Ctx())
            {
                var a = new ServiceArea { City = city, State = state, ZipCode = zip_code, SurroundingCities = s_cities, Radius = radius, CompanyId = companyId };
                db.ServiceAreas.Add(a);
                db.SaveChanges();
                return a;
            }

        }
        /***************************************************************************/
        public Service AddService(string industry, string phone_number, string facebook_category, string service_area_id)
        {
            using (var db = Ctx())
            {

                var s = new Service { Industry = industry, PhoneNumber = phone_number, FacebookCategory = facebook_category, ServiceAreaId = service_area_id };
                db.Services.Add(s);
                db.SaveChanges();
                return s;
            }
        }
        /***************************************************************************/
        public Avatar AddAvatar(string email, string userName, string password, string PhotoFolder, string serviceId)
        {
            using (var db = Ctx())
            {
                int port = PortAllocator.Next();
                Service service = GetServiceById(serviceId);
                ServiceArea service_area = GetServiceAreaById(service.ServiceAreaId);
                var avatar = new Avatar(userName, email, password, PhotoFolder, service_area.City, this)
                {
                    ServiceId = serviceId
                };
                avatar.ProxyAddress = ProxyManager.GetProxyForAvatar(avatar);
                db.Avatars.Add(avatar);
                db.SaveChanges();
                return avatar;
            }

        }
        /***************************************************************************/
        public IReadOnlyList<Avatar> AddAvatarsBulk(
            IEnumerable<(string Email, string UserName, string Password, string ServiceId,
                         string Number, string Id, string TwoFa, string photo_path)> rows,
            IProgress<int>? progress = null)
        {

            using var db = Ctx();
            var rowsList = rows.ToList();
            if (ProxyManager.AvailableCount < rowsList.Count)
                throw new InvalidOperationException("Not enough proxies available for bulk import");
            var list = new List<Avatar>();
            int done = 0;

            foreach (var r in rowsList)
            {
                Service service = GetServiceById(r.ServiceId);
                ServiceArea service_area = GetServiceAreaById(service.ServiceAreaId);
                var avatar = new Avatar(string.IsNullOrWhiteSpace(r.UserName) ? $"user{r.Number}" : r.UserName,
                                        r.Email,
                                        r.Password, r.photo_path, service_area.City,
                                        this)
                {
                    ServiceId = r.ServiceId,
                    _twoFactorHash = r.TwoFa
                };
                avatar.ProxyAddress = ProxyManager.GetProxyForAvatar(avatar);
                list.Add(avatar);
                progress?.Report(++done);
            }

            db.Avatars.AddRange(list);
            db.SaveChanges();
            return list;
        }
        /***************************************************************************/
        public Conversation StartConversation(Conversation conversation)
        {
            if (string.IsNullOrEmpty(conversation.ConversationId))
                throw new ArgumentException("Conversation ID must be a valid integer.", nameof(conversation.ConversationId));
            using (var db = Ctx())
            {
                var conv = new Conversation
                {
                    ConversationId = conversation.ConversationId,
                    _conversationLink = conversation._conversationLink,
                    AvatarId = conversation.AvatarId,
                    CreatedAt = DateTime.Now
                };
                try
                {
                    db.Conversations.Add(conv);
                    db.SaveChanges();
                    return conv;
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException($"Failed to start conversation with ID '{conversation.ConversationId}': {ex.Message}", ex);
                }
            }
        }
        /***************************************************************************/
        public ChatBlaster.Models.Message AddMessage(string conversationId, string sender, string text)
        {
            using (var db = Ctx())
            {
                var conv = db.Conversations
                .Include(c => c.Messages)
                .FirstOrDefault(c => c.ConversationId == conversationId); // Changed from ConversationId to Id
                if (conv == null)
                    throw new ArgumentException($"Conversation with ID '{conversationId}' not found.");
                var msg = new ChatBlaster.Models.Message
                {
                    ConversationId = conversationId,
                    Sender = sender,
                    Text = text,
                    Timestamp = DateTime.Now
                };
                bool isDuplicate = conv.Messages.Any(m => m.Text == text);
                if (isDuplicate)
                    return msg;
                try
                {
                    db.Messages.Add(msg);
                    db.SaveChanges();
                    return msg;
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException($"Failed to add message to conversation '{conversationId}': {ex.Message}", ex);
                }
            }
        }
        /***************************************************************************/
        public ServiceArea CreateServiceArea(string city, string state, string zip, double radius, int companyId, string surroundingCities)
        {
            using (var db = Ctx())
            {
                var serviceArea = new ServiceArea
                {
                    SurroundingCities = surroundingCities,
                    City = city,
                    State = state,
                    ZipCode = zip,
                    Radius = int.Parse(radius.ToString()),
                    CompanyId = companyId
                };
                db.ServiceAreas.Add(serviceArea);
                db.SaveChanges();
                return serviceArea;
            }
        }
        /***************************************************************************/
        public Service CreateService(string industry, string phoneNumber, string facebook_category, string serviceAreaId)
        {
            using (var db = Ctx())
            {
                var service = new Service
                {
                    Industry = industry,
                    PhoneNumber = phoneNumber,
                    ServiceAreaId = serviceAreaId,
                    FacebookCategory = facebook_category
                };
                db.Services.Add(service);
                db.SaveChanges();
                return service;
            }
        }

        /***********************************************************************************/
        /* ───────────────────────────────  Remove / Delete ────────────────────────────── */
        /***********************************************************************************/
        /* ───────────────────────────────────────────────────────────────────────── */
        /* ─────────────────────────────── Companies ─────────────────────────────── */
        /* ───────────────────────────────────────────────────────────────────────── */
        public void RemoveCompany(int companyId)
        {
            using var db = Ctx();
            var company = db.Companies
                            .Include(co => co.ServiceAreas)
                            .ThenInclude(sa => sa.Services)
                            .ThenInclude(s => s.Avatars)
                            .FirstOrDefault(co => co.CompanyId == companyId);

            if (company == null) return;         // already deleted elsewhere
            foreach (var sa in company.ServiceAreas)
            {
                foreach (var svc in sa.Services)
                {
                    foreach (var av in svc.Avatars)
                    {
                        ProxyManager.ReleaseProxy(av.Id);
                    }
                }
            }
            db.Companies.Remove(company);
            db.SaveChanges();                    // executes one transaction, no concurrency error
        }
        /***************************************************************************/
        public void DeleteCompany(int id)
        {
            using (var db = Ctx())
            {
                Company company = db.Companies.Find(id);
                RemoveCompany(company.CompanyId);
            }
        }
        /* ───────────────────────────────────────────────────────────────────────── */
        /* ───────────────────────────── SERVICE-AREAS ───────────────────────────── */
        /* ───────────────────────────────────────────────────────────────────────── */
        public void RemoveServiceArea(ServiceArea a)
        {
            using (var db = Ctx())
            {
                var services = db.Services.Where(s => s.ServiceAreaId == a.ServiceAreaId).ToList();
                foreach (var s in services) RemoveService(s);
                db.ServiceAreas.Remove(a);
                db.SaveChanges();
            }
        }
        /* ───────────────────────────────────────────────────────────────────────── */
        /* ───────────────────────────────── SERVICES ────────────────────────────── */
        /* ───────────────────────────────────────────────────────────────────────── */
        public void RemoveService(Service s)
        {
            using (var db = Ctx())
            {

                var avatars = db.Avatars.Where(a => a.ServiceId == s.ServiceId).ToList();
                foreach (var av in avatars)
                {
                    ProxyManager.ReleaseProxy(av.Id);
                }
                db.Avatars.RemoveRange(avatars);
                db.Services.Remove(s);
                db.SaveChanges();
            }
        }
        /***************************************************************************/
        public void DeleteService(int id)
        {
            using (var db = Ctx())
            {
                RemoveService(db.Services.Find(id));
            }
        }
        /* ───────────────────────────────────────────────────────────────────────── */
        /* ───────────────────────────────── AVATARS ─────────────────────────────── */
        /* ───────────────────────────────────────────────────────────────────────── */
        public void RemoveAvatar(Avatar a)
        {

            using (var db = Ctx())
            {
                if (a != null) ProxyManager.ReleaseProxy(a.Id);
                db.Avatars.Remove(a);
                db.SaveChanges();
            }

        }
        /***************************************************************************/
        public void DeleteArea(int id)
        {
            using (var db = Ctx())
            {
                RemoveServiceArea(db.ServiceAreas.Find(id));
            }
        }
        /***************************************************************************/
        public void DeleteAvatar(int id)
        {
            using (var db = Ctx())
            {
                RemoveAvatar(db.Avatars.Find(id));
            }
        }

        /********************************************************************************/
        /* ───────────────────────────────  Updat / save ────────────────────────────── */
        /********************************************************************************/
        /* ───────────────────────────────────────────────────────────────────────── */
        /* ─────────────────────────────── Companies ─────────────────────────────── */
        /* ───────────────────────────────────────────────────────────────────────── */
        public void UpdateCompany(Company c)
        {
            using var db = Ctx();
            db.Companies.Attach(c);
            db.Entry(c).State = EntityState.Modified;
            db.SaveChanges();
        }
        /* ───────────────────────────────────────────────────────────────────────── */
        /* ───────────────────────────── SERVICE-AREAS ───────────────────────────── */
        /* ───────────────────────────────────────────────────────────────────────── */
        public void UpdateServiceArea(ServiceArea a)
        {
            using var db = Ctx();
            db.ServiceAreas.Attach(a);
            db.Entry(a).State = EntityState.Modified;
            db.SaveChanges();
        }

        /* ───────────────────────────────────────────────────────────────────────── */
        /* ───────────────────────────── SERVICE ─────────────────────────────────── */
        /* ───────────────────────────────────────────────────────────────────────── */
        public void UpdateService(Service s)
        {
            using var db = Ctx();            
            db.Services.Attach(s);           
            db.Entry(s).State = EntityState.Modified;
            db.SaveChanges();                
        }
        /* ───────────────────────────────────────────────────────────────────────── */
        /* ───────────────────────────────── SERVICES ────────────────────────────── */
        /* ───────────────────────────────────────────────────────────────────────── */
        //public void UpdateService(Service s) => _context.SaveChanges();
        /***************************************************************************/
        public void SaveChanges() => _context.SaveChanges();
        /* ───────────────────────────────────────────────────────────────────────── */
        /* ───────────────────────────────── AVATARS ─────────────────────────────── */
        /* ───────────────────────────────────────────────────────────────────────── */
        public void UpdateAvatarStatus(string avatarId, string status)
        {
            using var db = Ctx();
            var av = db.Avatars.Find(avatarId);
            if (av == null) return;

            av._status = status;
            av.LastActionDescription = status;
            av.LastActiveTime = DateTime.Now;
            db.SaveChanges();
        }
        /***************************************************************************/
        public void UpdateAvatar(Avatar avatar)
        {
            using (var db = Ctx())
            {
                db.Avatars.Update(avatar);
                db.SaveChanges();
            }
        }

        /***************************************************************************/
        /* ───────────────────────────────  Getters ────────────────────────────── */
        /***************************************************************************/
        /* ───────────────────────────────────────────────────────────────────────── */
        /* ─────────────────────────────── Companies ─────────────────────────────── */
        /* ───────────────────────────────────────────────────────────────────────── */
        public List<Company> GetAllCompany()
        {
            using (var db = Ctx())
            {
                var companies_list = db.Companies;
                return companies_list.ToList();
            }
        }
        /***************************************************************************/
        public List<Company> AllCompanies()
        {
            using (var db = Ctx())
            {
                return db.Companies.ToList();
            }

        }
        /***************************************************************************/
        public Company GetCompanyById(int companyId)
        {
            using var db = Ctx();
            return db.Companies
                     .Include(c => c.ServiceAreas)               // eager‐load the areas
                       .ThenInclude(sa => sa.Services)           // – and their services
                     .FirstOrDefault(c => c.CompanyId == companyId);
        }
        /***************************************************************************/
        public Company GetCompanyByName(string name)
        {
            using (var db = Ctx())
            {
                return db.Companies.FirstOrDefault(c => c.Name == name);
            }
        }
        /* ───────────────────────────────────────────────────────────────────────── */
        /* ───────────────────────────── SERVICE-AREAS ───────────────────────────── */
        /* ───────────────────────────────────────────────────────────────────────── */
        public List<Service> ServicesByArea(string id)
        {
            using (var db = Ctx())
            {
                return db.Services.Where(s => s.ServiceAreaId == id).ToList();
            }
        }
        /***************************************************************************/
        public ServiceArea GetServiceAreaById(string serviceAreaId)
        {
            using var db = Ctx();
            return db.ServiceAreas
                     .Include(sa => sa.Company)
                     .FirstOrDefault(sa => sa.ServiceAreaId == serviceAreaId);
        }
        /***************************************************************************/
        public List<ServiceArea> GetAllServiceAreas(Company company)
        {
            using (var db = Ctx())
            {
                if (company == null)
                    throw new ArgumentNullException(nameof(company), "Company cannot be null.");
                return db.ServiceAreas
                    .Where(sa => sa.CompanyId == company.CompanyId)
                    .ToList();
            }
        }
        /***************************************************************************/
        public List<ServiceArea> GetAllServiceAreasByCompanyId(int company_id)
        {
            using (var db = Ctx())
            {
                return db.ServiceAreas
                .Where(sa => sa.CompanyId == company_id)
                .ToList();
            }
        }
        /* ───────────────────────────────────────────────────────────────────────── */
        /* ───────────────────────────────── SERVICES ────────────────────────────── */
        /* ───────────────────────────────────────────────────────────────────────── */
        public List<Service> GetAllServices(ServiceArea serviceArea)
        {
            using (var db = Ctx())
            {
                if (serviceArea == null)
                    throw new ArgumentNullException(nameof(serviceArea), "ServiceArea cannot be null.");
                return db.Services
                    .Where(s => s.ServiceAreaId == serviceArea.ServiceAreaId)
                    .ToList();
            }
        }
        /***************************************************************************/
        public Service GetServiceById(string serviceId)
        {
            using var db = Ctx();
            return db.Services
                     .Include(s => s.ServiceArea)
                     .FirstOrDefault(s => s.ServiceId == serviceId);
        }
        /* ───────────────────────────────────────────────────────────────────────── */
        /* ───────────────────────────────── AVATARS ─────────────────────────────── */
        /* ───────────────────────────────────────────────────────────────────────── */
        public Avatar GetAvatarById(string id)
        {
            using var db = Ctx();
            return db.Avatars
                     .Include(a => a.Service)
                       .ThenInclude(s => s.ServiceArea)
                     .FirstOrDefault(a => a.Id == id);
        }
        /***************************************************************************/
        public List<Avatar> GetAvatarsByState(string state)
        {
            using var db = Ctx();
            return db.Avatars
                     .Include(a => a.Service)
                     .ThenInclude(s => s.ServiceArea)
                     .Where(a => a.Service.ServiceArea.State == state)
                     .ToList();
        }
        /***************************************************************************/
        public List<ServiceArea> AreasByCompany(int id)
        {
            using (var db = Ctx())
            {
                return db.ServiceAreas.Where(sa => sa.CompanyId == id).ToList();
            }
        }
        /***************************************************************************/
        public List<Avatar> AvatarsByService(string id)
        {
            using (var db = Ctx())
            {
                return db.Avatars.Where(a => a.ServiceId == id).ToList();
            }
        }
        /***************************************************************************/
        public List<Avatar> GetAllAvatars(Service service)
        {
            using (var db = Ctx())
            {
                if (service == null)
                    throw new ArgumentNullException(nameof(service), "Service cannot be null.");
                return db.Avatars
                    .Where(a => a.ServiceId == service.ServiceId)
                    .ToList();
            }
        }
        /***************************************************************************/
        public List<Avatar> AvatarsByCompanyIds(IEnumerable<int> ids)
        {
            using (var db = Ctx())
            {
                return db.Avatars.Include(a => a.Service)
                .ThenInclude(s => s.ServiceArea)
                .Where(a => ids.Contains(a.Service.ServiceArea.CompanyId))
                .ToList();
            }
        }
        /***************************************************************************/
        public List<Avatar> AvatarsByAreaIds(IEnumerable<string> ids)
        {
            using (var db = Ctx())
            {
                return db.Avatars.Include(a => a.Service)
                        .Where(a => ids.Contains(a.Service.ServiceAreaId))
                        .ToList();
            }
        }
        /***************************************************************************/
        public List<Avatar> AvatarsByServiceIds(IEnumerable<string> ids)
        {
            using (var db = Ctx())
            {
                return db.Avatars.Where(a => ids.Contains(a.ServiceId)).ToList();
            }
        }
        /***************************************************************************/
        public List<Avatar> AvatarsByCompany(int companyId)
        {
            using (var db = Ctx())
            {
                return db.Avatars
                .Include(a => a.Service)
                .ThenInclude(s => s.ServiceArea)
                .Where(a => a.Service.ServiceArea.CompanyId == companyId)
                .ToList();
            }
        }
        /***************************************************************************/
        public List<Avatar> AvatarsByArea(string areaId)
        {
            using (var db = Ctx())
            {
                return db.Avatars
                .Include(a => a.Service)
                .Where(a => a.Service.ServiceAreaId == areaId)
                .ToList();
            }
        }


        /***************************************************************************/
        public Conversation? GetConversation(string conversationId)
        {
            using (var db = Ctx())
            {
                try
                {
                    return db.Conversations
                        .Include(c => c.Messages)
                        .FirstOrDefault(c => c.ConversationId == conversationId); // Changed from ConversationId to Id
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to retrieve conversation with ID '{conversationId}': {ex.Message}", ex);
                }
            }
        }
        /* ─────────── Groups ─────────── */
        /***************************************************************************/
        public List<Group> GetGroupsByAvatarId(string avatarId)
        {
            using var db = Ctx();
           return db.Groups.Where(g => g.AvatarId == avatarId).ToList();
         
        }
        /***************************************************************************/
        public Group AddGroup(string name, string url, string state, Avatar avatar)
        {
            using var db = Ctx();
            var existing = db.Groups.Where(g => g.AvatarId == avatar.Id).ToDictionary(g => g.Url, g => g);
            if (existing.TryGetValue(url, out var g))
            {
                g.Name = name;
                db.SaveChanges();
                return g;
            }
            Group group_tmp_iter = new Group()
            {
                GroupId = Guid.NewGuid().ToString(),
                AvatarId = avatar.Id,
                Url = url,
                Name = name,
                City = avatar._city,
                State = state
            };
            db.Groups.Add(group_tmp_iter);
            db.SaveChanges();
            return group_tmp_iter;
        }
    }
}
