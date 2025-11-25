# Dequeueable
A framework to simplify event driven applications in containerized host environments. It provides a reliable, easy-to-configure host for run-to-completion (ephemeral) jobs, guaranteeing efficient run-to-completion execution.

## Libraries
- [Azure Queue Storage](lib/Dequeueable.AzureQueueStorage/README.md)
Framework that handles the messages on the Azure Queue. A function will be invoked when new messages are detected on the queue. Dequeueing, exception handling and distributed singleton are handled for you.

