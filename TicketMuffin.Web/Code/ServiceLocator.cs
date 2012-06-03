using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ninject;

namespace GroupGiving.Web.Code
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