using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class RequestDataAccess
    {
        #region Insert
        public bool AddRequest(tbl_Requests row)
        {
            row.RequestDate = DateTime.Now;
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Requests.Add(row);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public long AddRequest2(tbl_Requests row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Requests.Add(row);
                    db.SaveChanges();
                    return row.ID;
                }
                
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        #endregion

        #region Update
        public bool UpdateRequest(tbl_Requests row , out long id)
        {
            row.RequestDate = DateTime.Now;
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Requests val = new DAL.tbl_Requests();
                    val = db.tbl_Requests.Where(a => a.ID == row.ID).FirstOrDefault();
                    val.RequestType = row.RequestType;
                    //val.AccountNo = row.AccountNo;
                    //val.CIFNo = row.CIFNo;
                    //val.CardNo = row.CardNo;
                    val.WaiveCharges = row.WaiveCharges;
                    val.StatusEligibility = row.StatusEligibility;
                    val.FinancialEligibility = row.FinancialEligibility;
                    val.AuthorizationComments = row.AuthorizationComments;
                    val.AuthorizationStatus = row.AuthorizationStatus;
                    val.RequestDate = row.RequestDate;
                    val.CardTitle = row.CardTitle;
                    val.CardTypeID = row.CardTypeID;
                    val.Salutation = row.Salutation;
                    val.LinkingDelinkingAccount = row.LinkingDelinkingAccount;
                    val.IsActive = row.IsActive;
                    val.IsExported = row.IsExported;
                    db.SaveChanges();
                    id = val.ID;
                }
                return true;
            }
            catch (Exception ex)
            {
                id = 0;
                return false;
            }
        }

        public bool UpdateRequestAuthorization(long RequestID, string Status, string Comments)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Requests val = new DAL.tbl_Requests();
                    val = db.tbl_Requests.Where(a => a.ID == RequestID).FirstOrDefault();
                    val.ID = val.ID;
                    val.RequestType = val.RequestType;
                    val.AccountNo = val.AccountNo;
                    val.CIFNo = val.CIFNo;
                    val.CardNo = val.CardNo;
                    val.WaiveCharges = val.WaiveCharges;
                    val.StatusEligibility = val.StatusEligibility;
                    val.FinancialEligibility = val.FinancialEligibility;
                    val.AuthorizationComments = Comments;
                    val.AuthorizationStatus = Status;
                    val.RequestDate = val.RequestDate;
                    val.CardTitle = val.CardTitle;
                    val.CardTypeID = val.CardTypeID;
                    val.Salutation = val.Salutation;
                    val.LinkingDelinkingAccount = val.LinkingDelinkingAccount;
                    val.IsActive = val.IsActive;
                    val.IsExported = val.IsExported;
                    db.SaveChanges();

                    if (Status == "A")
                    {
                        /* Add new row in user log, as because we can't get the authorizer's record of the request, as because the authorizer has functionality to export the file, when file export occurs, the authorizer changes as we get record of the user who update the request in last */
                        new UserLogDataAccess().AddUserLog(null, "tbl_Requests", "A", val.ID.ToString());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool UpdateRequestAuthorization(long RequestID, string Status, string Comments,int AuthorizerID)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Requests val = new DAL.tbl_Requests();
                    val = db.tbl_Requests.Where(a => a.ID == RequestID).FirstOrDefault();
                    val.ID = val.ID;
                    val.RequestType = val.RequestType;
                    val.AccountNo = val.AccountNo;
                    val.CIFNo = val.CIFNo;
                    val.CardNo = val.CardNo;
                    val.WaiveCharges = val.WaiveCharges;
                    val.StatusEligibility = val.StatusEligibility;
                    val.FinancialEligibility = val.FinancialEligibility;
                    val.AuthorizationComments = Comments;
                    val.AuthorizationStatus = Status;
                    val.RequestDate = val.RequestDate;
                    val.CardTitle = val.CardTitle;
                    val.CardTypeID = val.CardTypeID;
                    val.Salutation = val.Salutation;
                    val.LinkingDelinkingAccount = val.LinkingDelinkingAccount;
                    val.IsActive = val.IsActive;
                    val.IsExported = val.IsExported;
                    val.AuthorizerID = AuthorizerID;
                    db.SaveChanges();

                    if (Status == "A")
                    {
                        /* Add new row in user log, as because we can't get the authorizer's record of the request, as because the authorizer has functionality to export the file, when file export occurs, the authorizer changes as we get record of the user who update the request in last */
                        new UserLogDataAccess().AddUserLog(null, "tbl_Requests", "A", val.ID.ToString());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateRequestAfterExport(List<long> ids)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Requests.Where(a => ids.Contains(a.ID)).ToList().ForEach(e=> { e.IsExported = true; });
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool UpdateRequestAuthorization(long RequestID, string Status, string Comments,bool StatusEligibility,bool FinancialEligibility)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Requests val = new DAL.tbl_Requests();
                    val = db.tbl_Requests.Where(a => a.ID == RequestID).FirstOrDefault();
                    val.AuthorizationStatus = Status;
                    val.AuthorizationComments = Comments;
                    val.StatusEligibility = StatusEligibility;
                    val.FinancialEligibility = FinancialEligibility;
                    val.AuthorizerID = int.Parse(Convert.ToString(HttpContext.Current.Session["UserId"]));
                    db.SaveChanges();

                    if (Status == "A")
                    {
                        /* Add new row in user log, as because we can't get the authorizer's record of the request, as because the authorizer has functionality to export the file, when file export occurs, the authorizer changes as we get record of the user who update the request in last */
                        new UserLogDataAccess().AddUserLog(null, "tbl_Requests", "A", val.ID.ToString());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Select
        public List<tbl_Requests> GetAllRequests()
        {
            try
            {
                List<tbl_Requests> lst = new List<DAL.tbl_Requests>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Requests.Where(a => a.IsActive == true).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public tbl_Requests GetRequestById(int RequestID)
        //{
        //    try
        //    {
        //        tbl_Requests request= new DAL.tbl_Requests();
        //        using (var db = new SoneriCISEntities())
        //        {
        //            request = db.tbl_Requests.Where(a => a.ID==RequestID && a.AuthorizationStatus == "P").FirstOrDefault();
        //        }
        //        return request;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public tbl_Requests GetRequestById(int RequestID, string RequestType)
        {
            try
            {
                tbl_Requests request = new DAL.tbl_Requests();
                using (var db = new SoneriCISEntities())
                {
                    request = db.tbl_Requests.Where(a => a.AuthorizationStatus == "P" && a.IsActive == true && a.RequestType == RequestType && a.ID == RequestID).FirstOrDefault();
                }
                return request;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Requests GetRequestForImport(string CIF,string AccountNo)
        {
            try
            {
                tbl_Requests request = new DAL.tbl_Requests();
                using (var db = new SoneriCISEntities())
                {
                    request = db.tbl_Requests.Where(a => a.AuthorizationStatus == "A" && a.IsActive == true && a.IsExported == true
                    && (a.RequestType == "R" || a.RequestType == "N") && a.CIFNo == CIF && a.AccountNo == AccountNo).OrderByDescending(e => e.ID).FirstOrDefault();
                }
                return request;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Requests GetRequestToBeReviewed(int RequestID, string RequestType)
        {
            try
            {
                tbl_Requests request = new tbl_Requests();
                using (var db = new SoneriCISEntities())
                {
                    request = db.tbl_Requests.Where(a => a.AuthorizationStatus == "R" && a.IsActive == true
                    && a.RequestType == RequestType && a.ID == RequestID).FirstOrDefault();
                }
                return request;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Requests GetRequestByCardNo(string CardNo,string AuthorizationStatus)
        {
            try
            {
                tbl_Requests request = new DAL.tbl_Requests();
                using (var db = new SoneriCISEntities())
                {
                    List<tbl_Requests> lst = db.tbl_Requests.Where(a => a.AuthorizationStatus == AuthorizationStatus && a.IsActive == true
                     && a.CardNo==CardNo).ToList();
                    request = lst.Where(a => a.RequestType == "A" || a.RequestType == "R").FirstOrDefault();
                }
                return request;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool IsDuplicateRequest(string CardNo, string AccountNo, string RequestType, string CIF, out long requestId)
        {
            try
            {
                bool IsDuplicate = false;
                List<tbl_Requests> lstRequest = new List<tbl_Requests>();

                using (var db = new SoneriCISEntities())
                {
                    if (RequestType == "N" /* || RequestType == "R"*/) // Change: Can create replacement request if replacement request already exists, but not for new card request
                    {
                        //validating by account number in case of New card or Replacement requests
                        lstRequest = db.tbl_Requests.Where(a => a.AccountNo == AccountNo && a.CIFNo == CIF && a.RequestType == RequestType
                              && a.AuthorizationStatus != "C" && a.IsActive == true).ToList();
                    }
                    else if (RequestType == "R")
                    {
                        lstRequest = db.tbl_Requests.Where(a => a.CardNo == CardNo && a.AccountNo == AccountNo && a.CIFNo == CIF && a.RequestType == RequestType && a.AuthorizationStatus != "C" && a.IsActive == true).ToList();
                    }
                    else if (RequestType == "H")
                    {
                        lstRequest = db.tbl_Requests.Where(a => a.CardNo == CardNo && a.RequestType == RequestType && a.AuthorizationStatus== "P" && a.IsActive == true).ToList();
                    }
                    else
                    {
                        lstRequest = db.tbl_Requests.Where(a => a.CardNo == CardNo && a.RequestType == RequestType
                        && a.AuthorizationStatus != "A" && a.AuthorizationStatus != "C" && a.IsActive == true).ToList();
                    }
                }
                if (lstRequest.Count > 0)
                {
                    IsDuplicate = true;
                }

                requestId = lstRequest.Count > 0 ? lstRequest.OrderByDescending(e => e.ID).FirstOrDefault()?.ID ?? 0 : 0;

                return IsDuplicate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetRequestDataForExport(string requestType, int cardType, string branchCode)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@RequestType", requestType));
            parameters.Add(new SqlParameter("@CardTypeId", cardType));
            parameters.Add(new SqlParameter("@BranchCode", branchCode));

            var data = DatabaseGateway.GetDataUsingStoredProcedure("sp_ExportData", parameters);
            return data;
        }
        public DataTable GetRequestDataForExport(string requestType, int cardType, string branchCode, DateTime? requestDate)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@RequestType", requestType));
            parameters.Add(new SqlParameter("@CardTypeId", cardType));
            parameters.Add(new SqlParameter("@BranchCode", branchCode));
            parameters.Add(new SqlParameter("@RequestDate", requestDate));

            var data = DatabaseGateway.GetDataUsingStoredProcedure("sp_ReExportData", parameters);
            return data;
        }


        public DataTable GetRequestDataForPlainExport(string requestType, int cardType, string branchCode, DateTime? requestDate)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@RequestType", requestType));
            parameters.Add(new SqlParameter("@CardTypeId", cardType));
            parameters.Add(new SqlParameter("@BranchCode", branchCode));
            parameters.Add(new SqlParameter("@RequestDate", requestDate));

            var data = DatabaseGateway.GetDataUsingStoredProcedure("sp_PlainExportData", parameters);
            return data;
        }

        public List<tbl_Requests> GetRequestList(List<string> AccountNos)
        {
            try
            {
                List<tbl_Requests> lst = new List<DAL.tbl_Requests>();
                using (var db = new SoneriCISEntities())
                {
                    var requests = from r in db.tbl_Requests
                              where AccountNos.Contains(r.AccountNo)
                              && r.IsActive==true && r.AuthorizationStatus=="A"
                              && r.IsExported==true
                              && (r.RequestType=="N" || r.RequestType=="R")
                              select r;
                    lst = requests.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<tbl_Requests> GetRequestList(List<string> AccountNos,string RequestType)
        {
            try
            {
                List<tbl_Requests> lst = new List<DAL.tbl_Requests>();
                using (var db = new SoneriCISEntities())
                {
                    var requests = from r in db.tbl_Requests
                                   where AccountNos.Contains(r.AccountNo)
                                   && r.IsActive == true && r.AuthorizationStatus == "A"
                                   && r.IsExported == true
                                   && r.RequestType == RequestType
                                   select r;
                    lst = requests.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetRequestDataForReport(string CardNo, string AccountNo, string BranchCode, int? CardTypeId, string CIF, long? RequestNo, int? Waive, string RequestType,string ReportType, DateTime StartDate, DateTime EndDate, string AuthorizeStatus)
        {
            try
            {
                string startDate = Convert.ToString(StartDate);
                string endDate = Convert.ToString(EndDate);
                var builder = new StringBuilder();

                if (RequestType == "N" && ReportType == "false")
                {
                    builder.Append("exec sp_Reports_CardIssuance ");
                    builder.Append("'" + BranchCode + "','" + startDate + "','" + endDate + "',");
                    builder.Append("'" + AccountNo + "','" + CIF + "','" + CardNo + "',");
                    builder.Append("'" + RequestNo + "'," + Waive + "," + CardTypeId + ",");
                    builder.Append("'" + RequestType + "','" + AuthorizeStatus + "'");
                }
                else if (RequestType == "N" && ReportType == "true")
                {
                    builder.Append("exec sp_Reports_CardIssuance_log ");
                    builder.Append("'" + BranchCode + "','" + startDate + "','" + endDate + "',");
                    builder.Append("'" + AccountNo + "','" + CIF + "','" + CardNo + "',");
                    builder.Append("'" + RequestNo + "'," + Waive + "," + CardTypeId + ",");
                    builder.Append("'" + RequestType + "','" + AuthorizeStatus + "'");
                }

                else if (RequestType == "R" && ReportType == "false")
                {
                    builder.Append("exec sp_Reports_CardReplacement ");
                    builder.Append("'" + BranchCode + "','" + startDate + "','" + endDate + "',");
                    builder.Append("'" + AccountNo + "','" + CIF + "','" + CardNo + "',");
                    builder.Append("'" + RequestNo + "'," + Waive + "," + CardTypeId + ",");
                    builder.Append("'" + RequestType + "','" + AuthorizeStatus + "'");
                }
                else if (RequestType == "R" && ReportType == "true")
                {
                    builder.Append("exec sp_Reports_CardReplacement_log ");
                    builder.Append("'" + BranchCode + "','" + startDate + "','" + endDate + "',");
                    builder.Append("'" + AccountNo + "','" + CIF + "','" + CardNo + "',");
                    builder.Append("'" + RequestNo + "'," + Waive + "," + CardTypeId + ",");
                    builder.Append("'" + RequestType + "','" + AuthorizeStatus + "'");
                }
                else if (RequestType == "A" && ReportType =="false")
                {
                    builder.Append("exec sp_Reports_CardAmendment ");
                    builder.Append("'" + BranchCode + "','" + startDate + "','" + endDate + "',");
                    builder.Append("'" + AccountNo + "','" + CIF + "','" + CardNo + "',");
                    builder.Append("'" + RequestNo + "','" + AuthorizeStatus + "'");
                }
                else if (RequestType == "A" && ReportType == "true")
                {
                    builder.Append("exec sp_Reports_CardAmendment_log ");
                    builder.Append("'" + BranchCode + "','" + startDate + "','" + endDate + "',");
                    builder.Append("'" + AccountNo + "','" + CIF + "','" + CardNo + "',");
                    builder.Append("'" + RequestNo + "','" + AuthorizeStatus + "'");
                }
                else if (RequestType == "U")
                {
                    builder.Append("exec sp_Reports_CardUpgrade ");
                    builder.Append("'" + BranchCode + "','" + startDate + "','" + endDate + "',");
                    builder.Append("'" + AccountNo + "','" + CIF + "','" + CardNo + "',");
                    builder.Append("'" + RequestNo + "'," + Waive + "," + CardTypeId + ",");
                    builder.Append("'" + RequestType + "','" + AuthorizeStatus + "'");
                }
                else if (RequestType == "H" && ReportType == "false")
                {
                    builder.Append("exec sp_Reports_CardHotMark ");
                    builder.Append("'" + BranchCode + "','" + startDate + "','" + endDate + "',");
                    builder.Append("'" + AccountNo + "','" + CIF + "','" + CardNo + "',");
                    builder.Append("'" + RequestNo + "'," + CardTypeId + ",'" + AuthorizeStatus + "'");
                }
                else if (RequestType == "H" && ReportType == "false")
                {
                    builder.Append("exec sp_Reports_CardHotMark ");
                    builder.Append("'" + BranchCode + "','" + startDate + "','" + endDate + "',");
                    builder.Append("'" + AccountNo + "','" + CIF + "','" + CardNo + "',");
                    builder.Append("'" + RequestNo + "'," + CardTypeId + ",'" + AuthorizeStatus + "'");
                }


                //else if (RequestType == "H" && ReportType == "true")
                //{
                //    builder.Append("exec sp_Reports_CardHotMark_log");
                //    builder.Append("'" + BranchCode + "','" + startDate + "','" + endDate + "',");
                //    builder.Append("'" + AccountNo + "','" + CIF + "','" + CardNo + "',");
                //    builder.Append("'" + RequestNo + "'," + Waive + "," + CardTypeId + ",");
                //    builder.Append("'" + RequestType + "','" + AuthorizeStatus + "'");
                //}

                else if (RequestType == "L" && ReportType == "false")
                {
                    builder.Append("exec sp_Reports_CardLinking ");
                    builder.Append("'" + BranchCode + "','" + startDate + "','" + endDate + "',");
                    builder.Append("'" + AccountNo + "','" + CIF + "','" + CardNo + "',");
                    builder.Append("'" + RequestNo + "','" + AuthorizeStatus + "'");
                }
                else if (RequestType == "L" && ReportType == "true")
                {
                    builder.Append("exec sp_Reports_CardLinking_log");
                    builder.Append("'" + BranchCode + "','" + startDate + "','" + endDate + "',");
                    builder.Append("'" + AccountNo + "','" + CIF + "','" + CardNo + "',");
                    builder.Append("'" + RequestNo + "'," + Waive + "," + CardTypeId + ",");
                    builder.Append("'" + RequestType + "','" + AuthorizeStatus + "'");
                }

                else if (RequestType == "D" && ReportType == "false")
                {
                    builder.Append("exec sp_Reports_CardDeLinking ");
                    builder.Append("'" + BranchCode + "','" + startDate + "','" + endDate + "',");
                    builder.Append("'" + AccountNo + "','" + CIF + "','" + CardNo + "',");
                    builder.Append("'" + RequestNo + "','" + AuthorizeStatus + "'");
                }

                else if (RequestType == "D" && ReportType == "true")
                {
                    builder.Append("exec sp_Reports_CardDeLinking_log");
                    builder.Append("'" + BranchCode + "','" + startDate + "','" + endDate + "',");
                    builder.Append("'" + AccountNo + "','" + CIF + "','" + CardNo + "',");
                    builder.Append("'" + RequestNo + "'," + Waive + "," + CardTypeId + ",");
                    builder.Append("'" + RequestType + "','" + AuthorizeStatus + "'");
                }


                var query = builder.ToString();
                return string.IsNullOrEmpty(query) ? new DataTable() : DatabaseGateway.GetDataUsingStoredProcedureQuery(query);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Requests GetRequest(string CardNo, string AccountNo, string CIF)
        {
            using (var db = new SoneriCISEntities())
            {
                var query = db.tbl_Requests.Where(e => e.CardNo == CardNo && e.AccountNo == AccountNo && e.CIFNo == CIF && e.AuthorizationStatus == "A" && e.RequestType == "R" && e.IsActive == true).FirstOrDefault();

                if (query == null)
                {
                    //return db.tbl_Requests.FirstOrDefault(e => e.AccountNo == AccountNo && e.CIFNo == CIF && e.IsActive == true && e.RequestType == "N" && e.AuthorizationStatus == "A" && e.IsExported == true);
                    return db.tbl_Requests.Where(e => e.AccountNo == AccountNo && e.CIFNo == CIF && e.IsActive == true && e.AuthorizationStatus == "A" && (e.RequestType == "R" || e.RequestType == "N") && e.IsExported == true).OrderByDescending(e => e.ID).FirstOrDefault();
                }
                else
                {
                    return db.tbl_Requests.Where(e => e.AccountNo == AccountNo && e.CIFNo == CIF && e.IsActive == true && e.AuthorizationStatus == "A" && (e.RequestType == "R" || e.RequestType == "N") && e.IsExported == true && e.ID < query.ID).OrderByDescending(e=>e.ID).FirstOrDefault();
                }
            }
        }

        public DataTable GetRecoveryDataForReport(string CardNo, string AccountNo, string BranchCode, DateTime StartDate, DateTime EndDate)
        {
            string startDate = Convert.ToString(StartDate);
            string endDate = Convert.ToString(EndDate);
            var builder = new StringBuilder();

            try
            {
                builder.Append("exec sp_Reports_Recovery ");
                builder.Append("'" + BranchCode + "','" + startDate + "','" + endDate + "',");
                builder.Append("'" + AccountNo + "','" + CardNo + "'");

                var query = builder.ToString();
                return string.IsNullOrEmpty(query) ? new DataTable() : DatabaseGateway.GetDataUsingStoredProcedureQuery(query);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Check if Creator and Authorizer are same
        public bool CheckSameAuthorizer(int AuthorizerID,int RequestID)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Requests r = db.tbl_Requests.Where(a => a.ID == RequestID && a.CreatorID == AuthorizerID).FirstOrDefault();
                    if (r != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion
        #region New Recovery Reports
        public DataTable GetDataForCurrentRecovery(string CardNo, string AccountNo, string BranchCode, DateTime StartDate, DateTime EndDate, string RequestType, string CIF)
        {
            string startDate = StartDate.Date.ToString("d");
            string endDate = EndDate.Date.ToString("d");
            var builder = new StringBuilder();

            try
            {
                builder.Append("exec sp_Current_Recovery_Status");
                builder.Append("'" + CardNo + "','" + AccountNo + "',");
                builder.Append("'" + CIF + "','" + BranchCode + "',");
                builder.Append("'" + startDate + "','" + endDate + "','" + RequestType + "'");


                var query = builder.ToString();
                return string.IsNullOrEmpty(query) ? new DataTable() : DatabaseGateway.GetDataUsingStoredProcedureQuery(query);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public DataTable GetDataForDailyIncome(string CardNo, string AccountNo, string BranchCode, DateTime? TransDate)
        {
            var TransactionDate = TransDate.Value.Date.ToString("d");

            var builder = new StringBuilder();

            try
            {
                builder.Append("exec sp_Daily_Income");
                builder.Append(" '" + CardNo + "','" + AccountNo + "',");
                builder.Append("'" + BranchCode + "','" + TransactionDate + "'");



                var query = builder.ToString();
                return string.IsNullOrEmpty(query) ? new DataTable() : DatabaseGateway.GetDataUsingStoredProcedureQuery(query);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetDataForDailyReversal(string CardNo, string AccountNo, string BranchCode, DateTime? TransDate)
        {
            var TransactionDate = TransDate.Value.Date.ToString("d");

            var builder = new StringBuilder();

            try
            {
                builder.Append("exec sp_Daily_Reversals");
                builder.Append(" '" + CardNo + "','" + AccountNo + "',");
                builder.Append("'" + BranchCode + "','" + TransactionDate + "'");



                var query = builder.ToString();
                return string.IsNullOrEmpty(query) ? new DataTable() : DatabaseGateway.GetDataUsingStoredProcedureQuery(query);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetDataForDailyTransaction(string CardNo, string AccountNo, string BranchCode, DateTime? TransDate)
        {
            var TransactionDate = TransDate.Value.Date.ToString("d");

            var builder = new StringBuilder();

            try
            {
                builder.Append("exec sp_Daily_Transactions");
                builder.Append(" '" + CardNo + "','" + AccountNo + "',");
                builder.Append("'" + BranchCode + "','" + TransactionDate + "'");



                var query = builder.ToString();
                return string.IsNullOrEmpty(query) ? new DataTable() : DatabaseGateway.GetDataUsingStoredProcedureQuery(query);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetDataExceptionReport(string CardNo, string AccountNo, string BranchCode, string RequestType, string CIF, DateTime? TransDate)
        {
            var TransactionDate = TransDate.Value.Date.ToString("d");

            var builder = new StringBuilder();

            try
            {
                builder.Append("exec sp_Exception_Report");
                builder.Append(" '" + CardNo + "','" + AccountNo + "',");
                builder.Append("'" + CIF + "',");
                builder.Append("'" + BranchCode + "','" + TransactionDate + "',");
                builder.Append("'" + RequestType + "'");



                var query = builder.ToString();
                return string.IsNullOrEmpty(query) ? new DataTable() : DatabaseGateway.GetDataUsingStoredProcedureQuery(query);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
