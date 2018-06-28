using System;

namespace BSky.Lifetime.Interfaces
{
    /// <summary>
    /// Logging service interface
    /// </summary>
    public interface ILoggerService
    {
        ///leveles in order low to high priority ///
        /// Debug
        /// Info
        /// Warn
        /// Error
        /// Fatal

        void SetLogLevelFromConfig();

        void WriteToLogLevel(string message, LogLevelEnum level);

        void WriteToLogLevel(string message, LogLevelEnum level, Exception ex);
        /// <summary>
        /// Logs Debug message
        /// </summary>
        /// <param name="message"></param>
        void Debug(string message);

        /// <summary>
        /// Logs Debug message and exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        void Debug(string message, Exception ex);

        /// <summary>
        /// Logs Info message
        /// </summary>
        /// <param name="message"></param>
        void Info(string message);

        /// <summary>
        /// Logs Info message and exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        void Info(string message, Exception ex);

        /// <summary>
        /// Logs Warn messages
        /// </summary>
        /// <param name="message"></param>
        void Warn(string message);

        /// <summary>
        /// Logs Warn messages and exception
        /// </summary>
        /// <param name="message"></param>
        void Warn(string message, Exception ex);

        /// <summary>
        /// Logs Error message
        /// </summary>
        /// <param name="message"></param>
        void Error(string message);

        /// <summary>
        /// Logs Error message and exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        void Error(string message, Exception ex);

        /// <summary>
        /// Logs Fatal messages
        /// </summary>
        /// <param name="message"></param>
        void Fatal(string message);

        /// <summary>
        /// Logs Fatal messages and exception
        /// </summary>
        /// <param name="message"></param>
        void Fatal(string message, Exception ex);
    }
}
