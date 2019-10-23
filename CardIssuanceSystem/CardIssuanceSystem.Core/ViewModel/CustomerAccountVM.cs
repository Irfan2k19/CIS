using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardIssuanceSystem.DAL;

namespace CardIssuanceSystem.Core.ViewModel
{
   public class CustomerAccountVM : tbl_Customer_Accounts
    {
        //public long ID { get; set; }
        //public string AccountNo { get; set; }
        //public string CIF { get; set; }
        //public string CardNo { get; set; }
        //public Nullable<bool> AccountStatusActive { get; set; }
        //public string Salutation { get; set; }
        //public string AccountTitle { get; set; }
        //public string Address { get; set; }
        //public string Mobile { get; set; }
        ////public Nullable<System.DateTime> DateofBirth { get; set; }
        //public string DateofBirth { get; set; }
        //public string MotherMaidenName { get; set; }
        //public string Identification { get; set; }
        //public string CNIC { get; set; }
        //public string AddressType { get; set; }
        //public int? AccountTypeID { get; set; }
        //public bool? WaiveCharges { get; set; }
        
        public bool? CardStatusActive{get;set;}
        public int? CardTypeID      {get;set;}
        public string CardTitle       {get;set;}
        public DateTime? CardExpiry      {get;set;}
        public DateTime? CardIssuance    {get;set;}
        public string DOB { get; set; }

        public string CardExpiryStr { get; set; }
        public string CardIssuanceStr { get; set; }
    }
}
