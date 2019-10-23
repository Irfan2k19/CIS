using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.Helpers
{
    public class DBConString
    {
        public static string SoneriCISDBCon
        {
            get
            {
                string myCon = "";
                if (ConfigurationManager.ConnectionStrings["SoneriCISEntities"] == null)
                {
                    return null;
                }
                {
                    if (ConfigurationManager.ConnectionStrings["SoneriCISEntities"].ToString() == "")
                    {
                        return null;
                    }
                    else
                    {
                        string[] Conn = ConfigurationManager.ConnectionStrings["SoneriCISEntities"].ToString().Split(';');
                        string pwd = Conn[5].ToString().Replace("password=", "");
                        myCon = Conn[0].ToString() + ";" + Conn[1].ToString() + ";" + Conn[2].ToString() + ";" + Conn[3].ToString() + ";" + Conn[4].ToString() + ";password=" + EncryptDecrypt.Decrypt(pwd) + ";" + Conn[6].ToString() + ";" + Conn[7].ToString();
                        return myCon;
                    }
                }
                return myCon;
            }
        }
    }

    
}
