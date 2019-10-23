using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL.DataAccessClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.Methods
{
    public class CardChargesMethod
    {
        public static CardChargesVM GetCardChargeDetail(int requestID, string authorizeStatus = default(string))
        {
            var data = new CardChargesDataAccess().GetCardChargeById(requestID);
            if (!string.IsNullOrEmpty(authorizeStatus))
                data = (data?.AuthorizationStatus ?? string.Empty) == authorizeStatus ? data : null;
            if (data == null)
                return null;

            return new CardChargesVM
            {
                ID = data.ID,
                AccountTypeID = data.AccountTypeID,
                Amount = data.Amount,
                CardTypeID = data.CardTypeID,
                Frequency = data.Frequency,
                IsActive = data.IsActive,
                Title = data.Title,
                IsFED = data.IsFED,
                IsReplacement = data.IsReplacement
            };
        }

        public static CardChargesVM GetCardChargeDetail(int cardTypeId, int accountTypeId, string authorizeStatus = default(string))
        {
            var data = new CardChargesDataAccess().GetCardChargesByCardNAccount(accountTypeId, cardTypeId).FirstOrDefault();
            if (!string.IsNullOrEmpty(authorizeStatus))
                data = (data?.AuthorizationStatus ?? string.Empty) == authorizeStatus ? data : null;
            if (data == null)
                return null;

            return new CardChargesVM
            {
                ID = data.ID,
                AccountTypeID = data.AccountTypeID,
                Amount = data.Amount,
                CardTypeID = data.CardTypeID,
                Frequency = data.Frequency,
                IsActive = data.IsActive,
                Title = data.Title,
                IsFED = data.IsFED,
                IsReplacement = data.IsReplacement
            };
        }

        public static RegionalChargesVM GetRegionalChargesDetail(int cardChargeID, int regionID)
        {
            var data = new CardChargesDataAccess().GetRegionalCharges(cardChargeID, regionID);
            if (data == null)
                return null;

            return new RegionalChargesVM
            {
                PercentageAmount = data.PercentageAmount
            };
        }

        public static List<CardChargesVM> GetCardChargeByCardTypeAccount(int cardTypeId, int accountTypeId)
        {
            var data = new CardChargesDataAccess().GetCardChargeByCardTypeAccount(cardTypeId, accountTypeId);

            return data.Select(e => new CardChargesVM
            {
                ID = e.ID,
                Title = e.Title
            }).ToList();
        }
    }
}
