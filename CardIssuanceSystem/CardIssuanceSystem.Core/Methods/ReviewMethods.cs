using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL;
using System.Collections.Generic;
using System.Linq;

namespace CardIssuanceSystem.Core.Methods
{
    public class ReviewMethods
    {
        public static List<AuthorizationVM> GetRequestDetails(string request)
        {
            using (var db = new SoneriCISEntities())
            {
                var query = db.tbl_Requests.Where(e => e.IsActive == true && e.AuthorizationStatus == "R" && e.RequestType == request).Select(o => new AuthorizationVM
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
                    AuthorizationComments = o.AuthorizationComments
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
                var query = db.tbl_Requests.Where(e => e.IsActive == true && e.AuthorizationStatus == "R" && e.RequestType == request.RequestType).AsEnumerable();

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
                    AuthorizationComments = o.AuthorizationComments
                }).ToList();

                _query.ForEach(e => e.CardType = db.tbl_Card_Types.FirstOrDefault(o => o.ID == e.CardTypeID && o.IsActive == true));

                return _query;
            }
        }

        public static List<AccountTypeVM> GetAccountTypeRequestDetails(string authorizationStatus)
        {
            using (var db = new SoneriCISEntities())
            {
                return db.tbl_Account_Types.Where(e => /*e.IsActive == true && */e.AuthorizationStatus == authorizationStatus).Select(e => new AccountTypeVM
                {
                    AuthorizationComments = e.AuthorizationComments,
                    Code = e.Code,
                    AuthorizationStatus = e.AuthorizationStatus,
                    Description = e.Description,
                    ID = e.ID,
                    Name = e.Name
                }).ToList();
            }
        }

        public static List<AccountTypeVM> FilterAccountTypeRequestDetails(FilterAccountTypeVM request, string authorizationStatus)
        {
            using (var db = new SoneriCISEntities())
            {
                //var _query = default(IQueryable<tbl_Account_Types>);
                var query = db.tbl_Account_Types.Where(e => /*e.IsActive == true && */e.AuthorizationStatus == authorizationStatus).AsQueryable();

                if (!string.IsNullOrEmpty(request.AccountTypeCode))
                    query = query.Where(e => e.Code == request.AccountTypeCode).AsQueryable();
                if (request.AccountTypeId.HasValue)
                    query = query.Where(e => e.ID == request.AccountTypeId).AsQueryable();

                return query.Select(e => new AccountTypeVM
                {
                    AuthorizationComments = e.AuthorizationComments,
                    Code = e.Code,
                    AuthorizationStatus = e.AuthorizationStatus,
                    Description = e.Description,
                    ID = e.ID,
                    Name = e.Name
                }).ToList();
            }
        }

        public static List<CardChargesVM> GetCardChargesRequestDetails(string authorizationStatus)
        {
            using (var db = new SoneriCISEntities())
            {
                return db.tbl_Card_Charges.Where(e => /*e.IsActive == true && */e.AuthorizationStatus == authorizationStatus).Select(e => new CardChargesVM
                {
                    ID = e.ID,
                    AccountTypeID = e.AccountTypeID,
                    Amount = e.Amount,
                    AuthorizationComments = e.AuthorizationComments,
                    AuthorizationStatus = e.AuthorizationStatus,
                    CardTypeID = e.CardTypeID,
                    Title = e.Title,
                    AccountTypeName = e.tbl_Account_Types.Name,
                    CardTypeName = e.tbl_Card_Types.Title
                }).ToList();
            }
        }

        public static List<CardChargesVM> FilterCardChargesRequestDetails(FilterAccountTypeVM request, string authorizationStatus)
        {
            using (var db = new SoneriCISEntities())
            {
                //var _query = default(IQueryable<tbl_Card_Charges>);
                var query = db.tbl_Card_Charges.Where(e => /*e.IsActive == true && */e.AuthorizationStatus == authorizationStatus).AsQueryable();

                if (request.CardTypeId.HasValue)
                    query = query.Where(e => e.CardTypeID == request.CardTypeId).AsQueryable();
                if (request.AccountTypeId.HasValue)
                    query = query.Where(e => e.AccountTypeID == request.AccountTypeId).AsQueryable();

                return query.Select(e => new CardChargesVM
                {
                    ID = e.ID,
                    AccountTypeID = e.AccountTypeID,
                    Amount = e.Amount,
                    AuthorizationComments = e.AuthorizationComments,
                    AuthorizationStatus = e.AuthorizationStatus,
                    CardTypeID = e.CardTypeID,
                    Title = e.Title,
                    AccountTypeName = e.tbl_Account_Types.Name,
                    CardTypeName = e.tbl_Card_Types.Title
                }).ToList();
            }
        }

        public static List<CardTypeVM> GetCardTypeRequestDetails(string authorizationStatus)
        {
            using (var db = new SoneriCISEntities())
            {
                return db.tbl_Card_Types.Where(e => /*e.IsActive == true && */e.AuthorizationStatus == authorizationStatus).Select(e => new CardTypeVM
                {
                    ID = e.ID,
                    AuthorizationComments = e.AuthorizationComments,
                    AuthorizationStatus = e.AuthorizationStatus,
                    Title = e.Title,
                    Description = e.Description,
                    IsIssuance = e.IsIssuance,
                    IsReplacement = e.IsReplacement,
                    IsUpgrade = e.IsUpgrade
                }).ToList();
            }
        }

        public static List<CardTypeVM> FilterCardTypeRequestDetails(FilterAccountTypeVM request, string authorizationStatus)
        {
            using (var db = new SoneriCISEntities())
            {
                //var _query = default(IQueryable<tbl_Card_Types>);
                var query = db.tbl_Card_Types.Where(e => /*e.IsActive == true && */e.AuthorizationStatus == authorizationStatus).AsQueryable();

                if (request.CardTypeId.HasValue)
                    query = query.Where(e => e.ID == request.CardTypeId).AsQueryable();

                return query.Select(e => new CardTypeVM
                {
                    ID = e.ID,
                    AuthorizationComments = e.AuthorizationComments,
                    AuthorizationStatus = e.AuthorizationStatus,
                    Title = e.Title,
                    Description = e.Description,
                    IsIssuance = e.IsIssuance,
                    IsReplacement = e.IsReplacement,
                    IsUpgrade = e.IsUpgrade
                }).ToList();
            }
        }
    }
}
