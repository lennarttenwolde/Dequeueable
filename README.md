# Dequeueable
A framework to simplify event driven applications in containerized host environments. The aim of this project is to handle messages on a queue more efficiently.

## Libraries
- [Azure Queue Storage](lib/Dequeueable.AzureQueueStorage/README.md)
Framework that handles the messages on the Azure Queue. A function will be invoked when new messages are detected on the queue. Dequeueing, exception handling and distributed singleton are handled for you.
