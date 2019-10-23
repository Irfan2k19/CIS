using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.DAL;
using CardIssuanceSystem.DAL.DataAccessClasses;
using CardIssuanceSystem.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CardIssuanceSystem.Controllers
{
    public class FilePathController : BaseController
    {
        [AuthOp(RoleTitle = new string[] { "a" })]
        // GET: FilePath
        public ActionResult Index(int? id)
        {
            List<tbl_File_Paths> viewModel = new List<tbl_File_Paths>();
            ViewBag.RequestId = id ?? 0;
            viewModel = new FilePathDataAccess().GetFilePath();
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult AddUpdateFilePath(tbl_File_Paths request)
        {
            bool isSuccess = false;
            request.Path = request.Path.EndsWith("\\") ? request.Path : request.Path.Insert(request.Path.Length, "\\");
            if (Directory.Exists(request.Path))
                isSuccess = request.ID > 0 ? new FilePathDataAccess().UpdateFilePath(request) : new FilePathDataAccess().AddFilePath(request);

            return Json(new { IsSuccess = isSuccess, ErrorMessage = (isSuccess == false) ? CustomMessages.FilePathNotExists : CustomMessages.FilePathAdded }, JsonRequestBehavior.AllowGet);
        }
    }
}