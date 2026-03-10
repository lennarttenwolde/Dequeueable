# Custom Providers

Dequeueable provides default implementations for connecting to Azure Queue Storage and Azure Blob Storage. If the built-in options don't cover your use case — for example, when connecting to sovereign clouds or using advanced client configuration — you can override them with your own implementation.

## Custom Queue Provider

Implement `IQueueClientProvider` to take full control over how the `QueueClient` is constructed:
```csharp
internal class MyCustomQueueProvider : IQueueClientProvider
{
    public QueueClient GetQueue()
    {
        return new QueueClient(
            new Uri("https://myaccount.chinacloudapi.cn/myqueue"),
            new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
    }

    public QueueClient GetPoisonQueue()
    {
        return new QueueClient(
            new Uri("https://myaccount.chinacloudapi.cn/mypoisonqueue"),
            new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
    }
}
```

Register it in your DI container:
```csharp
services.AddDequeueable<MyJob>()
services.AddSingleton<IQueueClientProvider, MyCustomQueueProvider>();
```

::: tip
Registration order does not matter. Dequeueable will pick up your custom provider automatically.
:::

## Custom Blob Provider

When using the [Distributed Lock](../guide/distributed-lock), implement `IBlobClientProvider` to customize how the `BlobClient` is constructed:
```csharp
internal class MyCustomBlobClientProvider : IBlobClientProvider
{
    public BlobClient GetClient(string blobName)
    {
        return new BlobClient(
            new Uri($"https://myaccount.chinacloudapi.cn/mycontainer/{blobName}"),
            new BlobClientOptions
            {
                GeoRedundantSecondaryUri = new Uri($"https://mysecaccount.chinacloudapi.cn/mycontainer/{blobName}")
            });
    }
}
```

Register it in your DI container:
```csharp
services.AddDequeueable<MyJob>()
    .WithDistributedLock(opt => opt.Scope = "Id");
services.AddSingleton<IBlobClientProvider, MyCustomBlobClientProvider>();
```

## When to Use Custom Providers

- Connecting to **sovereign clouds** (Azure China, Azure Government)
- Using **geo-redundant** storage clients
- Applying **custom retry policies** or pipeline policies
- Integrating with **custom credential** systems not covered by `Azure.Identity`
- **Testing** — injecting mock or fake clients in integration tests