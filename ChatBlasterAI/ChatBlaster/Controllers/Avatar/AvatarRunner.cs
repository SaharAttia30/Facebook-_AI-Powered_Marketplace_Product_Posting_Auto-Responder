using System.Collections.Concurrent;
using System.Diagnostics;
using ChatBlaster.DB;
using ChatBlaster.Infrastructure;
using ChatBlaster.Models;
using ChatBlaster.Utilities;
using OpenQA.Selenium;

namespace ChatBlaster.Controllers.Avatar
{
    public class AvatarRunner
    {
        //(0:False) (1:True) (-1:Error)
        private static readonly ConcurrentDictionary<Models.Avatar, List<AvatarDailySession>> _today = new();
        private readonly ConcurrentDictionary<Models.Avatar, SeleniumAvatarBot> _bots = new();
        private TimeSpan _morningTimeStart = TimeSpan.FromHours(6);
        private TimeSpan _morningTimeEnd = TimeSpan.FromHours(12);
        private TimeSpan _dayTimeStart = TimeSpan.FromHours(12);
        private TimeSpan _datTimeEnd = TimeSpan.FromHours(17);
        private TimeSpan _eveningTimeStart = TimeSpan.FromHours(17);
        private TimeSpan _eveningTimeEnd = TimeSpan.FromHours(22);
        private static readonly string[] _slots = { "morning", "day", "evening" };
        private readonly ChatService _chat;
        private readonly ConcurrentDictionary<Models.Avatar, CancellationTokenSource> _tokens = new();
        private static readonly SemaphoreSlim _monitorSemaphore = new SemaphoreSlim(AvatarConfig.MaxConcurrentMonitors);
        private static readonly SemaphoreSlim _chromeSemaphore = new SemaphoreSlim(AvatarConfig.MaxConcurrentBrowsers);

        //*------------------------------------------------------------------------------------------------------------*/
        readonly Dictionary<string, List<AvatarDailySession>> _bySlot = new()
        {
            ["morning"] = new(),
            ["day"] = new(),
            ["evening"] = new()
        };
        //*------------------------------------------------------------------------------------------------------------*/
        public AvatarRunner(ChatService chat)
        {
            _chat = chat ?? throw new ArgumentNullException(nameof(chat));
            AppDomain.CurrentDomain.ProcessExit += (_, __) =>
            {
                foreach (var cts in _tokens.Values)
                {
                    cts.Cancel();
                }
            };
        }
        /*------------------------------------------------------------------------------------------------------------*/
        public static Dictionary<string, object> RandAvatarStartupVars(Random rng)
        {
            int sessionCount = rng.Next(1, 4);                         // 1-3
            List<string> periods = sessionCount switch                 // no duplicates
            {
                3 => _slots.ToList(),
                _ => _slots.OrderBy(_ => rng.Next()).Take(sessionCount).ToList()
            };
            int dailyPosts = rng.Next(0, Math.Min(sessionCount, AvatarConfig.MaxPostsToMarketPlace) + 1);
            var mins = Enumerable.Range(0, sessionCount).Select(_ => AvatarConfig.RandomBetween(rng, AvatarConfig.MinSessionLength, AvatarConfig.MaxSessionLength)).ToList();
            return new()
            {
                ["SessionCount"] = sessionCount,
                ["DailyPosts"] = dailyPosts,
                ["SessionsMinTime"] = mins,
                ["Periods"] = periods        // “morning” / “day” / “evening”
            };
        }
        /*------------------------------------------------------------------------------------------------------------*/
        void BuildGroupedSchedules(IReadOnlyList<Models.Avatar> avatars)
        {
            var rng = ThreadSafeRandom.Instance;
            foreach (var l in _bySlot.Values)
            {
                l.Clear();
            }
            var slotAvatars = new Dictionary<string, List<Models.Avatar>>
            {
                { "morning", new List<Models.Avatar>() },  // 06:00-12:00
                { "day",     new List<Models.Avatar>() },  // 12:00-17:00
                { "evening", new List<Models.Avatar>() }   // 17:00-22:00
            };
            var avatarPerformedActions = new Dictionary<Models.Avatar, Dictionary<AvatarActionType, int>>();
            var avatarVars = new Dictionary<Models.Avatar, Dictionary<string, object>>();
            foreach (var avatar in avatars)
            {
                if (_today.ContainsKey(avatar))
                {
                    _today.TryRemove(avatar, out _);
                }

                var vars = RandAvatarStartupVars(rng);
                avatarVars[avatar] = vars;
                avatarPerformedActions[avatar] = new Dictionary<AvatarActionType, int>();
                foreach (var period in (List<string>)vars["Periods"])
                {
                    slotAvatars[period].Add(avatar);
                }
            }

            var today = DateTime.Today;
            if (DateTime.Now < today + _morningTimeEnd)
                ScheduleSlot("morning", _morningTimeStart, _morningTimeEnd);

            if (DateTime.Now < today + _datTimeEnd)
                ScheduleSlot("day", _dayTimeStart, _datTimeEnd);

            if (DateTime.Now < today + _eveningTimeEnd)
                ScheduleSlot("evening", _eveningTimeStart, _eveningTimeEnd);

            foreach (var p in _today)
            {
                p.Value.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            }
            /*------------------------------------------------------------------------------------------------------------*/
            void ScheduleSlot(string slotName, TimeSpan windowStart, TimeSpan windowEnd)
            {
                var list = slotAvatars[slotName];
                if (list.Count == 0)
                {
                    return;
                }
                list = list.OrderBy(_ => rng.Next()).ToList();
                TimeSpan window = windowEnd - windowStart;
                TimeSpan sessionMax = AvatarConfig.MaxSessionLength;
                TimeSpan gap = TimeSpan.Zero;
                if (sessionMax * list.Count < window)
                {
                    var spare = window - sessionMax * list.Count;
                    gap = TimeSpan.FromTicks(spare.Ticks / (list.Count + 1));
                }
                DateTime cursor = DateTime.Today + windowStart + gap;
                cursor = DateTime.Now + TimeSpan.FromMinutes(2) > cursor ? DateTime.Now : cursor;
                foreach (var avatar in list)
                {
                    var vars = avatarVars[avatar];
                    var minTimes = (List<TimeSpan>)vars["SessionsMinTime"];
                    int index = _today.TryGetValue(avatar, out var _sessions) ? _sessions.Count : 0;
                    index = Math.Max(0, Math.Min(index, minTimes.Count - 1));
                    TimeSpan length = Clamp(minTimes[index], AvatarConfig.MinSessionLength, AvatarConfig.MaxSessionLength);
                    var actions = BuildActionList(avatar);
                    while (!IsTimeSlotFree(slotName, cursor, length))
                    {
                        cursor = NextFree(slotName, cursor, length);
                    }
                    if (cursor + length > DateTime.Today + windowEnd)
                    {
                        continue;
                    }
                    if (!_today.TryGetValue(avatar, out _sessions))
                    {
                        _sessions = new List<AvatarDailySession>();
                        _today[avatar] = _sessions;
                    }
                    var session = new AvatarDailySession(cursor, length, actions);
                    _sessions.Add(session);
                    _bySlot[slotName].Add(session);
                    cursor += length + gap;
                }
            }
            /*------------------------------------------------------------------------------------------------------------*/
            bool IsTimeSlotFree(string slot, DateTime start, TimeSpan len)
            {
                var end = start + len;
                return _bySlot[slot].All(s => s.StartTime >= end || s.StartTime + s.Duration <= start);
            }
            /*------------------------------------------------------------------------------------------------------------*/
            DateTime NextFree(string slot, DateTime cursor, TimeSpan len)
            {
                foreach (var s in _bySlot[slot].OrderBy(s => s.StartTime))
                {
                    var gap = s.StartTime - cursor;
                    if (gap >= len) return cursor;               // found space
                    cursor = s.StartTime + s.Duration;           // jump past occupied
                }
                return cursor;
            }
            /*------------------------------------------------------------------------------------------------------------*/
            List<AvatarActionType> BuildActionList(Models.Avatar avatar)
            {
                var list = new List<AvatarActionType> { AvatarActionType.AnswerUnansweredMessages };
                var performedActions = avatarPerformedActions[avatar];
                var extras = new List<AvatarActionType>
                {
                    AvatarActionType.PostToMarketplace,
                    AvatarActionType.MakeAFreind
                };
                var availableExtras = new List<AvatarActionType>();
                foreach (var action in extras)
                {
                    int currentCount = performedActions.ContainsKey(action) ? performedActions[action] : 0;
                    if (currentCount < GetMaxOccurrences(action))
                    {
                        availableExtras.Add(action);
                    }
                }
                int extraCount = rng.Next(1, Math.Min(3, availableExtras.Count + 1));
                for (int i = availableExtras.Count - 1; i > 0; i--)
                {
                    int j = rng.Next(0, i + 1);
                    AvatarActionType temp = availableExtras[i];
                    availableExtras[i] = availableExtras[j];
                    availableExtras[j] = temp;
                }
                for (int i = 0; i < extraCount && i < availableExtras.Count; i++)
                {
                    var action = availableExtras[i];
                    list.Add(action);
                    performedActions[action] = performedActions.ContainsKey(action) ? performedActions[action] + 1 : 1;
                }
                return list;
            }
            /*------------------------------------------------------------------------------------------------------------*/
            int GetMaxOccurrences(AvatarActionType action)
            {
                switch (action)
                {
                    case AvatarActionType.PostToMarketplace:
                        return AvatarConfig.MaxPostsToMarketPlace;
                    case AvatarActionType.MakeAFreind:
                        return AvatarConfig.MaxFreindRequests;
                    case AvatarActionType.AnswerUnansweredMessages:
                        return AvatarConfig.MaxMessages;
                    //case AvatarActionType.PostToGroups:
                    //    return AvatarConfig.MaxPostsToGroups;
                    //case AvatarActionType.CommentOnGroups:
                    //    return AvatarConfig.MaxCommentOnGroups;
                    //case AvatarActionType.CommentOnPrivatePosts:
                    //     return AvatarConfig.MaxCommentOnPrivatePosts;
                    case AvatarActionType.ReactToGroupPosts:
                    case AvatarActionType.ReactToPrivatePosts:
                        return AvatarConfig.MaxReactions;
                    case AvatarActionType.WatchVideos:
                        return AvatarConfig.MaxVideos;
                    default:
                        return 1;
                }
            }
            /*------------------------------------------------------------------------------------------------------------*/
            static TimeSpan Clamp(TimeSpan v, TimeSpan min, TimeSpan max)
                => v < min ? min : v > max ? max : v;
        }
        /*------------------------------------------------------------------------------------------------------------*/
        public async void StartAllAvatars(IEnumerable<Models.Avatar> avatars)
        {
            List<Task> tasks = new List<Task>();
            BuildGroupedSchedules(avatars.ToList());
            foreach (var avatar in avatars)
            {
                _tokens[avatar] = new CancellationTokenSource();
                var task = Task.Run(() => RunAvatarRoutine(avatar));
                task.ContinueWith(t => {
                    Console.Error.WriteLine($"[Avatar {avatar.Id}] Task error: {t.Exception?.GetBaseException().Message}");
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            //StartUnreadMonitors(avatars);
            PrintSchedule();
        }
        /*------------------------------------------------------------------------------------------------------------*/
        public List<AvatarDailySession> GetScheduleForAvatar(Models.Avatar avatar)
        {
            if (avatar == null) return new List<AvatarDailySession>();
            foreach (var avatar_and_its_session in _today)
            {
                if (avatar_and_its_session.Key.Id == avatar.Id)
                {
                    return avatar_and_its_session.Value;
                }
            }
            return new List<AvatarDailySession>();
        }
        /*------------------------------------------------------------------------------------------------------------*/
        public void StopAvatars(IEnumerable<Models.Avatar> list)
        {
            foreach (var av in list)
                if (_tokens.TryRemove(av, out var cts))
                    cts.Cancel();
        }
        /*------------------------------------------------------------------------------------------------------------*/
        public async Task RunAvatarRoutine(Models.Avatar avatar)
        {
            if (!_today.TryGetValue(avatar, out var todaySessions) || todaySessions.Count == 0)
            {
                return;
            }
            if (string.IsNullOrEmpty(avatar._port))
            {
                avatar._port = PortAllocator.Next().ToString();
            }
            DateTime currentDay = DateTime.Now.Date;
            avatar.FriendRequestSentToday = avatar.MessagesAnsweredToday = 0;
            avatar.PostsMadeToday = avatar.CommentsMadeToday = avatar.ReactionsMadeToday = avatar.VideosWatchedToday = 0;
            try
            {
                while (true)
                {
                    var schedule = new AvatarDailySchedule(DateTime.Today, DateTime.Today + AvatarConfig.MinWakeTime, DateTime.Today + AvatarConfig.MaxSleepTime, todaySessions);
                    avatar.LastWakeTime = schedule.WakeTime;
                    avatar.LastSleepTime = schedule.SleepTime;
                    using (var db = _chat.Ctx())
                    {
                        var dbAvatar = db.Avatars.Find(avatar.Id);
                        if (dbAvatar != null)
                        {
                            dbAvatar.LastWakeTime = schedule.WakeTime;
                            dbAvatar.LastSleepTime = schedule.SleepTime;
                            dbAvatar.FriendRequestSentToday = dbAvatar.FriendRequestSentToday = 0;
                            dbAvatar.PostsMadeToday = dbAvatar.CommentsMadeToday = dbAvatar.ReactionsMadeToday = dbAvatar.VideosWatchedToday = 0;
                            dbAvatar.LastActionDescription = $"Scheduled wake at {schedule.WakeTime:t}, sleep at {schedule.SleepTime:t}";
                            dbAvatar.LastActiveTime = DateTime.Now;
                            db.SaveChanges();
                        }
                    }
                    TimeSpan untilWake = schedule.WakeTime - DateTime.Now;
                    if (untilWake.TotalMilliseconds > 0)
                    {
                        await avatar.WaitAsync("wake-time", untilWake, _tokens[avatar].Token);
                    }
                    foreach (AvatarDailySession session in schedule.Sessions)
                    {
                        TimeSpan wait = session.StartTime - DateTime.Now;
                        if (wait.TotalMilliseconds > 0)
                        {
                            await avatar.WaitAsync("session-start", wait, _tokens[avatar].Token);
                        }
                        if (!(DateTime.Now < schedule.SleepTime))
                        {
                            break;
                        }
                        await ExecuteAvatarSessionAsync(avatar, schedule, session);
                    }
                    DateTime tomorrow = DateTime.Now.Date.AddDays(1);
                    TimeSpan nextWakeOffset = TimeSpan.FromMinutes(new Random().Next((int)AvatarConfig.MinWakeTime.TotalMinutes, (int)AvatarConfig.MaxWakeTime.TotalMinutes + 1));
                    DateTime nextWakeTime = tomorrow + nextWakeOffset;
                    TimeSpan sleepDuration = nextWakeTime - DateTime.Now;
                    using (var db = _chat.Ctx())
                    {
                        if (sleepDuration > TimeSpan.Zero)
                        {
                            var dbAvatar = db.Avatars.Find(avatar.Id);
                            if (dbAvatar != null)
                            {
                                dbAvatar.LastActionDescription = $"Sleeping until next wake time ({nextWakeTime:t})";
                                dbAvatar.LastActiveTime = DateTime.Now;
                                db.SaveChanges();
                            }
                            await avatar.WaitAsync("sleep", sleepDuration, _tokens[avatar].Token);
                        }
                    }
                    currentDay = DateTime.Now.Date;
                }
            }
            catch (Exception ex)
            {
                await avatar.WaitAsync($"Error in RunAvatarRoutine {ex.Message}", TimeSpan.FromSeconds(3), _tokens[avatar].Token);
            }
            finally
            {
                avatar.Dispose();
            }
        }
        /*------------------------------------------------------------------------------------------------------------*/
        async Task ExecuteAvatarSessionAsync(Models.Avatar avatar, AvatarDailySchedule schedule, AvatarDailySession session)
        {
            await _chromeSemaphore.WaitAsync();
            SeleniumAvatarBot bot = null;
            IWebDriver drv = null;
            try
            {
                using (var db = _chat.Ctx())
                {
                    var dbAvatar = db.Avatars.Find(avatar.Id);
                    if (dbAvatar != null)
                    {
                        dbAvatar.LastActionDescription = $"Starting session at {DateTime.Now:t} (opening browser)";
                        dbAvatar.LastActiveTime = DateTime.Now;
                        db.SaveChanges();
                    }
                }
                IEnumerable<AvatarActionType> action_session_list = session.Actions;
                while (true)
                {
                    try
                    {
                        //(0:False) (1:True) (-1:Error)
                        //int result_unansered_messages = await HasUnreadMessagesAsync(avatar, _tokens[avatar].Token);
                        //action_session_list = result_unansered_messages == 0 ? session.Actions.Skip(1).ToList() : session.Actions;
                        //if (!action_session_list.Any())
                        //{
                        //    return;
                        //}
                        string proxyHostPort = ProxyManager.GetProxyForAvatar(avatar);
                        bot = new SeleniumAvatarBot(avatar.Id, _chat, avatar._port, avatar._userPath, avatar._city,
                        proxy: $"http://{proxyHostPort}", pUser: ProxyManager.FormattedProxyUsername, pPass: ProxyManager.ProxyPassword);
                        drv = bot.Start();
                        _bots[avatar] = bot;
                        break;
                    }

                    catch (InvalidOperationException ex) when (ex.Message.Contains("Proxy check failed"))
                    {
                        ProxyManager.MarkProxyBad(avatar);
                        continue;
                    }
                }
                foreach (AvatarActionType action in action_session_list)
                {
                    bool success = false;
                    try
                    {
                        try
                        {
                            if (!CanPerformAction(avatar, action))
                            {
                                Console.WriteLine($"[Avatar {avatar.Id}] Skipping {action} (daily limit reached)");
                                continue;
                            }
                            success = await TryPerformActionAsync(avatar, drv, bot, action);
                            UpdateAvatarCounters(avatar, schedule, action, success);
                        }
                        catch
                        {
                            success = false;
                        }
                        using (var db = _chat.Ctx())
                        {
                            var dbAvatar = db.Avatars.Find(avatar.Id);
                            if (dbAvatar != null)
                            {
                                dbAvatar.LastActiveTime = DateTime.Now;
                                dbAvatar.LastActionDescription = success ? $"Completed {action} at {DateTime.Now:t}" : $"{action} failed at {DateTime.Now:t}";
                                dbAvatar.FriendRequestSentToday = avatar.FriendRequestSentToday;
                                dbAvatar.MessagesAnsweredToday = avatar.MessagesAnsweredToday;
                                dbAvatar.PostsMadeToday = avatar.PostsMadeToday;
                                dbAvatar.CommentsMadeToday = avatar.CommentsMadeToday;
                                dbAvatar.ReactionsMadeToday = avatar.ReactionsMadeToday;
                                dbAvatar.VideosWatchedToday = avatar.VideosWatchedToday;
                                db.SaveChanges();
                            }
                        }
                        if (action != session.Actions[^1])
                        {
                            int delayMs = new Random().Next((int)AvatarConfig.ActionDelayMin.TotalMilliseconds, (int)AvatarConfig.ActionDelayMax.TotalMilliseconds + 1);
                            await avatar.WaitAsync("sleep", TimeSpan.FromMilliseconds(delayMs), _tokens[avatar].Token);
                        }
                    }
                    catch (Exception ex)
                    {
                        await avatar.WaitAsync($"eroor at ExecuteAvatarSessionAsync {ex.Message}", TimeSpan.FromSeconds(3), _tokens[avatar].Token);
                    }
                }
                DateTime start_time = DateTime.Now;
                using (var db = _chat.Ctx())
                {
                    var dbAvatar = db.Avatars.Find(avatar.Id);
                    if (dbAvatar != null)
                    {
                        start_time = dbAvatar.LastActiveTime;
                    }
                }
                var sessionEnd = session.StartTime + session.Duration;
                var timeLeft = sessionEnd - DateTime.Now;
                if (timeLeft > TimeSpan.Zero)
                {
                    await bot.ApproveNumberOfFriends(avatar, drv, 2, _tokens[avatar].Token);
                    await bot.DefaultScroll(avatar, drv, sessionEnd, _tokens[avatar].Token);
                }
            }
            catch (Exception ex)
            {
                if (ex is ProxyUnavailableException)
                {
                    Console.Error.WriteLine($"[Avatar {avatar.Id}] Proxy assignment failed: {ex.Message}");
                    using var db = _chat.Ctx();
                    var dbAvatar = db.Avatars.Find(avatar.Id);
                    if (dbAvatar != null)
                    {
                        dbAvatar.LastActionDescription = "No proxies available";
                        dbAvatar.LastActiveTime = DateTime.Now;
                        db.SaveChanges();
                    }
                    _tokens[avatar].Cancel();
                    return;
                }
                using (var db = _chat.Ctx())
                {
                    Console.Error.WriteLine($"[Avatar {avatar.Id}] ERROR during session: {ex.Message}");
                    var dbAvatar = db.Avatars.Find(avatar.Id);
                    if (dbAvatar != null)
                    {
                        dbAvatar.LastActionDescription = $"Session error: {ex.Message}";
                        dbAvatar.LastActiveTime = DateTime.Now;
                        db.SaveChanges();
                    }
                }
            }
            finally
            {
                _chromeSemaphore.Release();
                try
                {

                    bot?.Stop(drv);
                    _bots.TryRemove(avatar, out _);
                }
                catch { /* ignore */ }
                using (var db = _chat.Ctx())
                {
                    var dbAvatar = db.Avatars.Find(avatar.Id);
                    if (dbAvatar != null)
                    {
                        dbAvatar.LastActionDescription = $"Finished session at {DateTime.Now:t}, browser closed";
                        dbAvatar.LastActiveTime = DateTime.Now;
                        db.SaveChanges();
                    }
                }
            }
        }
        /*------------------------------------------------------------------------------------------------------------*/
        public void StopAllAvatarsNow()
        {
            foreach (var kvp in _tokens)
            {
                try { kvp.Value.Cancel(); } catch { }
            }

            foreach (var bot in _bots.Values)
            {
                try
                {
                    var drv = bot.GetDriver();
                    bot.Stop(drv);
                }
                catch { }
            }

            _bots.Clear();

            foreach (var proc in Process.GetProcessesByName("chromedriver"))
            {
                try { proc.Kill(); } catch { }
            }

            foreach (var proc in Process.GetProcessesByName("chrome"))
            {
                try { proc.Kill(); } catch { }
            }

            Console.WriteLine("✅ All avatars stopped and Chrome processes terminated.");
        }
        /*------------------------------------------------------------------------------------------------------------*/
        private bool CanPerformAction(Models.Avatar avatar, AvatarActionType action)
        {
            switch (action)
            {
                case AvatarActionType.MakeAFreind:
                    return avatar.FriendRequestSentToday < AvatarConfig.MaxFreindRequests;
                case AvatarActionType.AnswerUnansweredMessages:
                    return true;
                case AvatarActionType.PostToMarketplace:
                case AvatarActionType.PostToGroups:
                    return avatar.PostsMadeToday < AvatarConfig.MaxPostsToMarketPlace;
                case AvatarActionType.CommentOnGroups:
                case AvatarActionType.CommentOnPrivatePosts:
                    return avatar.CommentsMadeToday < AvatarConfig.MaxComments;
                case AvatarActionType.ReactToGroupPosts:
                case AvatarActionType.ReactToPrivatePosts:
                    return avatar.ReactionsMadeToday < AvatarConfig.MaxReactions;
                case AvatarActionType.WatchVideos:
                    return avatar.VideosWatchedToday < AvatarConfig.MaxVideos;
                default:
                    return true;
            }
        }
        /*------------------------------------------------------------------------------------------------------------*/
        private async Task<bool> TryPerformActionAsync(Models.Avatar avatar, IWebDriver driver, SeleniumAvatarBot bot, AvatarActionType action)
        {
            string actionName = action.ToString();
            int maxRetries = 2;
            Random rnd = ThreadSafeRandom.Instance;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    Console.WriteLine($"[Avatar {avatar.Id}] Performing {actionName} (attempt {attempt})...");
                    switch (action)
                    {
                        case AvatarActionType.MakeAFreind:
                            await MakeAFreind(avatar, driver, bot, _tokens[avatar].Token);
                            break;
                        case AvatarActionType.AnswerUnansweredMessages:
                            await AnswerUnansweredMessagesAsync(avatar, driver, bot, _tokens[avatar].Token);
                            break;
                        case AvatarActionType.PostToMarketplace:
                            await PostToMarketplace(avatar, driver, bot, _tokens[avatar].Token);
                            break;
                        case AvatarActionType.PostToGroups:
                            await PostToGroups(avatar, driver, bot, _tokens[avatar].Token);
                            break;
                        case AvatarActionType.CommentOnGroups:
                            await CommentOnGroups(avatar, driver, bot, _tokens[avatar].Token);
                            break;
                        case AvatarActionType.CommentOnPrivatePosts:
                            await CommentOnPrivatePosts(avatar, driver, bot, _tokens[avatar].Token);
                            break;
                        case AvatarActionType.ReactToGroupPosts:
                            await ReactToGroupPosts(avatar, driver, bot, _tokens[avatar].Token);
                            break;
                        case AvatarActionType.ReactToPrivatePosts:
                            await ReactToPrivatePosts(avatar, driver, bot, _tokens[avatar].Token);
                            break;
                        case AvatarActionType.WatchVideos:
                            await WatchVideos(avatar, driver, bot, _tokens[avatar].Token);
                            break;
                    }
                    Console.WriteLine($"[Avatar {avatar.Id}] {actionName} succeeded.");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[Avatar {avatar.Id}] {actionName} failed on attempt {attempt}: {ex.Message}");
                    if (attempt == maxRetries)
                    {
                        return false;
                    }
                    else
                    {
                        await avatar.WaitAsync("Small delay for issues to resolve", TimeSpan.FromMilliseconds(3000), _tokens[avatar].Token);
                    }
                }
            }
            return false;
        }
        // ────────────────────────────────────────────────────────────────────────────────
        private async Task CheckForUnreadMessagesForMonitorAsync(Models.Avatar avatar, CancellationToken token)
        {
            await _monitorSemaphore.WaitAsync();
            SeleniumAvatarBot? bot = null;
            try
            {
                if (_bots.ContainsKey(avatar))
                {
                    return;  // skip monitor – release will happen in finally
                }
                string proxyHostPort = ProxyManager.GetProxyForAvatar(avatar);
                bot = new SeleniumAvatarBot(avatar.Id, _chat, avatar._port, avatar._userPath, avatar._city,
                    proxy: $"http://{proxyHostPort}",
                    pUser: ProxyManager.FormattedProxyUsername,
                    pPass: ProxyManager.ProxyPassword);
                try
                {
                    int result_has_unread = await HasUnreadMessagesAsync(avatar, token);
                    if (result_has_unread == 1 && !token.IsCancellationRequested)
                    {
                        IWebDriver driver =bot.Start();
                        await bot.AnswerUnansweredMessagesAsync(avatar, driver, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    await avatar.WaitAsync($"[Unread-check] {avatar._userName}: {ex.Message}", TimeSpan.FromSeconds(5), token);
                }
            }
            catch (OperationCanceledException) { /* normal shutdown */ }
            catch (Exception ex)
            {
                await avatar.WaitAsync($"[Unread-check] {avatar._userName}: {ex.Message}", TimeSpan.FromSeconds(5), token);
            }
            finally
            {
                _monitorSemaphore.Release();
                try
                {
                    var drv = bot?.GetDriver();
                    bot?.Stop(drv);
                    if (_bots.TryGetValue(avatar, out var existingBot) && existingBot == bot)
                    {
                        _bots.TryRemove(avatar, out _);
                    }
                }
                catch { }
            }
        }
        // ────────────────────────────────────────────────────────────────────────────────
        //(0:False) (1:True) (-1:Error)
        private async Task<int> HasUnreadMessagesAsync(Models.Avatar avatar, CancellationToken token)
        {
            string proxyHostPort = ProxyManager.GetProxyForAvatar(avatar);
            var bot = new SeleniumAvatarBot(
                avatar.Id, _chat, avatar._port, avatar._userPath, avatar._city,
                proxy: $"http://{proxyHostPort}",
                pUser: ProxyManager.FormattedProxyUsername,
                pPass: ProxyManager.ProxyPassword);
            int return_has_unread = 0;
            try
            {
                IWebDriver driver = bot.StartHeadless();
                bool has_unread = await bot.HasUnreadAsync(avatar, driver, token);
                return_has_unread = has_unread ? 1 : 0;
            }
            catch (Exception ex)
            {
                await avatar.WaitAsync($"[Unread-check] {avatar._userName}: {ex.Message}", TimeSpan.FromSeconds(5), token);
                return_has_unread = -1;
            }
            finally
            {
                var drv = bot.GetDriver();
                bot?.Stop(drv);
                _bots.TryRemove(avatar, out _);
            }
            return return_has_unread;
        }
        // ────────────────────────────────────────────────────────────────────────────────
        private void StartUnreadMonitors(IEnumerable<Models.Avatar> avatars)
        {
            var rng = ThreadSafeRandom.Instance;


            foreach (var avatar in avatars)
            {
                if (!_tokens.TryGetValue(avatar, out var cts)) continue;

                Task.Run(async () =>
                {
                    var timer = new PeriodicTimer(AvatarConfig.RandomBetween(rng, TimeSpan.FromMilliseconds(300000), TimeSpan.FromMilliseconds(600000)));
                    try
                    {
                        while (await timer.WaitForNextTickAsync(cts.Token))
                        {
                            try
                            {
                                if (cts.Token.IsCancellationRequested) break;

                                await CheckForUnreadMessagesForMonitorAsync(avatar, cts.Token);
                            }
                            catch (OperationCanceledException)
                            {
                                break; // Normal exit
                            }
                            catch (Exception ex)
                            {
                                await avatar.WaitAsync($"Monitor Error: {ex.Message}", TimeSpan.FromSeconds(3), cts.Token);
                            }
                        }
                    }
                    finally
                    {
                        timer.Dispose();
                    }
                }, cts.Token);
            }
        }

        /*------------------------------------------------------------------------------------------------------------*/
        private void UpdateAvatarCounters(Models.Avatar avatar, AvatarDailySchedule schedule, AvatarActionType action, bool success)
        {
            if (!success) return;

            switch (action)
            {
                case AvatarActionType.MakeAFreind:
                    schedule.FriendRequestSentToday = avatar.FriendRequestSentToday;
                    break;
                case AvatarActionType.AnswerUnansweredMessages:
                    schedule.MessagesAnsweredToday = avatar.MessagesAnsweredToday;
                    break;
                case AvatarActionType.PostToMarketplace:
                case AvatarActionType.PostToGroups:
                    schedule.PostsMadeToday = avatar.PostsMadeToday;
                    break;
                case AvatarActionType.CommentOnGroups:
                case AvatarActionType.CommentOnPrivatePosts:
                    //avatar.CommentsMadeToday += 1;
                    schedule.CommentsMadeToday = avatar.CommentsMadeToday;
                    break;
                case AvatarActionType.ReactToGroupPosts:
                case AvatarActionType.ReactToPrivatePosts:
                    //avatar.ReactionsMadeToday += 1;
                    schedule.ReactionsMadeToday = avatar.ReactionsMadeToday;
                    break;
                case AvatarActionType.WatchVideos:
                    //avatar.VideosWatchedToday += 1;
                    schedule.VideosWatchedToday = avatar.VideosWatchedToday;
                    break;
            }
        }
        // ===== Avatar action methods (stubs calling SeleniumAvatarBot) =====
        /*------------------------------------------------------------------------------------------------------------*/
        private async Task MakeAFreind(Models.Avatar avatar, IWebDriver driver, SeleniumAvatarBot bot, CancellationToken token)
        {
            if (avatar._blocked)
            {
                await avatar.WaitAsync($"This user is Blocked By Facebook", TimeSpan.FromSeconds(2), token);
                return;
            }
            await bot.MakeAFreind(avatar, driver, token);
            // TODO: Implement actual message-sending logic with Selenium
            //await bot.PostToGroups(avatar, token);
            //await bot.CreateAndUploadPostToMarketPlace(avatar, token);
            //await bot.AnswerUnansweredMessagesAsync(avatar, token);
        }
        /*------------------------------------------------------------------------------------------------------------*/
        private async Task AnswerUnansweredMessagesAsync(Models.Avatar avatar,IWebDriver driver, SeleniumAvatarBot bot, CancellationToken token)
        {
            if (avatar._blocked)
            {
                await avatar.WaitAsync($"This user is Blocked By Facebook", TimeSpan.FromSeconds(2), token);
                return;
            }
            await bot.AnswerUnansweredMessagesAsync(avatar, driver, token);
            //await bot.CreateAndUploadPostToMarketPlace(avatar, token);
        }
        /*------------------------------------------------------------------------------------------------------------*/
        private async Task PostToMarketplace(Models.Avatar avatar, IWebDriver driver, SeleniumAvatarBot bot, CancellationToken token)
        {
            if (avatar._blocked)
            {
                await avatar.WaitAsync($"This user is Blocked By Facebook", TimeSpan.FromSeconds(2), token);
                return;
            }
            await bot.CreateAndUploadPostToMarketPlace(avatar, driver, token);

            //await bot.PostToGroups(avatar, token);
            //await bot.AnswerUnansweredMessagesAsync(avatar, token);
        }
        /*------------------------------------------------------------------------------------------------------------*/
        private async Task PostToGroups(Models.Avatar avatar, IWebDriver driver, SeleniumAvatarBot bot, CancellationToken token)
        {
            if (avatar._blocked)
            {
                await avatar.WaitAsync($"This user is Blocked By Facebook", TimeSpan.FromSeconds(2), token);
                return;
            }
            // TODO: Implement posting logic with Selenium
            //await bot.PostToGroups(avatar, token);
            //await bot.CreateAndUploadPostToMarketPlace(avatar, token);
            //await bot.AnswerUnansweredMessagesAsync(avatar, token);
        }
        /*------------------------------------------------------------------------------------------------------------*/
        private async Task CommentOnGroups(Models.Avatar avatar, IWebDriver driver, SeleniumAvatarBot bot, CancellationToken token)
        {
            // Use SeleniumAvatarBot to comment on group posts
            // TODO: Implement commenting logic with Selenium
            //bot.CommentOnGroups(avatar, token);
            if (avatar._blocked)
            {
                await avatar.WaitAsync($"This user is Blocked By Facebook", TimeSpan.FromSeconds(2), token);
                return;
            }
            //await bot.PostToGroups(avatar, token);
            //await bot.CreateAndUploadPostToMarketPlace(avatar, token);
            //await bot.AnswerUnansweredMessagesAsync(avatar, token);
        }
        /*------------------------------------------------------------------------------------------------------------*/
        private async Task CommentOnPrivatePosts(Models.Avatar avatar, IWebDriver driver, SeleniumAvatarBot bot, CancellationToken token)
        {
            if (avatar._blocked)
            {
                await avatar.WaitAsync($"This user is Blocked By Facebook", TimeSpan.FromSeconds(2), token);
                return;
            }
            //await bot.PostToGroups(avatar, token);
            // TODO: Implement commenting logic with Selenium
            //await bot.CommentOnPrivatePosts(avatar, token);
            //await bot.CreateAndUploadPostToMarketPlace(avatar, token);
            //await bot.AnswerUnansweredMessagesAsync(avatar, token);

        }
        /*------------------------------------------------------------------------------------------------------------*/
        private async Task ReactToGroupPosts(Models.Avatar avatar, IWebDriver driver, SeleniumAvatarBot bot, CancellationToken token)
        {
            if (avatar._blocked)
            {
                await avatar.WaitAsync($"This user is Blocked By Facebook", TimeSpan.FromSeconds(2), token);
                return;
            }
            //await bot.PostToGroups(avatar, token);
            // TODO: Implement reacting logic with Selenium
            //bot.ReactToGroupPosts(avatar);
            //await bot.CreateAndUploadPostToMarketPlace(avatar, token);
            //await bot.AnswerUnansweredMessagesAsync(avatar, token);
        }
        /*------------------------------------------------------------------------------------------------------------*/
        private async Task ReactToPrivatePosts(Models.Avatar avatar, IWebDriver driver, SeleniumAvatarBot bot, CancellationToken token)
        {
            if (avatar._blocked)
            {
                await avatar.WaitAsync($"This user is Blocked By Facebook", TimeSpan.FromSeconds(2), token);
                return;
            }
            //await bot.PostToGroups(avatar, token);
            // TODO: Implement reacting logic with Selenium
            //bot.ReactToPrivatePosts(avatar);
            //await bot.CreateAndUploadPostToMarketPlace(avatar, token);
            //await bot.AnswerUnansweredMessagesAsync(avatar, token);
        }
        /*------------------------------------------------------------------------------------------------------------*/
        private async Task WatchVideos(Models.Avatar avatar, IWebDriver driver, SeleniumAvatarBot bot, CancellationToken token)
        {
            if (avatar._blocked)
            {
                await avatar.WaitAsync($"This user is Blocked By Facebook", TimeSpan.FromSeconds(2), token);
                return;
            }
            //await bot.PostToGroups(avatar, token);
            // TODO: Implement video-watching logic with Selenium
            //bot.WatchVideos(avatar);
            //await bot.CreateAndUploadPostToMarketPlace(avatar, token);
            //await bot.AnswerUnansweredMessagesAsync(avatar, token);

        }
        /*------------------------------------------------------------------------------------------------------------*/
        void PrintSchedule()
        {
            Console.WriteLine($"──────── DAILY SCHEDULE ({DateTime.Today:yyyy-MM-dd}) ────────");
            foreach (var (avatar, sessions) in _today.OrderBy(p => p.Key._userName))
            {
                Console.WriteLine($"Avatar {avatar._userName} [{avatar.Id}]");
                foreach (var s in sessions.OrderBy(s => s.StartTime))
                {
                    string slot = SlotName(s.StartTime.TimeOfDay);
                    string acts = string.Join(", ", s.Actions);
                    Console.WriteLine($"  {s.StartTime:HH:mm}  {slot,-7}  {acts}");
                }
            }
            Console.WriteLine("───────────────────────────────────────────────────────────────");
        }
        /*------------------------------------------------------------------------------------------------------------*/
        static string SlotName(TimeSpan t) => t < TimeSpan.FromHours(12) ? "morning" : t < TimeSpan.FromHours(17) ? "day" : "evening";
        /*------------------------------------------------------------------------------------------------------------*/
    }
}