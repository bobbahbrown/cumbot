using CumBot.Jobs;
using CumBot.Quartz;
using CumBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Serilog;
using Serilog.Filters;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace CumBot
{
    class Program
    {
        private static IConfiguration _configuration;
        private static IScheduler _scheduler;
        private static IServiceProvider _serviceProvider;
        private static DiscordSocketClient _client;
        private static ILogger<Program> _logger;

        static async Task Main(string[] args)
        {
            BuildConfiguration(args);

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Logger(lc =>
                {
                    lc.Filter.ByExcluding(Matching.FromSource("Quartz"));
                    lc.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}");
                })
                .CreateLogger();

            Log.Logger.ForContext<Program>()
                .Information($"Starting CumBot v{Assembly.GetExecutingAssembly().GetName().Version}");

            // Get a scheduler factory and scheduler
            StdSchedulerFactory factory = new StdSchedulerFactory();
            _scheduler = await factory.GetScheduler();

            // Get configuration
            var config = new BotConfig();
            _configuration.Bind("Discord", config);

            var init = new Initialize();
            _serviceProvider = init.BuildServiceProvider(_scheduler);
            _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();

            // Add logging from Discord.Net client
            var logger = _serviceProvider.GetRequiredService<LogService>();
            _client.Log += logger.LogAsync;
            _serviceProvider.GetRequiredService<CommandService>().Log += logger.LogAsync;

            // Add logging to Program
            _logger = _serviceProvider.GetRequiredService<ILogger<Program>>();

            // Build services provider and register it with the job factory
            _scheduler.JobFactory = new JobFactory(_serviceProvider);

            // Add cummer
            _client.MessageReceived += CumOnMessage;

            // Login
            await _client.LoginAsync(TokenType.Bot, config.Token);
            await _client.StartAsync();

            // Start command handler
            var ch = _serviceProvider.GetRequiredService<CommandHandler>();
            await ch.InitializeAsync();

            // Add job for monitoring cum-able channels
            var trigger = TriggerBuilder.Create()
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever())
                .Build();
            var job = JobBuilder.Create<CheckTextChannelsJob>().Build();
            await _scheduler.ScheduleJob(job, trigger);

            // Start scheduler
            await _scheduler.Start();

            await Task.Delay(-1);
        }

        static async Task CumOnMessage(SocketMessage message)
        {
            var r = new Random();
            if (r.NextDouble() > 0.98)
            {
                await message.AddReactionAsync(new Emoji("\uD83C\uDDE8"));
                await message.AddReactionAsync(new Emoji("\uD83C\uDDFA"));
                await message.AddReactionAsync(new Emoji("\uD83C\uDDF2"));
                _logger.LogInformation($"cummed on '{message.Content}' from {message.Author.Username}#{message.Author.Discriminator} in #{message.Channel.Name}");
            }
        }

        static void BuildConfiguration(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appconfig.json")
                .AddCommandLine(args)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
