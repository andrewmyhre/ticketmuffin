using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;

namespace TicketMuffin.Web.Areas.Api.Code
{
    public class XmlModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(Type modelType)
        {
            if (HttpContext.Current.Request.ContentType.ToLower().Contains("application/xml") 
                && modelType.GetCustomAttributes(typeof(DataContractAttribute), false).Count() > 0)
            {
                
                return new XmlModelBinder();
            }

            return null;
        }
    }

    public class XmlModelBinder : DefaultModelBinder
    {
        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            DataContractSerializer dcs = new DataContractSerializer(bindingContext.ModelType);

            controllerContext.HttpContext.Request.InputStream.Position = 0;
            try
            {
                var result = dcs.ReadObject(controllerContext.HttpContext.Request.InputStream);
                return result;
            }
            catch
            {
                return null;
            }

        }
    }
}