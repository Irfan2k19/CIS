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
    public class SystemAuthorizationController : BaseController
    {
        [AuthOp]
        public ActionResult AuthorizeUser()
        {
            var viewModel = new List<SystemRequestVM>();
            return View(viewModel);
        }

        [AuthOp]
        public ActionResult GetUserAddRequests(string requestType)
        {
            var lstRequests = SystemRequestMethods.GetUserDetails("P", requestType);
            return PartialView("_partialUserAuthorizeAddView", lstRequests);
        }
        [AuthOp]
        public ActionResult GetUserUpdateRequests(string requestType)
        {
            var lstRequests = SystemRequestMethods.GetSystemRequestForUser("P", requestType);
            return PartialView("_partialUserAuthorizeUpdateView", lstRequests);
        }
        [AuthOp]
        public ActionResult AuthorizeNewUser(int? Id)
        {
            var viewModel = new UserDataAccess().GetUserById(Id.GetValueOrDefault());
            if (viewModel == null || viewModel.AuthorizationStatus != "P")
                return RedirectToAction("AuthorizeUser");

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [AuthOp]
        public ActionResult AuthorizeNewUserRequest(tbl_Users request)
        {
            var user = new UserDataAccess().GetUserById(request.ID);
            var log = new UserLogDataAccess().GetUserLogByEntityId(request.ID, "tbl_Users");
            if (log.FirstOrDefault()?.ActionUserStamp == StateHelper.UserId)
            {
                return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.SameCreatorAndsAuthorizer }, JsonRequestBehavior.AllowGet);
            }

            user.AuthorizationStatus = request.AuthorizationStatus;
            user.AuthorizationComments = request.AuthorizationComments;
            var success = new UserDataAccess().UpdateUser(user);
            return Json(new { IsSuccess = success, ErrorMessage = (!success) ? CustomMessages.GenericErrorMessage : CustomMessages.AuthorizedSucessfully, Response = success }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp]
        public ActionResult AuthorizeExistingUser(int? Id)
        {
            var viewModel = new SystemRequestDataAccess().GetSystemRequest(Id.GetValueOrDefault());
            ViewBag.UserDetails = CustomHelper.ParseJson<UserVM>(viewModel.UpdatedData);
            if (viewModel.AuthorizationStatus != "P")
                return RedirectToAction("AuthorizeUser");

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [AuthOp]
        public ActionResult AuthorizeExistingUserRequest(tbl_System_Requests request)
        {
            var systemRequest = new SystemRequestDataAccess().GetSystemRequest(request.ID);
            if (systemRequest.CreatorID == StateHelper.UserId)
            {
                return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.SameCreatorAndsAuthorizer }, JsonRequestBehavior.AllowGet);
            }
            if (request.AuthorizationStatus == "A")
            {
                var data = CustomHelper.ParseJson<UserVM>(systemRequest.UpdatedData);
                var user = new tbl_Users
                {
                    ID = data.ID,
                    IsActive = data.IsActive,
                    RoleTitle = data.RoleTitle,
                    UserLogin = data.UserLogin,
                    UserName = data.UserName,
                    UserPassword = data.UserPassword,
                    AuthorizationStatus = data.AuthorizationStatus,
                    AuthorizationComments = data.AuthorizationComments,
                    ExpiryDate=data.ExpiryDate,
                    EmpCode=data.EmpCode
                };

                new UserDataAccess().UpdateUser(user);
            }

            systemRequest.AuthorizationStatus = request.AuthorizationStatus;
            systemRequest.AuthorizationComments = request.AuthorizationComments;
            systemRequest.AuthorizerID = StateHelper.UserId;

            var success = new SystemRequestDataAccess().UpdateSystemRequest(systemRequest);
            return Json(new { IsSuccess = success, ErrorMessage = (!success) ? CustomMessages.GenericErrorMessage : CustomMessages.AuthorizedSucessfully, Response = success }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp]
        public ActionResult AuthorizeUserProfile(int? Id)
        {
            var viewModel = new SystemRequestDataAccess().GetSystemRequest(Id.GetValueOrDefault(), "P");
            if (viewModel == null)
                return RedirectToAction("AuthorizeUser", "SystemAuthorization");

            var lstData = CustomHelper.ParseJson<List<ProfileUserVM>>(viewModel.UpdatedData);
            ViewBag.UserPermissions = lstData;
            ViewBag.UserId = new UserDataAccess().GetUserById(lstData.FirstOrDefault().UserID.GetValueOrDefault());

            return View(viewModel);
        }

        [AuthOp]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AuthorizeUserProfileRequest(tbl_System_Requests request)
        {
            var systemRequest = new SystemRequestDataAccess().GetSystemRequest(request.ID);
            if (systemRequest.CreatorID == StateHelper.UserId) {
                return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.SameCreatorAndsAuthorizer }, JsonRequestBehavior.AllowGet);
            }
            if (request.AuthorizationStatus == "A")
            {
                var lstData = CustomHelper.ParseJson<List<ProfileUserVM>>(systemRequest.UpdatedData);
                var userProfile = lstData.Select(e => new tbl_User_Profile
                {
                    ProfileID = e.ProfileID,
                    UserID = e.UserID
                }).ToList();

                ProfileMethods.AddProfileUsers(userProfile);
            }

            systemRequest.AuthorizationStatus = request.AuthorizationStatus;
            systemRequest.AuthorizationComments = request.AuthorizationComments;
            systemRequest.AuthorizerID = StateHelper.UserId;

            var success = new SystemRequestDataAccess().UpdateSystemRequest(systemRequest);
            return Json(new { IsSuccess = success, ErrorMessage = (!success) ? CustomMessages.GenericErrorMessage : CustomMessages.AuthorizedSucessfully, Response = success }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp]
        public ActionResult AuthorizeProfile()
        {
            var viewModel = new List<SystemRequestVM>();
            return View(viewModel);
        }

        [AuthOp]
        public ActionResult GetProfileAddRequests(string requestType)
        {
            var lstRequests = SystemRequestMethods.GetProfileDetails("P", requestType);
            return PartialView("_partialProfileAuthorizeAddView", lstRequests);
        }

        [AuthOp]
        public ActionResult GetProfileUpdateRequests(string requestType)
        {
            var lstRequests = SystemRequestMethods.GetSystemRequestForProfile("P", requestType);
            return PartialView("_partialProfileAuthorizeUpdateView", lstRequests);
        }
        [AuthOp]
        public ActionResult AuthorizeNewProfile(int? Id)
        {
            var viewModel = new ProfileDataAccess().GetProfileById(Id.GetValueOrDefault());
            if (viewModel.AuthorizationStatus != "P")
                return RedirectToAction("AuthorizeProfile");

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [AuthOp]
        public ActionResult AuthorizeNewProfileRequest(tbl_Profile request)
        {
            var profile = new ProfileDataAccess().GetProfileById(request.ID);
            var log = new UserLogDataAccess().GetUserLogByEntityId(request.ID, "tbl_Profile");
            if (log.FirstOrDefault()?.ActionUserStamp == StateHelper.UserId)
            {
                return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.SameCreatorAndsAuthorizer }, JsonRequestBehavior.AllowGet);
            }

            profile.AuthorizationStatus = request.AuthorizationStatus;
            profile.AuthorizationComments = request.AuthorizationComments;
            var success = new ProfileDataAccess().UpdateProfile(profile);
            return Json(new { IsSuccess = success, ErrorMessage = (!success) ? CustomMessages.GenericErrorMessage : CustomMessages.AuthorizedSucessfully, Response = success }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp]
        public ActionResult AuthorizeExistingProfile(int? Id)
        { 
            var viewModel = new SystemRequestDataAccess().GetSystemRequest(Id.GetValueOrDefault(), "P");
            if (viewModel == null)
                return RedirectToAction("AuthorizeProfile", "SystemAuthorization");

            ViewBag.ProfileData = viewModel.UpdatedData == null ? null : CustomHelper.ParseJson<tbl_Profile>(viewModel.UpdatedData);

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [AuthOp]
        public ActionResult AuthorizeExistingProfileRequest(tbl_System_Requests request)
        {
            var systemRequest = new SystemRequestDataAccess().GetSystemRequest(request.ID);
            if (systemRequest.CreatorID == StateHelper.UserId)
            {
                return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.SameCreatorAndsAuthorizer }, JsonRequestBehavior.AllowGet);
            }
            if (request.AuthorizationStatus == "A")
            {
                var lstData = CustomHelper.ParseJson<tbl_Profile>(systemRequest.UpdatedData);
                new ProfileDataAccess().UpdateProfile(lstData);
            }

            systemRequest.AuthorizationStatus = request.AuthorizationStatus;
            systemRequest.AuthorizationComments = request.AuthorizationComments;
            systemRequest.AuthorizerID = StateHelper.UserId;

            var success = new SystemRequestDataAccess().UpdateSystemRequest(systemRequest);
            return Json(new { IsSuccess = success, ErrorMessage = (!success) ? CustomMessages.GenericErrorMessage : CustomMessages.AuthorizedSucessfully, Response = success }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp]
        public ActionResult AuthorizeProfilePage(int? Id)
        {
            var viewModel = new SystemRequestDataAccess().GetSystemRequest(Id.GetValueOrDefault(), "P");
            if (viewModel == null)
                return RedirectToAction("AuthorizeProfile", "SystemAuthorization");

            var lstData = CustomHelper.ParseJson<List<ProfilePageVM>>(viewModel.UpdatedData);
            ViewBag.ProfilePermissions = lstData;
            ViewBag.ProfileId = new UserDataAccess().GetUserById(lstData.FirstOrDefault().ProfileID.GetValueOrDefault());

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [AuthOp]
        public ActionResult AuthorizeProfilePageRequest(tbl_System_Requests request)
        {
            var systemRequest = new SystemRequestDataAccess().GetSystemRequest(request.ID);
            if (systemRequest.CreatorID == StateHelper.UserId)
            {
                return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.SameCreatorAndsAuthorizer }, JsonRequestBehavior.AllowGet);
            }
            if (request.AuthorizationStatus == "A")
            {
                var lstData = CustomHelper.ParseJson<List<ProfilePageVM>>(systemRequest.UpdatedData);
                var profilePages = lstData.Select(e => new tbl_Profile_Page
                {
                    ProfileID = e.ProfileID,
                    PageID = e.PageID
                }).ToList();

                ProfileMethods.AddProfilePages(profilePages);
            }

            systemRequest.AuthorizationStatus = request.AuthorizationStatus;
            systemRequest.AuthorizationComments = request.AuthorizationComments;
            systemRequest.AuthorizerID = StateHelper.UserId;

            var success = new SystemRequestDataAccess().UpdateSystemRequest(systemRequest);
            return Json(new { IsSuccess = success, ErrorMessage = (!success) ? CustomMessages.GenericErrorMessage : CustomMessages.AuthorizedSucessfully, Response = success }, JsonRequestBehavior.AllowGet);
        }
    }
}