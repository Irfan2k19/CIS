using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardIssuanceSystem.DAL;
using CardIssuanceSystem.DAL.DataAccessClasses;
using CardIssuanceSystem.Core.Helpers;
using System.IO;
using CardIssuanceSystem.Filters;
using CardIssuanceSystem.Core.ViewModel;
using Newtonsoft.Json;

namespace CardIssuanceSystem.Controllers
{
    public class FileImportController : BaseController
    {
        [AuthOp(RoleTitle = new string[] { "u" })]
        // GET: FileImport
        public ActionResult Index(int? Id)
        {
            ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewBag.RequestId = Id ?? 0;
            var filePath = new FilePathDataAccess().GetFilePathByTypeID(((int)Constants.enPathType.Import).ToString());
            var viewModel = FileHelper.GetFileNamesFromDirectory(filePath.Path);
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        /// <summary>
        /// Sonaware:
        ///Field Name Index
        ///* Account Number 4 – AC : Account Number
        ///*Customer Number 37 – CP: CIF#
        ///Frequency Cycle Begin Date 50 – DA: Card Issuance Date(Expiry is +5 years)
        ///*PAN 6 – DA : Card #
        ///Pri/Sec Code 5 – DA : Main or Supplementary Card
        ///Tipper Flag 4 – DA : Card Code(*Card Type ID to be picked from Card Type table)
        ///Default Account 8 – AC: O: is def acct Y: linked account N: linked account
        ///Revised ACCeSS 3.5 Import Format For Host_2.2: CIS->IRIS
        ///ACCeSS Export File Utility for SonawareDotNet_1_0: IRIS->CIS
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult AddUpdateFileImport(tbl_File_Imports request, string filePath)
        {
            List<tbl_Customer_Cards> lst = new List<tbl_Customer_Cards>();
            List<ExceptionLogVM> lstExceptionLog = new List<ExceptionLogVM>();
            string[] record = new string[] { };
            string[] data = new string[] { };
            int? cardType = null;
            string result = string.Empty;
            var currentDate = DateTime.Now;
            var outputFilename = $"{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}.txt";

            //var filename = "card.txt"/*"DEMO2018-27-2--16-47-25.txt"*/;
            //var filePath = new FilePathDataAccess().GetFilePathByTypeID(((int)Constants.enPathType.Import).ToString());
            //string path = filePath?.Path ?? @"C:\";

            var path = filePath;
            var filename = Path.GetFileName(path);
            // Need to Uncomment its only for testing plain file
            //var response = new GNUPG_Wrapper().Decrypt(Path.Combine(path, filename), outputFilename);
            record = FileHelper.Import(path/*outputFilename*/); //Need to Uncomment its only for testing plain file

            if (record != null)
            {
                data = record;       /*record.Where(e => !string.IsNullOrEmpty(e) && !cardNumbers.Contains(e.Split('|')[3])).ToArray();*/
                if (data.Length > 0)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        var datum = data[i].Split('|');
                        datum = datum.Length > 7 ? datum.Take(7).ToArray() : datum;
                        if (datum.Length == 7)
                        {
                            var accno = datum[datum.Length - (datum.Length)];
                            var custno = datum[datum.Length - (datum.Length - 1)];
                            var strfreqdate = datum[datum.Length - (datum.Length - 2)];
                            var cardnum = datum[datum.Length - (datum.Length - 3)];
                            var priseccode = datum[datum.Length - (datum.Length - 4)];
                            var cardcode = datum[datum.Length - (datum.Length - 5)];
                            var deforlinked = datum[datum.Length - (datum.Length - 6)];
                            var dtfreqdate = new DateTime(int.Parse(strfreqdate.Substring(0, 4)), int.Parse(strfreqdate.Substring(4, 2)), int.Parse(strfreqdate.Substring(6, 2)), 0, 0, 0);
                            var dtexpiry = dtfreqdate.AddYears(5);

                            if (string.IsNullOrWhiteSpace(accno) || accno.Length != 15)
                                CreateExceptionLog(ref lstExceptionLog, accno, cardnum, custno, string.Format(CustomMessages.AccountNumberEmptyOrLimitExceed, 15), filename);

                            if (string.IsNullOrWhiteSpace(cardnum) || cardnum.Length != 16)
                                CreateExceptionLog(ref lstExceptionLog, accno, cardnum, custno, string.Format(CustomMessages.CardNumberEmptyOrLimitExceed, 16), filename);

                            if (string.IsNullOrWhiteSpace(custno) || custno.Length != 10)
                                CreateExceptionLog(ref lstExceptionLog, accno, cardnum, custno, string.Format(CustomMessages.CustomerNumberEmptyOrLimitExceed, 10), filename);

                            var requestRecord = new RequestDataAccess().GetRequestForImport(custno, accno);

                            if (requestRecord != null)
                            {
                                //cardType = new CardTypesDataAccess().GetCardTypeIdByCardCode(cardcode);
                                //request.CardTypes = Convert.ToString(cardType);
                                var salutation = requestRecord.Salutation ?? string.Empty;
                                var cardtitle = requestRecord.CardTitle ?? string.Empty;
                                cardType = requestRecord.CardTypeID ?? 0;
                                lst.Add(new tbl_Customer_Cards { AccountNo = accno, CardNo = cardnum, CardIssuance = dtfreqdate, CardExpiry = new DateTime(dtexpiry.Year, dtexpiry.Month, DateTime.DaysInMonth(dtexpiry.Year, dtexpiry.Month)), CardTypeID = cardType, Salutation = salutation, CardTitle = cardtitle, CardStatusActive = true, CIF = custno });
                            }
                            else
                                CreateExceptionLog(ref lstExceptionLog, accno, cardnum, custno, CustomMessages.RequestNotExists, filename);
                        }
                    }

                    // Create successfull card file for T24
                    var filepath = new FilePathDataAccess().GetFilePathByTypeID(((int)Constants.enPathType.Export).ToString());
                    var fullpath = (filepath?.Path ?? string.Empty) == string.Empty ? $"{@"C:\CIS_card_file_"}{currentDate.ToString(Constants.Formats.DateFormat)}{currentDate.ToString(Constants.Formats.TimeFormat)}" : $"{filepath?.Path}\\CIS_card_file_{currentDate.ToString(Constants.Formats.DateFormat)}{currentDate.ToString(Constants.Formats.TimeFormat)}";

                    var fileheader = $"caRD.NUMBER,ACCT.ID1::ACCTID2";
                    FileHelper.WriteFile(fullpath, new string[] { fileheader });

                    // Add cards to customer card table
                    foreach (var item in lst)
                    {
                        if (new CustomerCardDataAccess().AddCustomerCard(item))
                        {
                            request.RecordCount = (request.RecordCount == null) ? 1 : ++request.RecordCount;

                            var filedata = $"{item.CardNo},{item.AccountNo}::{item.CIF}";
                            FileHelper.WriteFile(fullpath, new string[] { filedata });
                        }
                        else
                            CreateExceptionLog(ref lstExceptionLog, item.AccountNo, item.CardNo, item.CIF, CustomMessages.CardAlreadyExists, filename);
                    }
                    /*new CustomerCardDataAccess().AddMultipleCustomerCard(lst);*/

                    // Upgrade Customer Account, link card to customer accounts
                    lst.ForEach(e => new CustomerAccountDataAccess().UpdateCustomerAccountCardNo(e.AccountNo, e.CIF, e.CardNo));

                    /*request.RecordCount = (request.RecordCount ?? 0) + lst.Count;*/
                }
            }

            var IsSuccess = new FileImportDataAccess().AddFileImport(request);
            if (IsSuccess)
            {
                AddExceptionLog(lstExceptionLog);

                return Json(new { IsSuccess = IsSuccess, Response = IsSuccess, ErrorMessage = string.Format(CustomMessages.ImportRecordsRetrieveAndImport, record.Length, request.RecordCount ?? 0) }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                AddExceptionLog(lstExceptionLog);

                return Json(new { IsSuccess = IsSuccess, ErrorMessage = CustomMessages.GenericErrorMessage, Response = IsSuccess }, JsonRequestBehavior.AllowGet);
            }
        }


        [AuthOp(RoleTitle = new string[] { "u" })]
        // GET: FileImport
        public ActionResult EncryptedFileImport(int? Id)
        {
            ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewBag.RequestId = Id ?? 0;
            var filePath = new FilePathDataAccess().GetFilePathByTypeID(((int)Constants.enPathType.Import).ToString());
            var viewModel = FileHelper.GetFileNamesFromDirectory(filePath.Path);
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        /// <summary>
        /// Sonaware:
        ///Field Name Index
        ///* Account Number 4 – AC : Account Number
        ///*Customer Number 37 – CP: CIF#
        ///Frequency Cycle Begin Date 50 – DA: Card Issuance Date(Expiry is +5 years)
        ///*PAN 6 – DA : Card #
        ///Pri/Sec Code 5 – DA : Main or Supplementary Card
        ///Tipper Flag 4 – DA : Card Code(*Card Type ID to be picked from Card Type table)
        ///Default Account 8 – AC: O: is def acct Y: linked account N: linked account
        ///Revised ACCeSS 3.5 Import Format For Host_2.2: CIS->IRIS
        ///ACCeSS Export File Utility for SonawareDotNet_1_0: IRIS->CIS
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult AddUpdateEncryptedFileImport(tbl_File_Imports request, string filePath)
        {
            List<tbl_Customer_Cards> lst = new List<tbl_Customer_Cards>();
            List<ExceptionLogVM> lstExceptionLog = new List<ExceptionLogVM>();
            string[] record = new string[] { };
            string[] data = new string[] { };
            int? cardType = null;
            string result = string.Empty;
            var currentDate = DateTime.Now;
            var outputFilename = $"{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}.txt";
            var path = filePath;
            // Need to Uncomment its only for testing plain file
            var response = new GNUPG_Wrapper().DecryptyWithoutPassBatch(path, outputFilename);
            var filename = Path.GetFileName(response);
            record = FileHelper.Import(response/*outputFilename*/); //Need to Uncomment its only for testing plain file

            if (record != null)
            {
                data = record;       /*record.Where(e => !string.IsNullOrEmpty(e) && !cardNumbers.Contains(e.Split('|')[3])).ToArray();*/
                if (data.Length > 0)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        var datum = data[i].Split('|');
                        datum = datum.Length > 7 ? datum.Take(7).ToArray() : datum;
                        if (datum.Length == 7)
                        {
                            var accno = datum[datum.Length - (datum.Length)];
                            var custno = datum[datum.Length - (datum.Length - 1)];
                            var strfreqdate = datum[datum.Length - (datum.Length - 2)];
                            var cardnum = datum[datum.Length - (datum.Length - 3)];
                            var priseccode = datum[datum.Length - (datum.Length - 4)];
                            var cardcode = datum[datum.Length - (datum.Length - 5)];
                            var deforlinked = datum[datum.Length - (datum.Length - 6)];
                            var dtfreqdate = new DateTime(int.Parse(strfreqdate.Substring(0, 4)), int.Parse(strfreqdate.Substring(4, 2)), int.Parse(strfreqdate.Substring(6, 2)), 0, 0, 0);
                            var dtexpiry = dtfreqdate.AddYears(5);

                            if (string.IsNullOrWhiteSpace(accno) || accno.Length != 15)
                                CreateExceptionLog(ref lstExceptionLog, accno, cardnum, custno, string.Format(CustomMessages.AccountNumberEmptyOrLimitExceed, 15), filename);

                            if (string.IsNullOrWhiteSpace(cardnum) || cardnum.Length != 16)
                                CreateExceptionLog(ref lstExceptionLog, accno, cardnum, custno, string.Format(CustomMessages.CardNumberEmptyOrLimitExceed, 16), filename);

                            if (string.IsNullOrWhiteSpace(custno) || custno.Length != 10)
                                CreateExceptionLog(ref lstExceptionLog, accno, cardnum, custno, string.Format(CustomMessages.CustomerNumberEmptyOrLimitExceed, 10), filename);

                            var requestRecord = new RequestDataAccess().GetRequestForImport(custno, accno);

                            if (requestRecord != null)
                            {
                                //cardType = new CardTypesDataAccess().GetCardTypeIdByCardCode(cardcode);
                                //request.CardTypes = Convert.ToString(cardType);
                                var salutation = requestRecord.Salutation ?? string.Empty;
                                var cardtitle = requestRecord.CardTitle ?? string.Empty;
                                cardType = requestRecord.CardTypeID ?? 0;
                                lst.Add(new tbl_Customer_Cards { AccountNo = accno, CardNo = cardnum, CardIssuance = dtfreqdate, CardExpiry = new DateTime(dtexpiry.Year, dtexpiry.Month, DateTime.DaysInMonth(dtexpiry.Year, dtexpiry.Month)), CardTypeID = cardType, Salutation = salutation, CardTitle = cardtitle, CardStatusActive = true, CIF = custno });
                            }
                            else
                                CreateExceptionLog(ref lstExceptionLog, accno, cardnum, custno, CustomMessages.RequestNotExists, filename);
                        }
                    }

                    // Add cards to customer card table
                    foreach (var item in lst)
                    {
                        if (new CustomerCardDataAccess().AddCustomerCard(item))
                        {
                            request.RecordCount = (request.RecordCount == null) ? 1 : ++request.RecordCount;

                            // Create successfull card file for T24
                            var filepath = new FilePathDataAccess().GetFilePathByTypeID(((int)Constants.enPathType.Export).ToString());
                            var fullpath = (filepath?.Path ?? string.Empty) == string.Empty ? $"{@"C:\CIS_card_file_"}{currentDate.ToString(Constants.Formats.DateFormat)}{currentDate.ToString(Constants.Formats.TimeFormat)}" : $"{filepath?.Path}\\CIS_card_file_{currentDate.ToString(Constants.Formats.DateFormat)}{currentDate.ToString(Constants.Formats.TimeFormat)}";

                            var filedata = $"{item.AccountNo}|{item.CIF}|{item.CardNo}";
                            FileHelper.WriteFile(fullpath, new string[] { filedata });
                        }
                        else
                            CreateExceptionLog(ref lstExceptionLog, item.AccountNo, item.CardNo, item.CIF, CustomMessages.CardAlreadyExists, filename);
                    }
                    /*new CustomerCardDataAccess().AddMultipleCustomerCard(lst);*/

                    // Upgrade Customer Account, link card to customer accounts
                    lst.ForEach(e => new CustomerAccountDataAccess().UpdateCustomerAccountCardNo(e.AccountNo, e.CIF, e.CardNo));

                    /*request.RecordCount = (request.RecordCount ?? 0) + lst.Count;*/
                }
            }

            var IsSuccess = new FileImportDataAccess().AddFileImport(request);
            if (IsSuccess)
            {
                AddExceptionLog(lstExceptionLog);

                return Json(new { IsSuccess = IsSuccess, Response = IsSuccess, ErrorMessage = string.Format(CustomMessages.ImportRecordsRetrieveAndImport, record.Length, request.RecordCount ?? 0) }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                AddExceptionLog(lstExceptionLog);

                return Json(new { IsSuccess = IsSuccess, ErrorMessage = CustomMessages.GenericErrorMessage, Response = IsSuccess }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// For logging the exception, that comes in between file import operation.
        /// </summary>
        /// <param name="lstExceptionLog"></param>
        /// <param name="accno"></param>
        /// <param name="cardnum"></param>
        /// <param name="custno"></param>
        /// <param name="result"></param>
        private void CreateExceptionLog(ref List<ExceptionLogVM> lstExceptionLog, string accno, string cardnum, string custno, string result, string filename)
        {
            var record = lstExceptionLog.Where(e => e.AccountNumber == accno && e.CardNumber == cardnum && e.CIF == custno && e.Filename == filename).FirstOrDefault();

            if (record != null)
                record.Result += $",{result}";

            else
            {
                lstExceptionLog.Add(new ExceptionLogVM
                {
                    AccountNumber = accno,
                    CardNumber = cardnum,
                    CIF = custno,
                    Result = result,
                    Filename = filename
                });
            }
        }

        private void AddExceptionLog(List<ExceptionLogVM> lstExceptionLog)
        {
            if (lstExceptionLog.Count > 0)
                new UserLogDataAccess().AddExceptionLog(JsonConvert.SerializeObject(lstExceptionLog), "Import");
        }

    }
}