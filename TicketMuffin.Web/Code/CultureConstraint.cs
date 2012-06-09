using System.Linq;
using System.Web;
using System.Web.Routing;

namespace TicketMuffin.Web.Code
{
    public class CultureConstraint : IRouteConstraint
    {
        private readonly string[] _values;

        public CultureConstraint(params string[] values)
        {
            _values = values;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            string value = values[parameterName].ToString();
            return _values.Contains(value);
        }
    }
}