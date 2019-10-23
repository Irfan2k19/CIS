using CardIssuanceSystem.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace CardIssuanceSystem.Core.Helpers
{
    public class FileHelper
    {
        /// <summary>
        /// Use to create a file
        /// </summary>
        /// <param name="path"></param>
        public static string Create(string path, string fileName)
        {
            try
            {
                var filename = string.IsNullOrEmpty(fileName) ? Guid.NewGuid().ToString().Replace("-", string.Empty) : fileName;
                path = string.IsNullOrEmpty(path) ? Path.Combine(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "bin"), $"{filename}.txt") : Path.Combine(path, $"{filename}.txt");

                if (!File.Exists(path))
                    File.Create(path).Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return path;
        }
        /// <summary>
        /// Use to get a file
        /// </summary>
        /// <param name="path"></param>
        public static string[] Import(string path/*, string filename*/)
        {
            try
            {
                var fullpath = path;//Path.Combine(path, filename);
                var text = File.ReadAllText(fullpath).Replace("\r\n", "$$&$$");
                if (!string.IsNullOrEmpty(text))
                    return text.Split(new[] { "$$&$$" }, StringSplitOptions.None).Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()).ToArray();
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void WriteFile(string path, string[] content)
        {
            var filePath = path;
            if (!File.Exists(path))
                filePath = Create(Path.GetDirectoryName(path), Path.GetFileName(path));

            using (StreamWriter w = File.AppendText(filePath))
            {
                foreach (var item in content)
                {
                    w.WriteLine(item);
                }
                w.Flush();
                w.Close();
            }
        }

        public static List<ImportFileVM> GetFileNamesFromDirectory(string path)
        {
            if (!Directory.Exists(path))
                return new List<ImportFileVM>();

            FileInfo[] arr = new DirectoryInfo(path).GetFiles("ACCESS_CARD_EXPORT*.txt", SearchOption.TopDirectoryOnly);
            arr.ToList().ForEach(f => f.Refresh());
            return arr.ToList().Select(e => new ImportFileVM
            {
                DirectoryName = e.DirectoryName,
                FileFullName = e.FullName,
                FileName = e.Name,
                FileCreationTime = e.CreationTime,
                FileModifiedTime = e.LastWriteTime
            }).OrderByDescending(e=>e.FileModifiedTime).ToList();
        }

        public static List<ImportFileVM> GetFileNamesFromDirectoryV1(string path)
        {
            if (!Directory.Exists(path))
                return new List<ImportFileVM>();

            FileInfo[] arr = new DirectoryInfo(path).GetFiles("*.txt", SearchOption.TopDirectoryOnly);
            arr.ToList().ForEach(f => f.Refresh());
            return arr.ToList().Select(e => new ImportFileVM
            {
                DirectoryName = e.DirectoryName,
                FileFullName = e.FullName,
                FileName = e.Name,
                FileCreationTime = e.CreationTime,
                FileModifiedTime = e.LastWriteTime
            }).OrderByDescending(e => e.FileModifiedTime).ToList();
        }

        #region Recovery Log
        public static bool RecoveryErrorLog(string ActionName, string Data, string Response, string Line)
        {
            try
            {
                string Date = DateTime.Now.Date.ToString("d");
                Date = Date.Replace("/", "");

                //var filePath = @"C:\CIS Logs\CIS_Recovery_log_" + Date + ".txt";
                var filePath = ConfigurationManager.AppSettings["LogFilePath"] + "CIS_Recovery_log_" + Date + ".txt";

                if (!File.Exists(filePath))
                {

                    //File.Create(filePath);
                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        //writer.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                        //   "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                        writer.WriteLine(DateTime.Now + "-> " + "Line# " + Line + "| Event: " + ActionName + "| Data: " + Data + "|" + Response + Environment.NewLine);
                        //writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                        writer.Close();
                    }
                }
                else
                {
                    //using (StreamWriter writer = new StreamWriter(filePath, true))
                    //{
                    //    //writer.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                    //    //   "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    //    writer.WriteLine(DateTime.Now + "-> " + "Line# " + Line + "| Event: " + ActionName + "| Data: " + Data + "|" + Response + Environment.NewLine);
                    //    //writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                    //    writer.Close();
                    //}

                    File.AppendAllText(filePath, DateTime.Now + "-> " + "Line# " + Line + "| Event: " + ActionName + "| Data: " + Data + "|" + Response + Environment.NewLine);

                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }


        }

        public static bool RecoveryTransactionLog(bool IsResponse, string DebitAccountNo, string CreditAccountNo, string Amount
    , string Narration, string STAN, string CardNo, string FT, string ResponseCode, string ResponseCodeDescription)
        {
            try
            {
                string Type = "Transaction Response";
                string Text = "";
                if (!IsResponse)
                {
                    Type = "Transaction Attempt";
                }
                Text = "TransactionTime:" + DateTime.Now + "|TransactionType=" + Type + "|DebitAccountNo:" + DebitAccountNo
                     + "|CreditAccountNo:" + CreditAccountNo + "|Amount:" + Amount + "|Narration:" + Narration + "|STAN:" + STAN + "|CardNo:" + CardNo + "|FT:"
                     + FT + "|ResponseCode:" + ResponseCode + "|ResponseCodeDescription:" + ResponseCodeDescription + "|";

                string Date = DateTime.Now.Date.ToString("d");
                Date = Date.Replace("/", "");

                //var filePath = @"C:\CIS Logs\CIS_Recovery_Transaction_log_" + Date + ".txt";
                var filePath = ConfigurationManager.AppSettings["TransactionLogFilePath"] + "CIS_Recovery_Transaction_log_" + Date + ".txt";
                if (!File.Exists(filePath))
                {

                    //File.Create(filePath);
                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        writer.WriteLine(Text + Environment.NewLine);
                        writer.Close();
                    }
                }
                else
                {
                    File.AppendAllText(filePath, Text + Environment.NewLine);

                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }


        }

        public static bool RecoveryExceptionLog(string ActionName, string RecoveryID, string STAN, string Account, string CardNo, string exp)
        {
            try
            {
                string Date = DateTime.Now.Date.ToString("d");
                Date = Date.Replace("/", "");

                //var filePath = @"C:\CIS Logs\CIS_Recovery_log_" + Date + ".txt";
                var filePath = ConfigurationManager.AppSettings["LogFilePath"] + "Recovery_Exception_log_" + Date + ".txt";

                if (!File.Exists(filePath))
                {


                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {

                        writer.WriteLine(DateTime.Now + "-> " + "| Event: " + ActionName + "| CardNo: " + CardNo + " | Account: " + Account + " | STAN: " + STAN + " | RecoveryID: " + RecoveryID + " | " + "Exception: " + exp + Environment.NewLine);
                        writer.Close();
                    }
                }
                else
                {

                    File.AppendAllText(filePath, DateTime.Now + "-> | Event: " + ActionName + "| CardNo: " + CardNo + " | Account: " + Account + " | STAN: " + STAN + " | RecoveryID: " + RecoveryID + " | " + "Exception: " + exp + Environment.NewLine);

                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }


        }
        #endregion


        #region  Request Transaction Logs
        public static bool RequestTransactionLog(bool IsResponse, string DebitAccountNo, string CreditAccountNo, string Amount, string Narration, string STAN, string FT, string ResponseCode, string ResponseCodeDescription,string RequestType)
        {
            try
            {
                string Type = "Transaction Response";
                string Text = "";
                if (!IsResponse)
                {
                    Type = "Transaction Attempt";
                }
                Text = "TransactionTime:" + DateTime.Now + "|TransactionType=" + Type + "|DebitAccountNo:" + DebitAccountNo
                     + "|CreditAccountNo:" + CreditAccountNo + "|Amount:" + Amount + "|Narration:" + Narration + "|STAN:" + STAN + "|FT:"
                     + FT + "|ResponseCode:" + ResponseCode + "|ResponseCodeDescription:" + ResponseCodeDescription + "|RequestType: "+ RequestType+" |";

                string Date = DateTime.Now.Date.ToString("d");
                Date = Date.Replace("/", "");

                //var filePath = @"C:\CIS Logs\CIS_Recovery_Transaction_log_" + Date + ".txt";
                var filePath = ConfigurationManager.AppSettings["RequestLogFilePath"] + "CIS_Request_Transaction_log_" + Date + ".txt";
                if (!File.Exists(filePath))
                {

                    //File.Create(filePath);
                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        writer.WriteLine(Text + Environment.NewLine);
                        writer.Close();
                    }
                }
                else
                {
                    File.AppendAllText(filePath, Text + Environment.NewLine);

                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }


        }


        #endregion
    }
}
