# Legacy Modules

This directory contains modules that have been deprecated in favor of modern cloud-native patterns.

1. Dequeueable.AmazonSQS

Original Purpose: Provided a polling host for SQS on EC2/ECS.

**Deprecation Reason**: AWS Lambda now supports Batch Windowing and Partial Responses natively. Using Lambda is now more cost-effective and robust than running a custom polling agent.

2. Dequeueable.AzureQueueStorage (Worker Mode)

Original Purpose: Long-running polling services.

**Deprecation Reason**: Replaced by Azure Container Apps Jobs (Run-to-completion pattern). We now recommend the "Job" mode (in the main lib folder) which saves ~40% on compute costs by eliminating idle polling time.