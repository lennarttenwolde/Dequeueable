# FAQ

## Why does Dequeueable only process one message per execution?

This is by design. Dequeueable is built around the ephemeral job runner pattern — each instance starts, processes a single message, and shuts down. Parallelism is achieved horizontally by running multiple instances simultaneously, controlled by an external scaler like KEDA.

This approach gives you fine-grained control over scaling: instead of one long-running process managing concurrency internally, you let your infrastructure decide how many instances to run.

## Why use this over Azure Functions?

Azure Functions is a great general-purpose serverless platform, but it comes with trade-offs:

Azure Functions is a great general-purpose serverless platform, but it comes with trade-offs:

- **Runtime lock-in** — you are tied to the Azure Functions host, its versioning, and its limitations.
- **Cold starts** — Functions can have cold start latency, especially on the Consumption plan.
- **Limited container control** — customizing the underlying container (e.g., using optimized alpine images) is not straightforward.
- **Scaling model** — the Functions host manages scaling internally, which can be harder to tune for specific workloads.
- **Resource contention** — when multiple messages are processed concurrently within a single host, they compete for the same CPU and memory. With Dequeueable, each instance processes one message and gets its own dedicated resources, making resource limits predictable and enforceable at the infra level.
- **Observability** — with multiple messages running in one host process, correlating logs and traces to a specific message can be difficult. Each Dequeueable instance maps to exactly one message, making distributed tracing straightforward.
- **Startup configuration** — the Functions host imposes opinions on logging, configuration, and middleware that can be hard to override. Dequeueable is a plain `IHostBuilder` — you configure it exactly like any other .NET application.

Dequeueable gives you a plain console app that you fully own. You can use any base image, configure your container exactly as needed, and let KEDA or any other scaler drive execution.

## When should I use this package?

Good scenarios for Dequeueable:

- **Background job processing** — processing orders, sending emails, resizing images, generating reports.
- **Event-driven workloads** — reacting to queue messages triggered by upstream services.
- **KEDA-scaled workloads** — you want Kubernetes to scale your job runners based on queue depth.
- **Workloads requiring distributed locking** — ensuring only one instance processes a specific entity at a time (e.g., per customer or per order).
- **Cost-sensitive workloads** — running only when there is work to do, with no idle compute.

## When should I NOT use this package?

Dequeueable is not the right fit for:

- **Long-running background services** — if you need a persistent process that continuously polls, use a `BackgroundService` instead.
- **High-throughput batch processing** — if you need to process thousands of messages per second with minimal overhead, a dedicated message broker solution may be more appropriate.
- **Non-Azure queue storage** — Dequeueable is purpose-built for Azure Queue Storage. It does not support Service Bus, RabbitMQ, or other brokers.

## What happens if no message is found on the queue?

The host starts, checks the queue, finds no message, logs a debug message, and shuts down cleanly. No processing occurs and no error is thrown. This is expected behavior when KEDA triggers an instance just as the queue becomes empty.

## What happens if my job throws an exception?

If your job throws an unhandled exception, the message becomes visible again on the queue after the `VisibilityTimeoutInSeconds` has elapsed. Once the `DequeueCount` reaches `MaxDequeueCount`, the message is automatically moved to the poison queue.

## What is the poison queue?

The poison queue is a separate queue where messages are moved after exceeding the `MaxDequeueCount`. The queue name is derived from the original queue name with the `PoisonQueueSuffix` appended, e.g. `my-queue-poison`. You are responsible for monitoring and handling messages in the poison queue.

## Is the message guaranteed to be processed exactly once?

Not strictly. Dequeueable makes a best effort to process each message once by using visibility timeouts and distributed locking. However, in failure scenarios — such as a renewal failure or a host crash — a message may be processed more than once. Design your job implementation to be idempotent where possible.

## Can I run this without KEDA?

Yes. KEDA is the recommended scaler but not a requirement. You can trigger the job from any system that can run a container — a Kubernetes CronJob, a CI pipeline, a shell script, or any other scheduler. Dequeueable does not care how it is invoked.

## Can I use this with Azure Managed Identity?

Yes, and it is the recommended approach for production. See the [Authentication](./authentication) guide for setup instructions.

## Does this work with .NET 8 and .NET 9?

Yes. Dequeueable targets net8.0, net9.0, and net10.0.