using System;
using Microsoft.Extensions.Logging;

namespace SocialShared.Logging
{
    public class ScopedLogger : IDisposable
    {
        private readonly string _separator = $@"**********************************************************{Environment.NewLine}**********************************************************";

        private readonly ILogger _logger;

        public ScopedLogger(ILogger logger, string message)
        {
            _logger = logger;
            _logger.LogInformation(_separator);
            _logger.LogInformation($"***** {message} *****");
            _logger.LogInformation(_separator);
        }

        public void Dispose()
        {
            _logger.LogInformation(_separator);
        }
    }
}