using System.Linq;
using System.Runtime.Serialization;

namespace TicketMuffin.Web.Areas.Api.Models
{
    [DataContract(Name = "link", Namespace = "http://schemas.ticketmuffin.com/2011")]
    public class ResourceLink<T>
    {
        private string _rel;
        public ResourceLink()
        {
            var type = typeof (T);
            var attribute =
                type.GetCustomAttributes(typeof (DataContractAttribute), false).SingleOrDefault() as
                DataContractAttribute;
            if (attribute != null)
            {
                _rel = attribute.Namespace;
            }
        }

        [DataMember(Name = "rel")]
        public string Rel
        {
            get { return _rel; }
            set { _rel = value; }
        }

        [DataMember(Name="href")]
        public string Href { get; set; }
    }
}