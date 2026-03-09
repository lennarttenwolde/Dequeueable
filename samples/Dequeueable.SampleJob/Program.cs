using Microsoft.Extensions.Hosting;
using Dequeueable.SampleJob.Jobs;
using Dequeueable.Extensions;

await Host.CreateDefaultBuilder(args)
.ConfigureServices(services =>
{
    services.AddDequeueable<TestJob>(options =>
    {
        //// Uncomment for identity flow
        //options.AuthenticationScheme = new DefaultAzureCredential();
        //options.AccountName = "storageaccountname";
    });

}).RunJobAsync();