using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class AccountTypeDataAccess
    {

        #region Insert
        public bool AddAccountType(tbl_Account_Types row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    if (db.tbl_Account_Types.Where(e => e.Name.ToLower() == row.Name.ToLower()).Any())
                        throw new CustomException("Account type name already exists!");

                    db.tbl_Account_Types.Add(row);
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
        public bool UpdateAccountType(tbl_Account_Types row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Account_Types val = new DAL.tbl_Account_Types();
                    val = db.tbl_Account_Types.Where(a => a.ID == row.ID).FirstOrDefault();
                    if (db.tbl_Account_Types.Where(e => e.Name.ToLower() != val.Name.ToLower() && e.Name.ToLower() == row.Name.ToLower()).Any())
                        throw new CustomException("Account type name already exists!");

                    val.Name = row.Name;
                    val.Code = row.Code;
                    val.AuthorizationComments = row.AuthorizationComments;
                    val.AuthorizationStatus = row.AuthorizationStatus;
                    val.Description = row.Description;
                    val.IsActive = row.IsActive;
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
                var query = db.tbl_Account_Types.FirstOrDefault(e => e.ID == RequestID);
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

        public bool DeleteAccountType(int AccountTypeID)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Account_Types val = new DAL.tbl_Account_Types();
                    val = db.tbl_Account_Types.Where(a => a.ID == AccountTypeID).FirstOrDefault();
                    val.Name = val.Name;
                    val.Code = val.Code;
                    val.AuthorizationComments = val.AuthorizationComments;
                    val.AuthorizationStatus = val.AuthorizationStatus;
                    val.Description = val.Description;
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

        public List<tbl_Account_Types> GetAllAccountTypes()
        {
            try
            {
                List<tbl_Account_Types> lst = new List<DAL.tbl_Account_Types>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Account_Types.Where(a => a.IsActive == true).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Account_Types> GetAccountTypes(string AuthorizationStatus)
        {
            try
            {
                List<tbl_Account_Types> lst = new List<DAL.tbl_Account_Types>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Account_Types.Where(a => a.IsActive == true && a.AuthorizationStatus == AuthorizationStatus).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<tbl_Account_Types> GetAccountTypesActiveInActive(string AuthorizationStatus)
        {
            try
            {
                List<tbl_Account_Types> lst = new List<DAL.tbl_Account_Types>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Account_Types.Where(a => a.AuthorizationStatus == AuthorizationStatus).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Account_Types GetAccountTypeById(int AccountTypeID)
        {
            try
            {
                tbl_Account_Types Val = new DAL.tbl_Account_Types();
                using (var db = new SoneriCISEntities())
                {
                    Val = db.tbl_Account_Types.Where(a => a.ID == AccountTypeID).FirstOrDefault();
                }
                return Val;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public tbl_Account_Types GetAccountTypeDetailsById(int AccountTypeID)
        {
            try
            {
                tbl_Account_Types Val = new DAL.tbl_Account_Types();
                using (var db = new SoneriCISEntities())
                {
                    Val = db.tbl_Account_Types.Include("tbl_Customer_Accounts").Include("tbl_Card_Charges").Where(a => a.ID == AccountTypeID).FirstOrDefault();
                }
                return Val;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public tbl_Account_Types GetAccountTypeByCode(string AccountCode)
        {
            try
            {
                tbl_Account_Types Val = new DAL.tbl_Account_Types();
                using (var db = new SoneriCISEntities())
                {
                    Val = db.tbl_Account_Types.Where(a => a.Code == AccountCode).FirstOrDefault();
                }
                return Val;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckAccountTypeEligibility(string AccountCode, int CardTypeID)
        {
            try
            {
                tbl_Card_Charges Val = new DAL.tbl_Card_Charges();
                using (var db = new SoneriCISEntities())
                {
                    var aa = from cc in db.tbl_Card_Charges
                             join act in db.tbl_Account_Types on cc.AccountTypeID equals act.ID
                             where cc.IsActive == true && act.IsActive == true && act.Code == AccountCode
                             && cc.CardTypeID == CardTypeID
                             select cc;
                    Val = aa.FirstOrDefault();
                }
                if (Val != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int GetAccountTypeID(string AccountCode)
        {
            try
            {
               int AccountTypeID = 0;
               using (var db = new SoneriCISEntities())
                {
                    AccountTypeID = db.tbl_Account_Types.Where(a => a.Code == AccountCode).FirstOrDefault()?.ID ?? 0;
                }
                return AccountTypeID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool CheckDuplicateCode(string AccountCode)
        {
            try
            {
                tbl_Account_Types card = new DAL.tbl_Account_Types();
                using (var db = new SoneriCISEntities())
                {
                    card = db.tbl_Account_Types.Where(a => a.IsActive == true && a.Code == AccountCode).FirstOrDefault();
                }
                if (card == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

     
        #endregion
    }
}
