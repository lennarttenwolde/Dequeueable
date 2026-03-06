using Dequeueable.AzureQueueStorage.Extentions;
using Dequeueable.AzureQueueStorage.SampleJob.Functions;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
.ConfigureServices(services =>
{
    services.AddAzureQueueStorageServices<TestFunction>()
    .RunAsJob(options =>
    {
        //// Uncomment for identity flow
        //options.AuthenticationScheme = new DefaultAzureCredential();
        //options.AccountName = "storageaccountname";
    });
})
.RunConsoleAsync();