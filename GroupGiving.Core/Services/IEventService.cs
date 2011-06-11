using System.Collections.Generic;
using GroupGiving.Core.Domain;

namespace GroupGiving.Core.Services
{
    public interface IEventService
    {
        IEnumerable<GroupGivingEvent> RetrieveAllEvents();
    }
}