using CardIssuanceSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class AccountTypeVM : tbl_Account_Types
    {
    }

    public class FilterAccountTypeVM
    {
        public int? AccountTypeId { get; set; }
        public string AccountTypeCode { get; set; }
        public int? CardTypeId { get; set; }
    }
}
