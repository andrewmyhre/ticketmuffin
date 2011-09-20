using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using GroupGiving.Web.Areas.Api.Models;
using GroupGiving.Web.Models;

namespace GroupGiving.Web.Areas.Api.Controllers
{
    public class EventsController : ApiControllerBase
    {
        //
        // GET: /Api/Event/

        public EventsController(IRepository<GroupGivingEvent> eventRepository) : base(eventRepository)
        {
        }

        public ActionResult Index(string shortUrl)
        {
            var @event = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);

            return Response(@event);
        }

        [HttpPost]
        public ActionResult ShareViaEmail(string shortUrl, ShareViaEmailViewModel request)
        {
            var @event = _eventRepository.Retrieve(e => e.ShortUrl == shortUrl);
            var eventModel = AutoMapper.Mapper.Map<GroupGivingEvent>(@event);

            return Response(
                new ApiResponse<EventModel>() {
                    Link = new ResourceLink<EventModel>(){Href="http://www.ticketmuffin.com/"+eventModel.ShortUrl}},
                    HttpStatusCode.OK);
        }
    }

    [DataContract(Name = "link", Namespace = "http://schemas.ticketmuffin.com/2011")]
    public class ResourceLink<T>
    {
        private string _rel;
        public ResourceLink()
        {
            var type = typeof (T);
            var attribute =
                type.GetCustomAttributes(typeof (DataContractAttribute), false).SingleOrDefault() as
                DataContractAttribute;
                if (attribute != null)
                {
                    _rel = attribute.Namespace;
                }
        }

        [DataMember(Name = "rel")]
        public string Rel
        {
            get { return _rel; }
            set { _rel = value; }
        }

        [DataMember(Name="href")]
        public string Href { get; set; }
    }
}
