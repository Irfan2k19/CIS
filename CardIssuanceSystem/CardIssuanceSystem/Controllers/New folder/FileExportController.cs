using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL;
using CardIssuanceSystem.DAL.DataAccessClasses;
using CardIssuanceSystem.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace CardIssuanceSystem.Controllers
{
    public class FileExportController : BaseController
    {
        [AuthOp(RoleTitle = new string[] { "u" })]
        // GET: FileExport
        public ActionResult Index(int? Id)
        {
            ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewBag.Branches = new RegionDataAccess().GetAllRegions().Where(e => e.IsActive == true).ToList();
            ViewBag.RequestId = Id ?? 0;
            return View();
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        // GET: FileExport
        public ActionResult ReExport(int? Id)
        {
            ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewBag.Branches = new RegionDataAccess().GetAllRegions().Where(e => e.IsActive == true).ToList();
            ViewBag.RequestId = Id ?? 0;
            return View();
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AddUpdateFileExport(tbl_File_Exports request, string requestTypeName, string cardTypeName, DateTime? requestDate = null)
        {
            bool IsSuccess = false;
            List<RequestForExportVM> records = null;
            List<RequestForExportVM> query = null;
            DataTable data = new DataTable();
            var currDate = DateTime.Now;
            request.BranchCodes = string.IsNullOrEmpty(request.BranchCodes) ? "0000" : request.BranchCodes;

            // Create filename for export or re-export file 
            var outputFilename = (requestDate == null) ? $"{cardTypeName} {requestTypeName} {currDate.ToString(Constants.Formats.DateFormat)} {currDate.ToString(Constants.Formats.TimeFormat)}" : $"{cardTypeName} {requestTypeName} {currDate.ToString(Constants.Formats.DateFormat)} {currDate.ToString(Constants.Formats.TimeFormat)} (re-export {requestDate.GetValueOrDefault().ToString(Constants.Formats.DateFormat)})";
            outputFilename = outputFilename.ToString().Replace(" ", "").ToString();
            outputFilename = outputFilename + ".txt";
            try
            {
                FileExportViewModel model = new FileExportViewModel();
                string currentDate = DateTime.Now.ToString(Constants.Formats.DateFormat);
                var filePath = new FilePathDataAccess().GetFilePathByTypeID(((int)Constants.enPathType.Export).ToString());

                // Get request data
                if (requestDate == null)
                    data = new RequestDataAccess().GetRequestDataForExport(request.RequestTypes, int.Parse(request.CardTypes), request.BranchCodes);
                else
                    data = new RequestDataAccess().GetRequestDataForExport(request.RequestTypes, int.Parse(request.CardTypes), request.BranchCodes, requestDate);

                records = data.Rows.Count > 0 ? DAL.ReflectionHelper.CreateGenericListFromDataTable<RequestForExportVM>(data) : null;

                // Filter records for duplicate CNIC
                query = (records != null) ? records.GroupBy(e => string.IsNullOrEmpty(e.NIC) ? Guid.NewGuid().ToString() : e.NIC).Select(e => e.FirstOrDefault()).ToList() : null;

                if (query != null)
                {
                    string path = FileHelper.Create(filePath?.Path ?? @"C:\", outputFilename);
                    int dataCount = 1;
                    string action = (request.RequestTypes == Constants.RequestTypes.Issuance || request.RequestTypes == Constants.RequestTypes.Replacement) ? Constants.ActionCodes.Add : (request.RequestTypes == Constants.RequestTypes.Ammendment || request.RequestTypes == Constants.RequestTypes.Upgrade || request.RequestTypes == Constants.RequestTypes.Linking || request.RequestTypes == Constants.RequestTypes.Delinking) ? Constants.ActionCodes.Update : Constants.ActionCodes.Add;

                    // Create header
                    model.Header = new HeaderRecord() { Date = currentDate.LimitTo(8), FileName = "CRD".LimitTo(20), Version = "03.05.00".LimitTo(8) };

                    // Write header
                    FileHelper.WriteFile(path, new string[] { string.Join(string.Empty, model.Header.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(model.Header, null))) });

                    foreach (var requestDetail in query)
                    {
                        var accounts = new List<AccountData>();
                        var account = new AccountData();
                        var lstString = new List<string>();
                        dataCount++;

                        // Create data
                        var cData = GetCardData(dataCount, requestDetail, action);
                        var profile = GetProfileData(dataCount, requestDetail, action);

                        //If Request type is ammendment then we have to create new rows for linked or delinked accounts
                        if (request.RequestTypes == Constants.RequestTypes.Ammendment)
                        {
                            var getDefaultNLinkingAccounts = new CustomerAccountDataAccess().GetCustomerAccountByCardNo(requestDetail.CardNo);
                            requestDetail.DefaultAccount = "O";//Default Account
                            accounts.Add(GetAccountData(dataCount, requestDetail, request.BranchCodes, action));
                            foreach (var item in getDefaultNLinkingAccounts.Where(e => e.AccountNo != requestDetail.AccountNo))
                            {
                                AccountSectionExtension(ref accounts, item.AccountNo, item.AccountTitle, string.IsNullOrEmpty(requestDetail.AccountType) ? "10" : requestDetail.AccountType, requestDetail.BranchCode, request.RequestTypes, dataCount, requestDetail.Pan);
                            }
                        }

                        //If Request type is linking or delinking then we have to create new rows for linking or delinking accounts
                        else if (request.RequestTypes == Constants.RequestTypes.Linking || request.RequestTypes == Constants.RequestTypes.Delinking)
                        {
                            requestDetail.DefaultAccount = "O";//Default Account
                            accounts.Add(GetAccountData(dataCount, requestDetail, request.BranchCodes, action));
                            foreach (var item in requestDetail.LinkingDelinkingAccount.Split(','))
                            {
                                var acc = GetCustomerAccountForLinkingAccounts(item);

                                AccountSectionExtensionForLinking(ref accounts, item, acc.FirstOrDefault()?.AccountTitle ?? string.Empty, string.IsNullOrEmpty(requestDetail.AccountType) ? "10" : requestDetail.AccountType, requestDetail.BranchCode, request.RequestTypes, dataCount, requestDetail.Pan,acc.FirstOrDefault().Currency);
                            }
                        }
                        else
                            accounts.Add(GetAccountData(dataCount, requestDetail, request.BranchCodes, action));

                        var relation = GetRelationData(dataCount, requestDetail, action);

                        // Write data
                        //FileHelper.WriteFile(path, new string[] { string.Join(string.Empty, cData.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(cData, null))), string.Join(string.Empty, profile.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(profile, null))), string.Join(string.Empty, account.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(account, null))), string.Join(string.Empty, relation.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(relation, null))) });

                        lstString.Add(string.Join(string.Empty, cData.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(cData, null))));
                        lstString.Add(string.Join(string.Empty, profile.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(profile, null))));
                        accounts.ForEach(e => lstString.Add(string.Join(string.Empty, e.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(e, null)))));
                        lstString.Add(string.Join(string.Empty, relation.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(relation, null))));

                        FileHelper.WriteFile(path, lstString.ToArray());
                    }

                    // Create footer
                    string datacount = (query.Count + 2).ToString();
                    //model.Footer = new FooterRecord() { Date = currentDate.LimitTo(8), FileName = outputFilename.Replace(".txt", string.Empty).LimitTo(20), RecordNumber = datacount.PadLeft(6, '0') };
                    model.Footer = new FooterRecord() { Date = currentDate.LimitTo(8), FileName = "CRD".LimitTo(20), RecordNumber = datacount.PadLeft(6, '0') };

                    // Write footer
                    FileHelper.WriteFile(path, new string[] { string.Join(string.Empty, model.Footer.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(model.Footer, null))) });

                    // Encrypt file [remove b/c we don't want encrypted file as it works on other action]
                    //new GNUPG_Wrapper().Encrypt(path, outputFilename);

                    // Update file export record
                    new RequestDataAccess().UpdateRequestAfterExport(query.Select(e => e.ID).ToList());
                    request.RecordCount = query.Count;

                    // Add record to fileexport table
                    IsSuccess = new FileExportDataAccess().AddFileExport(request);
                }

                if (IsSuccess)
                    return Json(new { IsSuccess = IsSuccess, Response = IsSuccess, ErrorMessage = string.Format(CustomMessages.ImportRecordsRetrieveAndExport, records == null ? 0 : records.Count, query == null ? 0 : query.Count) }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { IsSuccess = IsSuccess, ErrorMessage = CustomMessages.NoRecordFound, Response = IsSuccess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.PrintError(ex.Message, ex);
            }

            return Json(new { IsSuccess = true, Response = true, ErrorMessage = string.Format(CustomMessages.ImportRecordsRetrieveAndExport, records == null ? 0 : records.Count, query == null ? 0 : query.Count) }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult EncryptedFileExport(int? Id)
        {
            ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewBag.Branches = new RegionDataAccess().GetAllRegions().Where(e => e.IsActive == true).ToList();
            ViewBag.RequestId = Id ?? 0;
            return View();
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AddUpdateEncryptedFileExportNew(tbl_File_Exports request, string requestTypeName, string cardTypeName, DateTime? requestDate = null)
        {
            bool IsSuccess = false;
            List<RequestForExportVM> records = null;
            List<RequestForExportVM> query = null;
            DataTable data = new DataTable();
            var currDate = DateTime.Now;
            request.BranchCodes = string.IsNullOrEmpty(request.BranchCodes) ? "0000" : request.BranchCodes;

            // Create filename for export or re-export file 
            var outputFilename = (requestDate == null) ? $"{cardTypeName}{requestTypeName}{currDate.ToString(Constants.Formats.DateFormat)}{currDate.ToString(Constants.Formats.TimeFormat)}" : $"{cardTypeName}{requestTypeName}{currDate.ToString(Constants.Formats.DateFormat)}{currDate.ToString(Constants.Formats.TimeFormat)}(re-export {requestDate.GetValueOrDefault().ToString(Constants.Formats.DateFormat)})";
            //removing spaces fro name 10 jul 18
            outputFilename = outputFilename.ToString().Replace(" ", "").ToString();
            outputFilename = outputFilename + ".txt";
            try
            {
                FileExportViewModel model = new FileExportViewModel();
                string currentDate = DateTime.Now.ToString(Constants.Formats.DateFormat);
                var filePath = new FilePathDataAccess().GetFilePathByTypeID(((int)Constants.enPathType.Export).ToString());

                // Get request data
                if (requestDate == null)
                    data = new RequestDataAccess().GetRequestDataForExport(request.RequestTypes, int.Parse(request.CardTypes), request.BranchCodes);
                else
                    data = new RequestDataAccess().GetRequestDataForExport(request.RequestTypes, int.Parse(request.CardTypes), request.BranchCodes, requestDate);

                records = data.Rows.Count > 0 ? DAL.ReflectionHelper.CreateGenericListFromDataTable<RequestForExportVM>(data) : null;

                // Filter records for duplicate CNIC
                query = (records != null) ? records.GroupBy(e => string.IsNullOrEmpty(e.NIC) ? Guid.NewGuid().ToString() : e.NIC).Select(e => e.FirstOrDefault()).ToList() : null;

                if (query != null)
                {
                    string path = FileHelper.Create(filePath?.Path ?? @"C:\", outputFilename);
                    int dataCount = 1;
                    string CardProfileAction = (request.RequestTypes == Constants.RequestTypes.Issuance || request.RequestTypes == Constants.RequestTypes.Replacement) ? Constants.ActionCodes.Add : (request.RequestTypes == Constants.RequestTypes.Ammendment || request.RequestTypes == Constants.RequestTypes.Upgrade || request.RequestTypes == Constants.RequestTypes.Linking || request.RequestTypes == Constants.RequestTypes.Delinking || request.RequestTypes == Constants.RequestTypes.DefaultAccountUpdate) ? Constants.ActionCodes.Update : Constants.ActionCodes.Add;
                    string CustomerProfileAction = (request.RequestTypes == Constants.RequestTypes.Issuance || request.RequestTypes == Constants.RequestTypes.Replacement) ? Constants.ActionCodes.Add : (request.RequestTypes == Constants.RequestTypes.Ammendment || request.RequestTypes == Constants.RequestTypes.Upgrade || request.RequestTypes == Constants.RequestTypes.Linking || request.RequestTypes == Constants.RequestTypes.Delinking || request.RequestTypes == Constants.RequestTypes.DefaultAccountUpdate) ? Constants.ActionCodes.Update : Constants.ActionCodes.Add;
                    string AccountProfileAction = (request.RequestTypes == Constants.RequestTypes.Issuance || request.RequestTypes == Constants.RequestTypes.Replacement || request.RequestTypes == Constants.RequestTypes.Ammendment || request.RequestTypes == Constants.RequestTypes.Upgrade || request.RequestTypes == Constants.RequestTypes.Linking) ? Constants.ActionCodes.Add : (request.RequestTypes == Constants.RequestTypes.Delinking) ? Constants.ActionCodes.AccountDelink : Constants.RequestTypes.DefaultAccountUpdate;

                    // Create header
                    model.Header = new HeaderRecord() { Date = currentDate.LimitTo(8), FileName = "CRD".LimitTo(20), Version = "03.05.00".LimitTo(8) };

                    // Write header
                    FileHelper.WriteFile(path, new string[] { string.Join(string.Empty, model.Header.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(model.Header, null))) });

                    foreach (var requestDetail in query)
                    {
                        var accounts = new List<AccountDataNew>();
                        var account = new AccountDataNew();
                        var lstString = new List<string>();
                        dataCount++;

                        // Create data
                        var cData = GetCardDataNew(dataCount, requestDetail, CardProfileAction);
                        var profile = GetProfileDataNew(dataCount, requestDetail, CustomerProfileAction);

                        //If Request type is ammendment then we have to create new rows for linked or delinked accounts
                        if (request.RequestTypes == Constants.RequestTypes.Ammendment)
                        {
                            var getDefaultNLinkingAccounts = new CustomerAccountDataAccess().GetCustomerAccountByCardNo(requestDetail.CardNo);
                            // requestDetail.DefaultAccount = "O";//Default Account---Changes by Saad 15 March 2019 New IRIS
                            requestDetail.DefaultAccount = "1";//Default Account
                            accounts.Add(GetAccountDataNew(dataCount, requestDetail, request.BranchCodes, AccountProfileAction));
                            foreach (var item in getDefaultNLinkingAccounts.Where(e => e.AccountNo != requestDetail.AccountNo))
                            {
                                AccountSectionExtensionNew(ref accounts, item.AccountNo, item.AccountTitle, string.IsNullOrEmpty(requestDetail.AccountType) ? "10" : requestDetail.AccountType, requestDetail.BranchCode, request.RequestTypes, dataCount, requestDetail.Pan);
                            }
                        }

                        //If Request type is linking or delinking then we have to create new rows for linking or delinking accounts
                        else if (request.RequestTypes == Constants.RequestTypes.Linking || request.RequestTypes == Constants.RequestTypes.Delinking)
                        {
                            // requestDetail.DefaultAccount = "O";//Default Account---Changes by Saad 15 March 2019 New IRIS
                            requestDetail.DefaultAccount = "1";//Default Account
                            accounts.Add(GetAccountDataNew(dataCount, requestDetail, request.BranchCodes, AccountProfileAction));
                            foreach (var item in requestDetail.LinkingDelinkingAccount.Split(','))
                            {
                                var acc = GetCustomerAccountForLinkingAccounts(item);

                                AccountSectionExtensionForLinkingNew(ref accounts, item, acc.FirstOrDefault()?.AccountTitle ?? string.Empty, string.IsNullOrEmpty(requestDetail.AccountType) ? "10" : requestDetail.AccountType, requestDetail.BranchCode, request.RequestTypes, dataCount, requestDetail.Pan, acc.FirstOrDefault().Currency);
                            }
                        }
                        else
                            accounts.Add(GetAccountDataNew(dataCount, requestDetail, request.BranchCodes, AccountProfileAction));

                        //var relation = GetRelationData(dataCount, requestDetail, action);

                        // Write data
                        //FileHelper.WriteFile(path, new string[] { string.Join(string.Empty, cData.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(cData, null))), string.Join(string.Empty, profile.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(profile, null))), string.Join(string.Empty, account.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(account, null))), string.Join(string.Empty, relation.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(relation, null))) });

                        lstString.Add(string.Join(string.Empty, cData.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(cData, null))));
                        lstString.Add(string.Join(string.Empty, profile.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(profile, null))));
                        accounts.ForEach(e => lstString.Add(string.Join(string.Empty, e.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(e, null)))));
                        //lstString.Add(string.Join(string.Empty, relation.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(relation, null))));

                        FileHelper.WriteFile(path, lstString.ToArray());
                    }

                    // Create footer
                    string datacount = (query.Count + 2).ToString();
                    //model.Footer = new FooterRecord() { Date = currentDate.LimitTo(8), FileName = outputFilename.Replace(".txt", string.Empty).LimitTo(20), RecordNumber = datacount.PadLeft(6, '0') };
                    model.Footer = new FooterRecord() { Date = currentDate.LimitTo(8), FileName = "CRD".LimitTo(20), RecordNumber = datacount.PadLeft(6, '0'), EndSentinel = "." };

                    // Write footer
                    FileHelper.WriteFile(path, new string[] { string.Join(string.Empty, model.Footer.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(model.Footer, null))) });

                    // Encrypt file
                    outputFilename = (requestDate == null) ? $"{cardTypeName} {requestTypeName} {currDate.ToString(Constants.Formats.DateFormat)} {currDate.ToString(Constants.Formats.TimeFormat)}" : $"{cardTypeName} {requestTypeName} {currDate.ToString(Constants.Formats.DateFormat)} {currDate.ToString(Constants.Formats.TimeFormat)} (re-export {requestDate.GetValueOrDefault().ToString(Constants.Formats.DateFormat)})";
                    outputFilename = outputFilename.ToString().Replace(" ", "").ToString();
                  
                    // Encrypt file
                    new GNUPG_Wrapper().EncryptBatch(path, outputFilename);

                    // Update file export record
                    new RequestDataAccess().UpdateRequestAfterExport(query.Select(e => e.ID).ToList());
                    request.RecordCount = query.Count;

                    // Add record to fileexport table
                    IsSuccess = new FileExportDataAccess().AddFileExport(request);
                }

                if (IsSuccess)
                    return Json(new { IsSuccess = IsSuccess, Response = IsSuccess, ErrorMessage = string.Format(CustomMessages.ImportRecordsRetrieveAndExport, records == null ? 0 : records.Count, query == null ? 0 : query.Count) }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { IsSuccess = IsSuccess, ErrorMessage = CustomMessages.NoRecordFound, Response = IsSuccess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.PrintError(ex.Message, ex);
            }

            return Json(new { IsSuccess = true, Response = true, ErrorMessage = string.Format(CustomMessages.ImportRecordsRetrieveAndExport, records == null ? 0 : records.Count, query == null ? 0 : query.Count) }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Card Details For Export
        /// </summary>
        /// <param name="dataCount"></param>
        /// <param name="requestDetail"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private CardData GetCardData(int dataCount, RequestForExportVM requestDetail, string action)
        {
            string address = string.IsNullOrEmpty(requestDetail.AssociatedAddress) ? requestDetail.AssociatedAddress : requestDetail.AssociatedAddress.Insert(requestDetail.AssociatedAddress.Length, "$").Replace(" ", "*");
            return new CardData()
            {
                //ref: excel file for harcoded values
                RecordNumber = dataCount.ToString().PadLeft(6, '0'),
                ActionCode = action.LimitTo(2),
                RecordType = Constants.RecordTypes.CardData.LimitTo(2),
                TipperFlag = string.IsNullOrEmpty(requestDetail.TipperFlag) ? "1".LimitTo(1) : requestDetail.TipperFlag.LimitTo(1), //1 for silver, 2 for gold, to be check later
                PrisecCode = string.IsNullOrEmpty(requestDetail.PriSec) ? "0".LimitTo(1) : requestDetail.PriSec.LimitTo(1), //0 for primary, 1 for secondary, to be check later
                PAN = requestDetail.Pan.LimitTo(20),
                ProcessingCode = "02".LimitTo(2), // 
                CycleBeginDate = DateTime.Now.ToString(Constants.Formats.DateFormat).LimitTo(8),
                CycleLength = "01".LimitTo(2),
                CycleAmount = "0000000000000".LimitTo(13),
                AmountRemaining = "0000000000000".LimitTo(13),
                CardStatus = "00".LimitTo(2),
                CardExpiryDate = requestDetail.DateOfExpiry.ToString(Constants.Formats.DateFormat).LimitTo(8),
                EmbossingName = requestDetail.NameOnCard.LimitTo(30),
                Address1 = GetAddress(ref address, 30), //requestDetail.AssociatedAddress.LimitTo(30),
                Address2 = GetAddress(ref address, 30),//requestDetail.Address.LimitTo(30),
                Address3 = GetAddress(ref address, 30),//string.Empty.LimitTo(30),
                Address4 = GetAddress(ref address, 30),//string.Empty.LimitTo(30),
                PinDate = requestDetail.DateOfExpiry.ToString(Constants.Formats.DateFormat).LimitTo(8),
                PurchaseLimitCycleBeginDate = DateTime.Now.ToString(Constants.Formats.DateFormat).LimitTo(8),
                PurchaseLimitCycleLength = "01".LimitTo(2),
                PurchaseLimitCycleAmount = "0000000000000".LimitTo(13),
                PurchaseLimitAmountRemaining = "0000000000000".LimitTo(13),
                AllowSrcATM = "Y".LimitTo(1),
                AllowSrcCIRRUS = "Y".LimitTo(1),
                AllowSrcPHXSW = "Y".LimitTo(1),
                AllowSrcEFTPOS = "Y".LimitTo(1),
                AllowSrcVISA = "Y".LimitTo(1),
                AllowSrcAMEX = "Y".LimitTo(1),
                AllowSrcMNET = "Y".LimitTo(1),
                AllowSrcIVR = "Y".LimitTo(1),
                AllowSrcMobile = "Y".LimitTo(1),
                AllowSrcCardPro = "Y".LimitTo(1),
                AllowSrcCardPCASH = "Y".LimitTo(1),
                AllowSrcCardIB = "Y".LimitTo(1),
                FreqBalInq = "999".LimitTo(3),
                FreqWdwl = "999".LimitTo(3),
                FreqPurchase = "999".LimitTo(3),
                FreqMiniStmt = "999".LimitTo(3),
                FrequencyCycleBeginDate = DateTime.Now.ToString(Constants.Formats.DateFormat).LimitTo(8),
                FrequencyLimitCycleLength = "01".LimitTo(2),
                FundsTransferLimitCycleBeginDate = DateTime.Now.ToString(Constants.Formats.DateFormat).LimitTo(8),
                FundsTransferLimitCycleLength = "01".LimitTo(2),
                FundsTransferLimitCycleAmount = "0000000000000".LimitTo(13),
                FundsTransferLimitAmountRemaining = "0000000000000".LimitTo(13),
                Customer0rStaff = "0".LimitTo(1),
                PcashLimitCycleBeginDate = DateTime.Now.ToString(Constants.Formats.DateFormat).LimitTo(8),
                PcashLimitCycleLength = "01".LimitTo(2),
                PcashLimitCycleAmount = "0000000000000".LimitTo(13),
                PcashLimitAmountRemaining = "0000000000000".LimitTo(13)
            };
        }
      

        /// <summary>
        /// Get Customer Details For File Export
        /// </summary>
        /// <param name="dataCount"></param>
        /// <param name="requestDetail"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private CustomerProfile GetProfileData(int dataCount, RequestForExportVM requestDetail, string action)
        {
            return new CustomerProfile()
            {
                //ref: excel file for harcoded values
                RecordType = Constants.RecordTypes.CustomerProfile.LimitTo(2),
                ActionCode = action.LimitTo(2),
                RecordNumber = dataCount.ToString().PadLeft(6, '0'),
                OldNIC = string.Empty.LimitTo(20),
                Title = requestDetail.Salutation.LimitTo(3),
                FullName = requestDetail.NameOnCard.LimitTo(40),
                DateOfBirth = requestDetail.DateOfBirth.ToString(Constants.Formats.DateFormat).LimitTo(8),
                MothersMaidenName = requestDetail.MotherMaidenName.LimitTo(40),
                ResidentialPostalCode = string.Empty.LimitTo(10),
                ResidentialPhoneNumber = requestDetail.ResidentialPhoneNumber.LimitTo(15),
                EmailAddress = requestDetail.Email.LimitTo(40),
                StatusForPIN1 = requestDetail.StatusForPIN1.LimitTo(2),
                StatusForPIN2 = requestDetail.StatusForPIN2.LimitTo(2),
                StatusForPIN3 = requestDetail.StatusForPIN3.LimitTo(2),
                StatusForPIN4 = requestDetail.StatusForPIN4.LimitTo(2),
                PIN4IB = requestDetail.PIN4IB.LimitTo(32),
                CompanyName = requestDetail.CompanyName.LimitTo(40),
                // OfficeAddress1 = requestDetail.OfficeAddress1.LimitTo(40),
                //OfficeAddress2 = requestDetail.OfficeAddress2.LimitTo(40),
                 OfficeAddress1 = string.Empty.LimitTo(40),
                OfficeAddress2 = string.Empty.LimitTo(40),
                OfficeAddress3 = string.Empty.LimitTo(40),
                OfficeAddress4 = string.Empty.LimitTo(40),
                OfficeAddress5 = string.Empty.LimitTo(40),
                OfficePostalCode = string.Empty.LimitTo(10),
                OfficePhoneNumber = requestDetail.PhoneOff.LimitTo(15),
                MobileNumber = requestDetail.MobileNumber.LimitTo(15),
                CustomerNumber = requestDetail.CustomerNumber.LimitTo(20),
                NewNIC = requestDetail.NIC.LimitTo(20),
                BillingAddressFlag = requestDetail.BillingAddressFlag.LimitTo(1),
                AnniversaryDate = requestDetail.MemberSince.ToString(Constants.Formats.DateFormat).LimitTo(8),
                CustomerOf = requestDetail.CustomerOf.LimitTo(40),
                PassportNo = requestDetail.Passport.LimitTo(10),
                Nationality = requestDetail.Nationality.LimitTo(30),
                IBuID = string.Empty.LimitTo(20)
            };
        }


        /// <summary>
        /// Get Account Details For File Export
        /// </summary>
        /// <param name="dataCount"></param>
        /// <param name="requestDetail"></param>
        /// <param name="branchCode"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private AccountData GetAccountData(int dataCount, RequestForExportVM requestDetail, string branchCode, string action)
        {
            return new AccountData()
            {
                //ref: excel file for harcoded values
                RecordType = Constants.RecordTypes.AccountData.LimitTo(2),
                ActionCode = action.LimitTo(2),
                RecordNumber = dataCount.ToString().PadLeft(6, '0'),
                AccountNo = requestDetail.AccountNo.LimitTo(20),
                AccountType = string.IsNullOrEmpty(requestDetail.AccountType) ? "10".LimitTo(2) : requestDetail.AccountType.LimitTo(2), // 10 is saving, 20 is current, to be check later
                Currency = !string.IsNullOrEmpty(requestDetail.Currency) ? requestDetail.Currency.LimitTo(3) : "586".LimitTo(3), // currency in pkr, to be check later
                AccountTitle = requestDetail.NameOnCard.LimitTo(30),
                BankIMD = requestDetail.BankIMD.LimitTo(6),
                BranchCode = requestDetail.BranchCode.LimitTo(4),
                AccountNature = "0".LimitTo(1),
                DefaultAccount = requestDetail.DefaultAccount.LimitTo(1),
                Withdrawal = "Y".LimitTo(1),
                Purchase = "Y".LimitTo(1),
                TransferTo = "Y".LimitTo(1),
                TransferFrom = "Y".LimitTo(1),
                MiniStatement = "Y".LimitTo(1),
                BalanceInquiry = "Y".LimitTo(1),
                ChqBookReq = "Y".LimitTo(1),
                StmtReq = "Y".LimitTo(1),
                PayBill = "Y".LimitTo(1),
                ChequeDeposit = "N".LimitTo(1),
                CashDeposit = "N".LimitTo(1),
                SecondCurrency = "N".LimitTo(1),
                AccountMap = string.Empty.LimitTo(15),
                ProcessingCode = "02".LimitTo(2) //same as that in card record
            };
        }


        /// <summary>
        /// Get Accounts Details For File Export (Especially For Linking)
        /// </summary>
        /// <param name="dataCount"></param>
        /// <param name="requestDetail"></param>
        /// <param name="Pan"></param>
        /// <param name="branchCode"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private AccountData GetAccountData(int dataCount, AccountData requestDetail, string Pan, string branchCode, string action)
        {
            return new AccountData()
            {
                //ref: excel file for harcoded values
                RecordType = Constants.RecordTypes.AccountData.LimitTo(2),
                ActionCode = action.LimitTo(2),
                RecordNumber = dataCount.ToString().PadLeft(6, '0'),
                AccountNo = requestDetail.AccountNo.LimitTo(20),
                AccountType = requestDetail.AccountType.LimitTo(2), // 10 is saving, 20 is current, to be check later
                Currency = requestDetail.Currency.LimitTo(3),//"586".LimitTo(3), // currency in pkr, to be check later
                AccountTitle = requestDetail.AccountTitle.LimitTo(30),
                BankIMD = Pan.LimitTo(6),
                BranchCode = requestDetail.BranchCode.LimitTo(4),
                AccountNature = "0".LimitTo(1),
                DefaultAccount = requestDetail.DefaultAccount.LimitTo(1),
                Withdrawal = "Y".LimitTo(1),
                Purchase = "Y".LimitTo(1),
                TransferTo = "Y".LimitTo(1),
                TransferFrom = "Y".LimitTo(1),
                MiniStatement = "Y".LimitTo(1),
                BalanceInquiry = "Y".LimitTo(1),
                ChqBookReq = "Y".LimitTo(1),
                StmtReq = "Y".LimitTo(1),
                PayBill = "Y".LimitTo(1),
                ChequeDeposit = "N".LimitTo(1),
                CashDeposit = "N".LimitTo(1),
                SecondCurrency = "N".LimitTo(1),
                AccountMap = string.Empty.LimitTo(15),
                ProcessingCode = "02".LimitTo(2) //same as that in card record
            };
        }

        private AccountDataNew GetAccountDataNew(int dataCount, AccountDataNew requestDetail, string Pan, string branchCode, string action)
        {
            return new AccountDataNew()
            {
                //ref: excel file for harcoded values
                RecordType = Constants.RecordTypes.AccountData.LimitTo(2),
                ActionCode = action.LimitTo(2),
                RecordNumber = dataCount.ToString().PadLeft(6, '0'),
                AccountNo = requestDetail.AccountNo.LimitTo(20),
                AccountType = requestDetail.AccountType.LimitTo(2), // 10 is saving, 20 is current, to be check later
                Currency = requestDetail.Currency.LimitTo(3),//"586".LimitTo(3), // currency in pkr, to be check later
                AccountTitle = requestDetail.AccountTitle.LimitTo(30),
                BankIMD = Pan.LimitTo(6),
                BranchCode = requestDetail.BranchCode.LimitTo(4),
                AccountNature = "0".LimitTo(1),
                DefaultAccount = requestDetail.DefaultAccount.LimitTo(1),
            };
        }


        /// <summary>
        /// Get Relationship Details For File Export
        /// </summary>
        /// <param name="dataCount"></param>
        /// <param name="requestDetail"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private CustomerRelationship GetRelationData(int dataCount, RequestForExportVM requestDetail, string action)
        {
            return new CustomerRelationship()
            {
                RecordType = Constants.RecordTypes.CustomerRelationship.LimitTo(2),
                ActionCode = action.LimitTo(2),
                RecordNumber = dataCount.ToString().PadLeft(6, '0'),
                MobileNo = requestDetail.MobileNumber.LimitTo(20),
                NICNew = requestDetail.NIC.LimitTo(20),
                NICOld = string.Empty.LimitTo(20)
            };
        }

        public ActionResult EncryptDecryptTest()
        {
            var filePath = new FilePathDataAccess().GetFilePathByTypeID(((int)Constants.enPathType.Import).ToString());
            var viewModel = FileHelper.GetFileNamesFromDirectoryV1(filePath.Path);
            return View(viewModel);
        }
        public ActionResult Encrypt(string filename)
        {
            var path = @"D:\ImportExportCards\";
            var outputFilename = $"DEMO{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}.txt";
            // Encrypt file
            new GNUPG_Wrapper().EncryptBatch(Path.Combine(path, filename), outputFilename);
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public ActionResult Decrypt(string filename)
        {
            var path = @"D:\ImportExportCards\";
            var outputFilename = $"DEMO{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}.txt";
            // Encrypt file
            new GNUPG_Wrapper().DecryptyWithoutPassBatch(Path.Combine(path, filename), outputFilename);
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        [Obsolete]
        private List<RequestForExportVM> GetProcessedData(List<RequestForExportVM> request)
        {
            // Get data of request type "Linking" Or "Delinking" and having csv of account number
            var data = request.Where(e => (e.RequestType == "L" || e.RequestType == "D") && e.AccountNo.Contains(','));

            // Split data into multiple rows based on account number csv
            var grouped = (from r in data from fg in r.AccountNo.Split(',') select new { data = r, Group = (string.IsNullOrEmpty(fg)) ? "NA" : fg }).Where(e => e.Group != "NA").ToList();

            // Update account number of each record from splited value.
            grouped.ForEach(e => e.data.AccountNo = e.Group);

            // Remove all data from request lst that has been cloned or having splited account number value
            request.RemoveAll(e => (e.RequestType == "L" || e.RequestType == "D") && e.AccountNo.Contains(','));

            // Add grouped data to request lst
            request.AddRange(grouped.Select(e => e.data));

            return request;
        }

        /// <summary>
        /// Account section extension, use for linking/delinking/ammendment requests,
        /// as the IRIS wants multiple account rows for above requests.
        /// </summary>
        /// <param name="accounts"></param>
        /// <param name="accountNo"></param>
        /// <param name="accountTitle"></param>
        /// <param name="accountType"></param>
        /// <param name="branchCode"></param>
        /// <param name="requestType"></param>
        /// <param name="dataCount"></param>
        /// <param name="pan"></param>
        private void AccountSectionExtension(ref List<AccountData>accounts, string accountNo, string accountTitle, string accountType, string branchCode, string requestType, int dataCount, string pan)
        {
            var accDetail = new AccountData()
            {
                AccountNo = accountNo,
                AccountTitle = accountTitle,
                DefaultAccount = "Y",//Other Accounts
                AccountType = accountType,
                BranchCode = branchCode
             
            };

            var lkngAction = requestType == Constants.RequestTypes.Delinking ? Constants.ActionCodes.Remove : requestType == Constants.RequestTypes.Ammendment ? Constants.ActionCodes.Update : Constants.ActionCodes.Add;

            var account = GetAccountData(dataCount, accDetail, pan, branchCode, lkngAction);
            accounts.Add(account);
        }


        private void AccountSectionExtensionForLinking(ref List<AccountData> accounts, string accountNo, string accountTitle, string accountType, string branchCode, string requestType, int dataCount, string pan, string Currency)
        {
            var accDetail = new AccountData()
            {
                AccountNo = accountNo,
                AccountTitle = accountTitle,
                DefaultAccount = "Y",//Other Accounts
                AccountType = accountType,
                BranchCode = branchCode,
                Currency = Currency
            };

            var lkngAction = requestType == Constants.RequestTypes.Delinking ? Constants.ActionCodes.Remove : requestType == Constants.RequestTypes.Ammendment ? Constants.ActionCodes.Update : Constants.ActionCodes.Add;

            var account = GetAccountData(dataCount, accDetail, pan, branchCode, lkngAction);
            accounts.Add(account);
        }



        private static string GetAddress(ref string address, int length)
        {
            if (string.IsNullOrEmpty(address))
                return string.Empty.LimitTo(length);
            else if (address.IndexOf("$") <= length)
            {
                var addr = address.Replace("*", " ").Replace("$", string.Empty).LimitTo(length);
                address = string.Empty;
                return addr;
            }
            else
            {
                IEnumerable<int> result = address.AllIndexesOf("*");
                var index = result.Where(e => e <= length).LastOrDefault();
                var addr = address.Substring(0, index).Replace("*", " ").LimitTo(length);
                address = address.Substring(index, (address.Length - index));
                return addr;
            }
        }


        #region Plain Export
        public ActionResult PlainExport(int? Id)
        {
            ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewBag.Branches = new RegionDataAccess().GetAllRegions().Where(e => e.IsActive == true).ToList();
            ViewBag.RequestId = Id ?? 0;
            return View();
        }


      
        public ActionResult AddUpdateFilePlainExportNew(tbl_File_Exports request, string requestTypeName, string cardTypeName, DateTime? requestDate = null)
        {
            bool IsSuccess = false;
            List<RequestForExportVM> records = null;
            List<RequestForExportVM> query = null;
            DataTable data = new DataTable();
            var currDate = DateTime.Now;
            request.BranchCodes = string.IsNullOrEmpty(request.BranchCodes) ? "0000" : request.BranchCodes;

            // Create filename for export or re-export file 
            var outputFilename = (requestDate == null) ? $"{cardTypeName} {requestTypeName} {currDate.ToString(Constants.Formats.DateFormat)} {currDate.ToString(Constants.Formats.TimeFormat)}" : $"{cardTypeName} {requestTypeName} {currDate.ToString(Constants.Formats.DateFormat)} {currDate.ToString(Constants.Formats.TimeFormat)} (re-export {requestDate.GetValueOrDefault().ToString(Constants.Formats.DateFormat)})";
            outputFilename = outputFilename.ToString().Replace(" ", "").ToString();
            outputFilename = outputFilename + ".txt";
            try
            {
                FileExportViewModel model = new FileExportViewModel();
                string currentDate = DateTime.Now.ToString(Constants.Formats.DateFormat);
                var filePath = new FilePathDataAccess().GetFilePathByTypeID(((int)Constants.enPathType.Export).ToString());

                // Get request data
                if (requestDate == null)
                    data = new RequestDataAccess().GetRequestDataForExport(request.RequestTypes, int.Parse(request.CardTypes), request.BranchCodes);
                else
                    data = new RequestDataAccess().GetRequestDataForPlainExport(request.RequestTypes, int.Parse(request.CardTypes), request.BranchCodes, requestDate);

                records = data.Rows.Count > 0 ? DAL.ReflectionHelper.CreateGenericListFromDataTable<RequestForExportVM>(data) : null;

                // Filter records for duplicate CNIC
                query = (records != null) ? records.GroupBy(e => string.IsNullOrEmpty(e.NIC) ? Guid.NewGuid().ToString() : e.NIC).Select(e => e.FirstOrDefault()).ToList() : null;

                if (query != null)
                {
                    string path = FileHelper.Create(filePath?.Path ?? @"C:\", outputFilename);
                    int dataCount = 1;
                    string CardProfileAction = (request.RequestTypes == Constants.RequestTypes.Issuance || request.RequestTypes == Constants.RequestTypes.Replacement) ? Constants.ActionCodes.Add : (request.RequestTypes == Constants.RequestTypes.Ammendment || request.RequestTypes == Constants.RequestTypes.Upgrade || request.RequestTypes == Constants.RequestTypes.Linking || request.RequestTypes == Constants.RequestTypes.Delinking|| request.RequestTypes==Constants.RequestTypes.DefaultAccountUpdate) ? Constants.ActionCodes.Update : Constants.ActionCodes.Add;
                    string CustomerProfileAction = (request.RequestTypes == Constants.RequestTypes.Issuance || request.RequestTypes == Constants.RequestTypes.Replacement) ? Constants.ActionCodes.Add : (request.RequestTypes == Constants.RequestTypes.Ammendment || request.RequestTypes == Constants.RequestTypes.Upgrade || request.RequestTypes == Constants.RequestTypes.Linking || request.RequestTypes == Constants.RequestTypes.Delinking || request.RequestTypes == Constants.RequestTypes.DefaultAccountUpdate) ? Constants.ActionCodes.Update : Constants.ActionCodes.Add;
                    string AccountProfileAction = (request.RequestTypes == Constants.RequestTypes.Issuance || request.RequestTypes == Constants.RequestTypes.Replacement || request.RequestTypes == Constants.RequestTypes.Ammendment || request.RequestTypes == Constants.RequestTypes.Upgrade || request.RequestTypes == Constants.RequestTypes.Linking) ? Constants.ActionCodes.Add : (request.RequestTypes == Constants.RequestTypes.Delinking) ? Constants.ActionCodes.AccountDelink : Constants.RequestTypes.DefaultAccountUpdate;
                    // Create header
                    model.Header = new HeaderRecord() { Date = currentDate.LimitTo(8), FileName = "CRD".LimitTo(20), Version = "03.05.00".LimitTo(8) };

                    // Write header
                    FileHelper.WriteFile(path, new string[] { string.Join(string.Empty, model.Header.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(model.Header, null))) });

                    foreach (var requestDetail in query)
                    {
                        var accounts = new List<AccountDataNew>();
                        var account = new AccountDataNew();
                        var lstString = new List<string>();
                        dataCount++;

                        // Create data
                        var cData = GetCardDataNew(dataCount, requestDetail, CardProfileAction);
                        var profile = GetProfileDataNew(dataCount, requestDetail, CustomerProfileAction);

                        //If Request type is ammendment then we have to create new rows for linked or delinked accounts
                        if (request.RequestTypes == Constants.RequestTypes.Ammendment)
                        {
                            var getDefaultNLinkingAccounts = new CustomerAccountDataAccess().GetCustomerAccountByCardNo(requestDetail.CardNo);
                            // requestDetail.DefaultAccount = "O";//Default Account---Changes by Saad 15 March 2019 New IRIS
                            requestDetail.DefaultAccount = "1";//Default Account
                            accounts.Add(GetAccountDataNew(dataCount, requestDetail, request.BranchCodes, AccountProfileAction));
                            foreach (var item in getDefaultNLinkingAccounts.Where(e => e.AccountNo != requestDetail.AccountNo))
                            {
                                AccountSectionExtensionNew(ref accounts, item.AccountNo, item.AccountTitle, string.IsNullOrEmpty(requestDetail.AccountType) ? "10" : requestDetail.AccountType, requestDetail.BranchCode, request.RequestTypes, dataCount, requestDetail.Pan);
                            }
                        }

                        //If Request type is linking or delinking then we have to create new rows for linking or delinking accounts
                        else if (request.RequestTypes == Constants.RequestTypes.Linking || request.RequestTypes == Constants.RequestTypes.Delinking)
                        {
                            // requestDetail.DefaultAccount = "O";//Default Account---Changes by Saad 15 March 2019 New IRIS
                            requestDetail.DefaultAccount = "1";//Default Account
                            accounts.Add(GetAccountDataNew(dataCount, requestDetail, request.BranchCodes, AccountProfileAction));
                            foreach (var item in requestDetail.LinkingDelinkingAccount.Split(','))
                            {
                                var acc = GetCustomerAccountForLinkingAccounts(item);

                                AccountSectionExtensionForLinkingNew(ref accounts, item, acc.FirstOrDefault()?.AccountTitle ?? string.Empty, string.IsNullOrEmpty(requestDetail.AccountType) ? "10" : requestDetail.AccountType, requestDetail.BranchCode, request.RequestTypes, dataCount, requestDetail.Pan, acc.FirstOrDefault().Currency);
                            }
                        }
                        else
                            accounts.Add(GetAccountDataNew(dataCount, requestDetail, request.BranchCodes, AccountProfileAction));

                       // var relation = GetRelationData(dataCount, requestDetail, action);

                        // Write data
                        //FileHelper.WriteFile(path, new string[] { string.Join(string.Empty, cData.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(cData, null))), string.Join(string.Empty, profile.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(profile, null))), string.Join(string.Empty, account.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(account, null))), string.Join(string.Empty, relation.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(relation, null))) });
                        if (request.RequestTypes == Constants.RequestTypes.Ammendment)
                        {
                            
                            lstString.Add(string.Join(string.Empty, profile.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(profile, null))));
                            
                        }
                        else
                        {
                            lstString.Add(string.Join(string.Empty, cData.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(cData, null))));
                            lstString.Add(string.Join(string.Empty, profile.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(profile, null))));
                            accounts.ForEach(e => lstString.Add(string.Join(string.Empty, e.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(e, null)))));
                            // lstString.Add(string.Join(string.Empty, relation.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(relation, null))));
                        }


                        FileHelper.WriteFile(path, lstString.ToArray());
                    }

                    // Create footer
                    string datacount = (query.Count + 2).ToString();
                    //model.Footer = new FooterRecord() { Date = currentDate.LimitTo(8), FileName = outputFilename.Replace(".txt", string.Empty).LimitTo(20), RecordNumber = datacount.PadLeft(6, '0') };
                    model.Footer = new FooterRecord() { Date = currentDate.LimitTo(8), FileName = "CRD".LimitTo(20), RecordNumber = datacount.PadLeft(6, '0'),EndSentinel="." };

                    // Write footer
                    FileHelper.WriteFile(path, new string[] { string.Join(string.Empty, model.Footer.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(model.Footer, null))) });

                    // Encrypt file [remove b/c we don't want encrypted file as it works on other action]
                    //new GNUPG_Wrapper().Encrypt(path, outputFilename);

                    // Update file export record
                    new RequestDataAccess().UpdateRequestAfterExport(query.Select(e => e.ID).ToList());
                    request.RecordCount = query.Count;

                    // Add record to fileexport table
                    IsSuccess = new FileExportDataAccess().AddFileExport(request);
                }

                if (IsSuccess)
                    return Json(new { IsSuccess = IsSuccess, Response = IsSuccess, ErrorMessage = string.Format(CustomMessages.ImportRecordsRetrieveAndExport, records == null ? 0 : records.Count, query == null ? 0 : query.Count) }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { IsSuccess = IsSuccess, ErrorMessage = CustomMessages.NoRecordFound, Response = IsSuccess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.PrintError(ex.Message, ex);
            }

            return Json(new { IsSuccess = true, Response = true, ErrorMessage = string.Format(CustomMessages.ImportRecordsRetrieveAndExport, records == null ? 0 : records.Count, query == null ? 0 : query.Count) }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        public List<CustomerAccountVM> GetCustomerAccountForLinkingAccounts(string AccountNo)
        {
            try
            {
                List<CustomerAccountVM> lst = new List<CustomerAccountVM>();

                using (var db = new SoneriCISEntities())
                {
                    //lst = db.tbl_Customer_Accounts.Where(a => a.AccountNo == AccountNo) new { }.ToList();
                    lst = (from a in db.tbl_Customer_Accounts
                           join c in db.tbl_Currency on a.Currency equals c.ShortName
                           where a.AccountNo == AccountNo
                           select new CustomerAccountVM
                           {
                               AccountNo = a.AccountNo,
                               CIF = a.CIF,
                               CardNo = a.CardNo,
                               AccountStatusActive = a.AccountStatusActive,
                               Salutation = a.Salutation,
                               AccountTitle = a.AccountTitle,
                               Address = a.Address,
                               Mobile = a.Mobile,
                               DateofBirth = a.DateofBirth,
                               MotherMaidenName = a.MotherMaidenName,
                               Identification = a.Identification,
                               CNIC = a.CNIC,
                               AddressType = a.AddressType,
                               AccountTypeID = a.AccountTypeID,
                               WaiveCharges = a.WaiveCharges,
                               PassportNo = a.PassportNo,
                               LandlineNo = a.LandlineNo,
                               Email = a.Email,
                               Nationality = a.Nationality,
                               AccountCategoryCode = a.AccountCategoryCode,
                               PhoneOffice = a.PhoneOffice,
                               Company = a.Company,
                               IdentificationType = a.IdentificationType,
                               Mobile2 = a.Mobile2,
                               Address2 = a.Address2,
                               Address3 = a.Address3,
                               MainMobile = a.MainMobile,
                               MainLandline = a.MainLandline,
                               MainAddress = a.MainAddress,
                               CustomerName = a.CustomerName,
                               FatherName = a.FatherName,
                               ResidenceStatus = a.ResidenceStatus,
                               Currency = c.Code,
                               Mobile3 = a.Mobile3,
                               Mobile4 = a.Mobile4

                           }).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        #region New Changes for IRIS 15 March 2019
        private CardDataNew GetCardDataNew(int dataCount, RequestForExportVM requestDetail, string action)
        {
            string address = string.IsNullOrEmpty(requestDetail.AssociatedAddress) ? requestDetail.AssociatedAddress : requestDetail.AssociatedAddress.Insert(requestDetail.AssociatedAddress.Length, "$").Replace(" ", "*");
            string CustomerNIC = string.IsNullOrEmpty(requestDetail.NIC) ? string.Empty.LimitTo(20) : requestDetail.NIC.Replace("-", "").LimitTo(20);
            return new CardDataNew()
            {
                //ref: excel file for harcoded values
                RecordNumber = dataCount.ToString().PadLeft(6, '0'),
                RecordType = Constants.RecordTypes.CardData.LimitTo(2),
                ActionCode = action.LimitTo(2),
                CustomerId = CustomerNIC.LimitTo(20),
                EmbossingName = requestDetail.NameOnCard.LimitTo(30),
                Customer0rStaff = "1".LimitTo(1),
                PrimaryCNIC = CustomerNIC.LimitTo(20),
                ProductCode = string.IsNullOrEmpty(requestDetail.TipperFlag) ? "1".LimitTo(4) : requestDetail.TipperFlag.LimitTo(4), //1 for silver, 2 for gold, to be check later
                IssuanceType = "00".LimitTo(2)

            };
        }

        private CustomerProfileNew GetProfileDataNew(int dataCount, RequestForExportVM requestDetail, string action)
        {
            string CustomerNIC = string.IsNullOrEmpty(requestDetail.NIC) ? string.Empty.LimitTo(20) : requestDetail.NIC.Replace("-", "").LimitTo(20);
            return new CustomerProfileNew()
            {
                //ref: excel file for harcoded values
                RecordNumber = dataCount.ToString().PadLeft(6, '0'),
                RecordType = Constants.RecordTypes.CustomerProfile.LimitTo(2),
                ActionCode = action.LimitTo(2),
                CustomerId = CustomerNIC,
                Title = requestDetail.Salutation.LimitTo(3),
                FullName = requestDetail.NameOnCard.LimitTo(40),
                DateOfBirth = requestDetail.DateOfBirth.ToString(Constants.Formats.DateFormat).LimitTo(8),
                MothersMaidenName = requestDetail.MotherMaidenName.LimitTo(40),
                ResidentialAddress = requestDetail.AssociatedAddress.LimitTo(35),
                ResidentialAddress2 = requestDetail.AssociatedAddress.LimitTo(35),
                ResidentialAddress3 = requestDetail.AssociatedAddress.LimitTo(35),
                ResidentialAddress4 = requestDetail.AssociatedAddress.LimitTo(35),

                ResidentialPostalCode = string.Empty.LimitTo(10),
                ResidentialPhoneNumber = requestDetail.ResidentialPhoneNumber.LimitTo(15),
                EmailAddress = requestDetail.Email.LimitTo(40),
                Reserved = string.Empty.LimitTo(32),
                CompanyName = string.IsNullOrEmpty(requestDetail.CompanyName) ? string.Empty.LimitTo(40) : requestDetail.CompanyName.LimitTo(40),
                OfficeAddress1 = string.Empty.LimitTo(40),
                OfficeAddress2 = string.Empty.LimitTo(40),
                OfficeAddress3 = string.Empty.LimitTo(40),
                OfficeAddress4 = string.Empty.LimitTo(40),
                OfficeAddress5 = string.Empty.LimitTo(40),
                OfficePostalCode = string.Empty.LimitTo(10),
                OfficePhoneNumber = requestDetail.PhoneOff.LimitTo(15),
                MobileNumber = requestDetail.MobileNumber.LimitTo(15),
                CustomerNumber = requestDetail.CustomerNumber.LimitTo(20),
                BillingAddressFlag = requestDetail.BillingAddressFlag.LimitTo(1),
                AnniversaryDate = requestDetail.MemberSince.ToString(Constants.Formats.DateFormat).LimitTo(8),
                PassportNo = requestDetail.Passport.LimitTo(20),
                Nationality = requestDetail.Nationality.LimitTo(30),
                Reserved3 = string.Empty.LimitTo(36),
                Reserved4 = string.Empty.LimitTo(40),
                FatherName = requestDetail.FatherName.LimitTo(40),
                EndSentinel = ".".LimitTo(1)
            };
        }

        private AccountDataNew GetAccountDataNew(int dataCount, RequestForExportVM requestDetail, string branchCode, string action)
        {
            string CustomerNIC = string.IsNullOrEmpty(requestDetail.NIC) ? string.Empty.LimitTo(20) : requestDetail.NIC.Replace("-", "").LimitTo(20);
            return new AccountDataNew()
            {
                //ref: excel file for harcoded values
                RecordNumber = dataCount.ToString().PadLeft(6, '0'),
                RecordType = Constants.RecordTypes.AccountData.LimitTo(2),
                ActionCode = action.LimitTo(2),
                CustomerId = CustomerNIC,
                AccountNo = requestDetail.AccountNo.LimitTo(20),
                AccountType = string.IsNullOrEmpty(requestDetail.AccountType) ? "10".LimitTo(2) : requestDetail.AccountType.LimitTo(2), // 10 is saving, 20 is current, to be check later
                Currency = !string.IsNullOrEmpty(requestDetail.Currency) ? requestDetail.Currency.LimitTo(3) : "586".LimitTo(3), // currency in pkr, to be check later
                AccountStatus = requestDetail.AccountStatus.LimitTo(2),
                AccountTitle = requestDetail.NameOnCard.LimitTo(30),
                BankIMD = requestDetail.BankIMD.LimitTo(6),
                BranchCode = requestDetail.BranchCode.LimitTo(4),
                DefaultAccount = requestDetail.DefaultAccount.LimitTo(1)

            };
        }

        private void AccountSectionExtensionNew(ref List<AccountDataNew> accounts, string accountNo, string accountTitle, string accountType, string branchCode, string requestType, int dataCount, string pan)
        {
            var accDetail = new AccountDataNew()
            {
                AccountNo = accountNo,
                AccountTitle = accountTitle,
                DefaultAccount = "0",//Other Accounts
                AccountType = accountType,
                BranchCode = branchCode

            };

            var lkngAction = requestType == Constants.RequestTypes.Delinking ? Constants.ActionCodes.Remove : requestType == Constants.RequestTypes.Ammendment ? Constants.ActionCodes.Update : Constants.ActionCodes.Add;

            var account = GetAccountDataNew(dataCount, accDetail, pan, branchCode, lkngAction);
            accounts.Add(account);
        }


        private void AccountSectionExtensionForLinkingNew(ref List<AccountDataNew> accounts, string accountNo, string accountTitle, string accountType, string branchCode, string requestType, int dataCount, string pan, string Currency)
        {
            var accDetail = new AccountDataNew()
            {
                AccountNo = accountNo,
                AccountTitle = accountTitle,
                DefaultAccount = "0",//Other Accounts
                AccountType = accountType,
                BranchCode = branchCode,
                Currency = Currency
            };

            var lkngAction = requestType == Constants.RequestTypes.Delinking ? Constants.ActionCodes.Remove : requestType == Constants.RequestTypes.Ammendment ? Constants.ActionCodes.Update : Constants.ActionCodes.Add;

            var account = GetAccountDataNew(dataCount, accDetail, pan, branchCode, lkngAction);
            accounts.Add(account);
        }

        #endregion


        #region Old Export Methods
        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AddUpdateFilePlainExport(tbl_File_Exports request, string requestTypeName, string cardTypeName, DateTime? requestDate = null)
        {
            bool IsSuccess = false;
            List<RequestForExportVM> records = null;
            List<RequestForExportVM> query = null;
            DataTable data = new DataTable();
            var currDate = DateTime.Now;
            request.BranchCodes = string.IsNullOrEmpty(request.BranchCodes) ? "0000" : request.BranchCodes;

            // Create filename for export or re-export file 
            var outputFilename = (requestDate == null) ? $"{cardTypeName} {requestTypeName} {currDate.ToString(Constants.Formats.DateFormat)} {currDate.ToString(Constants.Formats.TimeFormat)}" : $"{cardTypeName} {requestTypeName} {currDate.ToString(Constants.Formats.DateFormat)} {currDate.ToString(Constants.Formats.TimeFormat)} (re-export {requestDate.GetValueOrDefault().ToString(Constants.Formats.DateFormat)})";
            outputFilename = outputFilename.ToString().Replace(" ", "").ToString();
            outputFilename = outputFilename + ".txt";
            try
            {
                FileExportViewModel model = new FileExportViewModel();
                string currentDate = DateTime.Now.ToString(Constants.Formats.DateFormat);
                var filePath = new FilePathDataAccess().GetFilePathByTypeID(((int)Constants.enPathType.Export).ToString());

                // Get request data
                if (requestDate == null)
                    data = new RequestDataAccess().GetRequestDataForExport(request.RequestTypes, int.Parse(request.CardTypes), request.BranchCodes);
                else
                    data = new RequestDataAccess().GetRequestDataForPlainExport(request.RequestTypes, int.Parse(request.CardTypes), request.BranchCodes, requestDate);

                records = data.Rows.Count > 0 ? DAL.ReflectionHelper.CreateGenericListFromDataTable<RequestForExportVM>(data) : null;

                // Filter records for duplicate CNIC
                query = (records != null) ? records.GroupBy(e => string.IsNullOrEmpty(e.NIC) ? Guid.NewGuid().ToString() : e.NIC).Select(e => e.FirstOrDefault()).ToList() : null;

                if (query != null)
                {
                    string path = FileHelper.Create(filePath?.Path ?? @"C:\", outputFilename);
                    int dataCount = 1;
                    string action = (request.RequestTypes == Constants.RequestTypes.Issuance || request.RequestTypes == Constants.RequestTypes.Replacement) ? Constants.ActionCodes.Add : (request.RequestTypes == Constants.RequestTypes.Ammendment || request.RequestTypes == Constants.RequestTypes.Upgrade || request.RequestTypes == Constants.RequestTypes.Linking || request.RequestTypes == Constants.RequestTypes.Delinking) ? Constants.ActionCodes.Update : Constants.ActionCodes.Add;

                    // Create header
                    model.Header = new HeaderRecord() { Date = currentDate.LimitTo(8), FileName = "CRD".LimitTo(20), Version = "03.05.00".LimitTo(8) };

                    // Write header
                    FileHelper.WriteFile(path, new string[] { string.Join(string.Empty, model.Header.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(model.Header, null))) });

                    foreach (var requestDetail in query)
                    {
                        var accounts = new List<AccountData>();
                        var account = new AccountData();
                        var lstString = new List<string>();
                        dataCount++;

                        // Create data
                        var cData = GetCardData(dataCount, requestDetail, action);
                        var profile = GetProfileData(dataCount, requestDetail, action);

                        //If Request type is ammendment then we have to create new rows for linked or delinked accounts
                        if (request.RequestTypes == Constants.RequestTypes.Ammendment)
                        {
                            var getDefaultNLinkingAccounts = new CustomerAccountDataAccess().GetCustomerAccountByCardNo(requestDetail.CardNo);
                            requestDetail.DefaultAccount = "O";//Default Account
                            accounts.Add(GetAccountData(dataCount, requestDetail, request.BranchCodes, action));
                            foreach (var item in getDefaultNLinkingAccounts.Where(e => e.AccountNo != requestDetail.AccountNo))
                            {
                                AccountSectionExtension(ref accounts, item.AccountNo, item.AccountTitle, string.IsNullOrEmpty(requestDetail.AccountType) ? "10" : requestDetail.AccountType, requestDetail.BranchCode, request.RequestTypes, dataCount, requestDetail.Pan);
                            }
                        }

                        //If Request type is linking or delinking then we have to create new rows for linking or delinking accounts
                        else if (request.RequestTypes == Constants.RequestTypes.Linking || request.RequestTypes == Constants.RequestTypes.Delinking)
                        {
                            requestDetail.DefaultAccount = "O";//Default Account
                            accounts.Add(GetAccountData(dataCount, requestDetail, request.BranchCodes, action));
                            foreach (var item in requestDetail.LinkingDelinkingAccount.Split(','))
                            {
                                var acc = GetCustomerAccountForLinkingAccounts(item);

                                AccountSectionExtensionForLinking(ref accounts, item, acc.FirstOrDefault()?.AccountTitle ?? string.Empty, string.IsNullOrEmpty(requestDetail.AccountType) ? "10" : requestDetail.AccountType, requestDetail.BranchCode, request.RequestTypes, dataCount, requestDetail.Pan, acc.FirstOrDefault().Currency);
                            }
                        }
                        else
                            accounts.Add(GetAccountData(dataCount, requestDetail, request.BranchCodes, action));

                        var relation = GetRelationData(dataCount, requestDetail, action);

                        // Write data
                        //FileHelper.WriteFile(path, new string[] { string.Join(string.Empty, cData.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(cData, null))), string.Join(string.Empty, profile.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(profile, null))), string.Join(string.Empty, account.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(account, null))), string.Join(string.Empty, relation.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(relation, null))) });

                        lstString.Add(string.Join(string.Empty, cData.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(cData, null))));
                        lstString.Add(string.Join(string.Empty, profile.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(profile, null))));
                        accounts.ForEach(e => lstString.Add(string.Join(string.Empty, e.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(e, null)))));
                        lstString.Add(string.Join(string.Empty, relation.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(relation, null))));

                        FileHelper.WriteFile(path, lstString.ToArray());
                    }

                    // Create footer
                    string datacount = (query.Count + 2).ToString();
                    //model.Footer = new FooterRecord() { Date = currentDate.LimitTo(8), FileName = outputFilename.Replace(".txt", string.Empty).LimitTo(20), RecordNumber = datacount.PadLeft(6, '0') };
                    model.Footer = new FooterRecord() { Date = currentDate.LimitTo(8), FileName = "CRD".LimitTo(20), RecordNumber = datacount.PadLeft(6, '0') };

                    // Write footer
                    FileHelper.WriteFile(path, new string[] { string.Join(string.Empty, model.Footer.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(model.Footer, null))) });

                    // Encrypt file [remove b/c we don't want encrypted file as it works on other action]
                    //new GNUPG_Wrapper().Encrypt(path, outputFilename);

                    // Update file export record
                    new RequestDataAccess().UpdateRequestAfterExport(query.Select(e => e.ID).ToList());
                    request.RecordCount = query.Count;

                    // Add record to fileexport table
                    IsSuccess = new FileExportDataAccess().AddFileExport(request);
                }

                if (IsSuccess)
                    return Json(new { IsSuccess = IsSuccess, Response = IsSuccess, ErrorMessage = string.Format(CustomMessages.ImportRecordsRetrieveAndExport, records == null ? 0 : records.Count, query == null ? 0 : query.Count) }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { IsSuccess = IsSuccess, ErrorMessage = CustomMessages.NoRecordFound, Response = IsSuccess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.PrintError(ex.Message, ex);
            }

            return Json(new { IsSuccess = true, Response = true, ErrorMessage = string.Format(CustomMessages.ImportRecordsRetrieveAndExport, records == null ? 0 : records.Count, query == null ? 0 : query.Count) }, JsonRequestBehavior.AllowGet);
        }


        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AddUpdateEncryptedFileExport(tbl_File_Exports request, string requestTypeName, string cardTypeName, DateTime? requestDate = null)
        {
            bool IsSuccess = false;
            List<RequestForExportVM> records = null;
            List<RequestForExportVM> query = null;
            DataTable data = new DataTable();
            var currDate = DateTime.Now;
            request.BranchCodes = string.IsNullOrEmpty(request.BranchCodes) ? "0000" : request.BranchCodes;

            // Create filename for export or re-export file 
            var outputFilename = (requestDate == null) ? $"{cardTypeName}{requestTypeName}{currDate.ToString(Constants.Formats.DateFormat)}{currDate.ToString(Constants.Formats.TimeFormat)}" : $"{cardTypeName}{requestTypeName}{currDate.ToString(Constants.Formats.DateFormat)}{currDate.ToString(Constants.Formats.TimeFormat)}(re-export {requestDate.GetValueOrDefault().ToString(Constants.Formats.DateFormat)})";
            //removing spaces fro name 10 jul 18
            outputFilename = outputFilename.ToString().Replace(" ", "").ToString();
            outputFilename = outputFilename + ".txt";
            try
            {
                FileExportViewModel model = new FileExportViewModel();
                string currentDate = DateTime.Now.ToString(Constants.Formats.DateFormat);
                var filePath = new FilePathDataAccess().GetFilePathByTypeID(((int)Constants.enPathType.Export).ToString());

                // Get request data
                if (requestDate == null)
                    data = new RequestDataAccess().GetRequestDataForExport(request.RequestTypes, int.Parse(request.CardTypes), request.BranchCodes);
                else
                    data = new RequestDataAccess().GetRequestDataForExport(request.RequestTypes, int.Parse(request.CardTypes), request.BranchCodes, requestDate);

                records = data.Rows.Count > 0 ? DAL.ReflectionHelper.CreateGenericListFromDataTable<RequestForExportVM>(data) : null;

                // Filter records for duplicate CNIC
                query = (records != null) ? records.GroupBy(e => string.IsNullOrEmpty(e.NIC) ? Guid.NewGuid().ToString() : e.NIC).Select(e => e.FirstOrDefault()).ToList() : null;

                if (query != null)
                {
                    string path = FileHelper.Create(filePath?.Path ?? @"C:\", outputFilename);
                    int dataCount = 1;
                    string action = (request.RequestTypes == Constants.RequestTypes.Issuance || request.RequestTypes == Constants.RequestTypes.Replacement) ? Constants.ActionCodes.Add : (request.RequestTypes == Constants.RequestTypes.Ammendment || request.RequestTypes == Constants.RequestTypes.Upgrade || request.RequestTypes == Constants.RequestTypes.Linking || request.RequestTypes == Constants.RequestTypes.Delinking) ? Constants.ActionCodes.Update : Constants.ActionCodes.Add;

                    // Create header
                    model.Header = new HeaderRecord() { Date = currentDate.LimitTo(8), FileName = "CRD".LimitTo(20), Version = "03.05.00".LimitTo(8) };

                    // Write header
                    FileHelper.WriteFile(path, new string[] { string.Join(string.Empty, model.Header.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(model.Header, null))) });

                    foreach (var requestDetail in query)
                    {
                        var accounts = new List<AccountData>();
                        var account = new AccountData();
                        var lstString = new List<string>();
                        dataCount++;

                        // Create data
                        var cData = GetCardData(dataCount, requestDetail, action);
                        var profile = GetProfileData(dataCount, requestDetail, action);

                        //If Request type is ammendment then we have to create new rows for linked or delinked accounts
                        if (request.RequestTypes == Constants.RequestTypes.Ammendment)
                        {
                            var getDefaultNLinkingAccounts = new CustomerAccountDataAccess().GetCustomerAccountByCardNo(requestDetail.CardNo);
                            requestDetail.DefaultAccount = "O";//Default Account
                            accounts.Add(GetAccountData(dataCount, requestDetail, request.BranchCodes, action));
                            foreach (var item in getDefaultNLinkingAccounts.Where(e => e.AccountNo != requestDetail.AccountNo))
                            {
                                AccountSectionExtension(ref accounts, item.AccountNo, item.AccountTitle, string.IsNullOrEmpty(requestDetail.AccountType) ? "10" : requestDetail.AccountType, requestDetail.BranchCode, request.RequestTypes, dataCount, requestDetail.Pan);
                            }
                        }

                        //If Request type is linking or delinking then we have to create new rows for linking or delinking accounts
                        else if (request.RequestTypes == Constants.RequestTypes.Linking || request.RequestTypes == Constants.RequestTypes.Delinking)
                        {
                            requestDetail.DefaultAccount = "O";//Default Account
                            accounts.Add(GetAccountData(dataCount, requestDetail, request.BranchCodes, action));
                            foreach (var item in requestDetail.LinkingDelinkingAccount.Split(','))
                            {
                                var acc = GetCustomerAccountForLinkingAccounts(item);

                                AccountSectionExtensionForLinking(ref accounts, item, acc.FirstOrDefault()?.AccountTitle ?? string.Empty, string.IsNullOrEmpty(requestDetail.AccountType) ? "10" : requestDetail.AccountType, requestDetail.BranchCode, request.RequestTypes, dataCount, requestDetail.Pan, acc.FirstOrDefault().Currency);
                            }
                        }
                        else
                            accounts.Add(GetAccountData(dataCount, requestDetail, request.BranchCodes, action));

                        var relation = GetRelationData(dataCount, requestDetail, action);

                        // Write data
                        //FileHelper.WriteFile(path, new string[] { string.Join(string.Empty, cData.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(cData, null))), string.Join(string.Empty, profile.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(profile, null))), string.Join(string.Empty, account.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(account, null))), string.Join(string.Empty, relation.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(relation, null))) });

                        lstString.Add(string.Join(string.Empty, cData.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(cData, null))));
                        lstString.Add(string.Join(string.Empty, profile.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(profile, null))));
                        accounts.ForEach(e => lstString.Add(string.Join(string.Empty, e.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(e, null)))));
                        lstString.Add(string.Join(string.Empty, relation.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(relation, null))));

                        FileHelper.WriteFile(path, lstString.ToArray());
                    }

                    // Create footer
                    string datacount = (query.Count + 2).ToString();
                    //model.Footer = new FooterRecord() { Date = currentDate.LimitTo(8), FileName = outputFilename.Replace(".txt", string.Empty).LimitTo(20), RecordNumber = datacount.PadLeft(6, '0') };
                    model.Footer = new FooterRecord() { Date = currentDate.LimitTo(8), FileName = "CRD".LimitTo(20), RecordNumber = datacount.PadLeft(6, '0') };

                    // Write footer
                    FileHelper.WriteFile(path, new string[] { string.Join(string.Empty, model.Footer.GetType().GetProperties().ToList().Select(p => (string)p.GetValue(model.Footer, null))) });

                    // Encrypt file
                    outputFilename = (requestDate == null) ? $"{cardTypeName} {requestTypeName} {currDate.ToString(Constants.Formats.DateFormat)} {currDate.ToString(Constants.Formats.TimeFormat)}" : $"{cardTypeName} {requestTypeName} {currDate.ToString(Constants.Formats.DateFormat)} {currDate.ToString(Constants.Formats.TimeFormat)} (re-export {requestDate.GetValueOrDefault().ToString(Constants.Formats.DateFormat)})";
                    outputFilename = outputFilename.ToString().Replace(" ", "").ToString();

                    // Encrypt file
                    new GNUPG_Wrapper().EncryptBatch(path, outputFilename);

                    // Update file export record
                    new RequestDataAccess().UpdateRequestAfterExport(query.Select(e => e.ID).ToList());
                    request.RecordCount = query.Count;

                    // Add record to fileexport table
                    IsSuccess = new FileExportDataAccess().AddFileExport(request);
                }

                if (IsSuccess)
                    return Json(new { IsSuccess = IsSuccess, Response = IsSuccess, ErrorMessage = string.Format(CustomMessages.ImportRecordsRetrieveAndExport, records == null ? 0 : records.Count, query == null ? 0 : query.Count) }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { IsSuccess = IsSuccess, ErrorMessage = CustomMessages.NoRecordFound, Response = IsSuccess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.PrintError(ex.Message, ex);
            }

            return Json(new { IsSuccess = true, Response = true, ErrorMessage = string.Format(CustomMessages.ImportRecordsRetrieveAndExport, records == null ? 0 : records.Count, query == null ? 0 : query.Count) }, JsonRequestBehavior.AllowGet);
        }

        #endregion


    }
}