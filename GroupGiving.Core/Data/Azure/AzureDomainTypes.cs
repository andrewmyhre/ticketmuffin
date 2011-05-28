using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace GroupGiving.Core.Data.Azure
{
    public class AzureDomainTypes
    {
        public IEnumerable<Type> All()
        {
            return (from t in Assembly.GetAssembly(this.GetType()).GetTypes()
                    where t.BaseType != null && t.BaseType.Name.Contains("TableServiceEntity")
                    select t);

        }
    }
}
