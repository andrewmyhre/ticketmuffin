using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;

namespace GroupGiving.Web.Areas.Api.Code
{
    public class XmlModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(Type modelType)
        {
            if (HttpContext.Current.Request.ContentType.ToLower().Contains("application/xml"))
            {
                return new XmlModelBinder();
            }

            return null;
        }
    }

    public class XmlModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            DataContractSerializer dcs = new DataContractSerializer(bindingContext.ModelType);
            


            var modelType = bindingContext.ModelType;
            var serializer = new XmlSerializer(modelType);

            MemoryStream ms = new MemoryStream();
            controllerContext.HttpContext.Request.InputStream.CopyTo(ms);
            
            StreamReader reader = new StreamReader(controllerContext.HttpContext.Request.InputStream);
            string content = reader.ReadToEnd();

            ms.Seek(0, SeekOrigin.Begin);
            XmlReader xmlReader = XmlReader.Create(ms);

            return dcs.ReadObject(xmlReader);
            //return serializer.Deserialize(xmlReader);
        }
    }
}