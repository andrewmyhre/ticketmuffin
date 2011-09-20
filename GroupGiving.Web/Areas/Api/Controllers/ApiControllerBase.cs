using System.Runtime.Serialization;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using GroupGiving.Core.Data;
using GroupGiving.Core.Domain;
using Ninject;

namespace GroupGiving.Web.Areas.Api.Controllers
{
    public class ApiControllerBase : Controller
    {
        protected IRepository<GroupGivingEvent> _eventRepository = null;
        public ApiControllerBase()
        {
            _eventRepository = MvcApplication.Kernel.Get<IRepository<GroupGivingEvent>>();
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
    }
}