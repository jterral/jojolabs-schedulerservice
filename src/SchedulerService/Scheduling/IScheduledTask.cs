using System.Threading;
using System.Threading.Tasks;

namespace JojoLabs.SchedulerService.Scheduling
{
    /// <summary>
    /// Scheduled task representation.
    /// </summary>
    public interface IScheduledTask
    {
        /// <summary>
        /// Gets the schedule.
        /// </summary>
        /// <value>The schedule.</value>
        string Schedule { get; }

        /// <summary>
        /// Executes the task asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task executed.</returns>
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
