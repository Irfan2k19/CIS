using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.Methods
{
    public class MenuMethods
    {
        public static List<MenuVM> GetMenuItems()
        {
            List<MenuVM> lst = new List<MenuVM>();
            var db = new SoneriCISEntities();
            try
            {
                lst=(from u in db.tbl_User_Profile join pp in db.tbl_Profile_Page on u.ProfileID equals pp.ProfileID 
                     join p in db.tbl_Page on pp.PageID equals p.ID join sec in db.tbl_Section on p.SectionID equals sec.ID where
                     u.UserID ==StateHelper.UserId && p.SequenceNo > 0 orderby sec.SequenceNo,p.SequenceNo
                     select new MenuVM
                     {
                        UserId =u.UserID,
                        Section=sec.Title,
                        SectionSequence=sec.SequenceNo,
                        Page =p.Title,
                        URL =p.URL,
                        PageSequence =p.SequenceNo
                    }).ToList();
                
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
