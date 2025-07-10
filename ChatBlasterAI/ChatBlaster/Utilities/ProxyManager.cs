using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ChatBlaster.Models;
using ChatBlaster.DB;

namespace ChatBlaster.Utilities
{
    public static class ProxyManager
    {
        private static readonly object _lock = new object();
        private static HashSet<string> availableProxies = new HashSet<string>();
        private static HashSet<string> badProxies = new HashSet<string>();
        private static Dictionary<string, string> assignedProxies = new Dictionary<string, string>();
        private static ChatService _chatService;
        public static string ProxyUsername { get; set; } = "saracochavi";
        public static string ProxyPassword { get; set; } = "JG2XT9J-DWAMXE2-1CSJNBW-WQAGCNT-XZYIP16-I6PGEVG-WXDFVQT";
        public static string CountryCode { get; set; } = "US";
        public static string City { get; set; } = "";
        // Username formatted with country parameter if needed (e.g., "saracochavi-country-US-city-LosAngeles")
        public static string FormattedProxyUsername
        {
            get
            {
                if (string.IsNullOrEmpty(City))
                {
                    if (string.IsNullOrEmpty(CountryCode))
                    {
                        return ProxyUsername;
                    }
                    return $"{ProxyUsername}-country-{CountryCode}";
                }
                return $"{ProxyUsername}-country-{CountryCode}-city-{City}";
            }
        }
        public static void Initialize(IEnumerable<Avatar> avatars, ChatService chatService)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            // Load all proxies from "proxy1.txt"
            if (!File.Exists("proxy1.txt"))
                throw new FileNotFoundException("Proxy list file 'proxy1.txt' not found");
            var proxyList = File.ReadAllLines("proxy1.txt")
                                 .Select(l => l.Trim())
                                 .Where(l => !string.IsNullOrEmpty(l))
                                 .ToList();
            // Load bad proxies from "bad_proxies.txt" if it exists
            if (File.Exists("bad_proxies.txt"))
            {
                foreach (string line in File.ReadAllLines("bad_proxies.txt"))
                {
                    string badProxy = line.Trim();
                    if (!string.IsNullOrEmpty(badProxy))
                        badProxies.Add(badProxy);
                }
            }
            availableProxies = new HashSet<string>(proxyList.Where(p => !badProxies.Contains(p)));
            lock (_lock)
            {
                foreach (var avatar in avatars)
                {
                    if (!string.IsNullOrEmpty(avatar.ProxyAddress))
                    {
                        string proxy = avatar.ProxyAddress;
                        if (badProxies.Contains(proxy))
                        {
                            avatar.ProxyAddress = null;
                        }
                        else
                        {
                            assignedProxies[avatar.Id] = proxy;
                            availableProxies.Remove(proxy);
                        }
                    }
                }
            }
        }
        public static bool HasAvailableProxies()
        {
            lock (_lock)
            {
                return availableProxies.Count > 0;
            }
        }

        public static int AvailableCount
        {
            get { lock (_lock) { return availableProxies.Count; } }
        }

        public static string GetProxyForAvatar(Avatar avatar)
        {
            lock (_lock)
            {
                City = avatar._city;
                if (assignedProxies.TryGetValue(avatar.Id, out string existingProxy))
                {
                    return existingProxy;
                }
                if (availableProxies.Count == 0)
                {
                    throw new ProxyUnavailableException("No more proxies available");
                }
                string newProxy = availableProxies.First();
                availableProxies.Remove(newProxy);
                assignedProxies[avatar.Id] = newProxy;
                avatar.ProxyAddress = newProxy; 
                return newProxy;
            }
        }

        public static void ReleaseProxy(string avatarId)
        {
            lock (_lock)
            {
                if (assignedProxies.TryGetValue(avatarId, out string proxy))
                {
                    if (!badProxies.Contains(proxy))
                        availableProxies.Add(proxy);
                    assignedProxies.Remove(avatarId);
                }
            }
        }

        /// <summary>Mark the current proxy for an avatar as bad and assign a new one.</summary>
        public static void MarkProxyBad(Avatar avatar)
        {
            lock (_lock)
            {
                string avatarId = avatar.Id;
                if (!assignedProxies.TryGetValue(avatarId, out string badProxy))
                    return; 

                badProxies.Add(badProxy);
                assignedProxies.Remove(avatarId);
                try
                {
                    File.AppendAllText("bad_proxies.txt", badProxy + Environment.NewLine);
                }
                catch { /* Ignore*/ }
                availableProxies.Remove(badProxy);
                if (availableProxies.Count == 0)
                {
                    avatar.ProxyAddress = null;
                    using (var db = _chatService.Ctx())
                    {
                        var dbAvatar = db.Avatars.Find(avatarId);
                        if (dbAvatar != null)
                        {
                            dbAvatar.ProxyAddress = null;
                            db.SaveChanges();
                        }
                    }
                    throw new ProxyUnavailableException("No more proxies available");
                }
                string newProxy = availableProxies.First();
                availableProxies.Remove(newProxy);
                assignedProxies[avatarId] = newProxy;
                avatar.ProxyAddress = newProxy;
                using (var db = _chatService.Ctx())
                {
                    var dbAvatar = db.Avatars.Find(avatarId);
                    if (dbAvatar != null)
                    {
                        dbAvatar.ProxyAddress = newProxy;
                        db.SaveChanges();
                    }
                }
            }
        }
    }
    public class ProxyUnavailableException : Exception
    {
        public ProxyUnavailableException(string message) : base(message) { }
    }
}
