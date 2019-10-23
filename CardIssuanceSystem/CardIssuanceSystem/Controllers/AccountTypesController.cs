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
    public class AccountTypesController : BaseController
    {
        [AuthOp(RoleTitle = new string[] { "a" })]
        // GET: AccountTypes
        public ActionResult Index()
        {
            LogHelper.PrintDebug("Hello");
            return RedirectToAction("Authorize");
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        // GET: AddUpdateAccountTypes
        public ActionResult AddUpdateAccountType(int? Id)
        {
            AccountTypeVM viewModel = new AccountTypeVM();
            ViewBag.RequestId = Id ?? 0;

            if (Id.HasValue)
            {
                viewModel = AccountTypeMethods.GetAccountTypeById(Id.GetValueOrDefault(), "R");
                ViewBag.AccountTypes = new AccountTypeDataAccess().GetAccountTypesActiveInActive("R");
                if (Id.GetValueOrDefault() > 0 && viewModel == null)
                    return RedirectToAction("ReviewAccountTypeScreen", "Review");
            }
            else
                ViewBag.AccountTypes = new AccountTypeDataAccess().GetAccountTypesActiveInActive("A");

            ViewBag.RequestType = "Add";
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        // GET: AddUpdateAccountTypes
        public ActionResult AddUpdateAccountTypeForUpdateRequest(int? Id)
        {
            AccountTypeVM viewModel = new AccountTypeVM();
            ViewBag.RequestId = Id ?? 0;

            if (Id.HasValue)
            {
                var getSystemRequest = new SystemRequestDataAccess().GetSystemRequest(Id.GetValueOrDefault(), "R");
                viewModel = getSystemRequest == null ? null : CustomHelper.ParseJson<AccountTypeVM>(getSystemRequest.UpdatedData);
                ViewBag.AccountTypes = new AccountTypeDataAccess().GetAccountTypesActiveInActive("A");
                if (Id.GetValueOrDefault() > 0 && viewModel == null)
                    return RedirectToAction("ReviewAccountTypeScreen", "Review");
            }
            else
                ViewBag.AccountTypes = new AccountTypeDataAccess().GetAccountTypesActiveInActive("A");

            ViewBag.RequestType = "Update";
            return View("AddUpdateAccountType", viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        // POST: AddUpdateAccountTypes
        [HttpPost]
        public ActionResult AddUpdateAccountType(tbl_Account_Types request, int? RequestId = null)
        {
            if (IsDuplicateCode(request.ID, request.Code))
                return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.DuplicateAccountCode }, JsonRequestBehavior.AllowGet);

            //existing account type modification requests to be handled through System Request Section
            if (RequestId.HasValue/*request.ID != 0*/)
            {
                var getSystemRequest = new SystemRequestDataAccess().GetSystemRequest(RequestId.GetValueOrDefault(), "R");
                var AccountTypeDetails = new AccountTypeDataAccess().GetAccountTypeDetailsById(request.ID);
                var existingJson = AccountTypeDetails == null ? null : CustomHelper.GetJson(AccountTypeDetails);

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
                        RequestType = Constants.RequestTypes.AccountTypes_SystemRequest
                    };
                }
                else {
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
                //New Account Type
                var response = request.ID <= 0 ? new AccountTypeDataAccess().AddAccountType(request) : new AccountTypeDataAccess().UpdateAccountType(request);
                return Json(new { IsSuccess = response, ErrorMessage = response == true ? string.Empty : CustomMessages.GenericErrorMessage, Response = response }, JsonRequestBehavior.AllowGet);
            }
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult GetAccountTypeDetail(int Id)
        {
            var AccountTypes = AccountTypeMethods.GetAccountTypeById(Id);
            return Json(new { IsSuccess = AccountTypes == null ? false : true, ErrorMessage = AccountTypes == null ? CustomMessages.GenericErrorMessage : string.Empty, Response = AccountTypes }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult Authorize()
        {
            List<AccountTypeVM> viewModel = ReviewMethods.GetAccountTypeRequestDetails("P");
            ViewBag.AccountTypes = new AccountTypeDataAccess().GetAccountTypesActiveInActive("A");//AccountTypeMethods.GetAllAccountTypes();
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult FilterAuthorize(FilterAccountTypeVM request,string RequestType)
        {
            if (RequestType == "Add")
            {
                List<AccountTypeVM> viewModel = ReviewMethods.FilterAccountTypeRequestDetails(request, "P");
                return PartialView("_partialAuthorizeAccountTypeAddView", viewModel);
            }
            else
            {
                //fetch from system requests
                List<tbl_System_Requests> lst = new SystemRequestDataAccess().GetAllSystemRequests("P", Constants.RequestTypes.AccountTypes_SystemRequest);
                
                List<AccountTypeVM> lst2 = new List<AccountTypeVM>();
                for (int i = 0; i < lst.Count; i++)
                {
                    AccountTypeVM item = new AccountTypeVM();
                    item = CustomHelper.ParseJson<AccountTypeVM>( lst[i].UpdatedData);
                    item.Code = lst[i].ID.ToString();//setting code as ID so it could be used at hyperlink
                    lst2.Add(item);
                }
                  
                return PartialView("_partialAuthorizeAccountTypeUpdateView", lst2);
            }

        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public bool IsDuplicateCode(int AccountTypeID1,string Code)
        {
            try
            { 
                int AccountTypeID2 = new AccountTypeDataAccess().GetAccountTypeID(Code);
                if (AccountTypeID2 > 0 && AccountTypeID1 != AccountTypeID2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return true;
            }
        }

    }
}