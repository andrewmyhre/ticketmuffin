using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GroupGiving.Web.Areas.Api.Models
{
    [CollectionDataContract(Name = "errors", ItemName = "error", Namespace = Areas.Api.Code.Api.Namespace)]
    public class ErrorResponse : List<Error>
    {
        public ErrorResponse()
        {
            
        }

        public ErrorResponse(IEnumerable<Error> errorList)
        {
            base.AddRange(errorList);
        }

        public void AddRange(IEnumerable<Error> errors)
        {
            base.AddRange(errors);  
        }

        public void Add(Error error)
        {
            base.Add(error);
        }
    }
}