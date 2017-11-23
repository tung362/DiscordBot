using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.Static;
using Discord.Net.Providers.WS4Net;
using Discord.Net.Providers.UDPClient;

namespace DiscordBot
{
    public class Program
    {
        static void Main(string[] args)
            => new Program().Start().GetAwaiter().GetResult();

        private static DiscordSocketClient _client = new DiscordSocketClient();
        private static Commands _command = new Commands();
        private static ChurchCommands _churchcommand = new ChurchCommands();
        private static VoteCommands _votecommand = new VoteCommands();
        public static VoteCommands VoteCommand { get { return _votecommand; } }

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
