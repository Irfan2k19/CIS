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
    
    public partial class tbl_Regional_Charges
    {
        public int ID { get; set; }
        public Nullable<int> RegionID { get; set; }
        public Nullable<int> CardChargeID { get; set; }
        public string Description { get; set; }
        public Nullable<double> PercentageAmount { get; set; }
        public Nullable<double> TotalAmount { get; set; }
    
        public virtual tbl_Card_Charges tbl_Card_Charges { get; set; }
        public virtual tbl_Region tbl_Region { get; set; }
    }
}