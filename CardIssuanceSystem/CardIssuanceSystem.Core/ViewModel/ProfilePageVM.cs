using CardIssuanceSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class ProfilePageVM : tbl_Profile_Page
    {
        public string PageTitle { get; set; }
        public string ProfileName { get; set; }
        public int? SectionID { get; set; }
        public string SectionName { get; set; }
    }
}
