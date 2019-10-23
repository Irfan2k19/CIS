using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardIssuanceSystem.DAL;
using System.Data.SqlClient;
using System.Data;

namespace CardIssuanceSystem.Core.Methods
{
    public class CommonMethods
    {
        public static List<CustomerAccountVM> GetAccountInfo(string CardNo,string AccountNo)
        {
            List<CustomerAccountVM> lst = new List<CustomerAccountVM>();
            var db = new SoneriCISEntities();
            lst = (from a in db.tbl_Customer_Accounts.Where(a => a.CardNo == CardNo && a.AccountStatusActive==true && a.AccountNo!=AccountNo)
                   select new CustomerAccountVM
                   {
                       ID = a.ID,
                       AccountNo = a.AccountNo,
                       CIF = a.CIF,
                       CardNo = a.CardNo,
                       AccountStatusActive = a.AccountStatusActive,
                       Salutation = a.Salutation,
                       AccountTitle = a.AccountTitle,
                       Address = a.Address,
                       Mobile = a.Mobile,
                       DateofBirth = (a.DateofBirth),
                       MotherMaidenName = a.MotherMaidenName,
                       Identification = a.Identification,
                       CNIC = a.CNIC,
                       AddressType = a.AddressType,
                       AccountTypeID = a.AccountTypeID,
                       WaiveCharges = a.WaiveCharges

                   }).ToList();
            return lst;
        }

        public static List<CustomerCardVM> GetCardInfo(string CardNo)
        {
            List<CustomerCardVM> lst = new List<CustomerCardVM>();
            var db = new SoneriCISEntities();
            try
            {
            
            lst = (from a in db.tbl_Customer_Cards join b in db.tbl_Customer_Accounts on a.AccountNo equals b.AccountNo
                   where a.CardNo==CardNo && a.CIF == b.CIF
                   select new CustomerCardVM
                   {
                       
                    ID = a.ID,
                    CardNo = a.CardNo,
                    CardStatusActive=a.CardStatusActive,
                    CardTypeID=a.CardTypeID,
                    AccountNo=a.AccountNo,
                    WaiveCharges=a.WaiveCharges,
                    Salutation=a.Salutation,
                    CardTitle=a.CardTitle,
                    CardExpiry=a.CardExpiry,
                    CardIssuance=a.CardIssuance,
                    CIFNo=b.CIF

                   }).ToList();
            return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<CustomerCardVM> GetLstCardByCardNo(string CardNo)
        {
            List<CustomerCardVM> lst = new List<CustomerCardVM>();
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    lst = (from a in db.tbl_Customer_Cards
                           where a.CardNo == CardNo
                           select new CustomerCardVM
                           {
                               ID = a.ID,
                               CardNo = a.CardNo,
                               CardStatusActive = a.CardStatusActive,
                               CardTypeID = a.CardTypeID,
                               AccountNo = a.AccountNo,
                               WaiveCharges = a.WaiveCharges,
                               Salutation = a.Salutation,
                               CardTitle = a.CardTitle,
                               CardExpiry = a.CardExpiry,
                               CardIssuance = a.CardIssuance,
                               CIFNo = a.CIF
                           }).ToList();
                }

                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<CustomerCardVM> GetCardInfo(string CardNo, DateTime expiry)
        {
            List<CustomerCardVM> lst = new List<CustomerCardVM>();
            var db = new SoneriCISEntities();
            try
            {

                lst = (from a in db.tbl_Customer_Cards
                       join b in db.tbl_Customer_Accounts on a.AccountNo equals b.AccountNo
                       where a.CardNo == CardNo && a.CIF == b.CIF && a.CardExpiry == expiry && a.CardStatusActive == true
                       select new CustomerCardVM
                       {

                           ID = a.ID,
                           CardNo = a.CardNo,
                           CardStatusActive = a.CardStatusActive,
                           CardTypeID = a.CardTypeID,
                           AccountNo = a.AccountNo,
                           WaiveCharges = a.WaiveCharges,
                           Salutation = a.Salutation,
                           CardTitle = a.CardTitle,
                           CardExpiry = a.CardExpiry,
                           CardIssuance = a.CardIssuance,
                           CIFNo = b.CIF
                       }).ToList();

                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool SaveAccountInfo(CustomerAccountVM customer)
        {
            bool flag=false;
           
            tbl_Customer_Accounts cust = new tbl_Customer_Accounts();
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    cust.AccountNo = customer.AccountNo;
                    cust.CIF = customer.CIF;
                    cust.CardNo = customer.CardNo;
                    cust.AccountStatusActive = customer.AccountStatusActive;
                    cust.AccountTitle = customer.AccountTitle;
                    cust.Address = customer.Address;
                    cust.Mobile = customer.Mobile;
                    cust.CNIC = customer.CNIC;
                    db.tbl_Customer_Accounts.Add(cust);
                    db.SaveChanges();
                    flag = true;
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return flag;
        }

        public static string CheckStatusEligibility(string AccountStatus)
        {
            string success= CustomMessages.InActiveAccount;
            if (AccountStatus!= "Y")
            {
                success = CustomMessages.Success;
            }
            return success;
        }

        public static bool CheckAccountEligibility(string val1,string val2,string val3)
        {
            bool success = true;
            if (val1 == "0001" || val1=="abc")
            {
                success = false;
            }
            if (val2 == "0001" || val2=="")
            {
                success = false;
            }
            if (val3 == "0001" || val3 == "")
            {
                success = false;
            }

            return true;
        }

        public static bool CheckBalanceEligibility(decimal AvailBalance,double? TotalAmount)
        {
            bool success = false;
            if (AvailBalance > Convert.ToDecimal(TotalAmount ?? 0))
            {
                success = true;
            }
            return success;
        }

        public static object T24TestObject(string AccountNo)
        {
            var AccountInfo= new
            {
                ACALTID5 = "",
                ACALTID6 = "",
                AccountAddress = "",
                AccountBranchCode = "",
                AccountCategory = "",
                AccountCurrency = "",
                AccountID = "",
                AccountMobile = "",
                AccountNature = "",
                AccountNumber = "",
                AccountOpeningDate = "",
                AccountPhone = "",
                AccountRestraints = "[]",
                AccountStatus = "",
                AccountTitle = "",
                AvailableBalance = 0,
                BranchCode = "",
                CategoryDesc = "",
                CompName = "",
                CustomerID = "",
                IdentificationNo = "",
                LegacyAcNo = "",
                OffPhone = "",
                OrganizationCode = "",
                ProductCode = "",
                ResponseCode = "",
                ResponseCodeDescription = "",
                SDNAccountNo = "",
                SDNIBAN = "",
                SchemeCode = "",
                SectorDesc = "",
                T24ACNO = "",
                T24IBAN = ""
            };
            if (AccountNo == "20000024679")
            { 
            AccountInfo = new {
                                    ACALTID5 = "001520000024679",
                                    ACALTID6 = "",
                                    AccountAddress = "NEWS HD 47 N GULBERG II",
                                    AccountBranchCode = "0015",
                                    AccountCategory = "",
                                    AccountCurrency = "PKR",
                                    AccountID = "20000024679\'COK",
                                    AccountMobile = "",
                                    AccountNature ="1001",
                                    AccountNumber = "",
                                    AccountOpeningDate = "/Date(1518375600000)/",
                                    AccountPhone = "",
                                    AccountRestraints = "[]",
                                    AccountStatus = "0001",
                                    AccountTitle = "MUBASHER LUCMAN",
                                    AvailableBalance = 1023,
                                    BranchCode = "",
                                    CategoryDesc = "Current Deposit Account Customer",
                                    CompName = "Gulberg Br. LHR-0015",
                                    CustomerID = "1000109118",
                                    IdentificationNo = "3520156521059",
                                    LegacyAcNo = "",
                                    OffPhone = "042357642524",
                                    OrganizationCode = "",
                                    ProductCode = "",
                                    ResponseCode = "0000",
                                    ResponseCodeDescription ="Success",
                                    SDNAccountNo = "001502013091429",
                                    SDNIBAN = "PK34SONE0001502013091429",
                                    SchemeCode = "",
                                    SectorDesc = "Individual Particuliers",
                                    T24ACNO = "",
                                    T24IBAN ="PK46SONE0000020000024679"
                                    };
            }
            return AccountInfo;
        }

        public static string GenerateSTAN()
        {
            try
            {
                string STAN = "CIF";
                string DT = DateTime.Now.ToString("yyyyMM");
                string T = DateTime.Now.ToString("HHmmSS");
                DT = DT.Remove(0, 2);
                Random r = new Random();
                DT = DT+r.Next(0, 9).ToString() + r.Next(0, 9).ToString() + r.Next(0, 9).ToString();
                DT = DT + r.Next(0, 9).ToString() + r.Next(0, 9).ToString();
                STAN += DT;
                return STAN;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static String GetRandomString()
        {
            var allowedChars = "abcdefghijkmnopqrstuvwxyz0123456789";
            var length = 9;

            var chars = new char[length];
            var rd = new Random();

            for (var i = 0; i < length; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new String(chars);
        }


        public static string GetRequestSTAN(string RequestId, int LstCount,string RequestType)
        {
            string STAN;
           
           STAN =RequestId + (LstCount).ToString();
            
            
            if(STAN.Length < 8)
            {
              STAN=STAN.PadLeft(8,'0');
        
               
            }

            
                STAN = RequestType + STAN;
            STAN = STAN.Length <= 9 ? STAN : STAN.Substring(0, 9);
            
            return STAN;
        }
        public static string GetRecoverySTAN(string RequestId, string TransType)
        {
            string STAN;
            string[] MonthArray = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l" };
            string[] DaysArray = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "1", "2", "3", "4", "5" };
            string Day = DateTime.Now.Day.ToString();
            string Month = DateTime.Now.Month.ToString();
            string Year = DateTime.Now.Year.ToString();
            Year = Year.Substring(2, 2);
            int d = Convert.ToInt32(Day) - 1;
            int m = Convert.ToInt32(Month) - 1;
            Day = DaysArray[d];
            Month = MonthArray[m];

            //STAN = RequestId + (LstCount).ToString();


            int x = Int32.Parse(RequestId);
            string RequestIdHex = x.ToString("X");
            if (RequestIdHex.Length < 6)
            {
                RequestIdHex = RequestIdHex.PadLeft(6, 'Z');


            }
            STAN = Year + Month + Day + RequestIdHex + TransType;
            STAN = "CIF" + STAN;




            // STAN = STAN.Length <= 9 ? STAN : STAN.Substring(0, 9);

            return STAN;
        }

        public static string GetUniqueSTAN(string RequestId, int LstCount,string RequestType)
        {
            try
            {
                DAL.DataAccessClasses.TransactionDataAccess tda = new DAL.DataAccessClasses.TransactionDataAccess();
                //string STAN = GenerateSTAN();
                //string STAN = GetRandomString();
                string STAN=GetRequestSTAN(RequestId,LstCount,RequestType);
                bool IsUnique = tda.CheckUniqueSTAN(STAN);

                while (IsUnique == false)
                {
                    STAN = GetRequestSTAN(RequestId, LstCount, RequestType);
                    IsUnique = tda.CheckUniqueSTAN(STAN);
                }
                STAN = "CIF" + STAN;
                
                return STAN;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetUniqueSTAN()
        {
            try
            {
                DAL.DataAccessClasses.TransactionDataAccess tda = new DAL.DataAccessClasses.TransactionDataAccess();
                //string STAN = GenerateSTAN();
                string STAN = GetRandomString();
              
                bool IsUnique = tda.CheckUniqueSTAN(STAN);

                while (IsUnique == false)
                {
                    STAN = GetRandomString();
                    IsUnique = tda.CheckUniqueSTAN(STAN);
                }
                STAN = "CIF" + STAN;
                return STAN;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<CustomerCardVM> GetCustomerCardByAccountNo(string AccountNo)
        {
            try
            {
                List<CustomerCardVM> card = new List<CustomerCardVM>();

                using (var db = new SoneriCISEntities())
                {
                    
                    card = (from cc in db.tbl_Customer_Cards
                            join ca in db.tbl_Customer_Accounts on
                            cc.AccountNo equals ca.AccountNo
                            //where cc.CardStatusActive == true && cc.AccountNo==AccountNo
                            where cc.AccountNo == AccountNo
                            select new CustomerCardVM
                            {
                                CardNo = cc.CardNo,
                                AccountNo=cc.AccountNo,
                                CIFNo=cc.CIF,
                                CardStatusActive=cc.CardStatusActive
                                

                            }).ToList();
                }

                return card;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<CustomerCardVM> GetCustomerCardsByAccountNo(string AccountNo)
        {
            try
            {
                List<CustomerCardVM> card = new List<CustomerCardVM>();
                using (var db = new SoneriCISEntities())
                {
                    card = (from cc in db.tbl_Customer_Cards
                            where cc.AccountNo == AccountNo
                            select new CustomerCardVM
                            {
                                CardNo = cc.CardNo,
                                AccountNo = cc.AccountNo,
                                CIFNo = cc.CIF,
                                CardStatusActive = cc.CardStatusActive
                            }).ToList();
                }

                return card;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<CustomerCardVM> GetCustomerCardByAccountNoCIF(string AccountNo,string CIF)
        {
            try
            {
                List<CustomerCardVM> card = new List<CustomerCardVM>();

                using (var db = new SoneriCISEntities())
                {

                    card = (from cc in db.tbl_Customer_Cards
                            join ca in db.tbl_Customer_Accounts on
                            cc.AccountNo equals ca.AccountNo
                            //where cc.CardStatusActive == true && cc.AccountNo==AccountNo
                            where cc.AccountNo == AccountNo && cc.CIF==CIF
                            select new CustomerCardVM
                            {
                                CardNo = cc.CardNo,
                                AccountNo = cc.AccountNo,
                                CIFNo = ca.CIF,
                                CardStatusActive = cc.CardStatusActive


                            }).ToList();
                }

                return card;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetCardsInfoforRequest(string AccountNo,string CIF,string RequestType)
        {
            try
            {
                string IsRequestEligible = "true";
                List<CustomerCardVM> card = new List<CustomerCardVM>();
                using (var db = new SoneriCISEntities())
                {
                    card = (from cc in db.tbl_Customer_Cards
                            where cc.AccountNo == AccountNo && cc.CIF==CIF
                            select new CustomerCardVM
                            {
                                CardNo = cc.CardNo,
                                AccountNo = cc.AccountNo,
                                CIFNo = cc.CIF,
                                CardStatusActive = cc.CardStatusActive
                            }).ToList();
                }

                if (RequestType=="N")
                {
                    if (card.Count > 0 && card.Any(x => x.CardStatusActive == true))
                    {
                        IsRequestEligible = CustomMessages.CardAlreadyExists;
                    }
                    else if(card.Count > 0 && card.Any(x => x.CardStatusActive == false))
                    {
                        IsRequestEligible = CustomMessages.HotCardExists;
                    }
                    
                }

                if (RequestType == "R")
                {
                    if (card.Count > 0 && card.Any(x => x.CardStatusActive == true))
                    {
                        IsRequestEligible = CustomMessages.CardAlreadyExists;
                    }
                    else if (card.Count > 0 && card.Any(x => x.CardStatusActive == false))
                    {
                        IsRequestEligible = "true";
                    }

                }
              
                

                return IsRequestEligible;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<string> IsEligible(EligibilityVM elg)
        {
            //bool success = false;
            List<string> lst = new List<string>();
            var Residency = ResidencyEligibility(elg.IdentificationType, elg.IdentificationNumber, elg.Resident, elg.Nationality);
            var Sector = SectorEligibility(elg.SectorCode);
            var Status = CheckStatusEligibility(elg.AccountStatus);
            var Field  = FieldEligibility(elg.MotherName, elg.IdentificationType, elg.IdentificationNumber, elg.DOB);
            var Posting = PostingRestrictionEligibility(elg.PostingRestriction);
            var OpInstruction = OperatingInstructionEligibility(elg.OpInstruction,elg.AccountNo);

            if (Residency != CustomMessages.Success)
            {
                lst.Add(Residency);
            }
            if (Sector !=CustomMessages.Success)
            {
                lst.Add(Sector); 
            }
            if (Status != CustomMessages.Success)
            {
                lst.Add(Status);
            }
            if (!Field.Contains(CustomMessages.Success))
            {
                foreach(var item in Field)
                {
                    lst.Add(item);
                }
                
            }
            if (Posting != CustomMessages.Success)
            {
                lst.Add(Posting); 
            }
            if (OpInstruction != CustomMessages.Success)
            {
                lst.Add(OpInstruction); 
            }
            return lst;
        }

        #region Residency
        public static string ResidencyEligibility(string IdentificationType, string IdentificationNo,string Resident,string Nationality)
        {
        string flag="";
        if (Resident.ToUpper() == "PK")
        { 
            if (Nationality.ToUpper() == "PK")
            {
                if (IdentificationType.ToUpper().Contains("CNIC") || IdentificationType.ToUpper().Contains("SNIC") ||IdentificationType.ToUpper().Contains("NICOP"))
                {
                    if (!string.IsNullOrWhiteSpace(IdentificationNo) && IdentificationNo != "0" && IdentificationNo != "undefined")
                    {
                        flag = CustomMessages.Success;
                    }
                    else
                    {
                        flag = CustomMessages.InvalidIdentificationNumber;
                    }
                }
                else
                {
                    flag = CustomMessages.InvalidIdentification;
                }

            }
            else
            {
                if (IdentificationType.ToUpper().Contains("PASSPORT") || IdentificationType.ToUpper().Contains("POC") || IdentificationType.ToUpper().Contains("POR"))
                {
                    if (!string.IsNullOrEmpty(IdentificationNo) && IdentificationNo != "0" && IdentificationNo != "undefined")
                    {
                        flag = CustomMessages.Success;
                    }
                    else
                    {
                        flag = CustomMessages.InvalidIdentificationNumber;
                    }
                    
                }
                else
                {
                    flag = CustomMessages.InvalidIdentification;
                }
            }
        }else
        {
                flag = CustomMessages.NonResident;
        }
            return flag;
        }
        #endregion
        #region Sector
        public static string SectorEligibility(string SectorCode)
        {
            string flag = "";

            DAL.DataAccessClasses.SectorDataAccess sda = new DAL.DataAccessClasses.SectorDataAccess();
            if (sda.CheckSector(SectorCode))
            {
                flag = CustomMessages.Success;
            }else
            {
                flag = CustomMessages.SectorInEligible;
            }
            return flag;
        }
        #endregion
        #region Field Eligibility
        public static List<string> FieldEligibility(string MotherName,string IdentificationType,string IdentificationNumber,string DOB)
        {
            string success="";
            List<string> lst = new List<string>();
            if (string.IsNullOrWhiteSpace(MotherName))
            {
                success = CustomMessages.MotherNameError;
                lst.Add(success);
            }
            if(string.IsNullOrWhiteSpace(IdentificationType)) 
            {
                success = CustomMessages.IdentificationTypeError;
                lst.Add(success);
            }
            if (string.IsNullOrWhiteSpace(IdentificationNumber))
            {
                success = CustomMessages.IdentificationNumberError;
                lst.Add(success);
            }
            if (string.IsNullOrWhiteSpace(DOB))
            {
                success = CustomMessages.DOBError;
                lst.Add(success);
            }
            if (!string.IsNullOrWhiteSpace(MotherName) && !string.IsNullOrWhiteSpace(IdentificationType) && !string.IsNullOrWhiteSpace(IdentificationNumber) && !string.IsNullOrWhiteSpace(DOB))
            {
                success = CustomMessages.Success;
                lst.Add(success);
            }
           
            return lst;
        }
        #endregion
        #region Posting Restriction
        public static string PostingRestrictionEligibility(string PostingRestriction)
        {

            string flag = "";
            List<string> lstCodes = null;
            DAL.DataAccessClasses.PostingRestrictionDataAccess sda = new DAL.DataAccessClasses.PostingRestrictionDataAccess();
            if (!string.IsNullOrEmpty(PostingRestriction))
            {
                lstCodes = PostingRestriction.Split(' ').Where(e => !string.IsNullOrEmpty(e)).Select(e => e.ToUpper()).ToList();
                if (sda.CheckPostingCode(lstCodes))
                {
                    flag = CustomMessages.PostingRestriction;
                }
                else
                {
                    flag = CustomMessages.Success;
                }
            }
            else
            {
                flag = CustomMessages.Success;
            }

            return flag;
        }
        #endregion

        #region Operating Instruction
        public static string OperatingInstructionEligibility(string OpInstruction, string AccountNo)
        {

            string flag = CustomMessages.Success;
            if (OpInstruction == null || OpInstruction == "")
            {
                //flag = CustomMessages.OperatingInstruction;
            }
            else
            {
                DAL.DataAccessClasses.OperatingInstructionDataAccess sda = new DAL.DataAccessClasses.OperatingInstructionDataAccess();
                if (sda.CheckPostingCode(OpInstruction))
                {
                    flag = CustomMessages.OperatingInstruction;
                }
            }
        
            return flag;
        }
        #endregion



        #region Is Primary Card Exists
        public static bool IsPrimaryCardExist(string AccountNo,string CIF)
        {
            var db = new SoneriCISEntities();
            bool flag = true;
            try
            {
                var result =db.tbl_Customer_Accounts.Where(x=> x.AccountNo==AccountNo && x.CIF==CIF).Select(c=>c.CardNo).FirstOrDefault();

                if (result==null || result =="")
                {
                    flag = false;
                }

                return flag;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Card Exist for Delinking
        public static bool CardExistforDelinking(string AccountNo,string CIF)
        {
            try
            {
                var db = new SoneriCISEntities();
                var flag = true;

                var result = db.tbl_Customer_Cards.Where(x => x.AccountNo == AccountNo && x.CIF == CIF).Select(c => c.CardNo).FirstOrDefault();
                if (result != null)
                {
                    flag = false;
                }

                return flag;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        #region New Linking Scenario 3 Dec 2018
        public static List<RequestVM> GetLinkedAccounts(string CardNo,string AccountNo)
        {
            List<RequestVM> result = new List<RequestVM>();
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@CardNo", CardNo));
                parameters.Add(new SqlParameter("@AccountNo", AccountNo));

                var data = DatabaseGateway.GetDataUsingStoredProcedure("sp_LinkedAccounts", parameters);
                result = data.Rows.Count > 0 ? DAL.ReflectionHelper.CreateGenericListFromDataTable<RequestVM>(data) : null;
                return result;
                
            }
            catch(Exception ec)
            {
                throw ec;
            }
        }
        #endregion
    }
}
