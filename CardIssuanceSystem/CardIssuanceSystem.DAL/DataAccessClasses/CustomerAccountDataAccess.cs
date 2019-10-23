using System;
using System.Collections.Generic;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class CustomerAccountDataAccess
    {
        #region Insert
        public bool AddCustomerAccount(tbl_Customer_Accounts row)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Customer_Accounts.Add(row);
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
        public bool UpdateCustomerAccount(tbl_Customer_Accounts row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Customer_Accounts val = new DAL.tbl_Customer_Accounts();
                    val = db.tbl_Customer_Accounts.Where(a => a.ID == row.ID).FirstOrDefault();
                    val.ID = row.ID;
                    val.AccountNo = row.AccountNo;
                    val.CIF = row.CIF;
                    val.CardNo = row.CardNo;
                    val.AccountStatusActive = row.AccountStatusActive;
                    val.Salutation = row.Salutation;
                    val.AccountTitle = row.AccountTitle;
                    val.Address = row.Address;
                    val.Mobile = row.Mobile;
                    val.DateofBirth = row.DateofBirth;
                    val.MotherMaidenName = row.MotherMaidenName;
                    val.Identification = row.Identification;
                    val.CNIC = row.CNIC;
                    val.AddressType = row.AddressType;
                    val.AccountTypeID = row.AccountTypeID;
                    val.WaiveCharges = row.WaiveCharges;
                    val.PassportNo = row.PassportNo;
                    val.LandlineNo = row.LandlineNo;
                    val.Email = row.Email;
                    val.Nationality = row.Nationality;
                    val.AccountCategoryCode = row.AccountCategoryCode;
                    val.PhoneOffice = row.PhoneOffice;
                    val.Company = row.Company;
                    val.IdentificationType = row.IdentificationType;
                    val.Mobile2 = row.Mobile2;
                    val.Address2 = row.Address2;
                    val.Address3 = row.Address3;
                    val.MainMobile = row.MainMobile;
                    val.MainLandline = row.MainLandline;
                    val.MainAddress = row.MainAddress;
                    val.CustomerName = row.CustomerName;
                    val.FatherName = row.FatherName;
                    val.ResidenceStatus = row.ResidenceStatus;
                    val.Currency = row.Currency;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool UpdateCustomerAccountStatus(string AccountNo,string CIF)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Customer_Accounts val = new DAL.tbl_Customer_Accounts();
                    val = db.tbl_Customer_Accounts.Where(a => a.AccountNo == AccountNo && a.CIF==CIF).FirstOrDefault();
                    val.ID = val.ID;
                    val.AccountNo = val.AccountNo;
                    val.CIF = val.CIF;
                    val.CardNo = null;
                    val.AccountStatusActive = val.AccountStatusActive;
                    val.Salutation = val.Salutation;
                    val.AccountTitle = val.AccountTitle;
                    val.Address = val.Address;
                    val.Mobile = val.Mobile;
                    val.DateofBirth = val.DateofBirth;
                    val.MotherMaidenName = val.MotherMaidenName;
                    val.Identification = val.Identification;
                    val.CNIC = val.CNIC;
                    val.AddressType = val.AddressType;
                    val.AccountTypeID = val.AccountTypeID;
                    val.WaiveCharges = val.WaiveCharges;
                    val.PassportNo = val.PassportNo;
                    val.LandlineNo = val.LandlineNo;
                    val.Email = val.Email;
                    val.Nationality = val.Nationality;
                    val.AccountCategoryCode = val.AccountCategoryCode;
                    val.PhoneOffice = val.PhoneOffice;
                    val.Company = val.Company;
                    val.IdentificationType = val.IdentificationType;
                    val.Mobile2 = val.Mobile2;
                    val.Address2 = val.Address2;
                    val.Address3 = val.Address3;
                    val.MainMobile = val.MainMobile;
                    val.MainLandline = val.MainLandline;
                    val.MainAddress = val.MainAddress;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool UpdateCustomerAccountCardNo(string AccountNo, string CIF, string CardNo)
        {
            var success = false;
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    var query = db.tbl_Customer_Accounts.FirstOrDefault(e => e.AccountNo.Contains(AccountNo) && e.CIF.Contains(CIF));

                    if (query != null)
                    {
                        query.CardNo = CardNo;
                        db.SaveChanges();
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return success;
        }
        #endregion

            #region Delete
        public bool DeleteCustomerAccount(string AccountNo)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    List<tbl_Customer_Accounts> lstAccounts = new List<DAL.tbl_Customer_Accounts>();
                    lstAccounts = db.tbl_Customer_Accounts.Where(a => a.AccountNo == AccountNo).ToList();
                    db.tbl_Customer_Accounts.RemoveRange(lstAccounts);
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
    
        public List<tbl_Customer_Accounts> GetAllCustomerAccounts()
        {
            try
            {
                List<tbl_Customer_Accounts> lst = new List<DAL.tbl_Customer_Accounts>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Customer_Accounts.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Customer_Accounts> GetCustomerAccount(string AccountNo)
        {
            try
            {
                List<tbl_Customer_Accounts> lst = new List<tbl_Customer_Accounts>();

                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Customer_Accounts.Where(a=>a.AccountNo==AccountNo).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       



        public List<tbl_Customer_Accounts> GetCustomerAccount(string AccountNo,string CIF,string CardNo)
        {
            try
            {
                List<tbl_Customer_Accounts> lst = new List<tbl_Customer_Accounts>();

                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Customer_Accounts.Where(a => a.AccountNo.Contains(AccountNo) && a.CIF==CIF && a.CardNo==CardNo).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<tbl_Customer_Accounts> GetCustomerAccountbyCIF(string AccountNo, string CIF)
        {
            try
            {
                List<tbl_Customer_Accounts> lst = new List<tbl_Customer_Accounts>();

                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Customer_Accounts.Where(a => a.AccountNo == AccountNo && a.CIF == CIF).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Customer_Accounts> GetCustomerAccountByCardNo(string CardNo)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_Customer_Accounts.Where(e => e.CardNo == CardNo).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
    #endregion
}
