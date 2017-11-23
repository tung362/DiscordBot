using Discord.Audio;
using System.Diagnostics;

namespace DiscordBot.References
{
    class VoiceInfo
    {
        public static int PreviousVoiceChannelIndex = -1;
        public static IAudioClient PreviousAudioClient;
        public static bool SkippingSong = false;
        public static Process Previousffmpeg;
    }
}
