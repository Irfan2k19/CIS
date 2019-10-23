using CardIssuanceSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class SystemRequestVM : tbl_System_Requests
    {
        public string UserName { get; set; }
        public string ProfileName { get; set; }
        public DateTime RequestDate { get; set; }
    }
}
