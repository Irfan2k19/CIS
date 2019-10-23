using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class IncomeAccountsDataAccess
    {
        #region Insert
        public bool AddIncomeAccounts(tbl_IncomeAccounts request)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_IncomeAccounts.Add(request);
                    db.SaveChanges();
                    return true;
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Update
        public bool UpdateIncomeAccounts(tbl_IncomeAccounts request)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    var query = db.tbl_IncomeAccounts.FirstOrDefault(e => e.ID == request.ID);
                    if (query == null)
                        return false;
                    else
                    {
                        query.AccountMapping = request.AccountMapping;
                        query.AccountType = request.AccountType;
                        query.Category = request.Category;
                        query.Classification = request.Classification;
                        query.CodeGL = request.CodeGL;
                        query.DescriptionGL = request.DescriptionGL;
                        query.InternalAccount = request.InternalAccount;
                        query.IsActive = request.IsActive;
                        query.TakeOn = request.TakeOn;
                        db.SaveChanges();
                        return true;
                    }
                }
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

        public List<tbl_IncomeAccounts> GetAllIncomeAccounts()
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_IncomeAccounts.ToList();
                }
            }
            catch (Exception ex)
            {
                return new List<tbl_IncomeAccounts>();
            }
        }

        public tbl_IncomeAccounts GetIncomeAccountById(int requestId)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_IncomeAccounts.FirstOrDefault(e => e.ID == requestId);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GetIncomeAccountByType(string Type)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_IncomeAccounts.FirstOrDefault(e => e.AccountType == Type).InternalAccount.ToString();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public string GetIncomeAccountByCardType(string CardNo)
        {
            try
            {
                string CreditAccount;
                if (!string.IsNullOrEmpty(CardNo))
                {
                    CardNo = CardNo.Substring(0, 6);
                }

                if (CardNo == "537939" || CardNo == "529779")
                {
                    CreditAccount = "PL52542";
                }
                else if (CardNo == "461688" || CardNo == "461689")
                {
                    CreditAccount = "PL52529";
                }
                else
                {
                    CreditAccount = "PL52542";
                }

                return CreditAccount;



            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion
    }
}
