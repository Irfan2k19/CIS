using CardIssuanceSystem.Core.ViewModel;
using CardIssuanceSystem.DAL.DataAccessClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.Methods
{
    public class AuthorizationMethods
    {
        public static CustomerCardVM GetCustomerCardByAccountNo(string accountNumber)
        {
            var query = new CustomerCardDataAccess().GetCustomerCardByAccountNo(accountNumber);
            if (query == null)
                return null;

            return new CustomerCardVM
            {
                CardNo = query.CardNo,
                CardTypeID = query.CardTypeID
            };
        }
    }
}
