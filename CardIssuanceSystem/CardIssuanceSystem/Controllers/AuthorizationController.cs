using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.Core.Methods;
using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL;
using CardIssuanceSystem.DAL.DataAccessClasses;
using CardIssuanceSystem.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CardIssuanceSystem.Controllers
{
    public class TransComments
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class AuthorizationController : BaseController
    {
        [AuthOp(RoleTitle = new string[] { "u" })]
        public JsonResult UpdateRequestAuthorization(int RequestId, string AuthorizationStatus, string AuthorizeComments)
        {
            bool success;
            RequestDataAccess lst = new RequestDataAccess();
            success = lst.UpdateRequestAuthorization(RequestId, AuthorizationStatus, AuthorizeComments);
            string msg;
            if (success)
            {
                msg = CustomMessages.Reject;
            }
            else
            {
                msg = CustomMessages.SomethingWentWrong;
            }
            return Json(new { IsSuccess = success, ErrorMessage = msg }, JsonRequestBehavior.AllowGet);
        }

        #region Authorize New Request Detail

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AuthorizeNewRequestDetail(int RequestId)
        {
            CardTypesDataAccess obj = new CardTypesDataAccess();
            tbl_Requests model = new tbl_Requests();
            RequestDataAccess lst = new RequestDataAccess();
            CustomerRequestAccountDataAccess crad = new CustomerRequestAccountDataAccess();
            model = lst.GetRequestById(RequestId, "N");
            if (model == null)
            {
                return RedirectToAction("AuthorizeCardIssuanceScreen", "Request");
            }

            ViewBag.CardTypes = obj.GetCardTypes("A");
            ViewBag.CardTypes = ((IEnumerable<tbl_Card_Types>)ViewBag.CardTypes).Where(s => s.ID == model.CardTypeID);

            /*[13-04-2018]:Change occur account data saves on creator end when making request, on authorizer we get data from customer accounts to show*/

            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType t24resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            try
            {
                t24resp = T24Methods.FetchAccount(model.AccountNo, model.CIFNo);
                t24resp.Sector = new SectorDataAccess().GetSectorbyCode(t24resp.Sector)?.Description ?? string.Empty;
                //t24resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(t24resp.Product)?.Name ?? string.Empty;
                //t24resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(t24resp.OpInstructions)?.Description ?? string.Empty;
            }
            catch
            {
                
            }
            //resp = T24Methods.FetchAccount(model.AccountNo, model.CIFNo);
            //resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
            //resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
            //resp.Sector = new SectorDataAccess().GetSectorbyCode(resp.Sector)?.Description ?? string.Empty;
            //resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(resp.Product)?.Name ?? string.Empty;

            var resp = new CustomerAccountDataAccess().GetCustomerAccountbyCIF(model.AccountNo, model.CIFNo).FirstOrDefault();
            if (resp == null)
                resp = new tbl_Customer_Accounts();
            else
            {
                resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
                resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
                resp.MotherMaidenName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MotherMaidenName);
                t24resp.Product = new AccountTypeDataAccess().GetAccountTypeById(resp.AccountTypeID.GetValueOrDefault())?.Name ?? string.Empty;
            }
            // resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(resp.OpInstructions)?.Description ?? string.Empty;
            ViewBag.AccountInfo = resp;
            ViewBag.T24AccountInfo = t24resp;
            var RequestAccount = crad.GetRequestCustomerAccountByRequestId(RequestId);

            var MainAddress = OtherAddressMapping(RequestAccount?.MainAddress ?? string.Empty);
            var SetAddress = "";
            ViewBag.MainAddress = MainAddress;
            
            if (MainAddress == "Other")
            {
                SetAddress = RequestAccount?.MainAddress ?? string.Empty;
            }
            ViewBag.SetAddress = SetAddress;
            /*mobile*/
            var MainMobile = AuthorizationController.OtherMobileNoMapping(RequestAccount?.MainMobile ?? string.Empty);
            var SetMobileNo = "";
            ViewBag.MainMobile = MainMobile;

            if (MainMobile == "Other")
            {
                SetMobileNo = RequestAccount?.MainMobile ?? string.Empty;
            }
            ViewBag.SetMobileNo = SetMobileNo;

            /*landline*/
            var MainLandlineNo = AuthorizationController.OtherLandlineNoMapping(RequestAccount?.LandlineNo ?? string.Empty);
            var SetLandlineNo = "";
            ViewBag.MainLandlineNo = MainLandlineNo;

            if (MainLandlineNo == "Other")
            {
                SetLandlineNo = RequestAccount?.LandlineNo ?? string.Empty;
            }
            ViewBag.SetLandlineNo = SetLandlineNo;

            /*office*/
            var MainOfficeNo = AuthorizationController.OtherOfficeNoMapping(RequestAccount?.PhoneOffice ?? string.Empty);
            var SetOfficeNo = "";
            ViewBag.MainOfficeNo = MainOfficeNo;

            if (MainOfficeNo == "Other")
            {
                SetOfficeNo = RequestAccount?.PhoneOffice ?? string.Empty;
            }
            ViewBag.SetOfficeNo = SetOfficeNo;
            /**/
            //ViewBag.MainMobile = RequestAccount?.MainMobile ?? string.Empty;
            ViewBag.MainLandline = RequestAccount?.MainLandline ?? string.Empty;
            
            return View(model);
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public JsonResult AuthorizeRequest(TransactionVM transaction)
        {
            //check for duplicate request submission Oct-2018 Issue 
            string RequestStatus = new RequestDataAccess().GetAllRequests().Where(a=>a.ID==transaction.RequestId).FirstOrDefault().AuthorizationStatus;
            if (RequestStatus == "A")
            {
                return Json(new { IsSuccess = true, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }

            var CardExist = CommonMethods.GetCardsInfoforRequest(transaction.DrCustomerAccountNumber,transaction.CIF,transaction.RequestType);
            if (CardExist == "true")
            {
                bool success = false;
                string MainAddressValue;
                string MainMobileValue;
                string MainLandlineValue;
                string MainLandlineNoValue;
                string MainOfficeNoValue;
                decimal ReserveAmount = 0;
                int LstCount;
                Dictionary<string, string> result = new Dictionary<string, string>();
                Dictionary<string, string> ErrorMessages = new Dictionary<string, string>();
                int resCt = 0;
                //List<TransComments> result = new List<Controllers.TransComments>();
                bool BalanceEligibility = false;
                List<string> result2 = new List<string>();
                Dictionary<string, decimal> Transactions = new Dictionary<string, decimal>();
                RequestDataAccess lst = new RequestDataAccess();

                CardChargesDataAccess cc = new CardChargesDataAccess();
                List<tbl_Card_Charges> lst1 = new List<tbl_Card_Charges>();
                //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
                //resp = T24Methods.FetchAccount(transaction.DrCustomerAccountNumber);
                NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();

                resp = T24Methods.FetchAccount(transaction.DrCustomerAccountNumber, transaction.CIF);

                var accountsData = new CustomerAccountDataAccess().GetCustomerAccountbyCIF(transaction.DrCustomerAccountNumber, transaction.CIF).FirstOrDefault();
                resp.BranchCode = string.IsNullOrEmpty(resp.BranchCode) ? string.Empty : resp.BranchCode.Substring(resp.BranchCode.Length - 4, 4).ToString();
                resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
                //transaction.AccountTypeId = 2;
                //transaction.CardTypeId = 2;
                //transaction.BranchCode = "001";
                //string Frequency = "12";
                string Frequency = "0";
                string IsReplacement = "0";

                string AccountStatus = resp.AccountStatus;
                decimal AvailBalance = Convert.ToDecimal((!string.IsNullOrEmpty(resp.AvailableBalance)) ? resp.AvailableBalance : "0");

                if (transaction.RequestType == "R")
                {
                    IsReplacement = "1";
                }

                lst1 = cc.GetCardCharges(resp.AccountNature, transaction.CardTypeId, resp.BranchCode, Frequency, IsReplacement);
                //lst1 = cc.GetCardCharges("001", transaction.CardTypeId, transaction.BranchCode, Frequency);

                //bool StatusEligibility = CommonMethods.CheckStatusEligibility(AccountStatus);
                if (lst1.Count > 0)
                {
                    LstCount = 0;
                    var TotalAmount = lst1.Sum(item => item.Amount);
                    EligibilityVM elg = new EligibilityVM();
                    elg.IdentificationType = resp.IdentificationType;
                    elg.IdentificationNumber = resp.IdentificationNo;
                    elg.Resident = resp.ResidenceStatus;
                    elg.Nationality = resp.Nationality;
                    elg.SectorCode = resp.Sector;
                    elg.AccountStatus = resp.AccountStatus;
                    elg.FatherName = resp.FathersName;
                    elg.MotherName = resp.MothersName;
                    elg.DOB = resp.CustomerDOB;
                    elg.PostingRestriction = resp.PostingRestriction;
                    elg.OpInstruction = resp.OpInstructions;
                    elg.AccountNo = transaction.DrCustomerAccountNumber;
                    List<string> StatusEligibility = CommonMethods.IsEligible(elg);

                    if (StatusEligibility.Count <= 0)
                    {
                        if (transaction.WaiveCharges == false)
                        {

                            //Change for Free Products on 31 January 2019
                            BalanceEligibility = true;
                            if (TotalAmount > 0)
                            {
                                BalanceEligibility = CommonMethods.CheckBalanceEligibility(AvailBalance, TotalAmount);
                            }

                            //BalanceEligibility = CommonMethods.CheckBalanceEligibility(AvailBalance, TotalAmount);
                            if (transaction.AuthorizationStatus == "A")
                            {
                                //check for duplicate request submission Oct-2018 Issue 
                                RequestStatus = new RequestDataAccess().GetAllRequests().Where(a => a.ID == transaction.RequestId).FirstOrDefault().AuthorizationStatus;
                                if (RequestStatus == "A")
                                {
                                    return Json(new { IsSuccess = true, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
                                }
                                if (BalanceEligibility == true && TotalAmount > 0)
                                {
                                    foreach (var item in lst1)
                                    {
                                        resCt++;
                                        if (item.Amount > 0)
                                        {
                                            CISSB.TransactionInfoResponse response = new CISSB.TransactionInfoResponse();
                                            var DrCustomerAccountNumber = transaction.DrCustomerAccountNumber;//AccountNo1;//"20001266498";
                                            var TransactionAmount = Convert.ToDecimal(item.Amount ?? 0);
                                            var TransactionNarration = item.Title;

                                            response = T24Methods.DebitTransaction(DrCustomerAccountNumber, TransactionAmount, TransactionNarration, transaction.RequestType, resp.BranchCode, (transaction.RequestId).ToString(), LstCount);
                                            if (response.ResponseCode == "0000" && response.ResponseCodeDescription == "Success")
                                            {
                                                /*
                                                TransComments val = new Controllers.TransComments();
                                                val.Key = item.Title;
                                                val.Value = "Posted Successfully";
                                                result.Add(val);
                                                */
                                                result.Add(item.Title + "-" + resCt.ToString(), "Posted Successfully");
                                                ErrorMessages.Add(item.Title + "-" + resCt.ToString(), "Posted Successfully");
                                                Transactions.Add(TransactionNarration + "-" + resCt.ToString(), TransactionAmount);

                                                ReserveAmount = ReserveAmount + Convert.ToDecimal(item.Amount);
                                                LstCount++;
                                            }
                                            else
                                            {
                                                LstCount = 0;
                                                /*
                                                TransComments val = new Controllers.TransComments();
                                                val.Key = item.Title;
                                                val.Value = "Error in Transaction";
                                                result.Add(val);
                                                */
                                                result.Add(item.Title + "-" + resCt.ToString(), "Error in Transaction");
                                                ErrorMessages.Add(item.Title + "-" + resCt.ToString(), response.ResponseCodeDescription.ToString());// change on 01 March 2019 for displaying proper error message
                                                                                                                                                    //Transactions.Add(TransactionNarration, TransactionAmount);
                                                foreach (var action in Transactions)
                                                {
                                                    response = T24Methods.ReverseTransaction(DrCustomerAccountNumber, action.Value, action.Key, "X", resp.BranchCode, (transaction.RequestId).ToString(), LstCount);
                                                    LstCount++;
                                                }
                                                //if (ReserveAmount > 0)
                                                //{
                                                //    response = T24Methods.ReverseTransaction(DrCustomerAccountNumber, ReserveAmount, "Transaction Reverted", transaction.RequestType, resp.BranchCode);
                                                //}

                                                break;
                                            }




                                        }
                                        else
                                        {
                                            result.Add(item.Title + "-" + resCt.ToString(), "Posted Successfully");
                                            ErrorMessages.Add(item.Title + "-" + resCt.ToString(), "Posted Successfully");
                                            /*
                                            TransComments val = new Controllers.TransComments();
                                            val.Key = item.Title;
                                            val.Value = "Posted Successfully";
                                            result.Add(val);
                                            */
                                        }
                                    }

                                    if (result.ContainsValue("Error in Transaction"))
                                    {
                                        success = lst.UpdateRequestAuthorization(Convert.ToInt64(transaction.RequestId), "C", transaction.AuthorizeComments, false, false);
                                        /*string json = JsonConvert.SerializeObject(result, Formatting.Indented);*/// change on 01 March 2019 for displaying proper error message
                                        string json = JsonConvert.SerializeObject(ErrorMessages, Formatting.Indented);
                                        return Json(new { IsSuccess = success, ErrorMessage = json }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        success = lst.UpdateRequestAuthorization(Convert.ToInt64(transaction.RequestId), transaction.AuthorizationStatus, transaction.AuthorizeComments, true, true);
                                        if (transaction.RequestType == "N" || transaction.RequestType == "R")
                                        {
                                            //SaveAccountInfo(resp);
                                            //AccountExists(transaction.DrCustomerAccountNumber, resp);

                                            MainAddressValue = FieldMappingAddress(transaction.MainAddress, accountsData);
                                            MainMobileValue = FieldMappingMobile(transaction.MainMobile, accountsData);
                                            MainLandlineValue = FieldMappingLandline(transaction.MainLandline, accountsData);
                                            MainLandlineNoValue = FieldMappingLandlineNo(transaction.LandlineNo, accountsData);
                                            MainOfficeNoValue = FieldMappingOfficeNo(transaction.PhoneOff, accountsData);
                                            transaction.MainAddress = MainAddressValue;
                                            transaction.MainMobile = MainMobileValue;
                                            transaction.MainLandline = MainLandlineValue;
                                            transaction.PhoneOff = MainOfficeNoValue;
                                            transaction.LandlineNo = MainLandlineNoValue;
                                            AccountExists(transaction.RequestId, transaction.DrCustomerAccountNumber, resp, transaction.MainAddress, transaction.MainMobile, transaction.MainLandline, transaction.LandlineNo, transaction.PhoneOff, transaction.CIF, transaction.CardNo);
                                        }
                                        else if (transaction.RequestType == "U")
                                        {
                                            var bol = CustomerCardDataAccess.UpdateCustomerCardType(transaction.CardNo, transaction.CardTypeId);
                                            if (bol == false)
                                            {
                                                return Json(new { IsSuccess = false, ErrorMessage = "Card Type not Upgraded" }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                        return Json(new { IsSuccess = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                //change for free products on 31 January 2019
                                else if (BalanceEligibility == true && TotalAmount <= 0)
                                {
                                    success = lst.UpdateRequestAuthorization(Convert.ToInt64(transaction.RequestId), transaction.AuthorizationStatus, transaction.AuthorizeComments, true, false);
                                    return Json(new { IsSuccess = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    //even if request is marked as authorized, system should mark it as closed in case of insufficient balance
                                    //success = lst.UpdateRequestAuthorization(Convert.ToInt64(transaction.RequestId), transaction.AuthorizationStatus, transaction.AuthorizeComments, true, false);
                                    success = lst.UpdateRequestAuthorization(Convert.ToInt64(transaction.RequestId), "C", transaction.AuthorizeComments, true, false);
                                    return Json(new { IsSuccess = success, ErrorMessage = CustomMessages.InsufficientBalance }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                success = lst.UpdateRequestAuthorization(Convert.ToInt64(transaction.RequestId), transaction.AuthorizationStatus, transaction.AuthorizeComments);

                                if (transaction.RequestType == "N" || transaction.RequestType == "R")
                                {
                                    //SaveAccountInfo(resp);
                                    //AccountExists(transaction.DrCustomerAccountNumber, resp);
                                    MainAddressValue = FieldMappingAddress(transaction.MainAddress, accountsData);
                                    MainMobileValue = FieldMappingMobile(transaction.MainMobile, accountsData);
                                    MainLandlineValue = FieldMappingLandline(transaction.MainLandline, accountsData);
                                    MainLandlineNoValue = FieldMappingLandlineNo(transaction.LandlineNo, accountsData);
                                    MainOfficeNoValue = FieldMappingOfficeNo(transaction.PhoneOff, accountsData);
                                    transaction.MainAddress = MainAddressValue;
                                    transaction.MainMobile = MainMobileValue;
                                    transaction.MainLandline = MainLandlineValue;
                                    transaction.LandlineNo = MainLandlineNoValue;
                                    transaction.PhoneOff = MainOfficeNoValue;
                                    AccountExists(transaction.RequestId, transaction.DrCustomerAccountNumber, resp, transaction.MainAddress, transaction.MainMobile, transaction.MainLandline, MainLandlineNoValue, MainOfficeNoValue, transaction.CIF, transaction.CardNo);
                                }

                                return Json(new { IsSuccess = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            success = lst.UpdateRequestAuthorization(Convert.ToInt64(transaction.RequestId), transaction.AuthorizationStatus, transaction.AuthorizeComments, true, true);//status eligibility zero;

                            if ((transaction.RequestType == "N" || transaction.RequestType == "R") && transaction.AuthorizationStatus == "A")
                            {
                                //SaveAccountInfo(resp);
                                //AccountExists(transaction.DrCustomerAccountNumber, resp);
                                MainAddressValue = FieldMappingAddress(transaction.MainAddress, accountsData);
                                MainMobileValue = FieldMappingMobile(transaction.MainMobile, accountsData);
                                MainLandlineValue = FieldMappingLandline(transaction.MainLandline, accountsData);
                                MainLandlineNoValue = FieldMappingLandlineNo(transaction.LandlineNo, accountsData);
                                MainOfficeNoValue = FieldMappingOfficeNo(transaction.PhoneOff, accountsData);
                                transaction.MainAddress = MainAddressValue;
                                transaction.MainMobile = MainMobileValue;
                                transaction.MainLandline = MainLandlineValue;
                                transaction.LandlineNo = MainLandlineNoValue;
                                transaction.PhoneOff = MainOfficeNoValue;
                                AccountExists(transaction.RequestId, transaction.DrCustomerAccountNumber, resp, transaction.MainAddress, transaction.MainMobile, transaction.MainLandline, MainLandlineNoValue, MainOfficeNoValue, transaction.CIF, transaction.CardNo);
                            }
                            else if (transaction.RequestType == "U" && transaction.AuthorizationStatus == "A")
                            {
                                var bol = CustomerCardDataAccess.UpdateCustomerCardType(transaction.CardNo, transaction.CardTypeId);
                                if (bol == false)
                                {
                                    return Json(new { IsSuccess = false, ErrorMessage = "Card Type not Upgraded" }, JsonRequestBehavior.AllowGet);
                                }
                            }

                            return Json(new { IsSuccess = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        success = lst.UpdateRequestAuthorization(Convert.ToInt64(transaction.RequestId), "C", transaction.AuthorizeComments, false, true);//status eligibility zero;
                        string ErrorString = string.Join(",", StatusEligibility.ToArray());
                        return Json(new { IsSuccess = success, ErrorMessage = ErrorString }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    success = lst.UpdateRequestAuthorization(Convert.ToInt64(transaction.RequestId), "C", transaction.AuthorizeComments, false, BalanceEligibility);//status eligibility zero;
                    return Json(new
                    {
                        IsSuccess = success,
                        ErrorMessage = CustomMessages.AccountNotEligible
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                RequestDataAccess lst = new RequestDataAccess();
                var success = lst.UpdateRequestAuthorization(Convert.ToInt64(transaction.RequestId), "C", transaction.AuthorizeComments, false, false);//status eligibility zero;
                return Json(new { IsSuccess = success, ErrorMessage = CardExist }, JsonRequestBehavior.AllowGet);
            }
           

            //return Json(new { IsSuccess = true, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
        }

       

        public static bool TransactionMethod(string DebitAccount,decimal TransactionAmount,string TransactionNarration,string RequestType,string BranchCode,string RequestId,int LstCount)
        {
            CISSB.TransactionInfoResponse response = new CISSB.TransactionInfoResponse();
            response = T24Methods.DebitTransaction(DebitAccount, TransactionAmount, TransactionNarration, RequestType, BranchCode,RequestId,LstCount);
            if (response.ResponseCode == "0000" && response.ResponseCodeDescription == "Success")
            {
                return true;
            }else
            {
                return false;
            }
        }
        #endregion


        #region Authorize Ammend Request Detail
        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AuthorizeAmmendRequestDetail(int RequestId)
        {
            CustomerRequestAccountDataAccess crad = new CustomerRequestAccountDataAccess();
            CardTypesDataAccess obj = new CardTypesDataAccess();
            tbl_Requests model = new tbl_Requests();
            RequestDataAccess lst = new RequestDataAccess();
            model = lst.GetRequestById(RequestId, "A");
            if (model == null)
            {
                return RedirectToAction("AuthorizeCardAmendmentScreen", "Request");
            }
            ViewBag.CardTypes = obj.GetCardTypes("A");
            ViewBag.CardTypes = ((IEnumerable<tbl_Card_Types>)ViewBag.CardTypes).Where(s => s.ID == model.CardTypeID);

            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType t24resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            try
            {
                t24resp = T24Methods.FetchAccount(model.AccountNo, model.CIFNo);
                t24resp.Sector = new SectorDataAccess().GetSectorbyCode(t24resp.Sector)?.Description ?? string.Empty;
                //t24resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(t24resp.Product)?.Name ?? string.Empty;
                //t24resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(t24resp.OpInstructions)?.Description ?? string.Empty;
            }
            catch 
            {
               
            }
            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            //resp = T24Methods.FetchAccount(model.AccountNo);
            //NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            //resp = T24Methods.FetchAccount(model.AccountNo, model.CIFNo);
            //resp.Sector = new SectorDataAccess().GetSectorbyCode(resp.Sector)?.Description ?? string.Empty;
            //resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(resp.Product)?.Name ?? string.Empty;
            //resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(resp.OpInstructions)?.Description ?? string.Empty;

            var resp = new CustomerAccountDataAccess().GetCustomerAccountbyCIF(model.AccountNo, model.CIFNo).FirstOrDefault();
            if (resp == null)
                resp = new tbl_Customer_Accounts();
            else
            {
                resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
                resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
                resp.MotherMaidenName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MotherMaidenName);
                t24resp.Product = new AccountTypeDataAccess().GetAccountTypeById(resp.AccountTypeID.GetValueOrDefault())?.Name ?? string.Empty;
            }

            //resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
            //resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
            //resp.CustomerDOB = resp.CustomerDOB.ToValidDateTime("dd-MM-yyyy");
            //resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
            ViewBag.AccountInfo = resp;
            ViewBag.T24AccountInfo = t24resp;
            ViewBag.RequestCustomerInfo = new CustomerRequestAccountDataAccess().GetRequestCustomerAccountByRequestId(RequestId);
            var RequestAccount = crad.GetRequestCustomerAccountByRequestId(RequestId);
            //var MainAddress = FieldMappingAddress(RequestAccount.MainAddress);
            //var MainMobile = FieldMappingMobile(RequestAccount.MainMobile);
            //var MainLandline = FieldMappingLandline(RequestAccount.MainLandline);

            var MainAddress = OtherAddressMapping(RequestAccount.MainAddress);
            var SetAddress = "";
            ViewBag.MainAddress = MainAddress;

            if (MainAddress == "Other")
            {
                SetAddress = RequestAccount.MainAddress;
            }
            ViewBag.SetAddress = SetAddress;
            /*mobile*/
            var MainMobile = AuthorizationController.OtherMobileNoMapping(RequestAccount?.MainMobile ?? string.Empty);
            var SetMobileNo = "";
            ViewBag.MainMobile = MainMobile;

            if (MainMobile == "Other")
            {
                SetMobileNo = RequestAccount?.MainMobile ?? string.Empty;
            }
            ViewBag.SetMobileNo = SetMobileNo;

            /*landline*/
            var MainLandlineNo = AuthorizationController.OtherLandlineNoMapping(RequestAccount?.LandlineNo ?? string.Empty);
            var SetLandlineNo = "";
            ViewBag.MainLandlineNo = MainLandlineNo;

            if (MainLandlineNo == "Other")
            {
                SetLandlineNo = RequestAccount?.LandlineNo ?? string.Empty;
            }
            ViewBag.SetLandlineNo = SetLandlineNo;

            /*office*/
            var MainOfficeNo = AuthorizationController.OtherOfficeNoMapping(RequestAccount?.PhoneOffice ?? string.Empty);
            var SetOfficeNo = "";
            ViewBag.MainOfficeNo = MainOfficeNo;

            if (MainOfficeNo == "Other")
            {
                SetOfficeNo = RequestAccount?.PhoneOffice ?? string.Empty;
            }
            ViewBag.SetOfficeNo = SetOfficeNo;
            /**/
            //ViewBag.MainMobile = RequestAccount.MainMobile;
            ViewBag.MainLandline = RequestAccount.MainLandline;
            return View(model);
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public JsonResult AuthorizeAmmendRequest(TransactionVM transaction)
        {
            string MainAddressValue;
            string MainMobileValue;
            string MainLandlineValue;
            string MainLandlineNoValue;
            string MainOfficeNoValue;
            bool success = false;
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            RequestDataAccess lst = new RequestDataAccess();

            CardChargesDataAccess cc = new CardChargesDataAccess();
            List<tbl_Card_Charges> lst1 = new List<tbl_Card_Charges>();
            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            //resp = T24Methods.FetchAccount(transaction.DrCustomerAccountNumber);

            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            resp = T24Methods.FetchAccount(transaction.DrCustomerAccountNumber, transaction.CIF);
           
            MainAddressValue = FieldMappingAddress(transaction.MainAddress, resp);
            MainMobileValue = FieldMappingMobile(transaction.MainMobile, resp);
            MainLandlineValue = FieldMappingLandline(transaction.MainLandline, resp);
            MainLandlineNoValue = FieldMappingLandlineNo(transaction.LandlineNo, resp);
            MainOfficeNoValue = FieldMappingOfficeNo(transaction.PhoneOff, resp);
            string AccountStatus = resp.AccountStatus;

            decimal AvailBalance = Convert.ToDecimal((!string.IsNullOrEmpty(resp.AvailableBalance)) ? resp.AvailableBalance : "0");
            resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
            //bool StatusEligibility = CommonMethods.CheckStatusEligibility(AccountStatus);
            EligibilityVM elg = new EligibilityVM();
            elg.IdentificationType = resp.IdentificationType;
            elg.IdentificationNumber = resp.IdentificationNo;
            elg.Resident = resp.ResidenceStatus;
            elg.Nationality = resp.Nationality;
            elg.SectorCode = resp.Sector;
            elg.AccountStatus = resp.AccountStatus;
            elg.FatherName = resp.FathersName;
            elg.MotherName = resp.MothersName;
            elg.DOB = resp.CustomerDOB;
            elg.PostingRestriction = resp.PostingRestriction;
            elg.OpInstruction = resp.OpInstructions;
            elg.AccountNo = transaction.DrCustomerAccountNumber;
            List<string> StatusEligibility = CommonMethods.IsEligible(elg);

            if (StatusEligibility.Count <=0)
            {
                if (transaction.AuthorizationStatus == "A")
                {

                    //AccountExists(resp, transaction.MainAddress, transaction.MainMobile, transaction.MainLandline);
                    transaction.MainAddress = MainAddressValue;
                    transaction.MainMobile = MainMobileValue;
                    transaction.MainLandline = MainLandlineValue;
                    transaction.PhoneOff = MainOfficeNoValue;
                    transaction.LandlineNo = MainLandlineNoValue;
                    AccountExists(transaction.RequestId, transaction.DrCustomerAccountNumber, resp, transaction.MainAddress, transaction.MainMobile, transaction.MainLandline, transaction.LandlineNo, transaction.PhoneOff, transaction.CIF, transaction.CardNo);
                }

                success = lst.UpdateRequestAuthorization(transaction.RequestId, transaction.AuthorizationStatus, transaction.AuthorizeComments, true, true);//status eligibility zero;
                return Json(new { IsSuccess = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                success = lst.UpdateRequestAuthorization(transaction.RequestId, "C", transaction.AuthorizeComments, false, true);//status eligibility zero;
                string ErrorString = string.Join(",", StatusEligibility.ToArray());
                return Json(new { IsSuccess = success, ErrorMessage = ErrorString }, JsonRequestBehavior.AllowGet);
            }

        }


        #endregion

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AuthorizeUpgradeRequestDetail(int RequestId)
        {
            CardTypesDataAccess obj = new CardTypesDataAccess();
            tbl_Requests model = new tbl_Requests();
            RequestDataAccess lst = new RequestDataAccess();
            model = lst.GetRequestById(RequestId, "U");
            if (model == null)
                return RedirectToAction("AuthorizeCardUpgradeScreen", "Request");

            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType t24resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            try
            {
                t24resp = T24Methods.FetchAccount(model.AccountNo, model.CIFNo);
                t24resp.Sector = new SectorDataAccess().GetSectorbyCode(t24resp.Sector)?.Description ?? string.Empty;
                //t24resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(t24resp.Product)?.Name ?? string.Empty;
                //t24resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(t24resp.OpInstructions)?.Description ?? string.Empty;
            }
            catch 
            {
                
            }

            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            //resp = T24Methods.FetchAccount(model.AccountNo);
            //NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            //resp = T24Methods.FetchAccount(model.AccountNo, model.CIFNo);
            //resp.Sector = new SectorDataAccess().GetSectorbyCode(resp.Sector)?.Description ?? string.Empty;
            //resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(resp.Product)?.Name ?? string.Empty;
            // resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(resp.OpInstructions)?.Description ?? string.Empty;
            //resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
            //resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
            //resp.CustomerDOB = resp.CustomerDOB.ToValidDateTime("dd-MM-yyyy");
            //resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);

            var resp = new CustomerAccountDataAccess().GetCustomerAccountbyCIF(model.AccountNo, model.CIFNo).FirstOrDefault();
            if (resp == null)
                resp = new tbl_Customer_Accounts();
            else
            {
                resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
                resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
                resp.MotherMaidenName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MotherMaidenName);
                t24resp.Product = new AccountTypeDataAccess().GetAccountTypeById(resp.AccountTypeID.GetValueOrDefault())?.Name ?? string.Empty;
            }

            ViewBag.ExistingCardNumber = AuthorizationMethods.GetCustomerCardByAccountNo(model.AccountNo);
            ViewBag.AccountInfo = resp;
            ViewBag.T24AccountInfo = t24resp;
            ViewBag.CardTypes = obj.GetCardTypes("A");
            ViewBag.CardTypes = ((IEnumerable<tbl_Card_Types>)ViewBag.CardTypes).Where(s => s.ID == model.CardTypeID);

            CardTypesDataAccess ctda = new CardTypesDataAccess();
            CustomerCardDataAccess ccda = new CustomerCardDataAccess();
            //existing card
            ViewBag.ExistingCardType1 = ctda.GetCardType(int.Parse((ccda.GetCustomerCard(model.CardNo)?.CardTypeID??0).ToString()))?.Title/*.ToString()*/;
            //upgrade to card
            ViewBag.NewCardType1 = ctda.GetCardType(int.Parse(model.CardTypeID.ToString())).Title;
            return View(model);
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AuthorizeCardReplacementScreen(int Id)
        {
            CustomerRequestAccountDataAccess crad = new CustomerRequestAccountDataAccess();
            CardTypesDataAccess obj = new CardTypesDataAccess();
            tbl_Requests model = new tbl_Requests();
            RequestDataAccess lst = new RequestDataAccess();
            model = lst.GetRequestById(Id, "R");
            if (model == null)
                return RedirectToAction("AuthorizeCardReplacementScreen", "Request");

            ViewBag.CardTypes = obj.GetCardTypes("A");
            ViewBag.CardTypes = ((IEnumerable<tbl_Card_Types>)ViewBag.CardTypes).Where(s => s.ID == model.CardTypeID);

            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType t24resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            try
            {
                t24resp = T24Methods.FetchAccount(model.AccountNo, model.CIFNo);
                t24resp.Sector = new SectorDataAccess().GetSectorbyCode(t24resp.Sector)?.Description ?? string.Empty;
                //t24resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(t24resp.Product)?.Name ?? string.Empty;
                //t24resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(t24resp.OpInstructions)?.Description ?? string.Empty;
            }
            catch
            {
               
            }

            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            //resp = T24Methods.FetchAccount(model.AccountNo);
            //NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            //resp = T24Methods.FetchAccount(model.AccountNo, model.CIFNo);
            //resp.Sector = new SectorDataAccess().GetSectorbyCode(resp.Sector)?.Description ?? string.Empty;
            //resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(resp.Product)?.Name ?? string.Empty;
            //resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(resp.OpInstructions)?.Description ?? string.Empty;
            //resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
            //resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
            //resp.CustomerDOB = resp.CustomerDOB.ToValidDateTime("dd-MM-yyyy");
            //resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);

            var resp = new CustomerAccountDataAccess().GetCustomerAccountbyCIF(model.AccountNo, model.CIFNo).FirstOrDefault();
            if (resp == null)
                resp = new tbl_Customer_Accounts();
            else
            {
                resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
                resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
                resp.MotherMaidenName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MotherMaidenName);
                t24resp.Product = new AccountTypeDataAccess().GetAccountTypeById(resp.AccountTypeID.GetValueOrDefault())?.Name ?? string.Empty;
            }
            ViewBag.AccountInfo = resp;
            ViewBag.T24AccountInfo = t24resp;
            var RequestAccount = crad.GetRequestCustomerAccountByRequestId(Id);
            var MainAddress = OtherAddressMapping(RequestAccount.MainAddress);
            var SetAddress = "";
            ViewBag.MainAddress = MainAddress;

            if (MainAddress == "Other")
            {
                SetAddress = RequestAccount.MainAddress;
            }
            ViewBag.SetAddress = SetAddress;
            /*mobile*/
            var MainMobile = AuthorizationController.OtherMobileNoMapping(RequestAccount?.MainMobile ?? string.Empty);
            var SetMobileNo = "";
            ViewBag.MainMobile = MainMobile;

            if (MainMobile == "Other")
            {
                SetMobileNo = RequestAccount?.MainMobile ?? string.Empty;
            }
            ViewBag.SetMobileNo = SetMobileNo;

            /*landline*/
            var MainLandlineNo = AuthorizationController.OtherLandlineNoMapping(RequestAccount?.LandlineNo ?? string.Empty);
            var SetLandlineNo = "";
            ViewBag.MainLandlineNo = MainLandlineNo;

            if (MainLandlineNo == "Other")
            {
                SetLandlineNo = RequestAccount?.LandlineNo ?? string.Empty;
            }
            ViewBag.SetLandlineNo = SetLandlineNo;

            /*office*/
            var MainOfficeNo = AuthorizationController.OtherOfficeNoMapping(RequestAccount?.PhoneOffice ?? string.Empty);
            var SetOfficeNo = "";
            ViewBag.MainOfficeNo = MainOfficeNo;

            if (MainOfficeNo == "Other")
            {
                SetOfficeNo = RequestAccount?.PhoneOffice ?? string.Empty;
            }
            ViewBag.SetOfficeNo = SetOfficeNo;
            /**/
            //ViewBag.MainMobile = RequestAccount.MainMobile;
            ViewBag.MainLandline = RequestAccount.MainLandline;
            return View(model);
        }

        #region Linking/Delinking
        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AuthorizeLinkingScreen(int Id)
        {
            CardTypesDataAccess obj = new CardTypesDataAccess();
            tbl_Requests model = new tbl_Requests();
            RequestDataAccess lst = new RequestDataAccess();
            model = lst.GetRequestById(Id, "L");
            if (model == null)
                return RedirectToAction("AuthorizeLinkingScreen", "Request");
            ViewBag.CardTypes = obj.GetCardTypes("A");
            ViewBag.CardTypes = ((IEnumerable<tbl_Card_Types>)ViewBag.CardTypes).Where(s => s.ID == model.CardTypeID);
            string[] AccountstoLink = model.LinkingDelinkingAccount == null ? new string[] { } : model.LinkingDelinkingAccount.Split(',');
            ViewBag.AccountstoLink = AccountstoLink;

            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType t24resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            try
            {
                t24resp = T24Methods.FetchAccount(model.AccountNo, model.CIFNo);
                t24resp.Sector = new SectorDataAccess().GetSectorbyCode(t24resp.Sector)?.Description ?? string.Empty;
                //t24resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(t24resp.Product)?.Name ?? string.Empty;
                //t24resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(t24resp.OpInstructions)?.Description ?? string.Empty;
            }
            catch 
            {
                
            }

            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            //resp = T24Methods.FetchAccount(model.AccountNo);
            //NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            //resp = T24Methods.FetchAccount(model.AccountNo, model.CIFNo);
            //resp.Sector = new SectorDataAccess().GetSectorbyCode(resp.Sector)?.Description ?? string.Empty;
            //resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(resp.Product)?.Name ?? string.Empty;
            // resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(resp.OpInstructions)?.Description ?? string.Empty;
            //resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
            //resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
            //resp.CustomerDOB = resp.CustomerDOB.ToValidDateTime("dd-MM-yyyy");
            //resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);

            var resp = new CustomerAccountDataAccess().GetCustomerAccountbyCIF(model.AccountNo, model.CIFNo).FirstOrDefault();
            if (resp == null)
                resp = new tbl_Customer_Accounts();
            else
            {
                resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
                resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
                resp.MotherMaidenName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MotherMaidenName);
                t24resp.Product = new AccountTypeDataAccess().GetAccountTypeById(resp.AccountTypeID.GetValueOrDefault())?.Name ?? string.Empty;
            }
            ViewBag.AccountInfo = resp;
            ViewBag.T24AccountInfo = t24resp;
            ViewBag.ExistingCardNumber = AuthorizationMethods.GetCustomerCardByAccountNo(model.AccountNo);
            return View(model);
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AuthorizeDelinkingScreen(int Id)
        {
            CardTypesDataAccess obj = new CardTypesDataAccess();
            tbl_Requests model = new tbl_Requests();
            RequestDataAccess lst = new RequestDataAccess();
            model = lst.GetRequestById(Id, "D");
            if (model == null)
                return RedirectToAction("AuthorizeDelinkingScreen", "Request");
            ViewBag.CardTypes = obj.GetCardTypes("A");
            ViewBag.CardTypes = ((IEnumerable<tbl_Card_Types>)ViewBag.CardTypes).Where(s => s.ID == model.CardTypeID);
            string[] AccountstoDeLink = model.LinkingDelinkingAccount == null ? new string[] { } : model.LinkingDelinkingAccount.Split(',');
            ViewBag.AccountstoDeLink = AccountstoDeLink;

            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType t24resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            try
            {
                t24resp = T24Methods.FetchAccount(model.AccountNo, model.CIFNo);
                t24resp.Sector = new SectorDataAccess().GetSectorbyCode(t24resp.Sector)?.Description ?? string.Empty;
                //t24resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(t24resp.Product)?.Name ?? string.Empty;
                //t24resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(t24resp.OpInstructions)?.Description ?? string.Empty;
            }
            catch
            {
                
            }

            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            //resp = T24Methods.FetchAccount(model.AccountNo);
            //NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            //resp = T24Methods.FetchAccount(model.AccountNo, model.CIFNo);
            //resp.Sector = new SectorDataAccess().GetSectorbyCode(resp.Sector)?.Description ?? string.Empty;
            //resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(resp.Product)?.Name ?? string.Empty;
            //resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(resp.OpInstructions)?.Description ?? string.Empty;
            //resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
            //resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
            //resp.CustomerDOB = resp.CustomerDOB.ToValidDateTime("dd-MM-yyyy");
            //resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
            var resp = new CustomerAccountDataAccess().GetCustomerAccountbyCIF(model.AccountNo, model.CIFNo).FirstOrDefault();
            if (resp == null)
                resp = new tbl_Customer_Accounts();
            else
            {
                resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
                resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
                resp.MotherMaidenName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MotherMaidenName);
                t24resp.Product = new AccountTypeDataAccess().GetAccountTypeById(resp.AccountTypeID.GetValueOrDefault())?.Name ?? string.Empty;
            }

            ViewBag.AccountInfo = resp;
            ViewBag.T24AccountInfo = t24resp;
            ViewBag.ExistingCardNumber = AuthorizationMethods.GetCustomerCardByAccountNo(model.AccountNo);

            return View(model);
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public JsonResult AuthorizeLinkingRequest(TransactionVM transaction)
        {
            
            string[] AccountstoLink = transaction.LinkRequest.Split(',');

            bool success = false;
            List<string> StatusEligibility = new List<string>();
            RequestDataAccess lst = new RequestDataAccess();
            AccountTypeDataAccess ada = new AccountTypeDataAccess();
            CustomerAccountVM custvm = new CustomerAccountVM();
            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();

            //string[] Successful = new string[AccountstoLink.Length];
            //string[] Failed = new string[AccountstoLink.Length];
            Dictionary<string, string> Successful = new Dictionary<string, string>();
            Dictionary<string, string> Failed = new Dictionary<string, string>();
            var SuccessCount = 0;
            var FailureCount = 0;
            if (transaction.AuthorizationStatus == "A")
            {

                var defaultAccountResp = T24Methods.FetchAccount(transaction.DrCustomerAccountNumber, transaction.CIF);

                EligibilityVM elgVM = new EligibilityVM();
                elgVM.IdentificationType = defaultAccountResp.IdentificationType;
                elgVM.IdentificationNumber = defaultAccountResp.IdentificationNo;
                elgVM.Resident = defaultAccountResp.ResidenceStatus;
                elgVM.Nationality = defaultAccountResp.Nationality;
                elgVM.SectorCode = defaultAccountResp.Sector;
                elgVM.AccountStatus = defaultAccountResp.AccountStatus;
                elgVM.FatherName = defaultAccountResp.FathersName;
                elgVM.MotherName = defaultAccountResp.MothersName;
                elgVM.DOB = defaultAccountResp.CustomerDOB;
                elgVM.PostingRestriction = defaultAccountResp.PostingRestriction;
                elgVM.OpInstruction = defaultAccountResp.OpInstructions;
                elgVM.AccountNo = transaction.DrCustomerAccountNumber;

                List<string> DefaultAccountStatusEligibility = CommonMethods.IsEligible(elgVM);

                if (DefaultAccountStatusEligibility.Count <= 0)
                {
                    int i = 0;
                    foreach (var item in AccountstoLink)
                    {
                        //resp = T24Methods.FetchAccount(item);
                        resp = T24Methods.FetchAccount(item, transaction.CIF);
                        resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
                        //StatusEligibility = CommonMethods.CheckStatusEligibility(resp.AccountStatus);
                        EligibilityVM elg = new EligibilityVM();
                        elg.IdentificationType = resp.IdentificationType;
                        elg.IdentificationNumber = resp.IdentificationNo;
                        elg.Resident = resp.ResidenceStatus;
                        elg.Nationality = resp.Nationality;
                        elg.SectorCode = resp.Sector;
                        elg.AccountStatus = resp.AccountStatus;
                        elg.FatherName = resp.FathersName;
                        elg.MotherName = resp.MothersName;
                        elg.DOB = resp.CustomerDOB;
                        elg.PostingRestriction = resp.PostingRestriction;
                        elg.OpInstruction = resp.OpInstructions;
                        elg.AccountNo = transaction.DrCustomerAccountNumber;
                        StatusEligibility = CommonMethods.IsEligible(elg);
                        if (StatusEligibility.Count <= 0)
                        {
                            var EligibleAccounts = ada.GetAccountTypeByCode(resp.AccountNature);
                            if (EligibleAccounts==null)
                            {
                                Failed.Add(item,"Account Not Eligible for Linking");
                                
                            }

                            custvm.AccountNo = item;
                            custvm.CIF = resp.CustomerID;
                            custvm.CardNo = transaction.CardNo;
                            custvm.AccountStatusActive = true;
                            custvm.AccountTitle = resp.AccountTitle;
                            custvm.Address = resp.AccountAddress;
                            custvm.Mobile = resp.AccountMobile;
                            custvm.CNIC = resp.IdentificationNo;
                            custvm.FatherName = resp.FathersName;
                            custvm.MotherMaidenName = resp.MothersName;
                            custvm.Nationality = resp.Nationality;
                            custvm.ResidenceStatus = resp.ResidenceStatus;
                            custvm.IdentificationType = resp.IdentificationType;

                            if (resp.CustomerDOB.Length == 8)
                            {
                                int y = int.Parse(resp.CustomerDOB.Substring(0, 4));
                                int m = int.Parse(resp.CustomerDOB.Substring(4, 2));
                                int d = int.Parse(resp.CustomerDOB.Substring(6, 2));
                                DateTime dt1 = new DateTime(y, m, d);
                                custvm.DateofBirth = dt1;
                            }
                            else
                            {
                                custvm.DateofBirth = null;
                            }
                            //custvm.DateofBirth = Convert.ToDateTime(resp.CustomerDOB);

                            //CommonMethods.SaveAccountInfo(custvm);
                            //AccountExists(item, resp, transaction.CardNo, transaction.CIF);//change made for Null Account Titles
                            var IsPrimaryCardExists = CommonMethods.IsPrimaryCardExist(item, transaction.CIF);
                            if (!IsPrimaryCardExists && !Failed.ContainsValue("Account Not Eligible for Linking"))
                            {
                                AccountExistsForLinking(item, resp, transaction.CardNo, transaction.CIF);
                            }

                            //Successful[i] = item;
                            if (!Failed.ContainsKey(item))
                            {
                                Successful.Add(item, string.Join(",", StatusEligibility.ToArray()));
                                i++;
                            }
                            
                        }
                        else
                        {
                            //Failed[i] = item;
                            Failed.Add(item, string.Join(",", StatusEligibility.ToArray()));
                            i++;
                        }
                    }
                }
                else
                {
                    int i = 0;
                    foreach (var fail in AccountstoLink)
                    {
                        //Failed[i] = fail;
                        Failed.Add(fail, string.Join(",", StatusEligibility.ToArray()));
                        i++;
                    }

                    transaction.AuthorizationStatus = "C";
                }
            }
            else
            {
                int i = 0;
                foreach (var fail in AccountstoLink)
                {
                    //Failed[i] = fail;
                    Failed.Add(fail, string.Join(",", StatusEligibility.ToArray()));
                    i++;
                }

            }

            SuccessCount = Successful.Count;
            FailureCount = Failed.Count;

            //for (int a = 0; a < Successful.Length; a++)
            //{
            //    if (!string.IsNullOrEmpty(Successful[a]))
            //    {
            //        SuccessCount = SuccessCount + 1;
            //    };
            //}

            //for (int b = 0; b < Failed.Length; b++)
            //{
            //    if (!string.IsNullOrEmpty(Failed[b]))
            //    {
            //        FailureCount = FailureCount + 1;
            //    };
            //}

            if (FailureCount >=1)
            {
                success = lst.UpdateRequestAuthorization(transaction.RequestId, "C", transaction.AuthorizeComments, true, true);
                string ErrorString = string.Join(",", StatusEligibility.ToArray());
                return Json(new { IsSuccess = false, LinkedSuccessfully = SuccessCount, LinkedFailed = FailureCount, ErrorMessage = Failed }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                success = lst.UpdateRequestAuthorization(transaction.RequestId, transaction.AuthorizationStatus, transaction.AuthorizeComments, true, true);
            }

            
            return Json(new { IsSuccess = success, LinkedSuccessfully = SuccessCount, LinkedFailed = FailureCount}, JsonRequestBehavior.AllowGet);
            //return Json(new { IsSuccess = true, LinkedSuccessfully = new int[] { }, LinkedFailed = new int[] { } }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public JsonResult AuthorizeDeLinkingRequest(TransactionVM transaction)
        {
            string[] AccountstoDeLink = transaction.DeLinkRequest.Split(',');
            bool success = false;
            
            CustomerAccountDataAccess model = new CustomerAccountDataAccess();
            RequestDataAccess lst = new RequestDataAccess();
            if (transaction.AuthorizationStatus == "A")
            {
                foreach (var item in AccountstoDeLink)
                {
                    var CardExistforDelinking=CommonMethods.CardExistforDelinking(item, transaction.CIF);
                    // model.DeleteCustomerAccount(item);
                    if (CardExistforDelinking)
                    {
                        model.UpdateCustomerAccountStatus(item, transaction.CIF);
                    }
                   
                }
            }
            success = lst.UpdateRequestAuthorization(transaction.RequestId, transaction.AuthorizationStatus, transaction.AuthorizeComments, true, true);
            //return Json(success, JsonRequestBehavior.AllowGet);
            return Json(new { IsSuccess = success}, JsonRequestBehavior.AllowGet);
        }


        #endregion
        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult AuthorizeAccountTypesScreen(int? Id)
        {
            AccountTypeVM viewModel = new AccountTypeVM();
            ViewBag.RequestId = Id ?? 0;

            if (Id.HasValue)
            {
                viewModel = AccountTypeMethods.GetAccountTypeById(Id.GetValueOrDefault(), "P");
                if (viewModel == null)
                    return RedirectToAction("Authorize", "AccountTypes");
            }

            ViewBag.AccountTypes = new AccountTypeDataAccess().GetAccountTypes("A"); //AccountTypeMethods.GetAllAccountTypes();
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult AuthorizeCardChargesScreen(int? Id)
        {
            CardChargesVM viewModel = new CardChargesVM();
            ViewBag.AccountTypes = new AccountTypeDataAccess().GetAccountTypes("A");
            ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypes("A");
            ViewBag.CardCharges = new CardChargesDataAccess().GetCardCharges("A");
            ViewBag.Regions = new RegionDataAccess().GetAllRegions();
            if (Id.HasValue)
            {
                viewModel = CardChargesMethod.GetCardChargeDetail(Id.GetValueOrDefault(), "P");
                if (viewModel == null)
                    return RedirectToAction("Authorize", "CardCharges");
            }
            ViewBag.RequestId = Id ?? 0;
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult AuthorizeCardTypeScreen(int? Id)
        {
            CardTypeVM viewModel = new CardTypeVM();
            ViewBag.RequestId = Id ?? 0;
            viewModel = CardTypeMethods.GetCardTypeById(Id.GetValueOrDefault(), "P");
            if (viewModel == null)
                return RedirectToAction("Authorize", "CardType");

            ViewBag.CardTypes = new CardTypesDataAccess().GetCardTypes("A");//CardTypeMethods.GetAllCardTypes();
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        /// <summary>
        /// To Update AuthorizationStatus for AccountTypes, CardCharges and CardType
        /// </summary>
        /// <param name="RequestId"></param>
        /// <param name="RequestType">To determine whether its accountType, cardCharges or cardType</param>
        /// <param name="AuthorizationStatus"></param>
        /// <param name="AuthorizeComments"></param>
        /// <returns></returns>
        public ActionResult UpdateACAuthorizeScreen(int RequestId, string RequestType, string AuthorizationStatus, string AuthorizeComments)
        {
            dynamic lst = default(dynamic);
            bool success;
            switch (RequestType)
            {
                case "AccountTypes":
                    lst = new AccountTypeDataAccess();
                    break;
                case "CardCharges":
                    lst = new CardChargesDataAccess();
                    break;
                case "CardType":
                    lst = new CardTypesDataAccess();
                    break;
                default:
                    break;
            }

            success = lst.Authorize(RequestId, AuthorizationStatus, AuthorizeComments);
            return Json(success, JsonRequestBehavior.AllowGet);
            // return Json(true, JsonRequestBehavior.AllowGet);
        }

       

        //[AuthOp(RoleTitle = new string[] { "u" })]
        //public static void SaveAccountInfo(CISSB.AccountInfoResponse customer)
        //{
        //    bool flag = false;

        //    tbl_Customer_Accounts cust = new tbl_Customer_Accounts();
        //    try
        //    {
        //        using (var db = new SoneriCISEntities())
        //        {
        //            cust.AccountNo = customer.ACALTID5;
        //            cust.CIF = customer.CustomerID;
        //            cust.Currency = !string.IsNullOrEmpty(customer.AccountCurrency) ? customer.AccountCurrency : "PKR";
        //            cust.CardNo = null;
        //            if (customer.AccountStatus == "0001")
        //            {
        //                cust.AccountStatusActive = true;
        //            }
        //            else
        //            {
        //                cust.AccountStatusActive = false;
        //            }

        //            cust.AccountTitle = (customer.AccountTitle == null ? null : customer.AccountTitle);
        //            cust.Address = (customer.AccountAddress == null ? null : customer.AccountAddress);
        //            cust.Mobile = (customer.AccountMobile == null ? null : customer.AccountMobile);
        //            cust.CNIC = (customer.IdentificationNo == null ? null : customer.IdentificationNo);
        //            //cust.Salutation = (customer.Salutation == null ? null : customer.Salutation);
        //            //cust.DateofBirth        = (customer.DateofBirth==null ?null : customer.DateofBirth);
        //            //cust.MotherMaidenName   = (customer.MotherMaidenName == null ?null : customer.MotherMaidenName);
        //            //cust.Identification     = (customer.Identification == null ?null : customer.Identification);
        //            //cust.AddressType        = (customer.AddressType == null ?null : customer.AddressType);
        //            //cust.AccountTypeID      = (customer.AccountTypeID == null ?null : customer.AccountTypeID);
        //            //cust.WaiveCharges       = (customer.WaiveCharges == null ?null : customer.WaiveCharges);
        //            //cust.PassportNo         = (customer.PassportNo == null ?null : customer.PassportNo);
        //            //cust.LandlineNo         = (customer.LandlineNo==null ?null : customer.LandlineNo);
        //            //cust.Email              = (customer.Email == null ?null : customer.Email);
        //            //cust.Nationality        = (customer.Nationality == null ?null : customer.Nationality);
        //            //cust.AccountCategoryCode= (customer.AccountCategoryCode == null ?null : customer.AccountCategoryCode);
        //            //cust.PhoneOffice        = (customer.PhoneOffice == null ?null : customer.PhoneOffice);
        //            //cust.Company            = (customer.Company == null ?null : customer.Company);
        //            //cust.IdentificationType = (customer.IdentificationType == null ?null : customer.IdentificationType);
        //            //cust.Mobile2            = (customer.Mobile2 == null ?null : customer.Mobile2);
        //            //cust.Address2           = (customer.Address2 == null ?null : customer.Address2);
        //            //cust.Address3           = (customer.Address3 == null ?null : customer.Address3);
        //            //cust.MainMobile         = (customer.MainMobile == null ?null : customer.MainMobile);
        //            //cust.MainLandline       = (customer.MainLandline == null ?null : customer.MainLandline);
        //            //cust.MainAddress = (customer.MainAddress == null ? null : customer.MainAddress);
        //            db.tbl_Customer_Accounts.Add(cust);
        //            db.SaveChanges();
        //            flag = true;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    //return flag;
        //}

        //[AuthOp(RoleTitle = new string[] { "u" })]
        //public static void RequestAccount(CISSB.AccountInfoResponse resp)
        //{

        //    try
        //    {

        //        tbl_Request_Customer_Accounts cvm = new tbl_Request_Customer_Accounts();
        //        var db = new SoneriCISEntities();

        //        cvm.AccountNo = resp.ACALTID5;
        //        cvm.CIF = resp.CustomerID;
        //        // cvm.CardNo = check.FirstOrDefault().CardNo;
        //        if (string.IsNullOrEmpty(resp.AccountStatus))
        //        {
        //            cvm.AccountStatusActive = true;
        //        }
        //        else
        //        {
        //            cvm.AccountStatusActive = false;
        //        }
        //        //cvm.Salutation          = resp.
        //        cvm.AccountTitle = (resp.AccountTitle == null ? null : resp.AccountTitle);
        //        cvm.Address = (resp.AccountAddress == null ? null : resp.AccountAddress);
        //        cvm.Mobile = (resp.AccountMobile == null ? null : resp.AccountMobile);
        //        //cvm.DateofBirth         = resp.
        //        //cvm.MotherMaidenName    = resp.
        //        //cvm.Identification      = resp.
        //        cvm.CNIC = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
        //        //cvm.AddressType         = resp.
        //        cvm.AccountTypeID = (Convert.ToInt32(resp.AccountNature) == 0 ? 0 : Convert.ToInt32(resp.AccountNature));
        //        //cvm.WaiveCharges        = resp.
        //        //cvm.PassportNo          = resp.
        //        cvm.LandlineNo = (resp.AccountPhone == null ? null : resp.AccountPhone);
        //        //cvm.Email               = resp.
        //        //cvm.Nationality         = resp.
        //        //cvm.AccountCategoryCode = resp.
        //        cvm.PhoneOffice = (resp.OffPhone == null ? null : resp.OffPhone);
        //        //cvm.Company             = resp.
        //        //cvm.IdentificationType  = resp.
        //        //cvm.Mobile2             = resp.
        //        //cvm.Address2            = resp.
        //        //cvm.Address3            = resp.
        //        //cvm.MainMobile          = resp.
        //        //cvm.MainLandline        = resp. 
        //        //cvm.MainAddress         = resp.                       
        //        db.tbl_Request_Customer_Accounts.Add(cvm);
        //        db.SaveChanges();

        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}


        private static void AccountExists(string AccountNo,NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp, string CardNo,string CIF)
        {
            try
            {
                var BranchCode = "";
                if (AccountNo.Length < 15)
                {
                    BranchCode = string.IsNullOrEmpty(resp.BranchCode) ? string.Empty : resp.BranchCode.Substring(resp.BranchCode.Length - 4, 4);//resp.BranchCode.Substring(BranchCode.Length - 4, 4);

                    AccountNo = BranchCode + AccountNo;
                }
                
                tbl_Customer_Accounts cvm = new tbl_Customer_Accounts();
                var db = new SoneriCISEntities();
                //var check = db.tbl_Customer_Accounts.Where(x => x.AccountNo == resp.ACALTID5);
                var acuntNature = (from a in db.tbl_Account_Types where a.Code == resp.AccountNature select a.ID).ToList();
                CustomerAccountDataAccess check = new CustomerAccountDataAccess();
                var exists = check.GetCustomerAccountbyCIF(AccountNo,CIF);
                if (exists.Count <= 0)
                {
                    cvm.AccountNo = AccountNo;
                    cvm.CIF = resp.CustomerID;
                    cvm.Currency = !string.IsNullOrEmpty(resp.AccountCurrency) ? resp.AccountCurrency : "PKR";
                    cvm.CardNo = CardNo;
                    if (string.IsNullOrEmpty(resp.AccountStatus))
                    {
                        cvm.AccountStatusActive = true;
                    }
                    else
                    {
                        cvm.AccountStatusActive = false;
                    }

                    cvm.AccountTitle = (resp.AccountTitle == null ? null : resp.AccountTitle);
                    cvm.Address = (resp.AccountAddress == null ? null : resp.AccountAddress);
                    cvm.Address2 = (resp.CustomerAddress1 == null ? null : resp.CustomerAddress1);
                    cvm.Address2 = (resp.CustomerAddress2 == null ? null : resp.CustomerAddress2);
                    cvm.Mobile = (resp.AccountMobile == null ? null : resp.AccountMobile);
                    cvm.Mobile2 = (resp.SMS == null ? null : resp.SMS);
                    cvm.Mobile3 = (resp.SMS2 == null ? null : resp.SMS2);
                    cvm.Mobile4 = (resp.SMS3 == null ? null : resp.SMS3);
                    cvm.CNIC = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                    //cvm.Salutation = (resp.Salutation == null ? null : resp.Salutation);
                    //cvm.DateofBirth = Convert.ToDateTime(resp.CustomerDOB == null ? null : resp.CustomerDOB);
                    if (resp.CustomerDOB.Length == 8)
                    {
                        int y = int.Parse(resp.CustomerDOB.Substring(0, 4));
                        int m = int.Parse(resp.CustomerDOB.Substring(4, 2));
                        int d = int.Parse(resp.CustomerDOB.Substring(6, 2));
                        DateTime dt1 = new DateTime(y, m, d);
                        cvm.DateofBirth = dt1;
                    }
                    else
                    {
                        cvm.DateofBirth = null;
                    }
                    cvm.MotherMaidenName = (resp.MothersName == null ? null : resp.MothersName);
                    cvm.Identification = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                    //cvm.AddressType = (resp.AddressType == null ? null : resp.AddressType);
                    cvm.AccountTypeID = acuntNature.Count <= 0 ? 0 : acuntNature[0]; //Convert.ToInt32(resp.AccountNature == null ? null : resp.AccountNature);
                    //cvm.WaiveCharges = (resp.WaiveCharges == null ? null : resp.WaiveCharges);
                    //cvm.PassportNo = (resp.PassportNo == null ? null : resp.PassportNo);
                    cvm.LandlineNo = (resp.PhoneResidence == null ? null : resp.PhoneResidence);
                    cvm.Email = (resp.Email == null ? null : resp.Email);
                    cvm.Nationality = (resp.Nationality == null ? null : resp.Nationality);
                    //cvm.AccountCategoryCode = (resp.AccountCategoryCode == null ? null : resp.AccountCategoryCode);
                    cvm.PhoneOffice = (resp.OffPhone == null ? null : resp.OffPhone);
                    //cvm.Company = (resp.Company == null ? null : resp.Company);
                    cvm.IdentificationType = (resp.IdentificationType == null ? null : resp.IdentificationType);
                    
                    //cvm.Address2 = (resp.Address2 == null ? null : resp.Address2);
                    //cvm.Address3 = (resp.Address3 == null ? null : resp.Address3);
                    cvm.CustomerName = (resp.CustomerName == null ? null : resp.CustomerName);
                    cvm.FatherName = (resp.FathersName == null ? null : resp.FathersName);
                    cvm.ResidenceStatus = (resp.ResidenceStatus == null ? null : resp.ResidenceStatus);

                    db.tbl_Customer_Accounts.Add(cvm);
                    db.SaveChanges();

                }
                else
                {
                    tbl_Customer_Accounts result = new tbl_Customer_Accounts();
                    if (string.IsNullOrEmpty(CardNo))
                    {
                        result = (from p in db.tbl_Customer_Accounts
                                  where p.AccountNo == AccountNo && p.CIF == CIF
                                  select p).SingleOrDefault();
                    }
                    else
                    {
                        result = (from p in db.tbl_Customer_Accounts
                                  where p.AccountNo == AccountNo && p.CIF == CIF 
                                  && p.CardNo == CardNo
                                  select p).SingleOrDefault();

                        if (result == null)
                            result = (from p in db.tbl_Customer_Accounts
                                      where p.AccountNo == AccountNo && p.CIF == CIF
                                      select p).SingleOrDefault();
                    }

                    result.AccountNo = AccountNo;
                    /*result.CIF = resp.CustomerID;*/
                    /*result.Currency = !string.IsNullOrEmpty(resp.AccountCurrency) ? resp.AccountCurrency : "PKR";*/
                    result.CardNo = CardNo;
                    if (string.IsNullOrEmpty(resp.AccountStatus))
                    {
                        result.AccountStatusActive = true;
                    }
                    else
                    {
                        result.AccountStatusActive = false;
                    }

                    /*result.AccountTitle = (resp.AccountTitle == null ? null : resp.AccountTitle);*/
                    /*result.Address = (resp.AccountAddress == null ? null : resp.AccountAddress);*/
                    /*result.Address2 = (resp.CustomerAddress1 == null ? null : resp.CustomerAddress1);*/
                    /*result.Address2 = (resp.CustomerAddress2 == null ? null : resp.CustomerAddress2);*/
                    /*result.Mobile = (resp.AccountMobile == null ? null : resp.AccountMobile);*/
                    /*result.Mobile2 = (resp.SMS == null ? null : resp.SMS);*/
                    /*result.Mobile3 = (resp.SMS2 == null ? null : resp.SMS2);*/
                    /*result.Mobile4 = (resp.SMS3 == null ? null : resp.SMS3);*/
                    /*result.CNIC = (resp.IdentificationNo == null ? null : resp.IdentificationNo);*/
                    //result.Salutation = (resp.Salutation == null ? null : resp.Salutation);
                    
                    /*if (resp.CustomerDOB.Length == 8)
                    {
                        int y = int.Parse(resp.CustomerDOB.Substring(0, 4));
                        int m = int.Parse(resp.CustomerDOB.Substring(4, 2));
                        int d = int.Parse(resp.CustomerDOB.Substring(6, 2));
                        DateTime dt1 = new DateTime(y, m, d);
                        result.DateofBirth = dt1;
                    }
                    else
                    {
                        result.DateofBirth = null;
                    }*/
                    /*result.MotherMaidenName = (resp.MothersName == null ? null : resp.MothersName);*/
                    /*result.Identification = (resp.IdentificationNo == null ? null : resp.IdentificationNo);*/
                    //result.AddressType = (resp.AddressType == null ? null : resp.AddressType);
                    // result.AccountTypeID = acuntNature[0];//Convert.ToInt32(resp.AccountNature == null ? null : resp.AccountNature);
                    result.AccountTypeID = acuntNature.Count <= 0 ? 0 : acuntNature[0];
                    //result.WaiveCharges = (resp.WaiveCharges == null ? null : resp.WaiveCharges);
                    //result.PassportNo = (resp.PassportNo == null ? null : resp.PassportNo);
                    /*result.LandlineNo = (resp.PhoneResidence == null ? null : resp.PhoneResidence);*/
                    /*result.Email = (resp.Email == null ? null : resp.Email);*/
                    /*result.Nationality = (resp.Nationality == null ? null : resp.Nationality);*/
                    //result.AccountCategoryCode = (resp.AccountCategoryCode == null ? null : resp.AccountCategoryCode);
                    /*result.PhoneOffice = (resp.OffPhone == null ? null : resp.OffPhone);*/
                    //result.Company = (resp.Company == null ? null : resp.Company);
                    /*result.IdentificationType = (resp.IdentificationType == null ? null : resp.IdentificationType);*/

                    /*result.CustomerName = (resp.CustomerName == null ? null : resp.CustomerName);*/
                    /*result.FatherName = (resp.FathersName == null ? null : resp.FathersName);*/
                    /*result.ResidenceStatus = (resp.ResidenceStatus == null ? null : resp.ResidenceStatus);*/
                    db.Entry(result).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private static void AccountExistsForLinking(string AccountNo, NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp, string CardNo, string CIF)
        {
            try
            {
                var BranchCode = "";
                if (AccountNo.Length < 15)
                {
                    BranchCode = string.IsNullOrEmpty(resp.BranchCode) ? string.Empty : resp.BranchCode.Substring(resp.BranchCode.Length - 4, 4);//resp.BranchCode.Substring(BranchCode.Length - 4, 4);

                    AccountNo = BranchCode + AccountNo;
                }

                tbl_Customer_Accounts cvm = new tbl_Customer_Accounts();
                var db = new SoneriCISEntities();
                //var check = db.tbl_Customer_Accounts.Where(x => x.AccountNo == resp.ACALTID5);
                var acuntNature = (from a in db.tbl_Account_Types where a.Code == resp.AccountNature select a.ID).ToList();
                CustomerAccountDataAccess check = new CustomerAccountDataAccess();
                var exists = check.GetCustomerAccountbyCIF(AccountNo, CIF);
                if (exists.Count <= 0)
                {
                    cvm.AccountNo = AccountNo;
                    cvm.CIF = resp.CustomerID;
                    cvm.Currency = !string.IsNullOrEmpty(resp.AccountCurrency) ? resp.AccountCurrency : "PKR";
                    cvm.CardNo = CardNo;
                    if (string.IsNullOrEmpty(resp.AccountStatus))
                    {
                        cvm.AccountStatusActive = true;
                    }
                    else
                    {
                        cvm.AccountStatusActive = false;
                    }

                    cvm.AccountTitle = (resp.AccountTitle == null ? null : resp.AccountTitle);
                    cvm.Address = (resp.AccountAddress == null ? null : resp.AccountAddress);
                    cvm.Address2 = (resp.CustomerAddress1 == null ? null : resp.CustomerAddress1);
                    cvm.Address2 = (resp.CustomerAddress2 == null ? null : resp.CustomerAddress2);
                    cvm.Mobile = (resp.AccountMobile == null ? null : resp.AccountMobile);
                    cvm.Mobile2 = (resp.SMS == null ? null : resp.SMS);
                    cvm.Mobile3 = (resp.SMS2 == null ? null : resp.SMS2);
                    cvm.Mobile4 = (resp.SMS3 == null ? null : resp.SMS3);
                    cvm.CNIC = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                    //cvm.Salutation = (resp.Salutation == null ? null : resp.Salutation);
                    //cvm.DateofBirth = Convert.ToDateTime(resp.CustomerDOB == null ? null : resp.CustomerDOB);
                    if (resp.CustomerDOB.Length == 8)
                    {
                        int y = int.Parse(resp.CustomerDOB.Substring(0, 4));
                        int m = int.Parse(resp.CustomerDOB.Substring(4, 2));
                        int d = int.Parse(resp.CustomerDOB.Substring(6, 2));
                        DateTime dt1 = new DateTime(y, m, d);
                        cvm.DateofBirth = dt1;
                    }
                    else
                    {
                        cvm.DateofBirth = null;
                    }
                    cvm.MotherMaidenName = (resp.MothersName == null ? null : resp.MothersName);
                    cvm.Identification = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                    //cvm.AddressType = (resp.AddressType == null ? null : resp.AddressType);
                    cvm.AccountTypeID = acuntNature.Count <= 0 ? 0 : acuntNature[0]; //Convert.ToInt32(resp.AccountNature == null ? null : resp.AccountNature);
                    //cvm.WaiveCharges = (resp.WaiveCharges == null ? null : resp.WaiveCharges);
                    //cvm.PassportNo = (resp.PassportNo == null ? null : resp.PassportNo);
                    cvm.LandlineNo = (resp.PhoneResidence == null ? null : resp.PhoneResidence);
                    cvm.Email = (resp.Email == null ? null : resp.Email);
                    cvm.Nationality = (resp.Nationality == null ? null : resp.Nationality);
                    //cvm.AccountCategoryCode = (resp.AccountCategoryCode == null ? null : resp.AccountCategoryCode);
                    cvm.PhoneOffice = (resp.OffPhone == null ? null : resp.OffPhone);
                    //cvm.Company = (resp.Company == null ? null : resp.Company);
                    cvm.IdentificationType = (resp.IdentificationType == null ? null : resp.IdentificationType);

                    //cvm.Address2 = (resp.Address2 == null ? null : resp.Address2);
                    //cvm.Address3 = (resp.Address3 == null ? null : resp.Address3);
                    cvm.CustomerName = (resp.CustomerName == null ? null : resp.CustomerName);
                    cvm.FatherName = (resp.FathersName == null ? null : resp.FathersName);
                    cvm.ResidenceStatus = (resp.ResidenceStatus == null ? null : resp.ResidenceStatus);

                    db.tbl_Customer_Accounts.Add(cvm);
                    db.SaveChanges();

                }
                else
                {
                    tbl_Customer_Accounts result = new tbl_Customer_Accounts();
                    if (string.IsNullOrEmpty(CardNo))
                    {
                        result = exists.FirstOrDefault();
                    }
                    else
                    {
                        result = (from p in db.tbl_Customer_Accounts
                                  where p.AccountNo == AccountNo && p.CIF == CIF
                                  && p.CardNo == CardNo
                                  select p).SingleOrDefault();

                        if (result == null)
                            result = exists.FirstOrDefault();
                    }

                    if (result.AccountTitle !=null && result.AccountTitle!="" && result.AccountTitle!=string.Empty && result.AccountTitle!="undefined")
                    {
                        result.AccountNo = AccountNo;
                        result.CardNo = CardNo;
                        if (string.IsNullOrEmpty(resp.AccountStatus))
                        {
                            result.AccountStatusActive = true;
                        }
                        else
                        {
                            result.AccountStatusActive = false;
                        }
                        result.AccountTypeID = acuntNature.Count <= 0 ? 0 : acuntNature[0];
                        db.Entry(result).State = EntityState.Modified;
                        db.SaveChanges();

                    }
                    else
                    {
                        result.AccountNo = AccountNo;
                        result.CIF = resp.CustomerID; 
                        result.Currency = !string.IsNullOrEmpty(resp.AccountCurrency) ? resp.AccountCurrency : "PKR";
                         result.CardNo = CardNo;
                         if (string.IsNullOrEmpty(resp.AccountStatus))
                         {
                             result.AccountStatusActive = true;
                         }
                         else
                         {
                             result.AccountStatusActive = false;
                         }

                         result.AccountTitle = (resp.AccountTitle == null ? null : resp.AccountTitle);
                         result.Address = (resp.AccountAddress == null ? null : resp.AccountAddress);
                         result.Address2 = (resp.CustomerAddress1 == null ? null : resp.CustomerAddress1);
                         result.Address2 = (resp.CustomerAddress2 == null ? null : resp.CustomerAddress2);
                         result.Mobile = (resp.AccountMobile == null ? null : resp.AccountMobile);
                         result.Mobile2 = (resp.SMS == null ? null : resp.SMS);
                         result.Mobile3 = (resp.SMS2 == null ? null : resp.SMS2);
                         result.Mobile4 = (resp.SMS3 == null ? null : resp.SMS3);
                         result.CNIC = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                        //result.Salutation = (resp.Salutation == null ? null : resp.Salutation);

                        if (resp.CustomerDOB.Length == 8)
                        {
                            int y = int.Parse(resp.CustomerDOB.Substring(0, 4));
                            int m = int.Parse(resp.CustomerDOB.Substring(4, 2));
                            int d = int.Parse(resp.CustomerDOB.Substring(6, 2));
                            DateTime dt1 = new DateTime(y, m, d);
                            result.DateofBirth = dt1;
                        }
                        else
                        {
                            result.DateofBirth = null;
                        }
                        result.MotherMaidenName = (resp.MothersName == null ? null : resp.MothersName);
                        result.Identification = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                         //result.AddressType = (resp.AddressType == null ? null : resp.AddressType);
                         // result.AccountTypeID = acuntNature[0];//Convert.ToInt32(resp.AccountNature == null ? null : resp.AccountNature);
                         result.AccountTypeID = acuntNature.Count <= 0 ? 0 : acuntNature[0];
                         result.LandlineNo = (resp.PhoneResidence == null ? null : resp.PhoneResidence);
                         result.Email = (resp.Email == null ? null : resp.Email);
                         result.Nationality = (resp.Nationality == null ? null : resp.Nationality);
                         //result.AccountCategoryCode = (resp.AccountCategoryCode == null ? null : resp.AccountCategoryCode);
                         result.PhoneOffice = (resp.OffPhone == null ? null : resp.OffPhone);
                         //result.Company = (resp.Company == null ? null : resp.Company);
                         result.IdentificationType = (resp.IdentificationType == null ? null : resp.IdentificationType);

                         result.CustomerName = (resp.CustomerName == null ? null : resp.CustomerName);
                         result.FatherName = (resp.FathersName == null ? null : resp.FathersName);
                         result.ResidenceStatus = (resp.ResidenceStatus == null ? null : resp.ResidenceStatus);
                        db.Entry(result).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private static void AccountExists(int RequestId, string AccountNo,NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp, string MainAddress, string MainMobile, string MainLandline, string MainLandlineNo, string MainOfficeNo,string CIF, string CardNo = default(string))
        {
            try
            {

                //Update Request Customer Account Table
                CustomerRequestAccountDataAccess crda = new CustomerRequestAccountDataAccess();
                crda.UpdateCustomerRequestAccountOnAuthorization(RequestId, MainAddress, MainMobile, MainLandlineNo, MainOfficeNo);
                
                var BranchCode = "";
                if (AccountNo.Length < 15)
                {
                    BranchCode = string.IsNullOrEmpty(resp.BranchCode) ? string.Empty : resp.BranchCode.Substring(resp.BranchCode.Length - 4, 4);

                    AccountNo = BranchCode + AccountNo;
                }

                tbl_Customer_Accounts cvm = new tbl_Customer_Accounts();
                var db = new SoneriCISEntities();
                //var check = db.tbl_Customer_Accounts.Where(x => x.AccountNo == resp.ACALTID5);
                var acuntNature = (from a in db.tbl_Account_Types where a.Code == resp.AccountNature select a.ID).ToList();
                CustomerAccountDataAccess check = new CustomerAccountDataAccess();
                var exists = check.GetCustomerAccountbyCIF(AccountNo, CIF);

                if (exists.Count <= 0)
                {
                    cvm.AccountNo = AccountNo;
                    cvm.CIF = resp.CustomerID;
                    cvm.Currency = !string.IsNullOrEmpty(resp.AccountCurrency) ? resp.AccountCurrency : "PKR";
                    cvm.CardNo = null;
                    if (string.IsNullOrEmpty(resp.AccountStatus))
                    {
                        cvm.AccountStatusActive = true;
                    }
                    else
                    {
                        cvm.AccountStatusActive = false;
                    }

                    cvm.AccountTitle = (resp.AccountTitle == null ? null : resp.AccountTitle);
                    cvm.Address = (resp.AccountAddress == null ? null : resp.AccountAddress);
                    cvm.Address2 = (resp.CustomerAddress1 == null ? null : resp.CustomerAddress1);
                    cvm.Address2 = (resp.CustomerAddress2 == null ? null : resp.CustomerAddress2);
                    cvm.Mobile = (resp.AccountMobile == null ? null : resp.AccountMobile);
                    cvm.Mobile2 = (resp.SMS == null ? null : resp.SMS);
                    cvm.Mobile3 = (resp.SMS2 == null ? null : resp.SMS2);
                    cvm.Mobile4 = (resp.SMS3 == null ? null : resp.SMS3);
                    cvm.CNIC = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                    //cvm.Salutation = (resp.Salutation == null ? null : resp.Salutation);
                    //cvm.DateofBirth = Convert.ToDateTime(resp.CustomerDOB == null ? null : resp.CustomerDOB);
                    if (resp.CustomerDOB.Length == 8)
                    {
                        int y = int.Parse(resp.CustomerDOB.Substring(0, 4));
                        int m = int.Parse(resp.CustomerDOB.Substring(4, 2));
                        int d = int.Parse(resp.CustomerDOB.Substring(6, 2));
                        DateTime dt1 = new DateTime(y, m, d);
                        cvm.DateofBirth = dt1;
                    }
                    else
                        cvm.DateofBirth = null;

                    cvm.MotherMaidenName = (resp.MothersName == null ? null : resp.MothersName);
                    cvm.Identification = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                    //cvm.AddressType = (resp.AddressType == null ? null : resp.AddressType);
                    //cvm.AccountTypeID = acuntNature[0];//Convert.ToInt32(resp.AccountNature == null ? null : resp.AccountNature);
                    cvm.AccountTypeID = acuntNature.Count <= 0 ? 0 : acuntNature[0];
                    //cvm.WaiveCharges = (resp.WaiveCharges == null ? null : resp.WaiveCharges);
                    //cvm.PassportNo = (resp.PassportNo == null ? null : resp.PassportNo);
                    cvm.LandlineNo = (MainLandlineNo == null ? null : MainLandlineNo);
                    cvm.Email = (resp.Email == null ? null : resp.Email);
                    cvm.Nationality = (resp.Nationality == null ? null : resp.Nationality);
                    //cvm.AccountCategoryCode = (resp.AccountCategoryCode == null ? null : resp.AccountCategoryCode);
                    cvm.PhoneOffice = (MainOfficeNo == null ? null : MainOfficeNo);
                    //cvm.Company = (resp.Company == null ? null : resp.Company);
                    cvm.IdentificationType = (resp.IdentificationType == null ? null : resp.IdentificationType);
                    cvm.MainMobile = (MainMobile == null ? null : MainMobile);
                    cvm.MainLandline = (MainLandline == null ? null : MainLandline);
                    cvm.MainAddress = (MainAddress == null ? null : MainAddress);
                    cvm.CustomerName = (resp.CustomerName == null ? null : resp.CustomerName);
                    cvm.FatherName = (resp.FathersName == null ? null : resp.FathersName);
                    cvm.ResidenceStatus = (resp.ResidenceStatus == null ? null : resp.ResidenceStatus);
                    db.tbl_Customer_Accounts.Add(cvm);
                    db.SaveChanges();

                }
                else
                {
                    tbl_Customer_Accounts result = new tbl_Customer_Accounts();
                    if (string.IsNullOrEmpty(CardNo))
                    {
                        result = (from p in db.tbl_Customer_Accounts
                                  where p.AccountNo == AccountNo && p.CIF == CIF
                                  select p).FirstOrDefault();
                    }
                    else
                    {
                        result = (from p in db.tbl_Customer_Accounts
                                  where p.AccountNo == AccountNo && p.CIF == CIF
                                  && p.CardNo == CardNo
                                  select p).FirstOrDefault();

                    }


                    result.AccountNo = AccountNo;
                    /*result.CIF = resp.CustomerID;*/
                    result.CardNo = result.CardNo; //exists.FirstOrDefault().CardNo;
                    /*result.Currency = !string.IsNullOrEmpty(resp.AccountCurrency) ? resp.AccountCurrency : "PKR";*/
                    if (string.IsNullOrEmpty(resp.AccountStatus))
                    {
                        result.AccountStatusActive = true;
                    }
                    else
                    {
                        result.AccountStatusActive = false;
                    }

                    /*result.AccountTitle = (resp.AccountTitle == null ? null : resp.AccountTitle);*/
                    /*result.Address = (resp.AccountAddress == null ? null : resp.AccountAddress);*/
                    /* result.Address2 = (resp.CustomerAddress1 == null ? null : resp.CustomerAddress1);*/
                    /*result.Address2 = (resp.CustomerAddress2 == null ? null : resp.CustomerAddress2);*/
                    /*cvm.Mobile = (resp.AccountMobile == null ? null : resp.AccountMobile);*/
                    /*cvm.Mobile2 = (resp.SMS == null ? null : resp.SMS);*/
                    /*cvm.Mobile3 = (resp.SMS2 == null ? null : resp.SMS2);*/
                    /*cvm.Mobile4 = (resp.SMS3 == null ? null : resp.SMS3);*/
                    /*result.CNIC = (resp.IdentificationNo == null ? null : resp.IdentificationNo);*/
                    //result.Salutation = (resp.Salutation == null ? null : resp.Salutation);
                    /*result.DateofBirth = Convert.ToDateTime(resp.CustomerDOB == null ? null : resp.CustomerDOB);*/
                    /*if (resp.CustomerDOB.Length == 8)
                    {
                        int y = int.Parse(resp.CustomerDOB.Substring(0, 4));
                        int m = int.Parse(resp.CustomerDOB.Substring(4, 2));
                        int d = int.Parse(resp.CustomerDOB.Substring(6, 2));
                        DateTime dt1 = new DateTime(y, m, d);
                        result.DateofBirth = dt1;
                    }
                    else
                    {
                        result.DateofBirth = null;
                    }*/
                    /*result.MotherMaidenName = (resp.MothersName == null ? null : resp.MothersName);*/
                    /*result.Identification = (resp.IdentificationNo == null ? null : resp.IdentificationNo);*/
                    //result.AddressType = (resp.AddressType == null ? null : resp.AddressType);
                    result.AccountTypeID = acuntNature[0];//Convert.ToInt32(resp.AccountNature == null ? null : resp.AccountNature);
                    
                    /*result.AccountTypeID = acuntNature.Count <= 0 ? 0 : acuntNature[0];*/
                    
                    //result.WaiveCharges = (resp.WaiveCharges == null ? null : resp.WaiveCharges);
                    //result.PassportNo = (resp.PassportNo == null ? null : resp.PassportNo);
                    /*result.LandlineNo = (resp.PhoneResidence == null ? null : resp.PhoneResidence);*/
                    /*result.Email = (resp.Email == null ? null : resp.Email);*/
                    /*result.Nationality = (resp.Nationality == null ? null : resp.Nationality);*/
                    //result.AccountCategoryCode = (resp.AccountCategoryCode == null ? null : resp.AccountCategoryCode);
                    /*result.PhoneOffice = (resp.OffPhone == null ? null : resp.OffPhone);*/
                    //result.Company = (resp.Company == null ? null : resp.Company);
                    /*result.IdentificationType = (resp.IdentificationType == null ? null : resp.IdentificationType);*/
                    //result.Mobile2 = (resp.Mobile2 == null ? null : resp.Mobile2);
                    //result.Address2 = (resp.Address2 == null ? null : resp.Address2);
                    //result.Address3 = (resp.Address3 == null ? null : resp.Address3);
                    result.MainMobile = (MainMobile == null ? null : MainMobile);
                    result.MainLandline = (MainLandline == null ? null : MainLandline);
                    result.MainAddress = (MainAddress == null ? null : MainAddress);
                    result.LandlineNo = (MainLandlineNo == null ? null : MainLandlineNo);
                    result.PhoneOffice = (MainOfficeNo == null ? null : MainOfficeNo);
                    /*result.CustomerName = (resp.CustomerName == null ? null : resp.CustomerName);*/
                    /*result.FatherName = (resp.FathersName == null ? null : resp.FathersName);*/
                    /*result.ResidenceStatus = (resp.ResidenceStatus == null ? null : resp.ResidenceStatus);*/
                    db.Entry(result).State = EntityState.Modified;
                    db.SaveChanges();

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string FieldMappingAddress(string MainAddress, NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp)
        {
            string result="";
           
            if (MainAddress.Trim() != "Account Address" && MainAddress.Trim() != "Customer Address 1" && MainAddress.Trim() != "Customer Address 2")
            {
                result = MainAddress;
            }
            else if (MainAddress== "Account Address")
            {
                result = resp.AccountAddress;
            }else if (MainAddress == "Customer Address 1")
            {
                result = resp.CustomerAddress1;
            }else if (MainAddress == "Customer Address 2")
            {
                result = resp.CustomerAddress2;
            }
            return result;

        }

        public static string FieldMappingAddress(string MainAddress, tbl_Customer_Accounts resp)
        {
            string result = "";

            if (MainAddress.Trim() != "Account Address" && MainAddress.Trim() != "Customer Address 1" && MainAddress.Trim() != "Customer Address 2")
                result = MainAddress;

            else if (MainAddress == "Account Address")
                result = resp.Address;

            else if (MainAddress == "Customer Address 1")
                result = resp.Address2;

            else if (MainAddress == "Customer Address 2")
                result = resp.Address3;

            return result;
        }

        public static string FieldMappingMobile(string MainMobile, NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp)
        {
            string result = "";
            if(MainMobile == "Mobile")
            {
                result = resp.AccountMobile;
            }
            else if (MainMobile == "Mobile 1")
            {
                result = resp.SMS;
            }
            else if (MainMobile == "Mobile 2")
            {
                result = resp.SMS2;
            }
            else if (MainMobile == "Mobile 3")
            {
                result = resp.SMS3;
            }
            else
            {
                result = MainMobile;
            }
            return result;

        }

        public static string FieldMappingMobile(string MainMobile, tbl_Customer_Accounts resp)
        {
            string result = "";
            if (MainMobile == "Mobile")
                result = resp.Mobile;

            else if (MainMobile == "Mobile 1")
                result = resp.Mobile2;

            else if (MainMobile == "Mobile 2")
                result = resp.Mobile3;

            else if (MainMobile == "Mobile 3")
                result = resp.Mobile4;

            else
                result = MainMobile;

            return result;
        }

        public static string FieldMappingLandline(string MainLandline, NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp)
        {
            string result = "";
            if (string.IsNullOrEmpty(MainLandline) || MainLandline == "Residence")
            {
                result = resp.PhoneResidence;
            }
            else
            {
                result = resp.OffPhone;
            }

            return result;
        }

        public static string FieldMappingLandline(string MainLandline, tbl_Customer_Accounts resp)
        {
            string result = "";
            if (string.IsNullOrEmpty(MainLandline) || MainLandline == "Residence")
                result = resp.LandlineNo;

            else
                result = resp.PhoneOffice;

            return result;
        }

        public static string FieldMappingLandlineNo(string LandlineNo, NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp)
        {
            string result = "";
            if (string.IsNullOrEmpty(LandlineNo) || LandlineNo == "Landline No")
            {
                result = resp.PhoneResidence;
            }
            else
            {
                result = LandlineNo;
            }

            return result;
        }

        public static string FieldMappingLandlineNo(string LandlineNo, tbl_Customer_Accounts resp)
        {
            string result = "";
            if (string.IsNullOrEmpty(LandlineNo) || LandlineNo == "Landline No")
                result = resp.LandlineNo;

            else
                result = LandlineNo;

            return result;
        }

        public static string FieldMappingOfficeNo(string OfficeNo, NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp)
        {
            string result = "";
            if (string.IsNullOrEmpty(OfficeNo) || OfficeNo == "Office No")
            {
                result = resp.OffPhone;
            }
            else
            {
                result = OfficeNo;
            }

            return result;
        }

        public static string FieldMappingOfficeNo(string OfficeNo, tbl_Customer_Accounts resp)
        {
            string result = "";
            if (string.IsNullOrEmpty(OfficeNo) || OfficeNo == "Office No")
                result = resp.PhoneOffice;

            else
                result = OfficeNo;

            return result;
        }

        public static string OtherAddressMapping(string MainAddress)
        {
            string result = MainAddress;
            if (MainAddress != "Account Address" && MainAddress != "Customer Address 1" && MainAddress!= "Customer Address 2")
            {
                result = "Other";
            }

            return result;
        }
        public static string OtherMobileNoMapping(string MainMobileNo)
        {
            string result = MainMobileNo;
            if (MainMobileNo != "Mobile" && MainMobileNo != "Mobile 1" && MainMobileNo != "Mobile 2" && MainMobileNo != "Mobile 3")
            {
                result = "Other";
            }

            return result;
        }
        public static string OtherLandlineNoMapping(string MainLandlineNo)
        {
            string result = MainLandlineNo;
            if (MainLandlineNo != "Landline No")
            {
                result = "Other";
            }

            return result;
        }
        public static string OtherOfficeNoMapping(string MainOfficeNumber)
        {
            string result = MainOfficeNumber;
            if (MainOfficeNumber != "Office No")
            {
                result = "Other";
            }

            return result;
        }




        private static void AccountExistsForLinkingDelinkingPrimary(string AccountNo, NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp, string CardNo, string CIF)
        {
            try
            {
                var BranchCode = "";
                if (AccountNo.Length < 15)
                {
                    BranchCode = string.IsNullOrEmpty(resp.BranchCode) ? string.Empty : resp.BranchCode.Substring(resp.BranchCode.Length - 4, 4);//resp.BranchCode.Substring(BranchCode.Length - 4, 4);

                    AccountNo = BranchCode + AccountNo;
                }

                tbl_Customer_Accounts cvm = new tbl_Customer_Accounts();
                var db = new SoneriCISEntities();
                var acuntNature = (from a in db.tbl_Account_Types where a.Code == resp.AccountNature select a.ID).ToList();
                CustomerAccountDataAccess check = new CustomerAccountDataAccess();
                var exists = check.GetCustomerAccountbyCIF(AccountNo, CIF);
                if (exists.Count <= 0)
                {
                    cvm.AccountNo = AccountNo;
                    cvm.CIF = resp.CustomerID;
                    cvm.Currency = !string.IsNullOrEmpty(resp.AccountCurrency) ? resp.AccountCurrency : "PKR";
                    cvm.CardNo = CardNo;
                    if (string.IsNullOrEmpty(resp.AccountStatus))
                    {
                        cvm.AccountStatusActive = true;
                    }
                    else
                    {
                        cvm.AccountStatusActive = false;
                    }

                    cvm.AccountTitle = (resp.AccountTitle == null ? null : resp.AccountTitle);
                    cvm.Address = (resp.AccountAddress == null ? null : resp.AccountAddress);
                    cvm.Address2 = (resp.CustomerAddress1 == null ? null : resp.CustomerAddress1);
                    cvm.Address2 = (resp.CustomerAddress2 == null ? null : resp.CustomerAddress2);
                    cvm.Mobile = (resp.AccountMobile == null ? null : resp.AccountMobile);
                    cvm.Mobile2 = (resp.SMS == null ? null : resp.SMS);
                    cvm.Mobile3 = (resp.SMS2 == null ? null : resp.SMS2);
                    cvm.Mobile4 = (resp.SMS3 == null ? null : resp.SMS3);
                    cvm.CNIC = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                    //cvm.Salutation = (resp.Salutation == null ? null : resp.Salutation);
                    //cvm.DateofBirth = Convert.ToDateTime(resp.CustomerDOB == null ? null : resp.CustomerDOB);
                    if (resp.CustomerDOB.Length == 8)
                    {
                        int y = int.Parse(resp.CustomerDOB.Substring(0, 4));
                        int m = int.Parse(resp.CustomerDOB.Substring(4, 2));
                        int d = int.Parse(resp.CustomerDOB.Substring(6, 2));
                        DateTime dt1 = new DateTime(y, m, d);
                        cvm.DateofBirth = dt1;
                    }
                    else
                    {
                        cvm.DateofBirth = null;
                    }
                    cvm.MotherMaidenName = (resp.MothersName == null ? null : resp.MothersName);
                    cvm.Identification = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                    //cvm.AddressType = (resp.AddressType == null ? null : resp.AddressType);
                    cvm.AccountTypeID = acuntNature.Count <= 0 ? 0 : acuntNature[0]; //Convert.ToInt32(resp.AccountNature == null ? null : resp.AccountNature);
                    //cvm.WaiveCharges = (resp.WaiveCharges == null ? null : resp.WaiveCharges);
                    //cvm.PassportNo = (resp.PassportNo == null ? null : resp.PassportNo);
                    cvm.LandlineNo = (resp.PhoneResidence == null ? null : resp.PhoneResidence);
                    cvm.Email = (resp.Email == null ? null : resp.Email);
                    cvm.Nationality = (resp.Nationality == null ? null : resp.Nationality);
                    //cvm.AccountCategoryCode = (resp.AccountCategoryCode == null ? null : resp.AccountCategoryCode);
                    cvm.PhoneOffice = (resp.OffPhone == null ? null : resp.OffPhone);
                    //cvm.Company = (resp.Company == null ? null : resp.Company);
                    cvm.IdentificationType = (resp.IdentificationType == null ? null : resp.IdentificationType);

                   //cvm.Address2 = (resp.Address2 == null ? null : resp.Address2);
                   // cvm.Address3 = (resp.Address3 == null ? null : resp.Address3);
                    cvm.CustomerName = (resp.CustomerName == null ? null : resp.CustomerName);
                    cvm.FatherName = (resp.FathersName == null ? null : resp.FathersName);
                    cvm.ResidenceStatus = (resp.ResidenceStatus == null ? null : resp.ResidenceStatus);

                    db.tbl_Customer_Accounts.Add(cvm);
                    db.SaveChanges();

                }
                else
                {
                    tbl_Customer_Accounts result = new tbl_Customer_Accounts();
                    if (string.IsNullOrEmpty(CardNo))
                    {
                        result = (from p in db.tbl_Customer_Accounts
                                  where p.AccountNo == AccountNo && p.CIF == CIF
                                  select p).SingleOrDefault();
                    }
                    else
                    {
                        result = (from p in db.tbl_Customer_Accounts
                                  where p.AccountNo == AccountNo && p.CIF == CIF
                                  && p.CardNo == CardNo
                                  select p).SingleOrDefault();

                        if (result == null)
                            result = (from p in db.tbl_Customer_Accounts
                                      where p.AccountNo == AccountNo && p.CIF == CIF
                                      select p).SingleOrDefault();
                    }

                    result.AccountNo = AccountNo;
                    result.CIF = resp.CustomerID;
                    result.Currency = !string.IsNullOrEmpty(resp.AccountCurrency) ? resp.AccountCurrency : "PKR";
                    result.CardNo = CardNo;
                    if (string.IsNullOrEmpty(resp.AccountStatus))
                    {
                        result.AccountStatusActive = true;
                    }
                    else
                    {
                        result.AccountStatusActive = false;
                    }

                    result.AccountTitle = (resp.AccountTitle == null ? null : resp.AccountTitle);
                    result.Address = (resp.AccountAddress == null ? null : resp.AccountAddress);
                    result.Address2 = (resp.CustomerAddress1 == null ? null : resp.CustomerAddress1);
                    result.Address2 = (resp.CustomerAddress2 == null ? null : resp.CustomerAddress2);
                    result.Mobile = (resp.AccountMobile == null ? null : resp.AccountMobile);
                    result.Mobile2 = (resp.SMS == null ? null : resp.SMS);
                    result.Mobile3 = (resp.SMS2 == null ? null : resp.SMS2);
                    result.Mobile4 = (resp.SMS3 == null ? null : resp.SMS3);
                    result.CNIC = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                    //result.Salutation = (resp.Salutation == null ? null : resp.Salutation);

                    if (resp.CustomerDOB.Length == 8)
                    {
                        int y = int.Parse(resp.CustomerDOB.Substring(0, 4));
                        int m = int.Parse(resp.CustomerDOB.Substring(4, 2));
                        int d = int.Parse(resp.CustomerDOB.Substring(6, 2));
                        DateTime dt1 = new DateTime(y, m, d);
                        result.DateofBirth = dt1;
                    }
                    else
                    {
                        result.DateofBirth = null;
                    }
                    result.MotherMaidenName = (resp.MothersName == null ? null : resp.MothersName);
                    result.Identification = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                    //result.AddressType = (resp.AddressType == null ? null : resp.AddressType);
                    // result.AccountTypeID = acuntNature[0];//Convert.ToInt32(resp.AccountNature == null ? null : resp.AccountNature);
                    result.AccountTypeID = acuntNature.Count <= 0 ? 0 : acuntNature[0];
                    //result.WaiveCharges = (resp.WaiveCharges == null ? null : resp.WaiveCharges);
                    ///result.PassportNo = (resp.PassportNo == null ? null : resp.PassportNo);
                    result.LandlineNo = (resp.PhoneResidence == null ? null : resp.PhoneResidence);
                    result.Email = (resp.Email == null ? null : resp.Email);
                    result.Nationality = (resp.Nationality == null ? null : resp.Nationality);
                    //result.AccountCategoryCode = (resp.AccountCategoryCode == null ? null : resp.AccountCategoryCode);
                    result.PhoneOffice = (resp.OffPhone == null ? null : resp.OffPhone);
                    //result.Company = (resp.Company == null ? null : resp.Company);
                    result.IdentificationType = (resp.IdentificationType == null ? null : resp.IdentificationType);

                    result.CustomerName = (resp.CustomerName == null ? null : resp.CustomerName);
                    result.FatherName = (resp.FathersName == null ? null : resp.FathersName);
                    result.ResidenceStatus = (resp.ResidenceStatus == null ? null : resp.ResidenceStatus);
                    db.Entry(result).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}