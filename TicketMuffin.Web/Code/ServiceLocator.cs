using Ninject;

namespace TicketMuffin.Web.Code
{
    public class ServiceLocator
    {
        static ServiceLocator()
        {
            Instance = null;
        }

        public static IKernel Instance { get; set; }
    }
}