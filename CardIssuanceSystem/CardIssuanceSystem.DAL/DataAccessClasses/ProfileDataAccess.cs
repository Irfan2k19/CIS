using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class ProfileDataAccess
    {
        #region Insert
        public bool AddProfile(tbl_Profile row)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Profile.Add(row);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool AddUserProfile(List<tbl_User_Profile> rows)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_User_Profile.AddRange(rows);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool AddProfilePage(List<tbl_Profile_Page> rows)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Profile_Page.AddRange(rows);
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

        public bool UpdateProfile(tbl_Profile row)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Profile u = new DAL.tbl_Profile();
                    u = db.tbl_Profile.Where(a => a.ID == row.ID).FirstOrDefault();
                    u.IsActive = row.IsActive;
                    u.Title = row.Title;
                    u.AuthorizationStatus = row.AuthorizationStatus;
                    u.AuthorizationComments = row.AuthorizationComments;
                    u.IsActive = row.IsActive;
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

        public bool DeletePageProfile(int ProfileID)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    List<tbl_Profile_Page> lst = new List<DAL.tbl_Profile_Page>();
                    lst = db.tbl_Profile_Page.Where(a => a.ProfileID == ProfileID).ToList();
                    db.tbl_Profile_Page.RemoveRange(lst);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool DeleteProfilePage(int ProfileID)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    List<tbl_Profile_Page> lst = new List<DAL.tbl_Profile_Page>();
                    lst = db.tbl_Profile_Page.Where(a => a.ProfileID == ProfileID).ToList();
                    db.tbl_Profile_Page.RemoveRange(lst);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool DeleteProfileUser(int UserID)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    List<tbl_User_Profile> lst = new List<DAL.tbl_User_Profile>();
                    lst = db.tbl_User_Profile.Where(a => a.UserID == UserID).ToList();
                    db.tbl_User_Profile.RemoveRange(lst);
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

        public tbl_Profile GetProfileById(int id)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_Profile.FirstOrDefault(e=>e.ID == id);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<tbl_Profile> GetAllProfiles(string AuthorizationStatus)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_Profile.Where(e => e.IsActive == true && e.AuthorizationStatus == AuthorizationStatus).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Profile> GetAllProfiles()
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_Profile.Where(e => e.IsActive == true ).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_User_Profile> GetUserProfiles(int UserID)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_User_Profile.Where(e => e.UserID==UserID).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Profile_Page> GetProfilePage(int ProfileID)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_Profile_Page.Where(e => e.ProfileID == ProfileID).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<tbl_Page> GetProfilePageSection(int ProfileID)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    var data = from p in db.tbl_Page
                               join pp in db.tbl_Profile_Page on p.ID equals pp.PageID
                               where pp.ProfileID == ProfileID
                               select p;
                    return data.ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Pages
        public List<tbl_Page> GetAllPages()
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_Page.Where(e => e.IsActive==true).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetSectionName(int SectionID)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    return db.tbl_Section.Where(e => e.IsActive == true && e.ID==SectionID).FirstOrDefault().Title;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Permissions

        public bool UserPagePermission(int UserID, string PageURL)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    var data = from p in db.tbl_Page
                               join pp in db.tbl_Profile_Page on p.ID equals pp.PageID
                               join pu in db.tbl_User_Profile on pp.ProfileID equals pu.ProfileID
                               where pu.UserID == UserID && PageURL.Contains(p.URL)
                               select p;
                    if (data.Count() > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            catch(Exception ex)
            {
                return false;
            }
        }

        #endregion
    }
}
