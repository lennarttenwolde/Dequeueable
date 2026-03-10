# Authentication

Dequeueable supports two authentication methods: connection string (SAS) and Azure Identity.

## Connection String

The simplest way to authenticate. Set the `ConnectionString` option either in `appsettings.json`:
```json
{
  "Dequeueable": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "QueueName": "my-queue"
  }
}
```

Or via code:
```csharp
services.AddDequeueable<MyJob>(options =>
{
    options.ConnectionString = "UseDevelopmentStorage=true";
    options.QueueName = "my-queue";
});
```

## Azure Identity

The recommended option for production. Authenticate using any `TokenCredential` provider from the `Azure.Identity` package.

Assign the following roles to the identity on the storage account:

- `Storage Queue Data Contributor`
- `Storage Blob Data Contributor` — only required when using the [Distributed Lock](./distributed-lock)
```csharp
services.AddDequeueable<MyJob>(options =>
{
    options.AuthenticationScheme = new DefaultAzureCredential();
    options.AccountName = "mystorageaccount";
    options.QueueName = "my-queue";
});
```

Any credential that inherits `Azure.Core.TokenCredential` is supported, for example `ManagedIdentityCredential`, `WorkloadIdentityCredential`, or `ClientSecretCredential`.

### URI Format

The `QueueUriFormat` option controls how the queue URI is constructed. The default is:
```
https://{accountName}.queue.core.windows.net/{queueName}
```

Use `{accountName}` and `{queueName}` as placeholders for variable substitution.

## Advanced

If neither option fits your setup, you can override the default queue client by implementing `IQueueClientProvider`, see [Custom Provides](../advanced/custom-providers.md) for details
