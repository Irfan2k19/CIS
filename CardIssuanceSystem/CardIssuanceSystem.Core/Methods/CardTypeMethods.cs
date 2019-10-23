using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL.DataAccessClasses;
using System.Collections.Generic;
using System.Linq;

namespace CardIssuanceSystem.Core.Methods
{
    public class CardTypeMethods
    {
        public static CardTypeVM GetCardTypeById(int id, string authorizeStatus = default(string))
        {
            var query = new CardTypesDataAccess().GetCardType(id);
            if (!string.IsNullOrEmpty(authorizeStatus))
                query = (query?.AuthorizationStatus ?? string.Empty) == authorizeStatus ? query : null;
            if (query == null)
                return null;

            return new CardTypeVM
            {
                Description = query.Description,
                AuthorizationStatus = query.AuthorizationStatus,
                ID = query.ID,
                AuthorizationComments = query.AuthorizationComments,
                IsActive = query.IsActive,
                Title = query.Title,
                CardCode = query.CardCode,
                IsIssuance = query.IsIssuance,
                IsReplacement = query.IsReplacement,
                IsUpgrade = query.IsUpgrade,
                UpgradeChildTypes = query.tbl_Card_Upgrade_Types.Select(e => e.ParentCardType).ToList(),
                ParentTypes = new CardTypesDataAccess().GetParentCardTypesByChildId(query.ID)
            };
        }

        public static List<CardTypeVM> GetAllCardTypes()
        {
            return new CardTypesDataAccess().GetAllCardTypes().Select(e => new CardTypeVM
            {
                ID = e.ID,
                AuthorizationComments = e.AuthorizationComments,
                AuthorizationStatus = e.AuthorizationStatus,
                Description = e.Description,
                IsActive = e.IsActive,
                Title = e.Title
            }).ToList();
        }
    }
}
