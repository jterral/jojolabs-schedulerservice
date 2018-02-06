using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace JojoLabs.SchedulerService
{
    /// <summary>
    /// Hosted service implementation.
    /// </summary>
    /// <seealso cref="IHostedService"/>
    /// <remarks>Base class code kindly provided by David Fowler: <c>https://gist.github.com/davidfowl/a7dd5064d9dcf35b6eae1a7953d615e3</c>.</remarks>
    public abstract class HostedService : IHostedService
    {
        /// <summary>
        /// The executing task.
        /// </summary>
        private Task _executingTask;

        /// <summary>
        /// The cancellation token source.
        /// </summary>
        private CancellationTokenSource _cts;

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The start task.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a linked token so we can trigger cancellation outside of this token's cancellation
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Store the task we're executing
            _executingTask = ExecuteAsync(_cts.Token);

            // If the task is completed then return it, otherwise it's running
            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The stop task.</returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }

            // Signal cancellation to the executing method
            _cts.Cancel();

            // Wait until the task completes or the stop token triggers
            await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));

            // Throw if cancellation triggered
            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Executes the main task.
        /// </summary>
        /// <remarks>Derived classes should override this and execute a long running method until cancellation is requested.</remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task execution.</returns>
        protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
