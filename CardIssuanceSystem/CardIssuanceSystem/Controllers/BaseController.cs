using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CardIssuanceSystem.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                string controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                string action = filterContext.ActionDescriptor.ActionName;
                if ((controller != "Login") /*&& action != "Login"*/ && (Session.IsNewSession || Session["UserId"] == null))
                {
                    if (filterContext.HttpContext.Request.IsAjaxRequest())
                    {
                        // For AJAX requests, return result as a simple string, 
                        // and inform calling JavaScript code that a user should be redirected.
                        SessionLogout();
                        JsonResult result = Json("SessionTimeout", "text/html");
                        filterContext.Result = result;
                    }
                    else
                    {
                        // For round-trip requests,
                        SessionLogout();
                        filterContext.Result = new RedirectToRouteResult(new
                            RouteValueDictionary(new
                            {
                                controller = "Login",
                                action = "Index",
                                target = 0,
                            }));
                        filterContext.Result.ExecuteResult(filterContext);
                    }
                }

                else if ((controller != "Login")/*action != "Login"*/ && StateHelper.UserId <= 0)
                {
                    SessionLogout();
                    //filterContext.Result = new RedirectResult(Url.Action("Login", "Account"));

                    filterContext.Result = new RedirectToRouteResult(new
                    RouteValueDictionary(new
                    {
                        controller = "Login",
                        action = "Index",
                        target = 0,
                    }));
                    filterContext.Result.ExecuteResult(filterContext);

                    //filterContext.HttpContext.Response.Redirect(Url.Action("Login", "Account"), true);
                }

                base.OnActionExecuting(filterContext);
            }
            catch (Exception ex)
            {
                LogHelper.PrintError(ex.Message, ex);
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            LogHelper.PrintError(filterContext.Exception.Message, filterContext.Exception.InnerException);
            if (filterContext.ExceptionHandled)
                return;

            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { IsSuccess = false, ErrorMessage = filterContext.Exception.Message, Response = (int)HttpStatusCode.InternalServerError }
                };

            }
            else
                filterContext.Result = RedirectToAction("Error", "Base", new { title = filterContext.Exception.GetType() == typeof(CustomException) ? "Error Occurred!" : string.Empty, message = filterContext.Exception.GetType() == typeof(CustomException) ? filterContext.Exception.Message : string.Empty });

            filterContext.ExceptionHandled = true;
            
            base.OnException(filterContext);
        }

        public ActionResult Error(string title, string message)
        {
            var ViewModel = new MessageVM
            {
                Title = !string.IsNullOrEmpty(title) ? title : "Oops!",
                Paragraph = !string.IsNullOrEmpty(message) ? message : CustomMessages.SomethingWentWrong
            };

            return View("Message", ViewModel);
        }

        public ActionResult DashboardView()
        {
            return View("Dashboard");
        }

        public bool SessionLogout()
        {
            Session.Clear();
            return true;
        }
    }
}