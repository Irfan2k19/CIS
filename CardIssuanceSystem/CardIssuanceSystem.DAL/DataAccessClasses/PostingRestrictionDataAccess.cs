using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class PostingRestrictionDataAccess
    {
        #region Insert
        public bool AddPostingRestrictions(tbl_Posting_Restrictions row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Posting_Restrictions.Add(row);
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
        public bool UpdatePostingRestrictions(tbl_Posting_Restrictions row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Posting_Restrictions val = new tbl_Posting_Restrictions();
                    val = db.tbl_Posting_Restrictions.Where(a => a.Code == row.Code).FirstOrDefault();
                    val.Code = row.Code;
                    val.Description = row.Description;
                    val.IsActive = row.IsActive;
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

        public bool DeletePostingRestrictions(tbl_Posting_Restrictions row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Posting_Restrictions val = new tbl_Posting_Restrictions();
                    val = db.tbl_Posting_Restrictions.Where(a => a.ID == row.ID).FirstOrDefault();
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

        #endregion

        #region Select

        public List<tbl_Posting_Restrictions> GetPosting_Restrictions()
        {
            try
            {
                List<tbl_Posting_Restrictions> lst = new List<tbl_Posting_Restrictions>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Posting_Restrictions.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Posting_Restrictions GetPostingRestrictionsbyID(int ID)
        {
            try
            {
                tbl_Posting_Restrictions row = new tbl_Posting_Restrictions();
                using (var db = new SoneriCISEntities())
                {
                    row = db.tbl_Posting_Restrictions.FirstOrDefault(e => e.ID == ID);
                }
                return row;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckPostingCode(List<string> Codes)
        {
            try
            {
                tbl_Posting_Restrictions row = new tbl_Posting_Restrictions();
                using (var db = new SoneriCISEntities())
                {
                    row = db.tbl_Posting_Restrictions.FirstOrDefault(e => Codes.Contains(e.Code.ToUpper()) && e.IsActive == true);
                }
                if (row != null)
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
                return false;
            }
        }

        public tbl_Posting_Restrictions GetPostingRestrictionsbyCode(string Code)
        {
            try
            {
                tbl_Posting_Restrictions row = new tbl_Posting_Restrictions();
                using (var db = new SoneriCISEntities())
                {
                    row = db.tbl_Posting_Restrictions.FirstOrDefault(e => e.Code == Code);
                }
                return row;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion
    }
}
