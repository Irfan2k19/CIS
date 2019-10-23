using CardIssuanceSystem.Core.Helpers;
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
    public class ReviewController : BaseController
    {
        // GET: Review

        public ActionResult Index()
        {
            return View();
        }

        [AuthOp(RoleTitle = new string[] { "c" })]

        public ActionResult ReviewCardIssuanceScreen()
        {
            List<AuthorizationVM> viewModel = ReviewMethods.GetRequestDetails("N");
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult ReviewCardReplacementScreen()
        {
            List<AuthorizationVM> viewModel = ReviewMethods.GetRequestDetails("R");
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult ReviewCardAmendmentScreen()
        {
            List<AuthorizationVM> viewModel = ReviewMethods.GetRequestDetails("A");
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult ReviewCardUpgradeScreen()
        {
            List<AuthorizationVM> viewModel = ReviewMethods.GetRequestDetails("U");
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult ReviewLinkingScreen()
        {
            List<AuthorizationVM> viewModel = ReviewMethods.GetRequestDetails("L");
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult ReviewDelinkingScreen()
        {
            List<AuthorizationVM> viewModel = ReviewMethods.GetRequestDetails("D");
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult FilterAuthorizeScreen(FilterAuthorizationVM request)
        {
            List<AuthorizationVM> viewModel = ReviewMethods.FilterRequestDetails(request);
            return PartialView("_partialReviewAuthorizeView", viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult ReviewAccountTypeScreen()
        {
            List<AccountTypeVM> viewModel = ReviewMethods.GetAccountTypeRequestDetails("R");
            ViewBag.AccountTypes = new AccountTypeDataAccess().GetAccountTypes("A");//AccountTypeMethods.GetAllAccountTypes();
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult FilterAuthorizeAccountTypeScreen(FilterAccountTypeVM request, string RequestType)
        {
            if (RequestType == "Add")
            {
                List<AccountTypeVM> viewModel = ReviewMethods.FilterAccountTypeRequestDetails(request, "R");
                return PartialView("_partialReviewAuthorizeAccountTypeAddView", viewModel);
            }
            else {
                //fetch from system requests
                List<tbl_System_Requests> lst = new SystemRequestDataAccess().GetAllSystemRequests("R", Constants.RequestTypes.AccountTypes_SystemRequest);

                List<AccountTypeVM> lstData = new List<AccountTypeVM>();
                for (int i = 0; i < lst.Count; i++)
                {
                    AccountTypeVM item = new AccountTypeVM();
                    item = CustomHelper.ParseJson<AccountTypeVM>(lst[i].UpdatedData);
                    item.Code = lst[i].ID.ToString();//setting code as ID so it could be used at hyperlink
                    item.AuthorizationComments = lst[i].AuthorizationComments;
                    lstData.Add(item);
                }

                return PartialView("_partialReviewAuthorizeAccountTypeUpdateView", lstData);
            }
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult ReviewCardChargesScreen()
        {
            List<CardChargesVM> viewModel = ReviewMethods.GetCardChargesRequestDetails("R");
            ViewBag.AccountTypes = new AccountTypeDataAccess().GetAccountTypes("A");//AccountTypeMethods.GetAllAccountTypes();
            ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypes("A");//CardTypeMethods.GetAllCardTypes();
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult FilterAuthorizeCardChargesScreen(FilterAccountTypeVM request, string RequestType)
        {
            if (RequestType == "Add")
            {
                List<CardChargesVM> viewModel = ReviewMethods.FilterCardChargesRequestDetails(request, "R");
                return PartialView("_partialReviewAuthorizeCardChargesAddView", viewModel);
            }
            else
            {
                //fetch from system requests
                List<tbl_System_Requests> lst = new SystemRequestDataAccess().GetAllSystemRequests("R", Constants.RequestTypes.CardCharges_SystemRequest);

                List<CardChargesVM> lstData = new List<CardChargesVM>();
                for (int i = 0; i < lst.Count; i++)
                {
                    CardChargesVM item = new CardChargesVM();
                    item = CustomHelper.ParseJson<CardChargesVM>(lst[i].UpdatedData);
                    var cardChargesData = new CardChargesDataAccess().GetCardChargeDetailsById(item.ID);
                    item.Title = lst[i].ID.ToString();//setting title as ID so it could be used at hyperlink
                    item.AccountTypeName = cardChargesData.tbl_Account_Types.Name;
                    item.CardTypeName = cardChargesData.tbl_Card_Types.Title;
                    item.AuthorizationComments = lst[i].AuthorizationComments;
                    lstData.Add(item);
                }

                return PartialView("_partialReviewAuthorizeCardChargesUpdateView", lstData);
            }

        }
        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult ReviewCardTypeScreen()
        {
            List<CardTypeVM> viewModel = ReviewMethods.GetCardTypeRequestDetails("R");
            ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypes("A");//CardTypeMethods.GetAllCardTypes();
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult FilterAuthorizeCardTypeScreen(FilterAccountTypeVM request, string RequestType)
        {
            if (RequestType == "Add")
            {
                List<CardTypeVM> viewModel = ReviewMethods.FilterCardTypeRequestDetails(request, "R");
                return PartialView("_partialReviewAuthorizeCardTypeAddView", viewModel);
            }
            else
            {
                //fetch from system requests
                List<tbl_System_Requests> lst = new SystemRequestDataAccess().GetAllSystemRequests("R", Constants.RequestTypes.CardTypes_SystemRequest);

                List<CardTypeVM> lstData = new List<CardTypeVM>();
                for (int i = 0; i < lst.Count; i++)
                {
                    CardTypeVM item = new CardTypeVM();
                    item = CustomHelper.ParseJson<CardTypeVM>(lst[i].UpdatedData);
                    item.CardCode = lst[i].ID.ToString();//setting cardcode as ID so it could be used at hyperlink
                    item.AuthorizationComments = lst[i].AuthorizationComments;
                    lstData.Add(item);
                }

                return PartialView("_partialReviewAuthorizeCardTypeUpdateView", lstData);
            }
        }
    }
}