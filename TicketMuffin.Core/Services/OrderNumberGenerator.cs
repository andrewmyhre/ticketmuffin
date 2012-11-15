using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TicketMuffin.Core.Domain;

namespace TicketMuffin.Core.Services
{
    public class OrderNumberGenerator : IOrderNumberGenerator
    {
        public string Generate(GroupGivingEvent groupGivingEvent)
        {

            int nextAvailable = groupGivingEvent.AttendeeCount + 1;
            return nextAvailable.ToString("00000");
        }
    }

    public interface IOrderNumberGenerator
    {
        string Generate(GroupGivingEvent groupGivingEvent);
    }
}
