using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupGiving.Core.Domain;

namespace GroupGiving.Core.Data.Azure
{
    public class EntityMappings
    {
        public void CreateMaps()
        {
            AutoMapper.Mapper.CreateMap<GroupGivingEvent, EventRow>();
            AutoMapper.Mapper.CreateMap<EventRow, GroupGivingEvent>();
        }
    }
}
