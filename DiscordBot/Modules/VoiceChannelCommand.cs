using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace DiscordBot.Modules
{
    //The binary converted version of strings
    [System.Serializable]
    public class StringData
    {
        public string[] UrlsOrPaths;

        public StringData(string[] newUrlsOrPaths)
        {
            UrlsOrPaths = new string[newUrlsOrPaths.Length];
            for (int i = 0; i < UrlsOrPaths.Length; i++) UrlsOrPaths[i] = newUrlsOrPaths[i];
        }
    }

    public class VoiceChannelCommand : ModuleBase<SocketCommandContext>
    {
        #region Channel commands
        /*Channel commands*/
        [Command("channels")]
        public async Task Channels()
        {
            string text = "";
            List<SocketVoiceChannel> voiceChannels = Context.Guild.VoiceChannels.ToList<SocketVoiceChannel>();
            for (int i = 0; i < voiceChannels.Count; i++)
            {
                if (i == 0) text += "```Channel Indexes \n";
                text += i + ": " + voiceChannels[i].Name + "\n";
                if (i == voiceChannels.Count - 1) text += "```";
            }
            await Context.Channel.SendMessageAsync(text);
            await Context.Message.DeleteAsync();
        }

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinChannel(int index)
        {
            DeletePlaylist();
            List<SocketVoiceChannel> voiceChannels = Context.Guild.VoiceChannels.ToList<SocketVoiceChannel>();
            await voiceChannels[index].ConnectAsync();
            Program.PreviousVoiceChannelIndex = index;

            await Context.Message.DeleteAsync();
        }

        [Command("joinme", RunMode = RunMode.Async)]
        public async Task JoinMeChannel()
        {
            List<SocketVoiceChannel> voiceChannels = Context.Guild.VoiceChannels.ToList<SocketVoiceChannel>();
            for (int i = 0; i < voiceChannels.Count; i++)
            {
                List<SocketGuildUser> channelUsers = voiceChannels[i].Users.ToList<SocketGuildUser>();
                for(int j = 0; j < channelUsers.Count; j++)
                {
                    if(channelUsers[j].Username == Context.User.Username)
                    {
                        DeletePlaylist();
                        await voiceChannels[i].ConnectAsync();
                        Program.PreviousVoiceChannelIndex = i;
                        break;
                    }
                }
            }
            await Context.Message.DeleteAsync();
        }

        [Command("leave")]
        public async Task LeaveChannel()
        {
            await Context.Message.DeleteAsync();
            if (Program.PreviousAudioClient != null) await Program.PreviousAudioClient.StopAsync();
        }
        #endregion

        #region Voice channel commands
        /*Voice channel commands*/
        [Command("playlist")]
        public async Task PlaylistChannel()
        {
            await Context.Message.DeleteAsync();

            //Load
            List<string> entries = LoadPlaylist();

            if (entries.Count == 0) return;

            string text = "";
            for (int i = 0; i < entries.Count; i++)
            {
                if (i == 0) text += "```Playlist Queue \n";
                text += i + ": " + entries[i] + "\n";
                if (i == entries.Count - 1) text += "```";
            }
            await Context.Channel.SendMessageAsync(text);
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayChannel(string pathOrUrl)
        {
            await Context.Message.DeleteAsync();

            //Don't do anything if never joined a voice channel
            if (Program.PreviousVoiceChannelIndex == -1) return;

            //Load
            List<string> entries = LoadPlaylist();

            //Add new entry
            entries.Add(pathOrUrl);

            //Save
            SavePlaylist(entries);

            //Force play if theres only 1 song on queue
            if (entries.Count == 1)
            {
                List<SocketVoiceChannel> voiceChannels = Context.Guild.VoiceChannels.ToList<SocketVoiceChannel>();
                IAudioClient audioClient = await voiceChannels[Program.PreviousVoiceChannelIndex].ConnectAsync();
                Program.PreviousAudioClient = audioClient;
                await SendAsync(audioClient, entries[0]);
            }
        }

        [Command("skip", RunMode = RunMode.Async)]
        public async Task SkipChannel()
        {
            await Context.Message.DeleteAsync();

            //Don't do anything if never joined a voice channel
            if (Program.PreviousVoiceChannelIndex == -1 || Program.SkippingSong) return;

            //Load
            List<string> entries = LoadPlaylist();

            //Remove the previous played song from the queue
            if (entries.Count != 0) entries.RemoveAt(0);

            //Save
            SavePlaylist(entries);

            Program.SkippingSong = true;

            //Play if theres any songs left on queue
            if (entries.Count != 0)
            {
                List<SocketVoiceChannel> voiceChannels = Context.Guild.VoiceChannels.ToList<SocketVoiceChannel>();
                IAudioClient audioClient = await voiceChannels[Program.PreviousVoiceChannelIndex].ConnectAsync();
                Program.PreviousAudioClient = audioClient;
                await SendAsync(audioClient, entries[0]);
            }
        }

        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopChannel()
        {
            await Context.Message.DeleteAsync();

            //Don't do anything if never joined a voice channel
            if (Program.PreviousVoiceChannelIndex == -1) return;

            DeletePlaylist();
            List<SocketVoiceChannel> voiceChannels = Context.Guild.VoiceChannels.ToList<SocketVoiceChannel>();
            IAudioClient audioClient = await voiceChannels[Program.PreviousVoiceChannelIndex].ConnectAsync();
            Program.PreviousAudioClient = audioClient;
        }
        #endregion

        #region Voice channel tools
        /*Voice channel tools*/
        private async Task NextSong()
        {
            if (Program.PreviousVoiceChannelIndex == -1) return;

            //Load
            List<string> entries = LoadPlaylist();

            //Remove the previous played song from the queue
            if (entries.Count != 0) entries.RemoveAt(0);

            //Save
            SavePlaylist(entries);

            //Play if theres any songs left on queue
            if (entries.Count != 0)
            {
                List<SocketVoiceChannel> voiceChannels = Context.Guild.VoiceChannels.ToList<SocketVoiceChannel>();
                IAudioClient audioClient = await voiceChannels[Program.PreviousVoiceChannelIndex].ConnectAsync();
                Program.PreviousAudioClient = audioClient;
                await SendAsync(audioClient, entries[0]);
            }
        }

        List<string> LoadPlaylist()
        {
            //Playlist
            List<string> entries = new List<string>();

            //Load
            if (File.Exists(Application.StartupPath + "/Playlist/Playlist"))
            {
                BinaryFormatter loadFormater = new BinaryFormatter();
                //Open file
                FileStream loadStream = new FileStream(Application.StartupPath + "/Playlist/Playlist", FileMode.Open);

                //Obtain the saved data
                StringData theLoadedPlaylistData = loadFormater.Deserialize(loadStream) as StringData;
                loadStream.Close();

                //Copy the entries
                for (int i = 0; i < theLoadedPlaylistData.UrlsOrPaths.Length; i++) entries.Add(theLoadedPlaylistData.UrlsOrPaths[i]);
            }
            return entries;
        }

        void SavePlaylist(List<string> Entries)
        {
            StringData thePlaylistData = new StringData(Entries.ToArray());
            BinaryFormatter saveFormater = new BinaryFormatter();
            //Create file
            FileStream saveStream = new FileStream(Application.StartupPath + "/Playlist/Playlist", FileMode.Create);
            //Write to file
            saveFormater.Serialize(saveStream, thePlaylistData);
            saveStream.Close();
        }

        void DeletePlaylist()
        {
            DirectoryInfo fileInfo = new DirectoryInfo("Playlist");
            foreach (FileInfo file in fileInfo.GetFiles()) file.Delete();
        }

        private void DeleteMusic()
        {
            //Kill ffmpeg before deleting songs
            if (Program.Previousffmpeg != null)
            {
                if(!Program.Previousffmpeg.HasExited)
                {
                    Program.Previousffmpeg.Kill();
                    Program.Previousffmpeg.WaitForExit();
                    Program.Previousffmpeg = null;
                }
            }
            DirectoryInfo fileInfo = new DirectoryInfo("Temp");
            foreach (FileInfo file in fileInfo.GetFiles()) file.Delete();
        }

        private Process Youtube(string url)
        {
            var youtube = new ProcessStartInfo
            {
                FileName = "youtube-dl",
                Arguments = $"--extract-audio --audio-format mp3 --ignore-errors {url} -o /Temp/Downloaded1.%(ext)s",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            return Process.Start(youtube);
        }

        private Process CreateStream(bool isUrl, string path)
        {
            string finalPath = path;
            if (isUrl) finalPath = "Temp/Downloaded1.mp3";

            var ffmpeg = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-loglevel quiet -i {finalPath} -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            return Process.Start(ffmpeg);
        }

        private async Task SendAsync(IAudioClient client, string PathOrUrl)
        {
            //Youtube url
            if (PathOrUrl.Contains("youtu"))
            {
                //Delete song
                DeleteMusic();
                //Download new song
                var youtubeDL = Youtube(PathOrUrl);
                youtubeDL.WaitForExit();
                Program.SkippingSong = false;
                //Stream song
                var ffmpeg = CreateStream(true, "");
                Program.Previousffmpeg = ffmpeg;
                var output = ffmpeg.StandardOutput.BaseStream;
                var discord = client.CreatePCMStream(AudioApplication.Music);
                await output.CopyToAsync(discord);
                //await discord.FlushAsync();
            }
            //Physical song on computer or url to mp3
            else
            {
                Program.SkippingSong = false;
                //Stream song
                var ffmpeg = CreateStream(false, PathOrUrl);
                Program.Previousffmpeg = ffmpeg;
                var output = ffmpeg.StandardOutput.BaseStream;
                var discord = client.CreatePCMStream(AudioApplication.Music);
                await output.CopyToAsync(discord);
                //await discord.FlushAsync();
            }
            //Go to next song after current song ends
            await NextSong();
        }
        #endregion
    }
}