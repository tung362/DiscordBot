﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows.Forms;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;

namespace DiscordBot.Modules
{
    public class InfoCommand : ModuleBase<SocketCommandContext>
    {
        [Command("about")]
        public async Task About()
        {
            var eb = new EmbedBuilder();
            eb.Title = "🎉 Credits: ";
            string text =
                "Made By:" + "\n" +
                "```" + "\n" +
                "Tung Nguyen" + "\n" +
                "```" + "\n" +

                "Refactored By:" + "\n" +
                "```" + "\n" +
                "Riley Smith" + "\n" +
                "```";
            eb.WithDescription(text);
            eb.WithColor(Color.Purple);
            await Context.Channel.SendMessageAsync("", false, eb);
            await Context.Message.DeleteAsync();
        }

        [Command("status")]
        public async Task Status()
        {
            var eb = new EmbedBuilder();
            eb.Title = "👾 Status: ";
            string text =
                "```" +
                "Server owner's name is: " + Context.Guild.Owner.Username + "\n" +
                "Server owner's id is: " + Context.Guild.Owner.Id + "\n" +
                "Server owner is: " + Context.Guild.Owner.Status + "\n" +
                "Current channel: " + Context.Channel.Name + "\n" +
                "Current channel ID: " + Context.Channel.Id + "\n" +
                "Total Users: " + Context.Guild.Users.Count + "\n" +
                "Total number of channels: " + Context.Guild.Channels.Count + "\n" +
                "Number of text channels: " + Context.Guild.TextChannels.Count + "\n" +
                "Number of voice channels: " + Context.Guild.VoiceChannels.Count + "\n" +
                "```";
            eb.WithDescription(text);
            eb.WithColor(Color.Gold);
            await Context.Channel.SendMessageAsync("", false, eb);
            await Context.Message.DeleteAsync();
        }

        [Command("servers")]
        public async Task ServerList()
        {
            var eb = new EmbedBuilder();
            eb.Title = "🌊 List of Servers: ";
            string text = "```css\n";
            List<SocketGuild> guilds = Context.Client.Guilds.ToList<SocketGuild>();
            for (int i = 0; i < guilds.Count; i++)
            {
                text += i + ": " + guilds[i].Name + "\n";
                if (i == guilds.Count - 1) text += "```";
            }
            eb.WithDescription(text);
            eb.WithColor(Color.Blue);
            await Context.Channel.SendMessageAsync("", false, eb);
            await Context.Message.DeleteAsync();
        }

        [Command("roles")]
        public async Task RoleList(int guildID)
        {
            List<SocketGuild> guilds = Context.Client.Guilds.ToList<SocketGuild>();
            List<SocketRole> roles = guilds[guildID].Roles.ToList<SocketRole>();

            var eb = new EmbedBuilder();
            eb.Title = "🏄 Role list of " + guilds[guildID].Name + " Server";
            string text = "```css\n";
            for (int i = 0; i < roles.Count; i++)
            {
                text += i + ": " + roles[i].Name + "\n";
                if (i == roles.Count - 1) text += "```";
            }
            eb.WithDescription(text);
            eb.WithColor(Color.Orange);
            await Context.Channel.SendMessageAsync("", false, eb);
            await Context.Message.DeleteAsync();
        }

        [Command("permissions")]
        public async Task PermissionList(int guildID, int roleID)
        {
            List<SocketGuild> guilds = Context.Client.Guilds.ToList<SocketGuild>();
            List<SocketRole> roles = guilds[guildID].Roles.ToList<SocketRole>();

            var eb = new EmbedBuilder();
            eb.Title = "💦 List of permissions for " + roles[roleID].Name + " of " + guilds[guildID].Name + " Server";
            string text =
                "```css" + "\n" +
                "********Permissions*********" + "\n" +
                "AddReactions                      :   " + roles[roleID].Permissions.AddReactions + "\n" +
                "Administrator                     :   " + roles[roleID].Permissions.Administrator + "\n" +
                "AttachFiles                       :   " + roles[roleID].Permissions.AttachFiles + "\n" +
                "BanMembers                        :   " + roles[roleID].Permissions.BanMembers + "\n" +
                "ChangeNickname                    :   " + roles[roleID].Permissions.ChangeNickname + "\n" +
                "Connect                           :   " + roles[roleID].Permissions.Connect + "\n" +
                "CreateInstantInvite               :   " + roles[roleID].Permissions.CreateInstantInvite + "\n" +
                "DeafenMembers                     :   " + roles[roleID].Permissions.DeafenMembers + "\n" +
                "EmbedLinks                        :   " + roles[roleID].Permissions.EmbedLinks + "\n" +
                "KickMembers                       :   " + roles[roleID].Permissions.KickMembers + "\n" +
                "ManageChannels                    :   " + roles[roleID].Permissions.ManageChannels + "\n" +
                "ManageEmojis                      :   " + roles[roleID].Permissions.ManageEmojis + "\n" +
                "ManageGuild                       :   " + roles[roleID].Permissions.ManageGuild + "\n" +
                "ManageMessages                    :   " + roles[roleID].Permissions.ManageMessages + "\n" +
                "ManageNicknames                   :   " + roles[roleID].Permissions.ManageNicknames + "\n" +
                "ManageRoles                       :   " + roles[roleID].Permissions.ManageRoles + "\n" +
                "ManageWebhooks                    :   " + roles[roleID].Permissions.ManageWebhooks + "\n" +
                "MentionEveryone                   :   " + roles[roleID].Permissions.MentionEveryone + "\n" +
                "MoveMembers                       :   " + roles[roleID].Permissions.MoveMembers + "\n" +
                "MuteMembers                       :   " + roles[roleID].Permissions.MuteMembers + "\n" +
                "RawValue                          :   " + roles[roleID].Permissions.RawValue + "\n" +
                "ReadMessageHistory                :   " + roles[roleID].Permissions.ReadMessageHistory + "\n" +
                "ReadMessages                      :   " + roles[roleID].Permissions.ReadMessages + "\n" +
                "SendMessages                      :   " + roles[roleID].Permissions.SendMessages + "\n" +
                "SendTTSMessages                   :   " + roles[roleID].Permissions.SendTTSMessages + "\n" +
                "Speak                             :   " + roles[roleID].Permissions.Speak + "\n" +
                "UseExternalEmojis                 :   " + roles[roleID].Permissions.UseExternalEmojis + "\n" +
                "UseVAD                            :   " + roles[roleID].Permissions.UseVAD + "\n" +
                "ReadMessages                      :   " + roles[roleID].Permissions.ReadMessages + "\n" +
                "```";
            eb.WithDescription(text);
            eb.WithColor(Color.Red);
            await Context.Channel.SendMessageAsync("", false, eb);
            await Context.Message.DeleteAsync();
        }
    }
}
