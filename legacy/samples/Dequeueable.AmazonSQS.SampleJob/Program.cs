using Dequeueable.AmazonSQS.Extentions;
using Dequeueable.AmazonSQS.SampleJob.Functions;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
.ConfigureServices(services =>
{
    services
    .AddAmazonSQSServices<TestFunction>()
    .RunAsJob(options =>
    {
        options.VisibilityTimeoutInSeconds = 600;
        options.BatchSize = 4;
    });
})
.RunConsoleAsync();

