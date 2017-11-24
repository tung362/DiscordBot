using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;

namespace DiscordBot.Modules
{
    public class CoreCommand : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help()
        {
            var eb = new EmbedBuilder();
            eb.Title = "📝 List of commands: ";
            string text =
                "```css" + "\n" +
                "👑 [General]" + "\n" +
                ".help                          :   Display commands" + "\n" +
                ".begone                        :   Log off" + "\n" +
                ".about                         :   Credits" + "\n" +
                ".status                        :   Server info" + "\n" +
                ".servers                       :   All servers connected to" + "\n" +
                ".roles <ServerID>              :   All roles of a server" + "\n" +
                ".permissions <ServerID> <RoleID> :   All permissions of a role" + "\n" +
                "```" + "\n" +

                "```css" + "\n" +
                "🔊 [Channel]" + "\n" +
                ".channels                      :   Displays channel name and index" + "\n" +
                ".join <ChannelID>              :   Joins a voice channel by index" + "\n" +
                ".joinme                        :   Joins current channel" + "\n" +
                ".leave                         :   Leaves the current channel" + "\n" +
                "```" + "\n" +

                "```css" + "\n" +
                "🎶 [Music]" + "\n" +
                ".playlist                      :   All songs on queue" + "\n" +
                ".play <Url or Path>            :   Adds music from url or root path to queue" + "\n" +
                ".skip                          :   Skip to next queue" + "\n" +
                ".stop                          :   Stop playing and clear playlist" + "\n" +
                "```" + "\n" +

                "```css" + "\n" +
                "🤡 [Cancer]" + "\n" +
                ".insult                        :   Insults a random online user" + "\n" +
                ".insultlist                    :   List of all insult entry indexes" + "\n" +
                ".addinsult <Insult \"\">         :   Add insult to list of insults" + "\n" +
                ".editinsult <Index> <Insult>   :   Edits an existing insult" + "\n" +
                ".deleteinsult <Index>          :   Deletes an existing insult" + "\n" +
                ".goodvideo                     :   Send a random video" + "\n" +
                ".goodvideolist                 :   List of all video entry indexes" + "\n" +
                ".addgoodvideo <video \"\">       :   Add video to list of videos" + "\n" +
                ".editgoodvideo <Index> <video> :   Edits an existing video" + "\n" +
                ".deletegoodvideo <Index>       :   Deletes an existing video" + "\n" +
                ".say <Text>                    :   Make bot say something" + "\n" +
                ".votetopic <Topic \"\">          :   Change voting topic" + "\n" +
                "```";
            eb.WithDescription(text);
            eb.WithColor(Color.Green);
            await Context.Channel.SendMessageAsync("", false, eb);
            await Context.Message.DeleteAsync();
        }

        [Command("clear")]
        public async Task Clear()
        {
            await Context.Message.DeleteAsync();

            //Gets messages
            var messages = await Context.Channel.GetMessagesAsync(int.MaxValue).Flatten();
            var msgs = messages.ToList<IMessage>();

            //Make filter out any non bot messages
            for(int i = 0; i < msgs.Count; i++)
            {
                TimeSpan difference = DateTimeOffset.Now.Subtract(msgs[i].CreatedAt);
                if (msgs[i].Author.Id != Context.Client.CurrentUser.Id || difference.TotalMilliseconds >= (double)1203552000)
                {
                    msgs.RemoveAt(i);
                    i -= 1;
                }
            }

            await Context.Channel.DeleteMessagesAsync(msgs);
        }

        [Command("clearuser")]
        public async Task Clear(ulong id)
        {
            await Context.Message.DeleteAsync();

            //Gets messages
            var messages = await Context.Channel.GetMessagesAsync(int.MaxValue).Flatten();
            var msgs = messages.ToList<IMessage>();

            //Make filter out any non bot messages
            for (int i = 0; i < msgs.Count; i++)
            {
                TimeSpan difference = DateTimeOffset.Now.Subtract(msgs[i].CreatedAt);
                if (msgs[i].Author.Id != id || difference.TotalMilliseconds >= (double)1203552000)
                {
                    msgs.RemoveAt(i);
                    i -= 1;
                }
            }

            await Context.Channel.DeleteMessagesAsync(msgs);
        }

        [Command("begone")]
        public async Task Exit()
        {
            await Context.Message.DeleteAsync();
        }
    }
}
