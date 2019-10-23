using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.Core.Methods;
using CardIssuanceSystem.DAL.DataAccessClasses;
using CardIssuanceSystem.DAL;
using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.Filters;
using System.Text.RegularExpressions;

namespace CardIssuanceSystem.Controllers
{
    public class RequestController : BaseController
    {
        #region Card Issuance
        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult CardIssuanceScreen(int? Id)
        {
            CardChargesDataAccess cc = new CardChargesDataAccess();
            List<tbl_Card_Charges> lst = new List<tbl_Card_Charges>();

            UpdateRequestVM viewModel = new UpdateRequestVM();
            CardTypesDataAccess obj = new CardTypesDataAccess();

            if (Id.HasValue)
            {
                viewModel.RequestData = GetRequestToBeReviewed(Id, "N");
                CustomerRequestAccountDataAccess crad = new CustomerRequestAccountDataAccess();
                var RequestAccount = crad.GetRequestCustomerAccountByRequestId(Id ?? 0);

                var MainAddress = AuthorizationController.OtherAddressMapping(RequestAccount?.MainAddress ?? string.Empty);
                var SetAddress = "";
                ViewBag.MainAddress = MainAddress;

                if (MainAddress == "Other")
                {
                    SetAddress = RequestAccount?.MainAddress ?? string.Empty;
                }
                ViewBag.SetAddress = SetAddress;

                /*mobile*/
                var MainMobile = AuthorizationController.OtherMobileNoMapping(RequestAccount?.MainMobile ?? string.Empty);
                var SetMobileNo = "";
                ViewBag.MainMobile = MainMobile;

                if (MainMobile == "Other")
                {
                    SetMobileNo = RequestAccount?.MainMobile ?? string.Empty;
                }
                ViewBag.SetMobileNo = SetMobileNo;

                /*landline*/
                var MainLandlineNo = AuthorizationController.OtherLandlineNoMapping(RequestAccount?.LandlineNo ?? string.Empty);
                var SetLandlineNo = "";
                ViewBag.MainLandlineNo = MainLandlineNo;

                if (MainLandlineNo == "Other")
                {
                    SetLandlineNo = RequestAccount?.LandlineNo ?? string.Empty;
                }
                ViewBag.SetLandlineNo = SetLandlineNo;

                /*office*/
                var MainOfficeNo = AuthorizationController.OtherOfficeNoMapping(RequestAccount?.PhoneOffice ?? string.Empty);
                var SetOfficeNo = "";
                ViewBag.MainOfficeNo = MainOfficeNo;

                if (MainOfficeNo == "Other")
                {
                    SetOfficeNo = RequestAccount?.PhoneOffice ?? string.Empty;
                }
                ViewBag.SetOfficeNo = SetOfficeNo;
                /**/
                //ViewBag.MainMobile = RequestAccount?.MainMobile ?? string.Empty;
                ViewBag.MainLandline = RequestAccount?.MainLandline ?? string.Empty;

                if (viewModel.RequestData == null)
                    return RedirectToAction("ReviewCardIssuanceScreen", "Review");
            }

            ViewBag.RequestId = Id ?? 0;
            ViewBag.RequestCustomerInfo = Id.HasValue ? new CustomerRequestAccountDataAccess().GetRequestCustomerAccountByRequestId(Id ?? 0) : null;
            ViewBag.CardTypes = obj.GetCardTypes("A", "N");
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "u", "c", "a" })]
        public ActionResult SearchAccount(string AccountNo, string CIF)
        {
            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            CustomerAccountVM lst = new CustomerAccountVM();
            CustomerCardDataAccess obj = new CustomerCardDataAccess();
            //var CardExist = obj.GetCustomerCardByAccountNo(AccountNo);
            // var CardExist = obj.GetCustomerCardByAccountNoCIF(AccountNo,CIF);
            /*[09/05/2018: Issue occurs because of joining customer card and account table to get customer cards details of user, else we dont need to join, as because sometimes the user has cardno value in customer cards but not in accounts table]*/
            var CardExist = CommonMethods.GetCustomerCardsByAccountNo(AccountNo);/*CommonMethods.GetCustomerCardByAccountNo(AccountNo);*/
            if (CardExist.Count <= 0)
            {
                resp = T24Methods.FetchAccount(AccountNo, CIF);
                var branchCode = string.IsNullOrEmpty(resp.BranchCode) ? string.Empty : resp.BranchCode.Substring(resp.BranchCode.Length - 4, 4);
                resp.T24ACNO = resp.T24ACNO.Length < 15 ? branchCode + resp.T24ACNO : resp.T24ACNO;

                resp.CustomerDOB = resp.CustomerDOB.ToValidDateTime("dd-MM-yyyy");
                resp.Sector = new SectorDataAccess().GetSectorbyCode(resp.Sector)?.Description ?? string.Empty;
                resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(resp.Product)?.Name ?? string.Empty;
                //resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(resp.OpInstructions)?.Description ?? string.Empty;
                resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
                resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
                resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
                if (string.IsNullOrEmpty(resp.T24ACNO) || string.IsNullOrWhiteSpace(resp.AccountTitle))
                    return Json(new { Result = resp, ErrorMessage = CustomMessages.AccountNotFound }, JsonRequestBehavior.AllowGet);

                return Json(new { Result = resp, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }

            else
            {
                var CIFExist = CardExist.Where(x => x.CIFNo == CIF);
                if (CIFExist.Count() <= 0)
                {
                    resp = T24Methods.FetchAccount(AccountNo, CIF);
                    resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
                    resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
                    resp.CustomerDOB = resp.CustomerDOB.ToValidDateTime("dd-MM-yyyy");
                    resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
                    if (string.IsNullOrEmpty(resp.T24ACNO) || string.IsNullOrWhiteSpace(resp.AccountTitle))
                    {
                        return Json(new { Result = resp, ErrorMessage = CustomMessages.AccountNotFound }, JsonRequestBehavior.AllowGet);
                    }
                    //change 2may18: op inst not required to check at search account,if invalid op instr then it will be stopped at submit
                    else
                    {
                        return Json(new { Result = resp, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
                    }
                    /*
                    if (resp.OpInstructions=="")
                    {
                        return Json(new { Result = resp, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
                    }else
                    {
                        return Json(new { Result = resp, ErrorMessage = CustomMessages.CardAlreadyExists }, JsonRequestBehavior.AllowGet);
                    }
                    */
                }
                else if (CIFExist.Any(x => x.CardStatusActive == true))
                {
                    return Json(new { Result = resp, ErrorMessage = CustomMessages.CardAlreadyExists }, JsonRequestBehavior.AllowGet);
                }
                else if (CIFExist.Any(x => x.CardStatusActive == false))
                {
                    return Json(new { Result = resp, ErrorMessage = CustomMessages.HotCardExists }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Result = resp, ErrorMessage = CustomMessages.MultipleCards }, JsonRequestBehavior.AllowGet);
                }

            }

            //return Json(resp, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchAccount22(string AccountNo, string CIF)
        {
            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            CustomerAccountVM lst = new CustomerAccountVM();
            CustomerCardDataAccess obj = new CustomerCardDataAccess();

                resp = T24Methods.FetchAccount(AccountNo, CIF);

            resp.CustomerDOB = resp.CustomerDOB.ToValidDateTime("dd-MM-yyyy");
                resp.Sector = new SectorDataAccess().GetSectorbyCode(resp.Sector)?.Description ?? string.Empty;
                resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(resp.Product)?.Name ?? string.Empty;
            //resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(resp.OpInstructions)?.Description ?? string.Empty;
            
            resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
                resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;

               resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
            if (string.IsNullOrEmpty(resp.T24ACNO) || string.IsNullOrWhiteSpace(resp.AccountTitle))
                    return Json(new { Result = resp, ErrorMessage = CustomMessages.AccountNotFound }, JsonRequestBehavior.AllowGet);

                var CardHistory=RequestMethods.GetCustomerCardByAccountNoCIF(AccountNo,CIF);
                
               
                return Json(new { Result = resp, CardHistory = CardHistory,ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);


            //return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "c" })]
        [HttpPost]
        public JsonResult SubmitNewRequest(RequestVM request)
        {
            //change audit trail report
            string AddressValue = ""; string MobileValue = "";
            string LandlineValue = ""; string OfficePhoneValue = "";
            //

            long ID = 0;
            if (IsDuplicateEntry(request.CardNo, request.AccountNo, request.RequestType, request.CIFNo, out ID))
                return Json(new { IsSuccess = ID, ErrorMessage = CustomMessages.RequestAlreadyExists }, JsonRequestBehavior.AllowGet);

            if (request.RequestType == "R" && (new CustomerCardDataAccess().IsCustomerCardByAccountNoExists(request.AccountNo,request.CIFNo, request.CardNo)))
            {
                return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.CardAlreadyLinkedToThisAccount }, JsonRequestBehavior.AllowGet);
            }

            long? success = 0;
            CardChargesDataAccess cc = new CardChargesDataAccess();
            List<tbl_Card_Charges> lst = new List<tbl_Card_Charges>();
            //request.AccountTypeId = Convert.ToInt32("001");
            //request.CardTypeId = 2;
            //request.BranchCode = "001";
            //string Frequency = "12";
            string Frequency = "0";
            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            resp = T24Methods.FetchAccount(request.AccountNo, request.CustomerId);
            resp.BranchCode = string.IsNullOrEmpty(resp.BranchCode) ? string.Empty : resp.BranchCode.Substring(resp.BranchCode.Length - 4, 4);//request.AccountNo.Substring(0, 4);
            request.AccountNo = request.AccountNo.Length < 15 ? resp.BranchCode + request.AccountNo : request.AccountNo;
            string AccountStatus = resp.AccountStatus;
            decimal AvailBalance = Convert.ToDecimal((!string.IsNullOrEmpty(resp.AvailableBalance)) ? resp.AvailableBalance : "0");

            resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
            string IsReplacement = "0";
            if (request.RequestType=="R")
            {
                IsReplacement = "1";
            }
            lst = cc.GetCardCharges(resp.AccountNature, request.CardTypeId, resp.BranchCode, Frequency, IsReplacement);
            //var TotalCharges = lst.Sum(item => item.Amount);
            if (lst.Count > 0)
            {
                //bool StatusEligibility = CommonMethods.CheckStatusEligibility(AccountStatus);
                EligibilityVM elg = new EligibilityVM();
                elg.IdentificationType = resp.IdentificationType;
                elg.IdentificationNumber = resp.IdentificationNo;
                elg.Resident = resp.ResidenceStatus;
                elg.Nationality = resp.Nationality;
                elg.SectorCode = resp.Sector;
                elg.AccountStatus = resp.AccountStatus;
                elg.FatherName = resp.FathersName;
                elg.MotherName = resp.MothersName;
                elg.DOB = resp.CustomerDOB;
                elg.PostingRestriction = resp.PostingRestriction;
                elg.OpInstruction = resp.OpInstructions;
                elg.AccountNo = request.AccountNo ;
                List<string> StatusEligibility = CommonMethods.IsEligible(elg);

                if (StatusEligibility.Count <= 0)
                {
                    //lst = cc.GetCardCharges("001", request.CardTypeId, request.BranchCode, Frequency);

                    var TotalAmount = lst.Sum(item => item.Amount);
                    if (request.Waive == false)
                    {
                        //Change for Free Products on 31 January 2019
                        bool BalanceEligibility = true;
                        if (TotalAmount > 0)
                        {
                            BalanceEligibility = CommonMethods.CheckBalanceEligibility(AvailBalance, TotalAmount);
                        }
                        //bool BalanceEligibility = CommonMethods.CheckBalanceEligibility(AvailBalance, TotalAmount);
                        if (BalanceEligibility == true)
                        {
                            request.AuthorizationStatus = "P";
                            request.StatusEligibility = true;
                            request.FinancialEligibility = BalanceEligibility;
                            request.CustomerId = resp.CustomerID;
                            success = RequestMethods.AddNewRequest(request);
                            //mk change
                            if (request.RequestType == "U")
                                UpdateUserLog(success.GetValueOrDefault(), CustomHelper.SaveConvertToInt32(request.ExistingCardType, 0));

                            if (request.RequestType == "N" || request.RequestType == "R" || request.RequestType == "A")
                            {
                                var MainAddress = AuthorizationController.FieldMappingAddress(request.MainAddress, resp);
                                MainAddress = !string.IsNullOrEmpty(MainAddress)? MainAddress.Replace("\n", " "):" ";
                                var MainMobile = AuthorizationController.FieldMappingMobile(request.MainMobile, resp);
                                MainMobile = !string.IsNullOrEmpty(MainMobile)?MainMobile.Replace("\n", " "):" ";
                                var MainLandline = AuthorizationController.FieldMappingLandline(request.MainLandline, resp);
                                MainLandline = !string.IsNullOrEmpty(MainLandline)?MainLandline.Replace("\n"," "):" ";
                                var MainLandlineNo = AuthorizationController.FieldMappingLandlineNo(request.LandlineNo, resp);
                                MainLandlineNo = !string.IsNullOrEmpty(MainLandlineNo)?MainLandlineNo.Replace("\n", " "):string.Empty;
                                var MainOfficeNo = AuthorizationController.FieldMappingOfficeNo(request.PhoneOff, resp);
                                MainOfficeNo = !string.IsNullOrEmpty(MainOfficeNo)?MainOfficeNo.Replace("\n", " "):string.Empty;

                                AddressValue = MainAddress;
                                MobileValue = MainMobile;
                                LandlineValue = MainLandline;
                                OfficePhoneValue = MainOfficeNo;

                                AccountExists(request.AccountNo, resp, MainAddress, MainMobile, MainLandline, MainLandlineNo, MainOfficeNo, request.CIFNo, request.CardNumber);
                            }
                            RequestMethods.AddRequestCustomerAccount(request, AddressValue, MobileValue, LandlineValue, OfficePhoneValue);


                            return Json(new { IsSuccess = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            request.AuthorizationStatus = "C";
                            request.StatusEligibility = false;
                            request.FinancialEligibility = BalanceEligibility;
                            request.CustomerId = resp.CustomerID;
                            success = RequestMethods.AddNewRequest(request);
                            /*Update user log for exceptions*/
                            AddException(success, CustomMessages.InsufficientBalance);
                            // mk change
                            RequestMethods.AddRequestCustomerAccount(request, AddressValue, MobileValue, LandlineValue, OfficePhoneValue);
                            return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.InsufficientBalance }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        request.AuthorizationStatus = "P";
                        request.StatusEligibility = false;
                        //request.FinancialEligibility = BalanceEligibility;
                        request.CustomerId = resp.CustomerID;
                        success = RequestMethods.AddNewRequest(request);
                        if (request.RequestType == "U")
                            UpdateUserLog(success.GetValueOrDefault(), CustomHelper.SaveConvertToInt32(request.ExistingCardType, 0));

                        if (request.RequestType == "N" || request.RequestType == "R" || request.RequestType == "A")
                        {
                            var MainAddress = AuthorizationController.FieldMappingAddress(request.MainAddress, resp);
                            MainAddress = !string.IsNullOrEmpty(MainAddress) ? MainAddress.Replace("\n", " ") : " ";
                            var MainMobile = AuthorizationController.FieldMappingMobile(request.MainMobile, resp);
                            MainMobile = !string.IsNullOrEmpty(MainMobile) ? MainMobile.Replace("\n", " ") : " ";
                            var MainLandline = AuthorizationController.FieldMappingLandline(request.MainLandline, resp);
                            MainLandline = !string.IsNullOrEmpty(MainLandline) ? MainLandline.Replace("\n", " ") : " ";
                            var MainLandlineNo = AuthorizationController.FieldMappingLandlineNo(request.LandlineNo, resp);
                            MainLandlineNo = !string.IsNullOrEmpty(MainLandlineNo) ? MainLandlineNo.Replace("\n", " ") : string.Empty;
                            var MainOfficeNo = AuthorizationController.FieldMappingOfficeNo(request.PhoneOff, resp);
                            MainOfficeNo = !string.IsNullOrEmpty(MainOfficeNo) ? MainOfficeNo.Replace("\n", " ") : string.Empty;

                            AddressValue = MainAddress;
                            MobileValue = MainMobile;
                            LandlineValue = MainLandline;
                            OfficePhoneValue = MainOfficeNo;

                            AccountExists(request.AccountNo, resp, MainAddress, MainMobile, MainLandline, MainLandlineNo, MainOfficeNo, request.CIFNo, request.CardNumber);
                        }
                        RequestMethods.AddRequestCustomerAccount(request, AddressValue, MobileValue, LandlineValue, OfficePhoneValue);


                        return Json(new { IsSuccess = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {

                    request.AuthorizationStatus = "C";
                    request.StatusEligibility = false;
                    request.CustomerId = resp.CustomerID;
                    success = RequestMethods.AddNewRequest(request);
                    string ErrorString = string.Join(",", StatusEligibility.ToArray());
                    /*Update user log for exceptions*/
                    AddException(success, ErrorString);

                    RequestMethods.AddRequestCustomerAccount(request,AddressValue,MobileValue,LandlineValue,OfficePhoneValue);
                    return Json(new { IsSuccess = success, ErrorMessage = ErrorString }, JsonRequestBehavior.AllowGet);
                }
                //
            }
            else 
            {
                request.AuthorizationStatus = "C";
                request.CustomerId = resp.CustomerID;
                request.StatusEligibility = false;
                success = RequestMethods.AddNewRequest(request);
                /*Update user log for exceptions*/
                AddException(success, CustomMessages.AccountNotEligible);

                RequestMethods.AddRequestCustomerAccount(request, AddressValue, MobileValue, LandlineValue, OfficePhoneValue);
                return Json(new
                {
                    IsSuccess = success,
                    ErrorMessage = CustomMessages.AccountNotEligible
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AuthorizeCardIssuanceScreen()
        {
            List<AuthorizationVM> viewModel = RequestMethods.GetRequestDetails("N");
            return View(viewModel);
        }
        #endregion Card Issuance


        #region Card Replacement

        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult CardReplacementScreen(int? Id)
        {
            UpdateRequestVM viewModel = new UpdateRequestVM();
            //ViewBag.RequestId = requestId ?? 0;
            CardTypesDataAccess obj = new CardTypesDataAccess();
            if (Id.HasValue)
            {
                viewModel.RequestData = GetRequestToBeReviewed(Id, "R");
                CustomerRequestAccountDataAccess crad = new CustomerRequestAccountDataAccess();
                var RequestAccount = crad.GetRequestCustomerAccountByRequestId(Id ?? 0);

                var MainAddress = AuthorizationController.OtherAddressMapping(RequestAccount?.MainAddress??string.Empty);
                var SetAddress = "";
                ViewBag.MainAddress = MainAddress;

                if (MainAddress == "Other")
                {
                    SetAddress = RequestAccount?.MainAddress??string.Empty;
                }
                ViewBag.SetAddress = SetAddress;

                /*mobile*/
                var MainMobile = AuthorizationController.OtherMobileNoMapping(RequestAccount?.MainMobile ?? string.Empty);
                var SetMobileNo = "";
                ViewBag.MainMobile = MainMobile;

                if (MainMobile == "Other")
                {
                    SetMobileNo = RequestAccount?.MainMobile ?? string.Empty;
                }
                ViewBag.SetMobileNo = SetMobileNo;

                /*landline*/
                var MainLandlineNo = AuthorizationController.OtherLandlineNoMapping(RequestAccount?.LandlineNo ?? string.Empty);
                var SetLandlineNo = "";
                ViewBag.MainLandlineNo = MainLandlineNo;

                if (MainLandlineNo == "Other")
                {
                    SetLandlineNo = RequestAccount?.LandlineNo ?? string.Empty;
                }
                ViewBag.SetLandlineNo = SetLandlineNo;

                /*office*/
                var MainOfficeNo = AuthorizationController.OtherOfficeNoMapping(RequestAccount?.PhoneOffice ?? string.Empty);
                var SetOfficeNo = "";
                ViewBag.MainOfficeNo = MainOfficeNo;

                if (MainOfficeNo == "Other")
                {
                    SetOfficeNo = RequestAccount?.PhoneOffice ?? string.Empty;
                }
                ViewBag.SetOfficeNo = SetOfficeNo;
                /**/
                //ViewBag.MainMobile = RequestAccount?.MainMobile??string.Empty;
                ViewBag.MainLandline = RequestAccount?.MainLandline ?? string.Empty;
                if (viewModel.RequestData == null)
                    return RedirectToAction("ReviewCardReplacementScreen", "Review");
            }

            ViewBag.RequestId = Id ?? 0;
            ViewBag.RequestCustomerInfo = Id.HasValue ? new CustomerRequestAccountDataAccess().GetRequestCustomerAccountByRequestId(Id ?? 0) : null;
            ViewBag.CardTypes = obj.GetCardTypes("A", "R");
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "c", "u", "a" })]
        public ActionResult SearchAccountByCardNo(string CardNo)
        {
            List<CustomerCardVM> lst = new List<CustomerCardVM>();
            CustomerAccountDataAccess obj = new CustomerAccountDataAccess();
            lst = CommonMethods.GetCardInfo(CardNo);

            //ViewBag.LinkedAccount=CommonMethods.GetAccountInfo(CardNo);
            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();

            if (lst.Count > 0)
            {
                var parentIds = new CardTypesDataAccess().GetParentCardTypeIds(lst.FirstOrDefault()?.CardTypeID ?? 0);
                lst.FirstOrDefault().ParentIds = parentIds;
                if (lst.FirstOrDefault().CardStatusActive == true)
                {
                    resp = T24Methods.FetchAccount(lst.FirstOrDefault().AccountNo, lst.FirstOrDefault().CIFNo);
                    var branchCode = string.IsNullOrEmpty(resp.BranchCode) ? string.Empty : resp.BranchCode.Substring(resp.BranchCode.Length - 4, 4);
                    resp.T24ACNO = resp.T24ACNO.Length < 15 ? branchCode + resp.T24ACNO : resp.T24ACNO;

                    resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(resp.Product)?.Name ?? string.Empty;
                    resp.Sector = new SectorDataAccess().GetSectorbyCode(resp.Sector)?.Description ?? string.Empty;
                    //resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(resp.OpInstructions)?.Description ?? string.Empty;
                    resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
                    resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
                    resp.CustomerDOB = resp.CustomerDOB.ToValidDateTime("dd-MM-yyyy");
                    resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
                    if (string.IsNullOrEmpty(resp.AccountStatus))
                    {
                        return Json(new { AccountInfo = resp, CardInfo = lst, Message = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { AccountInfo = resp, CardInfo = lst, Message = CustomMessages.InActiveAccount }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    resp = T24Methods.FetchAccount(lst.FirstOrDefault().AccountNo, lst.FirstOrDefault().CIFNo);
                    resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(resp.Product)?.Name ?? string.Empty;
                    resp.Sector = new SectorDataAccess().GetSectorbyCode(resp.Sector)?.Description ?? string.Empty;
                    //resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(resp.OpInstructions)?.Description ?? string.Empty;
                    resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
                    resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
                    resp.CustomerDOB = resp.CustomerDOB.ToValidDateTime("dd-MM-yyyy");
                    resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
                    return Json(new { AccountInfo = resp, CardInfo = lst, Message = CustomMessages.CardNotActive }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { AccountInfo = resp, CardInfo = lst, Message = CustomMessages.CardNotExists }, JsonRequestBehavior.AllowGet);
            }
        }


        [AuthOp(RoleTitle = new string[] { "c", "u", "a" })]
        public ActionResult SearchAccountByAccountNoAndCIF(string AccountNo, string CIF)
        {
            List<CustomerCardVM> lst = new List<CustomerCardVM>();
            CustomerAccountDataAccess obj = new CustomerAccountDataAccess();
            string cno = new CustomerCardDataAccess().CardReplacementValid(AccountNo, CIF);

            //ViewBag.LinkedAccount=CommonMethods.GetAccountInfo(CardNo);
            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();

            if (cno == "0")
            {
                return Json(new { AccountInfo = resp, CardInfo = lst, Message = CustomMessages.CardNotEligible }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                lst = CommonMethods.GetCardInfo(cno);
                if (lst.Count > 0)
                {
                    var parentIds = new CardTypesDataAccess().GetParentCardTypeIds(lst.FirstOrDefault()?.CardTypeID ?? 0);

                    lst.FirstOrDefault().ParentIds = parentIds;

                    resp = T24Methods.FetchAccount(lst.FirstOrDefault().AccountNo, lst.FirstOrDefault().CIFNo);
                    var branchCode = string.IsNullOrEmpty(resp.BranchCode) ? string.Empty : resp.BranchCode.Substring(resp.BranchCode.Length - 4, 4);
                    resp.T24ACNO = resp.T24ACNO.Length < 15 ? branchCode + resp.T24ACNO : resp.T24ACNO;

                    resp.Product = new AccountTypeDataAccess().GetAccountTypeByCode(resp.Product)?.Name ?? string.Empty;
                    resp.Sector = new SectorDataAccess().GetSectorbyCode(resp.Sector)?.Description ?? string.Empty;
                    //resp.OpInstructions = new OperatingInstructionDataAccess().GetOperatingInstructionsbyCode(resp.OpInstructions)?.Description ?? string.Empty;
                    resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
                    resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
                    resp.CustomerDOB = resp.CustomerDOB.ToValidDateTime("dd-MM-yyyy");
                    resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
                    return Json(new { AccountInfo = resp, CardInfo = lst, Message = CustomMessages.CardNotActive }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { AccountInfo = resp, CardInfo = lst, Message = CustomMessages.CardNotExists }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [AuthOp(RoleTitle = new string[] { "c" })]
        [HttpPost]
        public JsonResult SubmitReplacementRequest(RequestVM request)
        {
            //change audit trail report
            string AddressValue = ""; string MobileValue = "";
            string LandlineValue = ""; string OfficePhoneValue = "";
            //

            long ID = 0;
            if (IsDuplicateEntry(request.CardNo, request.AccountNo, request.RequestType, request.CIFNo, out ID))
                return Json(new { IsSuccess = ID, ErrorMessage = CustomMessages.RequestAlreadyExists }, JsonRequestBehavior.AllowGet);

            if (request.RequestType == "R" && (new CustomerCardDataAccess().IsCustomerCardByAccountNoExists(request.AccountNo,request.CIFNo, request.CardNo)))
            {
                return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.CardAlreadyLinkedToThisAccount }, JsonRequestBehavior.AllowGet);
            }

            long? success = 0;
            CardChargesDataAccess cc = new CardChargesDataAccess();
            List<tbl_Card_Charges> lst = new List<tbl_Card_Charges>();

            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            ////resp = T24Methods.FetchAccount(request.AccountNo);
            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            //change mk(need to remove comment)
            resp = T24Methods.FetchAccount(request.AccountNo, request.CIFNo);
            resp.BranchCode = string.IsNullOrEmpty(resp.BranchCode) ? string.Empty : resp.BranchCode.Substring(resp.BranchCode.Length - 4, 4); //request.AccountNo.Substring(0, 4);
            request.AccountNo = request.AccountNo.Length < 15 ? resp.BranchCode + request.AccountNo : request.AccountNo;
            //request.AccountTypeId = 001;
            //request.CardTypeId = 2;
            //request.BranchCode = "001";
            //string Frequency = "6";
            string Frequency = "0";
            //lst = cc.GetCardCharges("001", request.CardTypeId, request.BranchCode, Frequency);
            resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
            string IsReplacement = "1";
            lst = cc.GetCardCharges(resp.AccountNature, request.CardTypeId, resp.BranchCode, Frequency,IsReplacement);
            // mk chnage (need to remove the line)
            //lst= new List<tbl_Card_Charges>();
            if (lst.Count > 0)
            {
                var TotalAmount = lst.Sum(item => item.Amount);
                string AccountStatus = resp.AccountStatus;
                decimal AvailBalance =Convert.ToDecimal((!string.IsNullOrEmpty(resp.AvailableBalance)) ? resp.AvailableBalance : "0");
                //bool StatusEligibility = CommonMethods.CheckStatusEligibility(AccountStatus);
                EligibilityVM elg = new EligibilityVM();
                elg.IdentificationType = resp.IdentificationType;
                elg.IdentificationNumber = resp.IdentificationNo;
                elg.Resident = resp.ResidenceStatus;
                elg.Nationality = resp.Nationality;
                elg.SectorCode = resp.Sector;
                elg.AccountStatus = resp.AccountStatus;
                elg.FatherName = resp.FathersName;
                elg.MotherName = resp.MothersName;
                elg.DOB = resp.CustomerDOB;
                elg.PostingRestriction = resp.PostingRestriction;
                elg.OpInstruction = resp.OpInstructions;
                elg.AccountNo = request.AccountNo;
                List<string> StatusEligibility = CommonMethods.IsEligible(elg);
                //Change for Free Products 31 January 2019
                bool BalanceEligibility = true;
                if (TotalAmount > 0)
                {
                    BalanceEligibility = CommonMethods.CheckBalanceEligibility(AvailBalance, TotalAmount);
                }

                //bool BalanceEligibility = CommonMethods.CheckBalanceEligibility(AvailBalance, TotalAmount);
                if (request.Waive == false && StatusEligibility.Count<=0 && BalanceEligibility == true)
                {
                    request.RequestType = "R";
                    request.AuthorizationStatus = "P";
                    request.StatusEligibility = true;
                    request.CustomerId = resp.CustomerID;
                    success = RequestMethods.AddReplacementRequest(request);

                    var MainAddress = AuthorizationController.FieldMappingAddress(request.MainAddress, resp);
                    MainAddress = !string.IsNullOrEmpty(MainAddress) ? MainAddress.Replace("\n", " ") : string.Empty;
                    var MainMobile = AuthorizationController.FieldMappingMobile(request.MainMobile, resp);
                    MainMobile = !string.IsNullOrEmpty(MainMobile) ? MainMobile.Replace("\n", " ") : string.Empty;
                    var MainLandline = AuthorizationController.FieldMappingLandline(request.MainLandline, resp);
                    MainLandline = !string.IsNullOrEmpty(MainLandline) ? MainLandline.Replace("\n", " ") : string.Empty;
                    var MainLandlineNo = AuthorizationController.FieldMappingLandlineNo(request.LandlineNo, resp);
                    MainLandlineNo = !string.IsNullOrEmpty(MainLandlineNo) ? MainLandlineNo.Replace("\n", " ") : string.Empty;
                   

                    AccountExists(request.AccountNo, resp, MainAddress, MainMobile,MainLandline, MainLandline, request.CIFNo, request.CardNumber);
                }
                else if (request.Waive == true && StatusEligibility.Count<=0)
                {
                    request.RequestType = "R";
                    request.AuthorizationStatus = "P";
                    request.StatusEligibility = true;
                    request.CustomerId = resp.CustomerID;
                    success = RequestMethods.AddReplacementRequest(request);

                    var MainAddress = AuthorizationController.FieldMappingAddress(request.MainAddress, resp);
                    MainAddress = !string.IsNullOrEmpty(MainAddress) ? MainAddress.Replace("\n", " ") : string.Empty;
                    var MainMobile = AuthorizationController.FieldMappingMobile(request.MainMobile, resp);
                    MainMobile = !string.IsNullOrEmpty(MainMobile) ? MainMobile.Replace("\n", " ") : string.Empty;
                    var MainLandline = AuthorizationController.FieldMappingLandline(request.MainLandline, resp);
                    MainLandline = !string.IsNullOrEmpty(MainLandline) ? MainLandline.Replace("\n", " ") : string.Empty;
                    var MainLandlineNo = AuthorizationController.FieldMappingLandlineNo(request.LandlineNo, resp);
                    MainLandlineNo = !string.IsNullOrEmpty(MainLandlineNo) ? MainLandlineNo.Replace("\n", " ") : string.Empty;

                    
                    AccountExists(request.AccountNo, resp, MainAddress, MainMobile,MainLandline, MainLandline, request.CIFNo, request.CardNumber);
                }

                else
                {
                    request.RequestType = "R";
                    request.AuthorizationStatus = "C";
                    request.StatusEligibility = false;
                    request.CustomerId = resp.CustomerID;
                    success = RequestMethods.AddReplacementRequest(request);
                    var ErrorString = string.Join(",", StatusEligibility.ToArray());
                    /*Update user log for exceptions*/
                    AddException(success, ErrorString);

                }
            }
            else
            {
                request.RequestType = "R";
                request.AuthorizationStatus = "C";
                request.StatusEligibility = false;
                request.CustomerId = resp.CustomerID;
                success = RequestMethods.AddReplacementRequest(request);
                /*Update user log for exceptions*/
                AddException(success, CustomMessages.CardChargesNotFound);

            }
            return Json(success, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AuthorizeCardReplacementScreen()
        {
            List<AuthorizationVM> viewModel = RequestMethods.GetRequestDetails("R");
            return View(viewModel);
        }


        

        #endregion Card Replacement


        #region Linking Screen

        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult LinkingScreen(int? Id)
        {
            UpdateRequestVM viewModel = new UpdateRequestVM();
            CardTypesDataAccess obj = new CardTypesDataAccess();
            if (Id.HasValue)
            {
                viewModel.RequestData = GetRequestToBeReviewed(Id, "L");
                if (viewModel.RequestData == null)
                    return RedirectToAction("ReviewLinkingScreen", "Review");
            }

            
            ViewBag.CardTypes = obj.GetCardTypes("A");
            ViewBag.RequestId = Id ?? 0;
            return View(viewModel);
        }


        [AuthOp(RoleTitle = new string[] { "u", "c", "a" })]
        public ActionResult SearchAccountforLinking(string[] Accounts, string CIF)
        {
            CustomerAccountVM lst = new CustomerAccountVM();
            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var item in Accounts)
                {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    //resp = T24Methods.FetchAccount(item);
                    resp = T24Methods.FetchAccount(item, CIF);
                    var branchCode = string.IsNullOrEmpty(resp.BranchCode) ? string.Empty : resp.BranchCode.Substring(resp.BranchCode.Length - 4, 4);
                    resp.T24ACNO = resp.T24ACNO.Length < 15 ? branchCode + resp.T24ACNO : resp.T24ACNO;
                    resp.Nationality = !string.IsNullOrEmpty(resp.Nationality) ? resp.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
                    resp.ResidenceStatus = !string.IsNullOrEmpty(resp.ResidenceStatus) ? resp.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
                    resp.CustomerDOB = resp.CustomerDOB.ToValidDateTime("dd-MM-yyyy");
                    resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
                    if (resp.ErrorField != "Customer Does not belong to this Account")
                    {
                        result.Add(item, resp.AccountStatus!="Y" ? "Active": "InActive/Different CIF/Not Found");

                        

                    }
                    else
                    {
                        result.Add(item, "InActive/Different CIF/Not Found");
                    }

                    //result.Add(item, resp.AccountStatus != "" ? "InActive/Different CIF" : "Active");


                }

            }
            if (result.ContainsValue("InActive/Different CIF/Not Found"))
            {
                return Json(new { IsSuccess = false, Result = result }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { IsSuccess = true, Result = result }, JsonRequestBehavior.AllowGet);
            }
            //string json = JsonConvert.SerializeObject(result, Formatting.Indented);
            //return Json(result, JsonRequestBehavior.AllowGet);

        }


        [AuthOp(RoleTitle = new string[] { "c" })]
        [HttpPost]
        public JsonResult SubmitLinkingRequest(RequestVM request)
        {
            string AddressValue = ""; string MobileValue = "";
            string LandlineValue = ""; string OfficePhoneValue = "";
            long ID = 0;
            // string LinkedAccount = string.Join(",", request.LinkAccount1);
            if (IsDuplicateEntry(request.CardNo, request.DefaultAccountNo, request.RequestType, request.CIFNo, out ID))
                return Json(new { IsSuccess = ID, ErrorMessage = CustomMessages.RequestAlreadyExists }, JsonRequestBehavior.AllowGet);

            foreach (var item in request.LinkAccount1)
            {
                if (new CustomerAccountDataAccess().GetCustomerAccount(item, request.CIFNo, request.CardNo).Count > 0)
                    return Json(new { IsSuccess = false, ErrorMessage = string.Format(CustomMessages.AccountAlreadyLinked, item) }, JsonRequestBehavior.AllowGet);
            }

            long? success;
            CardChargesDataAccess cc = new CardChargesDataAccess();
            List<tbl_Card_Charges> lst = new List<tbl_Card_Charges>();
            tbl_Customer_Accounts cvm = new tbl_Customer_Accounts();
            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            //resp = T24Methods.FetchAccount(request.AccountNo);
            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            resp = T24Methods.FetchAccount(request.AccountNo, request.CIFNo);
            var branchCode = string.IsNullOrEmpty(resp.BranchCode) ? string.Empty : resp.BranchCode.Substring(resp.BranchCode.Length - 4, 4);
            request.DefaultAccountNo = request.DefaultAccountNo.Length < 15 ? branchCode + request.DefaultAccountNo : request.DefaultAccountNo;
            request.AccountNo = request.AccountNo.Length < 15 ? branchCode + request.AccountNo : request.AccountNo;

            resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
            string AccountStatus = resp.AccountStatus;
            decimal AvailBalance = Convert.ToDecimal((!string.IsNullOrEmpty(resp.AvailableBalance)) ? resp.AvailableBalance : "0");
            //bool StatusEligibility = CommonMethods.CheckStatusEligibility(AccountStatus);
            EligibilityVM elg = new EligibilityVM();
            elg.IdentificationType = resp.IdentificationType;
            elg.IdentificationNumber = resp.IdentificationNo;
            elg.Resident = resp.ResidenceStatus;
            elg.Nationality = resp.Nationality;
            elg.SectorCode = resp.Sector;
            elg.AccountStatus = resp.AccountStatus;
            elg.FatherName = resp.FathersName;
            elg.MotherName = resp.MothersName;
            elg.DOB = resp.CustomerDOB;
            elg.PostingRestriction = resp.PostingRestriction;
            elg.OpInstruction = resp.OpInstructions;
            elg.AccountNo = request.AccountNo;
           

         

            //fixing for 11 digit account codes

            string[] LinkedAccounts1 = request.LinkAccount1;
            for (int i = 0; i < LinkedAccounts1.Length; i++)
            {
                if (LinkedAccounts1[i].Length == 11)
                {
                    var resp1 = T24Methods.FetchAccount(LinkedAccounts1[i], request.CIFNo);
                    var brcode = string.IsNullOrEmpty(resp1.BranchCode) ? string.Empty : resp1.BranchCode.Substring(resp1.BranchCode.Length - 4, 4);
                    LinkedAccounts1[i] = brcode + "" + LinkedAccounts1[i].ToString();
                }
            }

            //test
            List<string> StatusEligibility = CommonMethods.IsEligible(elg);
            if (StatusEligibility.Count <= 0)
            {
                request.RequestType = "L";
                request.AuthorizationStatus = "P";
                request.StatusEligibility = true;
                request.LinkAccount = string.Join(",", LinkedAccounts1);
                request.CustomerId = resp.CustomerID;
                success = RequestMethods.AddLinkingRequest(request);

                //var MainAddress = AuthorizationController.FieldMappingAddress(request.MainAddress, resp);
                //var MainMobile = AuthorizationController.FieldMappingMobile(request.MainMobile, resp);
                //var MainLandline = AuthorizationController.FieldMappingLandline(request.MainLandline, resp);

                //AccountExists(request.AccountNo, resp, MainAddress, MainMobile, MainLandline, request.CIFNo);

                var MainAddress = request.MainAddress;
                MainAddress = !string.IsNullOrEmpty(MainAddress) ? MainAddress.Replace("\n", " ") : " ";
                var MainMobile = request.MainMobile;
                MainMobile = !string.IsNullOrEmpty(MainMobile) ? MainMobile.Replace("\n", " ") : " ";
                var MainLandline = request.MainLandline;
                MainLandline = !string.IsNullOrEmpty(MainLandline) ? MainLandline.Replace("\n", " ") : " ";
                var MainLandlineNo = request.LandlineNo;
                MainLandlineNo = !string.IsNullOrEmpty(MainLandlineNo) ? MainLandlineNo.Replace("\n", " ") : string.Empty;
                var MainOfficeNo = request.PhoneOff;
                MainOfficeNo = !string.IsNullOrEmpty(MainOfficeNo) ? MainOfficeNo.Replace("\n", " ") : string.Empty;

                AddressValue = MainAddress;
                MobileValue = MainMobile;
                LandlineValue = MainLandline;
                OfficePhoneValue = MainOfficeNo;


                // AccountExists(request.AccountNo, resp, MainAddress, MainMobile, MainLandline, MainLandlineNo, MainOfficeNo, request.CIFNo, request.CardNumber);

                  RequestMethods.AddRequestCustomerAccount(request, AddressValue, MobileValue, LandlineValue, OfficePhoneValue);


                return Json(new { IsSuccess = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                request.RequestType = "L";
                request.AuthorizationStatus = "C";
                request.StatusEligibility = false;
                request.LinkAccount = string.Join(",", LinkedAccounts1);
                request.CustomerId = resp.CustomerID;
                success = RequestMethods.AddLinkingRequest(request);
                string ErrorString = string.Join(",", StatusEligibility.ToArray());
                /*Update user log for exceptions*/
                AddException(success, ErrorString);

                return Json(new { IsSuccess = success, ErrorMessage = ErrorString }, JsonRequestBehavior.AllowGet);
            }

        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AuthorizeLinkingScreen()
        {
            List<AuthorizationVM> viewModel = RequestMethods.GetRequestDetails("L");
            return View(viewModel);
        }


        #endregion Linking Screen

        #region DeLinking Screen

        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult DelinkingScreen(int? Id)
        {
            UpdateRequestVM viewModel = new UpdateRequestVM();
            if (Id.HasValue)
            {
                viewModel.RequestData = GetRequestToBeReviewed(Id, "D");
                if (viewModel.RequestData == null)
                    return RedirectToAction("ReviewDelinkingScreen", "Review");
            }

            ViewBag.RequestId = Id ?? 0;
            CardTypesDataAccess obj = new CardTypesDataAccess();
            ViewBag.CardTypes = obj.GetCardTypes("A");
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "c" })]
        [HttpPost]
        public JsonResult SubmitDeLinkingRequest(RequestVM request)
        {
            string AddressValue = ""; string MobileValue = "";
            string LandlineValue = ""; string OfficePhoneValue = "";
            long ID = 0;
            string DeLinkedAccount = string.Join(",", request.DeLinkAccount1);
            if (IsDuplicateEntry(request.CardNo, request.AccountNo, request.RequestType, request.CIFNo, out ID))
                return Json(new { IsSuccess = ID, ErrorMessage = CustomMessages.RequestAlreadyExists }, JsonRequestBehavior.AllowGet);

            long? success;
            CardChargesDataAccess cc = new CardChargesDataAccess();
            List<tbl_Card_Charges> lst = new List<tbl_Card_Charges>();

            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            resp = T24Methods.FetchAccount(request.AccountNo, request.CIFNo);

            request.RequestType = "D";
            request.AuthorizationStatus = "P";
            request.LinkAccount = DeLinkedAccount;
            request.CardTypeId = int.Parse(request.CardType);
            success = RequestMethods.AddLinkingRequest(request);

            //var MainAddress = AuthorizationController.FieldMappingAddress(request.MainAddress, resp);
            //var MainMobile = AuthorizationController.FieldMappingMobile(request.MainMobile, resp);
            //var MainLandline = AuthorizationController.FieldMappingLandline(request.MainLandline, resp);

            //AccountExists(request.AccountNo, resp, MainAddress, MainMobile, MainLandline, request.CIFNo);
            // return Json(success, JsonRequestBehavior.AllowGet);
            var MainAddress = request.MainAddress;
            MainAddress = !string.IsNullOrEmpty(MainAddress) ? MainAddress.Replace("\n", " ") : " ";
            var MainMobile = request.MainMobile;
            MainMobile = !string.IsNullOrEmpty(MainMobile) ? MainMobile.Replace("\n", " ") : " ";
            var MainLandline = request.MainLandline;
            MainLandline = !string.IsNullOrEmpty(MainLandline) ? MainLandline.Replace("\n", " ") : " ";
            var MainLandlineNo = request.LandlineNo;
            MainLandlineNo = !string.IsNullOrEmpty(MainLandlineNo) ? MainLandlineNo.Replace("\n", " ") : string.Empty;
            var MainOfficeNo = request.PhoneOff;
            MainOfficeNo = !string.IsNullOrEmpty(MainOfficeNo) ? MainOfficeNo.Replace("\n", " ") : string.Empty;

            AddressValue = MainAddress;
            MobileValue = MainMobile;
            LandlineValue = MainLandline;
            OfficePhoneValue = MainOfficeNo;


            // AccountExists(request.AccountNo, resp, MainAddress, MainMobile, MainLandline, MainLandlineNo, MainOfficeNo, request.CIFNo, request.CardNumber);

            RequestMethods.AddRequestCustomerAccount(request, AddressValue, MobileValue, LandlineValue, OfficePhoneValue);

            return Json(new { IsSuccess = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AuthorizeDelinkingScreen()
        {
            List<AuthorizationVM> viewModel = RequestMethods.GetRequestDetails("D");
            return View(viewModel);
        }
        #endregion

        #region Card Ammendment
        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult CardAmendmentScreen(int? Id)
        {
            UpdateRequestVM viewModel = new UpdateRequestVM();
            if (Id.HasValue)
            {
                viewModel.RequestData = GetRequestToBeReviewed(Id, "A");
                CustomerRequestAccountDataAccess crad = new CustomerRequestAccountDataAccess();
                var RequestAccount = crad.GetRequestCustomerAccountByRequestId(Id ?? 0);

                var MainAddress = AuthorizationController.OtherAddressMapping(RequestAccount.MainAddress);
                var SetAddress = "";
                ViewBag.MainAddress = MainAddress;

                if (MainAddress == "Other")
                {
                    SetAddress = RequestAccount.MainAddress;
                }
                ViewBag.SetAddress = SetAddress;
                /*mobile*/
                var MainMobile = AuthorizationController.OtherMobileNoMapping(RequestAccount?.MainMobile ?? string.Empty);
                var SetMobileNo = "";
                ViewBag.MainMobile = MainMobile;

                if (MainMobile == "Other")
                {
                    SetMobileNo = RequestAccount?.MainMobile ?? string.Empty;
                }
                ViewBag.SetMobileNo = SetMobileNo;

                /*landline*/
                var MainLandlineNo = AuthorizationController.OtherLandlineNoMapping(RequestAccount?.LandlineNo ?? string.Empty);
                var SetLandlineNo = "";
                ViewBag.MainLandlineNo = MainLandlineNo;

                if (MainLandlineNo == "Other")
                {
                    SetLandlineNo = RequestAccount?.LandlineNo ?? string.Empty;
                }
                ViewBag.SetLandlineNo = SetLandlineNo;

                /*office*/
                var MainOfficeNo = AuthorizationController.OtherOfficeNoMapping(RequestAccount?.PhoneOffice ?? string.Empty);
                var SetOfficeNo = "";
                ViewBag.MainOfficeNo = MainOfficeNo;

                if (MainOfficeNo == "Other")
                {
                    SetOfficeNo = RequestAccount?.PhoneOffice ?? string.Empty;
                }
                ViewBag.SetOfficeNo = SetOfficeNo;
                /**/
                //ViewBag.MainMobile = RequestAccount.MainMobile;
                ViewBag.MainLandline = RequestAccount.MainLandline;
                if (viewModel.RequestData == null)
                    return RedirectToAction("ReviewCardAmendmentScreen", "Review");
            }

            ViewBag.RequestCustomerInfo = Id.HasValue ? new CustomerRequestAccountDataAccess().GetRequestCustomerAccountByRequestId(Id ?? 0) : null;
            ViewBag.RequestId = Id ?? 0;
            CardTypesDataAccess obj = new CardTypesDataAccess();
            ViewBag.CardTypes = obj.GetCardTypes("A");
            return View(viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "c" })]
        public JsonResult AddUpdateCardAmendmentScreen(RequestVM request)
        {
            //change audit trail report
            string AddressValue = ""; string MobileValue = "";
            string LandlineValue = ""; string OfficePhoneValue = "";
            //

            long ID = 0;
            if (IsDuplicateEntry(request.CardNo, request.AccountNo, request.RequestType, request.CIFNo, out ID))
                return Json(new { IsSuccess = ID, ErrorMessage = CustomMessages.RequestAlreadyExists }, JsonRequestBehavior.AllowGet);

            long? success;
            CardChargesDataAccess cc = new CardChargesDataAccess();
            List<tbl_Card_Charges> lst = new List<tbl_Card_Charges>();


            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();

            //resp = T24Methods.FetchAccount(request.AccountNo);
            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            resp = T24Methods.FetchAccount(request.AccountNo,request.CustomerId);
            var branchCode = string.IsNullOrEmpty(resp.BranchCode) ? string.Empty : resp.BranchCode.Substring(resp.BranchCode.Length - 4, 4);
            request.AccountNo = request.AccountNo.Length < 15 ? branchCode + request.AccountNo : request.AccountNo;
            string AccountStatus = resp.AccountStatus;
            resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
            //bool StatusEligibility = CommonMethods.CheckStatusEligibility(AccountStatus);
            EligibilityVM elg = new EligibilityVM();
            elg.IdentificationType = resp.IdentificationType;
            elg.IdentificationNumber = resp.IdentificationNo;
            elg.Resident = resp.ResidenceStatus;
            elg.Nationality = resp.Nationality;
            elg.SectorCode = resp.Sector;
            elg.AccountStatus = resp.AccountStatus;
            elg.FatherName = resp.FathersName;
            elg.MotherName = resp.MothersName;
            elg.DOB = resp.CustomerDOB;
            elg.PostingRestriction = resp.PostingRestriction;
            elg.OpInstruction = resp.OpInstructions;
            elg.AccountNo = request.AccountNo;
            List<string> StatusEligibility = CommonMethods.IsEligible(elg);
            var MainAddress = AuthorizationController.FieldMappingAddress(request.MainAddress, resp);
            MainAddress = !string.IsNullOrEmpty(MainAddress) ? MainAddress.Replace("\n", " ") : string.Empty;
            var MainMobile = AuthorizationController.FieldMappingMobile(request.MainMobile, resp);
            MainMobile = !string.IsNullOrEmpty(MainMobile) ? MainMobile.Replace("\n", " ") : string.Empty;
            var MainLandline = AuthorizationController.FieldMappingLandline(request.MainLandline, resp);
            MainLandline = !string.IsNullOrEmpty(MainLandline) ? MainLandline.Replace("\n", " ") : string.Empty;
            var MainLandlineNo = AuthorizationController.FieldMappingLandlineNo(request.LandlineNo, resp);
            MainLandlineNo = !string.IsNullOrEmpty(MainLandlineNo) ? MainLandlineNo.Replace("\n", " ") : string.Empty;
            var MainOfficeNo = AuthorizationController.FieldMappingOfficeNo(request.PhoneOff, resp);
            MainOfficeNo = !string.IsNullOrEmpty(MainOfficeNo) ? MainOfficeNo.Replace("\n", " ") : string.Empty;
            AddressValue = MainAddress;
            MobileValue = MainMobile;
            LandlineValue = MainLandline;
            OfficePhoneValue = MainOfficeNo;

            if (StatusEligibility.Count <=0)
            {
                request.AuthorizationStatus = "P";
                request.StatusEligibility = true;
                request.RequestType = "A";
                request.CardTypeId = int.Parse(request.CardType);
                //request.CardTypeId = int.Parse(request.CardType);
                request.CustomerId = resp.CustomerID;
                success = RequestMethods.AddAmmendRequest(request);
                RequestMethods.AddRequestCustomerAccount(request,AddressValue,MobileValue,LandlineValue,OfficePhoneValue);
                AccountExists(request.AccountNo, resp, MainAddress, MainMobile, MainLandline, MainLandlineNo, MainOfficeNo, request.CIFNo, request.CardNumber);
                return Json(new { IsSuccess = success, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                request.AuthorizationStatus = "C";
                request.StatusEligibility = true;
                request.RequestType = "A";
                request.CardTypeId = int.Parse(request.CardType);
                //request.CardTypeId = int.Parse(request.CardType);
                request.CustomerId = resp.CustomerID;
                success = RequestMethods.AddAmmendRequest(request);
                RequestMethods.AddRequestCustomerAccount(request, AddressValue, MobileValue, LandlineValue, OfficePhoneValue);
                string ErrorString = string.Join(",", StatusEligibility.ToArray());
                /*Update user log for exceptions*/
                AddException(success, ErrorString);

                return Json(new { IsSuccess = success, ErrorMessage = ErrorString }, JsonRequestBehavior.AllowGet);
            }
            
            
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AuthorizeCardAmendmentScreen()
        {
            List<AuthorizationVM> viewModel = RequestMethods.GetRequestDetails("A");
            return View(viewModel);
        }
        #endregion

        #region Card Upgrade
        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult CardUpgradeScreen(int? Id)
        {
            UpdateRequestVM viewModel = new UpdateRequestVM();
            CardTypesDataAccess obj = new CardTypesDataAccess();
            if (Id.HasValue)
            {
                viewModel.RequestData = GetRequestToBeReviewed(Id, "U");
                if (viewModel.RequestData == null)
                    return RedirectToAction("ReviewCardUpgradeScreen", "Review");
            }

            ViewBag.RequestId = Id ?? 0;

            ViewBag.CardTypes = obj.GetCardTypes("A", "U");
            
            return View(viewModel);
        }
        public ActionResult GetCustomerCard(string request)
        {
            CustomerCardVM response = new CustomerCardVM();
            response = RequestMethods.GetCustomerCard(request);


            //var data = response;
            return Json(new { IsSuccess = (response == null) ? false : true, ErrorMessage = (response == null) ? CustomMessages.GenericErrorMessage : string.Empty, Response = response }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CheckCardUpgradeEligibility(int ParentCardType, int ChildCardType)
        {
            //List<AuthorizationVM> viewModel = RequestMethods.GetRequestDetails("L");
            //return View(viewModel);
            var response = new CardTypesDataAccess().CheckCardTypeEligibility(ParentCardType, ChildCardType);
            return Json(new { IsSuccess = response, Response = response }, JsonRequestBehavior.AllowGet);
            //bool success;
            //request.RequestType = "D";
            //success = RequestMethods.AddLinkingRequest(request);
        }
        public JsonResult CheckAccountTypeEligibility(string AccountCode, int CardTypeID)
        {
            var response = new AccountTypeDataAccess().CheckAccountTypeEligibility(AccountCode, CardTypeID);
            return Json(new { IsSuccess = response, Response = response }, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "c" })]
        [HttpPost]
        public JsonResult AddUpdateCardUpgradeScreen(RequestVM request)
        {
            long ID = 0;
            if (IsDuplicateEntry(request.CardNo, request.AccountNo, request.RequestType, request.CIFNo, out ID))
                return Json(new { IsSuccess = ID, ErrorMessage = CustomMessages.RequestAlreadyExists }, JsonRequestBehavior.AllowGet);

            long? success = 0;
            CardChargesDataAccess cc = new CardChargesDataAccess();
            List<tbl_Card_Charges> lst = new List<tbl_Card_Charges>();
            List<string> StatusEligibility = new List<string>();
            //request.AccountTypeId = 001;
            ////request.CardTypeId = 1;
            //request.BranchCode = "001";
            string Frequency = "0";
            //string Frequency = "0";
            //CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
            //resp = T24Methods.FetchAccount(request.AccountNo);
            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            resp = T24Methods.FetchAccount(request.AccountNo, request.CIFNo);
            resp.BranchCode = string.IsNullOrEmpty(resp.BranchCode) ? resp.BranchCode : resp.BranchCode.Substring(resp.BranchCode.Length - 4, 4); //request.AccountNo.Substring(0, 4);
            request.AccountNo = request.AccountNo.Length < 15 ? resp.BranchCode + request.AccountNo : request.AccountNo;
            //lst = cc.GetCardCharges("001", request.CardTypeId, request.BranchCode, Frequency);
            resp.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(resp.MothersName);
            lst = cc.GetCardCharges(resp.AccountNature, request.CardTypeId, resp.BranchCode, Frequency,"0");
            var TotalAmount = lst.Sum(item => item.Amount);
            string AccountStatus = resp.AccountStatus;
            decimal AvailBalance = Convert.ToDecimal((!string.IsNullOrEmpty(resp.AvailableBalance)) ? resp.AvailableBalance : "0");
            if (lst.Count > 0)
            {
                //bool StatusEligibility = CommonMethods.CheckStatusEligibility(AccountStatus);
                EligibilityVM elg = new EligibilityVM();
                elg.IdentificationType = resp.IdentificationType;
                elg.IdentificationNumber = resp.IdentificationNo;
                elg.Resident = resp.ResidenceStatus;
                elg.Nationality = resp.Nationality;
                elg.SectorCode = resp.Sector;
                elg.AccountStatus = resp.AccountStatus;
                elg.FatherName = resp.FathersName;
                elg.MotherName = resp.MothersName;
                elg.DOB = resp.CustomerDOB;
                elg.PostingRestriction = resp.PostingRestriction;
                elg.OpInstruction = resp.OpInstructions;
                elg.AccountNo = request.AccountNo;
                StatusEligibility = CommonMethods.IsEligible(elg);
                bool BalanceEligibility = CommonMethods.CheckBalanceEligibility(AvailBalance, TotalAmount);
                if (request.Waive == false && StatusEligibility.Count<=0 && BalanceEligibility == true)
                {
                    request.RequestType = "U";
                    request.AuthorizationStatus = "P";
                    request.StatusEligibility = true;
                    success = RequestMethods.AddNewRequest(request);

                }
                else if (request.Waive == true && StatusEligibility.Count<=0)
                {
                    request.RequestType = "U";
                    request.AuthorizationStatus = "P";
                    request.StatusEligibility = true;
                    success = RequestMethods.AddNewRequest(request);

                    //var MainAddress = AuthorizationController.FieldMappingAddress(request.MainAddress, resp);
                    //var MainMobile = AuthorizationController.FieldMappingMobile(request.MainMobile, resp);
                    //var MainLandline = AuthorizationController.FieldMappingLandline(request.MainLandline, resp);

                    //AccountExists(request.AccountNo, resp, MainAddress, MainMobile, MainLandline, request.CIFNo);
                }

                else
                {
                    request.RequestType = "U";
                    request.AuthorizationStatus = "C";
                    request.StatusEligibility = false;
                    success = RequestMethods.AddNewRequest(request);
                    var ErrorString = string.Join(",", StatusEligibility.ToArray());
                    /*Update user log for exceptions*/
                    AddException(success, ErrorString);

                }
            }
            else
            {
                request.RequestType = "U";
                request.AuthorizationStatus = "C";
                request.StatusEligibility = false;
                success = RequestMethods.AddNewRequest(request);
                var ErrorString = string.Join(",", StatusEligibility.ToArray());
                /*Update user log for exceptions*/
                AddException(success, ErrorString);

            }

            return Json(success, JsonRequestBehavior.AllowGet);
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AuthorizeCardUpgradeScreen()
        {
            List<AuthorizationVM> viewModel = RequestMethods.GetRequestDetails("U");
            return View(viewModel);
        }
        #endregion


        [AuthOp(RoleTitle = new string[] { "u" })]
        //#endregion Card Upgrade
        public ActionResult FilterAuthorizeScreen(FilterAuthorizationVM request)
        {
            List<AuthorizationVM> viewModel = RequestMethods.FilterRequestDetails(request);
            return PartialView("_partialAuthorizeView", viewModel);
        }

        [AuthOp(RoleTitle = new string[] { "c" })]
        [HttpPost]
        public JsonResult UpdateReviewRequest(tbl_Requests request, bool Waive, string CustomerId, string CardType, int CustomerRequestId, string MainAddress, string MainMobile, string MainLandline, string LandlineNo, string PhoneOff, string[] LinkAccount1 = default(string[]), string[] DeLinkAccount1 = default(string[]))
        {
            long ID = 0;
            int cardTypeId = 0;
            request.WaiveCharges = Waive;
            request.CIFNo = CustomerId;
            int.TryParse(CardType, out cardTypeId);
            request.CardTypeID = cardTypeId;
            request.IsActive = true;
            request.StatusEligibility = true;
            request.FinancialEligibility = true;
            request.AuthorizationStatus = "P";
            if (request.RequestType == "L")
                request.LinkingDelinkingAccount = string.Join(",", LinkAccount1);
            else if (request.RequestType == "D")
                request.LinkingDelinkingAccount = string.Join(",", DeLinkAccount1);
            request.IsExported = false;

            var resp = T24Methods.FetchAccount(request.AccountNo, request.CIFNo);
            var success = new RequestDataAccess().UpdateRequest(request, out ID);
            if (success && CustomerRequestId > 0)
                RequestMethods.UpdateRequestCustomerAccount(new RequestVM() { CustomerRequestID = CustomerRequestId, AccountNo = request.AccountNo, MainAddress = MainAddress, MainLandline = MainLandline, MainMobile = MainMobile, LandlineNo = LandlineNo, PhoneOff = PhoneOff });
            if (success == true)
            {
                if (request.RequestType == "N" || request.RequestType == "R" || request.RequestType == "A")
                {
                    var MainAddressData = AuthorizationController.FieldMappingAddress(MainAddress, resp);
                    var MainMobileData = AuthorizationController.FieldMappingMobile(MainMobile, resp);
                    var MainLandlineData = AuthorizationController.FieldMappingLandline(MainLandline, resp);
                    var MainLandlineNoData = AuthorizationController.FieldMappingLandlineNo(LandlineNo, resp);
                    var MainOfficeNoData = AuthorizationController.FieldMappingOfficeNo(PhoneOff, resp);

                    AccountExists(request.AccountNo, resp, MainAddressData, MainMobileData, MainLandlineData, MainLandlineNoData, MainOfficeNoData, request.CIFNo, request.CardNo);
                }
                return Json(new { IsSuccess = ID, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { IsSuccess = ID, ErrorMessage = CustomMessages.GenericErrorMessage }, JsonRequestBehavior.AllowGet);
            }
            
        }

        private tbl_Requests GetRequestToBeReviewed(int? Id, string AuthorizationStatusType)
        {
            return new RequestDataAccess().GetRequestToBeReviewed(Id.GetValueOrDefault(), AuthorizationStatusType);
        }

        public static object T24TestObject(string AccountNo, int? CardTypeId = null)
        {
            var AccountInfo = new
            {
                ACALTID5 = "",
                ACALTID6 = "",
                AccountAddress = "",
                AccountBranchCode = "",
                AccountCategory = "",
                AccountCurrency = "",
                AccountID = "",
                AccountMobile = "",
                AccountNature = "",
                AccountNumber = "",
                AccountOpeningDate = "",
                AccountPhone = "",
                AccountRestraints = "[]",
                AccountStatus = "",
                AccountTitle = "",
                AvailableBalance = 0,
                BranchCode = "",
                CategoryDesc = "",
                CompName = "",
                CustomerID = "",
                IdentificationNo = "",
                LegacyAcNo = "",
                OffPhone = "",
                OrganizationCode = "",
                ProductCode = "",
                ResponseCode = "",
                ResponseCodeDescription = "",
                SDNAccountNo = "",
                SDNIBAN = "",
                SchemeCode = "",
                SectorDesc = "",
                T24ACNO = "",
                T24IBAN = "",
                CardTypeId = CardTypeId
            };
            if (AccountNo == "20001266498")
            {
                AccountInfo = new
                {
                    ACALTID5 = "001520000024679",
                    ACALTID6 = "",
                    AccountAddress = "NEWS HD 47 N GULBERG II",
                    AccountBranchCode = "0015",
                    AccountCategory = "",
                    AccountCurrency = "PKR",
                    AccountID = "20000024679\'COK",
                    AccountMobile = "",
                    AccountNature = "1001",
                    AccountNumber = "",
                    AccountOpeningDate = "/Date(1518375600000)/",
                    AccountPhone = "",
                    AccountRestraints = "[]",
                    AccountStatus = "0001",
                    AccountTitle = "MUBASHER LUCMAN",
                    AvailableBalance = 1023,
                    BranchCode = "",
                    CategoryDesc = "Current Deposit Account Customer",
                    CompName = "Gulberg Br. LHR-0015",
                    CustomerID = "1000109118",
                    IdentificationNo = "3520156521059",
                    LegacyAcNo = "",
                    OffPhone = "042357642524",
                    OrganizationCode = "",
                    ProductCode = "",
                    ResponseCode = "0000",
                    ResponseCodeDescription = "Success",
                    SDNAccountNo = "001502013091429",
                    SDNIBAN = "PK34SONE0001502013091429",
                    SchemeCode = "",
                    SectorDesc = "Individual Particuliers",
                    T24ACNO = "",
                    T24IBAN = "PK46SONE0000020000024679",
                    CardTypeId = CardTypeId
                };
            }
            return AccountInfo;
        }

        private bool IsDuplicateEntry(string cardNumber, string accountNumber, string requestType, string CIF, out long requestId)
        {
            //return false;
            return new RequestDataAccess().IsDuplicateRequest(cardNumber ?? string.Empty, accountNumber ?? string.Empty, requestType ?? string.Empty, CIF ?? string.Empty, out requestId);
        }

        public ActionResult SearchAccountForDelinking(string CardNo)
        {
            List<CustomerCardVM> CardInfo = new List<CustomerCardVM>();
            List<CustomerAccountVM> DelinkInfo = new List<CustomerAccountVM>();
            CustomerAccountDataAccess obj = new CustomerAccountDataAccess();
            CardInfo = CommonMethods.GetCardInfo(CardNo);
            DelinkInfo = CommonMethods.GetAccountInfo(CardNo,CardInfo.FirstOrDefault()?.AccountNo ?? "0");
            //List<string> DelinkInfoStr = new List<string>();

            
            var DelinkInfoStr = CommonMethods.GetLinkedAccounts(CardNo,CardInfo.FirstOrDefault()?.AccountNo ?? "0");
            //DelinkInfoStr = linkacct.Split(',').ToList();
            //CISSB.AccountInfoResponse AccountInfo = new CISSB.AccountInfoResponse();
            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType AccountInfo = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
            if (DelinkInfoStr != null)
            {
                if (CardInfo.Count > 0)
                {
                    AccountInfo = T24Methods.FetchAccount(CardInfo.FirstOrDefault().AccountNo, CardInfo.FirstOrDefault().CIFNo);
                    var branchCode = string.IsNullOrEmpty(AccountInfo.BranchCode) ? string.Empty : AccountInfo.BranchCode.Substring(AccountInfo.BranchCode.Length - 4, 4);
                    AccountInfo.T24ACNO = AccountInfo.T24ACNO.Length < 15 ? branchCode + AccountInfo.T24ACNO : AccountInfo.T24ACNO;

                    AccountInfo.Nationality = !string.IsNullOrEmpty(AccountInfo.Nationality) ? AccountInfo.Nationality.Equals("PK") ? "Pakistani" : "Other" : string.Empty;
                    AccountInfo.ResidenceStatus = !string.IsNullOrEmpty(AccountInfo.ResidenceStatus) ? AccountInfo.ResidenceStatus.Equals("PK") ? "Pakistan" : "Other" : string.Empty;
                    AccountInfo.CustomerDOB = AccountInfo.CustomerDOB.ToValidDateTime("dd-MM-yyyy");
                    AccountInfo.MothersName = CustomHelper.GetOnlyAlphabetsAndSpaces(AccountInfo.MothersName);
                    var result = new { AccountInfo = AccountInfo, CardInfo = CardInfo, DelinkInfoStr = DelinkInfoStr };
                    return Json(new { Result = result, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
                }else
                {
                    return Json(new {ErrorMessage = CustomMessages.CardNotExists }, JsonRequestBehavior.AllowGet);
                }
            }else
            {
                return Json(new { ErrorMessage = CustomMessages.SingleAccountForCard }, JsonRequestBehavior.AllowGet);
            }
            
        }

        private bool UpdateUserLog(long requestId, int currentCardTypeId)
        {
            return new UserLogDataAccess().UpdateUserLog(requestId, currentCardTypeId);
        }

        private static void AccountExists(string AccountNo, NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp, string MainAddress, string MainMobile, string MainLandline, string MainLandlineNo, string MainOfficeNo, string CIF, string CardNo = default(string))
        {
            try
            {
                var BranchCode = "";
                if (AccountNo.Length < 15)
                {
                    BranchCode = string.IsNullOrEmpty(resp.BranchCode) ? string.Empty : resp.BranchCode.Substring(resp.BranchCode.Length - 4, 4);
                    AccountNo = BranchCode + AccountNo;
                }

                tbl_Customer_Accounts cvm = new tbl_Customer_Accounts();
                var db = new SoneriCISEntities();
                var acuntNature = (from a in db.tbl_Account_Types where a.Code == resp.AccountNature select a.ID).ToList();
                CustomerAccountDataAccess check = new CustomerAccountDataAccess();
                var exists = check.GetCustomerAccountbyCIF(AccountNo, CIF);
                if (exists.Count <= 0)
                {
                    cvm.AccountNo = AccountNo;
                    cvm.CIF = resp.CustomerID;
                    cvm.Currency = !string.IsNullOrEmpty(resp.AccountCurrency) ? resp.AccountCurrency : "PKR";
                    cvm.CardNo = null;
                    if (string.IsNullOrEmpty(resp.AccountStatus))
                        cvm.AccountStatusActive = true;
                    else
                        cvm.AccountStatusActive = false;

                    cvm.AccountTitle = (resp.AccountTitle == null ? null : resp.AccountTitle);
                    cvm.Address = (resp.AccountAddress == null ? null : resp.AccountAddress);
                    cvm.Address2 = (resp.CustomerAddress1 == null ? null : resp.CustomerAddress1);
                    cvm.Address3 = (resp.CustomerAddress2 == null ? null : resp.CustomerAddress2);
                    cvm.Mobile = (resp.AccountMobile == null ? null : resp.AccountMobile);
                    cvm.Mobile2 = (resp.SMS == null ? null : resp.SMS);
                    cvm.Mobile3 = (resp.SMS2 == null ? null : resp.SMS2);
                    cvm.Mobile4 = (resp.SMS3 == null ? null : resp.SMS3);
                    cvm.CNIC = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                    if (resp.CustomerDOB.Length == 8)
                    {
                        int y = int.Parse(resp.CustomerDOB.Substring(0, 4));
                        int m = int.Parse(resp.CustomerDOB.Substring(4, 2));
                        int d = int.Parse(resp.CustomerDOB.Substring(6, 2));
                        DateTime dt1 = new DateTime(y, m, d);
                        cvm.DateofBirth = dt1;
                    }
                    else
                        cvm.DateofBirth = null;

                    cvm.MotherMaidenName = (resp.MothersName == null ? null : resp.MothersName);
                    cvm.Identification = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                    cvm.AccountTypeID = acuntNature.Count <= 0 ? 0 : acuntNature[0];
                    cvm.LandlineNo = (MainLandlineNo == null ? null : MainLandlineNo);
                    cvm.Email = (resp.Email == null ? null : resp.Email);
                    cvm.Nationality = (resp.Nationality == null ? null : resp.Nationality);
                    cvm.PhoneOffice = (MainOfficeNo == null ? null : MainOfficeNo);
                    cvm.IdentificationType = (resp.IdentificationType == null ? null : resp.IdentificationType);
                    cvm.MainMobile = (MainMobile == null ? null : MainMobile);
                    cvm.MainLandline = (MainLandline == null ? null : MainLandline);
                    cvm.MainAddress = (MainAddress == null ? null : MainAddress);
                    cvm.CustomerName = (resp.CustomerName == null ? null : resp.CustomerName);
                    cvm.FatherName = (resp.FathersName == null ? null : resp.FathersName);
                    cvm.ResidenceStatus = (resp.ResidenceStatus == null ? null : resp.ResidenceStatus);
                    db.tbl_Customer_Accounts.Add(cvm);
                    db.SaveChanges();
                }
                else
                {
                    tbl_Customer_Accounts result = new tbl_Customer_Accounts();
                    if (string.IsNullOrEmpty(CardNo))
                    {
                        result = (from p in db.tbl_Customer_Accounts
                                  where p.AccountNo == AccountNo && p.CIF == CIF
                                  select p).FirstOrDefault();
                    }
                    else
                    {
                        result = (from p in db.tbl_Customer_Accounts
                                  where p.AccountNo == AccountNo && p.CIF == CIF
                                  && p.CardNo == CardNo
                                  select p).FirstOrDefault();
                        if (result == null)
                            result = (from p in db.tbl_Customer_Accounts
                                      where p.AccountNo == AccountNo && p.CIF == CIF
                                      select p).SingleOrDefault();
                    }

                    result.AccountNo = AccountNo;
                    result.CIF = resp.CustomerID;
                    //result.CardNo = result.CardNo; //not updating card number to mantain linked accounts
                    result.Currency = !string.IsNullOrEmpty(resp.AccountCurrency) ? resp.AccountCurrency : "PKR";
                    if (string.IsNullOrEmpty(resp.AccountStatus))
                        result.AccountStatusActive = true;
                    else
                        result.AccountStatusActive = false;

                    result.AccountTitle = (resp.AccountTitle == null ? null : resp.AccountTitle);
                    result.Address = (resp.AccountAddress == null ? null : resp.AccountAddress);
                    result.Address2 = (resp.CustomerAddress1 == null ? null : resp.CustomerAddress1);
                    result.Address3 = (resp.CustomerAddress2 == null ? null : resp.CustomerAddress2);
                    result.Mobile = (resp.AccountMobile == null ? null : resp.AccountMobile);
                    result.Mobile2 = (resp.SMS == null ? null : resp.SMS);
                    result.Mobile3 = (resp.SMS2 == null ? null : resp.SMS2);
                    result.Mobile4 = (resp.SMS3 == null ? null : resp.SMS3);
                    result.CNIC = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                    if (resp.CustomerDOB.Length == 8)
                    {
                        int y = int.Parse(resp.CustomerDOB.Substring(0, 4));
                        int m = int.Parse(resp.CustomerDOB.Substring(4, 2));
                        int d = int.Parse(resp.CustomerDOB.Substring(6, 2));
                        DateTime dt1 = new DateTime(y, m, d);
                        result.DateofBirth = dt1;
                    }
                    else
                        result.DateofBirth = null;

                    result.MotherMaidenName = (resp.MothersName == null ? null : resp.MothersName);
                    result.Identification = (resp.IdentificationNo == null ? null : resp.IdentificationNo);
                    result.AccountTypeID = acuntNature[0];
                    result.AccountTypeID = acuntNature.Count <= 0 ? 0 : acuntNature[0];
                    result.LandlineNo = (MainLandlineNo == null ? null : MainLandlineNo);
                    result.Email = (resp.Email == null ? null : resp.Email);
                    result.Nationality = (resp.Nationality == null ? null : resp.Nationality);
                    result.PhoneOffice = (MainOfficeNo == null ? null : MainOfficeNo);
                    result.IdentificationType = (resp.IdentificationType == null ? null : resp.IdentificationType);
                    result.MainMobile = (MainMobile == null ? null : MainMobile);
                    result.MainLandline = (MainLandline == null ? null : MainLandline);
                    result.MainAddress = (MainAddress == null ? null : MainAddress);
                    result.CustomerName = (resp.CustomerName == null ? null : resp.CustomerName);
                    result.FatherName = (resp.FathersName == null ? null : resp.FathersName);
                    result.ResidenceStatus = (resp.ResidenceStatus == null ? null : resp.ResidenceStatus);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AddException(long? requestId, string dataString)
        {
            var json = new { ErrorMessages = dataString };
            new UserLogDataAccess().UpdateUserLog(requestId.GetValueOrDefault(), CustomHelper.GetJson(json));
        }

        #region Test Screen
        [AuthOp(RoleTitle = new string[] { "u", "c" })]
        public ActionResult GetAccountScreen()
        {
            return View();
        }


        //public ActionResult SearchAccountTest(string AccountNo)
        //{
        //    CISSB.AccountInfoResponse resp = new CISSB.AccountInfoResponse();
        //    //NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();
        //    CustomerAccountVM lst = new CustomerAccountVM();
        //    CustomerCardDataAccess obj = new CustomerCardDataAccess();
        //    //var CardExist = obj.GetCustomerCardByAccountNo(AccountNo);
        //    // var CardExist = obj.GetCustomerCardByAccountNoCIF(AccountNo,CIF);
        //    var CardExist = CommonMethods.GetCustomerCardByAccountNo(AccountNo);
        //    resp = T24Methods.FetchAccount(AccountNo);
        //    if (CardExist.Count <= 0)
        //    {
        //        //resp = T24Methods.FetchAccount(AccountNo, CIF);
        //        resp = T24Methods.FetchAccount(AccountNo);
        //        if (string.IsNullOrEmpty(resp.T24ACNO) || string.IsNullOrWhiteSpace(resp.AccountTitle))
        //            return Json(new { Result = resp, ErrorMessage = CustomMessages.AccountNotFound }, JsonRequestBehavior.AllowGet);

        //        return Json(new { Result = resp, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        //var CIFExist = CardExist.Where(x => x.CIFNo == CIF);
        //        //if (CIFExist.Count() <= 0)
        //        //{
        //        //    resp = T24Methods.FetchAccount(AccountNo, CIF);
        //        //    if (string.IsNullOrEmpty(resp.T24ACNO) || string.IsNullOrWhiteSpace(resp.AccountTitle))
        //        //        return Json(new { Result = resp, ErrorMessage = CustomMessages.AccountNotFound }, JsonRequestBehavior.AllowGet);
        //        //    if (resp.OpInstructions == "Joint")
        //        //    {
        //        //        return Json(new { Result = resp, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
        //        //    }
        //        //    else
        //        //    {
        //        //        return Json(new { Result = resp, ErrorMessage = CustomMessages.CardAlreadyExists }, JsonRequestBehavior.AllowGet);
        //        //    }
        //        //}
        //        //else
        //        //{
        //        //    return Json(new { Result = resp, ErrorMessage = CustomMessages.CardAlreadyExists }, JsonRequestBehavior.AllowGet);
        //        //}
        //        return Json(new { Result = resp, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
        //    }

        //    //return Json(resp, JsonRequestBehavior.AllowGet);
        //}
        #endregion
    }
}