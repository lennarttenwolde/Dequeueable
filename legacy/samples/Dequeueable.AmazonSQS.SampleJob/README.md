# Amazon Simple Queue Service Sample job


> [!WARNING]
> **Deprecation Notice:** Support for AWS SQS (`Dequeueable.AmazonSQS`) has been discontinued.
>
> **Migration Advice:** For modern AWS serverless applications, we recommend using native [AWS Lambda SQS Triggers](https://docs.aws.amazon.com/lambda/latest/dg/with-sqs.html). They now support batch windowing and partial batch responses, which solves the concurrency issues this library originally addressed.



## Docker

### Build

```
docker build -t <yourtagname> -f samples/Dequeueable.AmazonSQS.SampleJob/deployment/Dockerfile .
```

Image stats:

```
docker images -f reference=lenndewolten/dequeueable:aws-sqs-samplejob-v1

> REPOSITORY                 TAG                    IMAGE ID       CREATED              SIZE
> lenndewolten/dequeueable   aws-sqs-samplejob-v1   7cfdf41b4bbb   About a minute ago   84.2MB
```

## Kubernetes

### Deployment

This sample is using [KEDA](https://keda.sh/) to automatically schedule the jobs based on the messages on the queue

```
kubectl apply -f scaledjob.yaml
```

#### **Magic!**

After a message is added to the queue:

```
kubectl get pods

> NAME                                          READY   STATUS    RESTARTS   AGE
> queuejob-consumer-m8zpl-jpqws                 1/1     Running   0          7s
```

```
kubectl get pods

> NAME                                           READY   STATUS      RESTARTS       AGE
> queuejob-consumer-m8zpl-jpqws                  0/1     Completed   0              2m51s
```

Logs when when four messages are handled:

```
kubectl logs pods/queuejob-consumer-m8zpl-jpqws

> info: Microsoft.Hosting.Lifetime[0]
>       Application started. Press Ctrl+C to shut down.
> info: Microsoft.Hosting.Lifetime[0]
>       Hosting environment: Production
> info: Microsoft.Hosting.Lifetime[0]
>       Content root path: /app
> info: Dequeueable.AmazonSQS.SampleJob.Functions.TestFunction[0]
>       Function called with MessageId 7c28f4fe-28d3-4372-84d8-2a116c13520a and content fdfdfdfdfdf
> info: Dequeueable.AmazonSQS.Services.Queues.QueueMessageHandler[0]
>       Executed message with id '7c28f4fe-28d3-4372-84d8-2a116c13520a' (Succeeded)
> info: Microsoft.Hosting.Lifetime[0]
>       Application is shutting down...
```
