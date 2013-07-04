using System.ComponentModel.DataAnnotations;

namespace TicketMuffin.PayPal.Configuration
{
    public class AdaptiveAccountsConfiguration
    {
        [Display(Name="PayPal merchant account email")]
        public string PayPalAccountEmail { get; set; }

        [Display(Prompt="api password")]
        [Required]
        public string ApiPassword { get; set; }

        [Display(Prompt = "api username")]
        [Required]
        public string ApiUsername { get; set; }

        [Display(Prompt = "api signature")]
        [Required]
        public string ApiSignature { get; set; }

        [Display(Prompt = "sandbox application id")]
        [Required]
        public string SandboxApplicationId { get; set; }

        [Display(Prompt = "live application id")]
        [Required]
        public string ApplicationId { get; set; }

        [Required]
        [Display(Prompt = "device ip address e.g: 127.0.0.1")]
        public string DeviceIpAddress { get; set; }
        
        [Required]
        [Display(Prompt = "sandbox api base url e.g: https://svcs.sandbox.paypal.com/")]
        public string SandboxApiBaseUrl { get; set; }

        [Required]
        [Display(Prompt = "live api base url e.g: https://svcs.paypal.com/")]
        public string ApiBaseUrl { get; set; }

        [Required]
        [Display(Prompt = "SOAP11")]
        public string RequestDataFormat { get; set; }

        [Required]
        [Display(Prompt = "SOAP11")]
        public string ResponseDataFormat { get; set; }

        [Required]
        public string SandboxMailAddress { get; set; }

        /* payflowpro */
        public string SuccessCallbackUrl { get; set; }
        public string FailureCallbackUrl { get; set; }

        [Required]
        [Display(Prompt = "https://www.sandbox.paypal.com/webscr?cmd=_ap-payment&amp;paykey={0}")]
        public string SandboxPayFlowProPaymentPage { get; set; }
        [Required]
        [Display(Prompt = "https://www.paypal.com/webscr?cmd=_ap-payment&amp;paykey={0}")]
        public string PayFlowProPaymentPage { get; set; }

        [Required]
        [Display(Prompt = "1.1.0")]
        public string ApiVersion { get; set; }

        [Required]
        [Display(Prompt = "XML")]
        public string RequestDataBinding { get; set; }

        [Required]
        [Display(Prompt = "XML")]
        public string ResponseDataBinding { get; set; }

    }
}