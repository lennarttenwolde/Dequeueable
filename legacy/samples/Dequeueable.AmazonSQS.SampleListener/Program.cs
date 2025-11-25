using Dequeueable.AmazonSQS.Extentions;
using Dequeueable.AmazonSQS.SampleListener.Functions;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
.ConfigureServices(services =>
{
    services
    .AddAmazonSQSServices<TestFunction>()
    .RunAsListener(options =>
    {
        options.VisibilityTimeoutInSeconds = 300;
        options.BatchSize = 4;
    });
})
.RunConsoleAsync();