using System;
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
    public class VoteCommand : ModuleBase<SocketCommandContext>
    {
        [Command("votetopic")]
        public async Task VoteTopic(string topic)
        {
            await Context.Message.DeleteAsync();
            SaveTopic(topic);
            List<VoteEntrieData> entries = new List<VoteEntrieData>();
            SaveVotes(entries);
            await Program.VoteCommand.VoteLoopAsync();
        }

        #region Vote tools
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
