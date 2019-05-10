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
    [Group("match")]
    public class Match : ModuleBase<CommandContext>
    {
        [Command]
        public async Task Help()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Match Command Help");
            builder.WithDescription("!match info <match ID> - Returns information about a match.");
            await ReplyAsync("", false, builder.Build());
        }

        [Command("info")]
        public async Task Info(string matchID)
        {
            WebClient client = new WebClient();
            EmbedBuilder builder = new EmbedBuilder();
            Dictionary<string, string> teams = new Dictionary<string, string>();

            client.Headers.Add("Authorization", $"Bearer {Program.Config.FaceitAPIToken}");
            JObject data = JObject.Parse(client.DownloadString("https://open.faceit.com/data/v4/matches/" + matchID));

            foreach(var team in data["teams"].Children())
            {
                string players = "";
                foreach(var player in team.First["roster"])
                {
                    players += player["nickname"].ToString() + "\n";
                }
                teams.Add(((JProperty)team).Name, players);
            }

            builder.WithTitle($"{data["competition_name"].ToString()} - Best Of {data["best_of"].ToString()}")
            .WithDescription($"Status: {data["status"].ToString()}");

            if(data["status"].ToString() == "FINISHED")
            {
                Console.WriteLine(teams[data["results"]["winner"].ToString()]);
                builder.Description += $"\nWinner: Team {teams.Keys.ToList().IndexOf(data["results"]["winner"].ToString()) + 1}";
                builder.WithFooter($"Demo: {data["demo_url"][0].ToString()}");
            }

            for(int i = 0; i < teams.Count; i++)
            {
                builder.AddField($"Team {i + 1}", teams.Values.ElementAt(i), true);
            }

            await ReplyAsync("", false, builder.Build());
        }
    }
}
