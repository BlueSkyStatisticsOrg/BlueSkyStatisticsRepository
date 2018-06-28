using System;
using System.Diagnostics;

namespace BSky.Statistics.Common
{
    /// <summary>
    /// A logging class for logging data to the Windows Event Log and a log file.
    /// </summary>
    public class Log
    {
        #region Data members
        //=========================================================================================
        private const string NewLine = "\r\n";
        public const string TraceSourceName = "Unlimited Analytics";
        private static System.Diagnostics.TraceSource s_traceSource = new System.Diagnostics.TraceSource(TraceSourceName);
        #endregion

        #region Public Methods
        //=========================================================================================

        /// <summary>
        /// Logs debugging events.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="exception"></param>
        public static void DebugInfo(string description, Exception exception)
        {
            try
            {
                string exceptionString = "";
                if (exception != null)
                {
                    exceptionString = ExceptionToString(exception);
                }

                if (description != null)
                {
                    DebugInfo(description + NewLine + exceptionString);
                }
                else
                {
                    DebugInfo(exceptionString);
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Logs debugging events.
        /// </summary>
        /// <param name="description"></param>
        public static void DebugInfo(string description)
        {
            try
            {

                WriteEvent(System.Diagnostics.TraceEventType.Verbose, 0, GetCallerName() + "=>" + description);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Logs function calls
        /// </summary>
        /// <param name="function"></param>
        [System.Diagnostics.ConditionalAttribute("TRACE")]
        public static void TraceEnter()
        {
            try
            {
                WriteEvent(System.Diagnostics.TraceEventType.Verbose, 0, "> " + GetCallerName());
            }
            catch
            {
            }
        }

        /// <summary>
        /// Logs function calls
        /// </summary>
        /// <param name="function"></param>
        [System.Diagnostics.ConditionalAttribute("TRACE")]
        public static void TraceExit()
        {
            try
            {
                WriteEvent(System.Diagnostics.TraceEventType.Verbose, 0, "< " + GetCallerName());
            }
            catch
            {
            }
        }

        /// <summary>
        /// Logs function calls
        /// </summary>
        /// <param name="function"></param>
        [System.Diagnostics.ConditionalAttribute("TRACE")]
        public static void TraceExit(object retval)
        {
            try
            {
                string retvalString = (retval != null) ? retval.ToString() : "null";
                WriteEvent(System.Diagnostics.TraceEventType.Verbose, 0, "< " + GetCallerName() + " return (" + retvalString + ")");
            }
            catch
            {
            }
        }

        /// <summary>
        /// Logs informational events to the logfile.
        /// </summary>
        /// <param name="description"></param>
        public static void InformationEvent(string format, params object[] args)
        {
            try
            {
                WriteEvent(System.Diagnostics.TraceEventType.Information, 0, format, args);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Logs warning events. 
        /// </summary>
        /// <param name="description"></param>
        public static void WarningEvent(string description)
        {
            try
            {
                WarningEvent(GetCallerName() + "=>" + description, null);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Logs warning events. 
        /// </summary>
        /// <param name="description"></param>
        public static void WarningEvent(string description, Exception exception)
        {
            string message = "";

            try
            {
                message += description + NewLine;

                if (exception != null)
                {
                    message += ExceptionToString(exception);
                }

                WriteEvent(System.Diagnostics.TraceEventType.Warning, 0, message);
            }
            catch
            {
            }
        }
        public static void ErrorEvent(string description)
        {
            try
            {
                ErrorEvent(GetCallerName() + "=>" + description, null);
            }
            catch
            {
            }
        }
        /// <summary>
        /// Logs warning events. 
        /// </summary>
        /// <param name="description"></param>
        public static void ErrorEvent(string description, Exception exception)
        {
            string message = "";

            try
            {
                message += description + NewLine;

                if (exception != null)
                {
                    message += ExceptionToString(exception);
                }

                WriteEvent(System.Diagnostics.TraceEventType.Error, 0, message);
            }
            catch
            {
            }
        }

        #endregion

        #region Private Methods
        //=========================================================================================
        private static void WriteEvent(TraceEventType eventType, int id, string format, params object[] args)
        {
            Trace.WriteLine(eventType.ToString() + "->" + String.Format(format, args));
        }
        private static void WriteEvent(TraceEventType eventType, int id, string message)
        {
            Trace.WriteLine(eventType.ToString() + "->" + message);
        }
        public static System.Diagnostics.TraceSource TraceSource
        {
            get
            {
                return s_traceSource;
            }
        }

        private static string ExceptionToString(Exception exception)
        {
            string exceptionString = "";

            if (exception != null)
            {
                Exception e = exception;
                do
                {
                    exceptionString += e.Message;

                    // Add additional info for COMExceptions
                    if (e is System.Runtime.InteropServices.COMException)
                    {
                        System.Runtime.InteropServices.COMException cex = (System.Runtime.InteropServices.COMException)exception;
                        exceptionString += " (HRESULT : 0x" + cex.ErrorCode.ToString("X") + ")";
                    }

                    e = e.InnerException;
                    if (e != null)
                    {
                        exceptionString += " | ";
                    }
                } while (e != null);


                exceptionString += " | ";

                if (exception.StackTrace != null)
                {
                    exceptionString += "Trace: " + NewLine + exception.StackTrace.ToString() + NewLine;
                }
                else
                {
                    exceptionString += "Trace: Not Available" + NewLine;
                }
            }

            return exceptionString;
        }

        /// <summary>
        /// Returns function name in call stack
        /// </summary>
        /// <returns></returns>
        private static string GetCallerName()
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true); // true means get line numbers.
            System.Diagnostics.StackFrame[] frames = st.GetFrames();

            // [2] == <Caller>
            // [1] == TraceEnter()/TraceExit(), 
            // [0] == GetCallerName(), 
            System.Diagnostics.StackFrame f = frames[2];

            return f.GetMethod().DeclaringType.Name + "." + f.GetMethod().Name;
        }

        #endregion
    }
}

