using CardIssuanceSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class CustomerCardVM : tbl_Customer_Cards
    {
        public string CIFNo { get; set; }
        public List<int?> ParentIds { get; set; }
    }
}
