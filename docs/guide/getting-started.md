# Getting Started

Dequeueable is a cloud-native ephemeral job runner for Azure Queue Storage. It is designed to be triggered by external queue scalers (e.g., KEDA), process a **single message**, and immediately shut down upon completion. If no message is found, the host shuts down without executing.

## Prerequisites

- .NET 8, 9, or 10
- An Azure Storage Account or local [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) emulator

## Installation
```bash
dotnet add package Dequeueable
```

## Quick Start

### 1. Implement IQueueJob
```csharp
public class MyJob : IQueueJob
{
    public async Task ExecuteAsync(Message message, CancellationToken cancellationToken)
    {
        var body = message.Body.ToString();
        // process your message here
    }
}
```

### 2. Register and run
```csharp
await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddDequeueable<MyJob>(options =>
        {
            options.ConnectionString = "UseDevelopmentStorage=true";
            options.QueueName = "my-queue";
        });
    })
    .RunJobAsync();
```

### 3. Configure via appsettings.json

Alternatively, configure the host via `appsettings.json`:
```json
{
  "Dequeueable": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "QueueName": "my-queue"
  }
}
```


## Before & After

::: code-group
```csharp [Before (BackgroundService)]
public class QueueWorker : BackgroundService
{
    private readonly QueueClient _queueClient;
    private readonly ILogger<QueueWorker> _logger;

    public QueueWorker(QueueClient queueClient, ILogger<QueueWorker> logger)
    {
        _queueClient = queueClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var response = await _queueClient.ReceiveMessageAsync(cancellationToken: stoppingToken);
            var message = response.Value;

            if (message is null)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                continue;
            }

            try
            {
                await DoWorkAsync(message.Body.ToString(), stoppingToken);
                await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message {MessageId}", message.MessageId);
                // visibility timeout, poison queue, renewal... all your problem
            }
        }
    }
}
```
```csharp [After (Dequeueable)]
public class MyJob : IQueueJob
{
    public async Task ExecuteAsync(Message message, CancellationToken cancellationToken)
    {
        var body = message.Body.ToString();
        await DoWorkAsync(body, cancellationToken);
    }
}

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddDequeueable<MyJob>(options =>
        {
            options.AccountName = "mystorageaccount";
            options.AuthenticationScheme = new DefaultAzureCredential();
            options.QueueName = "my-queue";
        });
    })
    .RunJobAsync();
```

:::

## Configuration

| Setting | Description | Default | Required |
| --- | --- | --- | --- |
| QueueName | The queue used to retrieve messages. | | Yes |
| ConnectionString | The connection string used to authenticate. | | Yes, when not using Identity |
| PoisonQueueSuffix | Suffix appended to the queue name for the poison queue. | poison | No |
| AccountName | The storage account name, used for identity flow. | | Only when using Identity |
| QueueUriFormat | The URI format to the queue storage. Use `{accountName}` and `{queueName}` for substitution. | https://{accountName}.queue.core.windows.net/{queueName} | No |
| AuthenticationScheme | Token credential used to authenticate via Azure AD. | | Yes, if using Identity |
| MaxDequeueCount | Max dequeue count before moving to the poison queue. | 5 | No |
| VisibilityTimeoutInSeconds | The timeout after which the message is visible again. | 300 | No |

## Next Steps

- [Authentication](./authentication) — connect using a connection string or Azure Identity
- [Distributed Lock](./distributed-lock) — ensure only one instance processes the same message