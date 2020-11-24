using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShivOhm.Infrastructure
{
    public class LogNLog : ILog
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();       

        public void Debug(string message)
        {
            logger.Debug(message);
        }

        public void Error(string message)
        {
            logger.Error(message);
        }

        public void Information(string message)
        {
            logger.Info(message);
        }

        public void Warning(string message)
        {
            logger.Warn(message);
        }
    }
}
