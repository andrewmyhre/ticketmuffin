using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GroupGiving.Core.Domain;
using GroupGiving.Web.Areas.Api.Models;

namespace GroupGiving.Web.Areas.Api.Code
{
    public static class AutoMappingConfiguration
    {
        public static void Configure()
        {
            AutoMapper.Mapper.CreateMap<GroupGivingEvent, EventModel>();
        }
    }
}