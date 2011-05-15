using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GroupGiving.PayPal.Model
{
    [XmlRoot(ElementName="PayResponse")]
    public class PayResponse
    {
        [XmlElement(ElementName="responseEnvelope", Order=0)]
        public ResponseEnvelope ResponseEnvelope { get; set; }
        [XmlElement(ElementName="payKey", Order=1)]
        public string PayKey { get; set; }
        [XmlElement(ElementName = "paymentExecStatus", Order = 2)]
        public string PaymentExecStatus { get; set; }
        [XmlArrayItem(ElementName = "error")]
        public IEnumerable<ResponseError> Errors { get; set; }
    }

    [XmlRoot(ElementName="responseEnvelope")]
    public class ResponseEnvelope
    {
        [XmlElement(ElementName = "timestamp", Order = 0)]
        public DateTime Timestamp { get; set; }
        [XmlElement(ElementName = "ack", Order = 1)]
        public string Ack { get; set; }
        [XmlElement(ElementName = "correlationId", Order = 2)]
        public string CorrelationId { get; set; }
        [XmlElement(ElementName = "build", Order = 3)]
        public long Build { get; set; }
    }
}
