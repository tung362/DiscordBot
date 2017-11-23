using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.Static;
using Discord.Net.Providers.WS4Net;
using Discord.Net.Providers.UDPClient;
using System;

namespace DiscordBot
{
    public class Program
    {
        private static DiscordSocketClient _client = new DiscordSocketClient();
        private static Commands _command = new Commands();

        static void Main(string[] args)
            => new Program().Start().GetAwaiter().GetResult();

        private async Task Start()
        {
            CreateSocketClient();
            await Task.Factory.StartNew(async () => await InitClientWithInfoAsync());
            await Task.Factory.StartNew(async () => await InitCommandAsync());

            await Task.Delay(-1);
        }

        private void CreateSocketClient()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                WebSocketProvider = WS4NetProvider.Instance,
                UdpSocketProvider = UDPClientProvider.Instance,
                ConnectionTimeout = 10000 // Ten seconds
            });
        }

        private async Task InitClientWithInfoAsync()
        {
            string token = BotInfo.Token;
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _client.SetGameAsync(BotInfo.Game);
            await _client.SetStatusAsync(BotInfo.Status);

            _client.Log += (m) =>
            {
                Console.Write(m.ToString() + "\n");
                return Task.Delay(0);
            };
        }

        private async Task InitCommandAsync()
        {
            await _command.InitializeAsync(_client);
            //await VoteInfo.VoteCommand.InitializeAsync(_client);
        }
    }
}