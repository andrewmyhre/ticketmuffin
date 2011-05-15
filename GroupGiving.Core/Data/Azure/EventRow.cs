using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace GroupGiving.Core.Data.Azure
{
    public class EventRow : TableServiceEntity
    {
        public EventRow()
        {
            
        }
        public EventRow(Guid id, string city)
            : base(city, id.ToString())
        {
            
        }

        public Guid Id
        {
            get
            {
                return Guid.Parse(RowKey); }
            set
            {
                RowKey=value.ToString();
            }
        }

        public string City
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public string Name { get; set; }
    }
}
