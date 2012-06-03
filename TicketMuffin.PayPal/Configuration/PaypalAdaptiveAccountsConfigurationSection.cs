using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace GroupGiving.PayPal.Configuration
{
    public class PaypalAdaptiveAccountsConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("ApiUsername")]
        public string ApiUsername
        {
            get { return (string) this["ApiUsername"]; }
        }

        [ConfigurationProperty("ApiPassword")]
        public string ApiPassword
        {
            get { return (string) this["ApiPassword"]; }
        }

        [ConfigurationProperty("ApiSignature")]
        public string ApiSignature
        {
            get { return (string) this["ApiSignature"]; }
        }

        [ConfigurationProperty("ApplicationId")]
        public string ApplicationId
        {
            get { return (string) this["ApplicationId"]; }
        }

        [ConfigurationProperty("DeviceIpAddress")]
        public string DeviceIpAddress
        {
            get { return (string) this["DeviceIpAddress"]; }
        }

        [ConfigurationProperty("Environment", DefaultValue = "https://svcs.sandbox.paypal.com/")]
        public string Environment
        {
            get { return (string)this["Environment"]; }
        }

        [ConfigurationProperty("RequestDataFormat", DefaultValue="SOAP11")]
        public string RequestDataFormat
        { get { return (string) this["RequestDataFormat"]; } }

    
        [ConfigurationProperty("ResponseDataFormat", DefaultValue = "SOAP11")]
        public string ResponseDataFormat
        {
            get { return (string) this["ResponseDataFormat"]; }
        }

        [ConfigurationProperty("SandboxMailAddress")]
        public string SandboxMailAddress
        {
            get { return (string) this["SandboxMailAddress"]; }
        }

        [ConfigurationProperty("Subject")]
        public string Subject
        {
            get { return (string) this["Subject"]; }
        }

        [ConfigurationProperty("Timeout")]
        public int Timeout
        {
            get { return (int) this["Timeout"]; }
        }
    }
}
