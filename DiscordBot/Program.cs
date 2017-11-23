using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Audio;
using System.Diagnostics;
using DiscordBot.Static;
using Discord.Net.Providers.WS4Net;
using Discord.Net.Providers.UDPClient;

namespace DiscordBot
{
    public class Program
    {        
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
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                WebSocketProvider = WS4NetProvider.Instance,
                UdpSocketProvider = UDPClientProvider.Instance,
                ConnectionTimeout = 10000 // Ten seconds
            });

            await _client.LoginAsync(TokenType.Bot, BotInfo.Token);
            await _client.StartAsync();

            await _client.SetGameAsync(BotInfo.Game);
            await _client.SetStatusAsync(BotInfo.Status);

            await _command.InitializeAsync(_client);

            //await _churchcommand.InitializeAsync(_client);
            await _votecommand.InitializeAsync(_client);

            await Task.Delay(-1);
        }
    }
}
