using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GroupGiving.Web.Code;
using Microsoft.WindowsAzure;

namespace GroupGiving.Web.Controllers
{
    public class BindingTestController : Controller
    {
        private readonly StorageCredentials _credentials;
        private readonly BindingTest _bindingTest;

        public BindingTestController(StorageCredentials credentials )
        {
            _credentials = credentials;
        }

        //
        // GET: /BindingTest/

        public ActionResult Index()
        {
            return View();
        }

    }
}
