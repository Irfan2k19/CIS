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
    public class CardHotMarkMethods
    {
        public static bool GenerateHotMarkRequest(string CardNo)
        {
            bool success = false;
            string AddressValue = ""; string MobileValue = "";
            string LandlineValue = ""; string OfficePhoneValue = "";

            try
            {
                List<CustomerCardVM> lst = new List<CustomerCardVM>();
                lst = CommonMethods.GetCardInfo(CardNo);
                if (lst.Count > 0)
                { 
                    using (var db = new SoneriCISEntities())
                    {
                        tbl_Requests request = new tbl_Requests();
                        request.RequestType = "H";
                        request.AccountNo = lst.FirstOrDefault().AccountNo;
                        request.CardNo = CardNo;
                        request.AuthorizationStatus = "P";
                        request.RequestDate = DateTime.Now;
                        request.CardTitle = lst.FirstOrDefault().CardTitle;
                        request.CardTypeID = lst.FirstOrDefault().CardTypeID;
                        request.Salutation = lst.FirstOrDefault().Salutation;
                        request.IsActive = true;
                        request.CIFNo = lst.FirstOrDefault().CIFNo;
                        request.CreatorID = StateHelper.UserId;
                        db.tbl_Requests.Add(request);
                       
                      
                        db.SaveChanges();
                        RequestVM rm = new RequestVM();
                        rm.AccountNo = request.AccountNo;
                        rm.CardNo = request.CardNo;
                        rm.ID = request.ID;
                        rm.RequestType = request.RequestType;
                        //  rm.AuthorizationStatus = request.AuthorizationStatus;
                        rm.CardTitle = request.CardTitle;
                        rm.CIFNo = request.CIFNo;
                        rm.Salutation = request.Salutation;
                        
                        RequestMethods.AddRequestCustomerAccount(rm, AddressValue, MobileValue, LandlineValue, OfficePhoneValue);


                    }
                    success = true;
                }
                else
                {
                    success = false;
                }
            }
            catch (Exception ex)
            {
                success = false;
               
                
            }
            return success;
        }

        public static bool GenerateHotMarkRequest(string CardNo, DateTime Expiry)
        {
            bool success = false;

            try
            {
                List<CustomerCardVM> lst = new List<CustomerCardVM>();
                lst = CommonMethods.GetCardInfo(CardNo, Expiry);
                if (lst.Count > 0)
                {
                    using (var db = new SoneriCISEntities())
                    {
                        tbl_Requests request = new tbl_Requests();
                        request.RequestType = "H";
                        request.AccountNo = lst.FirstOrDefault().AccountNo;
                        request.CardNo = CardNo;
                        request.AuthorizationStatus = "P";
                        request.RequestDate = DateTime.Now;
                        request.CardTitle = lst.FirstOrDefault().CardTitle;
                        request.CardTypeID = lst.FirstOrDefault().CardTypeID;
                        request.Salutation = lst.FirstOrDefault().Salutation;
                        request.IsActive = true;
                        request.CIFNo = lst.FirstOrDefault().CIFNo;
                        request.IsExported = false;
                        request.CreatorID = StateHelper.UserId;
                        db.tbl_Requests.Add(request);
                        db.SaveChanges();


                        var resp = db.tbl_User_Log.Where(e => e.EntityID == request.ID).FirstOrDefault();
                        if (resp != null)
                        {
                            resp.Descp = "A";
                            db.SaveChanges();
                        }
                    }
                    success = true;
                }
                else
                {
                    success = false;
                }
            }
            catch (Exception ex)
            {
                success = false;


            }
            return success;
        }

        public static List<HotMarkVM> SearchHotMarkRequest(string CardNo="")
        {
            try
            {
                
                var db = new SoneriCISEntities();
                var result = (from r in db.tbl_Requests join ct in db.tbl_Card_Types
                             on r.CardTypeID equals ct.ID where r.IsActive == true &&
                             r.RequestType == "H" && r.AuthorizationStatus == "P" && 
                             (r.CardNo == CardNo || CardNo==""||CardNo==null)
                              select new HotMarkVM
                              {
                                  RequestId=r.ID,
                                  CustomerID = r.CIFNo,
                                  AccountTitle=r.CardTitle,
                                  CardNo=r.CardNo,
                                  CardTypeId=r.CardTypeID,
                                  CardType=ct.Title,
                                  AccountNo=r.AccountNo
                             }).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool AuthorizeRequest(string CardNo,long RequestId,string AuthorizationStatus)
        {
            bool success = false;
            var db = new SoneriCISEntities();
            try
            { 
            tbl_Requests request = db.tbl_Requests.Single(x=>x.CardNo==CardNo && x.ID==RequestId);
            request.AuthorizationStatus = AuthorizationStatus;
                if (AuthorizationStatus == "A")
                {
                    /* Add new row in user log, as because we can't get the authorizer's record of the request, as because the authorizer has functionality to export the file, when file export occurs, the authorizer changes as we get record of the user who update the request in last */
                    new UserLogDataAccess().AddUserLog(null, "tbl_Requests", "A", RequestId.ToString());
                    request.AuthorizerID = StateHelper.UserId;
                }
                db.SaveChanges();
                
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
            }
            return success;

        }



        public static bool GenerateActivationRequest(string CardNo)
        {
            bool success = false;

            try
            {
                List<CustomerCardVM> lst = new List<CustomerCardVM>();
                lst = CommonMethods.GetCardInfo(CardNo);
                if (lst.Count > 0)
                {
                    using (var db = new SoneriCISEntities())
                    {
                        tbl_Requests request = new tbl_Requests();
                        request.RequestType = "W";
                        request.AccountNo = lst.FirstOrDefault().AccountNo;
                        request.CardNo = CardNo;
                        request.AuthorizationStatus = "P";
                        request.RequestDate = DateTime.Now;
                        request.CardTitle = lst.FirstOrDefault().CardTitle;
                        request.CardTypeID = lst.FirstOrDefault().CardTypeID;
                        request.Salutation = lst.FirstOrDefault().Salutation;
                        request.IsActive = true;
                        request.CreatorID = StateHelper.UserId;
                        request.CIFNo = lst.FirstOrDefault().CIFNo;
                        db.tbl_Requests.Add(request);
                        db.SaveChanges();
                    }
                    success = true;
                }
                else
                {
                    success = false;
                }
            }
            catch (Exception ex)
            {
                success = false;


            }
            return success;
        }


        public static List<HotMarkVM> SearchActiveMarkRequest(string CardNo = "")
        {
            try
            {

                var db = new SoneriCISEntities();
                var result = (from r in db.tbl_Requests
                              join ct in db.tbl_Card_Types
   on r.CardTypeID equals ct.ID
                              where r.IsActive == true &&
 r.RequestType == "W" && r.AuthorizationStatus == "P" &&
 (r.CardNo == CardNo || CardNo == "" || CardNo == null)
                              select new HotMarkVM
                              {
                                  RequestId = r.ID,
                                  CustomerID = r.CIFNo,
                                  AccountTitle = r.CardTitle,
                                  CardNo = r.CardNo,
                                  CardTypeId = r.CardTypeID,
                                  CardType = ct.Title,
                                  AccountNo = r.AccountNo
                              }).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
