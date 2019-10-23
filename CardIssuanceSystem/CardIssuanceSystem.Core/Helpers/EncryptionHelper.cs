using System;
using System.Security.Cryptography;
using System.Text;

namespace CardIssuanceSystem.Core.Helpers
{
    public class EncryptionHelper
    {
        #region Constant(s)
        private const string SALT_PREFIX_SUFFIX_FORMAT = "gU{SoneriCIS}iD";
        #endregion

        public static string HashString(string phrase)
        {
            SHA512Managed HashTool = new SHA512Managed();
            Byte[] PasswordAsByte = Encoding.UTF8.GetBytes(string.Concat(phrase, SALT_PREFIX_SUFFIX_FORMAT));
            Byte[] EncryptedBytes = HashTool.ComputeHash(PasswordAsByte);
            HashTool.Clear();
            return Convert.ToBase64String(EncryptedBytes);
        }
    }
}
