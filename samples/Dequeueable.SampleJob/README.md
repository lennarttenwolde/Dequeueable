# Dequeueable Sample Job

This sample demonstrates how to run a basic ephemeral job using Dequeueable with a local Azure Queue Storage emulator (Azurite).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started)

## Getting started

### 1. Start Azurite

Run Azurite locally using Docker:
```bash
docker run -d \
  --rm \
  --name azurite \
  -p 10000:10000 \
  -p 10001:10001 \
  -p 10002:10002 \
  mcr.microsoft.com/azure-storage/azurite:latest
```

### 2. Create the queue

Use the Azure Storage Explorer or the Azure CLI to create a queue named `testqueue`:
```bash
az storage queue create --name testqueue --connection-string "UseDevelopmentStorage=true"
```

### 3. Send a message
```bash
az storage message put \
  --queue-name testqueue \
  --content "Hello from Dequeueable!" \
  --connection-string "UseDevelopmentStorage=true"
```

### 4. Configure the sample

The sample is pre-configured to use Azurite via `appsettings.json`:
```json
{
  "Dequeueable": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "QueueName": "testqueue"
  }
}
```

### 5. Run the sample
```bash
cd samples/Dequeueable.SampleJob
dotnet run --framework net10.0
```

The job will retrieve the message from the queue, execute it, and shut down.

## Stopping Azurite
```bash
docker stop azurite && docker rm azurite
```