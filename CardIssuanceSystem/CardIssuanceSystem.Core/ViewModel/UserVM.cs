using CardIssuanceSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class UserVM : tbl_Users
    {
        public DateTime RequestDate { get; set; }
        public string ExpiryDateStr { get; set; }
    }
}
