﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using gardener.Filtering;
using gardener.Utilities;

namespace gardener
{
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private IServiceProvider _provider;

        public CommandHandlingService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;

            _discord.MessageReceived += MessageReceived;
        }

        public Task InitializeAsync(IServiceProvider provider)
        {
            _provider = provider;
            return _commands.AddModulesAsync(Assembly.GetEntryAssembly(), provider);
            // Add additional initialization code here...
        }

        private Task MessageReceived(SocketMessage rawMessage)
        {
            Task.Run(async () =>
            {
                // Ignore system messages and messages from bots
                if (!(rawMessage is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;

                if (!Config.Ready) return;

                if (Garden.TreeState.UsersConnecting.Contains(rawMessage.Author.Id))
                {
                    Garden.Tree.OnUserMessageAsync(rawMessage, rawMessage.Channel is SocketDMChannel).Forget();
                    return;
                }

                if (rawMessage.Channel.Id == 725059963566817372)
                {
                    await Garden.LetterMatchGame.OnText(rawMessage);
                    return;
                }

                int argPos = 0;
                if (await ChatFilter.OnChatAsync(rawMessage))
                {
                    if (message.HasStringPrefix(Config.Prefix, ref argPos))
                    {
                        var context = new SocketCommandContext(_discord, message);

                        var result = await _commands.ExecuteAsync(context, argPos, _provider).ConfigureAwait(false);

                        if (result.Error.HasValue &&
                            result.Error.Value != CommandError.UnknownCommand)
                            await context.Channel.SendMessageAsync(result.ToString()).ConfigureAwait(false);
                    }
                }
            });
            return Task.CompletedTask;
        }
    }
}
