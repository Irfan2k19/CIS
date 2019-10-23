using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.Core.Methods;
using CardIssuanceSystem.DAL.DataAccessClasses;
using CardIssuanceSystem.DAL;
using CardIssuanceSystem.Core.Helpers;
using System.Configuration;

namespace CardIssuanceSystem.Controllers
{
    public class T24Methods
    {
        public static CISSB.BEAPIAuthHeader GetHeader()
        {
            //UAT ref url:http://10.35.1.157:7800/SBL/SBLPaymentServices?wsdl 
            //dress reh ref url: http://10.20.206.22:7800/SBL/SBLPaymentServices?wsdl
            //prod url: http://10.20.206.22:7800/SBL/SBLPaymentServices?wsdl
            //UAT 1 url:http://10.35.1.167:7800/SBL/SBLPaymentServices?wsdl

            CISSB.BEAPIAuthHeader header = new CISSB.BEAPIAuthHeader();


            //UAT 1
            header.UserID = ConfigurationManager.AppSettings["UserPayment"];
           // header.Password = EncryptDecrypt.Decrypt(ConfigurationManager.AppSettings["PassPayment"].ToString());
            header.Password = ConfigurationManager.AppSettings["PassPayment"].ToString();


            return header;
        }
        public static CISSB.AccountInfoResponse FetchAccount(string AccountNo)
        {
            try
            {
                CISSB.BEAPITransactionService cis = new CISSB.BEAPITransactionService();
                CISSB.AccountInfoRequest req = new CISSB.AccountInfoRequest();
                CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
                cis.BEAPIAuthHeaderValue = GetHeader();

                req.AccountNumber = AccountNo;

                resp = cis.FetchAccountInfo(req);
                return resp;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public static NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType FetchAccount(string AccountNo, string CIF)
        {
            try
            {
                //UAT url:http://10.35.1.154:9121/SON.CARD.OPS/services?wsdl
                //dress reh url: http://10.20.206.25:9091/SON.CARD.OPS/services
                //prod url: http://10.20.206.25:9091/SON.CARD.OPS/services?wsdl
                //UAT 1 url: http://10.35.1.154:9092/SON.CARD.OPS/services?wsdl
                NewCISSB.webRequestCommon header = new NewCISSB.webRequestCommon();
                header.company = "PK0029001";

                //Credentials
                header.userName = ConfigurationManager.AppSettings["UserFetch"];
                //header.password = EncryptDecrypt.Decrypt(ConfigurationManager.AppSettings["PassFetch"].ToString());
                header.password = ConfigurationManager.AppSettings["PassFetch"].ToString();

                NewCISSB.T24WebServicesImplService service = new NewCISSB.T24WebServicesImplService();
                NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType response = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
                if (!string.IsNullOrEmpty(AccountNo) && !string.IsNullOrEmpty(CIF))
                {


                    NewCISSB.SONFETCHCARDINFOType[] SONFETCHCARDINFOType1 = new NewCISSB.SONFETCHCARDINFOType[2];
                    NewCISSB.enquiryInputCollection[] enqcollection = new NewCISSB.enquiryInputCollection[2];

                    NewCISSB.enquiryInputCollection enq = new NewCISSB.enquiryInputCollection();
                    enq.columnName = "ACCT.ID";
                    enq.criteriaValue = AccountNo.Trim();//"20000024679";
                    enq.operand = "EQ";
                    NewCISSB.enquiryInputCollection enq1 = new NewCISSB.enquiryInputCollection();
                    enq1.columnName = "CUST.ID";
                    enq1.criteriaValue = CIF.Trim();//"1000109118";
                    enq1.operand = "EQ";
                    enqcollection[0] = enq;
                    enqcollection[1] = enq1;
                    var results = service.CARDOPS(header, enqcollection, out SONFETCHCARDINFOType1);
                    response = SONFETCHCARDINFOType1[0].gSONFETCHCARDINFODetailType.mSONFETCHCARDINFODetailType[1];
                }

                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public static NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType FetchAccount1(string AccountNo, string CIF)
        {
            try
            {
                NewCISSB.webRequestCommon header = new NewCISSB.webRequestCommon();
                header.company = "PK0029001";
                header.userName = "CARDOPS";
                header.password = "123123";

                NewCISSB.T24WebServicesImplService service = new NewCISSB.T24WebServicesImplService();
                NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType response = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
                if (!string.IsNullOrEmpty(AccountNo) && !string.IsNullOrEmpty(CIF))
                {


                    NewCISSB.SONFETCHCARDINFOType[] SONFETCHCARDINFOType1 = new NewCISSB.SONFETCHCARDINFOType[2];
                    NewCISSB.enquiryInputCollection[] enqcollection = new NewCISSB.enquiryInputCollection[2];

                    NewCISSB.enquiryInputCollection enq = new NewCISSB.enquiryInputCollection();
                    enq.columnName = "ACCT.ID";
                    enq.criteriaValue = AccountNo.Trim();//"20000024679";
                    enq.operand = "EQ";
                    NewCISSB.enquiryInputCollection enq1 = new NewCISSB.enquiryInputCollection();
                    enq1.columnName = "CUST.ID";
                    enq1.criteriaValue = CIF.Trim();//"1000109118";
                    enq1.operand = "EQ";
                    enqcollection[0] = enq;
                    enqcollection[1] = enq1;
                    var results = service.CARDOPS(header, enqcollection, out SONFETCHCARDINFOType1);
                    response = SONFETCHCARDINFOType1[0].gSONFETCHCARDINFODetailType.mSONFETCHCARDINFODetailType[1];
                }

                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public static string TrimAccountNo(string AccountNumber)
        {
            string str = AccountNumber;
            string result = str.Substring(4, str.Length - 4);
            return result;
        }


        public static CISSB.TransactionInfoResponse DebitTransaction(string DrCustomerAccountNumber, decimal TransactionAmount, string TransactionNarration, string RequestType, string RequestId, int LstCount)
        {
            string STAN1 = Core.Methods.CommonMethods.GetUniqueSTAN(RequestId, LstCount, RequestType);
            CISSB.TransactionInfoRequest request = new CISSB.TransactionInfoRequest();
            CISSB.TransactionInfoResponse response = new CISSB.TransactionInfoResponse();
            CISSB.BEAPITransactionService cis = new CISSB.BEAPITransactionService();
            cis.BEAPIAuthHeaderValue = GetHeader();
            request.TransactionDate = DateTime.Now;
            request.TransactionBranchCode = "0002";
            request.STAN = STAN1;//Transaction No auto increnment with seed value 10001;
            request.TransactionType = "401201"; //Transaction Type tbp by SBL;
            request.DrCustomerAccountNumber = DrCustomerAccountNumber; //"20001266498"; //Debit AccountNumber of Card Holder;
            request.CrCustomerAccountNumber = "20000520676"; //"00022050684877";//"000220000000419";//"20000000389";//"20000404951"; ///Account Number to which the amount will be credited;
            request.InstrumentType = "0022"; //tbp by SBL;
            request.TransactionAmount = TransactionAmount; // TransactionAmount;
            request.InstrumentDate = DateTime.Now; ;
            request.OrginatingTranIdentNo = 401201; //tbp by SBL same as transaction type;
            request.Currency = "PKR";//transaction.Currency;
            request.Authorization = "0";//(0 fro authorized payment) transaction.Authorization;
            request.TransactionNarration = !string.IsNullOrEmpty(TransactionNarration) ? TransactionNarration.Length >= 16 ? TransactionNarration.Substring(0, 16) : TransactionNarration : string.Empty; //Description of charges
            response = cis.DoTransactions(request);
            AddTransaction(response, request, RequestType, STAN1);
            return response;
        }

        public static CISSB.TransactionInfoResponse DebitTransaction(string DrCustomerAccountNumber, decimal TransactionAmount, string TransactionNarration, string RequestType, string BranchCode, string RequestId, int LstCount)
        {
            try
            {
                DAL.DataAccessClasses.IncomeAccountsDataAccess da = new IncomeAccountsDataAccess();

                string CreditAccount = "";
                if (TransactionNarration.Contains("FED"))
                {
                    //CreditAccount = "PKR172530001";
                    CreditAccount = da.GetIncomeAccountByType("FED");
                }
                else if (RequestType == "RC")
                {
                    //CreditAccount = "PL52529";
                    CreditAccount = da.GetIncomeAccountByType("Annual");
                }
                else
                {
                    CreditAccount = da.GetIncomeAccountByType("Setup");
                }

                string STAN1 = Core.Methods.CommonMethods.GetUniqueSTAN(RequestId, LstCount, RequestType);
                //string STAN1 = "CIF30032018";

                FileHelper.RequestTransactionLog(false, DrCustomerAccountNumber, CreditAccount, TransactionAmount.ToString(), TransactionNarration, STAN1, "", "", "", RequestType);


                CISSB.TransactionInfoRequest request = new CISSB.TransactionInfoRequest();
                CISSB.TransactionInfoResponse response = new CISSB.TransactionInfoResponse();
                CISSB.BEAPITransactionService cis = new CISSB.BEAPITransactionService();
                cis.BEAPIAuthHeaderValue = GetHeader();
                request.TransactionDate = DateTime.Now;
                request.DrCustomerBranchCode = BranchCode;
                request.TransactionBranchCode = BranchCode;
                request.CrCustomerBranchCode = BranchCode;
                request.STAN = STAN1;//Transaction No auto increnment with seed value 10001;
                request.TransactionType = "401201"; //Transaction Type tbp by SBL;
                request.DrCustomerAccountNumber = DrCustomerAccountNumber; //"20001266498"; //Debit AccountNumber of Card Holder;
                request.CrCustomerAccountNumber = CreditAccount; //"00022050684877";//"000220000000419";//"20000000389";//"20000404951"; ///Account Number to which the amount will be credited;
                request.InstrumentType = "0022"; //tbp by SBL;
                request.TransactionAmount = TransactionAmount; // TransactionAmount;
                request.InstrumentDate = DateTime.Now; ;
                request.OrginatingTranIdentNo = 401201; //tbp by SBL same as transaction type;
                request.Currency = "PKR";//transaction.Currency;
                request.Authorization = "0";//(0 fro authorized payment) transaction.Authorization;
                request.TransactionNarration = !string.IsNullOrEmpty(TransactionNarration) ? TransactionNarration.Length >= 16 ? TransactionNarration.Substring(0, 16) : TransactionNarration : string.Empty; //Description of charges
                response = cis.DoTransactions(request);
                AddTransaction(response, request, RequestType, STAN1);
                FileHelper.RequestTransactionLog(true, DrCustomerAccountNumber, CreditAccount, TransactionAmount.ToString(), TransactionNarration, STAN1, response.TransactionIdentificationNo, response.ResponseCode, response.ResponseCodeDescription, RequestType);
                return response;
            }
            catch (Exception ex )
            {
                throw ex;
            }
            
        }

        //FOR RECOVERY
        public static CISSB.TransactionInfoResponse DebitTransaction(string DrCustomerAccountNumber, decimal TransactionAmount, string TransactionNarration, string RequestType, string BranchCode)
        {
            DAL.DataAccessClasses.IncomeAccountsDataAccess da = new IncomeAccountsDataAccess();

            string CreditAccount = "";
            if (TransactionNarration.Contains("FED"))
            {
                //CreditAccount = "PKR172530001";
                CreditAccount = da.GetIncomeAccountByType("FED");
            }
            else if (RequestType == "RC")
            {
                //CreditAccount = "PL52529";
                CreditAccount = da.GetIncomeAccountByType("Annual");
            }
            else
            {
                CreditAccount = da.GetIncomeAccountByType("Setup");
            }

            string STAN1 = Core.Methods.CommonMethods.GetUniqueSTAN();
            //string STAN1 = "CIF30032018";
            CISSB.TransactionInfoRequest request = new CISSB.TransactionInfoRequest();
            CISSB.TransactionInfoResponse response = new CISSB.TransactionInfoResponse();
            CISSB.BEAPITransactionService cis = new CISSB.BEAPITransactionService();
            cis.BEAPIAuthHeaderValue = GetHeader();
            request.TransactionDate = DateTime.Now;
            request.DrCustomerBranchCode = BranchCode;
            request.TransactionBranchCode = BranchCode;
            request.CrCustomerBranchCode = BranchCode;
            request.STAN = STAN1;//Transaction No auto increnment with seed value 10001;
            request.TransactionType = "401201"; //Transaction Type tbp by SBL;
            request.DrCustomerAccountNumber = DrCustomerAccountNumber; //"20001266498"; //Debit AccountNumber of Card Holder;
            request.CrCustomerAccountNumber = CreditAccount; //"00022050684877";//"000220000000419";//"20000000389";//"20000404951"; ///Account Number to which the amount will be credited;
            request.InstrumentType = "0022"; //tbp by SBL;
            request.TransactionAmount = TransactionAmount; // TransactionAmount;
            request.InstrumentDate = DateTime.Now; ;
            request.OrginatingTranIdentNo = 401201; //tbp by SBL same as transaction type;
            request.Currency = "PKR";//transaction.Currency;
            request.Authorization = "0";//(0 fro authorized payment) transaction.Authorization;
            request.TransactionNarration = !string.IsNullOrEmpty(TransactionNarration) ? TransactionNarration.Length >= 16 ? TransactionNarration.Substring(0, 16) : TransactionNarration : string.Empty; //Description of charges
            response = cis.DoTransactions(request);
            AddTransaction(response, request, RequestType, STAN1);
            return response;
        }

        #region New Logic For Recovery
        public static string NewTransactionSTAN(string DrCustomerAccountNumber, decimal TransactionAmount, string TransactionNarration, string RequestType, string BranchCode, string CardNo, string RequestId, string TransType)
        {
            //string STAN = CommonMethods.GetUniqueSTAN();
            string STAN = CommonMethods.GetRecoverySTAN(RequestId, TransType);
            AddRecoveryTransaction(DrCustomerAccountNumber, TransactionAmount, TransactionNarration, RequestType, BranchCode, STAN, CardNo);
            return STAN;
        }

        public static bool AddRecoveryTransaction(string DrCustomerAccountNumber, decimal TransactionAmount, string TransactionNarration, string RequestType, string BranchCode, string STAN1, string CardNo)
        {
            try
            {
                STAN1 = STAN1.Substring(3, STAN1.Length - 3);
                bool IsAdded = true;
                using (var db = new SoneriCISEntities())
                {
                    tbl_transactions t = new tbl_transactions();
                    t.Narration = TransactionNarration;
                    t.Amount = double.Parse(TransactionAmount.ToString());
                    t.DebitAccountNo = DrCustomerAccountNumber;
                    //t.CreditAccountNo = request.CrCustomerAccountNumber;
                    t.RequestType = RequestType;
                    t.STAN = STAN1;
                    //t.ResponseCode = response.ResponseCode;
                    //t.ResponseCodeDescription = response.ResponseCodeDescription;
                    //t.BranchCode = response.TransactionBranchCode;
                    t.TransactionDateTime = DateTime.Now;
                    //t.TransactionIdNo = response.TransactionIdentificationNo;
                    t.TransactionRefNo = CardNo;
                    db.tbl_transactions.Add(t);
                    db.SaveChanges();
                }
                return IsAdded;
            }
            catch (Exception ex)
            {
                return false;
            }
        }



        public static CISSB.TransactionInfoResponse RecoveryDebitTransaction(string DrCustomerAccountNumber, decimal TransactionAmount, string TransactionNarration, string RequestType, string BranchCode, string STAN, string CardNo)
        {
            try { 
            DAL.DataAccessClasses.IncomeAccountsDataAccess da = new IncomeAccountsDataAccess();

            string CreditAccount = "";

            if (RequestType == "RC")
            {

                CreditAccount = da.GetIncomeAccountByCardType(CardNo);
            }
            else
            {
                CreditAccount = da.GetIncomeAccountByType("Setup");
            }

            string STAN1 = STAN;
            //string STAN1 = "CIF30032018";
            CISSB.TransactionInfoRequest request = new CISSB.TransactionInfoRequest();
            CISSB.TransactionInfoResponse response = new CISSB.TransactionInfoResponse();
            CISSB.BEAPITransactionService cis = new CISSB.BEAPITransactionService();
            cis.BEAPIAuthHeaderValue = GetHeader();
            request.TransactionDate = DateTime.Now;
            request.DrCustomerBranchCode = BranchCode;
            request.TransactionBranchCode = BranchCode;
            request.CrCustomerBranchCode = BranchCode;
            request.STAN = STAN1;//Transaction No auto increnment with seed value 10001;
            request.TransactionType = "401201"; //Transaction Type tbp by SBL;
            request.DrCustomerAccountNumber = DrCustomerAccountNumber; //"20001266498"; //Debit AccountNumber of Card Holder;
            request.CrCustomerAccountNumber = CreditAccount; //"00022050684877";//"000220000000419";//"20000000389";//"20000404951"; ///Account Number to which the amount will be credited;
            request.InstrumentType = "0022"; //tbp by SBL;
            request.TransactionAmount = TransactionAmount; // TransactionAmount;
            request.InstrumentDate = DateTime.Now; ;
            request.OrginatingTranIdentNo = 401201; //tbp by SBL same as transaction type;
            request.Currency = "PKR";//transaction.Currency;
            request.Authorization = "0";//(0 fro authorized payment) transaction.Authorization;
            request.TransactionNarration = !string.IsNullOrEmpty(TransactionNarration) ? TransactionNarration.Length >= 16 ? TransactionNarration.Substring(0, 16) : TransactionNarration : string.Empty; //Description of charges
            //adding transaction log 13 Jul 18, Amir Sultan
            FileHelper.RecoveryTransactionLog(false, request.DrCustomerAccountNumber, request.CrCustomerAccountNumber, request.TransactionAmount.ToString()
                , request.TransactionNarration, request.STAN, request.TransactionRefNo, "", "", "");
            response = cis.DoTransactions(request);
            FileHelper.RecoveryTransactionLog(true, request.DrCustomerAccountNumber, request.CrCustomerAccountNumber, request.TransactionAmount.ToString()
                , request.TransactionNarration, request.STAN, request.TransactionRefNo, response.TransactionIdentificationNo, response.ResponseCode, response.ResponseCodeDescription);

            return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool UpdateRecoveryTransaction(CISSB.TransactionInfoResponse response, CISSB.TransactionInfoRequest request, string RequestType, string STAN1)
        {
            try
            {
                STAN1 = STAN1.Substring(3, STAN1.Length - 3);
                bool IsAdded = true;

                using (var db = new SoneriCISEntities())
                {
                    tbl_transactions t = new tbl_transactions();
                    t = db.tbl_transactions.Where(x => x.STAN == STAN1).FirstOrDefault();
                    t.Narration = request.TransactionNarration;
                    t.Amount = double.Parse(request.TransactionAmount.ToString());
                    t.DebitAccountNo = request.DrCustomerAccountNumber;
                    t.CreditAccountNo = request.CrCustomerAccountNumber;
                    t.RequestType = RequestType;
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
        #endregion
        public static bool AddTransaction(CISSB.TransactionInfoResponse response, CISSB.TransactionInfoRequest request, string RequestType, string STAN1)
        {
            try
            {
                bool IsAdded = true;
                using (var db = new SoneriCISEntities())
                {
                    tbl_transactions t = new tbl_transactions();
                    t.Narration = request.TransactionNarration;
                    t.Amount = double.Parse(request.TransactionAmount.ToString());
                    t.DebitAccountNo = request.DrCustomerAccountNumber;
                    t.CreditAccountNo = request.CrCustomerAccountNumber;
                    t.RequestType = RequestType;
                    t.STAN = STAN1;
                    t.ResponseCode = response.ResponseCode;
                    t.ResponseCodeDescription = response.ResponseCodeDescription;
                    t.BranchCode = response.TransactionBranchCode;
                    t.TransactionDateTime = DateTime.Now;
                    t.TransactionIdNo = response.TransactionIdentificationNo;
                    t.TransactionRefNo = response.TransactionRefNo;
                    db.tbl_transactions.Add(t);
                    db.SaveChanges();
                }
                return IsAdded;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static bool AddTransaction(CISSB.TransactionInfoResponse response, CISSB.TransactionInfoRequest request, string RequestType, string STAN1, string CardNo)
        {
            try
            {
                bool IsAdded = false;
                using (var db = new SoneriCISEntities())
                {
                    tbl_transactions t = new tbl_transactions();
                    t.Narration = request.TransactionNarration;
                    t.Amount = double.Parse(request.TransactionAmount.ToString());
                    t.DebitAccountNo = request.DrCustomerAccountNumber;
                    t.CreditAccountNo = request.CrCustomerAccountNumber;
                    t.RequestType = RequestType;
                    t.STAN = STAN1.Substring(3, STAN1.Length - 3);//STAN1;
                    t.ResponseCode = response.ResponseCode;
                    t.ResponseCodeDescription = response.ResponseCodeDescription;
                    t.BranchCode = response.TransactionBranchCode;
                    t.TransactionDateTime = DateTime.Now;
                    t.TransactionIdNo = response.TransactionIdentificationNo;
                    t.TransactionRefNo = CardNo;
                    db.tbl_transactions.Add(t);
                    db.SaveChanges();
                    IsAdded = true;
                    FileHelper.RecoveryErrorLog("Income Transaction DB Insert", "Response Code:" + response.ResponseCode + "Response Desc:" + response.ResponseCodeDescription + "FT: " + response.TransactionIdentificationNo + "STAN:" + STAN1 + "FT:" + response.TransactionIdentificationNo, "Status:" + IsAdded, new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber().ToString());
                }
                return IsAdded;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static bool CheckAccountEligibility(CISSB.TransactionInfoResponse resp)
        {
            bool success = true;
            if (resp.ResponseCode == "0001" || resp.ResponseCode == "abc")
            {
                success = false;
            }
            if (resp.ResponseCode == "0001" || resp.ResponseCode == "")
            {
                success = false;
            }
            if (resp.ResponseCode == "0001" || resp.ResponseCode == "")
            {
                success = false;
            }

            return true;
        }



        public static CISSB.TransactionInfoResponse ReverseTransaction(string DrCustomerAccountNumber, decimal TransactionAmount, string TransactionNarration, string RequestType, string BranchCode, string RequestId, int LstCount)
        {
            DAL.DataAccessClasses.IncomeAccountsDataAccess da = new IncomeAccountsDataAccess();

            string CreditAccount = "";
            if (TransactionNarration.Contains("FED"))
            {
                //CreditAccount = "PKR172530001";
                CreditAccount = da.GetIncomeAccountByType("FED");
            }
            else if (RequestType == "RC")
            {
                //CreditAccount = "PL52529";
                CreditAccount = da.GetIncomeAccountByType("Annual");
            }
            else
            {
                CreditAccount = da.GetIncomeAccountByType("Setup");
            }

            string STAN1 = Core.Methods.CommonMethods.GetUniqueSTAN(RequestId, LstCount, RequestType);
            //string STAN1 = "CIF180318217";
            CISSB.TransactionInfoRequest request = new CISSB.TransactionInfoRequest();
            CISSB.TransactionInfoResponse response = new CISSB.TransactionInfoResponse();
            CISSB.BEAPITransactionService cis = new CISSB.BEAPITransactionService();
            cis.BEAPIAuthHeaderValue = GetHeader();
            request.TransactionDate = DateTime.Now;
            request.DrCustomerBranchCode = BranchCode;
            request.TransactionBranchCode = BranchCode;
            request.CrCustomerBranchCode = BranchCode;
            request.STAN = STAN1;//Transaction No auto increnment with seed value 10001;
            request.TransactionType = "401201"; //Transaction Type tbp by SBL;
            request.DrCustomerAccountNumber = CreditAccount; //"20001266498"; //Debit AccountNumber of Card Holder;
            request.CrCustomerAccountNumber = DrCustomerAccountNumber; //"00022050684877";//"000220000000419";//"20000000389";//"20000404951"; ///Account Number to which the amount will be credited;
            request.InstrumentType = "0022"; //tbp by SBL;
            request.TransactionAmount = TransactionAmount; // TransactionAmount;
            request.InstrumentDate = DateTime.Now; ;
            request.OrginatingTranIdentNo = 401201; //tbp by SBL same as transaction type;
            request.Currency = "PKR";//transaction.Currency;
            request.Authorization = "0";//(0 fro authorized payment) transaction.Authorization;
            request.TransactionNarration = !string.IsNullOrEmpty(TransactionNarration) ? TransactionNarration.Length >= 16 ? TransactionNarration.Substring(0, 16) : TransactionNarration : string.Empty; //Description of charges
            response = cis.DoTransactions(request);
            AddTransaction(response, request, RequestType, STAN1);
            return response;
        }

        //FOR RECOVERY
        public static CISSB.TransactionInfoResponse ReverseTransactionLatest(string DrCustomerAccountNumber, decimal TransactionAmount, string TransactionNarration, string RequestType, string BranchCode, string CardNo, string STAN)
        {
            DAL.DataAccessClasses.IncomeAccountsDataAccess da = new IncomeAccountsDataAccess();

            string CreditAccount = "";
            if (TransactionNarration.Contains("FED"))
            {
                //CreditAccount = "PKR172530001";
                CreditAccount = da.GetIncomeAccountByType("FED");
            }
            else if (!TransactionNarration.Contains("FED"))
            {

                CreditAccount = da.GetIncomeAccountByCardType(CardNo);
            }
            else
            {
                CreditAccount = da.GetIncomeAccountByType("Setup");
            }

            string STAN1 = STAN;//CommonMethods.GetRecoverySTAN(RequestId, TransType);
            //string STAN1 = "CIF180318217";
            CISSB.TransactionInfoRequest request = new CISSB.TransactionInfoRequest();
            CISSB.TransactionInfoResponse response = new CISSB.TransactionInfoResponse();
            CISSB.BEAPITransactionService cis = new CISSB.BEAPITransactionService();
            cis.BEAPIAuthHeaderValue = GetHeader();
            request.TransactionDate = DateTime.Now;
            request.DrCustomerBranchCode = BranchCode;
            request.TransactionBranchCode = BranchCode;
            request.CrCustomerBranchCode = BranchCode;
            request.STAN = STAN1;//Transaction No auto increnment with seed value 10001;
            request.TransactionType = "401201"; //Transaction Type tbp by SBL;
            request.DrCustomerAccountNumber = CreditAccount; //"20001266498"; //Debit AccountNumber of Card Holder;
            request.CrCustomerAccountNumber = DrCustomerAccountNumber; //"00022050684877";//"000220000000419";//"20000000389";//"20000404951"; ///Account Number to which the amount will be credited;
            request.InstrumentType = "0022"; //tbp by SBL;
            request.TransactionAmount = TransactionAmount; // TransactionAmount;
            request.InstrumentDate = DateTime.Now; ;
            request.OrginatingTranIdentNo = 401201; //tbp by SBL same as transaction type;
            request.Currency = "PKR";//transaction.Currency;
            request.Authorization = "0";//(0 fro authorized payment) transaction.Authorization;
            request.TransactionNarration = !string.IsNullOrEmpty(TransactionNarration) ? TransactionNarration.Length >= 16 ? TransactionNarration.Substring(0, 16) : TransactionNarration : string.Empty; //Description of charges
            FileHelper.RecoveryTransactionLog(false, request.DrCustomerAccountNumber, request.CrCustomerAccountNumber, request.TransactionAmount.ToString()
                , request.TransactionNarration, request.STAN, request.TransactionRefNo, "", "", "");
            response = cis.DoTransactions(request);
            FileHelper.RecoveryTransactionLog(true, request.DrCustomerAccountNumber, request.CrCustomerAccountNumber, request.TransactionAmount.ToString()
                , request.TransactionNarration, request.STAN, request.TransactionRefNo, response.TransactionIdentificationNo, response.ResponseCode, response.ResponseCodeDescription);

            //AddReverseTransaction(response, request, RequestType, STAN1, CardNo);
            return response;
        }


        #region New Recovery Log Work

        ///FED Transaction 
        public static CISSB.TransactionInfoResponse RecoveryFEDDebitTransaction(string DrCustomerAccountNumber, decimal TransactionAmount, string TransactionNarration, string RequestType, string BranchCode, string STAN, string CardNo)
        {
            DAL.DataAccessClasses.IncomeAccountsDataAccess da = new IncomeAccountsDataAccess();

            string CreditAccount = "";

            CreditAccount = da.GetIncomeAccountByType("FED");


            string STAN1 = STAN;
            //string STAN1 = "CIF30032018";
            CISSB.TransactionInfoRequest request = new CISSB.TransactionInfoRequest();
            CISSB.TransactionInfoResponse response = new CISSB.TransactionInfoResponse();
            CISSB.BEAPITransactionService cis = new CISSB.BEAPITransactionService();
            cis.BEAPIAuthHeaderValue = GetHeader();
            request.TransactionDate = DateTime.Now;
            request.DrCustomerBranchCode = BranchCode;
            request.TransactionBranchCode = BranchCode;
            request.CrCustomerBranchCode = BranchCode;
            request.STAN = STAN1;//Transaction No auto increnment with seed value 10001;
            request.TransactionType = "401201"; //Transaction Type tbp by SBL;
            request.DrCustomerAccountNumber = DrCustomerAccountNumber; //"20001266498"; //Debit AccountNumber of Card Holder;
            request.CrCustomerAccountNumber = CreditAccount; //"00022050684877";//"000220000000419";//"20000000389";//"20000404951"; ///Account Number to which the amount will be credited;
            request.InstrumentType = "0022"; //tbp by SBL;
            request.TransactionAmount = TransactionAmount; // TransactionAmount;
            request.InstrumentDate = DateTime.Now; ;
            request.OrginatingTranIdentNo = 401201; //tbp by SBL same as transaction type;
            request.Currency = "PKR";//transaction.Currency;
            request.Authorization = "0";//(0 fro authorized payment) transaction.Authorization;
            request.TransactionNarration = !string.IsNullOrEmpty(TransactionNarration) ? TransactionNarration.Length >= 16 ? TransactionNarration.Substring(0, 16) : TransactionNarration : string.Empty; //Description of charges
            FileHelper.RecoveryTransactionLog(false, request.DrCustomerAccountNumber, request.CrCustomerAccountNumber, request.TransactionAmount.ToString()
                , request.TransactionNarration, request.STAN, request.TransactionRefNo, "", "", "");

            response = cis.DoTransactions(request);
            FileHelper.RecoveryTransactionLog(true, request.DrCustomerAccountNumber, request.CrCustomerAccountNumber, request.TransactionAmount.ToString()
                            , request.TransactionNarration, request.STAN, request.TransactionRefNo, response.TransactionIdentificationNo, response.ResponseCode, response.ResponseCodeDescription);


            return response;
        }

        
        #endregion
    }
}