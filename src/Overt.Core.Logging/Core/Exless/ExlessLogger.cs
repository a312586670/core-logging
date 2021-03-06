﻿#if ASP_NET_CORE
using Exceptionless;
using Microsoft.Extensions.Logging;
using System;

namespace Overt.Core.Logging
{
    /// <summary>
    /// Logger实现
    /// </summary>
    public class ExlessLogger : ILogger
    {
        private readonly string _categoryName;
        public ExlessLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return new NoopDisposable();
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                var message = formatter(state, exception);
                var source = $"{_categoryName}";
                var exlessLogLevel = Exceptionless.Logging.LogLevel.Trace;
                switch (logLevel)
                {
                    case LogLevel.Trace:
                        exlessLogLevel = Exceptionless.Logging.LogLevel.Trace;
                        break;
                    case LogLevel.Information:
                        exlessLogLevel = Exceptionless.Logging.LogLevel.Info;
                        break;
                    case LogLevel.Warning:
                        exlessLogLevel = Exceptionless.Logging.LogLevel.Warn;
                        break;
                    case LogLevel.Error:
                        exlessLogLevel = Exceptionless.Logging.LogLevel.Error;
                        break;
                    case LogLevel.Critical:
                        exlessLogLevel = Exceptionless.Logging.LogLevel.Fatal;
                        break;
                    default:
                        exlessLogLevel = Exceptionless.Logging.LogLevel.Debug;
                        break;
                }
                var eventBuilder = ExceptionlessClient.Default
                               .CreateLog(message, exlessLogLevel)
                               .SetSource(source)
                               .SetException(exception);

                if (eventId != null)
                    eventBuilder.SetProperty("Event", $"{eventId.ToString()}");

                var serverAndPoint = LoggingUtility.GetAddressIP();
                if (!string.IsNullOrEmpty(serverAndPoint))
                    eventBuilder.SetProperty("ServerEndPoint", serverAndPoint);

                eventBuilder.Submit();
            }
            catch { }
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
#endif
