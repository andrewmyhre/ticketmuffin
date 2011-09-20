using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace GroupGiving.Web.Models
{
    [CollectionDataContract(Name="errors", ItemName="error", Namespace="http://schemas.ticketmuffin.com/2011")]
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

    [DataContract(Name = "error", Namespace = "http://schemas.ticketmuffin.com/2011")]
    public class Error
    {
        [DataMember(Name="field")]
        public string Field { get; set; }
        [DataMember(Name="message")]
        public string ErrorMessage { get; set; }
    }
}