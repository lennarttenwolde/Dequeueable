using Dequeueable.Extentions;
using Microsoft.Extensions.Hosting;
using Dequeueable.SampleJob.Jobs;

await Host.CreateDefaultBuilder(args)
.ConfigureServices(services =>
{
    services.AddAzureQueueStorageServices<TestJob>()
    .RunAsJob(options =>
    {
        //// Uncomment for identity flow
        //options.AuthenticationScheme = new DefaultAzureCredential();
        //options.AccountName = "storageaccountname";
    });
})
.RunConsoleAsync();