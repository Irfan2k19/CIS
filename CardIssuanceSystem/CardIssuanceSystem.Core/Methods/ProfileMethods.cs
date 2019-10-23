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
    public class ProfileMethods
    {

        #region Profile Page
        public static List<PageSectionVM> GetAllProfilePages(int ProfileID)
        {
            try
            {
                var data = new ProfileDataAccess().GetProfilePageSection(ProfileID);
                if (data != null)
                {
                    return data.Select(e => new PageSectionVM
                    {
                        ID = e.ID,
                        Title = e.Title,
                        IsActive= e.IsActive,
                        SequenceNo = e.SequenceNo,
                        URL = e.URL,
                        SectionID = e.SectionID,
                        SectionTitle = new ProfileDataAccess().GetSectionName(int.Parse(e.SectionID.ToString())),
                    }).ToList();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Profile User
        public static List<ProfileUserVM> GetProfileUsers(int UserID)
        {
            try
            {
                var data = new ProfileDataAccess().GetUserProfiles(UserID);
                if (data != null)
                {
                    return data.Select(e => new ProfileUserVM
                    {
                        ID = e.ID,
                        ProfileID = e.ProfileID,
                        UserID = e.UserID,
                        ProfileTitle = new ProfileDataAccess().GetProfileById(int.Parse(e.ProfileID.ToString())).Title
                    }).ToList();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool AddProfileUsers(List<tbl_User_Profile> request)
        {
            var success = new ProfileDataAccess().DeleteProfileUser(request.FirstOrDefault().UserID.GetValueOrDefault());
            if (success)
            {
                return new ProfileDataAccess().AddUserProfile(request);
            }
            else
                return false;
        }

        #endregion

        #region Pages
        public static List<PageSectionVM> GetAllPages()
        {
            try
            {
                var data = new ProfileDataAccess().GetAllPages();
                if (data != null)
                {
                    return data.Select(e => new PageSectionVM
                    {
                        ID = e.ID,
                        Title = e.Title,
                        SectionID = e.SectionID,
                        SequenceNo = e.SequenceNo,
                        IsActive = e.IsActive,
                        URL = e.URL,
                        SectionTitle = new ProfileDataAccess().GetSectionName(int.Parse(e.SectionID.ToString()))
                    }).ToList();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool AddProfilePages(List<tbl_Profile_Page> request)
        {
            var success = new ProfileDataAccess().DeleteProfilePage(request.FirstOrDefault().ProfileID.GetValueOrDefault());
            if (success)
            {
                return new ProfileDataAccess().AddProfilePage(request);
            }
            else
                return false;
        }
        #endregion

    }
}
