using CardIssuanceSystem.Core.Methods;
using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL;
using CardIssuanceSystem.DAL.DataAccessClasses;
using CardIssuanceSystem.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CardIssuanceSystem.Controllers
{
    public class SystemReviewController : BaseController
    {
        [AuthOp]
        public ActionResult ProfileReview()
        {
            return View();
        }

        [AuthOp]
        public ActionResult UserProfileReview()
        {
            var viewModel = new List<SystemRequestVM>();
            return View(viewModel);
        }

        [AuthOp]
        public ActionResult FilteredUserProfileAddReview(string requestType)
        {
            //var lstRequests = SystemRequestMethods.GetSystemRequestForUser("R", requestType);
            var lstRequests = SystemRequestMethods.GetUserDetails("R", requestType);
            return PartialView("_partialUserProfileAddReview", lstRequests);
        }

        [AuthOp]
        public ActionResult FilteredUserProfileUpdateReview(string requestType)
        {
            var lstRequests = SystemRequestMethods.GetSystemRequestForUser("R", requestType);
            return PartialView("_partialUserProfileUpdateReview", lstRequests);
        }

        [AuthOp]
        public ActionResult PageProfileReview()
        {
            var viewModel = new List<SystemRequestVM>();
            return View(viewModel);
        }

        [AuthOp]
        public ActionResult FilteredPageProfileAddReview(string requestType)
        {
            var lstRequests = SystemRequestMethods.GetProfileDetails("R", requestType);
            return PartialView("_partialPageProfileAddReview", lstRequests);
        }
        [AuthOp]
        public ActionResult FilteredPageProfileUpdateReview(string requestType)
        {
            var lstRequests = SystemRequestMethods.GetSystemRequestForProfile("R", requestType);
            return PartialView("_partialPageProfileUpdateReview", lstRequests);
        }
        [AuthOp]
        public ActionResult ReviewAddUser()
        {
            List<tbl_Users> lst = new List<tbl_Users>();
            UserDataAccess uda = new UserDataAccess();
            lst = uda.GetUsers("R");
            return View(lst);
        }
    }
}