using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CumBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(IServiceProvider services, CommandService commands, DiscordSocketClient client)
        {
            _commands = commands;
            _services = services;
            _client = client;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _services);
            _client.MessageReceived += HandleCommandAsync;
        }

        public async Task HandleCommandAsync(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;
            if (!(message.HasStringPrefix(Debugger.IsAttached ? "~cum" : ".cum", ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);

            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services);
        }
    }
}
