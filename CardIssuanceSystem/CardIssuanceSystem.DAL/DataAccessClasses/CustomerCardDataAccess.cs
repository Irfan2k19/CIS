using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.DAL.DataAccessClasses
{
    public class CustomerCardDataAccess
    {

        #region insert 
        public bool AddCustomerCard(tbl_Customer_Cards row)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Customer_Cards.Add(row);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool AddMultipleCustomerCard(List<tbl_Customer_Cards> rows)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    db.tbl_Customer_Cards.AddRange(rows);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Update
        public bool UpdateCustomerCard(tbl_Customer_Cards row)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Customer_Cards val = new DAL.tbl_Customer_Cards();
                    val = db.tbl_Customer_Cards.Where(a => a.ID == row.ID).FirstOrDefault();
                    val.CardNo = row.CardNo;
                    val.CardStatusActive = row.CardStatusActive;
                    val.CardTypeID = row.CardTypeID;
                    val.AccountNo = row.AccountNo;
                    val.WaiveCharges = row.WaiveCharges;
                    val.Salutation = row.Salutation;
                    val.CardTitle = row.CardTitle;
                    val.CardExpiry = row.CardExpiry;
                    val.CardIssuance = row.CardIssuance;
                    val.CIF = row.CIF;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool UpdateCustomerCardType(string CardNo,int? CardTypeId)
        {

            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Customer_Cards val = new DAL.tbl_Customer_Cards();
                    val = db.tbl_Customer_Cards.Where(a => a.CardNo==CardNo).FirstOrDefault();
                    val.CardTypeID = CardTypeId;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Delete

        public bool CardHotMark(string CardNo)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Customer_Cards val = new DAL.tbl_Customer_Cards();
                    val = db.tbl_Customer_Cards.Where(a => a.CardNo == CardNo).FirstOrDefault();
                    val.CardNo = val.CardNo;
                    val.CardStatusActive = false;
                    val.CardTypeID = val.CardTypeID;
                    val.AccountNo = val.AccountNo;
                    val.WaiveCharges = val.WaiveCharges;
                    val.Salutation = val.Salutation;
                    val.CardTitle = val.CardTitle;
                    val.CardExpiry = val.CardExpiry;
                    val.CardIssuance = val.CardIssuance;
                    val.CIF = val.CIF;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool CardHotMark(string CardNo, DateTime? cardExpiry)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Customer_Cards val = new DAL.tbl_Customer_Cards();
                    val = db.tbl_Customer_Cards.Where(a => a.CardNo == CardNo && a.CardExpiry == cardExpiry && a.CardStatusActive == true).FirstOrDefault();
                    val.CardNo = val.CardNo;
                    val.CardStatusActive = false;
                    val.CardTypeID = val.CardTypeID;
                    val.AccountNo = val.AccountNo;
                    val.WaiveCharges = val.WaiveCharges;
                    val.Salutation = val.Salutation;
                    val.CardTitle = val.CardTitle;
                    val.CardExpiry = val.CardExpiry;
                    val.CardIssuance = val.CardIssuance;
                    val.CIF = val.CIF;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }



        public bool CardActiveMark(string CardNo)
        {
            try
            {
                using (var db = new SoneriCISEntities())
                {
                    tbl_Customer_Cards val = new DAL.tbl_Customer_Cards();
                    val = db.tbl_Customer_Cards.Where(a => a.CardNo == CardNo).FirstOrDefault();
                    val.CardNo = val.CardNo;
                    val.CardStatusActive = true;
                    val.CardTypeID = val.CardTypeID;
                    val.AccountNo = val.AccountNo;
                    val.WaiveCharges = val.WaiveCharges;
                    val.Salutation = val.Salutation;
                    val.CardTitle = val.CardTitle;
                    val.CardExpiry = val.CardExpiry;
                    val.CardIssuance = val.CardIssuance;
                    val.CIF = val.CIF;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region select
        public List<tbl_Customer_Cards> GetAllCustomerCards()
        {
            try
            {
                List<tbl_Customer_Cards> lst = new List<DAL.tbl_Customer_Cards>();
                using (var db = new SoneriCISEntities())
                {
                    lst = db.tbl_Customer_Cards.ToList();
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Customer_Cards GetCustomerCard(string CardNo)
        {
            try
            {
                tbl_Customer_Cards card = new tbl_Customer_Cards();

                using (var db = new SoneriCISEntities())
                {
                    card = db.tbl_Customer_Cards.Where(a=>a.CardNo==CardNo).FirstOrDefault();
                }
                return card;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Customer_Cards GetCustomerCardByAccountNo(string AccountNo)
        {
            try
            {
                tbl_Customer_Cards card = new tbl_Customer_Cards();

                using (var db = new SoneriCISEntities())
                {
                    card = db.tbl_Customer_Cards.Where(a => a.AccountNo == AccountNo).FirstOrDefault();
                }
                
                return card;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public tbl_Customer_Cards GetCustomerActiveCardByAccountNo(string AccountNo)
        {
            try
            {
                tbl_Customer_Cards card = new tbl_Customer_Cards();

                using (var db = new SoneriCISEntities())
                {
                    card = db.tbl_Customer_Cards.Where(a => a.AccountNo == AccountNo && a.CardStatusActive==true).FirstOrDefault();
                }

                return card;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool ActiveCardExists(string AccountNo)
        {
            try
            {
                tbl_Customer_Cards card = new tbl_Customer_Cards();

                using (var db = new SoneriCISEntities())
                {
                    card = db.tbl_Customer_Cards.Where(a => a.AccountNo == AccountNo && a.CardStatusActive == true).FirstOrDefault();
                }
                if (card == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string IsCardHotMarked(string CardNo)
        {
            try
            {
                string result = "";
                tbl_Customer_Cards card = new tbl_Customer_Cards();

                using (var db = new SoneriCISEntities())
                {
                    card = db.tbl_Customer_Cards.Where(a => a.CardNo == CardNo).FirstOrDefault();
                }
                if (card !=null)
                {
                    if (card.CardStatusActive == true)
                    {
                        //return false;
                        result = "Success";
                    }
                    else
                    {
                        //return true;
                        result = "Card Already Hot Marked";
                    }
                }else
                {
                    result = "Card Not Exist";
                }

                return result;
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool IsCustomerCardByAccountNoExists(string AccountNo, string CardNo)
        {
            try
            {
                bool card = false;

                using (var db = new SoneriCISEntities())
                {
                    card = db.tbl_Customer_Cards.Any(a => a.AccountNo == AccountNo && a.CardNo != CardNo && a.CardStatusActive == true);
                }

                return card;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool IsCustomerCardByAccountNoExists(string AccountNo,string CIF, string CardNo)
        {
            try
            {
                bool card = false;

                using (var db = new SoneriCISEntities())
                {
                    card = db.tbl_Customer_Cards.Any(a => a.AccountNo == AccountNo && a.CIF==CIF && a.CardNo != CardNo && a.CardStatusActive == true);
                }

                return card;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public List<tbl_Customer_Cards> GetCustomerCardByAccountNoCIF(string AccountNo,string CIF)
        {
            try
            {
                List<tbl_Customer_Cards> card = new List<tbl_Customer_Cards>();

                using (var db = new SoneriCISEntities())
                {
                    //card = db.tbl_Customer_Cards.Where(a => a.AccountNo == AccountNo).FirstOrDefault();
                    card = (from cc in db.tbl_Customer_Cards
                            join ca in db.tbl_Customer_Accounts on cc.AccountNo equals ca.AccountNo
                            where cc.AccountNo == AccountNo && ca.CIF == CIF && ca.AccountNo == AccountNo 
                            //&& cc.CardStatusActive==true
                            select cc).ToList();
                }
                return card;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string CardReplacementValid(string AccountNo, string CIF)
        {
            try
            {
                List<tbl_Customer_Cards> lstActiveCards = new List<DAL.tbl_Customer_Cards>();
                List<tbl_Customer_Cards> lstInActiveCards = new List<DAL.tbl_Customer_Cards>();
                tbl_Customer_Cards card = new tbl_Customer_Cards();

                using (var db = new SoneriCISEntities())
                {
                    lstActiveCards = db.tbl_Customer_Cards.Where(a => a.AccountNo == AccountNo && a.CIF == CIF && a.CardStatusActive == true).ToList();
                    lstInActiveCards = db.tbl_Customer_Cards.Where(a => a.AccountNo == AccountNo && a.CIF == CIF && a.CardStatusActive == false).ToList();
                }
                if (lstActiveCards.Count > 0)
                    throw new CustomException("Primary/Secondry card not mark as hot");

                else if (lstActiveCards.Count == 0 && lstInActiveCards.Count == 0)
                    throw new CustomException("Primary card does not exist.");

                if (lstActiveCards.Count == 0 && lstInActiveCards.Count > 0)
                {
                    string CardNo = Convert.ToString(lstInActiveCards.OrderByDescending(a => a.CardIssuance).ThenByDescending(b => b.ID).FirstOrDefault()?.CardNo);
                    if (string.IsNullOrEmpty(CardNo))
                    {
                        CardNo = "0";
                    }

                    return CardNo;
                }
                else
                {
                    return "0";
                }
            }
            catch (CustomException cex) { throw; }
            catch (Exception ex)
            {
                return "0";
            }
        }

        #endregion
    }
}
