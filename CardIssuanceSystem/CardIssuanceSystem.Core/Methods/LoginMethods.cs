using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CardIssuanceSystem.Core.Methods
{
    public class LoginMethods
    {
        public static bool Login(string UserName, string Password)
        {
            var hash = EncryptionHelper.HashString(Password);
            using (var db = new SoneriCISEntities())
            {
                var query = db.tbl_Users.FirstOrDefault(e => e.UserLogin.ToLower() == UserName.ToLower() && e.UserPassword == hash && e.IsActive == true);
                if (query == null)
                {
                    return false;
                }
                 

                else
                {
                    StateHelper.UserId = query.ID;
                    StateHelper.RoleId = query.RoleTitle;
                    StateHelper.Username = query.UserName;
                    StateHelper.Pages = db.tbl_Page.Select(e => new ViewModel.PageSectionVM
                    {
                        Title = e.Title,
                        URL = e.URL
                    }).ToList();
                }

                return true;
            }
        }

        public static int? IsUserExists(string UserName)
        {
            using (var db = new SoneriCISEntities())
            {
                var query = db.tbl_Users.FirstOrDefault(e => e.UserLogin.ToLower() == UserName.ToLower());
                if (query == null)
                    return null;
                else
                    return query.ID;
            }
        }

        public static bool AddUserLog(int UserId, long? EntityId = null)
        {
            List<SqlParameter> param = new List<SqlParameter>();
            param.Add(new SqlParameter("@EventName", "l"));
            param.Add(new SqlParameter("@EntityName", "Login"));
            param.Add(new SqlParameter("@EntityID", EntityId));
            param.Add(new SqlParameter("@UserId", UserId));

            var success = Convert.ToString(DatabaseGateway.GetScalarDataUsingStoredProcedure("sp_AddUserLog", param));

            return true;
        }


        public static bool ValidateLogin(string UserName)
        {
            var lastFiveMins = DateTime.Now.AddMinutes(-5); //mkhan: Need to change this, as don't know business
            var current = DateTime.Now;
            using (var db = new SoneriCISEntities())
            {
                var query = db.tbl_User_Log.Where(e => e.tbl_Users.UserLogin.ToLower() == UserName.ToLower() && e.EntityName == "login" && e.EventName == "l" && e.ActionTimestamp >= lastFiveMins && e.ActionTimestamp <= current && e.EntityID == null).Count();

                if (query >= int.MaxValue) //mkhan: Need to remove this, as don't know business
                    return false;

                return true;
            }
        }


        public static bool Login2(string UserName)
        {
            
            using (var db = new SoneriCISEntities())
            {
                var query = db.tbl_Users.FirstOrDefault(e => e.UserLogin.ToLower() == UserName.ToLower() && e.IsActive == true);
                if (query == null)
                {
                    return false;
                }
                    
                else
                {
                    StateHelper.UserId = query.ID;
                    StateHelper.RoleId = query.RoleTitle;
                    StateHelper.Username = query.UserName;
                    StateHelper.Pages = db.tbl_Page.Select(e => new ViewModel.PageSectionVM
                    {
                        Title = e.Title,
                        URL = e.URL
                    }).ToList();
                }

                return true;
            }
        }





        public static bool CheckExpiry(string UserName)
        {

            using (var db = new SoneriCISEntities())
            {
                var query = db.tbl_Users.FirstOrDefault(e => e.UserLogin.ToLower() == UserName.ToLower() && e.IsActive == true);
                if (query.ExpiryDate != null)
                {
                    if (query.ExpiryDate < DateTime.Now)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

    }
}