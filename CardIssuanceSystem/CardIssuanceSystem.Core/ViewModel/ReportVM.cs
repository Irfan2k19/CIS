using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class ReportVM
    {
    }

    public class ReportCardAmmendmentVM
    {
        public string Creator { get; set; }
        public string Authorizer { get; set; }
        public string ModifyDate { get; set; }
        public string RequestNo { get; set; }

        public string AccountNo { get; set; }
        public string CardType { get; set; }
        public string CardNo { get; set; }
        public string CardTitle { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string AssociateAddress { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string CardStatus { get; set; }
        public string Phone { get; set; }
        public string CellNo { get; set; }
        public string EmailAddress { get; set; }
        public string NIC { get; set; }
        public string PassportNo { get; set; }
        public string DateOfBirth { get; set; }
        public string MotherMaidenName { get; set; }
        public string AuthorizationStatus { get; set; }
    }

    public class ReportCardImportVM
    {
        public string CardNo { get; set; }
        public string CIF { get; set; }
        public string AccountNo { get; set; }
        public int? CardTypeId { get; set; }
        public string CardType { get; set; }
        public DateTime? CardProductionDate { get; set; }
        public DateTime? CardExpiryDate { get; set; }
        public DateTime? CardImportDate { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
    }
}
