using System;
using System.Configuration;
using GroupGiving.Core.Configuration;

namespace GroupGiving.PayPal
{
    public class PayPalConfiguration : ConfigurationSection, IPayPalConfiguration
    {
        [ConfigurationProperty("merchantUserName", IsRequired=true)]
        public string PayPalMerchantUsername
        {
            get { return (string) this["merchantUserName"]; }
            set { this["merchantUserName"]=value; }
        }

        [ConfigurationProperty("merchantPassword", IsRequired = true)]
        public string PayPalMerchantPassword
        {
            get { return (string)this["merchantPassword"]; }
            set { this["merchantPassword"]=value; }
        }

        [ConfigurationProperty("merchantSignature", IsRequired = true)]
        public string PayPalMerchantSignature
        {
            get { return (string)this["merchantSignature"]; }
            set { this["merchantSignature"]=value; }
        }

        [ConfigurationProperty("successCallbackUrl", IsRequired=true)]
        public string SuccessCallbackUrl
        {
            get { return (string)this["successCallbackUrl"]; }
            set { this["successCallbackUrl"]=value; }
        }

        [ConfigurationProperty("failureCallbackUrl", IsRequired = true)]
        public string FailureCallbackUrl
        {
            get { return (string)this["failureCallbackUrl"]; }
            set { this["failureCallbackUrl"] = value; }
        }

        [ConfigurationProperty("payFlowProPaymentPage", IsRequired = true)]
        public string PayFlowProPaymentPage
        {
            get { return (string)this["payFlowProPaymentPage"]; }
            set { this["payFlowProPaymentPage"] = value; }
        }

        [ConfigurationProperty("ticketMuffinPayPalAccountEmail", IsRequired=true)]
        public string TicketMuffinPayPalAccountEmail
        {
            get { return (string)this["ticketMuffinPayPalAccountEmail"]; }
        }
    }
}