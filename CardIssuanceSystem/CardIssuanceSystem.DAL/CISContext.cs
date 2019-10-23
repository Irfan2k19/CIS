using CardIssuanceSystem.DAL.DataAccessClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CardIssuanceSystem.DAL
{
    public class CISContext
    {
    }
    public partial class SoneriCISEntities : DbContext
    {
        public override int SaveChanges()
        {
            var AddedEntities = ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList();

            AddedEntities.ForEach(e =>
            {
                if (e.Entity.GetType().Name != "tbl_User_Log")
                {
                    List<SqlParameter> param = new List<SqlParameter>();
                    param.Add(new SqlParameter("@EventName", "i"));
                    param.Add(new SqlParameter("@EntityName", e.Entity.GetType().Name));
                    param.Add(new SqlParameter("@EntityID", "0"));
                    param.Add(new SqlParameter("@UserId", int.Parse(Convert.ToString(HttpContext.Current.Session["UserId"]))));
                    DataTable dt = DatabaseGateway.GetDataUsingStoredProcedure("sp_AddUserLog", param);
                }
                //tbl_User_Log log = new tbl_User_Log()
                //{
                //    EventName = "i",
                //    ActionTimestamp = DateTime.Now,
                //    ActionUserStamp = int.Parse(Convert.ToString(HttpContext.Current.Session["UserId"])),
                //    EntityName = e.Entity.GetType().Name
                //};

                //tbl_User_Log.Add(log);
            });

            var ModifiedEntities = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList();

            ModifiedEntities.ForEach(e =>
            {
                long eID = 0; 
                if (e.Property("ID").CurrentValue.GetType() == typeof(int))
                    eID = (long)((int)(e.Property("ID").CurrentValue));

                else if (e.Property("ID").CurrentValue.GetType() == typeof(long))
                    eID = (long)(e.Property("ID").CurrentValue);

                if (e.Entity.GetType().BaseType.Name != "tbl_User_Log")
                {
                    List<SqlParameter> param = new List<SqlParameter>();
                    param.Add(new SqlParameter("@EventName", "u"));
                    param.Add(new SqlParameter("@EntityName", e.Entity.GetType().BaseType != null ? e.Entity.GetType().BaseType.Name : e.Entity.GetType().Name));
                    param.Add(new SqlParameter("@EntityID", eID));
                    param.Add(new SqlParameter("@UserId", int.Parse(Convert.ToString(HttpContext.Current.Session["UserId"]))));
                    DataTable dt = DatabaseGateway.GetDataUsingStoredProcedure("sp_AddUserLog", param);
                }
                //tbl_User_Log log = new tbl_User_Log()
                //{
                //    EventName = "u",
                //    ActionTimestamp = DateTime.Now,
                //    ActionUserStamp = int.Parse(Convert.ToString(HttpContext.Current.Session["UserId"])),
                //    EntityName = e.Entity.GetType().BaseType.Name,
                //    EntityID = eID
                //};

                //tbl_User_Log.Add(log);
            });

            var DeletedEntities = ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted).ToList();

            DeletedEntities.ForEach(e =>
            {
                long eID = 0;
                if (e.Property("ID").CurrentValue.GetType() == typeof(int))
                    eID = (long)((int)(e.Property("ID").CurrentValue));

                else if (e.Property("ID").CurrentValue.GetType() == typeof(long))
                    eID = (long)(e.Property("ID").CurrentValue);

                if (e.Entity.GetType().BaseType.Name != "tbl_User_Log")
                {
                    List<SqlParameter> param = new List<SqlParameter>();
                    param.Add(new SqlParameter("@EventName", "d"));
                    param.Add(new SqlParameter("@EntityName", e.Entity.GetType().BaseType != null ? e.Entity.GetType().BaseType.Name : e.Entity.GetType().Name));
                    param.Add(new SqlParameter("@EntityID", eID));
                    param.Add(new SqlParameter("@UserId", int.Parse(Convert.ToString(HttpContext.Current.Session["UserId"]))));

                    DataTable dt = DatabaseGateway.GetDataUsingStoredProcedure("sp_AddUserLog", param);
                }
                //tbl_User_Log log = new tbl_User_Log()
                //{
                //    EventName = "d",
                //    ActionTimestamp = DateTime.Now,
                //    ActionUserStamp = int.Parse(Convert.ToString(HttpContext.Current.Session["UserId"])),
                //    EntityName = e.Entity.GetType().BaseType.Name,
                //    //EntityID = (long)(e.Property("ID").CurrentValue)
                //};

                //tbl_User_Log.Add(log);
            });

            return base.SaveChanges();
        }
    }
}
