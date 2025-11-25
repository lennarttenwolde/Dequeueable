# Azure Queue Storage Sample listener


> [!WARNING]
> **Deprecation Notice:**  The continuous polling/worker functionality (`RunAsListener()`) in `Dequeueable.AzureQueueStorage` has been **removed**.
> * **Reason:** Maintaining a custom continuous poller is redundant. Modern solutions like **KEDA (Kubernetes Event-driven Autoscaling)** or specialized frameworks like **MassTransit** offer superior, more reliable scaling and feature sets for long-running workers.
> * **Migration Advice:** For continuous, long-running processing, use **MassTransit** (for feature-rich workers) or the native **Azure Functions Queue Trigger** (on a Premium or Flex Consumption plan).


## Docker

### Build

```
docker build -t <yourtagname> -f samples/Dequeueable.AzureQueueStorage.SampleListener/deployment/Dockerfile .
```

Image stats:

```
docker images -f reference=lenndewolten/dequeueable:azure-queue-storage-samplelistener-v2

> REPOSITORY                 TAG                                     IMAGE ID       CREATED          SIZE
> lenndewolten/dequeueable   azure-queue-storage-samplelistener-v2   26a4b226e4b8   46 seconds ago   92.6MB
```

## Kubernetes

### Deployment

```
kubectl apply -f azurite.yaml
kubectl apply -f deployment.yaml
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
> storage-azurite-service   NodePort    10.104.26.110   <none>        10000:32318/TCP,10001:30528/TCP,> 10002:30802/TCP   208d
```

#### **Construct connection string**

With the output above, the connection string would be:

```
DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://kubernetes.docker.internal:32318/devstoreaccount1;QueueEndpoint=http://kubernetes.docker.internal:30528/devstoreaccount1;TableEndpoint=http://kubernetes.docker.internal:30802/devstoreaccount1;
```

#### **Magic!**

After a message is added to the queue:

```
kubectl get pods

> NAME                                          READY   STATUS    RESTARTS       AGE
> queuefunction-deployment-7cbc8d8649-sjp9v     1/1     Running   0              44s
> storage-azurite-deployment-67f6f9b87b-wfgmh   1/1     Running   57 (13m ago)   197d
```

Logs when when four messages are handled:

```
kubectl logs pods/queuefunction-deployment-7cbc8d8649-sjp9v

> info: Microsoft.Hosting.Lifetime[0]
>       Application started. Press Ctrl+C to shut down.
> info: Microsoft.Hosting.Lifetime[0]
>       Hosting environment: Production
> info: Microsoft.Hosting.Lifetime[0]
>       Content root path: /app
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 0
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 0
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 1
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 1
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 2
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 2
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 0
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 0
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 3
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 3
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 1
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 1
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 4
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 4
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 2
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 2
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 5
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 5
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 3
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 3
> info: Dequeueable.AzureQueueStorage.Services.Queues.QueueMessageHandler[0]
>       Executed message with id '3566484e-895d-4949-96fd-a02799f4933f' (Succeeded)
> info: Dequeueable.AzureQueueStorage.Services.Queues.QueueMessageHandler[0]
>       Executed message with id '5307f82e-9d38-48ce-a80d-ee2812578484' (Succeeded)
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 4
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 4
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 5
> info: Dequeueable.AzureQueueStorage.SampleListener.Function.TestFunction[0]
>       Executing function loop 5
> info: Dequeueable.AzureQueueStorage.Services.Queues.QueueMessageHandler[0]
>       Executed message with id 'be0f39db-ff76-44cc-bc50-a7342bb4a023' (Succeeded)
> info: Dequeueable.AzureQueueStorage.Services.Queues.QueueMessageHandler[0]
>       Executed message with id '5540cb19-c318-4c7c-8e89-5d80b972b1e9' (Succeeded)
```
