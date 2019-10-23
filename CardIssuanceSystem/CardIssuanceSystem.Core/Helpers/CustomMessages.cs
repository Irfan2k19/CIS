using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.Helpers
{
    public static class CustomMessages
    {
        public const string GenericErrorMessage = "Some error occured, please contact administrator.";
        public const string SomethingWentWrong = "Something went wrong.";
        public const string RequestAlreadyExists = "Request already exists in the system.";
        public const string AccountNotEligible = "Requested Account is not eligible";
        public const string CardAlreadyHot = "Card status is Hot";
        public const string Success = "Request Successfully Submited";
        public const string InActiveAccount = "Selected Account is InActive";
        public const string InsufficientBalance = "Insufficient Funds in selected Account";
        public const string DuplicateAccountCode = "Account Code already exists";
        public const string DuplicateCardCode = "Card Code already exists";
        public const string InvalidUsernameOrPassword = "Invalid username or password";
        public const string RegisteredSuccessfully = "Registered Successfully";
        public const string CardNotActive = "Card Status is not Active";
        public const string CardNotExists = "Card does not exist";
        public const string CardAlreadyExists = "Card Already Exists for this Customer";
        public const string CardAlreadyLinkedToThisAccount = "An existing card is already linked to this account";
        public const string AccountNotFound = "Account not found";
        public const string AccountCIF_combination = "Account Number and CIF combination is not correct";
        public const string Upgrade = "Card is not eligibile for the selected upgrade";
        public const string SingleAccountForCard = "Selected Customer does not have multiple accounts";
        public const string SingleAccountForCIF = "Selected Account Does not belong to this Customer";
        public const string MultipleCards = "Selected Account can not have multiple cards";
        public const string NoValueExist = "No Value Exists";
        public const string ImportRecordsRetrieveAndExport = "Records Retrieved: {0}, Records Exported: {1}";
        public const string ImportRecordsRetrieveAndImport = "Records Retrieved: {0}, Cards Created: {1}";

        public const string NonResident = "Account Holder is a Non-Resident";
        public const string InvalidIdentification = "Invalid Identification Type";//"No CNIC information found";
        public const string InvalidIdentificationNumber = "Invalid Identification Number";//"No CNIC Number found";

        public const string NoPassport = "No Passport information found";
        public const string NoPassportNumber = "No Passport Number found";
        public const string ForeignNational = "Account Holder is a Foreign National";
        public const string SectorInEligible = "Customer sector is ineligible for Card Issuance";
        public const string PostingRestriction = "There is a Posting Restriction on the selected Account";
        public const string OperatingInstruction = "Customer Operating Instruction is ineligible for this request";

        public const string MotherNameError = "Mother Name Not Found";
        public const string IdentificationTypeError = "Identification Instrument Not Found";
        public const string IdentificationNumberError = "Identification Number Not Found";
        public const string DOBError = "Date of Birth Not Found";
        public const string IncomeAccountError = "Income Account Not Found";
        public const string IncomeAccountSuccess = "Income Account Updated Successfully";
        public const string HotCardExists = "This account has a Hot card issued against it, please generate a replacement request";
        public const string LoginLimitExceed = "Login limit exceeded, please try again after 5 mins.";
        public const string AccountAlreadyLinked = "Account No {0} is already linked with the selected card.";
        public const string FilePathNotExists = "File Path does not exists in the system.";
        public const string FilePathAdded = "File Path has been added successfully.";
        public const string NoRecordFound = "No record found.";
        public const string RequestNotExists = "Request does not exist in the system";
        public const string AccountNumberEmptyOrLimitExceed = "Account number is empty or having length not equal to {0}";
        public const string CardNumberEmptyOrLimitExceed = "Card number is empty or having length not equal to {0}";
        public const string CustomerNumberEmptyOrLimitExceed = "Customer number is empty or having length not equal to {0}";

        public const string RecoveryError = "There was a problem running the Recovery process, please contact your administrator";
        public const string RecoverySuccess = "Recovery has been Processed Successfully";
        public const string RecoveryCount = "Charges Retrieved: {0} Charges Deducted: {1}";
        public const string CardNotEligible = "This card is not eligible for Replacement";
        public const string MultipleHotmarkApproval = "Cards Hotmarked : {0}, Cards Rejected: {1}";

        public const string PasswordNotCorrect = "Old Password is not Correct";
        public const string PasswordNotMatch = "Old Password and New Password not Match";
        public const string PasswordChanged = "Password Changed Successfully";
        public const string PasswordNotChanged = "Couldn't Change password";
        public const string UserNotExists = "User not exists into the system";
        public const string UserProfileUpdateSuccessfully = "User profile updated successfully";
        public const string CardChargesNotFound = "Card charges not found";
        public const string AuthorizedSucessfully = "Request Sucessfully Authorized";
        public const string SameCreatorAndsAuthorizer = "Creator and authorizer cannot be the same user";
        public const string NoDataFound = "No data found";
        public const string MultipleActivemarkApproval = "Cards Activated : {0}, Cards Rejected: {1}";
        public const string Reject = "Request Successfully Rejected";
    }
}
