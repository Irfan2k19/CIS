using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.Helpers
{
    public class GNUPG_Wrapper
    {
        #region CONSTANTS
        /// <summary>
        /// GNUPG Configuration Parameters
        /// =======================
        /// Change them according to your Computer
        /// </summary>
        private const string intendedRecipientKey = "sonericis@viftech.com.pk";
       // private const string intendedRecipientKey = "soneri@soneri.com";
        private const string programPath = @"C:\gnupg\";

        // Change the path of the below bat files to which are present in your computer
        private const string encryptBatFilePath = @"C:\Program Files (x86)\gnupg\bin\Encrypt.bat";
        private const string decryptWithoutPassFilePath = @"C:\Program Files (x86)\gnupg\bin\DecryptWithoutPass.bat";
        private const string decryptBatFilePath = @"C:\Program Files (x86)\gnupg\bin\Decrypt.bat";

        #endregion

        /// <summary>
        /// GNUPG Encypt Method
        /// ====================
        /// This method takes the path of file to be encypted
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <returns></returns>
        public string Encrypt(string inputFilePath, string filename)
        {
            string OutputPath = $"{Path.GetDirectoryName(inputFilePath)}\\{filename}";
            var isSuccess = ExecuteCommandSync("CMD", "/c gpg --recipient \"" + intendedRecipientKey + "\" --output \"" + OutputPath + "\" --encrypt \"" + inputFilePath + "\" ");
            if (isSuccess) { }
                //File.Delete(inputFilePath);

            return OutputPath;
        }

        /// <summary>
        /// GNUPG Decrypt Method From Program
        /// ====================
        /// This method takes the path of file to be encypted
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <returns></returns>
        public string Decrypt(string inputFilePath, string filename)
        {
            string OutputPath = $"{Path.GetDirectoryName(inputFilePath)}\\{filename}";
            var isSuccess = ExecuteCommandSync("CMD", "/c gpg --output \"" + OutputPath + "\" --decrypt \"" + inputFilePath + "\"");
            //if (isSuccess)
            //    File.Delete(inputFilePath);

            return OutputPath;
        }

        /// <summary>
        /// GNUPG Encypt Method From Batch File
        /// =========================
        /// This method takes the path of file to be encypted
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <returns></returns>
        public string EncryptBatch(string inputFilePath, string filename)
        {
            string OutputPath = $"{Path.GetDirectoryName(inputFilePath)}\\{filename}";
            var isSuccess = ExecuteCommandSync("CMD", "/c gpg --yes --batch --armor --recipient \"" + intendedRecipientKey + "\" --output \"" + OutputPath + "\" --encrypt \"" + inputFilePath + "\" ");
            //var isSuccess = ExecuteCommandSync(encryptBatFilePath, " \"" + intendedRecipientKey + "\" \"" + OutputPath + "\" \"" + inputFilePath + "\" ");
            if (isSuccess)
            {
                File.Delete(inputFilePath);
            }


            return OutputPath;
        }

        /// <summary>
        /// GNUPG Decrypt Without PassCode Method From Batch File
        /// ============================
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <returns></returns>
        public string DecryptyWithoutPassBatch(string inputFilePath, string filename)
        {
            string OutputPath = $"{Path.GetDirectoryName(inputFilePath)}\\{filename}";
            var password = string.Empty;
            var isSuccess = ExecuteCommandSync("CMD", "/c echo " + password + "|gpg --passphrase-fd 0 --output \"" + OutputPath + "\" --decrypt \"" + inputFilePath + "\"");
            //var isSuccess = ExecuteCommandSync(decryptWithoutPassFilePath, " \"" + OutputPath + "\" \"" + inputFilePath + "\" ");
            if (isSuccess) { }
                //File.Delete(inputFilePath);

            return OutputPath;
        }

        /// <summary>
        /// GNUPG Decrypt Method From Batch File
        /// =====================
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <param name="PassPhrase"></param>
        /// <returns></returns>
        public string DecryptBatch(string inputFilePath, string passPhrase, string filename)
        {
            string OutputPath = $"{Path.GetDirectoryName(inputFilePath)}\\{filename}";
            var isSuccess = ExecuteCommandSync(decryptBatFilePath, " \"" + OutputPath + "\" \"" + inputFilePath + "\"  \"" + passPhrase + "\" ");
            if (isSuccess)
                File.Delete(inputFilePath);

            return OutputPath;
        }


        /// <summary>
        /// create the ProcessStartInfo using "cmd" as the program to be run
        /// and "/c " as the parameters.
        /// Incidentally, /c tells cmd that we want it to execute the command that follows,
        /// and then exit.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool ExecuteCommandSync(string filePath, string command)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo(filePath, command);

                procStartInfo.WorkingDirectory = programPath;

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                LogHelper.PrintError(ex.Message, ex);
                return false;
            }
        }
    }
}
