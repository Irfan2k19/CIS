using CardIssuanceSystem.Core.Methods;
using CardIssuanceSystem.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CardIssuanceSystem.Controllers
{
    public class MainMenuController : Controller
    {
       public ActionResult GetMenu()
       {
            List<MenuVM> lst = new List<MenuVM>();
            lst = MenuMethods.GetMenuItems();
            return View(lst);
       }
    }
}