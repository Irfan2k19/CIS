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
    
    public partial class tbl_Card_Charges
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Card_Charges()
        {
            this.tbl_Regional_Charges = new HashSet<tbl_Regional_Charges>();
        }
    
        public int ID { get; set; }
        public string Title { get; set; }
        public Nullable<int> AccountTypeID { get; set; }
        public Nullable<int> CardTypeID { get; set; }
        public string Frequency { get; set; }
        public Nullable<double> Amount { get; set; }
        public string AuthorizationStatus { get; set; }
        public string AuthorizationComments { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsFED { get; set; }
        public Nullable<bool> IsReplacement { get; set; }
    
        public virtual tbl_Account_Types tbl_Account_Types { get; set; }
        public virtual tbl_Card_Types tbl_Card_Types { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Regional_Charges> tbl_Regional_Charges { get; set; }
    }
}
