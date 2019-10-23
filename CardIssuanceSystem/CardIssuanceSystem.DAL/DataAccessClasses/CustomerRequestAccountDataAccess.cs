using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class CustomerRequestAccountDataAccess
    {

        #region Insert
        public bool AddRequestCustomerAccountData(tbl_Request_Customer_Accounts row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Request_Customer_Accounts.Add(row);
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
        public bool UpdateCustomerRequestAccount(tbl_Request_Customer_Accounts row)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Request_Customer_Accounts val = new DAL.tbl_Request_Customer_Accounts();
                    val = db.tbl_Request_Customer_Accounts.Where(a => a.ID == row.ID).FirstOrDefault();
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
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool UpdateCustomerRequestAccount(int ID,string Address,string Mobile,string Landline)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Request_Customer_Accounts val = new DAL.tbl_Request_Customer_Accounts();
                    val = db.tbl_Request_Customer_Accounts.Where(a => a.ID == ID).FirstOrDefault();
                    val.ID = val.ID;
                    val.AccountNo = val.AccountNo;
                    val.CIF = val.CIF;
                    val.CardNo = val.CardNo;
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
                    val.MainMobile = Mobile;
                    val.MainLandline = Landline;
                    val.MainAddress = Address;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool UpdateCustomerRequestAccountOnAuthorization(int RequestID, string Address, string Mobile, string Landline,string OfficeNo)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Request_Customer_Accounts val = new DAL.tbl_Request_Customer_Accounts();
                    val = db.tbl_Request_Customer_Accounts.Where(a => a.RequestID==RequestID).OrderByDescending(a=>a.ID).FirstOrDefault();
                    val.Mobile2 = Mobile;
                    val.MainLandline = Landline;
                    val.Address2 = Address;
                    val.Company = OfficeNo;
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

        public List<tbl_Request_Customer_Accounts> GetRequestCustomerAccounts()
        {
            try
            {
                List<tbl_Request_Customer_Accounts> lst = new List<DAL.tbl_Request_Customer_Accounts>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Request_Customer_Accounts.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
      
        public tbl_Request_Customer_Accounts GetRequestCustomerAccount(int ID)
        {
            try
            {
                tbl_Request_Customer_Accounts imports = new DAL.tbl_Request_Customer_Accounts();
                using (var db = new SoneriCISEntities())
                {
                    imports = db.tbl_Request_Customer_Accounts.Where(a => a.ID == ID).FirstOrDefault();
                }
                return imports;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public tbl_Request_Customer_Accounts GetRequestCustomerAccount(int RequestID,string AccountNo)
        {
            try
            {
                tbl_Request_Customer_Accounts imports = new DAL.tbl_Request_Customer_Accounts();
                using (var db = new SoneriCISEntities())
                {
                    imports = db.tbl_Request_Customer_Accounts.Where(a => a.RequestID == RequestID && a.AccountNo==AccountNo).FirstOrDefault();
                }
                return imports;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Request_Customer_Accounts GetRequestCustomerAccountByRequestId(int RequestID)
        {
            try
            {
                tbl_Request_Customer_Accounts imports = new DAL.tbl_Request_Customer_Accounts();
                using (var db = new SoneriCISEntities())
                {
                    imports = db.tbl_Request_Customer_Accounts.Where(a => a.RequestID == RequestID).FirstOrDefault();
                }
                return imports;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
