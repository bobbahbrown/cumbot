using CumBot.Jobs;
using CumBot.Services;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace CumBot
{
    public class Initialize
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;

        public Initialize(CommandService commands = null, DiscordSocketClient client = null)
        {
            _commands = commands ?? new CommandService();
            _client = client ?? new DiscordSocketClient();
        }

        public IServiceProvider BuildServiceProvider(IScheduler scheduler)
        {
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(loggingBuilder =>
                loggingBuilder.AddSerilog(dispose: true));
            services.AddTransient<LogService>();

            // Add discord stuff
            services.AddSingleton(_client);
            services.AddSingleton(_commands);
            services.AddSingleton<CommandHandler>();
            services.AddSingleton<TalkableChannelMonitorService>();

            // Add scheduler
            services.AddSingleton(scheduler);

            // Add jobs
            services.AddTransient<CheckTextChannelsJob>();
            services.AddTransient<CumJob>();

            return services.BuildServiceProvider();
        }
    }
}
