using System.Xml.Serialization;

namespace GroupGiving.PayPal.Model
{
    [XmlType(AnonymousType = true, Namespace="http://svcs.paypal.com/types/ap")]
    [System.Serializable]
    public class PayPalError
    {
        [XmlElement("errorId", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ErrorId { get; set; }

        [XmlElement("domain", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Domain { get; set; }

        [XmlElement("subDomain", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SubDomain { get; set; }

        [XmlElement("severity", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Severity { get; set; }

        [XmlElement("category", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Category { get; set; }

        [XmlElement("message", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Message { get; set; }

        [XmlElement("parameter", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Parameter { get; set; }
    }
}