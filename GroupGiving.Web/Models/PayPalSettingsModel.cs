using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupGiving.Web.Models
{
    public class PayPalSettingsModel
    {
        public string PayPalFirstName { get; set; }
        public string PayPalLastName { get; set; }
        public string PayPalEmail { get; set; }
    }
}