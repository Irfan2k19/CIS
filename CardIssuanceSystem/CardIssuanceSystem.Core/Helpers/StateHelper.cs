using CardIssuanceSystem.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;

namespace CardIssuanceSystem.Core.Helpers
{
    public class StateHelper
    {
        public static HttpContext Context
        {
            get
            {
                return HttpContext.Current;
            }
        }

        public static HttpSessionState Session
        {
            get
            {
                return HttpContext.Current.Session;
            }
        }

        public static int UserId
        {
            get
            {
                if (Session[StateKeys.UserId] == null)
                    return 0;
                return Convert.ToInt32(Session[StateKeys.UserId]);
            }
            set
            {
                Context.Session[StateKeys.UserId] = value;
            }
        }

        public static string RoleId
        {
            get
            {
                if (Session[StateKeys.RoleId] == null)
                    return string.Empty;
                return Convert.ToString(Session[StateKeys.RoleId]);
            }
            set
            {
                Context.Session[StateKeys.RoleId] = value;
            }
        }

        public static string Username
        {
            get
            {
                if (Session[StateKeys.Username] == null)
                    return string.Empty;
                return Convert.ToString(Session[StateKeys.Username]);
            }
            set
            {
                Context.Session[StateKeys.Username] = value;
            }
        }

        public static List<PageSectionVM> Pages
        {
            get
            {
                if (Session[StateKeys.Pages] == null)
                    return new List<PageSectionVM>();
                return (List<PageSectionVM>)Session[StateKeys.Pages];
            }
            set
            {
                Context.Session[StateKeys.Pages] = value;
            }
        }
    }

    public static class StateKeys
    {
        public const string UserId = "UserId";
        public const string RoleId = "RoleId";
        public const string Username = "Username";
        public const string Pages = "Pages";
    }
}
