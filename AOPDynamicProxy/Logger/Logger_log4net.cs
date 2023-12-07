using Castle.Core.Logging;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOPDynamicProxy
{
  

    public class Logger_log4net : ILogger
    {
        private readonly ILog m_logger;

        public ILog Log4netLogger { get { return m_logger; } }

        public Logger_log4net(ILog logger)
        {
            m_logger = logger;
        }

        public bool IsTraceEnabled => throw new NotImplementedException();

        public bool IsDebugEnabled => m_logger.IsDebugEnabled;

        public bool IsErrorEnabled => m_logger.IsErrorEnabled;

        public bool IsFatalEnabled => m_logger.IsFatalEnabled;

        public bool IsInfoEnabled => m_logger.IsInfoEnabled;

        public bool IsWarnEnabled => m_logger.IsWarnEnabled;


        public void Debug(string message)
        {
            m_logger.Debug(message);
        }

        public void Debug(Func<string> messageFactory)
        {
            var message = messageFactory.Invoke();
            m_logger.Debug(message);
        }

        public void Debug(string message, Exception exception)
        {
            m_logger.Debug(message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            m_logger.DebugFormat(format, args);
        }

        public void DebugFormat(Exception exception, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            m_logger.DebugFormat(formatProvider, format, args);
        }

        public void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Error(string message)
        {
            m_logger.Error(message);
        }

        public void Error(Func<string> messageFactory)
        {
            var message = messageFactory.Invoke();
            m_logger.Error(message);
        }

        public void Error(string message, Exception exception)
        {
            m_logger.Error(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            m_logger.ErrorFormat(format, args);
        }

        public void ErrorFormat(Exception exception, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            m_logger.ErrorFormat(formatProvider, format, args);
        }

        public void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Fatal(string message)
        {
            m_logger.Fatal(message);
        }

        public void Fatal(Func<string> messageFactory)
        {
            var message = messageFactory;
            m_logger.Fatal(message);
        }

        public void Fatal(string message, Exception exception)
        {
            m_logger.Fatal(message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            m_logger.FatalFormat(format, args);
        }

        public void FatalFormat(Exception exception, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            m_logger.FatalFormat(formatProvider, format, args);
        }

        public void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Info(string message)
        {
            m_logger.Info(message);
        }

        public void Info(Func<string> messageFactory)
        {
            var message = messageFactory.Invoke();
            m_logger.Info(message);
        }

        public void Info(string message, Exception exception)
        {
            m_logger.Info(message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            m_logger.InfoFormat(format, args);
        }

        public void InfoFormat(Exception exception, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            m_logger.InfoFormat(formatProvider, format, args);
        }

        public void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Trace(string message)
        {
            throw new NotImplementedException();
        }

        public void Trace(Func<string> messageFactory)
        {
            throw new NotImplementedException();
        }

        public void Trace(string message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void TraceFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void TraceFormat(Exception exception, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void TraceFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void TraceFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Warn(string message)
        {
            m_logger.Warn(message);
        }

        public void Warn(Func<string> messageFactory)
        {
            var message = messageFactory.Invoke();
            m_logger.Warn(message);
        }

        public void Warn(string message, Exception exception)
        {
            m_logger.Warn(message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            m_logger.WarnFormat(format, args);
        }

        public void WarnFormat(Exception exception, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            m_logger.WarnFormat(formatProvider, format, args);
        }

        public void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public ILogger CreateChildLogger(string loggerName)
        {
            throw new NotImplementedException();
        }
    }
}
