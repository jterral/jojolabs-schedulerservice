using System;

namespace JojoLabs.SchedulerService.Cron
{
    /// <summary>
    /// Crontab field types.
    /// </summary>
    [Serializable]
    public enum CrontabFieldKind
    {
        /// <summary>
        /// Minute type.
        /// </summary>
        Minute,

        /// <summary>
        /// Hour type.
        /// </summary>
        Hour,

        /// <summary>
        /// Day type.
        /// </summary>
        Day,

        /// <summary>
        /// Month type.
        /// </summary>
        Month,

        /// <summary>
        /// Day of week type.
        /// </summary>
        DayOfWeek
    }
}
