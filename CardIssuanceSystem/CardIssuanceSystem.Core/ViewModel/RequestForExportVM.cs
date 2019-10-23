using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class RequestForExportVM
    {
        public long ID { get; set; }
        public string NameOnCard { get; set; }
        public string AssociatedAddress { get; set; }
        public string NameOfCustomer { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string MotherMaidenName { get; set; }
        public string ResidentialPhoneNumber { get; set; }
        public string MaxRetriesForPIN1 { get; set; }
        public string RetriesForPIN1 { get; set; }
        public string StatusForPIN1 { get; set; }
        public string MaxRetriesForPIN2 { get; set; }
        public string RetriesForPIN2 { get; set; }
        public string StatusForPIN2 { get; set; }
        public string MaxRetriesForPIN3 { get; set; }
        public string RetriesForPIN3 { get; set; }
        public string StatusForPIN3 { get; set; }
        public string PIN4IB { get; set; }
        public string MaxRetriesForPIN4 { get; set; }
        public string RetriesForPIN4 { get; set; }
        public string StatusForPIN4 { get; set; }
        public string CompanyName { get; set; }
        public string OfficeAddress1 { get; set; }
        public string MobileNumber { get; set; }
        public string CustomerNumber { get; set; }
        public string NIC { get; set; }
        public string BillingAddress { get; set; }
        public DateTime MemberSince { get; set; }
        public string CustomerOf { get; set; }
        public string Nationality { get; set; }
        public DateTime DateOfIssue { get; set; }
        public DateTime DateOfExpiry { get; set; }
        public string AccountNo { get; set; }
        public string MainAddress { get; set; }
        public string MainLandline { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string Mobile2 { get; set; }
        public string OfficeAddress2 { get; set; }
        public string PhoneOff { get; set; }
        public string Email { get; set; }
        public string Passport { get; set; }
        public string BillingAddressFlag { get; set; }
        public string AccountStatus { get; set; }
        public string Salutation { get; set; }
        public string DefaultAccount { get; set; }
        public string Pan { get; set; }
        public string RequestType { get; set; }
        public string BranchCode { get; set; }
        public string LinkingDelinkingAccount { get; set; }
        public string TipperFlag { get; set; }
        public string AccountType { get; set; }
        public string BankIMD { get; set; }
        public string Currency { get; set; }
        public string CardNo { get; set; }
        public string PriSec { get; set; }
        public string FatherName { get; set; }
    }
}
