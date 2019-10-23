using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
   public class RequestVM
    {
        public DateTime? RequestDate{get;set;}
        public string CardType{get;set;}
        public int CardTypeId { get; set; }
        public string CardNature{get;set;}
        public string Salutation{get;set;}
        public string CardTitle{get;set;}
        public string ExistingCard{get;set;}
        public string CustomerId{get;set;}
        public string CustomerName{get;set;}
        public string AddressType{get;set;}
        public string Correspondence{get;set;}
        public string AccountAddress { get;set;}
        public string CustomerAddress { get;set;}
        public string Address{get;set;}
        public string MobileNo{get;set;}
        public string PhoneRes{get;set;}
        public string PhoneOff{get;set;}
        public string FaxNo{get;set;}
        public string Email{get;set;}
        public string FatherName{get;set;}
        public string MotherName{get;set;}
        public string DateofBirth{get;set;}
        public string Identification{get;set;}
        public string CNIC{get;set;}
        public string OldCNIC{get;set;}
        public string Product{get;set;}
        public string Scheme{get;set;}
        public string AccountNo{get;set;}
        public string AccountNature{get;set;}
        public string AccountTitle{get;set;}
        public string Authorization{get;set;}
        public bool Waive { get; set; }
        public string CIFNo { get; set; }
        public bool? StatusEligibility { get; set; }
        public bool FinancialEligibility { get; set; }
        public string AuthorizationStatus { get; set; }
        public string RequestType { get; set; }
        public string DefaultAccountNo {get;set;}
        public string DefaultAccountTitle{get;set;}
        
        public string CardNo { get; set; }
        public string[] LinkAccount1 { get; set; }
        public string LinkAccount  {get;set;}
        public string DeLinkAccount { get; set; }
        public string[] DeLinkAccount1 { get; set; }
        public string ExistingCardNumber { get; set; }
        public string CardNumber { get; set; }

        public double AvailBalance { get; set; }

        public string AccountStatus { get; set; }
        public int AccountTypeId { get; set; }

        public string BranchCode { get; set; }
        public string MainAddress { get; set; }
        public string MainMobile { get; set; }

        public string MainLandline { get; set; }
        public string PassportNo { get; set; }
        public long? ID { get; set; }
        public long? CustomerRequestID { get; set; }
        public string MobileNo1 {get;set;}
        public string MobileNo2 {get;set;}
        public string MobileNo3 { get; set; }
        public int ExistingCardType { get; set; }
        public string LandlineNo { get; set; }
    }

    
}





