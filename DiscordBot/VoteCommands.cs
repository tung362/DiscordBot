using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows.Forms;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;
using DiscordBot.Modules;

namespace DiscordBot
{
    public class VoteCommands
    {
        private DiscordSocketClient _client;
        private CommandService _service;

        #region Commands
        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();

            await _service.AddModulesAsync(Assembly.GetEntryAssembly());

            _client.MessageReceived += HandleCommandAsync;

            //Loops
            await VoteLoopAsync();
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;

            var context = new SocketCommandContext(_client, msg);

            int argPos = 0;
            if (msg.HasCharPrefix('!', ref argPos)) //HasCharPrefix
            {
                var result = await _service.ExecuteAsync(context, argPos);

                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {

                }
            }

            //Commands
            if (context.Message.Author.Id != context.Guild.CurrentUser.Id && context.Channel.Id == Program.VoteChannel.Id)
            {
                if (context.Message.Content == "Yes" || context.Message.Content == "yes" || context.Message.Content == "Agree" || context.Message.Content == "agree") await Yes(context);
                else if (context.Message.Content == "No" || context.Message.Content == "no" || context.Message.Content == "Disagree" || context.Message.Content == "disagree") await No(context);
                else if (context.Message.Content == "Clear" || context.Message.Content == "clear") await Clear(context);
                else if (context.Message.Content == "Finish" || context.Message.Content == "finish") await Finish(context);
                await context.Message.DeleteAsync();
                await VoteLoopAsync();
            }
        }
        #endregion

        #region Commands
        public async Task Yes(SocketCommandContext context)
        {
            //Load
            List<VoteEntrieData> entries = LoadVotes();

            //Edit
            bool foundMatch = false;
            for (int i = 0; i < entries.Count; i++)
            {
                //Edit if entry already exist
                if (Convert.ToUInt64(entries[i].ID) == context.Message.Author.Id)
                {
                    foundMatch = true;
                    entries[i].TrueFalse = true;
                    break;
                }
            }
            //Create new entry is there was no matches
            if (!foundMatch)
            {
                VoteEntrieData newEntry = new VoteEntrieData(context.Message.Author.Id.ToString(), true);
                entries.Add(newEntry);
            }

            //Save
            SaveVotes(entries);
        }

        public async Task No(SocketCommandContext context)
        {
            //Load
            List<VoteEntrieData> entries = LoadVotes();

            //Edit
            bool foundMatch = false;
            for (int i = 0; i < entries.Count; i++)
            {
                //Edit if entry already exist
                if (Convert.ToUInt64(entries[i].ID) == context.Message.Author.Id)
                {
                    foundMatch = true;
                    entries[i].TrueFalse = false;
                    break;
                }
            }
            //Create new entry is there was no matches
            if (!foundMatch)
            {
                VoteEntrieData newEntry = new VoteEntrieData(context.Message.Author.Id.ToString(), false);
                entries.Add(newEntry);
            }

            //Save
            SaveVotes(entries);
        }

        public async Task Clear(SocketCommandContext context)
        {
            //Save
            List<VoteEntrieData> entries = new List<VoteEntrieData>();
            SaveVotes(entries);
        }

        public async Task Finish(SocketCommandContext context)
        {
            Program.AnnouncementChannel = GetAnnouncementChannel();
            if (Program.AnnouncementChannel == null) return;
            //Load
            List<VoteEntrieData> entries = LoadVotes();
            List<SocketGuildUser> unvotedusers = Program.VoteGuild.Users.ToList<SocketGuildUser>();
            List<SocketGuildUser> YesUsers = new List<SocketGuildUser>();
            List<SocketGuildUser> NoUsers = new List<SocketGuildUser>();

            //Sorted
            for (int i = 0; i < entries.Count; i++)
            {
                for (int j = 0; j < unvotedusers.Count; j++)
                {
                    //Attending
                    if (Convert.ToUInt64(entries[i].ID) == unvotedusers[j].Id && entries[i].TrueFalse)
                    {
                        YesUsers.Add(unvotedusers[j]);
                        unvotedusers.RemoveAt(j);
                        break;
                    }
                    //Not attending
                    else if (Convert.ToUInt64(entries[i].ID) == unvotedusers[j].Id && !entries[i].TrueFalse)
                    {
                        NoUsers.Add(unvotedusers[j]);
                        unvotedusers.RemoveAt(j);
                        break;
                    }
                }
            }

            if (YesUsers.Count > NoUsers.Count) await Program.AnnouncementChannel.SendMessageAsync("__**" + "Let It Be Known That The Server Has Agreed That " + LoadTopic() + "**__");
            else if (YesUsers.Count < NoUsers.Count) await Program.AnnouncementChannel.SendMessageAsync("__**" + "Let It Be Known That The Server Has Disagreed That " + LoadTopic() + "**__");
            else if (YesUsers.Count == NoUsers.Count && YesUsers.Count != 0 && NoUsers.Count != 0) await Program.AnnouncementChannel.SendMessageAsync("__**" + "Let It Be Known That The Server Could Not Decide If " + LoadTopic() + "**__");
            else if (YesUsers.Count == 0 && NoUsers.Count == 0) await Program.AnnouncementChannel.SendMessageAsync("__**" + "NO ONE VOTED >:V" + "**__");
            await Clear(context);
            SaveTopic("Need New Topic");
        }
        #endregion

        #region Vote Loop
        public async Task VoteLoopAsync()
        {
            Program.VoteGuild = GetVoteGuild();
            Program.VoteChannel = GetVoteChannel();
            if (Program.VoteGuild == null || Program.VoteChannel == null) await Reloop();

            //Clear chat
            await ClearMessage();

            //Load
            List<VoteEntrieData> entries = LoadVotes();
            List<SocketGuildUser> unvotedusers = Program.VoteGuild.Users.ToList<SocketGuildUser>();
            List<SocketGuildUser> YesUsers = new List<SocketGuildUser>();
            List<SocketGuildUser> NoUsers = new List<SocketGuildUser>();

            //Sorted
            for (int i = 0; i < entries.Count; i++)
            {
                for (int j = 0; j < unvotedusers.Count; j++)
                {
                    //Attending
                    if (Convert.ToUInt64(entries[i].ID) == unvotedusers[j].Id && entries[i].TrueFalse)
                    {
                        YesUsers.Add(unvotedusers[j]);
                        unvotedusers.RemoveAt(j);
                        break;
                    }
                    //Not attending
                    else if (Convert.ToUInt64(entries[i].ID) == unvotedusers[j].Id && !entries[i].TrueFalse)
                    {
                        NoUsers.Add(unvotedusers[j]);
                        unvotedusers.RemoveAt(j);
                        break;
                    }
                }
            }

            //Message generation
            string text =
                "__**" + LoadTopic() + "**__" + "\n" +
                "```css" + "\n" +
                "👍[Agrees]" + "\n";
            for (int i = 0; i < YesUsers.Count; i++)
            {
                if (!YesUsers[i].IsBot) text += "#" + YesUsers[i].Username + " 👉👈" + "\n";
            }
            text +=
                "```" + "```css" + "\n" +
                "👎[Disagrees]" + "\n";

            for (int i = 0; i < NoUsers.Count; i++)
            {
                if (!NoUsers[i].IsBot) text += "#" + NoUsers[i].Username + " 👉👌" + "\n";
            }
            text +=
                "```" + "```css" + "\n" +
                "❓[Have Not Decided]" + "\n";

            for (int i = 0; i < unvotedusers.Count; i++)
            {
                if (!unvotedusers[i].IsBot) text += "#" + unvotedusers[i].Username + " 🦍" + "\n";
            }
            text += "```";

            text +=
                "\n" +
                "***" + "\n" +
                "ℹ️ Information" + "\n" +
                "***" +
                "__**" + LoadTopic() + "**__" + "\n" +
                "Type \"__***yes***__\" if you agree." + "\n" +
                "Type \"__***no***__\" if you are disagree." + "\n" +
                "\n" +
                "\n" +
                "\n" +
                "**Updates whenever someone types something**" + "\n";
            await Program.VoteChannel.SendMessageAsync(text);

            //await Reloop();
        }

        private async Task Reloop()
        {
            await Task.Delay(1000);
            await VoteLoopAsync();
        }

        private SocketGuild GetVoteGuild()
        {
            if (Program.VoteGuild != null) return Program.VoteGuild;
            if (_client.Guilds.Count == 0) return null;

            SocketGuild retval = null;
            List<SocketGuild> guilds = _client.Guilds.ToList<SocketGuild>();
            for (int i = 0; i < guilds.Count; i++)
            {
                if (guilds[i].Name == Program.VoteGuildName)
                {
                    retval = guilds[i];
                    break;
                }
            }
            return retval;
        }

        private SocketTextChannel GetVoteChannel()
        {
            if (Program.VoteChannel != null) return Program.VoteChannel;
            if (Program.VoteGuild == null) return null;

            SocketTextChannel retval = null;
            List<SocketTextChannel> channels = Program.VoteGuild.TextChannels.ToList<SocketTextChannel>();
            for (int i = 0; i < channels.Count; i++)
            {
                if (channels[i].Name == Program.VoteChannelName)
                {
                    retval = channels[i];
                    break;
                }
            }
            return retval;
        }

        private SocketTextChannel GetAnnouncementChannel()
        {
            if (Program.AnnouncementChannel != null) return Program.AnnouncementChannel;
            if (Program.VoteGuild == null) return null;

            SocketTextChannel retval = null;
            List<SocketTextChannel> channels = Program.VoteGuild.TextChannels.ToList<SocketTextChannel>();
            for (int i = 0; i < channels.Count; i++)
            {
                if (channels[i].Name == Program.AnnouncementChannelName)
                {
                    retval = channels[i];
                    break;
                }
            }
            return retval;
        }

        public async Task ClearMessage()
        {
            //Gets messages
            var messages = await Program.VoteChannel.GetMessagesAsync(int.MaxValue).Flatten();
            var msgs = messages.ToList<IMessage>();

            //Make filter out any non bot messages
            for (int i = 0; i < msgs.Count; i++)
            {
                TimeSpan difference = DateTimeOffset.Now.Subtract(msgs[i].CreatedAt);
                if (difference.TotalMilliseconds >= (double)1203552000)
                {
                    msgs.RemoveAt(i);
                    i -= 1;
                }
            }

            await Program.VoteChannel.DeleteMessagesAsync(msgs);
        }

        private List<VoteEntrieData> LoadVotes()
        {
            //Entries
            List<VoteEntrieData> entries = new List<VoteEntrieData>();

            //Load
            if (File.Exists(Application.StartupPath + "/Vote/Vote"))
            {
                BinaryFormatter loadFormater = new BinaryFormatter();
                //Open file
                FileStream loadStream = new FileStream(Application.StartupPath + "/Vote/Vote", FileMode.Open);

                //Obtain the saved data
                VoteData theLoadedVoteData = loadFormater.Deserialize(loadStream) as VoteData;
                loadStream.Close();

                //Copy the entries
                for (int i = 0; i < theLoadedVoteData.Entries.Length; i++) entries.Add(theLoadedVoteData.Entries[i]);
            }
            return entries;
        }

        private void SaveVotes(List<VoteEntrieData> entries)
        {
            VoteData thePlaylistData = new VoteData(entries.ToArray());
            BinaryFormatter saveFormater = new BinaryFormatter();
            //Create file
            FileStream saveStream = new FileStream(Application.StartupPath + "/Vote/Vote", FileMode.Create);
            //Write to file
            saveFormater.Serialize(saveStream, thePlaylistData);
            saveStream.Close();
        }

        private string LoadTopic()
        {
            //Topic
            string topic = "";

            //Load
            if (File.Exists(Application.StartupPath + "/Vote/VoteTopic"))
            {
                BinaryFormatter loadFormater = new BinaryFormatter();
                //Open file
                FileStream loadStream = new FileStream(Application.StartupPath + "/Vote/VoteTopic", FileMode.Open);

                //Obtain the saved data
                string theLoadedTopic = loadFormater.Deserialize(loadStream) as string;
                loadStream.Close();

                //Copy the entries
                topic = theLoadedTopic;
            }
            return topic;
        }

        private void SaveTopic(string topic)
        {
            BinaryFormatter saveFormater = new BinaryFormatter();
            //Create file
            FileStream saveStream = new FileStream(Application.StartupPath + "/Vote/VoteTopic", FileMode.Create);
            //Write to file
            saveFormater.Serialize(saveStream, topic);
            saveStream.Close();
        }
        #endregion
    }
}
