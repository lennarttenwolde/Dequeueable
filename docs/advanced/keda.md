# KEDA Integration

[KEDA](https://keda.sh) (Kubernetes Event Driven Autoscaler) is the recommended way to trigger Dequeueable in a Kubernetes cluster. KEDA monitors your Azure Queue Storage and spins up a new job instance for each message, shutting it down when the queue is empty.

## How It Works

KEDA uses a `ScaledJob` resource to manage job scaling. Unlike a `ScaledObject` (which scales a `Deployment`), a `ScaledJob` creates a new Kubernetes `Job` for each trigger — which maps perfectly to Dequeueable's ephemeral model.
```
Queue has 5 messages → KEDA creates 5 Job instances → each processes 1 message → all shut down
```

## Setup

Follow the official KEDA documentation to get started:

- [Installing KEDA](https://keda.sh/docs/latest/deploy/)
- [Azure Queue Storage Scaler](https://keda.sh/docs/latest/scalers/azure-storage-queue/)
- [ScaledJob](https://keda.sh/docs/latest/concepts/scaling-jobs/)

## Recommendations

When configuring the Azure Queue Storage scaler, set `queueLength` to `1` to ensure KEDA creates exactly one job instance per message, matching Dequeueable's single-message model.

Since each job instance processes exactly one message, you can set tight and predictable resource limits on your container. This is one of the key advantages over a shared host model — resource usage per message is consistent and enforceable at the container level.

For production, use [Azure Workload Identity](https://keda.sh/docs/latest/authentication-providers/azure-workload-identity/) to authenticate KEDA with your storage account without storing credentials in Kubernetes secrets.