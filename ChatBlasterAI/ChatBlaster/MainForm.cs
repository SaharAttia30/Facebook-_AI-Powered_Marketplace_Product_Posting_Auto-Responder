using System.ComponentModel;
using ChatBlaster.Models;
using Microsoft.EntityFrameworkCore;
using ChatBlaster.Controllers.Avatar;
using ChatBlaster.DB;
using ChatBlaster.Utilities;
using System.Diagnostics;
using System.Threading;
using OpenQA.Selenium;
#nullable enable
namespace ChatBlaster
{
    public class MainForm : Form
    {
        internal enum ListType { Company = 0, AreaOfService = 1, Service = 2, Avatar = 3 }
        private ListView lvProfiles;        // avatars / profiles
        private ListView LvServiceAreas;    // service areas
        private ListView LvBlasterUsers;    // companies list
        private ListView LvServices;        // services
        private ListView LvFullInfo;
        private readonly ChatService _chat;
        private readonly Dictionary<Avatar, Thread> _runningThreads = new();
        private readonly Dictionary<Avatar, CancellationTokenSource> _cts = new();
        private readonly AvatarRunner _runner;
        private ComboBox comboBoxListToExecute;
        private ComboBox comboBoxActionToExecute;
        private readonly SemaphoreSlim _botSemaphore = new(4);
        private List<Company> _selectedCompanies = new();
        private List<ServiceArea> _selectedServiceAreas = new();
        private List<Service> _selectedServices = new();
        public List<Avatar> _selectedAvatars;
        private ProgressBar progressBulk;
        private Label lblBulk;
        private
#nullable disable
        IContainer components = (IContainer)null;
        private Button stopButton;
        private RadioButton rbElse;
        private RadioButton rbFemale;
        private RadioButton rbMale;
        private Button btKeywords;
        private Button btAvatar;
        private GroupBox Website;
        private GroupBox Gender;
        private GroupBox groupBox1;
        private ToolTip toolTip;

        private int NUMBEROFPOSTSBEFORERESET = 5;
        private bool isUpdating = false;
        public ColumnHeader columnHeader5;
        public ColumnHeader columnHeader6;
        public ColumnHeader columnHeader4;
        public ColumnHeader columnHeader1;
        private Label label1;
        private Label label2;
        private Label label5;
        private Label label4;
        private Label label3;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader7;
        public ColumnHeader columnHeader2;
        public ColumnHeader columnHeader9;
        public ColumnHeader columnHeader10;
        private ColumnHeader columnHeader11;
        private ColumnHeader columnHeader12;
        private ColumnHeader columnHeader13;
        private ColumnHeader columnHeader14;
        private ComboBox comboBox2;
        private ComboBox comboBox1;
        private ComboBox comboBoxAction;
        private ComboBox comboBoxWhatToExecute;
        private ColumnHeader columnHeader15;
        private ColumnHeader columnHeader16;
        private readonly Dictionary<Avatar, EventHandler<string>> _statusHandlers = new();
        private readonly Dictionary<Avatar, ListViewItem> _avatarItems = new Dictionary<Avatar, ListViewItem>();

       
        /* ─────────────────────────────────────────────────────────────────────────────────── */
        public MainForm(ChatService chatService)
        {
            _chat = chatService;                         // store singleton
            _chat.EnsureDatabase();
            _runner = new AvatarRunner(_chat);
            InitializeComponent();
            _selectedAvatars = new List<Avatar>();
            this.FormClosing += MainForm_FormClosing;
            InitializeProfiles();
            PopulateCompanies();
            //PopulateServiceAreas();
            //PopulateServices();
        }
        /**************************************************************************************************/
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Exit and stop all avatars?", "Confirm Exit",
                                         MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _runner.StopAllAvatarsNow();
                base.OnFormClosing(e);
            }
            else
            {
                e.Cancel = true;
            }
        }
        /**************************************************************************************************/
        public void StopAllAvatars()
        {
            _runner.StopAvatars(_selectedAvatars);
        }
        /**************************************************************************************************/
        private void stopButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to stop all avatars immediately?",
                              "Confirm Stop", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                _runner.StopAllAvatarsNow();
                MessageBox.Show("✅ All avatars stopped.", "Stopped", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        /**************************************************************************************************/
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show(
                "Stop all avatars and exit?", "Confirm Exit",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                StopAllAvatars();
                // Allow the form to close normally
            }
            else
            {
                // Cancel form close if user chooses No
                e.Cancel = true;
            }
        }
        /**************************************************************************************************/
        /***********************************main functions*************************************************/
        /**************************************************************************************************/
        private void InitializeProfiles()
        {
            _chat.EnsureDatabase();
            lvProfiles.ColumnClick += LvProfiles_ColumnClick;
            lvProfiles.ItemChecked += LvProfiles_ItemChecked;
            lvProfiles.Click += LvProfiles_ItemClicked;

            LvBlasterUsers.ItemChecked += LvCompanies_ItemChecked;
            LvBlasterUsers.ColumnClick += LvCompanies_ColumnClick;
            LvBlasterUsers.Click += LvCompanies_Click;
            LvServiceAreas.ItemChecked += LvServiceAreas_ItemChecked;
            LvServiceAreas.ColumnClick += LvServiceAreas_ColumnClick;
            LvServiceAreas.Click += LvServiceAreas_Click;
            LvServices.Click += LvServices_Click;
            LvServices.ItemChecked += LvServices_ItemChecked;
            LvServices.ColumnClick += LvServices_ColumnClick;
        }
        /**************************************************************************************************/
        private void AddFromList()
        {
            switch ((ListType)comboBoxListToExecute.SelectedIndex)
            {
                case ListType.Company:
                    using (var f = new AddUpdateCompanyForm())
                    {
                        if (f.ShowDialog(this) == DialogResult.OK)
                        {
                            var c = _chat.AddCompany(f.CompanyName, f.OwnerName, f.OwnerPhone, f.OfficePhone);
                            var it = new ListViewItem("") { Tag = c }; it.SubItems.Add(c.Name);
                            LvBlasterUsers.Items.Add(it);
                        }
                    }
                    break;

                case ListType.AreaOfService:
                    if (_selectedCompanies.Count != 1)
                    {
                        MessageBox.Show("Must Choose A Compoany And Only One");
                        return;
                    }
                    using (var f = new AddUpdateServiceAreaForm(_selectedCompanies[0].CompanyId))
                    {
                        if (f.ShowDialog(this) == DialogResult.OK)
                        {
                            if (int.TryParse(f.RadiusText, out int radius))
                            {
                                ServiceArea sa = _chat.AddServiceArea(f.City, f.State, f.ZipCode, f.SurroundingCities, radius, _selectedCompanies[0].CompanyId);
                                var it = new ListViewItem("") { Tag = sa }; it.SubItems.Add(sa.City);
                                LvServiceAreas.Items.Add(it);
                            }
                            else
                            {
                                MessageBox.Show("Invalid radius value. Please enter a valid integer.");
                                break;
                            }
                        }
                    }
                    break;
                case ListType.Service:
                    if (LvServiceAreas.SelectedItems.Count != 1) { MessageBox.Show("select area"); break; }
                    var area = (ServiceArea)LvServiceAreas.SelectedItems[0].Tag;
                    using (var f = new AddUpdateServiceForm(area.ServiceAreaId))
                    {
                        if (f.ShowDialog(this) == DialogResult.OK)
                        {
                            var s = _chat.AddService(f.Industry, f.PhoneNumber, f.FacebookCategory, area.ServiceAreaId);
                            var it = new ListViewItem("") { Tag = s };
                            it.SubItems.Add(s.Industry);
                            LvServices.Items.Add(it);
                        }
                    }
                    break;
                case ListType.Avatar:
                    if (LvServices.SelectedItems.Count != 1) { MessageBox.Show("select service"); break; }
                    var svc = (Service)LvServices.SelectedItems[0].Tag;
                    using (var f = new AddUpdateAvatarForm())
                    {
                        if (f.ShowDialog(this) == DialogResult.OK)
                        {
                            if (!ProxyManager.HasAvailableProxies())
                            {
                                MessageBox.Show("No more proxies are available to assign to a new avatar.");
                                break;
                            }
                            var av = _chat.AddAvatar(f.Email, f.Username, f.Password, f.PhotoFolder, svc.ServiceId);
                            var it = new ListViewItem("") { Tag = av };
                            it.SubItems.Add(av._userName);
                            it.SubItems.Add(av._email);
                            it.SubItems.Add(av._status);
                            lvProfiles.Items.Add(it);
                        }
                    }
                    break;
            }
        }
        /**************************************************************************************************/
        /* =====================  UPDATE  ===================== */
        /**************************************************************************************************/
        private void UpdateFromList()
        {
            switch ((ListType)comboBoxListToExecute.SelectedIndex)
            {
                case ListType.Company:
                    if (LvBlasterUsers.SelectedItems.Count != 1) { MessageBox.Show("select company"); break; }
                    var c = (Company)LvBlasterUsers.SelectedItems[0].Tag;
                    using (var f = new AddUpdateCompanyForm(c))
                        if (f.ShowDialog(this) == DialogResult.OK)
                        {
                            _chat.UpdateCompany(c);
                            LvBlasterUsers.SelectedItems[0].SubItems[1].Text = c.Name;
                        }
                    break;

                case ListType.AreaOfService:
                    if (LvServiceAreas.SelectedItems.Count != 1) { MessageBox.Show("select area"); break; }
                    var a = (ServiceArea)LvServiceAreas.SelectedItems[0].Tag;
                    using (var f = new AddUpdateServiceAreaForm(a.CompanyId))
                        if (f.ShowDialog(this) == DialogResult.OK)
                        {
                            _chat.UpdateServiceArea(a);
                            LvServiceAreas.SelectedItems[0].Text = a.City;
                        }
                    break;

                case ListType.Service:
                    if (LvServices.SelectedItems.Count != 1) { MessageBox.Show("select service"); break; }
                    var s = (Service)LvServices.SelectedItems[0].Tag;
                    using (var f = new AddUpdateServiceForm(null, s))
                        if (f.ShowDialog(this) == DialogResult.OK)
                        {
                            _chat.UpdateService(s);
                            LvServices.SelectedItems[0].SubItems[1].Text = s.Industry;
                        }
                    break;

                case ListType.Avatar:
                    if (lvProfiles.SelectedItems.Count != 1) { MessageBox.Show("select avatar"); break; }
                    var av = (Avatar)lvProfiles.SelectedItems[0].Tag;
                    using (var f = new AddUpdateAvatarForm(av))
                        if (f.ShowDialog(this) == DialogResult.OK)
                        {
                            _chat.UpdateAvatar(av);
                            lvProfiles.SelectedItems[0].Text = av._email;
                            lvProfiles.SelectedItems[0].SubItems[1].Text = av._userName;
                        }
                    break;
            }
        }
        /**************************************************************************************************/
        private void RemoveFromList()
        {
            switch ((ListType)comboBoxListToExecute.SelectedIndex)
            {
                case ListType.Company:
                    if (LvBlasterUsers.SelectedItems.Count == 0) { MessageBox.Show("select company"); break; }
                    foreach (ListViewItem it in LvBlasterUsers.SelectedItems.Cast<ListViewItem>().ToList())
                    {
                        var comp = (Company)it.Tag;
                        var avatars = _chat.AvatarsByService
                                       ("").Where(v => v.Service.ServiceArea.CompanyId == comp.CompanyId).ToList();
                        SaveAvatarsCsv(comp, avatars);
                        _chat.RemoveCompany(comp.CompanyId);
                        LvBlasterUsers.Items.Remove(it);
                    }
                    LvServiceAreas.Items.Clear(); LvServices.Items.Clear(); lvProfiles.Items.Clear();
                    break;

                case ListType.AreaOfService:
                    if (LvServiceAreas.SelectedItems.Count == 0) { MessageBox.Show("select area"); break; }
                    foreach (ListViewItem it in LvServiceAreas.SelectedItems.Cast<ListViewItem>().ToList())
                    {
                        var area = (ServiceArea)it.Tag;
                        _chat.RemoveServiceArea(area);
                        LvServiceAreas.Items.Remove(it);
                    }
                    LvServices.Items.Clear(); lvProfiles.Items.Clear();
                    break;

                case ListType.Service:
                    if (LvServices.SelectedItems.Count == 0) { MessageBox.Show("select service"); break; }
                    foreach (ListViewItem it in LvServices.SelectedItems.Cast<ListViewItem>().ToList())
                    {
                        var srv = (Service)it.Tag;
                        _chat.RemoveService(srv);
                        LvServices.Items.Remove(it);
                    }
                    lvProfiles.Items.Clear();
                    break;

                case ListType.Avatar:
                    if (lvProfiles.SelectedItems.Count == 0) { MessageBox.Show("select avatar"); break; }
                    foreach (ListViewItem it in lvProfiles.SelectedItems.Cast<ListViewItem>().ToList())
                    {
                        var avatar = (Avatar)it.Tag;
                        _chat.RemoveAvatar(avatar);
                        lvProfiles.Items.Remove(it);
                    }
                    break;
            }
        }
        /**************************************************************************************************/
        private static void SaveAvatarsCsv(Company company, IEnumerable<Avatar> avatars)
        {
            if (company == null || avatars == null || !avatars.Any()) return;

            string safeName = new string(company.Name
                                         .Where(ch => !Path.GetInvalidFileNameChars().Contains(ch))
                                         .ToArray())
                              .Replace(" ", "_");

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string filePath = Path.Combine(desktop, $"{safeName}_{company.CompanyId}_avatars.csv");

            var lines = new List<string> { "Email,Username,Password" };
            lines.AddRange(avatars.Select(a => $"{a._email},{a._userName},{a._password}"));

            File.WriteAllLines(filePath, lines);
        }
        /**************************************************************************************************/
        private void PopulateCompanies()
        {
            ListView.SelectedListViewItemCollection selectedItems = this.LvBlasterUsers.SelectedItems;
            this.LvBlasterUsers.Items.Clear();
            var companies_list = _chat.GetAllCompany();
            foreach (var comp in companies_list)
            {
                var it = new ListViewItem("") { Tag = comp };
                it.SubItems.Add(comp.Name);
                LvBlasterUsers.Items.Add(it);
            }
            if (companies_list.Count == 0)
            {
                LvServiceAreas.Items.Clear();
                LvServices.Items.Clear();
                lvProfiles.Items.Clear();
                ShowEntityInfo(null);
                return;
            }
            LvBlasterUsers.Items[0].Selected = true;
            FillServiceAreas(companies_list[0]);
        }
        /**************************************************************************************************/
        private void FillServiceAreas(Company company)
        {
            LvServiceAreas.Items.Clear();
            var areas = _chat.GetAllServiceAreasByCompanyId(company.CompanyId).ToList();
            foreach (var area in areas)
            {
                var it = new ListViewItem("") { Tag = area };
                it.SubItems.Add(area.City);
                LvServiceAreas.Items.Add(it);
            }
            if (areas.Count == 0)
            {
                LvServices.Items.Clear();
                lvProfiles.Items.Clear();
                ShowEntityInfo(company);
                return;
            }
            LvServiceAreas.Items[0].Selected = true;
            FillServices(areas[0]);
        }
        /**************************************************************************************************/
        private void FillServices(ServiceArea area)
        {
            LvServices.Items.Clear();
            if (area == null)
            {
                return;
            }
            var services = _chat.ServicesByArea(area.ServiceAreaId).ToList();

            foreach (var svc in services)
            {
                var it = new ListViewItem("") { Tag = svc };
                it.SubItems.Add(svc.Industry);
                LvServices.Items.Add(it);
            }

            if (services.Count == 0)
            {
                lvProfiles.Items.Clear();
                ShowEntityInfo(area);
                return;
            }
            ShowEntityInfo(area);
            LvServices.Items[0].Selected = true;
            FillAvatars(services[0]);
        }
        /**************************************************************************************************/
        private void FillAvatars(Service service)
        {
            if (service == null) return;
            lvProfiles.Items.Clear();
            var avatars = _chat.AvatarsByService(service.ServiceId).ToList();
            foreach (var av in avatars)
            {
                var it = new ListViewItem("") { Tag = av };
                it.SubItems.Add(av._userName);
                it.SubItems.Add(av._email);
                it.SubItems.Add(av._status);
                if (_statusHandlers.TryGetValue(av, out var oldHandler))
                {
                    av.StatusChanged -= oldHandler;
                }
                EventHandler<string> updateHandler = (_, st) =>
                {
                    BeginInvoke(new Action(() => it.SubItems[3].Text = st));
                    Task.Run(() => _chat.UpdateAvatarStatus(av.Id, st));
                };
                _statusHandlers[av] = updateHandler;
                av.StatusChanged += updateHandler;
                av.InfoUpdated += (sender, e) =>
                {
                    BeginInvoke(new Action(() => UpdateAvatarItem(av)));
                };
                lvProfiles.Items.Add(it);
                _avatarItems[av] = it;
            }
            ShowEntityInfo(service);
        }

        /**************************************************************************************************/
        private void UpdateAvatarItem(Avatar avatar)
        {
            if (_avatarItems.TryGetValue(avatar, out var item))
            {
                item.SubItems[1].Text = avatar._userName; // Column 1: username
                item.SubItems[2].Text = avatar._email;    // Column 2: email
                item.SubItems[3].Text = avatar._status;   // Column 3: status
            }
        }
        /**************************************************************************************************/
        /***********************************click functions************************************************/
        /**************************************************************************************************/
        private void LvCompanies_ColumnClick(object sender, ColumnClickEventArgs e)
        {

        }
        /**************************************************************************************************/
        private void LvCompanies_Click(object sender, EventArgs e)
        {
            Point mousePosition = LvBlasterUsers.PointToClient(Control.MousePosition);
            ListViewHitTestInfo hit = LvBlasterUsers.HitTest(mousePosition);
            if (hit.Item != null && hit.SubItem != null)
            {
                if (hit.Item.SubItems.IndexOf(hit.SubItem) > 0)
                {
                    var company = (Company)hit.Item.Tag;
                    int companyId = company.CompanyId;
                    var serviceAreas = _chat.GetAllServiceAreasByCompanyId(companyId);
                    LvServiceAreas.Items.Clear();
                    LvServices.Items.Clear();
                    lvProfiles.Items.Clear();
                    foreach (var sa in serviceAreas)
                    {
                        var item = new ListViewItem("");
                        item.SubItems.Add(sa.City);
                        item.SubItems.Add(sa.State);
                        item.Tag = sa;
                        LvServiceAreas.Items.Add(item);
                    }
                    ShowEntityInfo(company);
                    FillServiceAreas(company);
                    if (LvServiceAreas.Items.Count > 0)
                    {
                        var area = (ServiceArea)LvServiceAreas.Items[0].Tag;
                        FillServices(area);
                        if (LvServices.Items.Count > 0)
                        {
                            var svc = (Service)LvServices.Items[0].Tag;
                            FillAvatars(svc);
                        }
                    }
                }
            }
        }
        /**************************************************************************************************/
        private void LvServiceAreas_Click(object sender, EventArgs e)
        {
            Point mousePos = LvServiceAreas.PointToClient(Control.MousePosition);
            var hit = LvServiceAreas.HitTest(mousePos);
            if (hit.Item != null && hit.SubItem != null && hit.Item.SubItems.IndexOf(hit.SubItem) > 0)
            {
                var area = (ServiceArea)hit.Item.Tag;
                LvServices.Items.Clear();
                foreach (var s in _chat.ServicesByArea(area.ServiceAreaId))
                {
                    var it = new ListViewItem("") { Tag = s };
                    it.SubItems.Add(s.Industry);
                    LvServices.Items.Add(it);
                }
                FillServices(area);
                ShowEntityInfo(area);
            }
        }
        /**************************************************************************************************/
        private void LvServices_Click(object sender, EventArgs e)
        {
            Point mousePos = LvServices.PointToClient(Control.MousePosition);
            var hit = LvServices.HitTest(mousePos);
            if (hit.Item != null && hit.SubItem != null && hit.Item.SubItems.IndexOf(hit.SubItem) > 0)
            {
                var service = (Service)hit.Item.Tag;
                lvProfiles.Items.Clear();
                //foreach (var av in _chat.AvatarsByService(service.ServiceId))
                //{
                //    var it = new ListViewItem(av._email) { Tag = av };
                //    it.SubItems.Add(av._userName);
                //    lvProfiles.Items.Add(it);
                //}
                FillAvatars(service);
            }
        }
        /**************************************************************************************************/
        private void ShowEntityInfo(object? entity)
        {
            var dict = new Dictionary<string, string>();
            switch (entity)
            {
                case Company c:
                    dict["Company ID"] = c.CompanyId.ToString();
                    dict["Name"] = c.Name;
                    dict["Owner Name"] = c.OwnerName;
                    dict["Owner Phone"] = c.OwnerPhone;
                    dict["Office Phone"] = c.OfficePhone;
                    break;

                case ServiceArea a:
                    dict["Area ID"] = a.ServiceAreaId;
                    dict["City"] = a.City;
                    dict["State"] = a.State;
                    dict["Zip"] = a.ZipCode;
                    dict["Radius (mi)"] = a.Radius.ToString();
                    dict["Company ID"] = a.CompanyId.ToString();
                    dict["Surrounding"] = a.SurroundingCities;
                    break;

                case Service s:
                    dict["Service ID"] = s.ServiceId;
                    dict["Industry"] = s.Industry;
                    dict["Phone"] = s.PhoneNumber;
                    dict["Area ID"] = s.ServiceAreaId.ToString();
                    break;

                case Avatar v:
                    dict["Email"] = v._email;
                    dict["User Name"] = v._userName;
                    dict["City"] = v._city;
                    dict["Freinds Count"] = v._freindsCount.ToString();
                    dict["Profile Picture"] = v._hasProfilePicture ? "Have Picture" : "Missing Picture";
                    dict["Friend Request"] = v.FriendRequestSentToday.ToString();
                    dict["Messages Answered"] = v.MessagesAnsweredToday.ToString();
                    dict["Groups Count"] = v._groupsMemberCount.ToString();
                    dict["Status"] = v._status;
                    break;

                default:
                    LvFullInfo.Items.Clear();
                    LvFullInfo.Columns.Clear();
                    return;
            }
            LvFullInfo.BeginUpdate();
            LvFullInfo.Items.Clear();
            LvFullInfo.Columns.Clear();
            foreach (var _ in dict)
                LvFullInfo.Columns.Add(string.Empty, 120, HorizontalAlignment.Left);
            var titleItem = new ListViewItem(dict.Keys.First());
            foreach (var key in dict.Keys.Skip(1))
                titleItem.SubItems.Add(key);
            LvFullInfo.Items.Add(titleItem);
            var valueItem = new ListViewItem(dict.Values.First());
            foreach (var val in dict.Values.Skip(1))
                valueItem.SubItems.Add(val);
            LvFullInfo.Items.Add(valueItem);
            if (entity is Avatar avatar)
            {
                var sessions = _runner.GetScheduleForAvatar(avatar).ToList();
                if (sessions.Count > 0)
                {
                    var headerItem = new ListViewItem("Today's Schedule:");
                    while (headerItem.SubItems.Count < LvFullInfo.Columns.Count)
                        headerItem.SubItems.Add(string.Empty);
                    LvFullInfo.Items.Add(headerItem);
                    foreach (var session in sessions)
                    {
                        string start = session.StartTime.ToString("HH:mm");
                        string duration = session.Duration.ToString("m\\:ss") + " min";
                        string actions = string.Join(", ", session.Actions);
                        string sessionText = $"{start} ({duration}) – {actions}";
                        var sessionItem = new ListViewItem("    " + sessionText);
                        while (sessionItem.SubItems.Count < LvFullInfo.Columns.Count)
                            sessionItem.SubItems.Add(string.Empty);
                        LvFullInfo.Items.Add(sessionItem);
                    }
                }
            }
            FitColumns(LvFullInfo);
            LvFullInfo.EndUpdate();
        }
        /**************************************************************************************************/
        private void LvServiceAreas_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == 0)
            {
                bool allSelected = LvServiceAreas.Items.Cast<ListViewItem>().All(item => item.Checked);
                bool newState = !allSelected;
                foreach (ListViewItem item in LvServiceAreas.Items)
                {
                    item.Checked = newState;
                }
            }

        }
        /**************************************************************************************************/
        private void LvServices_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == 0)
            {
                bool allSelected = LvServices.Items.Cast<ListViewItem>().All(item => item.Checked);
                bool newState = !allSelected;
                foreach (ListViewItem item in LvServices.Items)
                {
                    item.Checked = newState;
                }
            }

        }
        /**************************************************************************************************/
        private void LvProfiles_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == 0)
            {
                bool allSelected = lvProfiles.Items.Cast<ListViewItem>().All(item => item.Checked);
                bool newState = !allSelected;
                foreach (ListViewItem item in lvProfiles.Items)
                {
                    item.Checked = newState;
                }
            }
        }
        /**************************************************************************************************/
        private void LvProfiles_ItemClicked(object sender, EventArgs e)
        {
            Point mousePos = lvProfiles.PointToClient(Control.MousePosition);
            var hit = lvProfiles.HitTest(mousePos);
            if (hit.Item != null && hit.SubItem != null && hit.Item.SubItems.IndexOf(hit.SubItem) > 0)            // only data columns
            {
                var profile = (Avatar)hit.Item.Tag;
                ShowEntityInfo(profile);
            }
        }
        /**************************************************************************************************/
        private static void FitColumns(ListView lv)
        {
            lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            for (int i = 0; i < lv.Columns.Count; i++)
            {
                int contentWidth = lv.Columns[i].Width;
                lv.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.HeaderSize);
                lv.Columns[i].Width = Math.Max(contentWidth, lv.Columns[i].Width);
            }
        }
        /**************************************************************************************************/
        public void DeleteEntity(string entityType, int id)
        {
            switch (entityType.ToLower())
            {
                case "company":
                    _chat.DeleteCompany(id);
                    break;
                case "area":
                    _chat.DeleteArea(id);
                    break;
                case "service":
                    _chat.DeleteService(id);
                    break;
                case "avatar":
                    _chat.DeleteAvatar(id);
                    break;
                default:
                    throw new ArgumentException("Invalid entity type");
            }
        }
        /**************************************************************************************************/
        private async void AvatarButton_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection selectedItems = this.lvProfiles.SelectedItems;
            ListViewItem listViewItem = selectedItems.Count == 1 ? selectedItems[0] : null;
            int runningAvatars = _selectedAvatars.Count;
            List<Task> tasks = new List<Task>();
            foreach (var profile in _selectedAvatars)
            {
                var cts = new CancellationTokenSource();
                //Thread thread = new Thread(() =>
                //{
                if (profile._blocked)
                {
                    profile.SetStatus("Avatar user is Blocked cant be used");
                }
                try
                {
                    tasks.Add(Task.Run(async () => { await profile.Run(_chat, cts.Token); }));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Avatar {profile.Id} failed: {ex.Message}");
                }
                finally
                {

                    if (Interlocked.Decrement(ref runningAvatars) == 0)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            Console.WriteLine("All avatars have completed.");
                        });
                    }
                }
                //});
                //_runningThreads[profile] = thread;
                //_cts[profile] = cts;
                //thread.Start();
            }
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                // log exceptions...
            }
        }
        /**************************************************************************************************/
        /***********************************change functions***********************************************/
        /**************************************************************************************************/
        private void LvProfiles_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ListViewItem item = e.Item;
            var avatar = item.Tag as Avatar;
            if (avatar != null)
            {
                if (item.Checked)
                {
                    if (!_selectedAvatars.Contains(avatar))
                    {
                        _selectedAvatars.Add(avatar);
                    }
                }
                else
                {
                    _selectedAvatars.Remove(avatar);
                }
            }
        }
        /**************************************************************************************************/
        private void LvCompanies_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ListViewItem item = e.Item;
            Company company = item.Tag as Company;
            if (company != null)
            {
                if (item.Checked)
                {
                    if (!_selectedCompanies.Contains(company))
                    {
                        _selectedCompanies.Add(company);
                    }
                }
                else
                {
                    _selectedCompanies.Remove(company);
                }
            }
        }
        /**************************************************************************************************/
        private void LvServiceAreas_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ListViewItem item = e.Item;
            ServiceArea service_area = item.Tag as ServiceArea;
            if (service_area != null)
            {
                if (item.Checked)
                {
                    if (!_selectedServiceAreas.Contains(service_area))
                    {
                        _selectedServiceAreas.Add(service_area);
                    }
                }
                else
                {
                    _selectedServiceAreas.Remove(service_area);
                }
            }
        }
        /**************************************************************************************************/
        private void LvServices_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ListViewItem item = e.Item;
            Service service = item.Tag as Service; // Use Company instead of Profile
            if (service != null)
            {
                if (item.Checked)
                {
                    if (!_selectedServices.Contains(service))
                    {
                        _selectedServices.Add(service);
                    }
                }
                else
                {
                    _selectedServices.Remove(service);
                }
            }
        }
        /**************************************************************************************************/
        /* ────────────────────────  target avatars for action  ──────────────────────── */
        private List<Avatar> GetTargetAvatars()
        {
            switch ((ListType)comboBoxListToExecute.SelectedIndex)
            {
                case ListType.Company:
                    if (_selectedCompanies.Count == 0) return new();
                    return _chat.AvatarsByCompanyIds(_selectedCompanies.Select(c => c.CompanyId));

                case ListType.AreaOfService:
                    if (_selectedServiceAreas.Count == 0) return new();
                    return _chat.AvatarsByAreaIds(_selectedServiceAreas.Select(a => a.ServiceAreaId));

                case ListType.Service:
                    if (_selectedServices.Count == 0) return new();
                    return _chat.AvatarsByServiceIds(_selectedServices.Select(s => s.ServiceId));

                case ListType.Avatar:
                    return _selectedAvatars
                           .Select(p => _chat.AddAvatar(p._email, p._userName, p._password, p._photoDirectory, "0")) // dummy serviceId 0
                           .ToList();    // you can replace with your own mapping

                default:
                    return new();
            }
        }
        /**************************************************************************************************/
        /* ────────────────────────────────  RUN  ───────────────────────────────── */
        private async void BtnRun_Click(object sender, EventArgs e)
        {
            var listType = (ListType)comboBoxListToExecute.SelectedIndex;
            List<Avatar> avatarsToRun = new List<Avatar>();
            switch (listType)
            {
                case ListType.Company:
                    if (_selectedCompanies.Count == 0)
                    {
                        MessageBox.Show("Please select at least one company.", "Run – nothing selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    avatarsToRun = _chat.AvatarsByCompanyIds(_selectedCompanies.Select(c => c.CompanyId)).ToList();
                    break;
                case ListType.AreaOfService:
                    if (_selectedServiceAreas.Count == 0)
                    {
                        MessageBox.Show("Please select at least one service area.", "Run – nothing selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    avatarsToRun = _chat.AvatarsByAreaIds(_selectedServiceAreas.Select(a => a.ServiceAreaId)).ToList();
                    break;
                case ListType.Service:
                    if (_selectedServices.Count == 0)
                    {
                        MessageBox.Show("Please select at least one service.", "Run – nothing selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    avatarsToRun = _chat.AvatarsByServiceIds(_selectedServices.Select(s => s.ServiceId)).ToList();
                    break;
                case ListType.Avatar:
                    bool anyChecked = lvProfiles.Items.Cast<ListViewItem>().Any(it => it.Checked);
                    if (!anyChecked)
                    {
                        MessageBox.Show("Please check at least one avatar in the list before pressing Run.", "Run – nothing selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    avatarsToRun = _selectedAvatars.ToList();
                    break;
                default:
                    MessageBox.Show("Please select a valid list type to run.", "Run – invalid selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
            }
            if (avatarsToRun.Count == 0)
            {
                MessageBox.Show("No avatars were found for the selected entity.", "Run – no avatars", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _runner.StartAllAvatars(avatarsToRun);
            MessageBox.Show($"{avatarsToRun.Count} avatar(s) started.", "Run started", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        /* ─────────────────────────────────────────────────────────────────────────────────── */
        private void BtnExecute_Click(object sender, EventArgs e)
        {
            int list_to_execute_idx = comboBoxListToExecute.SelectedIndex;
            if (list_to_execute_idx == -1)
            {
                MessageBox.Show("select List");
                return;
            }
            int action_to_execute_idx = comboBoxActionToExecute.SelectedIndex;
            if (action_to_execute_idx == -1)
            {
                MessageBox.Show("select Action");
                return;
            }
            else if (0 == action_to_execute_idx)
            {
                AvatarButton_Click(sender, e);
            }
            else if (1 == action_to_execute_idx)
            {
                AddFromList();
            }
            else if (2 == action_to_execute_idx)
            {
                RemoveFromList();
            }
            else if (3 == action_to_execute_idx)
            {
                UpdateFromList();

            }
            else if (4 == action_to_execute_idx)
            {
                AddAvatarBulk();
            }

        }
        /* ─────────────────────────────────────────────────────────────────────────────────── */
        public async void AddAvatarBulk()
        {
            if (LvServices.SelectedItems.Count != 1)
            { MessageBox.Show("select service"); return; }
            var svc = (Service)LvServices.SelectedItems[0].Tag;
            using var f = new AddUpdateAvatarBulkForm();
            if (f.ShowDialog(this) != DialogResult.OK) return;
            var rows = f.Avatars.Select(a => (a.Email, a.UserName, a.Password, svc.ServiceId, a.Number, a.Id, a.TwoFa, a.PhotosPath)).ToList();
            if (rows.Count == 0) return;
            // --- UI: show progress ---
            progressBulk.Visible = true;
            lblBulk.Visible = true;
            progressBulk.Minimum = 0;
            progressBulk.Maximum = rows.Count;
            progressBulk.Value = 0;
            lblBulk.Text = $"0 / {rows.Count} uploaded…";
            btAvatar.Enabled = false;
            // progress reporter marshals back to UI thread automatically
            var progress = new Progress<int>(done =>
            {
                progressBulk.Value = done;
                lblBulk.Text = $"{done} / {rows.Count} uploaded…";
            });

            try
            {
                var newAvatars = await Task.Run(() =>
                    _chat.AddAvatarsBulk(rows, progress));
                foreach (var av in newAvatars)
                {
                    var it = new ListViewItem("") { Tag = av };
                    it.SubItems.Add(av._userName);
                    it.SubItems.Add(av._email);
                    it.SubItems.Add(av._status);
                    lvProfiles.Items.Add(it);
                    av.StatusChanged += (_, st) =>
                    {
                        BeginInvoke(new Action(() => it.SubItems[3].Text = st));   // col-index 3 = “Status”
                    };
                }
                lblBulk.Text = "Upload complete ✔";
            }
            catch (Exception ex)
            {
                lblBulk.Text = "Upload failed!";
                MessageBox.Show($"Bulk upload error: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btAvatar.Enabled = true;
                await Task.Delay(2000);
                progressBulk.Visible = false;
                lblBulk.Visible = false;
            }
        }
        /**************************************************************************************************/
        /***********************************helper functions************************************************/
        /**************************************************************************************************/
        private void InitializeComponent()
        {
            components = new Container();
            stopButton = new Button();
            btAvatar = new Button();
            btKeywords = new Button();
            groupBox1 = new GroupBox();
            comboBoxActionToExecute = new ComboBox();
            comboBoxListToExecute = new ComboBox();
            label2 = new Label();
            LvFullInfo = new ListView();
            label1 = new Label();
            LvServices = new ListView();
            columnHeader16 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            lvProfiles = new ListView();
            columnHeader5 = new ColumnHeader();
            columnHeader6 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader7 = new ColumnHeader();
            LvServiceAreas = new ListView();
            columnHeader15 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            LvBlasterUsers = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader14 = new ColumnHeader();
            columnHeader9 = new ColumnHeader();
            columnHeader10 = new ColumnHeader();
            columnHeader11 = new ColumnHeader();
            columnHeader12 = new ColumnHeader();
            columnHeader13 = new ColumnHeader();
            progressBulk = new ProgressBar();
            lblBulk = new Label();
            toolTip = new ToolTip(components);
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // stopButton
            // 
            stopButton.BackColor = Color.Red;
            stopButton.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point);
            stopButton.ForeColor = SystemColors.ControlText;
            stopButton.Location = new Point(80, 60);
            stopButton.Name = "stopButton";
            stopButton.Size = new Size(70, 35);
            stopButton.TabIndex = 0;
            stopButton.Text = "Stop";
            stopButton.UseVisualStyleBackColor = false;
            stopButton.Click += stopButton_Click;
            // 
            // btAvatar
            // 
            btAvatar.BackColor = SystemColors.Highlight;
            btAvatar.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point);
            btAvatar.ForeColor = SystemColors.ButtonHighlight;
            btAvatar.Location = new Point(6, 135);
            btAvatar.Name = "btAvatar";
            btAvatar.Size = new Size(146, 35);
            btAvatar.TabIndex = 0;
            btAvatar.Text = "Execute";
            btAvatar.UseVisualStyleBackColor = false;
            btAvatar.Click += BtnExecute_Click;
            // 
            // btKeywords
            // 
            btKeywords.BackColor = Color.Lime;
            btKeywords.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btKeywords.Location = new Point(4, 60);
            btKeywords.Name = "btKeywords";
            btKeywords.Size = new Size(70, 35);
            btKeywords.TabIndex = 0;
            btKeywords.Text = "Run";
            btKeywords.UseVisualStyleBackColor = false;
            btKeywords.Click += BtnRun_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(comboBoxActionToExecute);
            groupBox1.Controls.Add(comboBoxListToExecute);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(LvFullInfo);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(LvServices);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(lvProfiles);
            groupBox1.Controls.Add(LvServiceAreas);
            groupBox1.Controls.Add(LvBlasterUsers);
            groupBox1.Controls.Add(btKeywords);
            groupBox1.Controls.Add(stopButton);
            groupBox1.Controls.Add(btAvatar);
            groupBox1.Location = new Point(12, 22);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1423, 613);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Set Profile and Start";
            // 
            // comboBoxActionToExecute
            // 
            comboBoxActionToExecute.AutoCompleteCustomSource.AddRange(new string[] { "First Login" });
            comboBoxActionToExecute.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxActionToExecute.FormattingEnabled = true;
            comboBoxActionToExecute.Items.AddRange(new object[] { "First Login", "Add", "Remove", "Update", "Add Avatar Bulk" });
            comboBoxActionToExecute.Location = new Point(4, 101);
            comboBoxActionToExecute.Name = "comboBoxActionToExecute";
            comboBoxActionToExecute.Size = new Size(148, 28);
            comboBoxActionToExecute.TabIndex = 34;
            comboBoxActionToExecute.TabStop = false;
            // 
            // comboBoxListToExecute
            // 
            comboBoxListToExecute.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxListToExecute.FormattingEnabled = true;
            comboBoxListToExecute.Items.AddRange(new object[] { "Company", "Area Of Service", "Services", "Avatars In Area" });
            comboBoxListToExecute.Location = new Point(6, 26);
            comboBoxListToExecute.MaxDropDownItems = 4;
            comboBoxListToExecute.Name = "comboBoxListToExecute";
            comboBoxListToExecute.Size = new Size(146, 28);
            comboBoxListToExecute.TabIndex = 33;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 408);
            label2.Name = "label2";
            label2.Size = new Size(87, 20);
            label2.TabIndex = 32;
            label2.Text = "Information";
            // 
            // LvFullInfo
            // 
            LvFullInfo.FullRowSelect = true;
            LvFullInfo.GridLines = true;
            LvFullInfo.Location = new Point(6, 432);
            LvFullInfo.Margin = new Padding(3, 4, 3, 4);
            LvFullInfo.MultiSelect = false;
            LvFullInfo.Name = "LvFullInfo";
            LvFullInfo.Size = new Size(1390, 174);
            LvFullInfo.TabIndex = 31;
            LvFullInfo.UseCompatibleStateImageBehavior = false;
            LvFullInfo.View = View.Details;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(642, 23);
            label1.Name = "label1";
            label1.Size = new Size(62, 20);
            label1.TabIndex = 30;
            label1.Text = "Services";
            // 
            // LvServices
            // 
            LvServices.CheckBoxes = true;
            LvServices.Columns.AddRange(new ColumnHeader[] { columnHeader16, columnHeader2 });
            LvServices.FullRowSelect = true;
            LvServices.GridLines = true;
            LvServices.Location = new Point(642, 47);
            LvServices.Margin = new Padding(3, 4, 3, 4);
            LvServices.MultiSelect = false;
            LvServices.Name = "LvServices";
            LvServices.Size = new Size(223, 357);
            LvServices.TabIndex = 29;
            LvServices.UseCompatibleStateImageBehavior = false;
            LvServices.View = View.Details;
            // 
            // columnHeader16
            // 
            columnHeader16.Text = "Select";
            // 
            // columnHeader2
            // 
            columnHeader2.Tag = "_service_lv";
            columnHeader2.Text = "Service";
            columnHeader2.Width = 160;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(1112, 23);
            label5.Name = "label5";
            label5.Size = new Size(109, 20);
            label5.TabIndex = 28;
            label5.Text = "Avatars In Area";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(435, 23);
            label4.Name = "label4";
            label4.Size = new Size(150, 20);
            label4.TabIndex = 27;
            label4.Text = "Users Area Of Service";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(203, 23);
            label3.Name = "label3";
            label3.Size = new Size(83, 20);
            label3.TabIndex = 26;
            label3.Text = "Companies";
            // 
            // lvProfiles
            // 
            lvProfiles.CheckBoxes = true;
            lvProfiles.Columns.AddRange(new ColumnHeader[] { columnHeader5, columnHeader6, columnHeader3, columnHeader7 });
            lvProfiles.FullRowSelect = true;
            lvProfiles.GridLines = true;
            lvProfiles.Location = new Point(871, 47);
            lvProfiles.Margin = new Padding(3, 4, 3, 4);
            lvProfiles.MultiSelect = false;
            lvProfiles.Name = "lvProfiles";
            lvProfiles.Size = new Size(525, 357);
            lvProfiles.TabIndex = 25;
            lvProfiles.UseCompatibleStateImageBehavior = false;
            lvProfiles.View = View.Details;
            // 
            // columnHeader5
            // 
            columnHeader5.Tag = "_selected";
            columnHeader5.Text = "Select";
            // 
            // columnHeader6
            // 
            columnHeader6.Text = "user name";
            columnHeader6.Width = 120;
            // 
            // columnHeader3
            // 
            columnHeader3.Tag = "_rank_avatar_lv";
            columnHeader3.Text = "Email";
            columnHeader3.Width = 120;
            // 
            // columnHeader7
            // 
            columnHeader7.Tag = "_status_avatar_lv";
            columnHeader7.Text = "Status";
            columnHeader7.Width = 210;
            // 
            // LvServiceAreas
            // 
            LvServiceAreas.CheckBoxes = true;
            LvServiceAreas.Columns.AddRange(new ColumnHeader[] { columnHeader15, columnHeader4 });
            LvServiceAreas.FullRowSelect = true;
            LvServiceAreas.GridLines = true;
            LvServiceAreas.Location = new Point(399, 47);
            LvServiceAreas.Margin = new Padding(3, 4, 3, 4);
            LvServiceAreas.MultiSelect = false;
            LvServiceAreas.Name = "LvServiceAreas";
            LvServiceAreas.Size = new Size(234, 357);
            LvServiceAreas.TabIndex = 24;
            LvServiceAreas.UseCompatibleStateImageBehavior = false;
            LvServiceAreas.View = View.Details;
            // 
            // columnHeader15
            // 
            columnHeader15.Text = "Select";
            // 
            // columnHeader4
            // 
            columnHeader4.Tag = "_service_area_lv";
            columnHeader4.Text = "Service area";
            columnHeader4.Width = 170;
            // 
            // LvBlasterUsers
            // 
            LvBlasterUsers.CheckBoxes = true;
            LvBlasterUsers.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader14 });
            LvBlasterUsers.FullRowSelect = true;
            LvBlasterUsers.GridLines = true;
            LvBlasterUsers.Location = new Point(158, 47);
            LvBlasterUsers.Margin = new Padding(3, 4, 3, 4);
            LvBlasterUsers.MultiSelect = false;
            LvBlasterUsers.Name = "LvBlasterUsers";
            LvBlasterUsers.Size = new Size(235, 357);
            LvBlasterUsers.TabIndex = 23;
            LvBlasterUsers.UseCompatibleStateImageBehavior = false;
            LvBlasterUsers.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Tag = "";
            columnHeader1.Text = "Select";
            // 
            // columnHeader14
            // 
            columnHeader14.Tag = "_blastUserCompanyName";
            columnHeader14.Text = "Company name";
            columnHeader14.Width = 170;
            // 
            // columnHeader9
            // 
            columnHeader9.Tag = "_selected";
            columnHeader9.Text = "Select";
            // 
            // columnHeader10
            // 
            columnHeader10.Text = "user name";
            columnHeader10.Width = 120;
            // 
            // columnHeader11
            // 
            columnHeader11.Tag = "_rank_avatar_lv";
            columnHeader11.Text = "Rank";
            columnHeader11.Width = 80;
            // 
            // columnHeader12
            // 
            columnHeader12.Tag = "_status_avatar_lv";
            columnHeader12.Text = "Logged In";
            columnHeader12.Width = 80;
            // 
            // columnHeader13
            // 
            columnHeader13.Tag = "_status_avatar_lv";
            columnHeader13.Text = "Status";
            columnHeader13.Width = 180;
            // 
            // progressBulk
            // 
            progressBulk.Location = new Point(871, 414);
            progressBulk.Name = "progressBulk";
            progressBulk.Size = new Size(525, 18);
            progressBulk.TabIndex = 0;
            progressBulk.Visible = false;
            // 
            // lblBulk
            // 
            lblBulk.Location = new Point(871, 394);
            lblBulk.Name = "lblBulk";
            lblBulk.Size = new Size(525, 18);
            lblBulk.TabIndex = 0;
            lblBulk.TextAlign = ContentAlignment.MiddleLeft;
            lblBulk.Visible = false;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1447, 650);
            Controls.Add(groupBox1);
            Name = "MainForm";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }
        /**************************************************************************************************/
        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }
    }

}

















