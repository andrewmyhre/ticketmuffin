using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Linq;

namespace TicketMuffin.Web.Areas.Api.Controllers
{
    public class ApiControllerBase : Controller
    {
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

        protected ActionResult ApiResponse<T>(T graph, HttpStatusCode statusCode)
        {
            base.Response.StatusCode = (int)statusCode;
            if (Request.AcceptTypes != null && Request.AcceptTypes.Contains("application/json"))
            {
                return Json(graph, JsonRequestBehavior.AllowGet);
            }

            return Xml(graph);
        }

        protected ActionResult ApiResponse<T>(T graph)
        {
            return ApiResponse(graph, HttpStatusCode.OK);
        }
    }
}