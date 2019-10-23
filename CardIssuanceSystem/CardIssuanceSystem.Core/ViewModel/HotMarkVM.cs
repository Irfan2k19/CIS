using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class HotMarkVM
    {
    public long RequestId { get; set; }
    public string CustomerID { get; set; }
    public string AccountTitle {get;set;}
    public string CardNo       {get;set;}
    public int? CardTypeId   {get;set;}
    public string CardType     {get;set;}
    public string AccountNo    {get;set;}
    }
}
