using IISCI.Web.Models;
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IISCI.Web.Controllers
{
    [Authorize(Users = "*")]
    public class HomeController : Controller
    {


        public ActionResult Index() {

            return View();
        } 

    }
}