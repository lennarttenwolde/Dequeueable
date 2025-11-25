using Dequeueable.AmazonSQS.Configurations;
using Dequeueable.AmazonSQS.Factories;
using Dequeueable.AmazonSQS.Services.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dequeueable.AmazonSQS.Extentions
{
    /// <summary>
    /// Extension methods for adding configuration related of the Queue services to the DI container via <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtentions
    {
        /// <summary>
        /// Adds the Amazon Simple Queue Service and the function of the type specified in <typeparamref name="TFunction"/> to the
        /// specified <see cref="IServiceCollection"/>. 
        /// </summary>
        /// <typeparam name="TFunction">The type implementing the <see cref="IAmazonSQSFunction"/></typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to register with.</param>
        /// <returns> <see cref="IDequeueableHostBuilder"/> </returns>
        public static IDequeueableHostBuilder AddAmazonSQSServices<TFunction>(this IServiceCollection services)
           where TFunction : class, IAmazonSQSFunction
        {
            services.AddSingleton<IQueueMessageManager, QueueMessageManager>();
            services.TryAddSingleton<IAmazonSQSClientFactory, AmazonSQSClientFactory>();
            services.AddTransient<IQueueMessageHandler, QueueMessageHandler>();
            services.AddTransient<IAmazonSQSFunction, TFunction>();

            services.AddTransient<IQueueMessageExecutor, QueueMessageExecutor>();
            services.AddTransient<IAmazonSQSFunction, TFunction>();
            services.TryAddSingleton(TimeProvider.System);

            return new HostBuilder(services);
        }
    }
}
