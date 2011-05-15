using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using GroupGiving.Core.Domain;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace GroupGiving.Core.Data.Azure
{
    public class AzureRepository<T> where T : TableServiceEntity
    {
        private readonly CloudStorageAccount _account;
        private readonly CloudTableClient _client;
        private readonly string _tableName;

        public AzureRepository(CloudStorageAccount account)
        {
            _account = account;
            _tableName = typeof (T).Name;
            _client = _account.CreateCloudTableClient();
            _client.CreateTableIfNotExist(_tableName);
        }

        public string Table { get { return typeof (T).Name; }}

        public IQueryable<T> All
        {
            get
            {
                var context = _client.GetDataServiceContext();
                return context.CreateQuery<T>(Table).AsTableServiceQuery();
            }
        }

        public T Retrieve(Guid id)
        {
            return All.Where(o => o.RowKey == id.ToString()).FirstOrDefault();
        }

        public void Save(T graph)
        {
            var context = _client.GetDataServiceContext();
            context.AddObject(_tableName, graph);

            try
            {
                context.SaveChangesWithRetries();
            }catch
            {
                throw;
            }
        }
    }
}
