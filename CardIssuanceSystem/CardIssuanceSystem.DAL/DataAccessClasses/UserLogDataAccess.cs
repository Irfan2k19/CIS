using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class UserLogDataAccess
    {
        public bool AddExceptionLog(string json, string entityName)
        {
            try
            {
                List<SqlParameter> param = new List<SqlParameter>();
                param.Add(new SqlParameter("@EventName", "M"));
                param.Add(new SqlParameter("@EntityName", entityName));
                param.Add(new SqlParameter("@EntityID", "0"));
                param.Add(new SqlParameter("@UserId", int.Parse(Convert.ToString(HttpContext.Current.Session["UserId"]))));
                param.Add(new SqlParameter("@Description", json));
                var success = DatabaseGateway.GetScalarDataUsingStoredProcedure("sp_AddUserLog", param);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool AddUserLog(string json, string entityName, string eventName, string entityId)
        {
            try
            {
                List<SqlParameter> param = new List<SqlParameter>();
                param.Add(new SqlParameter("@EventName", eventName));
                param.Add(new SqlParameter("@EntityName", entityName));
                param.Add(new SqlParameter("@EntityID", entityId));
                param.Add(new SqlParameter("@UserId", int.Parse(Convert.ToString(HttpContext.Current.Session["UserId"]))));
                param.Add(new SqlParameter("@Description", json));
                var success = DatabaseGateway.GetScalarDataUsingStoredProcedure("sp_AddUserLog", param);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_User_Log> GetExceptionLog(string entityName, DateTime? startTime, DateTime? endTime, string eventName)    
        {
            using (var db = new SoneriCISEntities())
            {
                if (startTime == null && endTime == null)
                    return db.tbl_User_Log.Where(e => e.EntityName.ToLower() == entityName.ToLower() && e.EventName == eventName).OrderByDescending(e => e.ID).ToList();
                else if (startTime == null && endTime != null)
                    return db.tbl_User_Log.Where(e => e.EntityName.ToLower() == entityName.ToLower() && e.ActionTimestamp <= endTime && e.EventName == eventName).OrderByDescending(e => e.ID).ToList();
                else if (startTime != null && endTime == null)
                    return db.tbl_User_Log.Where(e => e.EntityName.ToLower() == entityName.ToLower() && e.ActionTimestamp >= startTime && e.EventName == eventName).OrderByDescending(e => e.ID).ToList();

                return db.tbl_User_Log.Where(e => e.EntityName.ToLower() == entityName.ToLower() && e.ActionTimestamp >= startTime && e.ActionTimestamp <= endTime && e.EventName == eventName).OrderByDescending(e => e.ID).ToList();
            }
        }

        public List<tbl_User_Log> GetUserLogByEntityId(long ID, string EntityName)
        {
            using (var db =new SoneriCISEntities())
            {
                return db.tbl_User_Log.Include("tbl_Users").Where(e => e.EntityID == ID && e.EntityName == EntityName).ToList();
            }
        }
        public bool UpdateUserLog(long requestId, int currentCardTypeId)
        {
            using (var db = new SoneriCISEntities())
            {
                var query = db.tbl_User_Log.Where(e => e.EntityID == requestId).FirstOrDefault();
                if (query != null)
                {
                    query.Descp = $"CurrentCardTypeId={currentCardTypeId}";
                    db.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
        }

        public bool UpdateUserLog(long requestId, string json)
        {
            using (var db = new SoneriCISEntities())
            {
                var query = db.tbl_User_Log.Where(e => e.EntityID == requestId).FirstOrDefault();
                if (query != null)
                {
                    query.Descp = json;
                    db.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
        }

        public List<tbl_User_Log> GetUserLogByEntityId(long ID)
        {
            using (var db = new SoneriCISEntities())
            {
                return db.tbl_User_Log.Include("tbl_Users").Where(e => e.EntityID == ID).ToList();
            }
        }

    }
}
