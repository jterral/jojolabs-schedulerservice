using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JojoLabs.SchedulerService.Cron;

namespace JojoLabs.SchedulerService.Scheduling
{
    /// <summary>
    /// Task scheduler implementation.
    /// </summary>
    /// <seealso cref="JojoLabs.SchedulerService.HostedService"/>
    public class SchedulerHostedService : HostedService
    {
        /// <summary>
        /// Occurs when unobserved task throws exception..
        /// </summary>
        public event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

        /// <summary>
        /// The scheduled tasks list.
        /// </summary>
        private readonly List<SchedulerTaskWrapper> _scheduledTasks = new List<SchedulerTaskWrapper>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerHostedService"/> class.
        /// </summary>
        /// <param name="scheduledTasks">The scheduled tasks.</param>
        public SchedulerHostedService(IEnumerable<IScheduledTask> scheduledTasks)
        {
            var referenceTime = DateTime.UtcNow;

            foreach (var scheduledTask in scheduledTasks)
            {
                _scheduledTasks.Add(new SchedulerTaskWrapper
                {
                    Schedule = CrontabSchedule.Parse(scheduledTask.Schedule),
                    Task = scheduledTask,
                    NextRunTime = referenceTime
                });
            }
        }

        /// <summary>
        /// Executes the main task.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task execution.</returns>
        /// <remarks>Derived classes should override this and execute a long running method until cancellation is requested.</remarks>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await ExecuteOnceAsync(cancellationToken);

                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        /// <summary>
        /// Executes task once asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task result.</returns>
        private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            var taskFactory = new TaskFactory(TaskScheduler.Current);
            var referenceTime = DateTime.UtcNow;

            var tasksThatShouldRun = _scheduledTasks.Where(t => t.ShouldRun(referenceTime)).ToList();

            foreach (var taskThatShouldRun in tasksThatShouldRun)
            {
                taskThatShouldRun.Increment();

                await taskFactory.StartNew(
                    async () =>
                    {
                        try
                        {
                            await taskThatShouldRun.Task.ExecuteAsync(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            var args = new UnobservedTaskExceptionEventArgs(
                                ex as AggregateException ?? new AggregateException(ex));

                            UnobservedTaskException?.Invoke(this, args);

                            if (!args.Observed)
                            {
                                throw;
                            }
                        }
                    },
                    cancellationToken);
            }
        }

        /// <summary>
        /// Task wrapper.
        /// </summary>
        private class SchedulerTaskWrapper
        {
            /// <summary>
            /// Gets or sets the schedule.
            /// </summary>
            /// <value>The schedule.</value>
            public CrontabSchedule Schedule { get; set; }

            /// <summary>
            /// Gets or sets the task.
            /// </summary>
            /// <value>The task.</value>
            public IScheduledTask Task { get; set; }

            /// <summary>
            /// Gets or sets the last run time.
            /// </summary>
            /// <value>The last run time.</value>
            public DateTime LastRunTime { get; set; }

            /// <summary>
            /// Gets or sets the next run time.
            /// </summary>
            /// <value>The next run time.</value>
            public DateTime NextRunTime { get; set; }

            /// <summary>
            /// Increments this instance.
            /// </summary>
            public void Increment()
            {
                LastRunTime = NextRunTime;
                NextRunTime = Schedule.GetNextOccurrence(NextRunTime);
            }

            /// <summary>
            /// Shoulds the run.
            /// </summary>
            /// <param name="currentTime">The current time.</param>
            /// <returns><c>true</c> is task should run; otherwiese, <c>false</c>.</returns>
            public bool ShouldRun(DateTime currentTime)
            {
                return NextRunTime < currentTime && LastRunTime != NextRunTime;
            }
        }
    }
}
