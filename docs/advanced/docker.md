# Docker & Container Optimization

Dequeueable is designed to run in optimized, minimal containers. This page covers how to build a production-ready image using Alpine Linux and self-contained publishing.

## Why Alpine?

The `alpine` variants of the .NET base images are significantly smaller than the default Debian-based images. Since Dequeueable is an ephemeral job runner that starts and stops frequently, a smaller image means faster pull times and lower storage costs in your container registry.

## Recommended Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /app

ARG RUNTIME=linux-musl-x64

COPY src/ src/
COPY Directory.Build.props .

RUN dotnet restore src/MyJob/MyJob.csproj --runtime $RUNTIME -p:TargetFramework=net10.0
RUN dotnet publish src/MyJob/MyJob.csproj -c Release -o /app/publish \
  --no-restore \
  --runtime $RUNTIME \
  --framework net10.0 \
  --self-contained true \
  /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS runtime
RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app

RUN apk upgrade musl
RUN apk add openssl>3.1.0
RUN apk update && apk upgrade

USER dotnetuser
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./MyJob"]
```

## Key Decisions

**`runtime-deps` instead of `runtime`**
Since the app is published as self-contained, the .NET runtime is bundled into the binary. The `runtime-deps` image only contains the native OS dependencies, making it smaller than the full `runtime` image.

**`PublishSingleFile=true`**
Bundles the entire application into a single executable, simplifying the container entrypoint and reducing the number of files copied into the final image.

**Self-contained publishing**
The app includes the .NET runtime, so the container image does not need a .NET installation. This makes the runtime image as lean as possible.

**Non-root user**
Running as a non-root user (`dotnetuser`) follows container security best practices and is required by some Kubernetes security policies.

**Security updates**
The `apk upgrade` and `apk add openssl` steps ensure the base image has the latest security patches applied at build time.

## Multi-Architecture Builds

The `RUNTIME` build argument allows you to target different architectures without maintaining separate Dockerfiles:
```bash
# ARM64 (Apple Silicon, Graviton)
docker build --build-arg RUNTIME=linux-musl-arm64 -t my-job .

# x64 (most CI runners and cloud VMs)
docker build --build-arg RUNTIME=linux-musl-x64 -t my-job .
```

In CI, always build for `linux-musl-x64` unless your cluster runs ARM nodes.

## Building for Production
```bash
docker build \
  --build-arg RUNTIME=linux-musl-x64 \
  -t myregistry.azurecr.io/my-job:1.0.0 \
  -f src/MyJob/deployment/Dockerfile \
  .

docker push myregistry.azurecr.io/my-job:1.0.0
```

## Local Development with Docker Compose

For local testing with Azurite, use Docker Compose to run both services on the same network:
```yaml
services:
  azurite:
    image: mcr.microsoft.com/azure-storage/azurite:latest
    ports:
      - "10000:10000"
      - "10001:10001"
      - "10002:10002"
    networks:
      - dequeueable

  my-job:
    build:
      context: ../..
      dockerfile: src/MyJob/deployment/Dockerfile
    environment:
      - "Dequeueable__ConnectionString=AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OWIziMFQiiLM4GCjKTXsxoF0JKFfNdnGGFKvZPpN+VjEQJpO+v3Kw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://azurite:10000/devstoreaccount1;QueueEndpoint=http://azurite:10001/devstoreaccount1;TableEndpoint=http://azurite:10002/devstoreaccount1"
      - "Dequeueable__QueueName=my-queue"
    depends_on:
      - azurite
    networks:
      - dequeueable

networks:
  dequeueable:
    driver: bridge
```

::: tip
On Apple Silicon, set `RUNTIME=linux-musl-arm64` as the default in your Dockerfile and override it to `linux-musl-x64` in CI.
:::