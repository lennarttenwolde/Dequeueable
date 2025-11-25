FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine as build
WORKDIR /app

COPY /samples/Dequeueable.AzureQueueStorage.SampleListener samples/consoleapp
COPY /lib lib
COPY Directory.Build.props .

WORKDIR samples/consoleapp
RUN dotnet restore --runtime linux-musl-x64
RUN dotnet publish -c Release -o /app/publish \
  --no-restore \
  --runtime linux-musl-x64 \
  --self-contained true \
  /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS runtime
RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app

# upgrade to remove potential vulnerability
RUN apk upgrade musl
RUN apk add openssl>3.1.0
RUN apk update && apk upgrade
USER dotnetuser

WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["./Dequeueable.AzureQueueStorage.SampleListener"]