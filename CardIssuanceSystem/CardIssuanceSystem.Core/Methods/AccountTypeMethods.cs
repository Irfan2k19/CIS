using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL.DataAccessClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.Methods
{
    public class AccountTypeMethods
    {
        public static AccountTypeVM GetAccountTypeById(int id, string authorizeStatus = default(string))
        {
            var query = new AccountTypeDataAccess().GetAccountTypeById(id);
            if (!string.IsNullOrEmpty(authorizeStatus))
                query = (query?.AuthorizationStatus ?? string.Empty) == authorizeStatus ? query : null;
            if (query == null)
                return null;

            return new AccountTypeVM
            {
                Code = query.Code,
                Description = query.Description,
                AuthorizationStatus = query.AuthorizationStatus,
                Name = query.Name,
                ID = query.ID,
                AuthorizationComments = query.AuthorizationComments,
                IsActive = query.IsActive
            };
        }

        public static List<AccountTypeVM> GetAllAccountTypes()
        {
            return new AccountTypeDataAccess().GetAllAccountTypes().Select(e => new AccountTypeVM
            {
                ID = e.ID,
                AuthorizationComments = e.AuthorizationComments,
                AuthorizationStatus = e.AuthorizationStatus,
                Code = e.Code,
                Description = e.Description,
                IsActive = e.IsActive,
                Name = e.Name
            }).ToList();
        }
    }
}
