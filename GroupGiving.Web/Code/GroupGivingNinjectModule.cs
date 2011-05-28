using System;
using System.Configuration;
using GroupGiving.Core.Data;
using GroupGiving.Core.Data.Azure;
using GroupGiving.Core.Data.Fakes;
using GroupGiving.Core.Domain;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject.Modules;

namespace GroupGiving.Web.Code
{
    public class GroupGivingNinjectModule : NinjectModule
    {
        public override void Load()
        {
            //Bind<IRepository<GroupGivingEvent>>().To<FakeRepository<GroupGivingEvent>>();

            /*
            Bind<StorageCredentials>()
                .ToMethod(delegate { return CloudConfiguration.GetStorageAccount("DataConnectionString").Credentials; });
            Bind<IAzureRepository<EventRow>>().To<AzureRepository<EventRow>>();
            Bind<IRepository<GroupGivingEvent>>().To<AzureEventRepository>();
            Bind<AzureRepository<EventRow>>().ToSelf();

            Bind<CloudStorageAccount>()
                .ToMethod(delegate { return CloudConfiguration.GetStorageAccount("DataConnectionString"); });
             * */

            Bind<IRepository<GroupGivingEvent>>().To<AzureEventRepository>();
            Bind<IAzureRepository<EventRow>>().To<AzureRepository<EventRow>>();
            Bind<BindingTest>().ToMethod(delegate { return BindingTest.GetInstance(); });
            Bind<CloudStorageAccount>()
                .ToMethod(delegate
                              {
                                  CloudStorageAccount
                                                            .SetConfigurationSettingPublisher((configName, configSettingPublisher) =>
                                                            {
                                                                var connectionString = 
                                                                    RoleEnvironment.IsAvailable
                                                                    ? RoleEnvironment.GetConfigurationSettingValue (configName)
                                                                    : ConfigurationManager.AppSettings[configName];
                                                                configSettingPublisher (connectionString);
                                                            });
                                  return CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
                              });
            Bind<StorageCredentials>().ToMethod(delegate
                                                    {
                                                        CloudStorageAccount
                                                            .SetConfigurationSettingPublisher((configName, configSettingPublisher) =>
                                                            {
                                                                var connectionString = 
                                                                    RoleEnvironment.IsAvailable
                                                                    ? RoleEnvironment.GetConfigurationSettingValue (configName)
                                                                    : ConfigurationManager.AppSettings[configName];
                                                                configSettingPublisher (connectionString);
                                                            });
                                                        return CloudStorageAccount.FromConfigurationSetting("DataConnectionString").Credentials;
                                                    });
        }
    }

    
}