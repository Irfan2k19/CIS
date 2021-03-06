//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CardIssuanceSystem.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_Request_Customer_Accounts
    {
        public long ID { get; set; }
        public Nullable<long> RequestID { get; set; }
        public string AccountNo { get; set; }
        public string CIF { get; set; }
        public string CardNo { get; set; }
        public Nullable<bool> AccountStatusActive { get; set; }
        public string Salutation { get; set; }
        public string AccountTitle { get; set; }
        public string Address { get; set; }
        public string Mobile { get; set; }
        public Nullable<System.DateTime> DateofBirth { get; set; }
        public string MotherMaidenName { get; set; }
        public string Identification { get; set; }
        public string CNIC { get; set; }
        public string AddressType { get; set; }
        public Nullable<int> AccountTypeID { get; set; }
        public Nullable<bool> WaiveCharges { get; set; }
        public string PassportNo { get; set; }
        public string LandlineNo { get; set; }
        public string Email { get; set; }
        public string Nationality { get; set; }
        public string AccountCategoryCode { get; set; }
        public string PhoneOffice { get; set; }
        public string Company { get; set; }
        public string IdentificationType { get; set; }
        public string Mobile2 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string MainMobile { get; set; }
        public string MainLandline { get; set; }
        public string MainAddress { get; set; }
    
        public virtual tbl_Requests tbl_Requests { get; set; }
    }
}
