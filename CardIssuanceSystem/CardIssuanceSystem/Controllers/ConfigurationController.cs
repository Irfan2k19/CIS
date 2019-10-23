using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardIssuanceSystem.DAL.DataAccessClasses;
using CardIssuanceSystem.DAL;
using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.Filters;
using System.Configuration;
using System.Web.Configuration;

namespace CardIssuanceSystem.Controllers
{
    public class ConfigurationController : BaseController
    {
        #region BranchesScreen
        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult BranchesScreen()
        {
            return View();
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public JsonResult SearchBranchByCodeNo(string CodeNumber)
        {
            RegionDataAccess obj = new RegionDataAccess();
            List<ConfigurationVM> lst = new List<ConfigurationVM>();
            lst = GetRegionsbyID(CodeNumber);
            if (lst.Count > 0)
            {
                return Json(new { result = lst.FirstOrDefault(), ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }else
            {
                return Json(new { result = lst.FirstOrDefault(), ErrorMessage = CustomMessages.NoValueExist }, JsonRequestBehavior.AllowGet);
            }
            
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public List<ConfigurationVM> GetRegionsbyID(string Id)
        {
            try
            {
                List<ConfigurationVM> lst = new List<ConfigurationVM>();
                using (var db = new SoneriCISEntities())
                {
                    //lst = db.tbl_Region.Where(x=>x.Description==Id).Select(x => new  {  x.ID,  x.Title, x.Description }).FirstOrDefault();
                    lst = (from a in db.tbl_Region where a.Description == Id select new ConfigurationVM { ID = a.ID, Title = a.Title, Description = a.Description,FED=a.FED,IsActive=a.IsActive}).ToList();

                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public JsonResult AddBranch(string Description, string Tilte,string FED, bool? IsActive)
        {
            bool success;
            RegionDataAccess obj = new RegionDataAccess();
            tbl_Region lst = new tbl_Region();
            //ConfigurationVM lst = new ConfigurationVM();
            lst.Description = Description;
            lst.Title = Tilte;
            lst.FED = FED;
            lst.IsActive = IsActive;
            
            success =obj.AddRegion(lst);
            if (success==true)
            {
                return Json(new { result = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = success, ErrorMessage = CustomMessages.GenericErrorMessage }, JsonRequestBehavior.AllowGet);
            }

        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public JsonResult UpdateBranch(string Description, string Tilte, string FED, bool? IsActive)
        {
            bool success;
            RegionDataAccess obj = new RegionDataAccess();
            tbl_Region lst = new tbl_Region();
            //ConfigurationVM lst = new ConfigurationVM();
            lst.Description = Description;
            lst.Title = Tilte;
            lst.FED = FED;
            lst.IsActive = IsActive;
            success =obj.UpdateRegion(lst);
            if (success == true)
            {
                return Json(new { result = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = success, ErrorMessage = CustomMessages.GenericErrorMessage }, JsonRequestBehavior.AllowGet);
            }

        }

        #endregion

        #region Sector
        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult SectorScreen()
        {
            return View();
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public JsonResult SearchSectorByCodeNo(string CodeNumber)
        {
            SectorDataAccess obj = new SectorDataAccess();
            tbl_Sector lst = new tbl_Sector();
            lst = obj.GetSectorbyCode(CodeNumber);
            if (lst!= null)
            {
                return Json(new { result = lst, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = lst, ErrorMessage = CustomMessages.NoValueExist }, JsonRequestBehavior.AllowGet);
            }

        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public JsonResult AddSector(string SectorDescription, string Code, bool? IsActive)
        {
            bool success;
            SectorDataAccess obj = new SectorDataAccess();
            tbl_Sector lst = new tbl_Sector();
            //ConfigurationVM lst = new ConfigurationVM();
            lst.Description = SectorDescription;
            lst.Code = Code;
            
            lst.IsActive = IsActive;

            success = obj.AddSector(lst);
            if (success == true)
            {
                return Json(new { result = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = success, ErrorMessage = CustomMessages.GenericErrorMessage }, JsonRequestBehavior.AllowGet);
            }

        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public JsonResult UpdateSector(string SectorDescription, string Code,bool? IsActive)
        {
            bool success;
            SectorDataAccess obj = new SectorDataAccess();
            tbl_Sector lst = new tbl_Sector();
            //ConfigurationVM lst = new ConfigurationVM();
            lst.Description = SectorDescription;
            lst.Code = Code;

            lst.IsActive = IsActive;
            success = obj.UpdateSector(lst);
            if (success == true)
            {
                return Json(new { result = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = success, ErrorMessage = CustomMessages.GenericErrorMessage }, JsonRequestBehavior.AllowGet);
            }

        }


        #endregion

        #region OperatingInstruction
        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult OperatingInstructionScreen()
        {
            return View();
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public JsonResult SearchOpInsByCodeNo(string CodeNumber)
        {
            OperatingInstructionDataAccess obj = new OperatingInstructionDataAccess();
            tbl_OperatingInstructions lst = new tbl_OperatingInstructions();
            lst = obj.GetOperatingInstructionsbyCode(CodeNumber);
            if (lst != null)
            {
                return Json(new { result = lst, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = lst, ErrorMessage = CustomMessages.NoValueExist }, JsonRequestBehavior.AllowGet);
            }

        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public JsonResult AddOperatingInstruction(string OperatingInstruction, string Code, bool? IsActive)
        {
            bool success;
            OperatingInstructionDataAccess obj = new OperatingInstructionDataAccess();
            tbl_OperatingInstructions lst = new tbl_OperatingInstructions();
            //ConfigurationVM lst = new ConfigurationVM();
            lst.Description = OperatingInstruction;
            lst.Code = Code;

            lst.IsActive = IsActive;

            success = obj.AddOperatingInstruction(lst);
            if (success == true)
            {
                return Json(new { result = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = success, ErrorMessage = CustomMessages.GenericErrorMessage }, JsonRequestBehavior.AllowGet);
            }

        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public JsonResult UpdateOperatingInstruction(string OperatingInstruction, string Code, bool? IsActive)
        {
            bool success;
            OperatingInstructionDataAccess obj = new OperatingInstructionDataAccess();
            tbl_OperatingInstructions lst = new tbl_OperatingInstructions();
            //ConfigurationVM lst = new ConfigurationVM();
            lst.Description = OperatingInstruction;
            lst.Code = Code;

            lst.IsActive = IsActive;
            success = obj.UpdateOperatingInstructions(lst);
            if (success == true)
            {
                return Json(new { result = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { result = success, ErrorMessage = CustomMessages.GenericErrorMessage }, JsonRequestBehavior.AllowGet);
            }

        }


        #endregion

        #region IncomeAccounts
        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult IncomeAccountScreen()
        {
            List<tbl_IncomeAccounts> viewModel = new IncomeAccountsDataAccess().GetAllIncomeAccounts();
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult GetIncomeAccount(int requestId)
        {
            var record = new IncomeAccountsDataAccess().GetIncomeAccountById(requestId);
            return Json(new { IsSuccess = (record == null) ? false : true, ErrorMessage = (record == null) ? CustomMessages.IncomeAccountError : string.Empty, Response = record }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult UpdateIncomeAccount(tbl_IncomeAccounts request)
        {
            var success = new IncomeAccountsDataAccess().UpdateIncomeAccounts(request);
            return Json(new { IsSuccess = success, ErrorMessage = (success == true) ? CustomMessages.IncomeAccountSuccess : CustomMessages.IncomeAccountError, Response = success });
        }

        #endregion

        #region PostingRestrictions
        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult PostingRestrictionScreen()
        {
            return View();
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public JsonResult SearchPostResByCodeNo(string CodeNumber)
        {
            var lst = new PostingRestrictionDataAccess().GetPostingRestrictionsbyCode(CodeNumber);
            if (lst != null)
                return Json(new { result = lst, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { result = lst, ErrorMessage = CustomMessages.NoValueExist }, JsonRequestBehavior.AllowGet);

        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public JsonResult AddPostingRestriction(string PostingRestriction, string Code, bool? IsActive)
        {
            var lst = new tbl_Posting_Restrictions()
            {
                Description = PostingRestriction,
                Code = Code,
                IsActive = IsActive
            };

            var success = new PostingRestrictionDataAccess().AddPostingRestrictions(lst);
            if (success == true)
                return Json(new { result = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { result = success, ErrorMessage = CustomMessages.GenericErrorMessage }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public JsonResult UpdatePostingRestriction(string PostingRestriction, string Code, bool? IsActive)
        {
            var lst = new tbl_Posting_Restrictions()
            {
                Description = PostingRestriction,
                Code = Code,
                IsActive = IsActive
            };

            var success = new PostingRestrictionDataAccess().UpdatePostingRestrictions(lst);
            if (success == true)
                return Json(new { result = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { result = success, ErrorMessage = CustomMessages.GenericErrorMessage }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region SessionTimeoutScreen
        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult SetTimeoutScreen()
        {
            SessionDataAccess sda = new SessionDataAccess();
            ViewBag.Timeout = sda.GetLastTimeout();
            return View();
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult AddTimeout(int sessiontime)
        {
            bool flag = false;
            SessionDataAccess sda = new SessionDataAccess();
            flag=sda.InsertSessionTimeout(sessiontime);
            Configuration config = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
            SystemWebSectionGroup syswebSection = (SystemWebSectionGroup)config.GetSectionGroup("system.web");

            //you can change the value as you want
            syswebSection.SessionState.Timeout = new TimeSpan(0, sessiontime, 0);
            config.Save();
            return Json(new { result = flag, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}