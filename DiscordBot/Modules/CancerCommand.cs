using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace DiscordBot.Modules
{
    public class CancerCommand : ModuleBase<SocketCommandContext>
    {
        #region Insult commands
        [Command("insult")]
        public async Task Insult()
        {
            await Context.Message.DeleteAsync();

            //load
            List<string> entries = LoadInsults();
            List<SocketGuildUser> users = Context.Guild.Users.ToList<SocketGuildUser>();

            //Removes bot from targeting self
            for(int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == Context.Client.CurrentUser.Id || users[i].Status == UserStatus.Offline)
                {
                    users.RemoveAt(i);
                    i -= 1;
                }
            }

            //Random user
            int randomTargetedUser = Program.Rand.Next(0, users.Count);
            //Random insult entry
            int randomInsult = Program.Rand.Next(0, entries.Count);

            //Ensures insult list isnt empty and then send message
            if (entries.Count != 0) await Context.Channel.SendMessageAsync(users[randomTargetedUser].Mention + " " + entries[randomInsult], true);
        }

        [Command("insultlist")]
        public async Task InsultList()
        {
            await Context.Message.DeleteAsync();

            //Load
            List<string> entries = LoadInsults();

            if (entries.Count == 0) return;

            string text = "";
            for (int i = 0; i < entries.Count; i++)
            {
                if (i == 0) text += "```List of Insults \n";
                text += i + ": " + entries[i] + "\n";
                if (i == entries.Count - 1) text += "```";
            }
            await Context.Channel.SendMessageAsync(text);
        }

        [Command("addinsult")]
        public async Task AddInsult(string newInsult)
        {
            await Context.Message.DeleteAsync();

            //Load
            List<string> entries = LoadInsults();
            //Add
            entries.Add(newInsult);
            //Save
            SaveInsults(entries);
        }

        [Command("editinsult")]
        public async Task EditInsult(int index, string newInsult)
        {
            await Context.Message.DeleteAsync();

            //Load
            List<string> entries = LoadInsults();
            //Add
            if (entries.Count >= index) entries[index] = newInsult;
            //Save
            SaveInsults(entries);
        }

        [Command("deleteinsult")]
        public async Task DeleteInsult(int index)
        {
            await Context.Message.DeleteAsync();

            //Load
            List<string> entries = LoadInsults();
            //Add
            if(entries.Count >= index) entries.RemoveAt(index);
            //Save
            SaveInsults(entries);
        }
        #endregion

        #region Good video commands
        [Command("goodvideo")]
        public async Task GoodVideo()
        {
            await Context.Message.DeleteAsync();

            //load
            List<string> entries = LoadGoodVideos();

            //Random good video entry
            int randomGoodVideo = Program.Rand.Next(0, entries.Count);

            //Ensures good video list isnt empty and then send message
            if (entries.Count != 0) await Context.Channel.SendMessageAsync(entries[randomGoodVideo]);
        }

        [Command("goodvideolist")]
        public async Task GoodVideoList()
        {
            await Context.Message.DeleteAsync();

            //Load
            List<string> entries = LoadGoodVideos();

            if (entries.Count == 0) return;

            string text = "";
            for (int i = 0; i < entries.Count; i++)
            {
                if (i == 0) text += "```List of Good Videos \n";
                text += i + ": " + entries[i] + "\n";
                if (i == entries.Count - 1) text += "```";
            }
            await Context.Channel.SendMessageAsync(text);
        }

        [Command("addgoodvideo")]
        public async Task AddGoodVideo(string newGoodVideo)
        {
            await Context.Message.DeleteAsync();

            //Load
            List<string> entries = LoadGoodVideos();
            //Add
            entries.Add(newGoodVideo);
            //Save
            SaveGoodVideos(entries);
        }

        [Command("editgoodvideo")]
        public async Task EditGoodVideo(int index, string newGoodVideo)
        {
            await Context.Message.DeleteAsync();

            //Load
            List<string> entries = LoadGoodVideos();
            //Add
            if (entries.Count >= index) entries[index] = newGoodVideo;
            //Save
            SaveGoodVideos(entries);
        }

        [Command("deletegoodvideo")]
        public async Task DeleteGoodVideo(int index)
        {
            await Context.Message.DeleteAsync();

            //Load
            List<string> entries = LoadGoodVideos();
            //Add
            if (entries.Count >= index) entries.RemoveAt(index);
            //Save
            SaveGoodVideos(entries);
        }
        #endregion

        #region UserCommands
        [Command("say")]
        public async Task Say(string text)
        {
            await Context.Message.DeleteAsync();

            await Context.Channel.SendMessageAsync(text);
        }

        [Command("sayloop")]
        public async Task SayLoop()
        {
            if (Program.SayLoopDeleteMessage)
            {
                await Context.Message.DeleteAsync();
                Program.SayLoopDeleteMessage = false;
            }
            await ConsoleCommand();
        }

        public async Task ConsoleCommand()
        {
            string input = await Console.In.ReadLineAsync();

            if (input == "exit")
            {
                Program.SayLoopDeleteMessage = true;
                return;
            }

            RestUserMessage output = await Context.Channel.SendMessageAsync(input, false);
            //await output.DeleteAsync();
            await SayLoop();
        }
        #endregion

        #region Save Load Tools
        /*Tools*/
        List<string> LoadInsults()
        {
            List<string> entries = new List<string>();

            //Load
            if (File.Exists(Application.StartupPath + "/Insult/Insult"))
            {
                BinaryFormatter loadFormater = new BinaryFormatter();
                //Open file
                FileStream loadStream = new FileStream(Application.StartupPath + "/Insult/Insult", FileMode.Open);

                //Obtain the saved data
                StringData theLoadedInsultData = loadFormater.Deserialize(loadStream) as StringData;
                loadStream.Close();

                //Copy the entries
                for (int i = 0; i < theLoadedInsultData.UrlsOrPaths.Length; i++) entries.Add(theLoadedInsultData.UrlsOrPaths[i]);
            }
            return entries;
        }

        void SaveInsults(List<string> Entries)
        {
            StringData theInsultData = new StringData(Entries.ToArray());
            BinaryFormatter saveFormater = new BinaryFormatter();
            //Create file
            FileStream saveStream = new FileStream(Application.StartupPath + "/Insult/Insult", FileMode.Create);
            //Write to file
            saveFormater.Serialize(saveStream, theInsultData);
            saveStream.Close();
        }

        List<string> LoadGoodVideos()
        {
            List<string> entries = new List<string>();

            //Load
            if (File.Exists(Application.StartupPath + "/GoodVideo/GoodVideo"))
            {
                BinaryFormatter loadFormater = new BinaryFormatter();
                //Open file
                FileStream loadStream = new FileStream(Application.StartupPath + "/GoodVideo/GoodVideo", FileMode.Open);

                //Obtain the saved data
                StringData theLoadedGoodVideoData = loadFormater.Deserialize(loadStream) as StringData;
                loadStream.Close();

                //Copy the entries
                for (int i = 0; i < theLoadedGoodVideoData.UrlsOrPaths.Length; i++) entries.Add(theLoadedGoodVideoData.UrlsOrPaths[i]);
            }
            return entries;
        }

        void SaveGoodVideos(List<string> Entries)
        {
            StringData theGoodVideoData = new StringData(Entries.ToArray());
            BinaryFormatter saveFormater = new BinaryFormatter();
            //Create file
            FileStream saveStream = new FileStream(Application.StartupPath + "/GoodVideo/GoodVideo", FileMode.Create);
            //Write to file
            saveFormater.Serialize(saveStream, theGoodVideoData);
            saveStream.Close();
        }
        #endregion
    }
}
