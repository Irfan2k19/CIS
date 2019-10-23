using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class SessionDataAccess
    {
        #region Select
        public string GetLastTimeout()
        {
            try
            {
                string result = "0";
                var db = new SoneriCISEntities();
                var lst = db.tbl_Session_Timeout.OrderByDescending(r => r.ID).FirstOrDefault();
                if (lst !=null)
                {
                    result = lst.TimeOutInMinutes.ToString();
                }

                
                return result;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Insert

        public bool InsertSessionTimeout(int? sessiontime)
        {

            try
            {
                tbl_Session_Timeout row = new tbl_Session_Timeout();
                row.UserID = int.Parse(Convert.ToString(HttpContext.Current.Session["UserId"]));
                row.LastUpdatedOn = DateTime.Now;
                row.TimeOutInMinutes = sessiontime;
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Session_Timeout.Add(row);
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
    }
}
