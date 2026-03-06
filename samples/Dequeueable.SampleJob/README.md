# Azure Queue Storage Sample job

## Docker

### Build

```
docker build -t <yourtagname> -f samples/Dequeueable.AzureQueueStorage.SampleJob/deployment/Dockerfile .
```

Image stats:

```
docker images -f reference=lenndewolten/dequeueable:azure-queue-storage-samplejob-v2

> REPOSITORY                 TAG                                IMAGE ID       CREATED         SIZE
> lenndewolten/dequeueable   azure-queue-storage-samplejob-v2   2709a261957e   6 minutes ago   90.8MB
```

## Kubernetes

### Deployment

This sample is using [KEDA](https://keda.sh/) to automatically schedule the jobs based on the messages on the queue

```
kubectl apply -f azurite.yaml
kubectl apply -f scaledjob.yaml
```

#### **Connect to azurite**

Get the public IP address of one of your nodes that is running a Hello World pod. How you get this address depends on how you set up your cluster. For example, if you are using Minikube or Docker Desktop, you can see the node address by running kubectl cluster-info.

```
kubectl cluster-info

> Kubernetes control plane is running at https://kubernetes.docker.internal:6443
> CoreDNS is running at https://kubernetes.docker.internal:6443/api/v1/namespaces/kube-system/services/kube-dns:dns/proxy
```

#### **Get Azurite NodePort IP**

```
 kubectl get svc

> NAME                      TYPE        CLUSTER-IP      EXTERNAL-IP   PORT> (S)                                           AGE
> kubernetes                ClusterIP   10.96.0.1       <none>        443/> TCP                                           209d
> storage-azurite-service   NodePort    10.106.222.95   <none>        10000:32444/TCP,10001:30623/TCP,10002:32460/TCP     26s
```

#### **Construct connection string**

With the output above, the connection string would be:

```
DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://kubernetes.docker.internal:32444/devstoreaccount1;QueueEndpoint=http://kubernetes.docker.internal:30623/devstoreaccount1;TableEndpoint=http://kubernetes.docker.internal:32460/devstoreaccount1;
```

#### **Magic!**

After a message is added to the queue:

```
kubectl get pods

> NAME                                          READY   STATUS    RESTARTS   AGE
> queuejob-consumer-m8zpl-jpqws                 1/1     Running   0          7s
> storage-azurite-deployment-6f5cffcf95-jd4zv   1/1     Running   0          3m44s
```

```
kubectl get pods

> NAME                                           READY   STATUS      RESTARTS       AGE
> queuejob-consumer-m8zpl-jpqws                  0/1     Completed   0              2m51s
> storage-azurite-deployment-6f5cffcf95-jd4zv    1/1     Running     56 (64m ago)   197d
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
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 0
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 0
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 0
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 0
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 1
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 1
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 1
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 1
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 2
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 2
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 2
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 2
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 3
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 3
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 3
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 3
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 4
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 4
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 4
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 4
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 5
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 5
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 5
> info: Dequeueable.AzureQueueStorage.SampleJob.Functions.TestFunction[0]
>       Executing job loop 5
> info: Dequeueable.AzureQueueStorage.Services.Queues.QueueMessageHandler[0]
>       Executed message with id 'c134e005-7f93-4415-b979-5e388771510b' (Succeeded)
> info: Dequeueable.AzureQueueStorage.Services.Queues.QueueMessageHandler[0]
>       Executed message with id 'fe0169f5-9a13-425e-bb42-fc4946774ab6' (Succeeded)
> info: Dequeueable.AzureQueueStorage.Services.Queues.QueueMessageHandler[0]
>       Executed message with id 'e370feff-855e-4f7c-8e0e-8f0c92170013' (Succeeded)
> info: Dequeueable.AzureQueueStorage.Services.Queues.QueueMessageHandler[0]
>       Executed message with id '45f69fe7-04b3-44ee-a110-816c21b60bce' (Succeeded)
> info: Dequeueable.AzureQueueStorage.Services.Queues.QueueMessageHandler[0]
>       Executed message with id 'c134e005-7f93-4415-b979-5e388771510b' (Succeeded)
> info: Dequeueable.AzureQueueStorage.Services.Queues.QueueMessageHandler[0]
>       Executed message with id 'e370feff-855e-4f7c-8e0e-8f0c92170013' (Succeeded)
> info: Dequeueable.AzureQueueStorage.Services.Queues.QueueMessageHandler[0]
>       Executed message with id '45f69fe7-04b3-44ee-a110-816c21b60bce' (Succeeded)
> info: Dequeueable.AzureQueueStorage.Services.Queues.QueueMessageHandler[0]
>       Executed message with id 'fe0169f5-9a13-425e-bb42-fc4946774ab6' (Succeeded)
> info: Microsoft.Hosting.Lifetime[0]
>       Application is shutting down...
```
