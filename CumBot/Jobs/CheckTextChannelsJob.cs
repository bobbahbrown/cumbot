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
    public class CheckTextChannelsJob : IJob
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger<CheckTextChannelsJob> _logger;
        private readonly TalkableChannelMonitorService _monitorService;

        public CheckTextChannelsJob(DiscordSocketClient client, ILogger<CheckTextChannelsJob> logger, TalkableChannelMonitorService monitorService)
        {
            _client = client;
            _logger = logger;
            _monitorService = monitorService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var guilds = _client.Guilds.ToList();
            for (int guild = 0; guild < guilds.Count; guild++)
            {
                var thisGuild = guilds[guild];
                var botUser = thisGuild.GetUser(_client.CurrentUser.Id);
                var channels = thisGuild.Channels.ToList();
                for (int channel = 0; channel < channels.Count; channel++)
                {
                    var thisChannel = channels[channel];
                    if (thisChannel is SocketTextChannel textChannel)
                    {
                        var permissions = botUser.GetPermissions(textChannel);
                        if (permissions.SendMessages && permissions.ReadMessages)
                        {
                            await _monitorService.TryMonitorChannel(textChannel.Id);
                        }
                    }
                }
            }
        }
    }
}
