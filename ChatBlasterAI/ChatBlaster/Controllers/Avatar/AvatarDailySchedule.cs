using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBlaster.Controllers.Avatar
{
    public sealed class AvatarDailySchedule
    {
        // ──────────────────────────────────────────────────────────────────────────────
        /* ---------- public surface ---------- */
        // ──────────────────────────────────────────────────────────────────────────────
        public DateTime Date { get; }
        public DateTime WakeTime { get; }
        public DateTime SleepTime { get; }
        public IReadOnlyList<AvatarDailySession> Sessions { get; }
        public int FriendRequestSentToday { get; set; }
        public int MessagesAnsweredToday { get; set; }
        public int PostsMadeToday { get; set; }
        public int CommentsMadeToday { get; set; }
        public int ReactionsMadeToday { get; set; }
        public int VideosWatchedToday { get; set; }
        // ──────────────────────────────────────────────────────────────────────────────
        private static readonly AvatarActionType[] AllDailyActions =
        {
            AvatarActionType.MakeAFreind,
            AvatarActionType.PostToMarketplace,
            AvatarActionType.PostToGroups,
            AvatarActionType.CommentOnGroups,
            AvatarActionType.CommentOnPrivatePosts,
            AvatarActionType.ReactToGroupPosts,
            AvatarActionType.ReactToPrivatePosts,
            AvatarActionType.WatchVideos
        };
        // ──────────────────────────────────────────────────────────────────────────────
        public AvatarDailySchedule(DateTime date, DateTime wake, DateTime sleep, IReadOnlyList<AvatarDailySession> sessions)
        {
            Date = date;
            WakeTime = wake;
            SleepTime = sleep;
            Sessions = sessions;
        }
    }
    // ──────────────────────────────────────────────────────────────────────────────
    /* ---------- support record ---------- */
    // ──────────────────────────────────────────────────────────────────────────────
    public sealed class AvatarDailySession
    {
        public DateTime StartTime { get; init; }
        public TimeSpan Duration { get; init; }
        public IReadOnlyList<AvatarActionType> Actions { get; init; }
        // ──────────────────────────────────────────────────────────────────────────────
        public AvatarDailySession(DateTime startTime, TimeSpan duration, IReadOnlyList<AvatarActionType> actions)
        {
            StartTime = startTime;
            Duration = duration;
            Actions = actions;
        }
    }
    // ──────────────────────────────────────────────────────────────────────────────
    /* ---------- enum (unchanged) ---------- */
    // ──────────────────────────────────────────────────────────────────────────────
    public enum AvatarActionType
    {
        MakeAFreind,
        AnswerUnansweredMessages,
        PostToMarketplace,
        PostToGroups,
        CommentOnGroups,
        CommentOnPrivatePosts,
        ReactToGroupPosts,
        ReactToPrivatePosts,
        WatchVideos
    }
    // ──────────────────────────────────────────────────────────────────────────────
    /* ---------- shared random helper ---------- */
    // ──────────────────────────────────────────────────────────────────────────────
    internal static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random? _local;
        public static Random Instance => _local ??= new Random(unchecked(Environment.TickCount * 31 + Environment.CurrentManagedThreadId));
    }
}

























































//private static IEnumerable<AvatarDailySession> PlanSessions(DateTime wake, DateTime sleep, Random rng)
//{
//    // Determine a random number of sessions (1 to 3) for the day
//    Dictionary<string, object> avatar_varuble_dict = RandAvatarStartupVars(rng);
//    int session_count = (int)avatar_varuble_dict["SessionCount"];

//    // Calculate total awake duration
//    TimeSpan totalAwake = sleep - wake;
//    if (session_count == 0 || totalAwake <= TimeSpan.Zero)
//        yield break;  // (just a safeguard; sessionCount is at least 1)

//    // We will select session start times uniformly at random over the awake period 
//    // while enforcing a minimum gap between sessions and leaving room for session duration.
//    TimeSpan minGap = AvatarConfig.MinGapBetweenSessions;
//    TimeSpan maxSession = AvatarConfig.MaxSessionLength;
//    // Ensure there's space for all sessions plus their durations:
//    if ((session_count - 1) * minGap + maxSession > totalAwake)
//    {
//        // If the awake period is too short to fit all sessions with required spacing, reduce sessionCount.
//        session_count = Math.Max(1, (int)((totalAwake - maxSession) / minGap) + 1);
//    }

//    // Compute an available span for random start offsets after accounting for gaps and session length.
//    TimeSpan guaranteedSpacing = (session_count - 1) * minGap + maxSession;
//    TimeSpan availableSpan = totalAwake - guaranteedSpacing;
//    if (availableSpan < TimeSpan.Zero) availableSpan = TimeSpan.Zero;

//    // Generate random start offsets (relative to wake) ensuring minimum gaps.
//    var randomOffsets = new double[session_count];
//    for (int i = 0; i < session_count; i++)
//    {
//        randomOffsets[i] = rng.NextDouble() * availableSpan.Ticks;
//    }
//    Array.Sort(randomOffsets);

//    DateTime[] startTimes = new DateTime[session_count];
//    for (int i = 0; i < session_count; i++)
//    {
//        // Each start time = wake + sortedOffset + gap * i
//        long offsetTicks = (long)randomOffsets[i];
//        TimeSpan offset = TimeSpan.FromTicks(offsetTicks);
//        startTimes[i] = wake + offset + TimeSpan.FromTicks(minGap.Ticks * i);
//    }

//    // Adjust start times if needed to ensure they are within [wake, sleep - maxSession] bounds
//    // (They should be by construction, but we enforce just in case due to rounding.)
//    for (int i = 0; i < session_count; i++)
//    {
//        if (startTimes[i] < wake) startTimes[i] = wake;
//        if (startTimes[i] > sleep - maxSession)
//            startTimes[i] = sleep - maxSession;
//        // Also ensure each subsequent session is at least minGap after the previous one:
//        if (i > 0 && startTimes[i] < startTimes[i - 1] + minGap)
//        {
//            startTimes[i] = startTimes[i - 1] + minGap;
//        }
//    }
//    Array.Sort(startTimes); // resort in case adjustments changed order

//    // Define the pool of possible actions (excluding the mandatory first action)
//        AvatarActionType[] actionPool = {
//        AvatarActionType.ReactToPrivatePosts,
//        AvatarActionType.ReactToGroupPosts,
//        AvatarActionType.CommentOnPrivatePosts,
//        AvatarActionType.CommentOnGroups,
//        AvatarActionType.PostToGroups,
//        AvatarActionType.PostToMarketplace
//    };

//    // Create each session with its start time, actions list, and duration
//    foreach (DateTime start in startTimes)
//    {
//        // First action is always AnswerUnansweredMessages
//        var actionsList = new List<AvatarActionType> { AvatarActionType.AnswerUnansweredMessages };

//        // Choose 1–3 additional random actions from the pool (no repeats within this session)
//        int extraCount = rng.Next(1, 4);  // could randomize this range as needed
//                                          // Shuffle the pool and take the first 'extraCount' actions
//        actionPool = actionPool.OrderBy(_ => rng.Next()).ToArray();
//        actionsList.AddRange(actionPool.Take(extraCount));

//        // Assign a random session duration between MinSessionLength and MaxSessionLength
//        double sessionLenRangeTicks = (AvatarConfig.MaxSessionLength - AvatarConfig.MinSessionLength).Ticks;
//        long addTicks = sessionLenRangeTicks > 0 ? (long)(rng.NextDouble() * sessionLenRangeTicks) : 0;
//        TimeSpan sessionDuration = AvatarConfig.MinSessionLength + TimeSpan.FromTicks(addTicks);
//        if (sessionDuration > AvatarConfig.MaxSessionLength) sessionDuration = AvatarConfig.MaxSessionLength;

//        yield return new AvatarDailySession(start, sessionDuration, actionsList);
//    }
//}
//private static IEnumerable<AvatarDailySession> PlanSessions(DateTime wake, DateTime sleep, Random rng)
//{
//    int sessionCount = AvatarConfig.SessionsPerDay;
//    TimeSpan totalAwake = sleep - wake;
//    TimeSpan slotLength = TimeSpan.FromTicks(totalAwake.Ticks / sessionCount);
//    /* session start times with jitter */
//    var starts = Enumerable.Range(0, sessionCount)
//                           .Select(i => wake + slotLength * i)
//                           .Select(t => t + RandomJitter(rng))
//                           .OrderBy(t => t)          // keep chronological order
//                           .ToArray();

//    for (var i = 1; i < starts.Length; i++)
//    {
//        if (starts[i] < starts[i - 1] + AvatarConfig.MinGapBetweenSessions)
//            starts[i] = starts[i - 1] + AvatarConfig.MinGapBetweenSessions;
//    }

//    var actions = AllDailyActions.ToList();
//    Shuffle(actions, rng);
//    var baseSize = actions.Count / sessionCount;
//    var remainder = actions.Count % sessionCount;   // distribute the extras

//    int take = 0;
//    for (var i = 0; i < sessionCount; i++)
//    {
//        int size = baseSize + (i < remainder ? 1 : 0);
//        var slice = actions.GetRange(take, size);
//        take += size;
//        yield return new AvatarDailySession(starts[i], slice);
//    }
//}
//public static AvatarDailySchedule Generate(DateTime date, Random? rng = null)
//{
//    AvatarConfig.Validate();
//    rng ??= ThreadSafeRandom.Instance;

//    /* ---- wake / sleep ---- */
//    var wake = date + GetRandomTimeInRange(AvatarConfig.MinWakeTime, AvatarConfig.MaxWakeTime, rng);
//    var sleep = date + GetRandomTimeInRange(AvatarConfig.MinSleepTime, AvatarConfig.MaxSleepTime, rng);
//    sleep = EnsureSleepAfterWake(wake, sleep);

//    /* ---- sessions ---- */
//    var sessions = PlanSessions(wake, sleep, rng).ToList();

//    return new AvatarDailySchedule(date, wake, sleep, sessions);
//}
//private static TimeSpan GetRandomTimeInRange(TimeSpan min, TimeSpan max, Random rng)
//{
//    var deltaMinutes = (int)(max - min).TotalMinutes;
//    return min + TimeSpan.FromMinutes(rng.Next(deltaMinutes + 1));
//}

//private static DateTime EnsureSleepAfterWake(DateTime wake, DateTime sleep)
//{
//    if (sleep <= wake)
//    {
//        sleep = wake + TimeSpan.FromHours(12);
//    }
//    return sleep;
//}
//private static TimeSpan RandomJitter(Random rng)
//{
//    int minutes = rng.Next((int)AvatarConfig.SessionJitter.TotalMinutes + 1);
//    return TimeSpan.FromMinutes(rng.NextDouble() < 0.5 ? minutes : -minutes);
//}

//private static void Shuffle<T>(IList<T> list, Random rng)
//{
//    for (int n = list.Count; n > 1;)
//    {
//        int k = rng.Next(n--);
//        (list[n], list[k]) = (list[k], list[n]);
//    }
//}