using Discord;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CumBot.Services
{
    public class LogService
    {
        private ILogger _logger;

        public LogService(ILogger<LogService> logger)
        {
            _logger = logger;
        }

        public Task LogAsync(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Info:
                    _logger.LogInformation(msg.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(msg.Exception, msg.Message);
                    break;
                case LogSeverity.Error:
                    _logger.LogError(msg.Exception, msg.Message);
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(msg.Message);
                    break;
                case LogSeverity.Critical:
                    _logger.LogCritical(msg.Exception, msg.Message);
                    break;
                case LogSeverity.Verbose:
                    _logger.LogTrace(msg.Message);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}