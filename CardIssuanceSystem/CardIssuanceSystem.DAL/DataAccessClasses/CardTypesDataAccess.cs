using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class CardTypesDataAccess
    {
        #region Insert
        public bool AddCardType(tbl_Card_Types row,List<tbl_Card_Types> UpgradeCardTypes)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    if (db.tbl_Card_Types.Any(e => e.Title.ToLower() == row.Title.ToLower()))
                        throw new CustomException("Title already exists");
                    //if (db.tbl_Card_Types.Any(e => e.CardCode.ToLower() == row.CardCode.ToLower()))
                    //    throw new CustomException("Code already exists");

                    db.tbl_Card_Types.Add(row);
                    db.SaveChanges();

                    //add Upgrade card types
                    if(UpgradeCardTypes.Count > 0)
                        AddCardUpgradeType(UpgradeCardTypes, row.ID);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //public bool AddCardUpgradeType(List<tbl_Card_Types> CardTypes,int ParentCardType)
        public bool AddCardUpgradeType(List<tbl_Card_Types> ParentCardTypes, int ChildCardTypes)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    foreach (tbl_Card_Types CT in ParentCardTypes)
                    {
                        tbl_Card_Upgrade_Types CUT = new DAL.tbl_Card_Upgrade_Types();
                        CUT.ChildCardType = ChildCardTypes;
                        CUT.ParentCardType = CT.ID;
                        db.tbl_Card_Upgrade_Types.Add(CUT);
                        db.SaveChanges();
                    }
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
        public bool UpdateCardType(tbl_Card_Types row, List<tbl_Card_Types> UpgradeCardTypes)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Card_Types val = new DAL.tbl_Card_Types();
                    val = db.tbl_Card_Types.Where(a => a.ID == row.ID).FirstOrDefault();
                    if (db.tbl_Card_Types.Any(e => e.Title.ToLower() != val.Title.ToLower() && e.Title.ToLower() == row.Title.ToLower()))
                        throw new CustomException("Title already exists");
                    //if (db.tbl_Card_Types.Any(e => e.CardCode.ToLower() != val.CardCode.ToLower() && e.CardCode.ToLower() == row.CardCode.ToLower()))
                    //    throw new CustomException("Code already exists");

                    val.Title = row.Title;
                    val.AuthorizationComments = row.AuthorizationComments;
                    val.AuthorizationStatus = row.AuthorizationStatus;
                    val.Description = row.Description;
                    val.IsActive = row.IsActive;
                    val.CardCode = row.CardCode;
                    val.IsIssuance = row.IsIssuance;
                    val.IsReplacement = row.IsReplacement;
                    val.IsUpgrade = row.IsUpgrade;
                    db.SaveChanges();

                    //update Upgrade Card Types
                    if (UpgradeCardTypes.Count > 0) {
                        DeleteCardUpgradeType(row.ID);
                        AddCardUpgradeType(UpgradeCardTypes, row.ID);
                    }
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
                var query = db.tbl_Card_Types.FirstOrDefault(e => e.ID == RequestID);
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
        public bool DeleteCardType(int CardTypeID)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Card_Types val = new DAL.tbl_Card_Types();
                    val = db.tbl_Card_Types.Where(a => a.ID == CardTypeID).FirstOrDefault();
                    val.Title = val.Title;
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

        public bool DeleteCardUpgradeType(int CardTypeID)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    List<tbl_Card_Upgrade_Types> val = new List<DAL.tbl_Card_Upgrade_Types>();
                    //val = db.tbl_Card_Upgrade_Types.Where(a => a.ParentCardType == CardTypeID).ToList();
                    val = db.tbl_Card_Upgrade_Types.Where(a => a.ChildCardType == CardTypeID).ToList();
                    foreach (tbl_Card_Upgrade_Types CT in val)
                    {
                        db.tbl_Card_Upgrade_Types.Remove(CT);
                        db.SaveChanges();
                    }
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
        public List<tbl_Card_Types> GetAllCardTypes()
        {
            try
            {
                List<tbl_Card_Types> lst = new List<DAL.tbl_Card_Types>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Card_Types.Where(a => a.IsActive == true).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<tbl_Card_Types> GetAllCardTypesActiveInactive()
        {
            try
            {
                List<tbl_Card_Types> lst = new List<DAL.tbl_Card_Types>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Card_Types.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public tbl_Card_Types GetCardType(int CardTypeID)
        {
            try
            {
               tbl_Card_Types cardType = new DAL.tbl_Card_Types();
                using (var db = new SoneriCISEntities())
                {
                    cardType = db.tbl_Card_Types.Include("tbl_Card_Upgrade_Types").Include("tbl_Card_Upgrade_Types1").Include("tbl_Card_Charges").Where(a => a.ID==CardTypeID).FirstOrDefault();
                }
                return cardType;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Card_Types> GetCardTypes(string AuthorizationStatus)
        {
            try
            {
                List<tbl_Card_Types> lst = new List<DAL.tbl_Card_Types>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Card_Types.Where(a => a.IsActive == true && a.AuthorizationStatus==AuthorizationStatus).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Card_Types> GetCardTypes(string AuthorizationStatus, string RequestTypeId)
        {
            try
            {
                List<tbl_Card_Types> lst = new List<DAL.tbl_Card_Types>();
                using (var db = new SoneriCISEntities())
                {
                    switch (RequestTypeId)
                    {
                        case "N":
                            lst = db.tbl_Card_Types.Where(a => a.IsActive == true && a.AuthorizationStatus == AuthorizationStatus && (a.IsIssuance == null || a.IsIssuance == true)).ToList();
                            break;
                        case "R":
                            lst = db.tbl_Card_Types.Where(a => a.IsActive == true && a.AuthorizationStatus == AuthorizationStatus && (a.IsReplacement == null || a.IsReplacement == true)).ToList();
                            break;
                        case "U":
                            lst = db.tbl_Card_Types.Where(a => a.IsActive == true && a.AuthorizationStatus == AuthorizationStatus && (a.IsUpgrade == null || a.IsUpgrade == true)).ToList();
                            break;
                        default:
                            lst = db.tbl_Card_Types.Where(a => a.IsActive == true && a.AuthorizationStatus == AuthorizationStatus).ToList();
                            break;
                    }
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<tbl_Card_Types> GetCardTypesActiveInactive(string AuthorizationStatus)
        {
            try
            {
                List<tbl_Card_Types> lst = new List<DAL.tbl_Card_Types>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Card_Types.Where(a => a.AuthorizationStatus == AuthorizationStatus).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Card_Upgrade_Types> GetCardUpgradeTypes(int ParentCardType)
        {
            try
            {
                List<tbl_Card_Upgrade_Types> lst = new List<DAL.tbl_Card_Upgrade_Types>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Card_Upgrade_Types.Where(a => a.ParentCardType==ParentCardType).ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckCardTypeEligibility(int ParentCardType,int ChildCardType)
        {
            try
            {
                List<tbl_Card_Upgrade_Types> lst = new List<DAL.tbl_Card_Upgrade_Types>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Card_Upgrade_Types.Where(a => a.ParentCardType == ParentCardType && a.ChildCardType==ChildCardType).ToList();
                }
                if (lst.Count>0)
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

        public int GetCardTypeId(string CardCode)
        {
            try
            {
                int CardTypeId = 0;
                using (var db = new SoneriCISEntities())
                {
                    CardTypeId = db.tbl_Card_Types.Where(a => a.IsActive == true && a.CardCode==CardCode).FirstOrDefault().ID;
                }
                return CardTypeId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckDuplicateCode(string CardCode)
        {
            try
            {
                tbl_Card_Types card = new DAL.tbl_Card_Types();
                using (var db = new SoneriCISEntities())
                {
                    card = db.tbl_Card_Types.Where(a => a.IsActive == true && a.CardCode == CardCode).FirstOrDefault();
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

        public List<int?> GetParentCardTypesByChildId(int id)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    //return GetNestedCardTypesData(id);
                    return db.tbl_Card_Upgrade_Types.Where(e => e.ChildCardType == id).Select(e => e.ParentCardType).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<int> GetNestedCardTypesData(int child)
        {
            List<int> nested = new List<int>();
            using (var db = new SoneriCISEntities())
            {
                var data = db.tbl_Card_Upgrade_Types.Where(e => e.ChildCardType == child).Select(e => e.ParentCardType).ToList();
                foreach (var item in data)
                {
                    nested.Add(item.GetValueOrDefault());
                    var getParents = GetNestedCardTypesData(item.GetValueOrDefault());
                    if (getParents.Any())
                        nested.AddRange(getParents);
                }

                return nested;
            }
        }

        public List<int?> GetParentCardTypeIds(int cardTypeId)
        {
            using (var db = new SoneriCISEntities())
            {
                return db.tbl_Card_Upgrade_Types.Where(e => e.ChildCardType == cardTypeId).Select(e => e.ParentCardType).ToList();
            }
        }

        public int? GetCardTypeIdByCardCode(string cardCode)
        {
            using (var db = new SoneriCISEntities())
            {
                var query = db.tbl_Card_Types.FirstOrDefault(e => e.Description == cardCode);
                if (query == null)
                    return null;
                else
                    return query.ID;
            }
        }

            #endregion
        }
}
