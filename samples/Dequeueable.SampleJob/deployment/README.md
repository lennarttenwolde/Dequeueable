# Sample job Deployment examples

## Docker

### Build

```
docker build -t sample-job -f samples/Dequeueable.SampleJob/deployment/Dockerfile .
```


## Compose

```
# Start azurite
docker compose -f samples/Dequeueable.SampleJob/deployment/docker-compose.yml up azurite -d

# Send a message first (needs az cli or Storage Explorer)
az storage message put --queue-name testqueue \
  --content "Hello from Dequeueable!" \
  --connection-string "UseDevelopmentStorage=true"

# Run the job
docker compose -f samples/Dequeueable.SampleJob/deployment/docker-compose.yml up sample-job

```