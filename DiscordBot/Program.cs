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

namespace DiscordBot
{
    public class Program
    {
        /*Data*/
        public static string Token = "MzM1MjczOTU3MDEzOTc5MTQ3.DEnXuQ.Zrs7_8e_7k28ShwUqf0LAuEq-oQ";
        public static string Game = "-̸̷̨̨̢̯̰̣͔͍̼̯͔̺̦̾̃̋̌͆̍̏̋͋͋̂̾͊̄͠Dat A$$-̸̨̨̢̯̰̣͔͍̼̯͔̾̃̋̌͆̍̏̋͋͠";
        public static UserStatus Status = UserStatus.DoNotDisturb;
        //Voice Channel
        public static int PreviousVoiceChannelIndex = -1;
        public static IAudioClient PreviousAudioClient;
        public static bool SkippingSong = false;
        public static Process Previousffmpeg;
        //Cancer
        public static Random Rand = new Random();
        public static bool SayLoopDeleteMessage = true;
        //Church
        public static string ChurchGuildName = "Church Discord";
        public static string ChurchChannelName = "vote-channel";
        public static SocketGuild ChurchGuild;
        public static SocketTextChannel ChurchChannel;
        //Vote
        public static string VoteGuildName = "Relaxation Station";
        public static string VoteChannelName = "botspam";
        public static string AnnouncementChannelName = "general";
        public static SocketGuild VoteGuild;
        public static SocketTextChannel VoteChannel;
        public static SocketTextChannel AnnouncementChannel;

        static void Main(string[] args)
            => new Program().Start().GetAwaiter().GetResult();

        public static DiscordSocketClient _client = new DiscordSocketClient();
        public static Commands _command = new Commands();
        public static ChurchCommands _churchcommand = new ChurchCommands();
        public static VoteCommands _votecommand = new VoteCommands();

        public async Task Start()
        {
            await _client.LoginAsync(TokenType.Bot, Token);
            await _client.StartAsync();

            await _client.SetGameAsync(Game);
            await _client.SetStatusAsync(Status);

            await _command.InitializeAsync(_client);

            //await _churchcommand.InitializeAsync(_client);
            await _votecommand.InitializeAsync(_client);

            await Task.Delay(-1);
        }
    }
}
