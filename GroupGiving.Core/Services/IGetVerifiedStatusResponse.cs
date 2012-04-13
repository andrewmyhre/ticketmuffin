using System;
using System.Xml.Serialization;

namespace GroupGiving.Core.Services
{
    [XmlType(AnonymousType = true, Namespace = "http://svcs.paypal.com/types/aa")]
    [XmlRoot(ElementName = "GetVerifiedStatusResponse", Namespace = "http://svcs.paypal.com/types/aa", IsNullable = false)]
    public interface IGetVerifiedStatusResponse
    {
        string AccountStatus { get; }
        bool Verified { get; }
        bool Success { get; }
    }
}