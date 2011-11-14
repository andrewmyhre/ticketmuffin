using System.Xml.Schema;
using System.Xml.Serialization;
using GroupGiving.Core.Dto;

namespace GroupGiving.PayPal.Model
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

    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://svcs.paypal.com/types/ap", IsNullable = false)]
    public class RefundResponse : ResponseBase
    {
        private RefundResponseRefundInfo[] _refundInfoListField;

        [XmlElement(ElementName = "responseEnvelope", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public PayResponseResponseEnvelope ResponseEnvelope { get; set; }

        [System.Xml.Serialization.XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlArrayItemAttribute("refundInfo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public RefundResponseRefundInfo[] refundInfoList
        {
            get
            {
                return this._refundInfoListField;
            }
            set
            {
                this._refundInfoListField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    public partial class RefundResponseRefundInfo
    {

        private string refundStatusField;

        private string refundHasBecomeFullField;

        private RefundResponseRefundInfoReceiver[] receiverField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
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
        [System.Xml.Serialization.XmlElementAttribute("receiver", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    public partial class RefundResponseRefundInfoReceiver
    {

        private string amountField;

        private string emailField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
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