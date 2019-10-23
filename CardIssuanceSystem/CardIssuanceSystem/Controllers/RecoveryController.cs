using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardIssuanceSystem.DAL;
using CardIssuanceSystem.DAL.DataAccessClasses;
using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.Filters;
using System.Text.RegularExpressions;
using System.IO;
using CardIssuanceSystem.Core.Methods;
using System.Text;

namespace CardIssuanceSystem.Controllers
{
    public class RecoveryController : BaseController
    {
        [AuthOp(RoleTitle = new string[] { "a" })]
        // GET: Recovery
        public ActionResult Index()
        {
            RegionDataAccess rda = new RegionDataAccess();
            List<tbl_Region> lst = new List<tbl_Region>();
            tbl_Region lst2 = new tbl_Region();
            lst2.ID = 0;
            lst2.Title = "Please Select";
            lst2.Description = "";
            lst2.FED = "0";
            lst2.IsActive = true;


            lst = rda.GetAllRegions();
            lst.Insert(0, lst2);
            return View(lst);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult RunRecovery(bool IsFullRecovery, string FromBranchCode, string ToBranchCode)
        {
            bool IsRecoveryUpdated = false;
            //retrieving the list of pending recovery
            List<tbl_Card_Charges_Recovery> lstRecovery = new List<tbl_Card_Charges_Recovery>();
            RecoveryDataAccess rda = new RecoveryDataAccess();
            TransactionDataAccess tda = new TransactionDataAccess();
            bool UpdateCardRecovery = false;
            bool UpdateLastTransactionDate = false;
            bool AddLowBalanceRecovery = false;
            long AddRecoveryTransactions = 0;
            bool UpdateRecoveryTransactions = false;
            bool UpdateFEDTransactions = false;
            bool UpdateReversalTransactions = false;
            long AddFEDTransactions = 0;
            long AddReversalTransactions = 0;
            bool IsDBError = false;
            string STAN = "";
            string STAN1 = "";
            string RecoveryYear = DateTime.Now.ToString("yy");
            lstRecovery = rda.GetPendingRecovery(IsFullRecovery, FromBranchCode, ToBranchCode);
            string Data = " Full Recovery: " + IsFullRecovery + " FromBranch: " + FromBranchCode + " ToBranch " + ToBranchCode;
            FileHelper.RecoveryErrorLog("Recovery Records Fetch", Data, lstRecovery.Count + " Records Found", new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
            int PartialRecovery = 0;
            int FullRecovery = 0;
            bool UpdateStop;
            //Work for Null Response in db
            var CheckNullTransaction = tda.GetLastTransaction();
            if (CheckNullTransaction.ResponseCode == null)
            {
                var PreviousNullRecord =RunNullRecovery(CheckNullTransaction);
                if (PreviousNullRecord==false)
                {
                    return Json(new { IsSuccess = false, ErrorMessage = "Process Halted! Please Run Again ", Response = false }, JsonRequestBehavior.AllowGet);
                }
                
            }
            //running Recovery on each pending recovery
            for (int i = 0; i < lstRecovery.Count; i++)
            {
                var ShouldStop = ReadFile();
                if (ShouldStop == "true")
                {
                    UpdateStop = UpdateIsStop("false");
                    break;

                }
                double fedShouldDeduct = 0;
                double amountShouldDeduct = 0;
                bool isFullRecovery = false;

                if (string.IsNullOrEmpty(lstRecovery[i].CIF))
                    continue;
                if (IsFullRecovery == true)
                {
                    var isHot = new CustomerCardDataAccess().IsCardHotMarked(lstRecovery[i].CardNo);
                    if (isHot.ToLower() == "card already hot marked")
                    {
                        var logTime = new UserLogDataAccess().GetUserLogByEntityId(lstRecovery[i].CardID.GetValueOrDefault()).OrderByDescending(e => e.ID).FirstOrDefault()?.ActionTimestamp;
                        if (logTime != null)
                        {
                            if (logTime.GetValueOrDefault().Date <= lstRecovery[i].ApplicableDate.GetValueOrDefault().Date)
                            {
                                lstRecovery[i].IsValid = false;
                                lstRecovery[i].DeducatedAmount= Math.Round(lstRecovery[i].DeducatedAmount??Convert.ToDouble(lstRecovery[i].DeducatedAmount), 2);
                                
                                UpdateCardRecovery = rda.UpdateCardRecovery(lstRecovery[i]);
                                FileHelper.RecoveryErrorLog("Recovery Amount Update DB", "RecoveryID:" + lstRecovery[i].ID + "Amount:-", "Status:" + UpdateCardRecovery, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                                continue;
                            }
                        }
                    }
                }
                //Fetch details from T24 against Account No
                NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType response = T24Methods.FetchAccount(lstRecovery[i].AccountNo, lstRecovery[i].CIF);

                FileHelper.RecoveryErrorLog("T24 Account Fetch", "Account#:" + lstRecovery[i].AccountNo + "CIF:" + lstRecovery[i].CIF, response.ErrorField == "" ? "Success" : "Failure", new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                string BrCode = string.IsNullOrEmpty(response.BranchCode) ? string.Empty : response.BranchCode.Substring(response.BranchCode.Length - 4, 4).ToString();
                tbl_Card_Charges_Recovery recovery = lstRecovery[i];
                //have to check for posting restrictions, response pending from business
                if (string.IsNullOrEmpty(response.ErrorField))
                {
                    // FED Percentage to deduct (FED %)
                    var fedToDeduct = double.Parse(new RegionDataAccess().GetRegionsbyID(BrCode, true).OrderByDescending(e => e.ID).FirstOrDefault()?.FED ?? "0");
                    // Current Balance
                    double Balance = double.Parse((!string.IsNullOrEmpty(response.AvailableBalance)) ? response.AvailableBalance : "0");
                    double applicableAmount = lstRecovery[i].ApplicableAmount ?? 0;
                    double DeductedAmount = lstRecovery[i].DeducatedAmount ?? 0;
                    double remainingAmountToDeduct = applicableAmount - DeductedAmount;
                    //remainingAmountToDeduct = Math.Truncate(100 * remainingAmountToDeduct) / 100;
                    remainingAmountToDeduct = Math.Round(remainingAmountToDeduct, 2);
                    //if account type code doesn't match then set validity false and skip this record
                    string AccountCode = Convert.ToString(lstRecovery[i].AccountCode) ?? string.Empty;
                    //test during recovery test run
                    AccountCode = response.AccountNature;
                    //Check Account Status //if In-active then skip this record
                    if (Balance > 1.2 && remainingAmountToDeduct > 0)
                    {
                        if (response.AccountNature == AccountCode)
                        {
                            /* amount that need to deduct */
                            var finalDeductAmount = remainingAmountToDeduct + (remainingAmountToDeduct * (fedToDeduct / 100));
                            if (finalDeductAmount > Balance)
                            {

                                /* new fed need to deduct based on the user balance */
                                fedShouldDeduct = (fedToDeduct > 0) ? (Balance * fedToDeduct) / (Math.Round(fedToDeduct) + 100)/*Balance * (fedToDeduct / 100)*/ : 0;
                                /* new amount that need to deduct based on the user balance */
                                amountShouldDeduct = (Balance * 100) / (Math.Round(fedToDeduct) + 100);//Balance - fedShouldDeduct;

                                /*Round to two precision a/c to requirement*/
                                //fedShouldDeduct = Math.Truncate(100 * fedShouldDeduct) / 100;//Math.Floor(fedShouldDeduct);
                                fedShouldDeduct = Math.Round(fedShouldDeduct, 2);
                                /*amountShouldDeduct = Math.Truncate(100 * amountShouldDeduct) / 100;*///Math.Floor(amountShouldDeduct);
                                amountShouldDeduct = Math.Round(amountShouldDeduct, 2);
                                PartialRecovery++;
                                isFullRecovery = false;
                            }
                            else
                            {
                                /* new fed need to deduct based on the amount that need to deduct */
                                fedShouldDeduct = (fedToDeduct > 0) ? remainingAmountToDeduct * (fedToDeduct / 100) : 0;
                                /* new amount need to deduct based on the amount that need to deduct */
                                amountShouldDeduct = finalDeductAmount - fedShouldDeduct;

                                /*Round to two precision a/c to requirement*/
                                /*fedShouldDeduct = Math.Truncate(100 * fedShouldDeduct) / 100;*///Math.Floor(fedShouldDeduct);
                                fedShouldDeduct = Math.Round(fedShouldDeduct, 2);
                                /*amountShouldDeduct = Math.Truncate(100 * amountShouldDeduct) / 100;*///Math.Floor(amountShouldDeduct);
                                amountShouldDeduct = Math.Round(amountShouldDeduct, 2);
                                FullRecovery++;
                                isFullRecovery = true;
                            }

                            if (amountShouldDeduct > 0)
                            {
                                /*if income is less than 0.05 then income and FED both wont be charged*/
                                if (amountShouldDeduct < 0.05d)
                                    continue;

                                STAN = Core.Methods.CommonMethods.GetRecoverySTAN(lstRecovery[i].ID.ToString(), "I");
                                
                                
                                FileHelper.RecoveryErrorLog("Income Transaction Generation", "RecoveryID:" + lstRecovery[i].ID + "STAN :" + STAN + "Ac#:" + lstRecovery[i].AccountNo + "Narrartion:" + lstRecovery[i].ChargesDescription + "Amount:" + amountShouldDeduct, "Pending", new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                                AddRecoveryTransactions = AddTransaction(lstRecovery[i].ChargesDescription, Convert.ToDecimal(amountShouldDeduct), lstRecovery[i].AccountNo, "RC", STAN, lstRecovery[i].CardNo);
                                if (AddRecoveryTransactions <= 0)
                                {
                                    IsDBError = true;
                                    break;
                                }
                                CISSB.TransactionInfoResponse TransResponse = T24Methods.RecoveryDebitTransaction(lstRecovery[i].AccountNo, Convert.ToDecimal(amountShouldDeduct)
    , lstRecovery[i].ChargesDescription, "RC", BrCode, STAN, lstRecovery[i].CardNo);

                                
                                //update recovery amount in database
                                FileHelper.RecoveryErrorLog("Income Transaction Response", "Response Code:" + TransResponse.ResponseCode + "Response Desc:" + TransResponse.ResponseCodeDescription + "FT: " + TransResponse.TransactionIdentificationNo, "Response Code:" + TransResponse.ResponseCode, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());

                                UpdateRecoveryTransactions = UpdateRecoveryTransaction(TransResponse, AddRecoveryTransactions);

                                if (TransResponse.ResponseCode == "0000")
                                {
                                    if (fedShouldDeduct > 0)
                                    {
                                        /*if Income is less than 0.10 then only income will be charged and FED wont*/
                                        if (amountShouldDeduct >= 0.10d)
                                        {


                                            STAN1 = Core.Methods.CommonMethods.GetRecoverySTAN(lstRecovery[i].ID.ToString(), "F");
                                            FileHelper.RecoveryErrorLog("FED Transaction Generation", "RecoveryID:" + lstRecovery[i].ID + "STAN :" + STAN1 + "Ac#:" + lstRecovery[i].AccountNo + "Narrartion:" + $"FED on Card Chgs" + "Amount:" + fedShouldDeduct, "Pending", new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                                            AddFEDTransactions = AddFEDTransaction($"FED on Card Chgs", Convert.ToDecimal(fedShouldDeduct), lstRecovery[i].AccountNo, "RC", STAN1, lstRecovery[i].CardNo);

                                            CISSB.TransactionInfoResponse FedTransResponse = T24Methods.RecoveryFEDDebitTransaction(lstRecovery[i].AccountNo, Convert.ToDecimal(fedShouldDeduct)
                    , $"FED on Card Chgs", "RC", BrCode, STAN1, lstRecovery[i].CardNo);
                                            FileHelper.RecoveryErrorLog("FED Transaction Response ", "Response Code:" + FedTransResponse.ResponseCode + "Response Desc:" + FedTransResponse.ResponseCodeDescription + "FT: " + FedTransResponse.TransactionIdentificationNo, "Response Code:" + FedTransResponse.ResponseCode, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());

                                            UpdateFEDTransactions = UpdateRecoveryTransaction(FedTransResponse, AddFEDTransactions);

                                            if (FedTransResponse.ResponseCode == "0000")
                                            {
                                                IsRecoveryUpdated = UpdateCardRecoveryAmount(lstRecovery[i], ref rda, amountShouldDeduct);
                                                if (!IsRecoveryUpdated)
                                                {
                                                    IsDBError = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                UpdateLastTransactionDate = rda.UpdateLastTransactionDate(lstRecovery[i].ID);
                                                if (!UpdateLastTransactionDate)
                                                {
                                                    IsDBError = true;
                                                    break;
                                                }
                                                FileHelper.RecoveryErrorLog("Recovery Last Deduction Date Update DB", "RecoveryID:" + lstRecovery[i].ID, "Status:" + UpdateLastTransactionDate, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                                                var RevSTAN = Core.Methods.CommonMethods.GetRecoverySTAN(lstRecovery[i].ID.ToString(), "X");
                                                AddReversalTransactions = AddReverseTransaction(lstRecovery[i].ChargesDescription, Convert.ToDecimal(amountShouldDeduct), lstRecovery[i].AccountNo, "RC", lstRecovery[i].CardNo,RevSTAN);

                                                var revTrans = T24Methods.ReverseTransactionLatest(lstRecovery[i].AccountNo, Convert.ToDecimal(amountShouldDeduct)
            , lstRecovery[i].ChargesDescription, "RC", BrCode,lstRecovery[i].CardNo,RevSTAN);

                                                UpdateReversalTransactions = UpdateRecoveryTransaction(revTrans, AddReversalTransactions);
                                                if (isFullRecovery)
                                                    FullRecovery--;
                                                else
                                                    PartialRecovery--;
                                            }
                                            //}
                                        }
                                        else
                                        {
                                            IsRecoveryUpdated = UpdateCardRecoveryAmount(lstRecovery[i], ref rda, amountShouldDeduct);
                                            if (!IsRecoveryUpdated)
                                            {
                                                IsDBError = true;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        IsRecoveryUpdated = UpdateCardRecoveryAmount(lstRecovery[i], ref rda, amountShouldDeduct);
                                        if (!IsRecoveryUpdated)
                                        {
                                            IsDBError = true;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    UpdateLastTransactionDate = rda.UpdateLastTransactionDate(lstRecovery[i].ID);
                                    if (!UpdateLastTransactionDate)
                                    {
                                        IsDBError = true;
                                        break;
                                    }
                                    FileHelper.RecoveryErrorLog("Recovery Last Deduction Date Update DB", "RecoveryID:" + lstRecovery[i].ID, "Status:" + UpdateLastTransactionDate, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                                    //                                T24Methods.ReverseTransaction(lstRecovery[i].AccountNo, Convert.ToDecimal(amountShouldDeduct)
                                    //, lstRecovery[i].ChargesDescription, "RC", BrCode);
                                    if (isFullRecovery)
                                        FullRecovery--;
                                    else
                                        PartialRecovery--;
                                }
                                //}
                            }
                        }
                        else
                        {
                            recovery.IsValid = false;

                            recovery.DeducatedAmount = Math.Round(recovery.DeducatedAmount??Convert.ToDouble(recovery.DeducatedAmount), 2);
                            UpdateCardRecovery = rda.UpdateCardRecovery(recovery);
                            if (!IsRecoveryUpdated)
                            {
                                IsDBError = true;
                                break;//change 14 july18. Recommended by Amir Sultan sb to stop mis-entry in CIS DB against T24 Transaction
                            }
                            FileHelper.RecoveryErrorLog("Recovery Amount Update DB", "RecoveryID:" + lstRecovery[i].ID + "Amount:" + amountShouldDeduct, "Status:" + UpdateCardRecovery, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                        }
                    }
                    else
                    {
                        UpdateLastTransactionDate = rda.UpdateLastTransactionDate(lstRecovery[i].ID);
                        if (!UpdateLastTransactionDate)
                        {
                            IsDBError = true;
                            break;
                        }
                        FileHelper.RecoveryErrorLog("Recovery Last Deduction Date Update DB", "RecoveryID:" + lstRecovery[i].ID, "Status:" + UpdateLastTransactionDate, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                        AddLowBalanceRecovery = rda.AddLowBalanceRecovery(lstRecovery[i].ID);
                        if (!AddLowBalanceRecovery)
                        {
                            IsDBError = true;
                            break;
                        }
                        FileHelper.RecoveryErrorLog("Recovery Low Balance Update DB", "RecoveryID:" + lstRecovery[i].ID, "Status:" + AddLowBalanceRecovery, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                    }

                }
            }
            //setting the recovery numbers label
            string Total = lstRecovery.Count.ToString();
            string Full = FullRecovery.ToString();
            string Partial = PartialRecovery.ToString();
            var result1 = new { Total = Total, Full = Full, Partial = Partial };
            var IsSuccess = true;
            string ErrorMessage1 = "Recovery Process Halted because of Database Connection Error!";
            if (IsDBError == true)
            {
                IsSuccess = false;
                return Json(new { IsSuccess = IsSuccess, ErrorMessage = ErrorMessage1, Response = IsSuccess }, JsonRequestBehavior.AllowGet);
            }
            else if (IsSuccess)
            {
                return Json(new { result1, IsSuccess = IsSuccess, Response = IsSuccess }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { IsSuccess = IsSuccess, ErrorMessage = CustomMessages.GenericErrorMessage, Response = IsSuccess }, JsonRequestBehavior.AllowGet);
            }
        }


       public bool RunNullRecovery(tbl_transactions transaction)
       {
            try
            {
                
                bool IsRecoveryUpdated = false;
                //retrieving the list of pending recovery
                List<tbl_Card_Charges_Recovery> lstRecovery = new List<tbl_Card_Charges_Recovery>();
                RecoveryDataAccess rda = new RecoveryDataAccess();
                TransactionDataAccess tda = new TransactionDataAccess();
                bool UpdateCardRecovery = false;
                bool UpdateLastTransactionDate = false;
                bool AddLowBalanceRecovery = false;
                long AddRecoveryTransactions = 0;
                bool UpdateRecoveryTransactions = false;
                bool UpdateFEDTransactions = false;
                bool UpdateReversalTransactions = false;
                long AddFEDTransactions = 0;
                long AddReversalTransactions = 0;
                bool IsDBError = false;
                var IsSuccess = false;
                var RequestId = transaction.STAN.Remove(transaction.STAN.Length - 1, 1);
                RequestId = RequestId.Substring(RequestId.Length - 4);
                int RequestIdDec = (Convert.ToInt32(RequestId, 16));

                if (transaction.STAN.EndsWith("I") == true)
                {

                    CISSB.TransactionInfoResponse TransResponse = T24Methods.RecoveryDebitTransaction(transaction.DebitAccountNo, Convert.ToDecimal(transaction.Amount), transaction.Narration, "RC", transaction.BranchCode, transaction.STAN, transaction.TransactionRefNo);
                    if (TransResponse.ResponseCode != null)
                    {


                        FileHelper.RecoveryErrorLog("Income Transaction Response for Null Response", "Response Code:" + TransResponse.ResponseCode + "Response Desc:" + TransResponse.ResponseCodeDescription + "FT: " + TransResponse.TransactionIdentificationNo, "Response Code:" + TransResponse.ResponseCode, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());

                        UpdateRecoveryTransactions = UpdateRecoveryTransaction(TransResponse, transaction.ID);

                        double fedShouldDeduct = 0;
                        var fedToDeduct = double.Parse(new RegionDataAccess().GetRegionsbyID(transaction.BranchCode, true).OrderByDescending(e => e.ID).FirstOrDefault()?.FED ?? "0");
                        fedShouldDeduct = (fedToDeduct > 0) ? Convert.ToDouble(transaction.Amount) * (fedToDeduct / 100) : 0;
                        fedShouldDeduct = Math.Round(fedShouldDeduct, 2);
                        if (TransResponse.ResponseCode == "0000" || TransResponse.ResponseCodeDescription.Contains("STAN"))
                        {
                            if (fedShouldDeduct > 0)
                            {

                                if (transaction.Amount >= 0.10d)
                                {

                                    var STAN1 = transaction.STAN.Remove(transaction.STAN.Length - 1, 1) + "F";

                                    //STAN1 = Core.Methods.CommonMethods.GetRecoverySTAN(lstRecovery[i].ID.ToString(), "F");
                                    //FileHelper.RecoveryErrorLog("FED Transaction Generation", "RecoveryID:" + lstRecovery[i].ID + "STAN :" + STAN1 + "Ac#:" + lstRecovery[i].AccountNo + "Narrartion:" + $"FED on Card Chgs" + "Amount:" + fedShouldDeduct, "Pending", new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                                    AddFEDTransactions = AddFEDTransaction($"FED on Card Chgs", Convert.ToDecimal(fedShouldDeduct), transaction.DebitAccountNo, "RC", STAN1, transaction.TransactionRefNo);

                                    CISSB.TransactionInfoResponse FedTransResponse = T24Methods.RecoveryFEDDebitTransaction(transaction.DebitAccountNo, Convert.ToDecimal(fedShouldDeduct)
            , $"FED on Card Chgs", "RC", transaction.BranchCode, STAN1, transaction.DebitAccountNo);
                                    FileHelper.RecoveryErrorLog("FED Transaction Response ", "Response Code:" + FedTransResponse.ResponseCode + "Response Desc:" + FedTransResponse.ResponseCodeDescription + "FT: " + FedTransResponse.TransactionIdentificationNo, "Response Code:" + FedTransResponse.ResponseCode, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());

                                    UpdateFEDTransactions = UpdateRecoveryTransaction(FedTransResponse, AddFEDTransactions);

                                    if (FedTransResponse.ResponseCode == "0000")
                                    {

                                        IsRecoveryUpdated = UpdateCardRecoveryAmountForNull(RequestIdDec, ref rda, Convert.ToDouble(transaction.Amount));
                                        if (!IsRecoveryUpdated)
                                        {
                                            IsDBError = true;

                                        }
                                    }
                                    else
                                    {
                                        UpdateLastTransactionDate = rda.UpdateLastTransactionDate(RequestIdDec);
                                        if (!UpdateLastTransactionDate)
                                        {
                                            IsDBError = true;

                                        }
                                        FileHelper.RecoveryErrorLog("Recovery Last Deduction Date Update DB for Null Response Case", "RecoveryID:" + RequestIdDec, "Status:" + UpdateLastTransactionDate, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                                        var RevSTAN = Core.Methods.CommonMethods.GetRecoverySTAN(RequestIdDec.ToString(), "X");
                                        AddReversalTransactions = AddReverseTransaction(transaction.Narration, Convert.ToDecimal(transaction.Amount), transaction.DebitAccountNo, "RC", transaction.TransactionRefNo, RevSTAN);

                                        var revTrans = T24Methods.ReverseTransactionLatest(transaction.DebitAccountNo, Convert.ToDecimal(transaction.Amount)
    , transaction.Narration, "RC", transaction.BranchCode, transaction.TransactionRefNo, RevSTAN);

                                        UpdateReversalTransactions = UpdateRecoveryTransaction(revTrans, AddReversalTransactions);

                                    }
                                    //}
                                }
                                else
                                {
                                    IsRecoveryUpdated = UpdateCardRecoveryAmountForNull(RequestIdDec, ref rda, Convert.ToDouble(transaction.Amount));
                                    if (!IsRecoveryUpdated)
                                    {
                                        IsDBError = true;

                                    }
                                }
                            }
                            else
                            {
                                IsRecoveryUpdated = UpdateCardRecoveryAmountForNull(RequestIdDec, ref rda, Convert.ToDouble(transaction.Amount));
                                if (!IsRecoveryUpdated)
                                {
                                    IsDBError = true;

                                }
                            }
                        }
                        else
                        {

                            UpdateLastTransactionDate = rda.UpdateLastTransactionDate(RequestIdDec);
                            if (!UpdateLastTransactionDate)
                            {
                                IsDBError = true;

                            }
                            FileHelper.RecoveryErrorLog("Recovery Last Deduction Date Update DB", "RecoveryID:" + RequestIdDec, "Status:" + UpdateLastTransactionDate, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());

                        }

                        //return Json(new { result = true, JsonRequestBehavior.AllowGet });
                        IsSuccess = true;
                    }
                    else
                    {
                        //return Json(new { result = false, JsonRequestBehavior.AllowGet });
                        IsSuccess = false;
                    }
                }
                else if (transaction.STAN.EndsWith("F"))
                {
                    CISSB.TransactionInfoResponse FedTransResponse = T24Methods.RecoveryFEDDebitTransaction(transaction.DebitAccountNo, Convert.ToDecimal(transaction.Amount)
        , $"FED on Card Chgs", "RC", transaction.BranchCode, transaction.STAN, transaction.DebitAccountNo);
                    if (FedTransResponse != null)
                    {
                        FileHelper.RecoveryErrorLog("FED Transaction Response ", "Response Code:" + FedTransResponse.ResponseCode + "Response Desc:" + FedTransResponse.ResponseCodeDescription + "FT: " + FedTransResponse.TransactionIdentificationNo, "Response Code:" + FedTransResponse.ResponseCode, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());

                        UpdateFEDTransactions = UpdateRecoveryTransaction(FedTransResponse, AddFEDTransactions);
                        if (FedTransResponse.ResponseCode == "0000" || FedTransResponse.ResponseCodeDescription.Contains("STAN"))
                        {
                            IsRecoveryUpdated = UpdateCardRecoveryAmountForNull(RequestIdDec, ref rda, Convert.ToDouble(transaction.Amount));
                            if (!IsRecoveryUpdated)
                            {
                                IsDBError = true;

                            }
                        } else
                        {
                            var db = new SoneriCISEntities();
                            bool flag = false;
                            tbl_transactions val = new tbl_transactions();
                            val = db.tbl_transactions.Where(a => a.STAN.EndsWith("I") && a.STAN.Remove(a.STAN.Length - 1, 1) == transaction.STAN.Remove(transaction.STAN.Length - 1, 1) && a.ResponseCode == "0000").FirstOrDefault();
                            if (val != null)
                            {
                                var RevSTAN = Core.Methods.CommonMethods.GetRecoverySTAN(RequestIdDec.ToString(), "X");
                                AddReversalTransactions = AddReverseTransaction(transaction.Narration, Convert.ToDecimal(val.Amount), transaction.DebitAccountNo, "RC", transaction.TransactionRefNo, RevSTAN);

                                var revTrans = T24Methods.ReverseTransactionLatest(transaction.DebitAccountNo, Convert.ToDecimal(transaction.Amount)
        , transaction.Narration, "RC", transaction.BranchCode, transaction.TransactionRefNo, RevSTAN);

                                UpdateReversalTransactions = UpdateRecoveryTransaction(revTrans, AddReversalTransactions);
                            }
                        }
                        // return Json(new { result = true, JsonRequestBehavior.AllowGet });
                        IsSuccess = true;
                    }
                    else
                    {
                        // return Json(new { result = false, JsonRequestBehavior.AllowGet });
                        IsSuccess = false;
                    }
                }
                else if (transaction.STAN.Contains("X"))
                {

                    //AddReversalTransactions = AddReverseTransaction(transaction.Narration, Convert.ToDecimal(transaction.Amount), transaction.DebitAccountNo, "RC", transaction.TransactionRefNo, transaction.STAN);

                    var revTrans = T24Methods.ReverseTransactionLatest(transaction.DebitAccountNo, Convert.ToDecimal(transaction.Amount)
, transaction.Narration, "RC", transaction.BranchCode, transaction.TransactionRefNo, transaction.STAN);
                    if (revTrans != null)
                    { 
                    UpdateReversalTransactions = UpdateRecoveryTransaction(revTrans, AddReversalTransactions);

                    if (revTrans.ResponseCode == "0000" || revTrans.ResponseCodeDescription.Contains("STAN"))
                    {
                        UpdateReversalTransactions = UpdateRecoveryTransaction(revTrans, AddReversalTransactions);
                    }
                        //return Json(new { result = true, JsonRequestBehavior.AllowGet });
                        IsSuccess = true;
                    }
                    else
                    {
                        //return Json(new { result = false, JsonRequestBehavior.AllowGet });
                        IsSuccess = false;
                    }
                }

                //return Json(new { result = IsSuccess , JsonRequestBehavior.AllowGet });
                return IsSuccess;
            }
            
            catch (Exception ex)
            {
                return false;
            }
       }

        private bool UpdateCardRecoveryAmount(tbl_Card_Charges_Recovery recovery, ref RecoveryDataAccess rda, double amountShouldDeduct)
        {
            try
            {
                bool flag = false;

                var finalAmountDeducted = (recovery.DeducatedAmount ?? 0) + amountShouldDeduct;
                finalAmountDeducted = Math.Round(finalAmountDeducted, 2);
                flag = rda.UpdateCardRecoveryAmount(recovery.ID, Convert.ToSingle(finalAmountDeducted));
                FileHelper.RecoveryErrorLog("Recovery Amount Update DB", "RecoveryID:" + recovery.ID + "Amount:" + finalAmountDeducted, "Status:" + flag, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                return flag;
            }
            catch (Exception ex)
            {
                FileHelper.RecoveryExceptionLog("Update Recovery", recovery.ID.ToString(), "", recovery.AccountNo.ToString(), recovery.CardNo.ToString(), ex.ToString());
                return false;
            }

        }

        private bool UpdateCardRecoveryAmountForNull(int recoveryid, ref RecoveryDataAccess rda, double amountShouldDeduct)
        {
            var db = new SoneriCISEntities();
            bool flag = false;
            tbl_Card_Charges_Recovery val = new DAL.tbl_Card_Charges_Recovery();
            try
            {
               
                val = db.tbl_Card_Charges_Recovery.Where(a => a.ID == recoveryid).FirstOrDefault();
                var finalAmountDeducted = (val.DeducatedAmount ?? 0) + amountShouldDeduct;
                finalAmountDeducted = Math.Round(finalAmountDeducted, 2);
                flag = rda.UpdateCardRecoveryAmount(recoveryid, Convert.ToSingle(finalAmountDeducted));
                FileHelper.RecoveryErrorLog("Recovery Amount Update DB For Null Responses", "RecoveryID:" + recoveryid + "Amount:" + finalAmountDeducted, "Status:" + flag, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                return flag;
            }
            catch (Exception ex)
            {
                FileHelper.RecoveryExceptionLog("Update Recovery for Null Responses", recoveryid.ToString(), "", val.AccountNo.ToString(), val.CardNo.ToString(), ex.ToString());
                return false;
            }

        }
        public ActionResult GetPendingRecoveryCount(bool IsFullRecovery, string FromBranchCode, string ToBranchCode)
        {
            int Count;
            RecoveryDataAccess rda = new RecoveryDataAccess();
            Count = rda.GetPendingRecoveryCount(IsFullRecovery, FromBranchCode, ToBranchCode);
            return Json(new { result = Count }, JsonRequestBehavior.AllowGet);
        }





        #region New Records
        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult NewRecords()
        {
            RecoveryDataAccess cda = new RecoveryDataAccess();
            ViewBag.LastDate = cda.GetLastApplicableDate();

            return View();
        }


        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult GetNewRecoveryCount(DateTime StartDate, DateTime EndDate)
        {
            string Count;
            RecoveryDataAccess rda = new RecoveryDataAccess();
            Count = rda.GetNewRecoveryCount(StartDate, EndDate);
            return Json(new { result = Count }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "a" })]
        public ActionResult InsertNewRecovery(DateTime StartDate, DateTime EndDate)
        {
            bool Count;
            RecoveryDataAccess rda = new RecoveryDataAccess();
            Count = rda.InsertNewRecovery(StartDate, EndDate);
            return Json(new { result = Count }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        public static long AddTransaction(string Narration, decimal Amount, string DAccount, string RequestType, string STAN1, string CardNo)
        {
            try
            {
                DAL.DataAccessClasses.IncomeAccountsDataAccess da = new IncomeAccountsDataAccess();

                bool IsAdded = false;
                long ID = 0;
                using (var db = new SoneriCISEntities())
                {
                    tbl_transactions t = new tbl_transactions();
                    t.Narration = Narration;
                    t.Amount = double.Parse(Amount.ToString());
                    t.DebitAccountNo = DAccount;
                    t.CreditAccountNo = da.GetIncomeAccountByCardType(CardNo);
                    t.RequestType = RequestType;
                    t.STAN = STAN1.Substring(3, STAN1.Length - 3);//STAN1;
                    //t.ResponseCode = response.ResponseCode;
                    //t.ResponseCodeDescription = response.ResponseCodeDescription;
                    //t.BranchCode = response.TransactionBranchCode;
                    t.TransactionDateTime = DateTime.Now;
                    //t.TransactionIdNo = response.TransactionIdentificationNo;
                    t.TransactionRefNo = CardNo;
                    db.tbl_transactions.Add(t);
                    db.SaveChanges();
                    ID = t.ID;
                    IsAdded = true;
                    FileHelper.RecoveryErrorLog("Income Transaction DB Insert", "Debit Account : " + DAccount + " STAN: " + STAN1 + " Credit Account : " + da.GetIncomeAccountByCardType(CardNo) + " CardNo :" + CardNo , "Status:" + IsAdded, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                }
                //return IsAdded;
                return ID;
            }
            catch (Exception ex)
            {
                FileHelper.RecoveryExceptionLog("Add Income Transaction", "", STAN1, DAccount, CardNo, ex.ToString());
                return 0;
            }
        }

        public static long AddFEDTransaction(string Narration, decimal Amount, string DAccount, string RequestType, string STAN1, string CardNo)
        {
            try
            {
                DAL.DataAccessClasses.IncomeAccountsDataAccess da = new IncomeAccountsDataAccess();
                bool IsAdded = false;
                long ID = 0;
                using (var db = new SoneriCISEntities())
                {
                    tbl_transactions t = new tbl_transactions();
                    t.Narration = Narration;
                    t.Amount = double.Parse(Amount.ToString());
                    t.DebitAccountNo = DAccount;
                    t.CreditAccountNo = da.GetIncomeAccountByType("FED");
                    t.RequestType = RequestType;
                    t.STAN = STAN1.Substring(3, STAN1.Length - 3);//STAN1;
                    //t.ResponseCode = response.ResponseCode;
                    //t.ResponseCodeDescription = response.ResponseCodeDescription;
                    //t.BranchCode = response.TransactionBranchCode;
                    t.TransactionDateTime = DateTime.Now;
                    //t.TransactionIdNo = response.TransactionIdentificationNo;
                    t.TransactionRefNo = CardNo;
                    db.tbl_transactions.Add(t);
                    db.SaveChanges();
                    ID = t.ID;
                    IsAdded = true;
                    FileHelper.RecoveryErrorLog("FED Transaction DB Insert ", "Debit Account : " + DAccount + " STAN: " + STAN1 + " Credit Account : " + da.GetIncomeAccountByCardType(CardNo) + " CardNo :" + CardNo, "Status:" + IsAdded, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                }
                return ID;
            }
            catch (Exception ex)
            {
                FileHelper.RecoveryExceptionLog("Add FED Transaction", "", STAN1, DAccount, CardNo, ex.ToString());
                return 0;
            }
        }



        public static long AddReverseTransaction(string Narration, decimal Amount, string CAccount, string RequestType, string CardNo,string STAN1)
        {
            try
            {
                DAL.DataAccessClasses.IncomeAccountsDataAccess da = new IncomeAccountsDataAccess();

                bool IsAdded = false;
                long ID = 0;
                using (var db = new SoneriCISEntities())
                {
                    tbl_transactions t = new tbl_transactions();
                    t.Narration = Narration;
                    t.Amount = double.Parse(Amount.ToString());
                    t.DebitAccountNo = da.GetIncomeAccountByCardType(CardNo);
                    t.CreditAccountNo = CAccount;
                    t.RequestType = RequestType;
                    t.STAN = STAN1.Substring(3, STAN1.Length - 3);//STAN1;
                    //t.ResponseCode = response.ResponseCode;
                    //t.ResponseCodeDescription = response.ResponseCodeDescription;
                    //t.BranchCode = response.TransactionBranchCode;
                    t.TransactionDateTime = DateTime.Now;
                    //t.TransactionIdNo = response.TransactionIdentificationNo;
                    t.TransactionRefNo = CardNo;
                    db.tbl_transactions.Add(t);
                    db.SaveChanges();
                    IsAdded = true;
                    ID = t.ID;
                    FileHelper.RecoveryErrorLog("Reversal  Transaction DB Insert", "Debit Account : " + da.GetIncomeAccountByCardType(CardNo) + " Credit Account : " + CAccount + " CardNo :" + CardNo, "Status:" + IsAdded, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                }
                return ID;
            }
            catch (Exception ex)
            {
                FileHelper.RecoveryExceptionLog("Add Reversal Transaction", "",Amount.ToString() , CAccount, CardNo, ex.ToString());
                return 0;
            }
        }


        public static bool UpdateRecoveryTransaction(CISSB.TransactionInfoResponse response,long Id)
        {
            try
            {
                //STAN1 = STAN1.Substring(3, STAN1.Length - 3);
                bool IsAdded = true;

                using (var db = new SoneriCISEntities())
                {
                    tbl_transactions t = new tbl_transactions();
                    t = db.tbl_transactions.Where(x => x.ID==Id).FirstOrDefault();
                    //t.Narration = request.TransactionNarration;
                    //t.Amount = double.Parse(request.TransactionAmount.ToString());
                    //t.DebitAccountNo = request.DrCustomerAccountNumber;
                    //t.CreditAccountNo = request.CrCustomerAccountNumber;
                    //t.RequestType = RequestType;
                    //t.STAN = STAN1;
                    t.ResponseCode = response.ResponseCode;
                    t.ResponseCodeDescription = response.ResponseCodeDescription;
                    t.BranchCode = response.TransactionBranchCode;
                    t.TransactionDateTime = DateTime.Now;
                    t.TransactionIdNo = response.TransactionIdentificationNo;
                    //t.TransactionRefNo = response.TransactionRefNo;
                    //db.tbl_transactions.Add(t);
                    db.SaveChanges();
                }
                return IsAdded;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool UpdateReversalTransaction(CISSB.TransactionInfoResponse response, long Id,string STAN)
        {
            try
            {
                //STAN1 = STAN1.Substring(3, STAN1.Length - 3);
                bool IsAdded = true;

                using (var db = new SoneriCISEntities())
                {
                    tbl_transactions t = new tbl_transactions();
                    t = db.tbl_transactions.Where(x => x.ID == Id).FirstOrDefault();
                    //t.Narration = request.TransactionNarration;
                    //t.Amount = double.Parse(request.TransactionAmount.ToString());
                    //t.DebitAccountNo = request.DrCustomerAccountNumber;
                    //t.CreditAccountNo = request.CrCustomerAccountNumber;
                    //t.RequestType = RequestType;
                    t.STAN = STAN;
                    t.ResponseCode = response.ResponseCode;
                    t.ResponseCodeDescription = response.ResponseCodeDescription;
                    t.BranchCode = response.TransactionBranchCode;
                    t.TransactionDateTime = DateTime.Now;
                    t.TransactionIdNo = response.TransactionIdentificationNo;
                    //t.TransactionRefNo = response.TransactionRefNo;
                    //db.tbl_transactions.Add(t);
                    db.SaveChanges();
                }
                return IsAdded;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string CheckSTAN(string STAN)
        {
            try
            {
                tbl_transactions t = new DAL.tbl_transactions();
                using (var db = new SoneriCISEntities())
                {
                    t = db.tbl_transactions.Where(a => a.STAN == STAN && (a.ResponseCode==null|| a.ResponseCode==" ")).FirstOrDefault();
                }
                if (t == null)
                {
                    return "false";
                }
                else
                {
                    return t.STAN.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Recovery Test
        public ActionResult RecoveryTest()
        {
            RegionDataAccess rda = new RegionDataAccess();
            List<tbl_Region> lst = new List<tbl_Region>();
            tbl_Region lst2 = new tbl_Region();
            lst2.ID = 0;
            lst2.Title = "Please Select";
            lst2.Description = "";
            lst2.FED = "0";
            lst2.IsActive = true;


            lst = rda.GetAllRegions();
            lst.Insert(0, lst2);
            return View(lst);
        }


        //    [AuthOp(RoleTitle = new string[] { "a" })]
        //    public ActionResult RunRecovery22(bool IsFullRecovery, string FromBranchCode, string ToBranchCode)
        //    {
        //        bool IsRecoveryUpdated = false;
        //        //retrieving the list of pending recovery
        //        List<tbl_Card_Charges_Recovery> lstRecovery = new List<tbl_Card_Charges_Recovery>();
        //        RecoveryDataAccess rda = new RecoveryDataAccess();
        //        TransactionDataAccess tda = new TransactionDataAccess();
        //        bool UpdateCardRecovery = false;
        //        bool UpdateLastTransactionDate = false;
        //        bool AddLowBalanceRecovery = false;
        //        long AddRecoveryTransactions = 0;
        //        bool UpdateRecoveryTransactions = false;
        //        bool UpdateFEDTransactions = false;
        //        bool UpdateReversalTransactions = false;
        //        long AddFEDTransactions = 0;
        //        long AddReversalTransactions = 0;
        //        bool IsDBError = false;
        //        string STAN = "";
        //        string STAN1 = "";
        //        string RecoveryYear = DateTime.Now.ToString("yy");
        //        lstRecovery = rda.GetPendingRecovery(IsFullRecovery, FromBranchCode, ToBranchCode);
        //        string Data = " Full Recovery: " + IsFullRecovery + " FromBranch: " + FromBranchCode + " ToBranch " + ToBranchCode;
        //        FileHelper.RecoveryErrorLog("Recovery Records Fetch", Data, lstRecovery.Count + " Records Found", new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
        //        int PartialRecovery = 0;
        //        int FullRecovery = 0;
        //        bool UpdateStop;
        //        //running Recovery on each pending recovery
        //        for (int i = 0; i < lstRecovery.Count; i++)
        //        {

        //            var ShouldStop = ReadFile();
        //            if (ShouldStop=="true")
        //            {
        //                UpdateStop=UpdateIsStop("false");
        //                break;

        //            }
        //            double fedShouldDeduct = 0;
        //            double amountShouldDeduct = 0;
        //            bool isFullRecovery = false;

        //            if (string.IsNullOrEmpty(lstRecovery[i].CIF))
        //                continue;
        //            if (IsFullRecovery == true)
        //            {
        //                var isHot = new CustomerCardDataAccess().IsCardHotMarked(lstRecovery[i].CardNo);
        //                if (isHot.ToLower() == "card already hot marked")
        //                {
        //                    var logTime = new UserLogDataAccess().GetUserLogByEntityId(lstRecovery[i].CardID.GetValueOrDefault()).OrderByDescending(e => e.ID).FirstOrDefault()?.ActionTimestamp;
        //                    if (logTime != null)
        //                    {
        //                        if (logTime.GetValueOrDefault().Date <= lstRecovery[i].ApplicableDate.GetValueOrDefault().Date)
        //                        {
        //                            lstRecovery[i].IsValid = false;
        //                            lstRecovery[i].DeducatedAmount = Math.Round(lstRecovery[i].DeducatedAmount ?? Convert.ToDouble(lstRecovery[i].DeducatedAmount), 2);

        //                            UpdateCardRecovery = rda.UpdateCardRecovery(lstRecovery[i]);
        //                            FileHelper.RecoveryErrorLog("Recovery Amount Update DB", "RecoveryID:" + lstRecovery[i].ID + ",Amount:-", ",Status:" + UpdateCardRecovery, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
        //                            continue;
        //                        }
        //                    }
        //                }
        //            }
        //            //Fetch details from T24 against Account No
        //            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType response = T24Methods.FetchAccount(lstRecovery[i].AccountNo, lstRecovery[i].CIF);

        //            FileHelper.RecoveryErrorLog("T24 Account Fetch", "Account#:" + lstRecovery[i].AccountNo + ",CIF:" + lstRecovery[i].CIF, response.ErrorField == "" ? "Success" : "Failure", new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
        //            string BrCode = string.IsNullOrEmpty(response.BranchCode) ? string.Empty : response.BranchCode.Substring(response.BranchCode.Length - 4, 4).ToString();
        //            tbl_Card_Charges_Recovery recovery = lstRecovery[i];
        //            //have to check for posting restrictions, response pending from business
        //            if (string.IsNullOrEmpty(response.ErrorField))
        //            {
        //                // FED Percentage to deduct (FED %)
        //                var fedToDeduct = double.Parse(new RegionDataAccess().GetRegionsbyID(BrCode, true).OrderByDescending(e => e.ID).FirstOrDefault()?.FED ?? "0");
        //                // Current Balance
        //                double Balance = double.Parse((!string.IsNullOrEmpty(response.AvailableBalance)) ? response.AvailableBalance : "0");
        //                double applicableAmount = lstRecovery[i].ApplicableAmount ?? 0;
        //                double DeductedAmount = lstRecovery[i].DeducatedAmount ?? 0;
        //                double remainingAmountToDeduct = applicableAmount - DeductedAmount;
        //                //remainingAmountToDeduct = Math.Truncate(100 * remainingAmountToDeduct) / 100;
        //                remainingAmountToDeduct = Math.Round(remainingAmountToDeduct, 2);
        //                //if account type code doesn't match then set validity false and skip this record
        //                string AccountCode = Convert.ToString(lstRecovery[i].AccountCode) ?? string.Empty;
        //                //test during recovery test run
        //                AccountCode = response.AccountNature;
        //                //Check Account Status //if In-active then skip this record
        //                if (Balance > 1.2 && remainingAmountToDeduct > 0)
        //                {
        //                    if (response.AccountNature == AccountCode)
        //                    {
        //                        /* amount that need to deduct */
        //                        var finalDeductAmount = remainingAmountToDeduct + (remainingAmountToDeduct * (fedToDeduct / 100));
        //                        if (finalDeductAmount > Balance)
        //                        {

        //                            /* new fed need to deduct based on the user balance */
        //                            fedShouldDeduct = (fedToDeduct > 0) ? (Balance * fedToDeduct) / (Math.Round(fedToDeduct) + 100)/*Balance * (fedToDeduct / 100)*/ : 0;
        //                            /* new amount that need to deduct based on the user balance */
        //                            amountShouldDeduct = (Balance * 100) / (Math.Round(fedToDeduct) + 100);//Balance - fedShouldDeduct;

        //                            /*Round to two precision a/c to requirement*/
        //                            //fedShouldDeduct = Math.Truncate(100 * fedShouldDeduct) / 100;//Math.Floor(fedShouldDeduct);
        //                            fedShouldDeduct = Math.Round(fedShouldDeduct, 2);
        //                            /*amountShouldDeduct = Math.Truncate(100 * amountShouldDeduct) / 100;*///Math.Floor(amountShouldDeduct);
        //                            amountShouldDeduct = Math.Round(amountShouldDeduct, 2);
        //                            PartialRecovery++;
        //                            isFullRecovery = false;
        //                        }
        //                        else
        //                        {
        //                            /* new fed need to deduct based on the amount that need to deduct */
        //                            fedShouldDeduct = (fedToDeduct > 0) ? remainingAmountToDeduct * (fedToDeduct / 100) : 0;
        //                            /* new amount need to deduct based on the amount that need to deduct */
        //                            amountShouldDeduct = finalDeductAmount - fedShouldDeduct;

        //                            /*Round to two precision a/c to requirement*/
        //                            /*fedShouldDeduct = Math.Truncate(100 * fedShouldDeduct) / 100;*///Math.Floor(fedShouldDeduct);
        //                            fedShouldDeduct = Math.Round(fedShouldDeduct, 2);
        //                            /*amountShouldDeduct = Math.Truncate(100 * amountShouldDeduct) / 100;*///Math.Floor(amountShouldDeduct);
        //                            amountShouldDeduct = Math.Round(amountShouldDeduct, 2);
        //                            FullRecovery++;
        //                            isFullRecovery = true;
        //                        }

        //                        if (amountShouldDeduct > 0)
        //                        {
        //                            /*if income is less than 0.05 then income and FED both wont be charged*/
        //                            if (amountShouldDeduct < 0.05d)
        //                                continue;

        //                            STAN = Core.Methods.CommonMethods.GetRecoverySTAN(lstRecovery[i].ID.ToString(), "I");
        //                            FileHelper.RecoveryErrorLog("Income Transaction Generation", "RecoveryID:" + lstRecovery[i].ID + ",STAN :" + STAN + ",Ac#:" + lstRecovery[i].AccountNo + ",Narrartion:" + lstRecovery[i].ChargesDescription + ",Amount:" + amountShouldDeduct, "Pending", new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
        //                            AddRecoveryTransactions = AddTransaction(lstRecovery[i].ChargesDescription, Convert.ToDecimal(amountShouldDeduct), lstRecovery[i].AccountNo, "RC", STAN, lstRecovery[i].CardNo);
        //                            if (AddRecoveryTransactions <= 0)
        //                            {
        //                                IsDBError = true;
        //                                break;
        //                            }
        //                            CISSB.TransactionInfoResponse TransResponse = T24Methods.RecoveryDebitTransaction(lstRecovery[i].AccountNo, Convert.ToDecimal(amountShouldDeduct)
        //, lstRecovery[i].ChargesDescription, "RC", BrCode, STAN, lstRecovery[i].CardNo);

        //                            //update recovery amount in database
        //                            FileHelper.RecoveryErrorLog("Income Transaction Response", "Response Code:" + TransResponse.ResponseCode + ",Response Desc:" + TransResponse.ResponseCodeDescription + ",FT: " + TransResponse.TransactionIdentificationNo, ",Response Code:" + TransResponse.ResponseCode, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());

        //                            UpdateRecoveryTransactions = UpdateRecoveryTransaction(TransResponse, AddRecoveryTransactions);

        //                            if (TransResponse.ResponseCode == "0000")
        //                            {
        //                                if (fedShouldDeduct > 0)
        //                                {
        //                                    /*if Income is less than 0.10 then only income will be charged and FED wont*/
        //                                    if (amountShouldDeduct >= 0.10d)
        //                                    {


        //                                        STAN1 = Core.Methods.CommonMethods.GetRecoverySTAN(lstRecovery[i].ID.ToString(), "F");
        //                                        FileHelper.RecoveryErrorLog("FED Transaction Generation", "RecoveryID:" + lstRecovery[i].ID + ",STAN :" + STAN1 + ",Ac#:" + lstRecovery[i].AccountNo + ",Narrartion:" + $",FED on Card Chgs" + ",Amount:" + fedShouldDeduct, "Pending", new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
        //                                        AddFEDTransactions = AddFEDTransaction($"FED on Card Chgs", Convert.ToDecimal(fedShouldDeduct), lstRecovery[i].AccountNo, "RC", STAN1, lstRecovery[i].CardNo);

        //                                        CISSB.TransactionInfoResponse FedTransResponse = T24Methods.RecoveryFEDDebitTransaction(lstRecovery[i].AccountNo, Convert.ToDecimal(fedShouldDeduct)
        //                , $"FED on Card Chgs", "RC", BrCode, STAN1, lstRecovery[i].CardNo);
        //                                        FileHelper.RecoveryErrorLog("FED Transaction Response ", "Response Code:" + FedTransResponse.ResponseCode + ",Response Desc:" + FedTransResponse.ResponseCodeDescription + ",FT: " + FedTransResponse.TransactionIdentificationNo, ",Response Code:" + FedTransResponse.ResponseCode, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());

        //                                        UpdateFEDTransactions = UpdateRecoveryTransaction(FedTransResponse, AddFEDTransactions);

        //                                        if (FedTransResponse.ResponseCode == "0000")
        //                                        {
        //                                            IsRecoveryUpdated = UpdateCardRecoveryAmount(lstRecovery[i], ref rda, amountShouldDeduct);
        //                                            if (!IsRecoveryUpdated)
        //                                            {
        //                                                IsDBError = true;
        //                                                break;
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            UpdateLastTransactionDate = rda.UpdateLastTransactionDate(lstRecovery[i].ID);
        //                                            if (!UpdateLastTransactionDate)
        //                                            {
        //                                                IsDBError = true;
        //                                                break;
        //                                            }
        //                                            FileHelper.RecoveryErrorLog("Recovery Last Deduction Date Update DB", "RecoveryID:" + lstRecovery[i].ID, "Status:" + UpdateLastTransactionDate, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
        //                                            var RevSTAN = Core.Methods.CommonMethods.GetRecoverySTAN(lstRecovery[i].ID.ToString(), "X");
        //                                            AddReversalTransactions = AddReverseTransaction(lstRecovery[i].ChargesDescription, Convert.ToDecimal(amountShouldDeduct), lstRecovery[i].AccountNo, "RC", lstRecovery[i].CardNo, RevSTAN);

        //                                            var revTrans = T24Methods.ReverseTransactionLatest(lstRecovery[i].AccountNo, Convert.ToDecimal(amountShouldDeduct)
        //        , lstRecovery[i].ChargesDescription, "RC", BrCode, lstRecovery[i].CardNo, RevSTAN);

        //                                            UpdateReversalTransactions = UpdateRecoveryTransaction(revTrans, AddReversalTransactions);
        //                                            if (isFullRecovery)
        //                                                FullRecovery--;
        //                                            else
        //                                                PartialRecovery--;
        //                                        }
        //                                        //}
        //                                    }
        //                                    else
        //                                    {
        //                                        IsRecoveryUpdated = UpdateCardRecoveryAmount(lstRecovery[i], ref rda, amountShouldDeduct);
        //                                        if (!IsRecoveryUpdated)
        //                                        {
        //                                            IsDBError = true;
        //                                            break;
        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    IsRecoveryUpdated = UpdateCardRecoveryAmount(lstRecovery[i], ref rda, amountShouldDeduct);
        //                                    if (!IsRecoveryUpdated)
        //                                    {
        //                                        IsDBError = true;
        //                                        break;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                UpdateLastTransactionDate = rda.UpdateLastTransactionDate(lstRecovery[i].ID);
        //                                if (!UpdateLastTransactionDate)
        //                                {
        //                                    IsDBError = true;
        //                                    break;
        //                                }
        //                                FileHelper.RecoveryErrorLog("Recovery Last Deduction Date Update DB", "RecoveryID:" + lstRecovery[i].ID, "Status:" + UpdateLastTransactionDate, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
        //                                //                                T24Methods.ReverseTransaction(lstRecovery[i].AccountNo, Convert.ToDecimal(amountShouldDeduct)
        //                                //, lstRecovery[i].ChargesDescription, "RC", BrCode);
        //                                if (isFullRecovery)
        //                                    FullRecovery--;
        //                                else
        //                                    PartialRecovery--;
        //                            }
        //                            //}
        //                        }
        //                    }
        //                    else
        //                    {
        //                        recovery.IsValid = false;

        //                        recovery.DeducatedAmount = Math.Round(recovery.DeducatedAmount ?? Convert.ToDouble(recovery.DeducatedAmount), 2);
        //                        UpdateCardRecovery = rda.UpdateCardRecovery(recovery);
        //                        if (!IsRecoveryUpdated)
        //                        {
        //                            IsDBError = true;
        //                            break;//change 14 july18. Recommended by Amir Sultan sb to stop mis-entry in CIS DB against T24 Transaction
        //                        }
        //                        FileHelper.RecoveryErrorLog("Recovery Amount Update DB", "RecoveryID:" + lstRecovery[i].ID + ",Amount:" + amountShouldDeduct, "Status:" + UpdateCardRecovery, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
        //                    }
        //                }
        //                else
        //                {
        //                    UpdateLastTransactionDate = rda.UpdateLastTransactionDate(lstRecovery[i].ID);
        //                    if (!UpdateLastTransactionDate)
        //                    {
        //                        IsDBError = true;
        //                        break;
        //                    }
        //                    FileHelper.RecoveryErrorLog("Recovery Last Deduction Date Update DB", "RecoveryID:" + lstRecovery[i].ID, "Status:" + UpdateLastTransactionDate, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
        //                    AddLowBalanceRecovery = rda.AddLowBalanceRecovery(lstRecovery[i].ID);
        //                    if (!AddLowBalanceRecovery)
        //                    {
        //                        IsDBError = true;
        //                        break;
        //                    }
        //                    FileHelper.RecoveryErrorLog("Recovery Low Balance Update DB", "RecoveryID:" + lstRecovery[i].ID, "Status:" + AddLowBalanceRecovery, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
        //                }

        //            }
        //        }
        //        //setting the recovery numbers label
        //        string Total = lstRecovery.Count.ToString();
        //        string Full = FullRecovery.ToString();
        //        string Partial = PartialRecovery.ToString();
        //        var result1 = new { Total = Total, Full = Full, Partial = Partial };
        //        var IsSuccess = true;
        //        string ErrorMessage1 = "Recovery Process Halted because of Database Connection Error!";
        //        if (IsDBError == true)
        //        {
        //            IsSuccess = false;
        //            return Json(new { IsSuccess = IsSuccess, ErrorMessage = ErrorMessage1, Response = IsSuccess }, JsonRequestBehavior.AllowGet);
        //        }
        //        else if (IsSuccess)
        //        {
        //            return Json(new { result1, IsSuccess = IsSuccess, Response = IsSuccess }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { IsSuccess = IsSuccess, ErrorMessage = CustomMessages.GenericErrorMessage, Response = IsSuccess }, JsonRequestBehavior.AllowGet);
        //        }
        //    }


        public string ReadFile()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Logs\IsStopFlag.txt"); // Path.Combine(Environment.CurrentDirectory, @"Logs\IsStopFlag.txt");
                string text = System.IO.File.ReadLines(path, Encoding.UTF8).First().ToString();
                return text;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        public bool UpdateIsStop(string val)
        {
            try
            {
                bool success = false;
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Logs\IsStopFlag.txt");//Path.Combine(Environment.CurrentDirectory, @"Logs\IsStopFlag.txt");
                string text = System.IO.File.ReadLines(path, Encoding.UTF8).First().ToString();
                text = val;
                System.IO.File.WriteAllText(path, text);
                success = true;
                return success;

            }
            catch (Exception ex)
            { throw ex; }
        }
        #endregion

    }


}