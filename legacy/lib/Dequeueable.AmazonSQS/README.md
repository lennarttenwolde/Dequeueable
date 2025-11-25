# Dequeueable.AmazonSQS

> [!WARNING]
> **Deprecation Notice:** Support for AWS SQS (`Dequeueable.AmazonSQS`) has been discontinued.
>
> **Migration Advice:** For modern AWS serverless applications, we recommend using native [AWS Lambda SQS Triggers](https://docs.aws.amazon.com/lambda/latest/dg/with-sqs.html). They now support batch windowing and partial batch responses, which solves the concurrency issues this library originally addressed.

This project is an **opinionated** framework build for the Amazon (AWS) Simple Queue Service (SQS):
- Build as a Console App
- Being able to use optimized alpine/dotnet images
- Have the freedom to use Keda or any other scalers to retrieve queue messages

This framework can run as a **listener** or **job**:
- **Listener:**
Highly scalable queue listener that will be invoked automatically when new messages are detected on the SQS.
- **Job:**
Framework that depends on external queue triggers, eg; KEDA. When the host is started, new messages on the SQS are being retrieved and executed. After execution the host will shutdown automatically. 

## Getting started

Scaffold a new project, you can either use a console or web app.
1. Add a class that implements the `IAmazonSQSFunction`.
2. Add `.AddAmazonSQSServices<TFunction>()` in the DI container.
3. Specify how you want to run your service:
   - Add `.RunAsJob()` in the DI container of your app to run the host as a job.
   - Add `.RunAsListener()` in the DI container of your app to run the app as a back ground listener.


*function.cs*:
```csharp
internal class TestFunction : IAmazonSQSFunction
    {
        public Task ExecuteAsync(Message message, CancellationToken cancellationToken)
        {
            // Put your magic here!
        }
    }
```

*program.cs*:
```csharp
await Host.CreateDefaultBuilder(args)
.ConfigureServices(services =>
{
    services
    .AddAmazonSQSServices<TestFunction>()
    .RunAsJob(options =>
    {
        options.VisibilityTimeoutInSeconds = 300;
        options.BatchSize = 4;
    });
})
.RunConsoleAsync();

```

### Configurations
You can configure the host via the `appsettings.json` or the `IOptions` pattern during registration.

**Appsettings**

Use the `Dequeueable` section to configure the settings:

```json
"Dequeueable": {
     "QueueUrl": "https://sqs.<region>.amazonaws.com/<id>/<queuename>"
  }
```

**Options**

```csharp
await Host.CreateDefaultBuilder(args)
.ConfigureServices(services =>
{
    services
    .AddAmazonSQSServices<TestFunction>()
    // .RunAsListener(options =>
    .RunAsJob(options =>
    {
        options.VisibilityTimeoutInSeconds = 300;
        options.BatchSize = 4;
    });
})
.RunConsoleAsync();

```

###  Settings
The library uses the `IOptions` pattern to inject the configured app settings.

#### Job options
Setting | Description | Default | Required
--- | --- | --- | --- |
QueueUrl | The URL of the Amazon SQS queue from which messages are received. | | Yes |
BatchSize | The maximum number of messages processed in parallel. Valid values: 1 to 10. | 4 | No |
MaxDequeueCount | Max dequeue count before moving to the poison queue.  | 5 | No |
VisibilityTimeoutInSeconds | The timeout after the queue message is visible again for other services. Valid values: 30 to 43200 (12 hours) seconds. | 300 | No |
AttributeNames | A list of attributes that need to be returned along with each message | [] | No |

#### Listener options
Setting | Description | Default | Required
--- | --- | --- | --- |
QueueUrl | The URL of the Amazon SQS queue from which messages are received. | | Yes |
BatchSize | The maximum number of messages processed in parallel. Valid values: 1 to 10. | 4 | No |
NewBatchThreshold | The threshold at which a new batch of messages will be fetched. | BatchSize / 2 | No |
MaxDequeueCount | Max dequeue count before moving to the poison queue.  | 5 | No |
VisibilityTimeoutInSeconds | The timeout after the queue message is visible again for other services. Valid values: 30 to 43200 (12 hours) seconds. | 300 | No |
MinimumPollingIntervalInMilliseconds | The minimum polling interval to check the queue for new messages. | 5 | No |
VisibilityTimeoutInSeconds | The maximum polling interval to check the queue for new messages.  | 10000 | No |
DeltaBackOff | The delta used to randomize the polling interval. | MinimumPollingIntervalInMilliseconds | No |
AttributeNames | A list of attributes that need to be returned along with each message | [] | No |

## Authentication
The queue client is constructed  with the credentials loaded from the application's default configuration, using the `FallbackCredentialsFactory.GetCredentials()`.

### Custom AmazonSQSClientFactory
There are plenty ways to construct the AmazonSQSClient, and not all are by default supported. You can override the default implementations to retrieve the queue client by implementing the `IAmazonSQSClientFactory`. You still should register your custom factory in your DI container, specific registration order is not needed:

```csharp
internal class MyCustomQueueFactory : IAmazonSQSClientFactory
    {
        private AmazonSQSClient? _client;
        public AmazonSQSClient Create() => _client ??= new AmazonSQSClient(Amazon.RegionEndpoint.CNNorth1);
    }
```

## Singleton
The application can run as distributed singleton. The Amazon SQS message group ID is used to processed the messages one by one, in a strict order relative to the message group. Therefore a fifo queue is required.
Both the Job as the Listener services can run as singleton by defining this during registration:

```csharp
await Host.CreateDefaultBuilder(args)
.ConfigureServices(services =>
{
    services
    .AddAmazonSQSServices<TestFunction>()
    .RunAsJob(options =>
    {
        // some options
    })
    .AsSingleton();
})
.RunConsoleAsync();
```

## Timeouts

### Visibility Timeout Queue Message
The visibility timeout of the queue messages is automatically updated. It will be updated when the half `VisibilityTimeoutInSeconds` option is reached. Choose this setting wisely to prevent talkative hosts. When renewing the timeout fails, the host cannot guarantee if the message is executed only once. Therefore the CancelationToken is set to Cancelled. It is up to you how to handle this scenario!

## Sample
- [Job Console app](https://github.com/lenndewolten/Dequeueable/blob/main/samples/Dequeueable.AmazonSQS.SampleJob/README.md)
- [Listener Console app](https://github.com/lenndewolten/Dequeueable/blob/main/samples/Dequeueable.AmazonSQS.SampleListener/README.md)