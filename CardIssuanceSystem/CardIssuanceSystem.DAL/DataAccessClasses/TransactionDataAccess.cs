using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
   public class TransactionDataAccess
    {
        #region Insert
        public bool AddTransaction(tbl_transactions row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_transactions.Add(row);
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


        #endregion

        #region Delete



        #endregion

        #region Select

        public List<tbl_transactions> GetAllTransactions()
        {
            try
            {
                List<tbl_transactions> lst = new List<DAL.tbl_transactions>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_transactions.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string GetLastSTAN()
        {
            try
            {
                string STAN = "";
                tbl_transactions t = new DAL.tbl_transactions();
                using (var db = new SoneriCISEntities())
                {
                    t = db.tbl_transactions.OrderByDescending(a => a.ID).FirstOrDefault();
                }
                return t.STAN;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckUniqueSTAN(string STAN)
        {
            try
            {
                tbl_transactions t = new DAL.tbl_transactions();
                using (var db = new SoneriCISEntities())
                {
                    t = db.tbl_transactions.Where(a => a.STAN==STAN).FirstOrDefault();
                }
                if (t == null)
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

        public long GetLastID()
        {
            try
            {
                long ID = 0;
                tbl_transactions t = new DAL.tbl_transactions();
                using (var db = new SoneriCISEntities())
                {
                    t = db.tbl_transactions.OrderByDescending(a => a.ID).FirstOrDefault();
                }
                return t.ID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_transactions> GetTransactions(DateTime? startTime, DateTime? endTime)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    if (startTime == null && endTime == null)
                        return db.tbl_transactions.OrderByDescending(e => e.ID).ToList();
                    else if (startTime == null && endTime != null)
                        return db.tbl_transactions.Where(e => e.TransactionDateTime <= endTime).OrderByDescending(e => e.ID).ToList();
                    else if (startTime != null && endTime == null)
                        return db.tbl_transactions.Where(e => e.TransactionDateTime >= startTime).OrderByDescending(e => e.ID).ToList();

                    return db.tbl_transactions.Where(e => e.TransactionDateTime >= startTime && e.TransactionDateTime <= endTime).OrderByDescending(e => e.ID).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public bool IsUniqueTransaction(string AccountNo,double? AmountToDeduct,string Narration)
        {
            bool IsUnique = false;
            DateTime LastTransaction = DateTime.Now.AddSeconds(-60);
            List<tbl_transactions> lst = new List<tbl_transactions>();
            using (var db = new SoneriCISEntities())
            {
                lst = db.tbl_transactions.Where(x=>x.DebitAccountNo==AccountNo && x.Amount==AmountToDeduct && x.TransactionDateTime==LastTransaction).ToList();
            }
            if (lst.Count <=0)
            {
                IsUnique = true;
            }

            return IsUnique;
            

        }


        public tbl_transactions GetLastTransaction()
        {
            try
            {
                bool isnull = false;
                tbl_transactions lst = new tbl_transactions();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_transactions.Where(x => x.RequestType=="RC").OrderByDescending(a=>a.TransactionDateTime).FirstOrDefault();
                }

                
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
