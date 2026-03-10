# Distributed Lock

A distributed lock can be applied to the job to ensure that only a single instance processes the same message at any given time. It uses Azure Blob leases to guarantee the lock is distributed across all instances.

::: tip
When using the distributed lock with the Identity flow, the identity requires the **Storage Blob Data Contributor** role because the library writes and manages the lease blob.
:::

::: info
The blob files used for locking are not automatically deleted. Consider setting up [lifecycle management rules](https://learn.microsoft.com/en-us/azure/storage/blobs/lifecycle-management-overview) on the blob container if needed.
:::

## Setup

Call `.WithDistributedLock()` when registering Dequeueable:
```csharp
services.AddDequeueable<MyJob>()
    .WithDistributedLock(opt =>
    {
        opt.Scope = "Id";
    });
```

Or via `appsettings.json`:
```json
{
  "Dequeueable": {
    "DistributedLock": {
      "Scope": "Id"
    }
  }
}
```

## Scope

The scope determines which property in the message body is used as the lock identifier. Only messages with the same scope value will be locked against each other.

::: warning
Only messages containing a JSON body are supported. The scope must match a property key that exists in the message body. The match is **case sensitive**.
:::

Given a message with the following body:
```json
{
  "Id": "d89c209a-6b81-4266-a768-8cde6f613753"
}
```

With `Scope = "Id"`, only one instance processing a message with that specific `Id` value will hold the lock at any given time.

### Nested Properties

Nested properties are supported using `:` as a separator:
```json
{
  "My": {
    "Nested": {
      "Property": 500
    }
  }
}
```
```csharp
opt.Scope = "My:Nested:Property";
```

## Configuration

| Setting | Description | Default | Required |
| --- | --- | --- | --- |
| Scope | The property in the message body used as the lock identifier. | | Yes |
| LeaseDurationInSeconds | The duration of the blob lease. | 60 | No |
| MinimumPollingIntervalInSeconds | Minimum interval to check if a new lease can be acquired. | 10 | No |
| MaximumPollingIntervalInSeconds | Maximum interval to check if a new lease can be acquired. | 60 | No |
| MaxRetries | Maximum number of retries to acquire a lease. | 3 | No |
| ContainerName | The blob container name for lock files. | webjobshost | No |
| BlobUriFormat | The URI format for blob storage. Used for identity flow. | https://{accountName}.blob.core.windows.net/{containerName}/{blobName} | No |

## Advanced
If you need to customize how the `BlobClient` is constructed, implement `IBlobClientProvider`, see [Custom Provides](../advanced/custom-providers.md) for details
