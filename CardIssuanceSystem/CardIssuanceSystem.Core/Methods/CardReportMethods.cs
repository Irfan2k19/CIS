using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.Methods
{
    public class CardReportMethods
    {
        public static List<CustomerAccountVM> GetCustomerAccount(string CardNo)
        {
            try
            {
                List<CustomerAccountVM> lst = new List<CustomerAccountVM>();

                using (var db = new SoneriCISEntities())
                {
                    lst =(from cc in db.tbl_Customer_Cards join ca in db.tbl_Customer_Accounts on
                          cc.AccountNo equals ca.AccountNo 
                          where cc.CardNo== CardNo && cc.CIF==ca.CIF
                          select new CustomerAccountVM
                          {
                            AccountNo=ca.AccountNo,
                            CIF =ca.CIF,
                            AccountStatusActive = ca.AccountStatusActive,

                            AccountTitle = ca.AccountTitle,
                            Address = ca.Address,
                            Mobile = ca.Mobile,
                            DOB = ca.DateofBirth.ToString(),
                            MotherMaidenName = ca.MotherMaidenName,
                            Identification = ca.Identification,
                            CNIC = ca.CNIC,
                            AddressType = ca.AddressType,
                            AccountTypeID = ca.AccountTypeID,
                            PassportNo = ca.PassportNo,
                            LandlineNo = ca.LandlineNo,
                            Email = ca.Email,
                            Nationality = ca.Nationality,
                            AccountCategoryCode = ca.AccountCategoryCode,
                            PhoneOffice = ca.PhoneOffice,
                            Company = ca.Company,
                            IdentificationType = ca.IdentificationType,
                            Mobile2 = ca.Mobile2,
                            Address2 = ca.Address2,
                            Address3 = ca.Address3,
                            MainMobile = ca.MainMobile,
                            MainLandline = ca.MainLandline,
                            MainAddress = ca.MainAddress,
                            CustomerName = ca.CustomerName,
                            FatherName = ca.FatherName,
                            ResidenceStatus = ca.ResidenceStatus,
                            Currency = ca.Currency,

                            CardNo = cc.CardNo,
                            CardStatusActive = cc.CardStatusActive,
                            CardTypeID = cc.CardTypeID,
                            //AccountNo = cc.AccountNo,
                              
                            Salutation = cc.Salutation,
                            CardTitle = cc.CardTitle,
                            CardExpiryStr = cc.CardExpiry.ToString(),
                            CardIssuanceStr = cc.CardIssuance.ToString()
                          }).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
