using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Configuration;
using GroupGiving.PayPal.Configuration;
using PayPal.Platform.SDK;

namespace GroupGiving.PayPal.AdaptiveAccounts
{
    public class BaseApiProfileFactory
    {
        public static BaseAPIProfile CreateFromConfiguration(ISiteConfiguration configuration)
        {
            return new BaseAPIProfile()
                       {
                           APIPassword = configuration.AdaptiveAccountsConfiguration.ApiPassword,
                           APIProfileType = ProfileType.ThreeToken,
                           APISignature = configuration.AdaptiveAccountsConfiguration.ApiSignature,
                           APIUsername = configuration.AdaptiveAccountsConfiguration.ApiUsername,
                           ApplicationID = configuration.AdaptiveAccountsConfiguration.ApplicationId,
                           DeviceIpAddress = configuration.AdaptiveAccountsConfiguration.DeviceIpAddress,
                           Environment = configuration.AdaptiveAccountsConfiguration.ApiBaseUrl,
                           RequestDataformat = configuration.AdaptiveAccountsConfiguration.RequestDataFormat,
                           ResponseDataformat = configuration.AdaptiveAccountsConfiguration.ResponseDataFormat,
                           SandboxMailAddress = configuration.AdaptiveAccountsConfiguration.SandboxMailAddress
                       };
        }
    }
}
