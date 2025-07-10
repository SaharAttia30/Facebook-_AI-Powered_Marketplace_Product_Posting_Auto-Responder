using System;

namespace ChatBlaster.Controllers.Avatar
{
    public static class AvatarConfig
    {
        /* --- Daily rhythm --- */
        public static TimeSpan MinWakeTime { get; set; } = TimeSpan.FromHours(6);          // 06:00
        public static TimeSpan MaxWakeTime { get; set; } = TimeSpan.FromHours(8.5);        // 08:30
        public static TimeSpan MinSleepTime { get; set; } = TimeSpan.FromHours(22);         // 22:00
        public static TimeSpan MaxSleepTime { get; set; } = TimeSpan.FromHours(23.5);       // 23:30
        public static int SessionsPerDay { get; set; } = 3;                           // morning / noon / evening
        public static TimeSpan SessionJitter { get; set; } = TimeSpan.FromMinutes(15);    // ±15 min
        public static TimeSpan MinGapBetweenSessions { get; set; } = TimeSpan.FromMinutes(30);
        public static TimeSpan MinSessionLength { get; set; } = TimeSpan.FromMinutes(3.15); // ~3:09 min
        public static TimeSpan MaxSessionLength { get; set; } = TimeSpan.FromMinutes(6.5);
        /* --- Action pacing --- */
        public static TimeSpan ActionDelayMin { get; set; } = TimeSpan.FromSeconds(3);
        public static TimeSpan ActionDelayMax { get; set; } = TimeSpan.FromSeconds(12);
        /* --- Global resource limits --- */
        public static int MaxConcurrentBrowsers { get; set; } = 4;
        public static int MaxConcurrentMonitors { get; set; } = 4;
        /* --- Daily quotas --- */
        public static int MaxMessages { get; set; } = 50;
        public static int MaxFreindRequests { get; set; } = 3;
        public static int MaxPostsToMarketPlace { get; set; } = 2;
        public static int MaxComments { get; set; } = 5;
        public static int MaxReactions { get; set; } = 10;
        public static int MaxVideos { get; set; } = 10;
        /* Utility */
        // ──────────────────────────────────────────────────────────────────────────────
        public static TimeSpan RandomBetween(Random rng, TimeSpan min, TimeSpan max)
        {
            var range = max - min;                              // TimeSpan range
            long randTicks = (long)(range.Ticks * rng.NextDouble());
            return min + TimeSpan.FromTicks(randTicks);
        }
        // ──────────────────────────────────────────────────────────────────────────────
        internal static void Validate()
        {

            if (MinWakeTime >= MaxWakeTime)
                throw new InvalidOperationException("MinWakeTime must be earlier than MaxWakeTime.");

            if (MinSleepTime >= MaxSleepTime)
                throw new InvalidOperationException("MinSleepTime must be earlier than MaxSleepTime.");

            if (MinGapBetweenSessions < TimeSpan.Zero)
                throw new InvalidOperationException("MinGapBetweenSessions cannot be negative.");

            if (MinSessionLength > MaxSessionLength)
                throw new InvalidOperationException("MinSessionLength must be <= MaxSessionLength.");
        }
    }
}
