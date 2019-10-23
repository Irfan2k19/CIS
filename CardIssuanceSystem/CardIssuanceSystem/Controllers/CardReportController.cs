using CardIssuanceSystem.Core.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.Core.Helpers;
using CardIssuanceSystem.DAL;
using CardIssuanceSystem.DAL.DataAccessClasses;
using CardIssuanceSystem.Filters;


namespace CardIssuanceSystem.Controllers
{
    public class CardReportController : Controller
    {
        [AuthOp(RoleTitle = new string[] { "u", "c" })]
        public ActionResult CardReportScreen()
        {
            return View();
        }

        public JsonResult SearchCardDetail(string CardNo)
        {
            CardTypesDataAccess obj = new CardTypesDataAccess();
            List<CustomerAccountVM> lst = new List<CustomerAccountVM>();
            NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType();

            lst = CardReportMethods.GetCustomerAccount(CardNo);

            if (lst.Count <= 0)
            {
                return Json(new { Result = lst, ErrorMessage = CustomMessages.CardNotExists }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var request = new RequestDataAccess().GetRequest(CardNo, lst.FirstOrDefault()?.AccountNo ?? string.Empty, lst.FirstOrDefault()?.CIF ?? string.Empty);

                var requestdata = new RequestVM
                {
                    ID = request?.ID ?? 0,
                    RequestType = request?.RequestType ?? string.Empty,
                    Waive = request != null ? request.WaiveCharges.GetValueOrDefault() : false
                };

                var userlog = new UserLogDataAccess().GetUserLogByEntityId(request?.ID ?? 0, "tbl_Requests");
                var creator = userlog.Where(e => e.EventName == "i").FirstOrDefault()?.tbl_Users?.UserName ?? string.Empty;
                var authorizer = userlog.Where(e => e.EventName == "A").OrderByDescending(e => e.ID).FirstOrDefault()?.tbl_Users?.UserName ?? string.Empty;

                try
                {
                    resp = T24Methods.FetchAccount(lst.FirstOrDefault()?.AccountNo ?? string.Empty, lst.FirstOrDefault()?.CIF ?? string.Empty);
                }
                catch { resp = new NewCISSB.SONFETCHCARDINFOTypeGSONFETCHCARDINFODetailTypeMSONFETCHCARDINFODetailType(); }

                //var CardIssuance = lst.FirstOrDefault().CardIssuance.ToString();
                //var CardExpiry = lst.FirstOrDefault().CardExpiry.ToString();
                //var DOB = lst.FirstOrDefault().DateofBirth.ToString();
                var CardTypes = obj.GetCardTypes("A");
                CardTypes = (CardTypes.Where(s => s.ID == (lst.FirstOrDefault()?.CardTypeID ?? 0))).ToList();
                var CardType = (CardTypes.Count > 0) ? CardTypes[0].Title : string.Empty;
                List<CustomerAccountVM> DelinkInfo = new List<CustomerAccountVM>();
                //DelinkInfo = CommonMethods.GetAccountInfo(CardNo, lst.FirstOrDefault()?.AccountNo ?? "0");
               var  DelinkInfoStr = CommonMethods.GetLinkedAccounts(CardNo,lst.FirstOrDefault()?.AccountNo ?? "0");

                var Product = new AccountTypeDataAccess().GetAccountTypeById(lst.FirstOrDefault().AccountTypeID ?? 0)?.Name ?? string.Empty;
                return Json(new { Result = lst,/*CardIssuance=CardIssuance, CardExpiry= CardExpiry,DOB=DOB,*/DelinkInfoStr = DelinkInfoStr, CardType = CardType, Product = Product, Request = requestdata, AccountResponse = resp, Creator = creator, Authorizer = authorizer, ErrorMessage = CustomMessages.Success }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}