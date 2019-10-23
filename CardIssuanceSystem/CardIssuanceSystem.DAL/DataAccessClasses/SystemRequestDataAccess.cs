using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class SystemRequestDataAccess
    {
        #region Insert
        public bool AddSystemRequest(tbl_System_Requests row)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_System_Requests.Add(row);
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
        public bool UpdateSystemRequest(tbl_System_Requests row)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_System_Requests val = db.tbl_System_Requests.Where(a => a.ID == row.ID).FirstOrDefault();
                    val.AuthorizationComments = row.AuthorizationComments;
                    val.AuthorizationStatus = row.AuthorizationStatus;
                    val.AuthorizerID = row.AuthorizerID;
                    val.CreatorID = row.CreatorID;
                    val.IsActive = row.IsActive;
                    val.RequestType = row.RequestType;
                    val.UpdatedData = row.UpdatedData;
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
        public tbl_System_Requests GetSystemRequest(int SystemRequestID)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_System_Requests.Where(e => e.ID==SystemRequestID).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_System_Requests GetSystemRequest(int SystemRequestID, string AuthorizationStatus)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_System_Requests.Where(e => e.ID == SystemRequestID && e.AuthorizationStatus == AuthorizationStatus).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_System_Requests> GetAllSystemRequests(string AuthorizationStatus,string RequestType)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_System_Requests.Where(e => e.IsActive==true && e.AuthorizationStatus==AuthorizationStatus && e.RequestType==RequestType).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

    }
}
