using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.Helpers
{
    public class Constants
    {
        // public enum Response
        // {
        //     Successful="0000",
        //     InvalidAccount="0001",
        //     AccountNotAuthorized="0002",
        //     AccountDetailNotAuthorized="0003",
        //     AccountClosed="0004",
        //     AccountDormant="0005",
        //     AccountDeceased="0006",
        //     DebitFreeze="0007",
        //     CreditFreeze="0008",
        //     InvalidTransactionType="0009",
        //     InvalidVoucherType="0010",
        //     InvalidSystemCode="0011",
        //     InvalidGLAccount="0012",
        //     InSufficientBalance="0013",
        //     InvalidAmount="0014",
        //     InvalidInputParametersorValuesMissing="0015",
        //     InvalidDate="0016",
        //     DuplicateSTAN="0017",
        //     TransactionNotAllowed="0018",
        //     InstrumentHoldorCautionMarked="0019",
        //     InstrumentDormant="0020",
        //     InstrumentUnclaimed="0021",
        //     InstrumentAlreadyEncashed="0022",
        //     InstrumentCancel="0023",
        //     CustomerChequeDestroyedorLost="0024",



        //     CustomerChequeUtilized="0025",
        //     CustomerChequeStopped="0026",
        //     InstrumentStale="0027",
        //     SMSServiceNotAvailable="0028",
        //     InvalidMobileNumberFormatLength="0029",
        //     SMSServiceAlreadyActivated="0030",
        //     SMSServiceAlreadyInactive="0031"
        //     //SMS Service against multiple mobile number is not allowed="0032",
        //     //SMS Service is CUrrently Not Available For This Account="0033",
        //     //Same Mobile Number Entered="0034",
        //     //Same SMS Flag Entered="0035",
        //     //Instrument status is Report to SBP="0041",
        //     //Instrument status is Surrender to SBP="0042",
        //     //Instrument Hold Request Not Found="0043",
        //     //Signature Not Found="0044",
        //     //CHEQUE UN‐UTILIZED="0046",
        //     //Instrument has already been Utilized="0047",
        //     //Instrument has already been Stoped="0048",
        //     //Instrument has already been Released="0049",
        //     //Instrument has already been CHEQUE LODGED="0050",
        //     //Instrument has already been CHEQUE RETURN="0051",
        //     //Instrument has already been OPEN CHEQUE="0052",
        //     //Shadow Balance Going to be Less than Zero="0053",
        //     //Account Balance Going to be Less than Zero="0054",
        //     //Instrument is Hold by Another User="0055",
        //     //Instrument is Hold by Another Application="0056",
        //     //Instrument is marked with Status="0057",
        //     //Invalid Instrument="0058",

        //     //Authentication Failure="9001",
        //     //System Failure="9002",
        //     //Process Failure="9003",
        //     //Transaction Timeout="9004",
        //     //Transaction not foundx="9005",
        //     //Invalid Request or Request Not Found="9006",
        //     //Any unknown error occurred during web method call=" 9999",
        // }

        //public enum AccountStatus
        // {
        //  New="9999",
        //  Normal="0001",
        //  Dormant="0004",
        //  Unclaimed="0007",
        //  Closed="0008"
        // }

        #region Literals
        public static ActionCodeLiteral ActionCodes;
        public static RecordTypeLiteral RecordTypes;
        public static FormatLiteral Formats;
        public static RoleLiteral Roles;
        public static AppSettingsLiteral AppSettings;
        public static RequestTypesLiteral RequestTypes;
        public struct ActionCodeLiteral
        {
            public string Add { get { return "00"; } }
            public string Update { get { return "01"; } }
            public string Remove { get { return "02"; } }
            public string AccountDelink { get { return "03"; } }
            public string AccountDefaultFlagUpdate { get { return "09"; } }

            public string UpdateCardAndGenerateATMPIN { get { return "15"; } }
            public string UpdateCardAndGeneratePIN1IVR { get { return "16"; } }
            public string UpdateCardAndGeneratePIN2Mobile { get { return "17"; } }
            public string UpdateCardDetailsAndAllPins  { get { return "18"; } }
            public string CloneCard { get { return "19"; } }
            public string DebitCardAndPINProductionForCreditCard { get { return "20"; } }
        }
        public struct RecordTypeLiteral
        {
            public string CardData { get { return "DA"; } }
            public string CustomerProfile { get { return "CP"; } }
            public string AccountData { get { return "AC"; } }
            public string CustomerRelationship { get { return "CR"; } }
        }
        public struct FormatLiteral
        {
            public string DateFormat { get { return "yyyyMMdd"; } }
            public string TimeFormat { get { return "HHmmss"; } }
        }
        public struct RoleLiteral
        {
            public string Creator { get { return "c"; } }
            public string Authenticator { get { return "u"; } }
            public string Admin { get { return "a"; } }
        }

        public struct AppSettingsLiteral
        {
            public string SiteUrl { get { return Convert.ToString(ConfigurationManager.AppSettings["SiteUrl"]); } }
        }

        public struct RequestTypesLiteral
        {
            /// <summary>
            /// "N" - Card Issuance
            /// </summary>
            public string Issuance { get { return "N"; } }
            /// <summary>
            /// "A" - Card Ammendment
            /// </summary>
            public string Ammendment { get { return "A"; } }
            /// <summary>
            /// "U" - Card Upgradation
            /// </summary>
            public string Upgrade { get { return "U"; } }
            /// <summary>
            /// "R" - Card Replacement
            /// </summary>
            public string Replacement { get { return "R"; } }
            /// <summary>
            /// "L" - Card Linking
            /// </summary>
            public string Linking { get { return "L"; } }
            /// <summary>
            /// "D" - Card Delinking
            /// </summary>
            public string Delinking { get { return "D"; } }

            /// <summary>
            /// "D" - Card Delinking
            /// </summary>
            public string DefaultAccountUpdate { get { return "F"; } }

            /// <summary>
            /// "User" - User Modification
            /// </summary>
            public string UserModification { get { return "User"; } }
            /// <summary>
            /// "Profile_User" - User Profile
            /// </summary>
            public string UserProfile { get { return "Profile_User"; } }
            /// <summary>
            /// "Profile" - Profile Modification
            /// </summary>
            public string ProfileModification { get { return "Profile"; } }
            /// <summary>
            /// "Profile_Page" - Profile Page
            /// </summary>
            public string ProfilePage { get { return "Profile_Page"; } }

            public string CardCharges_SystemRequest { get { return "CardCharges"; } }

            public string CardTypes_SystemRequest { get { return "CardTypes"; } }

            public string AccountTypes_SystemRequest { get { return "AccountTypes"; } }
        }

        #endregion

        #region Enumerations
        public enum enPathType
        {
            Import = 1,
            Export
        }

        #endregion

    }
}
