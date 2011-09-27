using System.Collections.Generic;
using System.Xml.Serialization;

namespace GroupGiving.Core.Dto
{
    public interface IPaymentGatewayResponse
    {
        [XmlElement(ElementName = "responseEnvelope", Order = 0)]
        IResponseEnvelope ResponseEnvelope { get; set; }

        [XmlElement(ElementName = "payKey", Order = 1)]
        string TransactionId { get; set; }

        [XmlElement(ElementName = "paymentExecStatus", Order = 2)]
        string PaymentExecStatus { get; set; }

        [XmlArrayItem(ElementName = "error")]
        IEnumerable<ResponseError> Errors { get; set; }

        [XmlElement(ElementName = "paymentPageUrl")]
        string PaymentPageUrl { get; set; }
    }
}