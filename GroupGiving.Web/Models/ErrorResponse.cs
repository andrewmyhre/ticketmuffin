using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace GroupGiving.Web.Models
{
    [DataContract(Name = "errors", Namespace = "http://schemas.ticketmuffin.com/2011")]
    public class ErrorResponse
    {
        public IEnumerable<Error> Errors { get; set; } 
    }

    [DataContract(Name = "error", Namespace = "http://schemas.ticketmuffin.com/2011")]
    public class Error
    {
        [DataMember(Name="key")]
        public string Key { get; set; }
        [DataMember(Name="message")]
        public string ErrorMessage { get; set; }
    }
}