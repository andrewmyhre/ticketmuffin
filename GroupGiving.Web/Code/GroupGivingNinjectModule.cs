using System;
using GroupGiving.Core.Data;
using GroupGiving.Core.Data.Azure;
using GroupGiving.Core.Data.Fakes;
using GroupGiving.Core.Domain;
using Ninject.Modules;

namespace GroupGiving.Web.Code
{
    public class GroupGivingNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepository<GroupGivingEvent>>().To<FakeRepository<GroupGivingEvent>>();

            /*Bind<IRepository<GroupGivingEvent>>()
                .ToMethod(context=>new AzureEventRepository(
                              new AzureRepository<EventRow>(
                                  CloudConfiguration.GetStorageAccount("DataConnectionString"))));*/
        }
    }
}