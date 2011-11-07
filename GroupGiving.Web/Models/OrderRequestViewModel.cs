using System;
using System.Collections.Generic;
using GroupGiving.Core.Dto;
using GroupGiving.PayPal;

namespace GroupGiving.Web.Models
{
    public class OrderRequestViewModel
    {
        public string PayKey { get; set; }

        public string Ack { get; set; }

        public ResponseError Error { get; set; }

        public string PayPalPostUrl { get; set; }
    }
}