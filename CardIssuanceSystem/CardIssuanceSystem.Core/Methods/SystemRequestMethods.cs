using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL;
using CardIssuanceSystem.DAL.DataAccessClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.Methods
{
    public class SystemRequestMethods
    {
        public static List<Tuple<string, DateTime, int, string>> GetUserDetails(string AuthorizationStatus, string RequestType)
        {
            var rows = new UserDataAccess().GetUsers(AuthorizationStatus);
            if (rows.Count > 0)
            {
                using (var db = new SoneriCISEntities())
                {
                    return (from r in rows
                            let l = (from l in db.tbl_User_Log where l.EntityID == r.ID select l).FirstOrDefault()
                            select Tuple.Create(r.UserName, l.ActionTimestamp.GetValueOrDefault(), r.ID, "User_Register")).ToList();
                }
            }
            else
            {
                return new List<Tuple<string, DateTime, int, string>>();
            }
        }
        public static List<SystemRequestVM> GetSystemRequestForUser(string AuthorizationStatus, string RequestType)
        {
            List<SystemRequestVM> lstData = new List<SystemRequestVM>();
            var rows = new SystemRequestDataAccess().GetAllSystemRequests(AuthorizationStatus, RequestType);

            using (var db = new SoneriCISEntities())
            {
                lstData = (from r in rows
                           let l = (from l in db.tbl_User_Log where l.EntityID == r.ID && l.EntityName == "tbl_System_Requests" select l).FirstOrDefault()
                           select new SystemRequestVM
                           {
                               UserName = (RequestType == Constants.RequestTypes.UserModification) ? Helpers.CustomHelper.ParseJson<tbl_Users>(r.UpdatedData)?.UserName ?? string.Empty : Helpers.CustomHelper.ParseJson<List<ProfileUserVM>>(r.UpdatedData).FirstOrDefault()?.UserName ?? string.Empty,
                               RequestDate = l?.ActionTimestamp ?? new DateTime(),
                               ID = r.ID,
                               RequestType = r.RequestType
                           }).ToList();
            }

            return lstData;
        }

        public static List<Tuple<string, DateTime, int, string>> GetProfileDetails(string AuthorizationStatus, string RequestType)
        {
            var rows = new ProfileDataAccess().GetAllProfiles(AuthorizationStatus);
            if (rows.Count > 0)
            {
                using (var db = new SoneriCISEntities())
                {
                    return (from r in rows
                            let l = (from l in db.tbl_User_Log where l.EntityID == r.ID select l).FirstOrDefault()
                            select Tuple.Create(
                                r.Title,
                                l.ActionTimestamp.GetValueOrDefault(),
                                r.ID,
                                "Profile_Register"
                            )).ToList();
                }
            }
            else
            {
                return new List<Tuple<string, DateTime, int, string>>();
            }
        }
        public static List<SystemRequestVM> GetSystemRequestForProfile(string AuthorizationStatus, string RequestType)
        {
            List<SystemRequestVM> lstData = new List<SystemRequestVM>();
            if (RequestType.ToLower().Contains("register"))
            {
                var rows = new ProfileDataAccess().GetAllProfiles(AuthorizationStatus);
                if (rows.Count > 0)
                {
                    using (var db = new SoneriCISEntities())
                    {
                        lstData = (from r in rows
                                   let l = (from l in db.tbl_User_Log where l.EntityID == r.ID select l).FirstOrDefault()
                                   select new SystemRequestVM
                                   {
                                       ProfileName = r.Title,
                                       RequestDate = l.ActionTimestamp.GetValueOrDefault(),
                                       ID = r.ID,
                                       RequestType = "Profile_Register"
                                   }).ToList();
                    }
                }
            }
            else
            {
                var rows = new SystemRequestDataAccess().GetAllSystemRequests(AuthorizationStatus, RequestType);

                using (var db = new SoneriCISEntities())
                {
                    lstData = (from r in rows
                               let l = (from l in db.tbl_User_Log where l.EntityID == r.ID select l).FirstOrDefault()
                               select new SystemRequestVM
                               {
                                   ProfileName = (RequestType == Constants.RequestTypes.ProfileModification) ? Helpers.CustomHelper.ParseJson<tbl_Profile>(r.UpdatedData)?.Title ?? string.Empty : Helpers.CustomHelper.ParseJson<List<ProfilePageVM>>(r.UpdatedData).FirstOrDefault()?.ProfileName ?? string.Empty,
                                   RequestDate = l.ActionTimestamp.GetValueOrDefault(),
                                   ID = r.ID,
                                   RequestType = r.RequestType
                               }).ToList();
                }
            }

            return lstData;
        }
    }
}
