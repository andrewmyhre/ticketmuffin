using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TicketMuffin.PayPal.Model
{
    [CollectionDataContract(Name = "errors", ItemName = "error", Namespace = Api.Namespace)]
    public class ErrorResponse : List<Error>
    {
        public ErrorResponse()
        {
            
        }

        public ErrorResponse(IEnumerable<Error> errorList)
        {
            AddRange(errorList);
        }
    }
}