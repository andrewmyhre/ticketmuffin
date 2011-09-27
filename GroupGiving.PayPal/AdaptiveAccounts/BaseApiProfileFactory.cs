using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.PayPal.Configuration;
using PayPal.Platform.SDK;

namespace GroupGiving.PayPal.AdaptiveAccounts
{
    public class BaseApiProfileFactory
    {
        public static BaseAPIProfile CreateFromConfiguration(PaypalAdaptiveAccountsConfigurationSection configuration)
        {
            return new BaseAPIProfile()
                       {
                           APIPassword = configuration.ApiPassword,
                           APIProfileType = ProfileType.ThreeToken,
                           APISignature = configuration.ApiSignature,
                           APIUsername = configuration.ApiUsername,
                           ApplicationID = configuration.ApplicationId,
                           DeviceIpAddress = configuration.DeviceIpAddress,
                           Environment = configuration.Environment,
                           RequestDataformat = configuration.RequestDataFormat,
                           ResponseDataformat = configuration.ResponseDataFormat,
                           SandboxMailAddress = configuration.SandboxMailAddress,
                           Subject = configuration.Subject,
                           Timeout = configuration.Timeout
                       };
        }
    }
}
