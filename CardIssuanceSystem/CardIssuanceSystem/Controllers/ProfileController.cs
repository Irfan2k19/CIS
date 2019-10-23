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
    public class ProfileController : BaseController
    {
        [AuthOp]
        // GET: Profile [Use to add profile]
        public ActionResult Index(int? Id)
        {
            var viewModel = new tbl_Profile();
            if (Id.HasValue)
            {
                viewModel = new ProfileDataAccess().GetProfileById(Id.GetValueOrDefault());
                if (viewModel.AuthorizationStatus != "R")
                {
                    return RedirectToAction("DashboardView", "Base");
                }
            }

            return View(viewModel);
        }

        [AuthOp]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddUpdateProfile(tbl_Profile request)
        {
            var profile = new tbl_Profile
            {
                ID = request.ID,
                Title = request.Title,
                IsActive = request.IsActive,
                AuthorizationStatus = "P"
            };

            var success = request.ID > 0 ? new ProfileDataAccess().UpdateProfile(profile) : new ProfileDataAccess().AddProfile(profile);
            return Json(new {IsSuccess = success }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp]
        // GET: Update Profile [Use to edit/update profile]
        public ActionResult UpdateProfile(int? Id)
        {
            var systemRequest = new tbl_System_Requests();
            if (Id.HasValue)
            {
                systemRequest = new SystemRequestDataAccess().GetSystemRequest(Id.GetValueOrDefault(), "R");
                if (systemRequest == null)
                {
                    return RedirectToAction("DashboardView", "Base");
                }

                ViewBag.ProfileDetails = CustomHelper.ParseJson<tbl_Profile>(systemRequest.UpdatedData);
            }

            ViewBag.SystemRequest = systemRequest;
            var viewModel = new ProfileDataAccess().GetAllProfiles("A");
            return View(viewModel);
        }

        [AuthOp]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddProfileModificationRequest(tbl_Profile request, int? RequestId = null)
        {
            var getProfile = new ProfileDataAccess().GetProfileById(request.ID);
            var getSystemRequest = new SystemRequestDataAccess().GetSystemRequest(RequestId.GetValueOrDefault());

            request.AuthorizationStatus = getProfile.AuthorizationStatus;
            request.AuthorizationComments = getProfile.AuthorizationComments;

            var existingJson = getProfile == null ? null : CustomHelper.GetJson(getProfile);
            var updatedJson = CustomHelper.GetJson(request);

            var row = new tbl_System_Requests
            {
                ID = RequestId.GetValueOrDefault(),
                AuthorizationStatus = "P",
                CreatorID = StateHelper.UserId,
                IsActive = true,
                ExistingData = existingJson,
                UpdatedData = updatedJson,
                RequestType = Constants.RequestTypes.ProfileModification,
                AuthorizationComments = getSystemRequest?.AuthorizationComments,
                AuthorizerID = getSystemRequest?.AuthorizerID
            };

            var success = (!RequestId.HasValue || RequestId.GetValueOrDefault() <= 0) ? new SystemRequestDataAccess().AddSystemRequest(row) : new SystemRequestDataAccess().UpdateSystemRequest(row);
            return Json(new { IsSuccess = success }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp]
        // GET: User Profile [Use to assign profile to user]
        public ActionResult UserProfile(int? Id)
        {
            var viewModel = new ProfileDataAccess().GetAllProfiles("A");
            if (Id.HasValue)
            {
                var getSystemRequest = new SystemRequestDataAccess().GetSystemRequest(Id.GetValueOrDefault(), "R");
                if (getSystemRequest == null)
                {
                    return RedirectToAction("UserProfileReview", "SystemReview");
                }

                ViewBag.SystemRequest = new tbl_System_Requests { ID = Id.GetValueOrDefault() };//getSystemRequest;
                var profileUser = CustomHelper.ParseJson<List<ProfileUserVM>>(getSystemRequest.UpdatedData);
                var userData = new UserDataAccess().GetUserById(profileUser.FirstOrDefault()?.UserID ?? 0);
                
                ViewBag.ProfileUser = profileUser;
                ViewBag.UserID = userData.UserLogin;
               
            }
            var users = new UserDataAccess().GetUsers("A");
            ViewBag.Users = users;
            return View(viewModel);
        }

        [AuthOp]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddUpdateUserProfile(List<ProfileUserVM> request, int? RequestId = null)
        {
            var getUserProfile = new ProfileDataAccess().GetUserProfiles(request.FirstOrDefault() == null ? 0 : request.FirstOrDefault().UserID.GetValueOrDefault());
            var getSystemRequest = new SystemRequestDataAccess().GetSystemRequest(RequestId.GetValueOrDefault());

            var existingJson = getUserProfile.Count <= 0 ? null : CustomHelper.GetJson(getUserProfile);

            var updatedJson = CustomHelper.GetJson(request);
            var row = new tbl_System_Requests
            {
                ID = RequestId.GetValueOrDefault(),
                AuthorizationStatus = "P",
                CreatorID = StateHelper.UserId,
                IsActive = true,
                ExistingData = existingJson,
                UpdatedData = updatedJson,
                RequestType = Constants.RequestTypes.UserProfile,
                AuthorizationComments = getSystemRequest?.AuthorizationComments ?? string.Empty,
                AuthorizerID = getSystemRequest?.AuthorizerID ?? 0
            };

            var success = (!RequestId.HasValue || RequestId.GetValueOrDefault() <= 0) ? new SystemRequestDataAccess().AddSystemRequest(row) : new SystemRequestDataAccess().UpdateSystemRequest(row);
            return Json(new { IsSuccess = success }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GetUserByUserId(string UserId)
        {
            var data = new UserDataAccess().GetUserByUserID(UserId);
            var userProfiles = ProfileMethods.GetProfileUsers(data.ID).Select(e => e.ProfileID);
            var response = new UserVM
            {
                ID = data?.ID ?? 0,
                UserName = data?.UserName
            };

            return Json(new { IsSuccess = (data == null) ? false : true, Response = new { ProfileData = userProfiles, UserData = response}, ErrorMessage = (data == null) ? CustomMessages.UserNotExists : string.Empty }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp]
        // GET: Profile Pages [Use to assign pages to profile]
        public ActionResult ProfilePages(int? Id)
        {
            var viewModel = ProfileMethods.GetAllPages();
            if (Id.HasValue)
            {
                var getSystemRequest = new SystemRequestDataAccess().GetSystemRequest(Id.GetValueOrDefault(), "R");
                if (getSystemRequest == null)
                {
                    return RedirectToAction("PageProfileReview", "SystemReview");
                }

                ViewBag.SystemRequest = new tbl_System_Requests { ID = Id.GetValueOrDefault() };//getSystemRequest;
                var pageProfile = CustomHelper.ParseJson<List<ProfilePageVM>>(getSystemRequest.UpdatedData);

                ViewBag.PageProfile = pageProfile;
            }

            ViewBag.Profiles = new ProfileDataAccess().GetAllProfiles("A");
            return View(viewModel);
        }

        [AuthOp]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddUpdateProfilePages(List<ProfilePageVM> request, int? RequestId = null)
        {
            if (request == null)
            {
                return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.SomethingWentWrong }, JsonRequestBehavior.AllowGet);
            }

            var getSystemRequest = new SystemRequestDataAccess().GetSystemRequest(RequestId.GetValueOrDefault());

            var getProfilePages = new ProfileDataAccess().GetProfilePage(request.FirstOrDefault() != null ? request.FirstOrDefault().ProfileID.GetValueOrDefault() : 0);

            var existingJson = getProfilePages.Count <= 0 ? null : CustomHelper.GetJson(getProfilePages);

            var updatedJson = CustomHelper.GetJson(request);
            var row = new tbl_System_Requests
            {
                ID = RequestId.GetValueOrDefault(),
                AuthorizationStatus = "P",
                CreatorID = StateHelper.UserId,
                IsActive = true,
                ExistingData = existingJson,
                UpdatedData = updatedJson,
                RequestType = Constants.RequestTypes.ProfilePage,
                AuthorizationComments = getSystemRequest?.AuthorizationComments ?? string.Empty,
                AuthorizerID = getSystemRequest?.AuthorizerID ?? 0
            };

            var success = (!RequestId.HasValue || RequestId.GetValueOrDefault() <= 0) ? new SystemRequestDataAccess().AddSystemRequest(row): new SystemRequestDataAccess().UpdateSystemRequest(row);
            return Json(new { IsSuccess = success }, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GetPageProfileDetails(int? ProfileId)
        {
            var data = ProfileMethods.GetAllProfilePages(ProfileId.GetValueOrDefault());
            var pageProfiles = data.Select(e => new { PageID= e.ID, SectionID = e.SectionID });

            return Json(new { IsSuccess = (pageProfiles == null) ? false : true, Response = new { ProfilePagesData = pageProfiles}, ErrorMessage = (pageProfiles == null) ? CustomMessages.NoDataFound : string.Empty }, JsonRequestBehavior.AllowGet);
        }
    }
}