# Poison Queue

When a message fails to process repeatedly, it is moved to a poison queue to prevent it from blocking the queue indefinitely.

## How It Works

Every time a message is dequeued, Azure Queue Storage increments its `DequeueCount`. When the `DequeueCount` reaches `MaxDequeueCount`, Dequeueable automatically moves the message to the poison queue and deletes it from the original queue.

The poison queue name is derived from the original queue name with the `PoisonQueueSuffix` appended:
```
my-queue → my-queue-poison
```

## Configuration
```json
{
  "Dequeueable": {
    "MaxDequeueCount": 5,
    "PoisonQueueSuffix": "poison"
  }
}
```

| Setting | Description | Default |
| --- | --- | --- |
| MaxDequeueCount | Number of failed attempts before moving to the poison queue. | 5 |
| PoisonQueueSuffix | Suffix appended to the queue name for the poison queue. | poison |

## Monitoring

Dequeueable does not automatically process or alert on poison queue messages — that is your responsibility. Some options:

- **KEDA** — set up a separate `ScaledJob` targeting the poison queue to trigger an alert or reprocessing job
- **Azure Monitor** — set up a metric alert on the poison queue message count
- **Dead letter handler** — deploy a separate Dequeueable job that reads from the poison queue and handles failures explicitly

## Reprocessing

If you want to reprocess poison messages, deploy a separate job that reads from the poison queue:
```csharp
services.AddDequeueable<MyPoisonHandler>(options =>
{
    options.ConnectionString = "...";
    options.QueueName = "my-queue-poison";
});
```

::: warning
Make sure your handler is idempotent before reprocessing poison messages.
:::