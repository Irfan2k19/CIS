using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CardIssuanceSystem.Core.Methods;
using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.DAL;
using CardIssuanceSystem.DAL.DataAccessClasses;
using CardIssuanceSystem.Filters;
using CardIssuanceSystem.Core.ViewModel;
using System.DirectoryServices.AccountManagement;

namespace CardIssuanceSystem.Controllers
{
    public class LoginController : BaseController
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Index2()
        {
            return View();
        }
        public ActionResult MainIndex()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string UserName, string Password)
        {
            //var pasassas = EncryptDecrypt.Encrypt("PA$$W0RD");
            bool flag = false;
            string action = string.Empty;
            
            if (new UserDataAccess().GetUserByUserID(UserName) != null/*LoginMethods.ValidateLogin(UserName)*/)
            {
                flag = LoginMethods.Login(UserName,Password); //DB login

               //flag = Auth(UserName, Password); //AD login 
                var userId = LoginMethods.IsUserExists(UserName);

                if (flag)
                {

                    bool IsDBExist = LoginMethods.Login2(UserName);//AD login
                    if (IsDBExist == false)
                    {
                        return Json(new { IsSuccess = flag, ErrorMessage = (flag == true) ? string.Empty : "User Not Exist in System", Response = (flag == true) ? action : null }, JsonRequestBehavior.AllowGet);
                    }

                    bool expiry=LoginMethods.CheckExpiry(UserName);
                    if (expiry==false)
                    {
                        return Json(new { IsSuccess = false, ErrorMessage = "User Access Expired", Response = "" }, JsonRequestBehavior.AllowGet);
                    }
                    LoginMethods.AddUserLog(userId.GetValueOrDefault(), userId.GetValueOrDefault());
                }
                
                else
                {
                    if (userId > 0)
                        LoginMethods.AddUserLog(userId.GetValueOrDefault());
                }

                action = Url.Action("DashboardView", "Base");

                
                return Json(new { IsSuccess = flag, ErrorMessage = (flag == true) ? string.Empty : CustomMessages.InvalidUsernameOrPassword, Response = (flag == true) ? action : null }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { IsSuccess = flag, ErrorMessage = (flag == true) ? string.Empty : CustomMessages.UserNotExists, Response = (flag == true) ? action : null }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult Register(int? Id)
        {
            var viewModel = new UserVM();
            if (Id.HasValue)
            {
                viewModel = UserMethods.GetUserDetailsByID(Id);
                if(viewModel.AuthorizationStatus != "R")
                {
                    return RedirectToAction("DashboardView", "Base");
                }
            }

            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        [HttpPost]
        public ActionResult Register(tbl_Users request)
        {
            request.UserPassword = "password";
            request.UserPassword = EncryptionHelper.HashString(request.UserPassword);
            request.IsActive = true;
            request.AuthorizationStatus = "P";
            var flag = request.ID > 0 ? new UserDataAccess().UpdateUser(request) : new UserDataAccess().Register(request);

            return Json(new { IsSuccess = flag, ErrorMessage = (flag == true) ? CustomMessages.Success : CustomMessages.GenericErrorMessage, Response = (flag == true) ? Url.Action("Index", "Login") : null }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Logout()
        {
            SessionLogout();
            return RedirectToAction("Index", "Login");
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult UpdateUserProfile(int? Id)
        {
            var viewModel = new tbl_System_Requests();
            var result = new UserDataAccess().GetAllUsersForAdmin();
            ViewBag.GetAllUsers = result ?? new List<tbl_Users>();
            if (Id.HasValue)
            {
                viewModel = new SystemRequestDataAccess().GetSystemRequest(Id.GetValueOrDefault(), "R");
                if (viewModel == null)
                {
                    return RedirectToAction("DashboardView", "Base");
                }

                ViewBag.UserDetails = CustomHelper.ParseJson<tbl_Users>(viewModel.UpdatedData);
            }
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult GetUser(int Id)
        {
            var result = new UserDataAccess().GetUserById(Id);
            if (result == null)
                return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.UserNotExists, Response = result }, JsonRequestBehavior.AllowGet);
            else
            {
                var user = new UserVM { ID = result.ID, UserName = result.UserName, UserLogin = result.UserLogin, RoleTitle = result.RoleTitle, IsActive = result.IsActive,ExpiryDateStr=result.ExpiryDate.ToString(),EmpCode=result.EmpCode };
                return Json(new { IsSuccess = true, ErrorMessage = string.Empty, Response = user }, JsonRequestBehavior.AllowGet);
            }
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult UpdateProfile(tbl_Users request, int? RequestId = null)
        {
            //var flag = new UserDataAccess().UpdateUser(request);
            var getUserDetails = UserMethods.GetUserDetailsByID(request.ID);
            var getSystemRequest = new SystemRequestDataAccess().GetSystemRequest(RequestId.GetValueOrDefault());

            request.UserPassword = getUserDetails.UserPassword;
            request.AuthorizationStatus = getUserDetails.AuthorizationStatus;
            request.AuthorizationComments = getUserDetails.AuthorizationComments;

            var existingJson = getUserDetails == null ? null : CustomHelper.GetJson(getUserDetails);

            var updatedJson = CustomHelper.GetJson(request);
            var row = new tbl_System_Requests
            {
                ID = RequestId ?? 0,
                AuthorizationStatus = "P",
                CreatorID = StateHelper.UserId,
                IsActive = true,
                ExistingData = existingJson,
                UpdatedData = updatedJson,
                RequestType = Constants.RequestTypes.UserModification,
                AuthorizerID = getSystemRequest?.AuthorizerID ?? 0,
                AuthorizationComments = getSystemRequest?.AuthorizationComments ?? string.Empty
            };

            var flag = (!RequestId.HasValue || RequestId.GetValueOrDefault() <= 0) ? new SystemRequestDataAccess().AddSystemRequest(row) : new SystemRequestDataAccess().UpdateSystemRequest(row);

            return Json(new { IsSuccess = flag, ErrorMessage = (flag == true) ? CustomMessages.UserProfileUpdateSuccessfully : CustomMessages.GenericErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        #region Change Password

        [AuthOp(RoleTitle = new string[] { "a","c","u" })]
        public ActionResult ChangePassword()
        {

            return View();
        }
        [AuthOp(RoleTitle = new string[] { "a", "c", "u" })]
        [HttpPost]
        public ActionResult ChangePassword(string oldpassword,string newpassword)
        {
            int id = StateHelper.UserId;
            UserDataAccess obj = new UserDataAccess();
            oldpassword = EncryptionHelper.HashString(oldpassword);
            newpassword= EncryptionHelper.HashString(newpassword);
            bool success;
            bool IsCorrectPassword = obj.CheckPassword(id,oldpassword);
            if (IsCorrectPassword==true)
            {
                success = obj.ChangePassword(id, newpassword);
                if (success)
                {
                    return Json(new { Result = success, Message = CustomMessages.PasswordChanged }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Result = success, Message = CustomMessages.PasswordNotChanged }, JsonRequestBehavior.AllowGet);
                }
               
            }
            else
            {
                success = false;
                return Json(new { Result = success, Message = CustomMessages.PasswordNotCorrect }, JsonRequestBehavior.AllowGet);
            }
            
        }
        #endregion

        /// <summary>
        /// Use to authenticate user from domain
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Pw"></param>
        /// <returns></returns>
        public bool Auth(string ID, string Pw)
        {
            try
            {
                ID = ID + "@soneribank.com";
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, "soneribank.com"))
                {
                    // validate the credentials
                    bool isValid = pc.ValidateCredentials(ID, Pw);
                    
                    return isValid;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}