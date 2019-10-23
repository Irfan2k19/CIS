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
    public class CardChargesController : BaseController
    {
        [AuthOp(RoleTitle = new string[] { "a" })]
        // GET: CardCharges
        public ActionResult Index()
        {
            return View();
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult AddUpdateCardCharges(int? Id)
        {
            CardChargesVM viewModel = new CardChargesVM();
            ViewBag.RequestId = Id ?? 0;

            ViewBag.AccountTypes = new AccountTypeDataAccess().GetAccountTypes("A");
            ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewBag.CardCharges = new CardChargesDataAccess().GetCardChargesActiveInactive("A");

            //Modifications to be handled through system requests
            if (Id.HasValue)
            {
                viewModel = CardChargesMethod.GetCardChargeDetail(Id.GetValueOrDefault(), "R");
                if (Id > 0 && viewModel == null)
                    return RedirectToAction("ReviewCardChargesScreen", "Review");
            }

            ViewBag.Regions = new RegionDataAccess().GetAllRegions();
            ViewBag.RequestType = "Add";
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult AddUpdateCardChargesForUpdateRequest(int? Id)
        {
            CardChargesVM viewModel = new CardChargesVM();
            ViewBag.RequestId = Id ?? 0;

            ViewBag.AccountTypes = new AccountTypeDataAccess().GetAccountTypes("A");
            ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewBag.CardCharges = new CardChargesDataAccess().GetCardChargesActiveInactive("A");

            if (Id.HasValue)
            {
                var getSystemRequest = new SystemRequestDataAccess().GetSystemRequest(Id.GetValueOrDefault(), "R");
                viewModel = getSystemRequest == null ? null : CustomHelper.ParseJson<CardChargesVM>(getSystemRequest.UpdatedData);
                if (Id.GetValueOrDefault() > 0 && viewModel == null)
                    return RedirectToAction("ReviewCardChargesScreen", "Review");
            }

            ViewBag.Regions = new RegionDataAccess().GetAllRegions();
            ViewBag.RequestType = "Update";
            return View("AddUpdateCardCharges", viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        [HttpPost]
        public ActionResult AddUpdateCardCharges(tbl_Card_Charges request, List<tbl_Regional_Charges> regionRequest = default(List<tbl_Regional_Charges>), int? RequestId = null)
        {
            //existing account type modification requests to be handled through System Request Section
            if (RequestId.HasValue)
            {
                var getSystemRequest = new SystemRequestDataAccess().GetSystemRequest(RequestId.GetValueOrDefault(), "R");
                var CardChargesDetails = CardChargesMethod.GetCardChargeDetail(request.ID);
                var existingJson = CardChargesDetails == null ? null : CustomHelper.GetJson(CardChargesDetails);
                var updatedJson = CustomHelper.GetJson(request);
                tbl_System_Requests row = new tbl_System_Requests();
                if (RequestId.GetValueOrDefault() <= 0)
                {
                    row = new tbl_System_Requests
                    {
                        AuthorizationStatus = "P",
                        CreatorID = StateHelper.UserId,
                        IsActive = true,
                        ExistingData = existingJson,
                        UpdatedData = updatedJson,
                        RequestType = Constants.RequestTypes.CardCharges_SystemRequest
                    };
                }
                else
                {
                    row = getSystemRequest;
                    row.CreatorID = StateHelper.UserId;
                    row.AuthorizationStatus = "P";
                    row.AuthorizationComments = string.Empty;
                    row.UpdatedData = updatedJson;
                    row.ExistingData = existingJson;
                }

                var SystemRequestFlag = (RequestId.HasValue && RequestId.GetValueOrDefault() <= 0) ? new SystemRequestDataAccess().AddSystemRequest(row) : new SystemRequestDataAccess().UpdateSystemRequest(row);

                return Json(new { IsSuccess = SystemRequestFlag, ErrorMessage = SystemRequestFlag == true ? string.Empty : CustomMessages.GenericErrorMessage, Response = SystemRequestFlag }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                request.IsActive = true;
                var response = request.ID <= 0 ? new CardChargesDataAccess().AddCardCharges(request) : new CardChargesDataAccess().UpdateCardCharges(request);
                return Json(new { IsSuccess = response, ErrorMessage = response == true ? string.Empty : CustomMessages.GenericErrorMessage, Response = response }, JsonRequestBehavior.AllowGet);
            }
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult GetCardChargeByCardTypeAccount(int CardTypeID, int AccountTypeID)
        {
            var response = CardChargesMethod.GetCardChargeByCardTypeAccount(CardTypeID, AccountTypeID);
            return Json(new { IsSuccess = response == null ? false : true, Response = response }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult GetCardChargeDetail(int ID)
        {
            var response = CardChargesMethod.GetCardChargeDetail(ID);
            return Json(new { IsSuccess = response == null ? false : true, Response = response }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult GetRegionalChargesDetail(int cardChargeID, int regionID)
        {
            var response = CardChargesMethod.GetRegionalChargesDetail(cardChargeID, regionID);
            return Json(new { IsSuccess = response == null ? false : true, Response = response }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult Authorize()
        {
            List<CardChargesVM> viewModel = ReviewMethods.GetCardChargesRequestDetails("P");
            ViewBag.AccountTypes = new AccountTypeDataAccess().GetAccountTypes("A");//AccountTypeMethods.GetAllAccountTypes();
            ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypes("A"); //CardTypeMethods.GetAllCardTypes();
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult FilterAuthorize(FilterAccountTypeVM request, string RequestType)
        {
            if (RequestType == "Add")
            {
                List<CardChargesVM> viewModel = ReviewMethods.FilterCardChargesRequestDetails(request, "P");
                return PartialView("_partialAuthorizeCardChargesAddView", viewModel);
            }
            else
            {
                //fetch from system requests
                List<tbl_System_Requests> lst = new SystemRequestDataAccess().GetAllSystemRequests("P", Constants.RequestTypes.CardCharges_SystemRequest);

                List<CardChargesVM> lst2 = new List<CardChargesVM>();
                for (int i = 0; i < lst.Count; i++)
                {
                    CardChargesVM item = new CardChargesVM();
                    item = CustomHelper.ParseJson<CardChargesVM>(lst[i].UpdatedData);
                    var cardChargesData = new CardChargesDataAccess().GetCardChargeDetailsById(item.ID);
                    item.Title = lst[i].ID.ToString();//setting code as ID so it could be used at hyperlink
                    item.AccountTypeName = cardChargesData.tbl_Account_Types.Name;
                    item.CardTypeName = cardChargesData.tbl_Card_Types.Title;
                    lst2.Add(item);
                }

                return PartialView("_partialAuthorizeCardChargesUpdateView", lst2);
            }
        }
    }
}