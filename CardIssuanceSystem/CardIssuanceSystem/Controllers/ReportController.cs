using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL;
using CardIssuanceSystem.DAL.DataAccessClasses;
using CardIssuanceSystem.Filters;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace CardIssuanceSystem.Controllers
{
    public class ReportController : BaseController
    {
        public ActionResult DemoReport()
        {
            var data = new RequestDataAccess().GetRequestDataForExport("N", 1, "0002");
            var viewModel = data.Rows.Count > 0 ? DAL.ReflectionHelper.CreateGenericListFromDataTable<RequestForExportVM>(data) : new List<RequestForExportVM>();

            return View(viewModel);
        }

        public ActionResult PartialDemoReport(string branchCode, string requestType, int cardType)
        {
            var data = new RequestDataAccess().GetRequestDataForExport(requestType, cardType, branchCode);
            var viewModel = data.Rows.Count > 0 ? DAL.ReflectionHelper.CreateGenericListFromDataTable<RequestForExportVM>(data) : new List<RequestForExportVM>();

            return PartialView("_partialDemoReport", viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" ,"u","c"})]
        public ActionResult ExceptionReport()
        {
            var viewModel = new List<ExceptionLogReportVM>();
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult PartialExceptionReport(DateTime? startDate, DateTime? endDate)
        {
            var minStartDate = startDate == null ? startDate : CustomHelper.ConvertToMinDate(startDate);
            var maxEndDate = endDate == null ? endDate : CustomHelper.ConvertToMaxDate(endDate);

            var data = new UserLogDataAccess().GetExceptionLog("Import", minStartDate, maxEndDate, "M");
            var viewModel = new List<ExceptionLogReportVM>();
            foreach (var item in data.Where(e => !string.IsNullOrEmpty(e.Descp)).ToList())
            {
                var datum = JsonConvert.DeserializeObject<List<ExceptionLogReportVM>>(item.Descp);
                datum.ForEach(e => e.Timestamp = item.ActionTimestamp);
                viewModel.AddRange(datum);
            }

            return PartialView("_partialExceptionReport", viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult TransactionReport()
        {
            var viewModel = new List<tbl_transactions>();
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult PartialTransactionReport(DateTime? startDate, DateTime? endDate)
        {
            var minStartDate = startDate == null ? startDate : CustomHelper.ConvertToMinDate(startDate);
            var maxEndDate = endDate == null ? endDate : CustomHelper.ConvertToMaxDate(endDate);
            var viewModel = new TransactionDataAccess().GetTransactions(minStartDate, maxEndDate);

            return PartialView("_partialTransactionReport", viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult CardImportReport()
        {
            var viewModel = new List<ReportCardImportVM>();
            var branches = new RegionDataAccess().GetAllRegions();
            var cardtypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewData["Branches"] = branches ?? new List<tbl_Region>();
            ViewData["CardTypes"] = cardtypes ?? new List<tbl_Card_Types>();

            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult PartialCardImportReport(ReportCardImportVM request)
        {
            var minProductionDate = request.CardProductionDate == null ? request.CardProductionDate : CustomHelper.ConvertToMinDate(request.CardProductionDate);
            var minCardImportDate = request.CardImportDate == null ? request.CardImportDate : CustomHelper.ConvertToMinDate(request.CardImportDate);
            var data = new FileImportDataAccess().GetFileImports(request.AccountNo, request.CardNo, request.BranchCode, request.CardTypeId, minProductionDate, minCardImportDate);
            var html = data.ToHTMLTable(Url.Action("Export", "Report"));

            return Content(html);
        }

        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult RequestReport()
        {
            var branches = new RegionDataAccess().GetAllRegions();
            var cardtypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewData["Branches"] = branches ?? new List<tbl_Region>();
            ViewData["CardTypes"] = cardtypes ?? new List<tbl_Card_Types>();

            return View();
        }

        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult PartialRequestReport(string CardNo, string AccountNo, string BranchCode, int? CardTypeId, string CIF, long? RequestNo, int? Waive, string RequestType,string ReportType, DateTime? StartDate, DateTime? EndDate, string AuthorizeStatus)
        {
            Waive = Waive ?? -1;
            CardTypeId = CardTypeId ?? -1;
            RequestNo = RequestNo ?? -1;
            var minStartDate = StartDate == null ? StartDate : CustomHelper.ConvertToMinDate(StartDate);
            var maxEndDate = EndDate == null ? EndDate : CustomHelper.ConvertToMaxDate(EndDate);
            var data = new RequestDataAccess().GetRequestDataForReport(CardNo, AccountNo, BranchCode, CardTypeId, CIF, RequestNo, Waive, RequestType,ReportType, minStartDate.GetValueOrDefault(), maxEndDate.GetValueOrDefault(), AuthorizeStatus);
            var viewModel = data.ToHTMLTable(Url.Action("Export", "Report"));

            return Content(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult RecoveryReport()
        {
            var branches = new RegionDataAccess().GetAllRegions();
            var cardtypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewData["Branches"] = branches ?? new List<tbl_Region>();

            return View();
        }

        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult PartialRecoveryReport(string CardNo, string AccountNo, string BranchCode, DateTime? StartDate, DateTime? EndDate)
        {
            var minStartDate = StartDate == null ? StartDate : CustomHelper.ConvertToMinDate(StartDate);
            var maxEndDate = EndDate == null ? EndDate : CustomHelper.ConvertToMaxDate(EndDate);
            var data = new RequestDataAccess().GetRecoveryDataForReport(CardNo, AccountNo, BranchCode, minStartDate.GetValueOrDefault(), maxEndDate.GetValueOrDefault());
            var viewModel = data.ToHTMLTable(Url.Action("Export", "Report"));

            return Content(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public FileResult Export(string GridHtml, string GridCss)
        {
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                StringReader sr = new StringReader(GridHtml);
                Document pdfDoc = new Document();
                Rectangle one = new Rectangle(8000, 6000);
                Rectangle two = new Rectangle(700, 400);
                pdfDoc.SetPageSize(one);
                pdfDoc.SetMargins(2, 2, 2, 2);
                Font fdefault = FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                //writer.DirectContent.SetFontAndSize(GetFont("arial", "arial.TTF").BaseFont, 8f);
                //XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                using (var cssMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(GridCss)))
                {
                    using (var htmlMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(GridHtml)))
                    {
                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, htmlMemoryStream, cssMemoryStream);
                    }
                }
                pdfDoc.Close();
                return File(stream.ToArray(), "application/pdf", "Grid.pdf");
            }
        }

        private static Font GetFont(string fontName, string filename)
        {
            if (!FontFactory.IsRegistered(fontName))
            {
                var fontPath = Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\" + filename;
                FontFactory.Register(fontPath);
            }
            return FontFactory.GetFont(fontName, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        }

        #region New Recovery Reports
        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult CurrentRecoveryReport()
        {
            var branches = new RegionDataAccess().GetAllRegions();
            var cardtypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewData["Branches"] = branches ?? new List<tbl_Region>();

            return View();
        }

        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult PartialCurrentRecoveryReport(string CardNo, string AccountNo, string BranchCode, DateTime? StartDate, DateTime? EndDate, string RequestType, string CIF, DateTime? TransDate)
        {
            try
            {
                var minStartDate = StartDate == null ? StartDate : CustomHelper.ConvertToMinDate(StartDate);
                var maxEndDate = EndDate == null ? EndDate : CustomHelper.ConvertToMaxDate(EndDate);
                var data = new RequestDataAccess().GetDataForCurrentRecovery(CardNo, AccountNo, BranchCode, minStartDate.GetValueOrDefault(), maxEndDate.GetValueOrDefault(), RequestType, CIF);
                //***Add Row  Total in datatable
                if (data.Rows.Count >= 1)
                {
                    DataRow row = data.NewRow();
                    row[data.Columns[0].ColumnName.ToString()] = DBNull.Value;
                    row[data.Columns[1].ColumnName.ToString()] = "";
                    row[data.Columns[2].ColumnName.ToString()] = "";

                    row[data.Columns[3].ColumnName.ToString()] = "";
                    row[data.Columns[4].ColumnName.ToString()] = "";
                    row[data.Columns[5].ColumnName.ToString()] = "";

                    row[data.Columns[6].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[6].ColumnName.ToString() + "])", "[" + data.Columns[6].ColumnName.ToString() + "] > 0");
                    row[data.Columns[7].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[7].ColumnName.ToString() + "])", "[" + data.Columns[7].ColumnName.ToString() + "] > 0");
                    row[data.Columns[8].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[8].ColumnName.ToString() + "])", "[" + data.Columns[8].ColumnName.ToString() + "] > 0");

                    row[data.Columns[9].ColumnName.ToString()] = "";
                    row[data.Columns[10].ColumnName.ToString()] = DBNull.Value;


                    data.Rows.Add(row);
                }

                var viewModel = data.ToHTMLTable(Url.Action("Export", "Report"));

                return Content(viewModel);
            }
            catch (Exception ex)
            { throw ex; }

        }



        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult DailyIncomeReport()
        {
            var branches = new RegionDataAccess().GetAllRegions();
            var cardtypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewData["Branches"] = branches ?? new List<tbl_Region>();

            return View();
        }

        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult PartialDailyIncomeReport(string CardNo, string AccountNo, string BranchCode, DateTime? StartDate, DateTime? EndDate, string RequestType, string CIF, DateTime? TransDate)
        {
            try
            {
                var maxEndDate = TransDate == null ? TransDate : CustomHelper.ConvertToMaxDate(TransDate);
                var data = new RequestDataAccess().GetDataForDailyIncome(CardNo, AccountNo, BranchCode, maxEndDate.GetValueOrDefault());
                if (data.Rows.Count >= 1)
                {
                    DataRow row = data.NewRow();
                    row[data.Columns[0].ColumnName.ToString()] = DBNull.Value;
                    row[data.Columns[1].ColumnName.ToString()] = DBNull.Value;
                    row[data.Columns[2].ColumnName.ToString()] = "";

                    row[data.Columns[3].ColumnName.ToString()] = "";
                    row[data.Columns[4].ColumnName.ToString()] = "";
                    row[data.Columns[5].ColumnName.ToString()] = "";
                    row[data.Columns[6].ColumnName.ToString()] = "";

                    row[data.Columns[7].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[7].ColumnName.ToString() + "])", "[" + data.Columns[7].ColumnName.ToString() + "] > 0");
                    row[data.Columns[8].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[8].ColumnName.ToString() + "])", "[" + data.Columns[8].ColumnName.ToString() + "] > 0");

                    row[data.Columns[9].ColumnName.ToString()] = "";
                    row[data.Columns[10].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[10].ColumnName.ToString() + "])", "[" + data.Columns[10].ColumnName.ToString() + "] > 0");

                    row[data.Columns[11].ColumnName.ToString()] = "";
                    row[data.Columns[12].ColumnName.ToString()] = "";
                    row[data.Columns[13].ColumnName.ToString()] = "";
                    row[data.Columns[14].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[14].ColumnName.ToString() + "])", "[" + data.Columns[14].ColumnName.ToString() + "] > 0");
                    row[data.Columns[15].ColumnName.ToString()] = "";
                    row[data.Columns[16].ColumnName.ToString()] = "";


                    data.Rows.Add(row);
                }


                var viewModel = data.ToHTMLTable(Url.Action("Export", "Report"));

                return Content(viewModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult DailyReversalReport()
        {
            var branches = new RegionDataAccess().GetAllRegions();
            var cardtypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewData["Branches"] = branches ?? new List<tbl_Region>();

            return View();
        }

        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult PartialDailyReversalReport(string CardNo, string AccountNo, string BranchCode, DateTime? ApplicableDateFrom, DateTime? ApplicableDateTo, string RequestType, string CIF, DateTime? TransDate)
        {
            try
            {
                var maxEndDate = TransDate == null ? TransDate : CustomHelper.ConvertToMaxDate(TransDate);
                var data = new RequestDataAccess().GetDataForDailyReversal(CardNo, AccountNo, BranchCode, maxEndDate.GetValueOrDefault());
                if (data.Rows.Count >= 1)
                {
                    DataRow row = data.NewRow();
                    row[data.Columns[0].ColumnName.ToString()] = DBNull.Value;
                    row[data.Columns[1].ColumnName.ToString()] = DBNull.Value;
                    row[data.Columns[2].ColumnName.ToString()] = "";

                    row[data.Columns[3].ColumnName.ToString()] = "";
                    row[data.Columns[4].ColumnName.ToString()] = "";
                    row[data.Columns[5].ColumnName.ToString()] = "";
                    row[data.Columns[6].ColumnName.ToString()] = "";

                    row[data.Columns[7].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[7].ColumnName.ToString() + "])", "[" + data.Columns[7].ColumnName.ToString() + "] > 0");
                    row[data.Columns[8].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[8].ColumnName.ToString() + "])", "[" + data.Columns[8].ColumnName.ToString() + "] > 0");

                    row[data.Columns[9].ColumnName.ToString()] = "";
                    row[data.Columns[10].ColumnName.ToString()] = "";

                    row[data.Columns[11].ColumnName.ToString()] = "";
                    row[data.Columns[12].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[12].ColumnName.ToString() + "])", "[" + data.Columns[12].ColumnName.ToString() + "] > 0");
                    row[data.Columns[13].ColumnName.ToString()] = "";
                    row[data.Columns[14].ColumnName.ToString()] = "";
                    row[data.Columns[15].ColumnName.ToString()] = "";
                    row[data.Columns[16].ColumnName.ToString()] = "";

                    row[data.Columns[17].ColumnName.ToString()] = "";
                    row[data.Columns[18].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[18].ColumnName.ToString() + "])", "[" + data.Columns[18].ColumnName.ToString() + "] > 0");
                    row[data.Columns[19].ColumnName.ToString()] = "";
                    row[data.Columns[20].ColumnName.ToString()] = "";
                    row[data.Columns[21].ColumnName.ToString()] = "";
                    row[data.Columns[22].ColumnName.ToString()] = "";
                    row[data.Columns[23].ColumnName.ToString()] = "";
                    row[data.Columns[24].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[24].ColumnName.ToString() + "])", "[" + data.Columns[24].ColumnName.ToString() + "] > 0");
                    row[data.Columns[25].ColumnName.ToString()] = "";
                    row[data.Columns[26].ColumnName.ToString()] = "";



                    data.Rows.Add(row);

                }

                var viewModel = data.ToHTMLTable(Url.Action("Export", "Report"));

                return Content(viewModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult DailyTransactionReport()
        {
            var branches = new RegionDataAccess().GetAllRegions();
            var cardtypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewData["Branches"] = branches ?? new List<tbl_Region>();

            return View();
        }

        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult PartialDailyTransactionReport(string CardNo, string AccountNo, string BranchCode, DateTime? ApplicableDateFrom, DateTime? ApplicableDateTo, string RequestType, string CIF, DateTime? TransDate)
        {
            try
            {
                var maxEndDate = TransDate == null ? TransDate : CustomHelper.ConvertToMaxDate(TransDate);
                var data = new RequestDataAccess().GetDataForDailyTransaction(CardNo, AccountNo, BranchCode, maxEndDate.GetValueOrDefault());
                if (data.Rows.Count >= 1)
                {
                    DataRow row = data.NewRow();
                    row[data.Columns[0].ColumnName.ToString()] = DBNull.Value;
                    row[data.Columns[1].ColumnName.ToString()] = DBNull.Value;
                    row[data.Columns[2].ColumnName.ToString()] = "";

                    row[data.Columns[3].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[3].ColumnName.ToString() + "])", "[" + data.Columns[3].ColumnName.ToString() + "] > 0"); ;
                    row[data.Columns[4].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[4].ColumnName.ToString() + "])", "[" + data.Columns[4].ColumnName.ToString() + "] > 0"); ;
                    row[data.Columns[5].ColumnName.ToString()] = "";
                    row[data.Columns[6].ColumnName.ToString()] = "";

                    row[data.Columns[7].ColumnName.ToString()] = "";
                    row[data.Columns[8].ColumnName.ToString()] = "";

                    row[data.Columns[9].ColumnName.ToString()] = "";
                    row[data.Columns[10].ColumnName.ToString()] = "";

                    row[data.Columns[11].ColumnName.ToString()] = "";
                    row[data.Columns[12].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[12].ColumnName.ToString() + "])", "[" + data.Columns[12].ColumnName.ToString() + "] > 0");
                    row[data.Columns[13].ColumnName.ToString()] = "";
                    row[data.Columns[14].ColumnName.ToString()] = "";
                    row[data.Columns[15].ColumnName.ToString()] = "";
                    row[data.Columns[16].ColumnName.ToString()] = "";

                    row[data.Columns[17].ColumnName.ToString()] = "";
                    row[data.Columns[18].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[18].ColumnName.ToString() + "])", "[" + data.Columns[18].ColumnName.ToString() + "] > 0");
                    row[data.Columns[19].ColumnName.ToString()] = "";
                    row[data.Columns[20].ColumnName.ToString()] = "";
                    row[data.Columns[21].ColumnName.ToString()] = "";
                    row[data.Columns[22].ColumnName.ToString()] = "";
                    row[data.Columns[23].ColumnName.ToString()] = "";
                    row[data.Columns[24].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[24].ColumnName.ToString() + "])", "[" + data.Columns[24].ColumnName.ToString() + "] > 0");
                    row[data.Columns[25].ColumnName.ToString()] = "";
                    row[data.Columns[26].ColumnName.ToString()] = "";



                    data.Rows.Add(row);
                }

                var viewModel = data.ToHTMLTable(Url.Action("Export", "Report"));

                return Content(viewModel);
            }
            catch (Exception ex)
            { throw ex; }
        }



        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult RecoveryExceptionReport()
        {
            var branches = new RegionDataAccess().GetAllRegions();
            var cardtypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewData["Branches"] = branches ?? new List<tbl_Region>();

            return View();
        }

        [AuthOp(RoleTitle = new string[] { "a", "u", "c" })]
        public ActionResult PartialRecoveryExceptionReport(string CardNo, string AccountNo, string BranchCode, DateTime? ApplicableDateFrom, DateTime? ApplicableDateTo, string RequestType, string CIF, DateTime? TransDate)
        {
            try
            {
                var maxEndDate = TransDate == null ? TransDate : CustomHelper.ConvertToMaxDate(TransDate);
                var data = new RequestDataAccess().GetDataExceptionReport(CardNo, AccountNo, BranchCode, RequestType, CIF, maxEndDate.GetValueOrDefault());
                if (data.Rows.Count >= 1)
                {
                    DataRow row = data.NewRow();
                    row[data.Columns[0].ColumnName.ToString()] = DBNull.Value;
                    row[data.Columns[1].ColumnName.ToString()] = "";
                    row[data.Columns[2].ColumnName.ToString()] = "";

                    row[data.Columns[3].ColumnName.ToString()] = "";
                    row[data.Columns[4].ColumnName.ToString()] = "";
                    row[data.Columns[5].ColumnName.ToString()] = DBNull.Value;
                    row[data.Columns[6].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[6].ColumnName.ToString() + "])", "[" + data.Columns[6].ColumnName.ToString() + "] > 0");

                    row[data.Columns[7].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[7].ColumnName.ToString() + "])", "[" + data.Columns[7].ColumnName.ToString() + "] > 0");
                    row[data.Columns[8].ColumnName.ToString()] = data.Compute("Sum([" + data.Columns[8].ColumnName.ToString() + "])", "[" + data.Columns[8].ColumnName.ToString() + "] > 0");

                    row[data.Columns[9].ColumnName.ToString()] = "";
                    row[data.Columns[10].ColumnName.ToString()] = DBNull.Value;

                    row[data.Columns[11].ColumnName.ToString()] = "";
                    row[data.Columns[12].ColumnName.ToString()] = "";
                    data.Rows.Add(row);
                }

                var viewModel = data.ToHTMLTable(Url.Action("Export", "Report"));
                return Content(viewModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}