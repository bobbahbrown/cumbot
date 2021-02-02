using CumBot.Jobs;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CumBot.Services
{
    public class TalkableChannelMonitorService
    {
        public HashSet<ulong> Channels { get; set; }
        private readonly DiscordSocketClient _client;
        private readonly IScheduler _scheduler;
        private readonly ILogger<TalkableChannelMonitorService> _logger;

        public TalkableChannelMonitorService(IScheduler scheduler, DiscordSocketClient client, ILogger<TalkableChannelMonitorService> logger)
        {
            Channels = new HashSet<ulong>();
            _client = client;
            _scheduler = scheduler;
            _logger = logger;
        }

        public async Task TryMonitorChannel(ulong channel)
        {
            if (!Channels.Add(channel))
            {
                return;
            }

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"cum-{channel}")
                .StartNow()
                .Build();
            var details = new JobDataMap
            {
                ["channel"] = channel
            };
            var job = JobBuilder.Create<CumJob>().SetJobData(details).Build();

            _logger.LogInformation($"Found new channel to cum in, now cumming in <#{channel}>");
            await _scheduler.ScheduleJob(job, trigger);
        }

        public void NotifyChannelFailed(ulong channel)
        {
            if (Channels.Remove(channel))
            {
                _logger.LogInformation($"Can no longer cum in <#{channel}>, stopping tracking");
            }
            
        }
    }
}
