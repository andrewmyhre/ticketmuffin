using System.Linq;
using System.Xml.Serialization;

namespace TicketMuffin.PayPal.Model
{
    [System.SerializableAttribute()]
    [XmlType(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/ap")]
    [XmlRoot(Namespace = "http://svcs.paypal.com/types/ap", IsNullable = false)]
    public class RefundResponse : ResponseBase
    {
        private RefundResponseRefundInfo[] _refundInfoListField;

        [XmlElement(ElementName = "responseEnvelope", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public PayResponseResponseEnvelope ResponseEnvelope { get; set; }

        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlArrayItem("refundInfo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
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

        public bool Successful
        {
            get
            {
                return ResponseEnvelope.ack.StartsWith("Success")
                       && refundInfoList.All(ri => ri.refundStatus == "REFUNDED"
                                                            || ri.refundStatus == "REFUNDED_PENDING"
                                                            || ri.refundStatus == "NOT_PAID"
                                                            || ri.refundStatus == "ALREADY_REVERSED_OR_REFUNDED");
            }
        }
    }
}