using System;
using System.Collections.Generic;
using System.Linq;
using GroupGiving.Core.Data.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Diagnostics;
using System.Threading;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;

namespace GroupGiving.Web
{
    public class WebRole : RoleEntryPoint
    {
        [Inject]
        public CloudStorageAccount Account { get; set; }

        public override void Run()
        {
            // This is a sample webrole implementation. Replace with your logic.
            Trace.WriteLine("GroupGiving.Web entry point called", "Information");

            while (true)
            {
                Thread.Sleep(10000);
                Trace.WriteLine("Working", "Information");
            }
        }

        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            var types = new AzureDomainTypes().All();
            if (Account != null)
            {
                var client = Account.CreateCloudTableClient();
                foreach (var t in types)
                {
                    client.CreateTableIfNotExist(t.Name);
                }
            }

            return base.OnStart();
        }
    }
}
