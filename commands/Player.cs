using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FaceitBot.commands
{
    [Group("player")]
    public class Player : ModuleBase<CommandContext>
    {
        [Command("stats")]
        public async Task Stats(string playerName)
        {
            WebClient client = new WebClient();
            EmbedBuilder builder = new EmbedBuilder();

            client.Headers.Add("Authorization", $"Bearer {Program.Config.FaceitAPIToken}");
            JObject data = JObject.Parse(client.DownloadString("https://open.faceit.com/data/v4/players?nickname=" + playerName));

            builder.WithAuthor(data["nickname"].ToString());
            builder.WithDescription($"Country: :flag_{data["country"].ToString().ToLower()}:");
            if (data["avatar"].ToString() != "null") {
                builder.WithThumbnailUrl(data["avatar"].ToString());
            }

            string infractions = "";
            foreach(var infraction in data["infractions"].ToObject<Dictionary<object, object>>().Keys.ToList().Skip(1))
            {
                infractions += infraction + ": " + data["infractions"][infraction] + "\n";
            }

            builder.AddField("Infractions", infractions);

            await ReplyAsync("", false, builder.Build());
        }
    }
}
