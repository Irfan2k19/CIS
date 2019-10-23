using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
   public class RecoveryDataAccess
    {
        #region Insert
        public bool AddCardRecovery(tbl_Card_Charges_Recovery row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Card_Charges_Recovery.Add(row);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool AddLowBalanceRecovery(int RecoveryId)
        {

            try
            {
                //using (var db = new SoneriCISEntities())
                //{
                //    tbl_Recovery_Low_Balance row = new tbl_Recovery_Low_Balance();
                //    row.RecoveryID = RequsetId;
                //    row.RecoveryDate = DateTime.Now;
                //    db.tbl_Recovery_Low_Balance.Add(row);
                //    db.SaveChanges();
                //}
                //return true;
                List<SqlParameter> param = new List<SqlParameter>();
                param.Add(new SqlParameter("@RecoveryId", RecoveryId));
               

                var success = Convert.ToString(DatabaseGateway.GetScalarDataUsingStoredProcedure("sp_Add_Recovery_Low_Balance", param));

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Update
        public bool UpdateCardRecovery(tbl_Card_Charges_Recovery row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Card_Charges_Recovery val = new DAL.tbl_Card_Charges_Recovery();
                    val = db.tbl_Card_Charges_Recovery.Where(a => a.ID == row.ID).FirstOrDefault();
                    val.CardID = row.CardID;
                    val.ApplicableAmount = row.ApplicableAmount;
                    val.ApplicableDate = row.ApplicableDate;
                    val.CardChargesID = row.CardChargesID;
                    val.DeducatedAmount = row.DeducatedAmount;
                    val.LastDeducationDate = DateTime.Now;//row.LastDeducationDate;
                    val.CardNo = row.CardNo;
                    val.Regional_Charges_ID = row.Regional_Charges_ID;
                    val.AccountNo = row.AccountNo;
                    val.AccountCode = row.AccountCode;
                    val.ChargesDescription = row.ChargesDescription;
                    val.IsValid = row.IsValid;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateCardRecoveryAmount(int RecoveryID,float Deduction)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Card_Charges_Recovery val = new DAL.tbl_Card_Charges_Recovery();
                    val = db.tbl_Card_Charges_Recovery.Where(a => a.ID == RecoveryID).FirstOrDefault();
                    val.CardID = val.CardID;
                    val.ApplicableAmount = val.ApplicableAmount;
                    val.ApplicableDate = val.ApplicableDate;
                    val.CardChargesID = val.CardChargesID;
                    //val.DeducatedAmount = Deduction;
                    val.DeducatedAmount = Math.Round(Deduction, 2);
                    val.LastDeducationDate = DateTime.Now;
                    val.CardNo = val.CardNo;
                    val.Regional_Charges_ID = val.Regional_Charges_ID;
                    val.AccountNo = val.AccountNo;
                    val.AccountCode = val.AccountCode;
                    val.ChargesDescription = val.ChargesDescription;
                    val.IsValid = val.IsValid;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool UpdateLastTransactionDate(int RecoveryID)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Card_Charges_Recovery val = new DAL.tbl_Card_Charges_Recovery();
                    val = db.tbl_Card_Charges_Recovery.Where(a => a.ID == RecoveryID).FirstOrDefault();
                    val.LastDeducationDate = DateTime.Now;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Delete



        #endregion

        #region Select

        public List<tbl_Card_Charges_Recovery> GetAllCardChargesRecovery()
        {
            try
            {
                List<tbl_Card_Charges_Recovery> lst = new List<DAL.tbl_Card_Charges_Recovery>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Card_Charges_Recovery.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Card_Charges_Recovery> GetPendingRecovery()
        {
            var current = DateTime.Now;
            try
            {
                List<tbl_Card_Charges_Recovery> lst = new List<DAL.tbl_Card_Charges_Recovery>();
                using (var db = new SoneriCISEntities())
                {
                    // lst = db.tbl_Card_Charges_Recovery.Where(a => (a.ApplicableAmount-a.DeducatedAmount)!=0).ToList();
                    lst = db.tbl_Card_Charges_Recovery.Where(a => a.IsValid == true && a.ApplicableDate <= current).ToList();
                    lst = lst.Where(a => a.ApplicableAmount > a.DeducatedAmount).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Card_Charges_Recovery> GetRecoveryByCardID(int CardID)
        {
            try
            {
                List<tbl_Card_Charges_Recovery> lst = new List<DAL.tbl_Card_Charges_Recovery>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Card_Charges_Recovery.Where(a => a.CardID== CardID).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Card_Charges_Recovery GetCardChargeById(int CardChargeID)
        {
            try
            {
                tbl_Card_Charges_Recovery Val = new DAL.tbl_Card_Charges_Recovery();
                using (var db = new SoneriCISEntities())
                {
                    Val = db.tbl_Card_Charges_Recovery.Where(a => a.ID == CardChargeID).FirstOrDefault();
                }
                return Val;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        public List<tbl_Card_Charges_Recovery> GetPendingRecovery(bool IsFullRecovery, string FromBranchCode, string ToBranchCode)
        {


            var current = DateTime.Now;
            try
            {
                List<tbl_Card_Charges_Recovery> lst = new List<DAL.tbl_Card_Charges_Recovery>();
                using (var db = new SoneriCISEntities())
                {

                    // lst = db.tbl_Card_Charges_Recovery.Where(a => (a.ApplicableAmount-a.DeducatedAmount)!=0).ToList();
                    lst = db.tbl_Card_Charges_Recovery.Where(a => a.IsValid == true && a.ApplicableDate <= current).ToList();
                    lst = lst.Where(a => a.ApplicableAmount > a.DeducatedAmount).ToList();
                    if (IsFullRecovery)
                    {
                        lst = lst.Where(a => a.LastDeducationDate == null).ToList();
                    }
                    else
                    {
                        lst = lst.Where(a => a.LastDeducationDate != null).ToList();
                    }
                }
                if (FromBranchCode != "" && ToBranchCode != "")
                {
                    lst = lst.Where(a => a.AccountNo.Substring(0, 4).CompareTo(FromBranchCode) >= 0 && a.AccountNo.Substring(0, 4).CompareTo(ToBranchCode) <= 0).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int GetPendingRecoveryCount(bool IsFullRecovery, string FromBranchCode, string ToBranchCode)
        {
            var current = DateTime.Now;
            try
            {
                List<tbl_Card_Charges_Recovery> lst = new List<DAL.tbl_Card_Charges_Recovery>();
                using (var db = new SoneriCISEntities())
                {

                    // lst = db.tbl_Card_Charges_Recovery.Where(a => (a.ApplicableAmount-a.DeducatedAmount)!=0).ToList();
                    lst = db.tbl_Card_Charges_Recovery.Where(a => a.IsValid == true && a.ApplicableDate <= current).ToList();
                    lst = lst.Where(a => a.ApplicableAmount > a.DeducatedAmount).ToList();
                    if (IsFullRecovery)
                    {
                        lst = lst.Where(a => a.LastDeducationDate == null).ToList();
                    }
                    else
                    {
                        lst = lst.Where(a => a.LastDeducationDate != null).ToList();
                    }
                }
                if (FromBranchCode != "" && ToBranchCode != "")
                {
                    lst = lst.Where(a => a.AccountNo.Substring(0, 4).CompareTo(FromBranchCode) >= 0 && a.AccountNo.Substring(0, 4).CompareTo(ToBranchCode) <= 0).ToList();
                }
                return lst.Count;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public string GetLastApplicableDate()
        {
            try
            {
                string result;
                var db = new SoneriCISEntities();
                //var context = (from c in db.tbl_Card_Charges_Recovery select c.ApplicableDate).LastOrDefault();
                var context = db.tbl_Card_Charges_Recovery.OrderByDescending(r=>r.ID).First();
                
                result = context.ApplicableDate.Value.AddDays(1).Date.ToString("d");
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }


        public string GetNewRecoveryCount(DateTime StartDate, DateTime EndDate)
        {
            List<SqlParameter> param = new List<SqlParameter>();
            
            param.Add(new SqlParameter("@Date", StartDate));
            param.Add(new SqlParameter("@ToDate", EndDate));

            var success = Convert.ToString(DatabaseGateway.GetScalarDataUsingStoredProcedure("sp_Get_New_Recovery_Records_Count", param));
            return success;
        }


        public bool InsertNewRecovery(DateTime StartDate, DateTime EndDate)
        {
            
            List<SqlParameter> param = new List<SqlParameter>();

            param.Add(new SqlParameter("@Date", StartDate));
            param.Add(new SqlParameter("@ToDate", EndDate));

            var success = Convert.ToString(DatabaseGateway.GetScalarDataUsingStoredProcedure("sp_Insert_New_Recovery_Records_Count", param));
            return true;
        }

        #endregion
    }
}
