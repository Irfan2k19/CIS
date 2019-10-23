using CardIssuanceSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class AuthorizationVM : tbl_Requests
    {
        //public tbl_Card_Types CardType { get; set; } = new tbl_Card_Types();

        public tbl_Card_Types CardType { get; set; }
    }

    public class FilterAuthorizationVM
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public long? RequestNumber { get; set; }
        public string CIFNumber { get; set; }
        public string AccountNumber { get; set; }
        public string RequestType { get; set; }
    }


    //public class TransactionVM
    //{
    //    public DateTime TransactionDate { get; set; }
    //    public string TransactionType { get; set; }
    //    public string DrCustomerAccountNumber { get; set; }
    //    public string CrCustomerAccountNumber { get; set; }
    //    public string InstrumentType { get; set; }
    //    public decimal TransactionAmount { get; set; }
    //    public int OrginatingTranIdentNo { get; set; }
    //    public string Currency { get; set; }
    //    public string Authorization { get; set; }
    //    public DateTime? InstrumentDate { get; set; }
    //    public string STAN { get; set; }
    //    public int RequestId { get; set; }
    //    public string AuthorizationStatus { get; set; }
    //    public string AuthorizeComments { get; set; }
    //}
}
