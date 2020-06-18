﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using gardener.Tree;

namespace gardener.Modules
{
    public class FriendModule : ModuleBase<SocketCommandContext>
    {
        public Regex Expression = new Regex("<@!\\d+>");
        [Command("friend")]
        [RequireContext(ContextType.Guild)]
        public async Task Friend(string user)
        {
            if (Expression.IsMatch(user))
            {
                try
                {
                    ulong id = ulong.Parse(user.Substring(3, user.Length - 1));
                    var usr = await Garden.TheFriendTree.GetUserAsync(id);
                    var target = Garden.Tree.GetUser(usr.Id);
                    var cur = Garden.Tree.GetUser(Context.User.Id);
                    if (target != cur && target != null && !usr.IsBot)
                    {
                        target.Friends.Add(cur.TreeIndex);
                        cur.Friends.Add(target.TreeIndex);
                        await ReplyAsync($"Added {usr.Username}:#{usr.Discriminator} as a friend!");
                    }
                    else
                    {
                        await ReplyAsync($"The target user is either a bot or is not on this server!");
                    }
                }
                catch
                {
                    await ReplyAsync("An unknown error occurred!");
                }
            }
            else
            {
                await ReplyAsync("Please tag a user!");
            }
        }

        [Command("unfriend")]
        [RequireContext(ContextType.Guild)]
        public async Task UnFriend(string user)
        {
            if (Expression.IsMatch(user))
            {
                try
                {
                    ulong id = ulong.Parse(user.Substring(3, user.Length - 1));
                    var usr = await Garden.TheFriendTree.GetUserAsync(id);
                    var target = Garden.Tree.GetUser(usr.Id);
                    var cur = Garden.Tree.GetUser(Context.User.Id);
                    if (target != cur && target != null && !usr.IsBot &&
                        target.Friends.Contains(cur.TreeIndex) &&
                        cur.Friends.Contains(target.TreeIndex))
                    {
                        target.Friends.Remove(cur.TreeIndex);
                        cur.Friends.Remove(target.TreeIndex);
                        await ReplyAsync($"You are no longer the friend of {usr.Username}:#{usr.Discriminator}!");
                    }
                    else
                    {
                        await ReplyAsync($"The target user is either a bot or is not your friend!");
                    }
                }
                catch
                {
                    await ReplyAsync("An unknown error occurred!");
                }
            }
            else
            {
                await ReplyAsync("Please tag a user!");
            }
        }

        [Command("friends")]
        [RequireContext(ContextType.Guild)]
        public async Task Friends()
        {
            try
            {
                var cur = Garden.Tree.GetUser(Context.User.Id);
                if (cur != null)
                {
                    await ReplyAsync(embed: await GetEmbed(cur));
                }
                else
                {
                    await ReplyAsync($"The target user is either a bot or is not your friend!");
                }
            }
            catch
            {
                await ReplyAsync("An unknown error occurred!");
            }
        }

        public static async Task<Embed> GetEmbed(UserObject obj)
        {
            var footer = new EmbedFooterBuilder()
            {
                Text = "Contribute to the Server | https://github.com/encodeous/gardener/tree/master"
            };
            var st = (DateTime.Now - Program.StartTime);
            StringBuilder sb = new StringBuilder();

            sb.Append("**Friends:**\n");

            HashSet<int> actualFriends = new HashSet<int>();

            foreach (var user in obj.Friends)
            {
                actualFriends.Add(user);
            }

            actualFriends.Add(obj.InvitedBy);

            foreach (var user in obj.FriendsInvited)
            {
                actualFriends.Add(user);
            }

            foreach (var id in actualFriends)
            {
                var uid = Garden.Tree.TreeState.Users[id].UserId;
                var guildUser = await Garden.TheFriendTree.GetUserAsync(uid);
                sb.Append($"{guildUser.Username}:#{guildUser.Discriminator} [{id}]\n");
            }

            return new EmbedBuilder()
            {
                Color = Color.Green,
                Title = "**Your Friends**",
                Description = sb.ToString(),
                Footer = footer
            }.Build();
        }
    }
}
