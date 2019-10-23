using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class UserDataAccess
    {
        #region Insert
        public bool AddUser(tbl_Users row)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Users.Add(row);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool AddUserLog(tbl_User_Log row)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_User_Log.Add(row);
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

        public bool UpdateUser(tbl_Users row)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Users u = new DAL.tbl_Users();
                    u = db.tbl_Users.Where(a => a.ID == row.ID).FirstOrDefault();
                    u.IsActive = row.IsActive;
                    u.RoleTitle = row.RoleTitle;
                    u.UserLogin = row.UserLogin;
                    u.UserName = row.UserName;
                    u.ExpiryDate = row.ExpiryDate;
                    u.EmpCode = row.EmpCode;
                    if (row.UserPassword == "")
                    {
                        u.UserPassword = u.UserPassword;
                    }
                    else
                    {
                        u.UserPassword = row.UserPassword;
                    }
                    u.IsActive = row.IsActive;
                    u.AuthorizationStatus = row.AuthorizationStatus;
                    u.AuthorizationComments = row.AuthorizationComments;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool ChangePassword(int UserID,string UserPassword)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Users u = new DAL.tbl_Users();
                    u = db.tbl_Users.Where(a => a.ID == UserID).FirstOrDefault();
                    u.IsActive = u.IsActive;
                    u.RoleTitle = u.RoleTitle;
                    u.UserLogin = u.UserLogin;
                    u.UserName = u.UserName;
                    u.UserPassword = UserPassword;
                    u.IsActive = u.IsActive;
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


        #endregion

        #region Select

        public bool CheckPassword(int UserID, string UserPassword)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Users u = new DAL.tbl_Users();
                    u = db.tbl_Users.Where(a => a.ID == UserID && a.UserPassword==UserPassword).FirstOrDefault();
                    if (u == null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<tbl_User_Log> GetAllUserLogs()
        {
            try
            {
                List<tbl_User_Log> lst = new List<DAL.tbl_User_Log>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_User_Log.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Users GetUserById(int id)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_Users.FirstOrDefault(e=>e.ID == id);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Users GetUserByUserID(string UserID)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_Users.FirstOrDefault(e => e.UserLogin == UserID && e.AuthorizationStatus=="A");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Register(tbl_Users request)
        {
            try
            {
                //if (request != null)
                //    throw new Exception("Hello");
                using (var db = new SoneriCISEntities())
                {
                    if (db.tbl_Users.Any(e => e.UserLogin.ToLower() == request.UserLogin.ToLower()))
                        throw new CustomException("Username already exists");

                    db.tbl_Users.Add(request);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<tbl_Users> GetAllUsersForAdmin()
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_Users.Where(e => e.IsActive == true && e.RoleTitle != "a" && e.AuthorizationStatus == "A").ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Users> GetUsers(string AuthorizationStatus)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_Users.Where(e => e.IsActive == true && e.AuthorizationStatus == AuthorizationStatus).ToList();
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
