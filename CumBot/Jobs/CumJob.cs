using CumBot.Services;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CumBot.Jobs
{
    public class CumJob : IJob
    {
        static readonly TimeSpan SOMETIMES_LONG_CUM_TIME = new TimeSpan(2, 30, 0);
        static readonly TimeSpan MAXIMUM_CUM_TIME = new TimeSpan(1, 30, 0);
        static readonly TimeSpan MINIMUM_CUM_TIME = new TimeSpan(0, 30, 0);
        private readonly IScheduler _scheduler;
        private readonly DiscordSocketClient _client;
        private readonly ILogger<CumJob> _logger;
        private readonly TalkableChannelMonitorService _monitorService;

        public CumJob(IScheduler scheduler, DiscordSocketClient client, ILogger<CumJob> logger, TalkableChannelMonitorService monitorService)
        {
            _scheduler = scheduler;
            _client = client;
            _logger = logger;
            _monitorService = monitorService;
        }

        private static DateTime GetNextCum()
        {
            var r = new Random();
            var chosenCumLimit = r.NextDouble() > 0.95 ? SOMETIMES_LONG_CUM_TIME : MAXIMUM_CUM_TIME;
            return DateTime.Now + MINIMUM_CUM_TIME + TimeSpan.FromTicks((long)((chosenCumLimit - MINIMUM_CUM_TIME).Ticks * r.NextDouble()));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // get job data
            if (!ulong.TryParse(context.MergedJobDataMap["channel"].ToString(), out var channelId))
            {
                throw new Exception("Could not parse channel ID for cum target");
            }

            // do cum action
            try
            {
                var channel = (ISocketMessageChannel)_client.GetChannel(channelId);
                await channel.SendMessageAsync("cum");
            }
            catch (Exception)
            {
                _logger.LogInformation($"Unable to cum in channel <#{channelId}>, removing cum target.");
                _monitorService.NotifyChannelFailed(channelId);
            }

            // schedule next cum
            var newTrigger = TriggerBuilder.Create().StartAt(GetNextCum()).Build();
            var oldTrigger = context.Trigger;
            await _scheduler.RescheduleJob(oldTrigger.Key, newTrigger);
        }
    }
}
