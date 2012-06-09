using System.Xml.Serialization;

namespace TicketMuffin.PayPal.Model
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/common")]
    [XmlRoot(Namespace = "http://svcs.paypal.com/types/common", IsNullable = false)]
    public partial class FaultMessage : ResponseBase
    {

        private string payKeyField;

        private string paymentExecStatusField;

        private PayResponseResponseEnvelope responseEnvelopeField;

        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string payKey
        {
            get { return this.payKeyField; }
            set { this.payKeyField = value; }
        }

        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string paymentExecStatus
        {
            get { return this.paymentExecStatusField; }
            set { this.paymentExecStatusField = value; }
        }

        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public PayResponseResponseEnvelope responseEnvelope
        {
            get { return this.responseEnvelopeField; }
            set { this.responseEnvelopeField = value; }
        }

        [XmlElement("error", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public PayPalError Error { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Error.Parameter, Error.Message);
        }
    }
}