using CardIssuanceSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class CardTypeVM : tbl_Card_Types
    {
        public List<int?> UpgradeChildTypes { get; set; }
        public List<int?> ParentTypes { get; set; }
    }
}
