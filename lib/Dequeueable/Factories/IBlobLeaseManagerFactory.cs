using Dequeueable.Services.DistributedLock;


namespace Dequeueable.Factories
{
    internal interface IBlobLeaseManagerFactory
    {
        IBlobLeaseManager Create(string fileName);
    }
}