using Discord.WebSocket;

namespace DiscordBot.References
{
    class VoteInfo
    {
        public static string VoteGuildName = "Relaxation Station";
        public static string VoteChannelName = "botspam";
        public static string AnnouncementChannelName = "general";
        public static SocketGuild VoteGuild;
        public static SocketTextChannel VoteChannel;
        public static SocketTextChannel AnnouncementChannel;

        private static VoteCommands _votecommand = new VoteCommands();
        public static VoteCommands VoteCommand { get { return _votecommand; } }
    }
}
