FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /app

ARG RUNTIME=linux-musl-arm64

COPY /samples/Dequeueable.SampleJob samples/consoleapp
COPY lib lib
COPY Directory.Build.props .

WORKDIR /app/samples/consoleapp
RUN dotnet restore --runtime $RUNTIME -p:TargetFramework=net10.0
RUN dotnet publish -c Release -o /app/publish \
  --no-restore \
  --runtime $RUNTIME \
  --framework net10.0 \
  --self-contained true \
  /p:PublishSingleFile=true 

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS runtime
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
ENTRYPOINT ["./Dequeueable.SampleJob"]