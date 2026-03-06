using Dequeueable.AzureQueueStorage.Configurations;
using Dequeueable.AzureQueueStorage.Factories;
using Dequeueable.AzureQueueStorage.Services.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dequeueable.AzureQueueStorage.Extentions
{
    /// <summary>
    /// Extension methods for adding configuration related of the Queue services to the DI container via <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtentions
    {
        /// <summary>
        /// Adds the Azure Queue services and the function of the type specified in <typeparamref name="TFunction"/> to the
        /// specified <see cref="IServiceCollection"/>. 
        /// </summary>
        /// <typeparam name="TFunction">The type implementing the <see cref="IAzureQueueFunction"/></typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to register with.</param>
        /// <returns> <see cref="IDequeueableHostBuilder"/> </returns>
        public static IDequeueableHostBuilder AddAzureQueueStorageServices<TFunction>(this IServiceCollection services)
            where TFunction : class, IAzureQueueFunction
        {

            services.AddSingleton<IQueueMessageManager, QueueMessageManager>();
            services.AddTransient<IQueueMessageHandler, QueueMessageHandler>();
            services.AddTransient<IQueueMessageExecutor, QueueMessageExecutor>();
            services.AddTransient<IQueueClientFactory, QueueClientFactory>();
            services.AddTransient<IAzureQueueFunction, TFunction>();
            services.TryAddTransient<IQueueClientProvider, QueueClientProvider>();
            services.TryAddSingleton(TimeProvider.System);

            return new HostBuilder(services);
        }
    }
}
