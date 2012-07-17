using System.Globalization;
using System.Web.Mvc;

namespace TicketMuffin.Core.Services
{
    public class NonEnglishNumbersModelBinder : DefaultModelBinder
    {
        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext,
                                             System.ComponentModel.PropertyDescriptor propertyDescriptor)
        {
            base.BindProperty(controllerContext, bindingContext, propertyDescriptor);

            if (propertyDescriptor.PropertyType.IsEquivalentTo(typeof(float))
                && bindingContext.ModelState[propertyDescriptor.Name].Errors != null
                && bindingContext.ModelState[propertyDescriptor.Name].Errors.Count > 0)
            {
                // try converting using english format
                float tryValue = 0;
                if (float.TryParse(controllerContext.RequestContext.HttpContext.Request[propertyDescriptor.Name], 
                    NumberStyles.Float, new CultureInfo("en"), out tryValue))
                {
                    propertyDescriptor.SetValue(bindingContext.Model, tryValue);
                    bindingContext.ModelState[propertyDescriptor.Name].Errors.Clear();
                }
            }
            else if (propertyDescriptor.PropertyType.IsEquivalentTo(typeof(decimal))
              && bindingContext.ModelState[propertyDescriptor.Name].Errors != null
              && bindingContext.ModelState[propertyDescriptor.Name].Errors.Count > 0)
            {
                // try converting using english format
                decimal tryValue = 0;
                if (decimal.TryParse(controllerContext.RequestContext.HttpContext.Request[propertyDescriptor.Name],
                    NumberStyles.Currency, 
                    new CultureInfo("en"), 
                    out tryValue))
                {
                    propertyDescriptor.SetValue(bindingContext.Model, tryValue);
                    bindingContext.ModelState[propertyDescriptor.Name].Errors.Clear();
                }
            }
        }
    }
}