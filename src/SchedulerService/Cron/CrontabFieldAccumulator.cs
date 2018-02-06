namespace JojoLabs.SchedulerService.Cron
{
    /// <summary>
    /// Accumulator.
    /// </summary>
    /// <param name="start">The start.</param>
    /// <param name="end">The end.</param>
    /// <param name="interval">The interval.</param>
    public delegate void CrontabFieldAccumulator(int start, int end, int interval);
}
