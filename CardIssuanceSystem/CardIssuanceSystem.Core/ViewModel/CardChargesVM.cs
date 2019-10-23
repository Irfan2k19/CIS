using CardIssuanceSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class CardChargesVM : tbl_Card_Charges
    {
        public string AccountTypeName{ get; set; }
        public string CardTypeName { get; set; }
    }
}
