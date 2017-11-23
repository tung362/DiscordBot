using Discord.WebSocket;

namespace DiscordBot.References
{
    class ChurchInfo
    {
        public static string ChurchGuildName = "Church Discord";
        public static string ChurchChannelName = "vote-channel";
        public static SocketGuild ChurchGuild;
        public static SocketTextChannel ChurchChannel;

        private static ChurchCommands _churchcommand = new ChurchCommands();
    }
}
