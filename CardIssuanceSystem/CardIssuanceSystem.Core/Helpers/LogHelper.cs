using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.Helpers
{
    public static class LogHelper
    {
        #region DATA_MEMBERS
        private static ILog genericLogger;
        private static ILog infoFileLogger;
        #endregion

        #region CONSTANTS
        private const string NA = "NA";
        private const string GENERIC_LOGGER = "GenericLogging";
        private const string INFO_FILE_LOGGER = "INFOFileLogger";
        #endregion

        #region CONSTRUCTOR
        static LogHelper()
        {
            log4net.Config.XmlConfigurator.Configure();
            genericLogger = LogManager.GetLogger(GENERIC_LOGGER);
            infoFileLogger = LogManager.GetLogger(INFO_FILE_LOGGER);
        }
        #endregion

        #region METHODS
        public static void PrintGenericInfo(string info)
        {
            try
            {
                if (genericLogger.IsInfoEnabled)
                {
                    genericLogger.Info("@@Info:" + " " + info + "\n");
                }

            }
            catch (Exception exception)
            {
                if (genericLogger.IsErrorEnabled)
                {
                    genericLogger.Error("@@LogError:" + " " + exception.StackTrace.ToString() + "\n");
                }
            }

        }

        public static void PrintGenericInfo(string line, params object[] list)
        {
            try
            {
                string category = "@@Info: ";
                if (genericLogger.IsInfoEnabled)
                {
                    genericLogger.InfoFormat(category + "" + line, list);
                }

            }
            catch (Exception exception)
            {
                if (genericLogger.IsErrorEnabled)
                {
                    genericLogger.Error(" @@LogError:" + "" + exception.StackTrace.ToString() + "\n");
                }
            }

        }

        /// <summary>
        /// Logs some information
        /// </summary>
        /// <param name="info"></param>
        public static void LogInfo(string info)
        {
            try
            {
                if (infoFileLogger.IsInfoEnabled)
                {
                    infoFileLogger.Info(WrapDefaultLogData(info));
                }

            }
            catch (Exception exception)
            {
                if (genericLogger.IsErrorEnabled)
                {
                    genericLogger.Error(WrapDefaultLogData(exception.StackTrace.ToString() + "\n"));
                }
            }
        }
        private static string WrapDefaultLogData(string message)
        {
            try
            {
                return string.Format("[UserID: NA] {0}", message);
            }
            catch
            {
                return message;
            }
        }
        /// <summary>
        /// Logs information
        /// </summary>
        /// <param name="info">Information to log</param>
        /// <param name="args">arguments </param>
        public static void LogInfoFormat(string info, params object[] args)
        {
            LogInfo(string.Format(info, args));
        }

        public static void PrintDebug(string debug)
        {

            try
            {
                if (genericLogger.IsDebugEnabled)
                {
                    genericLogger.Debug(" @@Debug:" + "" + debug + "\n");
                }

            }
            catch (Exception exception)
            {
                if (genericLogger.IsErrorEnabled)
                {
                    genericLogger.Error(" @@LogError:" + "" + exception.StackTrace.ToString() + "\n");
                }
            }
        }

        /// <summary>
        /// Logs the exception
        /// </summary>
        /// <param name="error">Exception</param>
        public static void PrintError(Exception error)
        {
            PrintError(string.Empty, error);
        }

        /// <summary>
        /// Logs the error message
        /// </summary>
        /// <param name="customMessage">Custom Error Message</param>
        public static void PrintError(string customMessage)
        {
            PrintError(customMessage, null);
        }

        /// <summary>
        /// Logs the error in the file (along with a custom message)
        /// </summary>
        /// <param name="customMessage">Custom Message to add to the log</param>
        /// <param name="error">Exception</param>
        public static void PrintError(string customMessage, Exception error)
        {
            try
            {
                string errorMessage = NA;
                string stackTrace = NA;
                string innerErrorMessage = NA;
                string innerStackTrace = NA;

                if (string.IsNullOrEmpty(customMessage))
                    customMessage = NA;

                if (error != null)
                {
                    errorMessage = error.Message;
                    stackTrace = error.StackTrace;
                    if (error.InnerException != null)
                    {
                        innerErrorMessage = error.InnerException.Message;
                        innerStackTrace = error.InnerException.StackTrace;
                    }
                }

                if (genericLogger.IsErrorEnabled)
                {
                    if (error == null || error.InnerException == null)
                        genericLogger.Error(String.Format("{0:dd/MM/yyyy} \n@@Custom Message: {1}\n-----------------\n@@Error Message: {2}\n-----------------\n@@Error Trace: {3}",
                            System.DateTime.Now, customMessage, errorMessage, stackTrace));
                    else
                        genericLogger.Error(String.Format("{0:dd/MM/yyyy} \n@@Custom Message: {1}\n-----------------\n@@Error Message: {2}\n-----------------\n@@Error Trace: {3} \n **** INNER EXCEPTION ****************************** \n@@Inner Error Message: {4}\n-----------------\n@@Inner Error Trace: {5}",
                            System.DateTime.Now, customMessage, errorMessage, stackTrace, innerErrorMessage, innerStackTrace));
                }

            }
            catch (Exception exception)
            {
                if (genericLogger.IsErrorEnabled)
                {
                    genericLogger.Error(" @@LogError:" + " " + exception.StackTrace.ToString() + "\n");
                }
            }
        }

        public static void PrintLoggingDetails()
        {
            try
            {
                if (genericLogger.IsInfoEnabled)
                    genericLogger.Info("@@ IsInfoEnabled = TRUE.");
                if (genericLogger.IsWarnEnabled)
                    genericLogger.Warn("@@ IsWarnEnabled = TRUE.");
                if (genericLogger.IsFatalEnabled)
                    genericLogger.Fatal("@@ IsFatalEnabled = TRUE.");
                if (genericLogger.IsDebugEnabled)
                    genericLogger.Debug("@@ IsDebugEnabled = TRUE.");
                if (genericLogger.IsErrorEnabled)
                    genericLogger.Error("@@ IsErrorEnabled = TRUE.");

            }
            catch (Exception exception)
            {
                if (genericLogger.IsErrorEnabled)
                {
                    genericLogger.Error(" @@LogError:" + " " + exception.StackTrace.ToString() + "\n");
                }
            }

        }

        #endregion
    }
}
