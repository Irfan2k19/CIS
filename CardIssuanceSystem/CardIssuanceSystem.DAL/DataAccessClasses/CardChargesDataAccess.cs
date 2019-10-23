using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
   public class CardChargesDataAccess
    {

        #region Insert
        public bool AddCardCharges(tbl_Card_Charges row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    if (db.tbl_Card_Charges.Any(e => e.Title.ToLower() == row.Title.ToLower()))
                        throw new CustomException("Title already exists!");

                    db.tbl_Card_Charges.Add(row);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool AddCardChargesWithRegionalCharges(tbl_Card_Charges CardCharges,tbl_Regional_Charges RegionalCharges)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Card_Charges.Add(CardCharges);
                    db.SaveChanges();

                    RegionalCharges.CardChargeID = CardCharges.ID;
                    AddRegionalCharges(RegionalCharges);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool AddRegionalCharges(tbl_Regional_Charges row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Regional_Charges.Add(row);
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

        #region Update
        public bool UpdateCardCharges(tbl_Card_Charges row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Card_Charges val = new DAL.tbl_Card_Charges();
                    val = db.tbl_Card_Charges.Where(a => a.ID == row.ID).FirstOrDefault();
                    if (db.tbl_Card_Charges.Any(e => e.Title.ToLower() != val.Title.ToLower() && e.Title.ToLower() == row.Title.ToLower()))
                        throw new CustomException("Title already exists!");

                    val.Title = row.Title;
                    val.Amount = row.Amount;
                    val.AuthorizationComments = row.AuthorizationComments;
                    val.AuthorizationStatus = row.AuthorizationStatus;
                    val.CardTypeID = row.CardTypeID;
                    val.IsActive = row.IsActive;
                    val.IsFED = row.IsFED;
                    val.IsReplacement = row.IsReplacement;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateRegionalCharges(tbl_Regional_Charges row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Regional_Charges val = new DAL.tbl_Regional_Charges();
                    val = db.tbl_Regional_Charges.Where(a => a.ID == row.ID).FirstOrDefault();
                    val.PercentageAmount = row.PercentageAmount;
                    val.RegionID = row.RegionID;
                    val.CardChargeID = row.CardChargeID;
                    val.TotalAmount = row.TotalAmount;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool Authorize(int RequestID, string Status, string Comments)
        {
            using (var db = new SoneriCISEntities())
            {
                var query = db.tbl_Card_Charges.FirstOrDefault(e => e.ID == RequestID);
                if (query == null)
                    return false;
                else
                {
                    query.AuthorizationStatus = Status;
                    query.AuthorizationComments = Comments;
                    db.SaveChanges();
                }
            }
            return true;
        }

        #endregion

        #region Delete

        public bool DeleteCardCharge(int CardChargeID)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Card_Charges val = new DAL.tbl_Card_Charges();
                    val = db.tbl_Card_Charges.Where(a => a.ID == CardChargeID).FirstOrDefault();
                    val.Title = val.Title;
                    val.Amount = val.Amount;
                    val.AuthorizationComments = val.AuthorizationComments;
                    val.AuthorizationStatus = val.AuthorizationStatus;
                    val.CardTypeID = val.CardTypeID;
                    val.IsActive = false;
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

        #region Select

        public List<tbl_Card_Charges> GetAllCardCharges()
        {
            try
            {
                List<tbl_Card_Charges> lst = new List<DAL.tbl_Card_Charges>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Card_Charges.Where(a => a.IsActive == true).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Card_Charges> GetCardCharges(string AuthorizationStatus)
        {
            try
            {
                List<tbl_Card_Charges> lst = new List<DAL.tbl_Card_Charges>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Card_Charges.Where(a => a.IsActive == true && a.AuthorizationStatus == AuthorizationStatus).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Card_Charges> GetCardChargesActiveInactive(string AuthorizationStatus)
        {
            try
            {
                List<tbl_Card_Charges> lst = new List<DAL.tbl_Card_Charges>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Card_Charges.Where(a => a.AuthorizationStatus == AuthorizationStatus).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Card_Charges> GetCardChargesByCardAccount(int AccountTypeID,int CardTypeID)
        {
            try
            {
                List<tbl_Card_Charges> lst = new List<DAL.tbl_Card_Charges>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Card_Charges.Where(a => a.IsActive == true && a.AccountTypeID==AccountTypeID && a.CardTypeID==CardTypeID).ToList();
                    lst = lst.Where(a => a.AuthorizationStatus == "A").ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Card_Charges GetCardChargeById(int CardChargeID)
        {
            try
            {
                tbl_Card_Charges Val = new DAL.tbl_Card_Charges();
                using (var db = new SoneriCISEntities())
                {
                    Val = db.tbl_Card_Charges.Where(a => a.ID == CardChargeID).FirstOrDefault();
                }
                return Val;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Card_Charges GetCardChargeDetailsById(int CardChargeID)
        {
            try
            {
                tbl_Card_Charges Val = new DAL.tbl_Card_Charges();
                using (var db = new SoneriCISEntities())
                {
                    Val = db.tbl_Card_Charges.Include("tbl_Account_Types").Include("tbl_Card_Types").Where(a => a.ID == CardChargeID).FirstOrDefault();//using "include" because of json searializing issue
                }
                return Val;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Regional_Charges GetRegionalCharges(int CardChargeID,int RegionID)
        {
            try
            {
                tbl_Regional_Charges Val = new DAL.tbl_Regional_Charges();
                using (var db = new SoneriCISEntities())
                {
                    Val = db.tbl_Regional_Charges.Where(a => a.CardChargeID == CardChargeID && a.RegionID==RegionID).FirstOrDefault();
                }
                return Val;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Card_Charges> GetCardCharges(string AccountCode, int CardTypeID, string RegionCode, string Frequency,string IsReplacement)
        {
            List<tbl_Card_Charges> data = new List<tbl_Card_Charges>();
            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("@AccountCode", AccountCode));
            param.Add(new SqlParameter("@CardTypeID", CardTypeID));
            param.Add(new SqlParameter("@RegionCode", RegionCode));
            param.Add(new SqlParameter("@Frequency", Frequency));
            param.Add(new SqlParameter("@IsReplacement", IsReplacement));
            try
            {
                DataTable dt = DatabaseGateway.GetDataUsingStoredProcedure("sp_GetCardCharges", param);
                return ReflectionHelper.CreateGenericListFromDataTable<tbl_Card_Charges>(dt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<tbl_Card_Charges> GetCardChargesByCardNAccount(int AccountTypeID, int CardTypeID)
        {
            try
            {
                List<tbl_Card_Charges> lst = new List<DAL.tbl_Card_Charges>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Card_Charges.Where(a => a.IsActive == true && a.AccountTypeID == AccountTypeID && a.CardTypeID == CardTypeID).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Card_Charges> GetCardChargeByCardTypeAccount(int CardTypeID, int AccountTypeID)
        {
            try
            {
                List<tbl_Card_Charges> lst = new List<DAL.tbl_Card_Charges>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Card_Charges.Where(a => a.IsActive == true && a.AccountTypeID == AccountTypeID && a.CardTypeID == CardTypeID && a.AuthorizationStatus == "A").ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
