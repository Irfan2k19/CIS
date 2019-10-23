using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL;
using CardIssuanceSystem.DAL.DataAccessClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CardIssuanceSystem.Core.Methods
{
    public class RequestMethods
    {

        #region Card Issuance
        public static long? AddNewRequest(RequestVM model)
        {
            long? Result = 0;
            model.RequestDate = DateTime.Now;
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Requests request = new tbl_Requests();
                    request.RequestType = model.RequestType;
                    request.AccountNo = model.AccountNo;
                    request.CIFNo = model.CIFNo;
                    request.WaiveCharges = model.Waive;
                    request.StatusEligibility = model.StatusEligibility;
                    request.FinancialEligibility = model.FinancialEligibility;
                    request.AuthorizationStatus = model.AuthorizationStatus;
                    request.RequestDate = model.RequestDate;
                    request.CardTitle = model.CardTitle;
                    request.CardTypeID = model.CardTypeId;
                    request.Salutation = model.Salutation;
                    request.IsActive = true;
                    request.CardNo = model.CardNumber;
                    request.IsExported = false;
                    request.CreatorID = StateHelper.UserId;
                    db.tbl_Requests.Add(request);
                    db.SaveChanges();

                    model.ID = request.ID;
                    //AddRequestCustomerAccount(model);
                }
                Result = model.ID;
            }
            catch (Exception ex)
            {
                
            }

            return Result;
        }
        #endregion

        #region Card Replacement
        public static long? AddReplacementRequest(RequestVM model)
        {
            long? Result = 0;
            model.RequestDate = DateTime.Now;
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Requests request = new tbl_Requests();
                    request.RequestType = model.RequestType;
                    request.AccountNo = model.AccountNo;
                    request.CIFNo = model.CustomerId;
                    request.WaiveCharges = model.Waive;
                    request.StatusEligibility = model.StatusEligibility;
                    request.FinancialEligibility = model.FinancialEligibility;
                    request.AuthorizationStatus = model.AuthorizationStatus;
                    request.RequestDate = model.RequestDate;
                    request.CardTitle = model.CardTitle;
                    request.CardTypeID = model.CardTypeId;
                    request.Salutation = model.Salutation;
                    request.IsActive = true;
                    request.IsExported = false;
                    request.CreatorID = StateHelper.UserId;
                    db.tbl_Requests.Add(request);
                    db.SaveChanges();
                    model.ID = request.ID;
                }
                Result = model.ID;
            }
            catch (Exception ex)
            {
               
            }
            return Result;
        }
        #endregion

        #region Card Linking
        public static long? AddLinkingRequest(RequestVM model)
        {
            long? Result = 0;
            model.RequestDate = DateTime.Now;
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Requests request = new tbl_Requests();
                    request.RequestType = model.RequestType;
                    request.AccountNo = model.DefaultAccountNo;
                    request.CIFNo = model.CustomerId;
                    request.CardNo = model.CardNo;
                    request.LinkingDelinkingAccount = model.LinkAccount;
                    request.WaiveCharges = model.Waive;
                    request.StatusEligibility = model.StatusEligibility;
                    request.FinancialEligibility = model.FinancialEligibility;
                    request.AuthorizationStatus = model.AuthorizationStatus;
                    request.RequestDate = DateTime.Now;
                    request.CardTitle = model.CardTitle;
                    request.CardTypeID = model.CardTypeId;
                    request.Salutation = model.Salutation;
                    request.IsActive = true;
                    request.IsExported = false;
                    request.CreatorID = StateHelper.UserId;
                    db.tbl_Requests.Add(request);
                    db.SaveChanges();
                    model.ID = request.ID;
                    Result = model.ID;
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Result;
        }
        #endregion

        public static List<AuthorizationVM> GetRequestDetails(string request)
        {
            using (var db = new SoneriCISEntities())
            {
                var query = db.tbl_Requests.Where(e => e.IsActive == true && e.AuthorizationStatus == "P" && e.RequestType == request).Select(o => new AuthorizationVM
                {
                    ID = o.ID,
                    AccountNo = o.AccountNo,
                    CardNo = o.CardNo,
                    CardTitle = o.CardTitle,
                    CardTypeID = o.CardTypeID,
                    CIFNo = o.CIFNo,
                    RequestDate = o.RequestDate,
                    RequestType = o.RequestType,
                    WaiveCharges = o.WaiveCharges,
                }).ToList();

                query.ForEach(e => e.CardType = db.tbl_Card_Types.FirstOrDefault(o => o.ID == e.CardTypeID && o.IsActive == true));

                return query;
            }
        }

        public static List<AuthorizationVM> FilterRequestDetails(FilterAuthorizationVM request)
        {
            using (var db = new SoneriCISEntities())
            {
                var _query = new List<AuthorizationVM>();
                var query = db.tbl_Requests.Where(e => e.IsActive == true && e.AuthorizationStatus == "P" && e.RequestType == request.RequestType).AsEnumerable();

                if (request.From != null)
                    query = query.Where(a => a.RequestDate >= request.From);
                if (request.To != null)
                    query = query.Where(a => a.RequestDate <= request.To);
                if (request.CIFNumber != null)
                    query = query.Where(a => a.CIFNo == request.CIFNumber);
                if (request.AccountNumber != null)
                    query = query.Where(a => a.AccountNo == request.AccountNumber);
                if (request.RequestNumber != null)
                    query = query.Where(a => a.ID == request.RequestNumber);

                _query = query.Select(o => new AuthorizationVM
                {
                    ID = o.ID,
                    AccountNo = o.AccountNo,
                    CardNo = o.CardNo,
                    CardTitle = o.CardTitle,
                    CardTypeID = o.CardTypeID,
                    CIFNo = o.CIFNo,
                    RequestDate = o.RequestDate,
                    RequestType = o.RequestType,
                    WaiveCharges = o.WaiveCharges,
                }).ToList();

                _query.ForEach(e => e.CardType = db.tbl_Card_Types.FirstOrDefault(o => o.ID == e.CardTypeID && o.IsActive == true));

                return _query;
            }
        }

        public static CustomerCardVM GetCustomerCard(string request)
        {
            var data = new CustomerCardDataAccess().GetCustomerCard(request);
            if (data == null)
                return null;

            return new CustomerCardVM
            {
                AccountNo = data.AccountNo,
                CardExpiry = data.CardExpiry,
                CardIssuance = data.CardIssuance,
                CardNo = data.CardNo,
                CardStatusActive = data.CardStatusActive,
                CardTitle = data.CardTitle,
                CardTypeID = data.CardTypeID,
                ID = data.ID,
                Salutation = data.Salutation,
                WaiveCharges = data.WaiveCharges
            };
        }

        //public static string GetLinkedAccounts()
        //{

        //}

        public static bool AddRequestCustomerAccount(RequestVM request, string AddressValue, string MobileValue, string LandlineValue, string OfficePhoneValue)
        {
            try
            {
                //Check if Request Cust Account already exists 
                tbl_Request_Customer_Accounts row1 = new CustomerRequestAccountDataAccess().GetRequestCustomerAccountByRequestId(int.Parse(request.ID.ToString()));
                
                    request.MainAddress = !string.IsNullOrEmpty(request.MainAddress) ? request.MainAddress.Replace("\n", " ") : string.Empty;
                    request.MainLandline = !string.IsNullOrEmpty(request.MainLandline) ? request.MainLandline.Replace("\n", " ") : string.Empty;
                    request.MainMobile = !string.IsNullOrEmpty(request.MainMobile) ? request.MainMobile.Replace("\n", " ") : string.Empty;

                //string AddressValue = ""; string MobileValue = "";
                //string LandlineValue= ""; string OfficePhoneValue = "";
              

                var row = new tbl_Request_Customer_Accounts()
                    {
                        AccountNo = request.AccountNo,
                        CardNo = request.CardNo,
                        AccountTypeID = request.AccountTypeId,
                        AccountTitle = request.AccountTitle,
                        Address = request.AccountAddress,

                        Address3 = request.CustomerAddress,
                        AddressType = request.AddressType,
                        LandlineNo = request.LandlineNo,
                        MainAddress = request.MainAddress,

                        CNIC = request.CNIC,
                        //DateofBirth = !string.IsNullOrEmpty(request.DateofBirth) ? Convert.ToDateTime(request.DateofBirth) : (DateTime?)null,

                        Email = request.Email,
                        MainMobile = request.MainMobile,
                        Mobile = request.MobileNo,
                       // Nationality = "PAKISTANI",
                        PassportNo = request.PassportNo,
                        MotherMaidenName = request.MotherName,
                        Salutation = request.Salutation,
                        PhoneOffice = request.PhoneOff,
                        WaiveCharges = request.Waive,
                        CIF = request.CIFNo,
                        Identification = request.Identification,
                        RequestID = request.ID

                       //saving main values required for audit trail report Jul-19
                       ,Address2 = AddressValue
                       ,Mobile2 = MobileValue
                       ,MainLandline = LandlineValue
                       ,Company = OfficePhoneValue
                    };
                if (row.Identification == "PoR")
                {
                    row.Nationality = "AF";
                }

                else
                {
                    row.Nationality = "PK";

                }



                if (row1 != null)
                    {
                        return new CustomerRequestAccountDataAccess().UpdateCustomerRequestAccount(row);
                    }
                    else
                    {
                        return new CustomerRequestAccountDataAccess().AddRequestCustomerAccountData(row);
                    }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static bool UpdateRequestCustomerAccount(RequestVM request)
        {
            var row = new CustomerRequestAccountDataAccess().GetRequestCustomerAccount((int)request.CustomerRequestID.GetValueOrDefault());
            if (row != null)
            {
                row.AccountNo = request.AccountNo;
                row.Address = request.MainAddress;
                row.LandlineNo = request.LandlineNo;
                row.PhoneOffice = request.PhoneOff;
                row.MainAddress = request.MainAddress;
                //row.MainLandline = request.MainLandline;
                row.MainMobile = request.MainMobile;

                return new CustomerRequestAccountDataAccess().UpdateCustomerRequestAccount(row);
            }

            return false;
        }

        #region Ammendment Request
        public static long? AddAmmendRequest(RequestVM model)
        {
            long? flag;
            model.RequestDate = DateTime.Now;
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Requests request = new tbl_Requests();
                    request.RequestType = model.RequestType;
                    request.AccountNo = model.AccountNo;
                    request.CIFNo = model.CustomerId;
                    request.WaiveCharges = model.Waive;
                    request.StatusEligibility = model.StatusEligibility;
                    request.FinancialEligibility = model.FinancialEligibility;
                    request.AuthorizationStatus = model.AuthorizationStatus;
                    request.RequestDate = model.RequestDate;
                    request.CardTitle = model.CardTitle;
                    request.CardTypeID = model.CardTypeId;
                    request.Salutation = model.Salutation;
                    request.IsActive = true;
                    request.CardNo = model.CardNumber;
                    request.IsExported = false;
                    request.CreatorID = StateHelper.UserId;
                    db.tbl_Requests.Add(request);
                    db.SaveChanges();
                    flag = request.ID;
                    model.ID = request.ID;
                    
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return flag;
        }
        #endregion

        public static List<CustomVM> GetCustomerCardByAccountNoCIF(string AccountNo, string CIF)
        {
            try
            {
                List<CustomVM> card = new List<CustomVM>();

                using (var db = new SoneriCISEntities())

                    card = (from cc in db.tbl_Customer_Cards where cc.AccountNo == AccountNo && cc.CIF == CIF
                            select new CustomVM {
                                ID = cc.ID,
                                CardNo = cc.CardNo,
                                CardStatusActive = cc.CardStatusActive==true?"Active":"InActive",
                                CardExpiry = cc.CardExpiry.ToString()
                            }).ToList();
                
                return card;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
