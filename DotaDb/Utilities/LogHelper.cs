using NLog;
using System;
using System.Diagnostics;
using System.Text;

namespace DotaDb.Utilities
{
    public class LogHelper
    {
        private Logger logger;

        public LogHelper(string typeName)
        {
            logger = LogManager.GetLogger(typeName);
        }

        public void Error(string methodName, string message, object parameters = null)
        {
            Debug.Assert(!String.IsNullOrEmpty(methodName));
            Debug.Assert(!String.IsNullOrEmpty(message));

            StringBuilder logMessage = new StringBuilder();

            logMessage.AppendFormat("Action: {0}", methodName);
            logMessage.AppendFormat(", Message: {0}", message);

            if (parameters != null)
            {
                logMessage.Append(", Params: [");
                Type anonType = parameters.GetType();
                foreach (var property in anonType.GetProperties())
                {
                    logMessage.AppendFormat("{0}: {1}", property.Name, property.GetValue(parameters, null));
                }
                logMessage.Append("]");
            }

            logger.Error(logMessage.ToString());
        }

        public void Error(string message, string stackTrace)
        {
            Debug.Assert(!String.IsNullOrEmpty(message));
            Debug.Assert(!String.IsNullOrEmpty(stackTrace));

            StringBuilder logMessage = new StringBuilder();

            logMessage.AppendFormat("Message: {0}", message);
            logMessage.AppendFormat(", StackTrace: {0}", stackTrace);

            logger.Error(logMessage.ToString());
        }
        
        public void Error(string methodName, Exception e, object parameters = null)
        {
            Debug.Assert(!String.IsNullOrEmpty(methodName));
            Debug.Assert(e != null);

            StringBuilder logMessage = new StringBuilder();

            logMessage.AppendFormat("Action: {0}", methodName);
            logMessage.AppendFormat(", Message: {0}", e.Message);

            // recursively check inner exceptions
            LogInnerException(e, logMessage);

            if (parameters != null)
            {
                logMessage.Append(", Params: [");
                Type anonType = parameters.GetType();
                foreach (var property in anonType.GetProperties())
                {
                    logMessage.AppendFormat("{0}: {1}, ", property.Name, property.GetValue(parameters, null));
                }
                logMessage.Append("]");
            }

            logMessage.AppendFormat(", StackTrace: {0}", e.StackTrace);

            logger.Error(logMessage.ToString());
        }

        public void Info(string message)
        {
            Debug.Assert(!String.IsNullOrEmpty(message));

            logger.Info(message);
        }

        public void Warn(string actionName, string message, object parameters = null)
        {
            Debug.Assert(!String.IsNullOrEmpty(actionName));
            Debug.Assert(!String.IsNullOrEmpty(message));

            StringBuilder logMessage = new StringBuilder();

            logMessage.AppendFormat("Action: {0}", actionName);
            logMessage.AppendFormat(", Message: {0}", message);

            if (parameters != null)
            {
                logMessage.Append(", Params: [");
                Type anonType = parameters.GetType();
                foreach (var property in anonType.GetProperties())
                {
                    logMessage.AppendFormat("{0}: {1}", property.Name, property.GetValue(parameters, null));
                }
                logMessage.Append("]");
            }

            logger.Warn(logMessage.ToString());
        }

        /// <summary>
        /// Recursively appends inner exception details to the SringBuilder
        /// </summary>
        /// <param name="e"></param>
        /// <param name="logMessage"></param>
        private void LogInnerException(Exception e, StringBuilder logMessage)
        {
            if (e.InnerException != null)
            {
                logMessage.AppendFormat(", InnerMessage: {0}", e.InnerException.Message);

                LogInnerException(e.InnerException, logMessage);
            }
        }

    }
}