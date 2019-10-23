using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class TransactionVM
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionType{get;set;}
        public string DrCustomerAccountNumber{get;set;}
        public string CrCustomerAccountNumber{get;set;}
        public string InstrumentType{get;set;}
        public decimal TransactionAmount{get;set;}
        public int OrginatingTranIdentNo{get;set;}
        public string Currency{get;set;}
        public string Authorization{get;set;}
        public DateTime? InstrumentDate{get;set;}
        public string STAN { get; set; }
        public int RequestId{get;set;}
        public string AuthorizationStatus{get;set;}
        public string AuthorizeComments { get; set; }

        public decimal AvailableBalance { get; set; }
        public string AccountStatus { get; set; }
        public string PayeeName { get; set; }
        public int AccountTypeId { get; set; }
        public int CardTypeId    {get;set;}
        public string BranchCode    {get;set;}
        public string RequestType { get; set; }
        public string[] AccountstoLink{get;set;}
        public string CardNo { get; set; }
        public string LinkRequest { get; set; }
        public string DeLinkRequest { get; set; }
        public string MainAddress { get; set; }
        public string MainMobile { get; set; }

        public string MainLandline { get; set; }
        public string PassportNo { get; set; }
        public bool WaiveCharges { get; set; }
        public string CIF { get; set; }

        public string LandlineNo { get; set; }
        public string PhoneOff{ get; set; }
    }
}
