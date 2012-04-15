using System.Xml.Serialization;

namespace GroupGiving.PayPal.Model
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    public partial class PayResponseResponseEnvelope
    {

        private string timestampField;

        private string ackField;

        private string correlationIdField;

        private string buildField;

        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string timestamp
        {
            get { return this.timestampField; }
            set { this.timestampField = value; }
        }

        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ack
        {
            get { return this.ackField; }
            set { this.ackField = value; }
        }

        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string correlationId
        {
            get { return this.correlationIdField; }
            set { this.correlationIdField = value; }
        }

        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string build
        {
            get { return this.buildField; }
            set { this.buildField = value; }
        }
    }
}