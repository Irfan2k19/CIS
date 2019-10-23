using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.Core.Methods;
using CardIssuanceSystem.DAL.DataAccessClasses;
using CardIssuanceSystem.DAL;
using CardIssuanceSystem.Core.Helpers;
using System.IO;
using CardIssuanceSystem.Filters;
using System.Configuration;

namespace CardIssuanceSystem.Controllers
{
    public class CardHotMarkController : BaseController
    {
        // GET: CardHotMark
        #region Card Hot Mark Screen
        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult CardHotMarkScreen()
        {
            return View();
        }

        public JsonResult GetCardInfo(string CardNo)
        {
            CardTypesDataAccess obj = new CardTypesDataAccess();
            List<CustomerAccountVM> lst = new List<CustomerAccountVM>();

            lst = CardReportMethods.GetCustomerAccount(CardNo);

            if (lst.Count <= 0)
            {
                return Json(new { Result = lst, ErrorMessage = CustomMessages.CardNotExists }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                var CardIssuance = lst.FirstOrDefault().CardIssuance.ToString();
                var CardExpiry = lst.FirstOrDefault().CardExpiry.ToString();
                var DOB = lst.FirstOrDefault().DateofBirth.ToString();
                var CardTypes = obj.GetCardTypes("A");
                CardTypes = (CardTypes.Where(s => s.ID == lst.FirstOrDefault().CardTypeID)).ToList();
                var CardType = "";
                if (CardTypes.Count!=0)
                {
                    CardType = CardTypes[0].Title;
                }
            
                List<CustomerAccountVM> DelinkInfo = new List<CustomerAccountVM>();
                DelinkInfo = CommonMethods.GetAccountInfo(CardNo, lst.FirstOrDefault()?.AccountNo ?? "0");

                return Json(new { Result = lst, CardIssuance = CardIssuance, CardExpiry = CardExpiry, DOB = DOB, DelinkInfo = DelinkInfo, CardType = CardType, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
        }

        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult SubmitHotMarkRequest(string CardNo)
        {
            long ID = 0;
            if (IsDuplicateEntry(CardNo, "", "H", null, out ID))
                return Json(new { IsSuccess = ID, ErrorMessage = CustomMessages.RequestAlreadyExists }, JsonRequestBehavior.AllowGet);

            bool flag = false;
            CustomerCardDataAccess obj = new CustomerCardDataAccess();
            var AlreadyHot = obj.IsCardHotMarked(CardNo);
            if (AlreadyHot== "Success")
            {
                flag = CardHotMarkMethods.GenerateHotMarkRequest(CardNo);



                return Json(new {IsSuccess=true,ErrorMessage=CustomMessages.Success}, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { IsSuccess = false, ErrorMessage =AlreadyHot }, JsonRequestBehavior.AllowGet);
            }
            

        }
        #endregion

        #region Authorize Hot Marking
        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AuthorizeHotMarking()
        {
            return View();
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult _AuthorizeHotMarking(string CardNo = "")
        {
            List<HotMarkVM> model = new List<HotMarkVM>();
            model = CardHotMarkMethods.SearchHotMarkRequest(CardNo);
            return View(model);
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult ApproveHotMarkRequest(string CardNo, long RequestId)
        {
            //var getSystemRequest = new SystemRequestDataAccess().GetSystemRequest(int.Parse(RequestId.ToString()), "P");
            //Checking if Creator is same as Authorizer
            //if (getSystemRequest.CreatorID == StateHelper.UserId)
            //{
            //    return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.SameCreatorAndsAuthorizer }, JsonRequestBehavior.AllowGet);
            //}

            bool CheckAuthorizerSame = new RequestDataAccess().CheckSameAuthorizer(StateHelper.UserId,Convert.ToInt32(RequestId));
            if (CheckAuthorizerSame)
            {
                return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.SameCreatorAndsAuthorizer }, JsonRequestBehavior.AllowGet);
            }

            else
            {
                bool flag = false;
                CustomerCardDataAccess result = new CustomerCardDataAccess();
                var AlreadyHot = result.IsCardHotMarked(CardNo);
                if (AlreadyHot == "Success")
                {
                    CardHotMarkMethods.AuthorizeRequest(CardNo, RequestId, "A");
                    flag = result.CardHotMark(CardNo);
                    return Json(new { IsSuccess = true, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { IsSuccess = false, ErrorMessage = AlreadyHot }, JsonRequestBehavior.AllowGet);
                }
            }
            
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult RejectHotMarkRequest(string CardNo, long RequestId)
        {
            bool CheckAuthorizerSame = new RequestDataAccess().CheckSameAuthorizer(StateHelper.UserId, Convert.ToInt32(RequestId));
            if (CheckAuthorizerSame)
            {
                return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.SameCreatorAndsAuthorizer }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                bool flag = false;

                flag = CardHotMarkMethods.AuthorizeRequest(CardNo, RequestId, "C");
                if (flag == true)
                {
                    return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { IsSuccess = false, ErrorMessage = CustomMessages.GenericErrorMessage }, JsonRequestBehavior.AllowGet);
                }
            }
            

            //return Json(flag, JsonRequestBehavior.AllowGet);
        }

        #endregion

        [AuthOp(RoleTitle = new string[] { "c" })]
        [HttpPost]
        public ActionResult AutomaticHotMark(HttpPostedFileBase postedFile)
        {
            long ID = 0;
            CustomerCardDataAccess result = new CustomerCardDataAccess();
            //flag = result.CardHotMark(CardNo);

            string filePath = string.Empty;
            try
            {
                //if (postedFile != null)
                //{
                var filePathData = new FilePathDataAccess().GetFilePathByTypeID(((int)Constants.enPathType.Import).ToString());
                string path = filePathData?.Path ?? @"C:\Users\Mohsin\Desktop\ImportedFile\";
                string filename = "Hotmark.csv";
                //string path = Server.MapPath("~/CardHotMark/");
                int found = 0, notFound = 0, alreadyhot = 0, requestexist = 0;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                filePath = path + Path.GetFileName(filename);
                string extension = Path.GetExtension(filename);
                if (extension == ".csv" || extension == ".CSV")
                {
                    //postedFile.SaveAs(filePath);
                    string csvData = System.IO.File.ReadAllText(filePath).Replace("\r\n", "$$&$$");
                    //var data = csvData.Split('\n', '\r');
                    var data = csvData.Split(new[] { "$$&$$" }, StringSplitOptions.None).Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()).ToArray();
                    var AllowedQuantity= ConfigurationManager.AppSettings["HotFile"];
                   int  AllowedQuantityInt = Convert.ToInt32(AllowedQuantity);
                    if (Convert.ToInt32(data.Count()) -1 > AllowedQuantityInt)
                    {
                        return Json("Please Upload maximum of "+ AllowedQuantityInt + " records", JsonRequestBehavior.AllowGet);
                    }
                    foreach (string row in data.Skip(1).ToList())
                    {
                        //foreach (var item in row.Split(','))
                        //{
                        if (string.IsNullOrWhiteSpace(row))
                            continue;

                        var datarow = row.Split(',');
                        if (datarow.Length >= 2)
                        {
                            var cardno = datarow[0];
                            var expiry = new DateTime(int.Parse(datarow[1].Substring(0, 4)), int.Parse(datarow[1].Substring(4, 2)), int.Parse(datarow[1].Substring(6, 2)), 0, 0, 0);
                            if (!string.IsNullOrEmpty(cardno))
                            {
                                if (IsDuplicateEntry(cardno, "", "H", null, out ID))
                                {
                                    //notFound++;
                                    requestexist++;
                                    continue;
                                }

                                var AlreadyHot = new CustomerCardDataAccess().IsCardHotMarked(cardno);
                                if (AlreadyHot == "Success")
                                {
                                    var isSuccess = CardHotMarkMethods.GenerateHotMarkRequest(cardno, expiry);
                                    //var isSuccess = result.CardHotMark(cardno, expiry);
                                    if (!isSuccess)
                                        notFound++;
                                    else
                                        found++;
                                }
                                else
                                    //notFound++;
                                    alreadyhot++;
                            }
                        }
                        else {

                            return Json("Please Select Valid .csv file", JsonRequestBehavior.AllowGet);
                        }

                        //}
                    }
                    //TempData["mesage"] = "Sucessfully Retreived";
                    return Json($"Succesfull: {found}, Card Data Not Found: {notFound},Card Already Hot: {alreadyhot},Request Already Exists: {requestexist}", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //TempData["mesage"] = "Please Select Valid .csv file";
                    return Json("Please Select Valid .csv file", JsonRequestBehavior.AllowGet);
                }
                //}
            }
            catch (Exception ex)
            {
                //TempData["mesage"] = "Path or File Not Found";
                return Json("Path or File Not Found", JsonRequestBehavior.AllowGet);
                //throw ex;
            }
            // return View("CardHotMarkScreen");
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        [HttpPost]
        public ActionResult ApproveMultipleHotMarkRequest(List<HotMarkVM> request)
        {
            bool flag = false;
            int rejectCardsCount = 0, successCardsCount = 0;
            CustomerCardDataAccess result = new CustomerCardDataAccess();
            foreach (var item in request)
            {
                var AlreadyHot = result.IsCardHotMarked(item.CardNo);
                if (AlreadyHot == "Success")
                {
                    CardHotMarkMethods.AuthorizeRequest(item.CardNo, item.RequestId, "A");
                    flag = result.CardHotMark(item.CardNo);
                    successCardsCount++;
                }
                else
                {
                    rejectCardsCount++;
                }
            }

            return Json(new { IsSuccess = false, ErrorMessage = string.Format(CustomMessages.MultipleHotmarkApproval, successCardsCount, rejectCardsCount) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RejectMultipleHotMarkRequest(List<HotMarkVM> request)
        {

           
           
            bool flag = false;
            int rejectCardsCount = 0, successCardsCount = 0;
            CustomerCardDataAccess result = new CustomerCardDataAccess();
            foreach (var item in request)
            {
               
                
                    CardHotMarkMethods.AuthorizeRequest(item.CardNo, item.RequestId, "C");
                    flag = result.CardHotMark(item.CardNo);
                    rejectCardsCount++;
               
            }

            return Json(new { IsSuccess = false, ErrorMessage = string.Format(CustomMessages.MultipleHotmarkApproval, successCardsCount, rejectCardsCount) }, JsonRequestBehavior.AllowGet);
        }
        



        private bool IsDuplicateEntry(string cardNumber, string accountNumber, string requestType, string CIF, out long requestId)
        {
            return new RequestDataAccess().IsDuplicateRequest(cardNumber ?? string.Empty, accountNumber ?? string.Empty, requestType ?? string.Empty, CIF ?? string.Empty, out requestId);
        }


        #region Card ReActivation
        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult CardActivationScreen()
        {
            return View();
        }

        public JsonResult GetHotCardInfo(string CardNo)
        {
            CardTypesDataAccess obj = new CardTypesDataAccess();
            List<CustomerAccountVM> lst = new List<CustomerAccountVM>();

            lst = CardReportMethods.GetCustomerAccount(CardNo);

            if (lst.Count <= 0)
            {
                return Json(new { Result = lst, ErrorMessage = CustomMessages.CardNotExists }, JsonRequestBehavior.AllowGet);
            }
            else if(lst.Count > 0 && lst.FirstOrDefault().CardStatusActive == true)
            {
                return Json(new { Result = lst, ErrorMessage = "Card is Already Active" }, JsonRequestBehavior.AllowGet);
            } else
            {

                var CardIssuance = lst.FirstOrDefault().CardIssuance.ToString();
                var CardExpiry = lst.FirstOrDefault().CardExpiry.ToString();
                var DOB = lst.FirstOrDefault().DateofBirth.ToString();
                var CardTypes = obj.GetCardTypes("A");
                CardTypes = (CardTypes.Where(s => s.ID == lst.FirstOrDefault().CardTypeID)).ToList();
                var CardType = "";
                if (CardTypes.Count != 0)
                {
                    CardType = CardTypes[0].Title;
                }

                List<CustomerAccountVM> DelinkInfo = new List<CustomerAccountVM>();
                DelinkInfo = CommonMethods.GetAccountInfo(CardNo, lst.FirstOrDefault()?.AccountNo ?? "0");

                return Json(new { Result = lst, CardIssuance = CardIssuance, CardExpiry = CardExpiry, DOB = DOB, DelinkInfo = DelinkInfo, CardType = CardType, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
        }

        [AuthOp(RoleTitle = new string[] { "c" })]
        public ActionResult SubmitActivationRequest(string CardNo)
        {
            long ID = 0;
            if (IsDuplicateEntry(CardNo, "", "W", null, out ID))
                return Json(new { IsSuccess = ID, ErrorMessage = CustomMessages.RequestAlreadyExists }, JsonRequestBehavior.AllowGet);

            bool flag = false;
            CustomerCardDataAccess obj = new CustomerCardDataAccess();
            var AlreadyHot = obj.IsCardHotMarked(CardNo);
            if (AlreadyHot == "Card Already Hot Marked")
            {
                flag = CardHotMarkMethods.GenerateActivationRequest(CardNo);
                return Json(new { IsSuccess = true, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { IsSuccess = false, ErrorMessage = "Card is Active" }, JsonRequestBehavior.AllowGet);
            }


        }

        #endregion


        #region Authorize Activation

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult AuthorizeActivationScreen()
        {
            return View();
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult _AuthorizeActivationScreen(string CardNo = "")
        {
            List<HotMarkVM> model = new List<HotMarkVM>();
            model = CardHotMarkMethods.SearchActiveMarkRequest(CardNo);
            return View(model);
        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        public ActionResult ApproveActiveMarkRequest(string CardNo, long RequestId)
        {
            bool flag = false;
            CustomerCardDataAccess result = new CustomerCardDataAccess();
            var AlreadyHot = result.IsCardHotMarked(CardNo);
            if (AlreadyHot == "Card Already Hot Marked")
            {
                CardHotMarkMethods.AuthorizeRequest(CardNo, RequestId, "A");
                flag = result.CardActiveMark(CardNo);
                return Json(new { IsSuccess = true, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { IsSuccess = false, ErrorMessage = AlreadyHot }, JsonRequestBehavior.AllowGet);
            }

        }

        [AuthOp(RoleTitle = new string[] { "u" })]
        [HttpPost]
        public ActionResult ApproveMultipleActiveMarkRequest(List<HotMarkVM> request)
        {
            bool flag = false;
            int rejectCardsCount = 0, successCardsCount = 0;
            CustomerCardDataAccess result = new CustomerCardDataAccess();
            foreach (var item in request)
            {
                var AlreadyHot = result.IsCardHotMarked(item.CardNo);
                if (AlreadyHot == "Card Already Hot Marked")
                {
                    CardHotMarkMethods.AuthorizeRequest(item.CardNo, item.RequestId, "A");
                    flag = result.CardActiveMark(item.CardNo);
                    successCardsCount++;
                }
                else
                {
                    rejectCardsCount++;
                }
            }

            return Json(new { IsSuccess = false, ErrorMessage = string.Format(CustomMessages.MultipleActivemarkApproval, successCardsCount, rejectCardsCount) }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}