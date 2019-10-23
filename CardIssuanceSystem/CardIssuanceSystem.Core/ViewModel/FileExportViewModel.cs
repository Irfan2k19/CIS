using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.Core.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class FileExportViewModel
    {
        public HeaderRecord Header { get; set; }
        public DataRecord Data { get; set; }
        public FooterRecord Footer { get; set; }
    }

    public class HeaderRecord
    {
        public string RecordNumber { get; set; } = "000001".LimitTo(6);
        public string RecordType { get; set; } = "HE".LimitTo(2);
        public string Date { get; set; }
        public string FileName { get; set; }
        public string Version { get; set; }
    }
    public class DataRecord
    {
        public string RecordNumber { get; set; }
        public string RecordType { get; set; }
        public string ActionCode { get; set; }
    }
    public class FooterRecord
    {
        public string RecordNumber { get; set; }
        public string RecordType { get; set; } = "FO".LimitTo(2);
        public string Date { get; set; }
        public string FileName { get; set; } 
        public string EndSentinel { get; set; }
    }

    public class CardData
    {
        public string RecordNumber { get; set; }
        public string RecordType { get; set; }
        public string ActionCode { get; set; }
        public string TipperFlag { get; set; }
        public string PrisecCode { get; set; }
        public string PAN { get; set; }
        //public string CardSeqNo { get; set; } = "000".LimitTo(3);
        public string ProcessingCode { get; set; }
        public string CycleBeginDate { get; set; }
        public string CycleLength { get; set; }
        public string CycleAmount { get; set; }
        public string AmountRemaining { get; set; }
        public string CardStatus { get; set; }
        public string CardExpiryDate { get; set; }
        //public string PinRetries { get; set; } = "3".LimitTo(1);
        //public string MaxRetries { get; set; } = "3".LimitTo(1);
        public string EmbossingName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
        public string PinDate { get; set; }
        public string PurchaseLimitCycleBeginDate { get; set; }
        public string PurchaseLimitCycleLength { get; set; }
        public string PurchaseLimitCycleAmount { get; set; }
        public string PurchaseLimitAmountRemaining { get; set; }
        public string AllowSrcATM { get; set; }
        public string AllowSrcCIRRUS { get; set; }
        public string AllowSrcPHXSW { get; set; }
        public string AllowSrcEFTPOS { get; set; }
        public string AllowSrcVISA { get; set; }
        public string AllowSrcAMEX { get; set; }
        public string AllowSrcMNET { get; set; }
        public string AllowSrcIVR { get; set; }
        public string AllowSrcMobile { get; set; }
        public string AllowSrcCardPro { get; set; }
        public string AllowSrcCardPCASH { get; set; }
        public string AllowSrcCardIB { get; set; }
        public string Reserved2 { get; set; } = string.Empty.LimitTo(8);
        public string FreqBalInq { get; set; }
        public string FreqWdwl { get; set; }
        public string FreqPurchase { get; set; }
        public string FreqMiniStmt { get; set; }
        //public string FreqRsvd1 { get; set; } = "999".LimitTo(3);
        //public string FreqRsvd2 { get; set; } = "999".LimitTo(3);
        //public string FreqRsvd3 { get; set; } = "999".LimitTo(3);
        //public string FreqRsvd4 { get; set; } = "999".LimitTo(3);
        //public string FreqRsvd5 { get; set; } = "999".LimitTo(3);
        //public string FreqRsvd6 { get; set; } = "999".LimitTo(3);
        public string FrequencyCycleBeginDate { get; set; }
        public string FrequencyLimitCycleLength { get; set; }
        public string FundsTransferLimitCycleBeginDate { get; set; }
        public string FundsTransferLimitCycleLength { get; set; }
        public string FundsTransferLimitCycleAmount { get; set; }
        public string FundsTransferLimitAmountRemaining { get; set; }
        public string Customer0rStaff { get; set; }
        public string PcashLimitCycleBeginDate { get; set; }
        public string PcashLimitCycleLength { get; set; }
        public string PcashLimitCycleAmount { get; set; }
        public string PcashLimitAmountRemaining { get; set; }
        public string CustomerId { get; set; }
        public string ProductCode { get; set; }
        public string PrimaryCNIC { get; set; }
        public string IssuanceType { get; set; }
    }

    public class CustomerProfile
    {
        public string RecordNumber { get; set; }
        public string RecordType { get; set; }
        public string ActionCode { get; set; }
        public string OldNIC { get; set; }
        public string Title { get; set; }
        public string FullName { get; set; }
        public string DateOfBirth { get; set; }
        public string MothersMaidenName { get; set; }
        public string ResidentialPostalCode { get; set; }
        public string ResidentialPhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Reserved { get; set; } = string.Empty.LimitTo(32);
        public string MaxRetriesForPIN1 { get; set; } = "03".LimitTo(2);
        public string RetriesForPIN1 { get; set; } = "03".LimitTo(2);
        public string StatusForPIN1 { get; set; }
        public string Reserved1 { get; set; } = string.Empty.LimitTo(32);
        public string MaxRetriesForPIN2 { get; set; } = "03".LimitTo(2);
        public string RetriesForPIN2 { get; set; } = "03".LimitTo(2);
        public string StatusForPIN2 { get; set; }
        public string Reserved2 { get; set; } = string.Empty.LimitTo(32);
        public string MaxRetriesForPIN3 { get; set; } = "03".LimitTo(2);
        public string RetriesForPIN3 { get; set; } = "03".LimitTo(2);
        public string StatusForPIN3 { get; set; }
        public string PIN4IB { get; set; }
        public string MaxRetriesForPIN4 { get; set; } = "03".LimitTo(2);
        public string RetriesForPIN4 { get; set; } = "03".LimitTo(2);
        public string StatusForPIN4 { get; set; }
        public string CompanyName { get; set; }
        public string OfficeAddress1 { get; set; }
        public string OfficeAddress2 { get; set; }
        public string OfficeAddress3 { get; set; }
        public string OfficeAddress4 { get; set; }
        public string OfficeAddress5 { get; set; }
        public string OfficePostalCode { get; set; }
        public string OfficePhoneNumber { get; set; }
        public string MobileNumber { get; set; }
        public string CustomerNumber { get; set; } = string.Empty.LimitTo(20);
        public string NewNIC { get; set; }
        public string BillingAddressFlag { get; set; }
        public string AnniversaryDate { get; set; }
        public string CustomerOf { get; set; }
        public string PassportNo { get; set; }
        public string Nationality { get; set; }
        public string Reserved3 { get; set; } = string.Empty.LimitTo(40);
        public string Reserved4 { get; set; } = string.Empty.LimitTo(40);
        public string IBuID { get; set; }

        public string ResidentialAddress { get; set; }
        public string ResidentialAddress2 { get; set; }
        public string ResidentialAddress3 { get; set; }
        public string ResidentialAddress4 { get; set; }


        public string CustomerId { get; set; }

        public string FatherName { get; set; }
        public string EndSentinel { get; set; }
    }
    public class AccountData
    {
        public string RecordNumber { get; set; }
        public string RecordType { get; set; }
        public string ActionCode { get; set; }
        public string AccountNo { get; set; }
        public string AccountType { get; set; }
        public string Currency { get; set; }
        public string AccountTitle { get; set; }
        public string BankIMD { get; set; }
        public string BranchCode { get; set; }
        public string AccountNature { get; set; }
        public string DefaultAccount { get; set; }
        public string Withdrawal { get; set; }
        public string Purchase { get; set; }
        public string TransferTo { get; set; }
        public string TransferFrom { get; set; }
        public string MiniStatement { get; set; }
        public string BalanceInquiry { get; set; }
        public string ChqBookReq { get; set; }
        public string StmtReq { get; set; }
        public string PayBill { get; set; }
        public string ChequeDeposit { get; set; }
        public string CashDeposit { get; set; }
        public string SecondCurrency { get; set; }
        //public string ReservedPermissions { get; set; } = "NNNNNN".LimitTo(6);
        public string AccountMap { get; set; }
        public string ProcessingCode { get; set; }
        public string AccountStatus { get; set; } = "00".LimitTo(2);
        public string Reserved { get; set; } = string.Empty.LimitTo(30);
        public string CustomerId { get; set; }
    }

    public class CardDataNew //done
    {
        public string RecordNumber { get; set; }
        public string RecordType { get; set; }
        public string ActionCode { get; set; }
        public string CustomerId { get; set; }
        public string EmbossingName { get; set; }
        public string Customer0rStaff { get; set; }
        public string PrimaryCNIC { get; set; }
        public string ProductCode { get; set; }
        
        public string IssuanceType { get; set; }
    }

    public class CustomerProfileNew //done
    {
        
        public string RecordNumber { get; set; }
        public string RecordType { get; set; }
        public string ActionCode { get; set; }
        public string CustomerId { get; set; }
        public string Title { get; set; }
        public string FullName { get; set; }
        public string DateOfBirth { get; set; }
        public string MothersMaidenName { get; set; }
        public string ResidentialAddress { get; set; }
        public string ResidentialAddress2 { get; set; }
        public string ResidentialAddress3 { get; set; }
        public string ResidentialAddress4 { get; set; }
        public string ResidentialPostalCode { get; set; }
        public string ResidentialPhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Reserved { get; set; } = string.Empty.LimitTo(32);
        public string CompanyName { get; set; }
        public string OfficeAddress1 { get; set; }
        public string OfficeAddress2 { get; set; }
        public string OfficeAddress3 { get; set; }
        public string OfficeAddress4 { get; set; }
        public string OfficeAddress5 { get; set; }
        public string OfficePostalCode { get; set; }
        public string OfficePhoneNumber { get; set; }
        public string MobileNumber { get; set; }
        public string CustomerNumber { get; set; } = string.Empty.LimitTo(20);
        public string BillingAddressFlag { get; set; }
        public string AnniversaryDate { get; set; }
        public string PassportNo { get; set; }
        public string Nationality { get; set; }
        public string Reserved3 { get; set; } = string.Empty.LimitTo(36);
        public string Reserved4 { get; set; } = string.Empty.LimitTo(40);
        public string FatherName { get; set; }
        public string EndSentinel { get; set; }
    }
    public class AccountDataNew //done
    {
        public string RecordNumber { get; set; }
        public string RecordType { get; set; }
        public string ActionCode { get; set; }
        public string CustomerId { get; set; }
        public string AccountNo { get; set; }
        public string AccountType { get; set; }
        public string Currency { get; set; }
        public string AccountStatus { get; set; } = "00".LimitTo(2);
        public string AccountTitle { get; set; }
        
        public string BankIMD { get; set; }
        public string BranchCode { get; set; }
        public string AccountNature { get; set; }
        public string DefaultAccount { get; set; }

    }

    public class CustomerRelationship
    {
        public string RecordNumber { get; set; }
        public string RecordType { get; set; }
        public string ActionCode { get; set; }
        public string MobileNo { get; set; }
        public string NICOld { get; set; }
        public string NICNew { get; set; }
    }
}
