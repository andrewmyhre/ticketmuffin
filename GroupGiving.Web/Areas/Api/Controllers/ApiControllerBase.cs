using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using System.Linq;
using Ninject;

namespace GroupGiving.Web.Areas.Api.Controllers
{
    public class ApiControllerBase : Controller
    {
        protected IRepository<GroupGivingEvent> _eventRepository = null;
        public ApiControllerBase(IRepository<GroupGivingEvent> eventRepository)
        {
            _eventRepository = eventRepository;
        }

        protected ContentResult Xml<T>(T graph)
        {
            DataContractSerializer dcs = new DataContractSerializer(typeof(T));
            ContentResult result = new ContentResult();
            StringBuilder sb = new StringBuilder();
            XmlWriter w = XmlWriter.Create(sb);
            dcs.WriteObject(w, graph);
            w.Flush();
            result.Content = sb.ToString();
            result.ContentType = "application/xml";
            return result;
        }

        protected ActionResult Response<T>(T graph, HttpStatusCode statusCode)
        {
            base.Response.StatusCode = (int)statusCode;
            if (Request.AcceptTypes.Contains("application/json"))
            {
                return Json(graph, JsonRequestBehavior.AllowGet);
            }

            return Xml(graph);
        }

        protected ActionResult Response<T>(T graph)
        {
            return Response(graph, HttpStatusCode.OK);
        }
    }
}