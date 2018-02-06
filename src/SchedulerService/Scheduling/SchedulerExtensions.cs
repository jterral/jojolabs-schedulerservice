using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JojoLabs.SchedulerService.Scheduling
{
    /// <summary>
    /// Extensions for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class SchedulerExtensions
    {
        /// <summary>
        /// Adds the scheduler in services.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <returns>The service collection with scheduler.</returns>
        public static IServiceCollection AddScheduler(this IServiceCollection services)
        {
            return services.AddSingleton<IHostedService, SchedulerHostedService>();
        }

        /// <summary>
        /// Adds the scheduler in services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="unobservedTaskExceptionHandler">The unobserved task exception handler.</param>
        /// <returns>The service collection with scheduler.</returns>
        public static IServiceCollection AddScheduler(this IServiceCollection services, EventHandler<UnobservedTaskExceptionEventArgs> unobservedTaskExceptionHandler)
        {
            return services.AddSingleton<IHostedService, SchedulerHostedService>(serviceProvider =>
            {
                var instance = new SchedulerHostedService(serviceProvider.GetServices<IScheduledTask>());
                instance.UnobservedTaskException += unobservedTaskExceptionHandler;
                return instance;
            });
        }
    }
}
