using Microsoft.WindowsAzure.StorageClient;

namespace GroupGiving.Core.Data.Azure
{
    public interface IAzureRepository<T> :IRepository<T> where T : TableServiceEntity
    {
        
    }
}