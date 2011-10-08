using System;
using GroupGiving.Core.Domain;

namespace GroupGiving.Core.Services
{
    public class CreateEventResult
    {
        public bool Success { get; set; }

        public GroupGivingEvent Event { get; set; }
    }
}