namespace GroupGiving.PayPal
{
    public class ApiClientSettings
    {
        public ApiClientSettings()
        {
            ApiEndpoint = "https://svcs.sandbox.paypal.com/AdaptivePayments/Pay";
            ApiVersion = "1.1.0";
            RequestDataBinding = "XML";
            ResponseDataBinding = "XML";
        }

        public string ApiEndpoint { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Signature { get; set; }
        public string ApiVersion { get; set; }
        public string RequestDataBinding { get; set; }
        public string ResponseDataBinding { get; set; }
    }
}