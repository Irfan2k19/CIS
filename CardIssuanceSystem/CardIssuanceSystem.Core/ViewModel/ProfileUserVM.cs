using CardIssuanceSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class ProfileUserVM : tbl_User_Profile
    {
        public string ProfileTitle{ get; set; }
        public string UserName { get; set; }//check
    }
}
