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
using DiscordBot.References;

namespace DiscordBot
{
    //The binary converted version of votes
    [System.Serializable]
    public class VoteData
    {
        public VoteEntrieData[] Entries;

        public VoteData(VoteEntrieData[] newEntries)
        {
            Entries = new VoteEntrieData[newEntries.Length];
            for (int i = 0; i < Entries.Length; i++) Entries[i] = newEntries[i];
        }
    }

    //The binary converted version of vote entries
    [System.Serializable]
    public class VoteEntrieData
    {
        public string ID;
        public bool TrueFalse;

        public VoteEntrieData(string newID, bool newTrueFalse)
        {
            ID = newID;
            TrueFalse = newTrueFalse;
        }
    }

    public class ChurchCommands
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
            await ChurchLoopAsync();
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
            if (context.Message.Author.Id != context.Guild.CurrentUser.Id && context.Channel.Id == ChurchInfo.ChurchChannel.Id)
            {
                if (context.Message.Content == "Yes" || context.Message.Content == "yes") await Yes(context);
                else if (context.Message.Content == "No" || context.Message.Content == "no") await No(context);
                else if (context.Message.Content == "Clear" || context.Message.Content == "clear") await Clear(context);
                await context.Message.DeleteAsync();
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
            if(!foundMatch)
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
        #endregion

        #region Church Loop
        private async Task ChurchLoopAsync()
        {
            ChurchInfo.ChurchGuild = GetChurchGuild();
            ChurchInfo.ChurchChannel = GetChurchChannel();
            if (ChurchInfo.ChurchGuild == null || ChurchInfo.ChurchChannel == null) await Reloop();

            //Clear chat
            await ClearMessage();

            //Load
            List<VoteEntrieData> entries = LoadVotes();
            List<SocketGuildUser> unvotedusers = ChurchInfo.ChurchGuild.Users.ToList<SocketGuildUser>();
            List<SocketGuildUser> attendingUsers = new List<SocketGuildUser>();
            List<SocketGuildUser> notAttendingUsers = new List<SocketGuildUser>();

            //Sorted
            for (int i = 0; i < entries.Count; i++)
            {
                for(int j = 0; j < unvotedusers.Count; j++)
                {
                    //Attending
                    if (Convert.ToUInt64(entries[i].ID) == unvotedusers[j].Id && entries[i].TrueFalse)
                    {
                        attendingUsers.Add(unvotedusers[j]);
                        unvotedusers.RemoveAt(j);
                        break;
                    }
                    //Not attending
                    else if (Convert.ToUInt64(entries[i].ID) == unvotedusers[j].Id && !entries[i].TrueFalse)
                    {
                        notAttendingUsers.Add(unvotedusers[j]);
                        unvotedusers.RemoveAt(j);
                        break;
                    }
                }
            }

            //Message generation
            string text = 
                "```css" + "\n" +
                "🙏[Attending Church This Week]" + "\n";
            for (int i = 0; i < attendingUsers.Count; i++)
            {
                if(!attendingUsers[i].IsBot) text += "#" + attendingUsers[i].Username + " 👼🏽" + "\n";
            }
            text +=
                "```" + "```css" + "\n" +
                "👿[Not Attending Church This Week]" + "\n";

            for (int i = 0; i < notAttendingUsers.Count; i++)
            {
                if (!notAttendingUsers[i].IsBot) text += "#" + notAttendingUsers[i].Username + " 👹" + "\n";
            }
            text +=
                "```" + "```css" + "\n" +
                "❓[Have Not Voted]" + "\n";

            for (int i = 0; i < unvotedusers.Count; i++)
            {
                if (!unvotedusers[i].IsBot) text += "#" + unvotedusers[i].Username + " 🖕🏽" + "\n";
            }
            text += "```";

            text += 
                "\n" +
                "***" + "\n" + 
                "ℹ️ Information" + "\n" +
                "***" +
                "__**Are you attending church this week?**__" + "\n" +
                "Type \"__***yes***__\" if you are going." + "\n" +
                "Type \"__***no***__\" if you are not going." + "\n" +
                "\n" +
                "\n" +
                "\n" +
                "**Updates every 10 seconds**" + "\n";
            await ChurchInfo.ChurchChannel.SendMessageAsync(text);

            await Reloop();
        }

        private async Task Reloop()
        {
            await Task.Delay(10000);
            await ChurchLoopAsync();
        }

        private SocketGuild GetChurchGuild()
        {
            if (ChurchInfo.ChurchGuild != null) return ChurchInfo.ChurchGuild;
            if (_client.Guilds.Count == 0) return null;

            SocketGuild retval = null;
            List<SocketGuild> guilds = _client.Guilds.ToList<SocketGuild>();
            for (int i = 0; i < guilds.Count; i++)
            {
                if(guilds[i].Name == ChurchInfo.ChurchGuildName)
                {
                    retval = guilds[i];
                    break;
                }
            }
            return retval;
        }

        private SocketTextChannel GetChurchChannel()
        {
            if (ChurchInfo.ChurchChannel != null) return ChurchInfo.ChurchChannel;
            if (ChurchInfo.ChurchGuild == null) return null;

            SocketTextChannel retval = null;
            List<SocketTextChannel> channels = ChurchInfo.ChurchGuild.TextChannels.ToList<SocketTextChannel>();
            for (int i = 0; i < channels.Count; i++)
            {
                if (channels[i].Name == ChurchInfo.ChurchChannelName)
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
            var messages = await ChurchInfo.ChurchChannel.GetMessagesAsync(int.MaxValue).Flatten();
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

            await ChurchInfo.ChurchChannel.DeleteMessagesAsync(msgs);
        }

        private List<VoteEntrieData> LoadVotes()
        {
            //Playlist
            List<VoteEntrieData> entries = new List<VoteEntrieData>();

            //Load
            if (File.Exists(Application.StartupPath + "/ChurchVote/ChurchVote"))
            {
                BinaryFormatter loadFormater = new BinaryFormatter();
                //Open file
                FileStream loadStream = new FileStream(Application.StartupPath + "/ChurchVote/ChurchVote", FileMode.Open);

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
            FileStream saveStream = new FileStream(Application.StartupPath + "/ChurchVote/ChurchVote", FileMode.Create);
            //Write to file
            saveFormater.Serialize(saveStream, thePlaylistData);
            saveStream.Close();
        }

        #endregion
    }
}
