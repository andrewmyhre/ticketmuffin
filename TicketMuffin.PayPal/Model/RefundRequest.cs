using System.Xml.Serialization;

namespace TicketMuffin.PayPal.Model
{
    public class RefundRequest : IPayPalRequest
    {
        [XmlElement(ElementName = "payKey")]
        public string PayKey { get; set; }

        [XmlElement(ElementName = "currencyCode")]
        public string CurrencyCode { get; set; }

        [XmlElement(ElementName = "requestEnvelope")]
        public RequestEnvelope RequestEnvelope { get; set; }

        [XmlElement(ElementName = "clientDetails")]
        public ClientDetails ClientDetails { get; set; }

        public ReceiverList Receivers { get; set; }

        public RefundRequest()
        {
            
        }

        public RefundRequest(string payKey)
        {
            PayKey = payKey;
            ClientDetails = ClientDetails.Default;
            RequestEnvelope = new RequestEnvelope()
                                  {
                                      ErrorLanguage = "en_US"
                                  };

        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    public partial class RefundResponseRefundInfo
    {

        private string refundStatusField;

        private string refundHasBecomeFullField;

        private RefundResponseRefundInfoReceiver[] receiverField;

        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string refundStatus
        {
            get
            {
                return this.refundStatusField;
            }
            set
            {
                this.refundStatusField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string refundHasBecomeFull
        {
            get
            {
                return this.refundHasBecomeFullField;
            }
            set
            {
                this.refundHasBecomeFullField = value;
            }
        }

        /// <remarks/>
        [XmlElement("receiver", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public RefundResponseRefundInfoReceiver[] receiver
        {
            get
            {
                return this.receiverField;
            }
            set
            {
                this.receiverField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    public partial class RefundResponseRefundInfoReceiver
    {

        private string amountField;

        private string emailField;

        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string amount
        {
            get
            {
                return this.amountField;
            }
            set
            {
                this.amountField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string email
        {
            get
            {
                return this.emailField;
            }
            set
            {
                this.emailField = value;
            }
        }
    }
}