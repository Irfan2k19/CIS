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
    public class CardTypeController : BaseController
    {
        [AuthOp(RoleTitle = new string[] { "a" })]
        // GET: CardType
        public ActionResult Index()
        {
            return View();
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult AddUpdateCardType(int? Id)
        {
            CardTypeVM viewModel = new CardTypeVM();
            ViewBag.RequestId = Id ?? 0;

            if (Id.HasValue)
            {
                viewModel = CardTypeMethods.GetCardTypeById(Id.GetValueOrDefault(), "R");
                ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypesActiveInactive("A");
                if (Id.GetValueOrDefault() > 0 && viewModel == null)
                    return RedirectToAction("ReviewCardTypeScreen", "Review");
            }
            else
                ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypesActiveInactive("A");

            ViewBag.RequestType = "Add";
            return View(viewModel);
        }
        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult AddUpdateCardTypeForUpdateRequest(int? Id)
        {
            CardTypeVM viewModel = new CardTypeVM();
            ViewBag.RequestId = Id ?? 0;

            if (Id.HasValue)
            {
                var getSystemRequest = new SystemRequestDataAccess().GetSystemRequest(Id.GetValueOrDefault(), "R");
                viewModel = getSystemRequest == null ? null : CustomHelper.ParseJson<CardTypeVM>(getSystemRequest.UpdatedData);
                ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypesActiveInactive("A");
                if (Id.GetValueOrDefault() > 0 && viewModel == null)
                    return RedirectToAction("ReviewCardTypeScreen", "Review");
            }
            else
                ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypesActiveInactive("A");

            ViewBag.RequestType = "Update";
            return View("AddUpdateCardType", viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        [HttpPost]
        public ActionResult AddUpdateCardType(tbl_Card_Types request, List<tbl_Card_Types> upgradeTypes, int? RequestId = null)
        {
            if (RequestId.HasValue)
            {
                var getSystemRequest = new SystemRequestDataAccess().GetSystemRequest(RequestId.GetValueOrDefault(), "R");
                var CardTypeDetails = CardTypeMethods.GetCardTypeById(request.ID);
                CardTypeDetails.tbl_Card_Upgrade_Types1 = new List<tbl_Card_Upgrade_Types>();
                CardTypeDetails.tbl_Card_Charges = new List<tbl_Card_Charges>();
                var existingJson = CardTypeDetails == null ? null : CustomHelper.GetJson(CardTypeDetails);

                upgradeTypes.ForEach(e => request.tbl_Card_Upgrade_Types.Add(new tbl_Card_Upgrade_Types { ParentCardType = e.ID, ChildCardType = request.ID }));

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
                        RequestType = Constants.RequestTypes.CardTypes_SystemRequest
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
                upgradeTypes = upgradeTypes ?? new List<tbl_Card_Types>();
                //New Card Type
                var response = request.ID <= 0 ? new CardTypesDataAccess().AddCardType(request, upgradeTypes) : new CardTypesDataAccess().UpdateCardType(request, upgradeTypes);

                return Json(new { IsSuccess = response, ErrorMessage = response == true ? string.Empty : CustomMessages.GenericErrorMessage, Response = response }, JsonRequestBehavior.AllowGet);
            }
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult GetCardTypeDetail(int Id, string Status = default(string))
        {
            var CardTypes = CardTypeMethods.GetCardTypeById(Id, Status);
            return Json(new { IsSuccess = CardTypes == null ? false : true, ErrorMessage = CardTypes == null ? CustomMessages.GenericErrorMessage : string.Empty, Response = CardTypes }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult Authorize()
        {
            List<CardTypeVM> viewModel = ReviewMethods.GetCardTypeRequestDetails("P");
            ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypesActiveInactive("A"); //CardTypeMethods.GetAllCardTypes();
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult FilterAuthorize(FilterAccountTypeVM request, string RequestType)
        {
            if (RequestType == "Add")
            {
                List<CardTypeVM> viewModel = ReviewMethods.FilterCardTypeRequestDetails(request, "P");
                return PartialView("_partialAuthorizeCardTypeAddView", viewModel);
            }
            else
            {
                //fetch from system requests
                List<tbl_System_Requests> lst = new SystemRequestDataAccess().GetAllSystemRequests("P", Constants.RequestTypes.CardTypes_SystemRequest);

                List<CardTypeVM> lst2 = new List<CardTypeVM>();
                for (int i = 0; i < lst.Count; i++)
                {
                    CardTypeVM item = new CardTypeVM();
                    item = CustomHelper.ParseJson<CardTypeVM>(lst[i].UpdatedData);
                    item.CardCode = lst[i].ID.ToString();//setting code as ID so it could be used at hyperlink
                    lst2.Add(item);
                }

                return PartialView("_partialAuthorizeCardTypeUpdateView", lst2);
            }

        }
    }
}