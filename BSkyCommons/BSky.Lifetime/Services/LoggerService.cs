using System;
using log4net;
using System.IO;
using BSky.Lifetime.Interfaces;
using System.Diagnostics;
using BSky.ConfService.Intf.Interfaces;

namespace BSky.Lifetime.Services
{
    public class LoggerService : ILoggerService
    {
        #region Private Fields

        private ILog logger;
        private ConfigLogLevelEnum loglevel;

        #endregion

        public LoggerService()
        {
            //LogManager.ResetConfiguration();
            FileInfo loginfo = new FileInfo("Log4net.config");/// For Application log
            log4net.Config.XmlConfigurator.Configure(loginfo);
            logger = LogManager.GetLogger("root");//<root> from log4net.config
            // setting log level once app is launched. faster. /// need a restart if you want to change level
            // If you want to set level without restart. Move following code under each method. It will be slower.
            //IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012
            //loglevel = GetEnumLogLevelFromString(confService.GetConfigValueForKey("loglevel")); 
            loglevel = ConfigLogLevelEnum.Error;//set default level. After config loads set user's level from it.
        }


        public LoggerService(string upath)
        {

            string applogname = string.Format(@"{0}ApplicationLog.txt", upath);
            //LogManager.ResetConfiguration();
            FileInfo loginfo = new FileInfo("Log4net.config");/// For Application log
            log4net.GlobalContext.Properties["LogName"] = applogname;
            log4net.Config.XmlConfigurator.Configure(loginfo);
            logger = LogManager.GetLogger("root");//<root> from log4net.config
            //setting log level once app is launched. faster. /// need a restart if you want to change level
            //If you want to set level without restart. Move following code under each method. It will be slower.
            //IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012
            //loglevel = GetEnumLogLevelFromString(confService.GetConfigValueForKey("loglevel")); 
            loglevel = ConfigLogLevelEnum.Error;//set default level. After config loads set user's level from it.
        }

        #region ILoggerService Members

        public void SetLogLevelFromConfig()
        {
            IConfigService confService = LifetimeService.Instance.Container.Resolve<IConfigService>();//23nov2012
            loglevel = GetEnumLogLevelFromString(confService.GetConfigValueForKey("loglevel")); 
        }

        public void WriteToLogLevel(string message, LogLevelEnum level)
        {
            int depth = 1;
            StackFrame stackframe = new StackFrame(depth);

            //24Nov2015 While working with SQL found that stackframe is null. Reason unknown. But this caused app crash
            //so here is some code that may stop that crash even if the stackframe is null
            string classname = " -CLASS :Unknonwn-Class";
            string methodname = " -METHOD : Unknown-Method";
            if (stackframe.GetMethod() != null)
            {
                classname = " -CLASS :" + stackframe.GetMethod().DeclaringType.FullName;
                methodname= " -METHOD :" + stackframe.GetMethod().Name + "()";
            }

            

            //StackTrace stacktrace = new StackTrace(depth);
            //string _line=string.Empty;
            //string _te = stacktrace.ToString();
            //int _lis = _te.IndexOf("line");
            //if(_lis>0)
            //_line = _te.Substring(_lis, 7);//stackframe.GetFileLineNumber()

            string msgcname = message + classname + methodname;
                 
                // +
             //" -LINE :" + _line;
            if (loglevel.ToString()==(level.ToString()) || loglevel.ToString().Equals("All"))
            {
                switch (level) // writes 'level' category of log
                {
                    case LogLevelEnum.Debug:
                        Debug(msgcname); break;
                    case LogLevelEnum.Info:
                        Info(msgcname); break;
                    case LogLevelEnum.Warn:
                        Warn(msgcname); break;
                    case LogLevelEnum.Error:
                        Error(msgcname); break;
                    case LogLevelEnum.Fatal:
                        Fatal(msgcname); break;
                    default:
                        Error(msgcname); break;
                }
            }
            //else //if invalid log level in config, still Error level logging will be done
            //    Error("Invalid Log level set in Config file. Valid options(case sensitive) are Debug, Info, Warn, Error, Fatal, All\n"+msgcname);
        }

        public void WriteToLogLevel(string message, LogLevelEnum level, Exception ex)
        {


            int depth = 1;
            StackFrame stackframe = new StackFrame(depth+1);

            string _line="0";
            string _te = ex.StackTrace.ToString();
            int _lis = _te.IndexOf("line");
            if(_lis >= 0 )
                _line = _te.Substring(_lis, 7);//stackframe.GetFileLineNumber()

            string msgcname = message +
                " -CLASS :" + stackframe.GetMethod().DeclaringType.FullName +
                " -METHOD :" + stackframe.GetMethod().Name+"()" +
             " -LINE :" + _line;
            if (loglevel.ToString() == (level.ToString()) || loglevel.ToString().Equals("All"))
            {
                switch (level)// writes 'level' category of log
                {
                    case LogLevelEnum.Debug:
                        Debug(msgcname, ex); break;
                    case LogLevelEnum.Info:
                        Info(msgcname, ex); break;
                    case LogLevelEnum.Warn:
                        Warn(msgcname, ex); break;
                    case LogLevelEnum.Error:
                        Error(msgcname, ex); break;
                    case LogLevelEnum.Fatal:
                        Fatal(msgcname, ex); break;
                    default:
                        Error(msgcname, ex); break;
                }
            }
            else if (level.ToString().Trim().Equals("Error") || level.ToString().Trim().Equals("Fatal"))//if invalid log level in config, still Error level logging will be done if level is "error"/"fatal"
            {
                
                Error(" Off or Invalid Log level set in Config file. Valid options(case sensitive) are Debug, Info, Warn, Error, Fatal, All, Off.\n " + msgcname + "\n", ex);
            }
        }
        /// <summary>
        /// Logs Debug message
        /// </summary>
        /// <param name="message"></param>
        public void Debug(string message)
        {
            //logger = LogManager.GetLogger("root");//<root> from log4net.config
            logger.Debug(message);
        }

        /// <summary>
        /// Logs Debug message and exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Debug(string message, Exception ex)
        {
            //logger = LogManager.GetLogger("root");//<root> from log4net.config
            logger.Debug(message, ex);
        }

        /// <summary>
        /// Logs Info message
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            logger.Info(message);
        }

        /// <summary>
        /// Logs Info message and exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Info(string message, Exception ex)
        {
            logger.Info(message, ex);
        }

        /// <summary>
        /// Logs Warn message
        /// </summary>
        /// <param name="message"></param>
        public void Warn(string message)
        {
            //logger = LogManager.GetLogger("root");//<root> from log4net.config
            logger.Warn(message);
        }

        /// <summary>
        /// Logs Warn message and exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Warn(string message, Exception ex)
        {
            //logger = LogManager.GetLogger("root");//<root> from log4net.config
            logger.Warn(message, ex);
        }

        /// <summary>
        /// Logs Error message
        /// </summary>
        /// <param name="message"></param>
        public void Error(string message)
        {
            logger.Error(message);
        }

        /// <summary>
        /// Logs Error message and exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Error(string message, Exception ex)
        {
            logger.Error(message, ex);
        }

        /// <summary>
        /// Logs Fatal message
        /// </summary>
        /// <param name="message"></param>
        public void Fatal(string message)
        {
            logger.Fatal(message);
        }

        /// <summary>
        /// Logs Fatal message and exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Fatal(string message, Exception ex)
        {
            logger.Fatal(message, ex);
        }
        #endregion

        private ConfigLogLevelEnum GetEnumLogLevelFromString(string strlvl)
        {
            switch (strlvl)
            {
                case "Debug": return ConfigLogLevelEnum.Debug;
                case "Info": return ConfigLogLevelEnum.Info;
                case "Warn": return ConfigLogLevelEnum.Warn;
                case "Error": return ConfigLogLevelEnum.Error;
                case "Fatal": return ConfigLogLevelEnum.Fatal;
                case "All": return ConfigLogLevelEnum.All;
                default: return ConfigLogLevelEnum.Off;
            }
        }
    }

}
