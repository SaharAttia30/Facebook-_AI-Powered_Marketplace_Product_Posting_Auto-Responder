using System.ComponentModel.DataAnnotations.Schema;
using ChatBlaster.Controllers.Avatar;
using ChatBlaster.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ChatBlaster.DB;
using ChatBlaster.Utilities;
using Microsoft.VisualBasic.ApplicationServices;
using OpenQA.Selenium;
using Newtonsoft.Json.Linq;
namespace ChatBlaster.Models
{
    public class Avatar
    {
        private readonly ChatService _chat;
        public string Id { get; set; }
        public string _email { get; set; }
        public string _phoneNumber { get; set; }
        public string _userName { get; set; }
        public string _password { get; set; }
        public string _city { get; set; }
        public string _country { get; set; }
        public virtual ICollection<Conversation> Conversations { get; set; }
        public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
        public DateTime LastGroupsUpdateTime { get; set; }
        private SeleniumAvatarBot _seleniumAvatarBot { get; set; }
        public string ServiceId { get; set; }

        [ForeignKey(nameof(ServiceId))]
        public virtual Service Service { get; set; }
        public DateTime LastWakeTime { get; set; } = DateTime.Now;    // last scheduled wake time
        public DateTime LastSleepTime { get; set; }      // last scheduled sleep time
        public DateTime TodaySleep { get; set; }      // last scheduled sleep time
        public DateTime LastActiveTime { get; set; } = DateTime.Now;  // last time the avatar performed an action
        public string LastActionDescription { get; set; } = "Created"; // description of last action performed or status
        public int FriendRequestSentToday { get; set; } = 0;
        public int MessagesAnsweredToday { get; set; } = 0;
        public int PostsMadeToday { get; set; } = 0;
        public int CommentsMadeToday { get; set; } = 0;
        public int ReactionsMadeToday { get; set; } = 0;
        public int VideosWatchedToday { get; set; } = 0;
        public string _port { get; set; }
        public string _userPath { get; set; }
        public string _avatarFolderPath { get; set; }
        public string _chromePath { get; set; }
        public string _photoDirectory { get; set; }
        public string _twoFactorHash { get; set; }
        public string _status { get; set; }
        public string ProxyAddress { get; set; }
        public bool _hasProfilePicture { get; set; }
        public string _linkForAvatarProfile { get; set; }
        public int _freindsCount { get; set; } = 0;
        public int _groupsMemberCount { get; set; } = 0;
        public bool _blocked { get; set; } = false;
        static readonly SemaphoreSlim _slot = new SemaphoreSlim(AvatarConfig.MaxConcurrentBrowsers);   // ⬅️ max 5 chromes

        public event EventHandler<string> StatusChanged;
        public event EventHandler InfoUpdated;
        /**************************************************************************************/
        /******************************UMAPPED*************************************************/
        /**************************************************************************************/
        [NotMapped]
        public string _urlToRun { get; set; }

        [NotMapped]
        public string _base_url { get; set; }
        [NotMapped]
        public string _title { get; set; }
        // ──────────────────────────────────────────────────────────────────────────────
        public Avatar()
        {
            Conversations = new List<Conversation>();
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public Avatar(string user_name, string email, string password, string PhotoFolder, string city, ChatService chat) : this()
        {
            _chat = chat ?? throw new ArgumentNullException(nameof(chat));
            _twoFactorHash = "";
            Conversations = new List<Conversation>();
            _userName = user_name;
            _base_url = "https://www.google.com";
            _title = "Google";
            _email = email;
            _urlToRun = "https://www.facebook.com/";
            _password = password;
            _port = string.Empty;
            _status = "Initializing";
            TodaySleep = DateTime.Today.Add(AvatarConfig.MaxSleepTime);
            _hasProfilePicture = false;
            LastSleepTime = TodaySleep.AddDays(-1);
            Id = Guid.NewGuid().ToString();
            _phoneNumber = "";
            _linkForAvatarProfile = "";
            Dictionary<string, string> dictionary_path = StaticHelpers.CreateAvatarFolders(Id.ToString());
            _chromePath = dictionary_path.TryGetValue("_chromePath", out string? chrome_path) ? chrome_path : string.Empty;
            _userPath = dictionary_path.TryGetValue("_userPath", out string? user_path) ? user_path : string.Empty;
            _photoDirectory = dictionary_path.TryGetValue("_photoDirectory", out string? _photo_directory) ? _photo_directory : string.Empty;
            _avatarFolderPath = dictionary_path.TryGetValue("_avatarFolderPath", out string? avatar_path) ? avatar_path : string.Empty;
            StaticHelpers.CopyAllFoldeContent(PhotoFolder, _photoDirectory);
            _city = city;
            _country = "US";
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public void Dispose()
        {
            var drv = _seleniumAvatarBot.GetDriver();
            _seleniumAvatarBot?.Stop(drv);
            if (!string.IsNullOrEmpty(_port))
            {
                PortAllocator.Release(int.Parse(_port));
            }
            _port = null;
            _seleniumAvatarBot = null;
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task WaitAsync(string reason, TimeSpan span, CancellationToken token = default)
        {
            SetStatus($"Waiting: {reason} – {DateTime.Now.Add(span):HH:mm}");
            try
            {
                await Task.Delay(span, token);
            }
            finally
            {
                SetStatus($"Resumed after {reason}");
            }
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public void SetStatus(string newStatus)
        {
            _status = newStatus;
            LastActionDescription = newStatus;
            LastActiveTime = DateTime.Now;
            if (_chat != null)
            {
                try
                {
                    using var db = _chat.Ctx();
                    var me = db.Avatars.Find(Id);
                    if (me != null)
                    {
                        me._status = newStatus;
                        me.LastActionDescription = newStatus;
                        me.LastActiveTime = DateTime.Now;
                        db.SaveChanges();
                    }
                }
                catch (DbUpdateException ex)
                {
                    Console.Error.WriteLine($"Database update failed for avatar {Id}: {ex.Message}");
                }
            }
            StatusChanged?.Invoke(this, newStatus);
        }
        // ──────────────────────────────────────────────────────────────────────────────
        public async Task Run(ChatService chat, CancellationToken token)
        {
            if (_chat == null && chat == null)
                throw new ArgumentNullException(nameof(chat));
            var svc = _chat ?? chat;
            string ok = string.Empty;
            IWebDriver? driver = null;
            try
            {
                
                if (string.IsNullOrEmpty(_port))
                    _port = PortAllocator.Next().ToString();
                string assignedProxy;
                try
                {
                    assignedProxy = ProxyManager.GetProxyForAvatar(this);
                }
                catch (ProxyUnavailableException)
                {
                    SetStatus("No proxies available");
                    return;
                }

                _seleniumAvatarBot ??= new SeleniumAvatarBot(Id, svc, _port, _userPath, _city,
                proxy: $"http://{assignedProxy}",
                pUser: ProxyManager.FormattedProxyUsername,
                pPass: ProxyManager.ProxyPassword);
                _seleniumAvatarBot.StatusChanged += (_, s) => SetStatus(s);
                SetStatus("Opening browser");

                try
                {
                    driver = _seleniumAvatarBot.Start();
                    if (token.IsCancellationRequested) { _seleniumAvatarBot.Stop(driver); return; }
                    ok = await _seleniumAvatarBot.FirstLogin(this, driver, token);
                }
                catch(Exception ex)
                {
                    await WaitAsync($"Error {ex.Message}", TimeSpan.FromSeconds(2), token);
                }
            }
            finally
            {
                _seleniumAvatarBot?.Stop(driver);
                svc.UpdateAvatar(this);
                SetStatus("Logged-in");
                InfoUpdated?.Invoke(this, EventArgs.Empty);
                if (ok == "blocked")
                {
                    SetStatus("User Is Blocked");
                    _blocked = true;
                }
                else if (ok == "flase")
                {
                    SetStatus("Login failed");
                }
            }
        }
    }
}
