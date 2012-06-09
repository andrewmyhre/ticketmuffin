using TicketMuffin.Core.Domain;
using TicketMuffin.Web.Areas.Api.Models;

namespace TicketMuffin.Web.Areas.Api.Code
{
    public static class AutoMappingConfiguration
    {
        public static void Configure()
        {
            AutoMapper.Mapper.CreateMap<GroupGivingEvent, EventModel>();
        }
    }
}