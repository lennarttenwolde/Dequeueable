# Timeouts

Dequeueable automatically manages two types of timeouts to ensure reliable message processing.

## Visibility Timeout

When a message is dequeued, it becomes invisible to other consumers for the duration of the `VisibilityTimeoutInSeconds` option. Dequeueable automatically renews this timeout when half of the visibility window has elapsed, keeping the message invisible while it is being processed.

::: warning
When the renewal fails, the host can no longer guarantee the message is processed only once. The `CancellationToken` will be set to cancelled — it is up to you to handle this scenario in your job implementation.
:::

Configure the visibility timeout via options:
```csharp
services.AddDequeueable<MyJob>(options =>
{
    options.VisibilityTimeoutInSeconds = 600;
});
```

Or via `appsettings.json`:
```json
{
  "Dequeueable": {
    "VisibilityTimeoutInSeconds": 600
  }
}
```

::: tip
Choose this value wisely. A very low timeout causes frequent renewal calls. A very high timeout delays reprocessing if the job fails without releasing the message.
:::

## Lease Timeout

When using the [Distributed Lock](../guide/distributed-lock.md), the blob lease is automatically renewed when half of the `LeaseDurationInSeconds` has elapsed.

::: warning
When the lease renewal fails, the host can no longer guarantee the distributed lock. The `CancellationToken` will be set to cancelled — it is up to you to handle this scenario in your job implementation.
:::

### Handling Cancellation

Both timeouts signal failure via the `CancellationToken` passed to `ExecuteAsync`. A robust implementation should handle this gracefully:
```csharp
public class MyJob : IQueueJob
{
    public async Task ExecuteAsync(Message message, CancellationToken cancellationToken)
    {
        // Pass the token to all async operations
        await DoWorkAsync(cancellationToken);

        // Or check manually
        cancellationToken.ThrowIfCancellationRequested();
    }
}
```