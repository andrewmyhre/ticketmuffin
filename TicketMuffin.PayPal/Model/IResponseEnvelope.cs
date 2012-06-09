using System;
using System.Xml.Serialization;

namespace TicketMuffin.PayPal.Model
{
    public interface IResponseEnvelope
    {
        [XmlElement(ElementName = "timestamp", Order = 0)]
        DateTime Timestamp { get; set; }

        [XmlElement(ElementName = "ack", Order = 1)]
        string Ack { get; set; }

        [XmlElement(ElementName = "correlationId", Order = 2)]
        string CorrelationId { get; set; }

        [XmlElement(ElementName = "build", Order = 3)]
        long Build { get; set; }
    }
}