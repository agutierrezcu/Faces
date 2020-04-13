using System;
using Microsoft.Extensions.Logging;

namespace SocialShared.Logging
{
    public class ScopedLogger : IDisposable
    {
        private const string Separator = "*********************************************************";

        private readonly ILogger _logger;

        public ScopedLogger(ILogger logger, string message)
        {
            _logger = logger;
            _logger.LogInformation(Separator);
            _logger.LogInformation($"***** {message} *****");
            _logger.LogInformation(Separator);
        }

        public void Dispose()
        {
            _logger.LogInformation(Separator);
        }
    }
}