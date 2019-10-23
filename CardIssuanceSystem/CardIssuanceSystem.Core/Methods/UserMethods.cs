using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL.DataAccessClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.Methods
{
    public class UserMethods
    {
        public static UserVM GetUserDetailsByID(int? ID)
        {
            var query = new UserDataAccess().GetUserById(ID.GetValueOrDefault());
            return new UserVM
            {
                ID = query.ID,
                AuthorizationComments = query.AuthorizationComments,
                AuthorizationStatus = query.AuthorizationStatus,
                IsActive = query.IsActive,
                RoleTitle = query.RoleTitle,
                UserLogin = query.UserLogin,
                UserName = query.UserName,
                UserPassword = query.UserPassword,
                ExpiryDate = query.ExpiryDate,
                EmpCode=query.EmpCode

            };
        }
    }
}
