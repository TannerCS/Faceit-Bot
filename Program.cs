using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace FaceitBot
{
    class Program
    {
        public static DiscordSocketClient Client;
        public static Config Config;

        private CommandService _Commands;
        private IServiceProvider _Services;
        private string _ConfigFile = @"config.json";

        private static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            if (!File.Exists(_ConfigFile))
            {
                Config = new Config();
                Console.WriteLine("This is a one-time setup. You will not need to do this again.");
                Console.Write("Bot Token: ");
                Config.BotToken = Console.ReadLine();
                Console.Write("Faceit API Key (https://developers.faceit.com/apps/new): ");
                Config.FaceitAPIToken = Console.ReadLine();
                File.WriteAllText(_ConfigFile, JsonConvert.SerializeObject(Config));
            }
            else
            {
                string json = File.ReadAllText(_ConfigFile);
                Config = JsonConvert.DeserializeObject<Config>(json);
            }

            Client = new DiscordSocketClient();
            _Commands = new CommandService();

            _Services = new ServiceCollection()
                    .BuildServiceProvider();

            Client.Log += Log;
            await InstallCommands();


            await Client.LoginAsync(TokenType.Bot, Config.BotToken);
            await Client.StartAsync();
            await Task.Delay(-1);
        }

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            Client.MessageReceived += HandleCommand;
            // Discover all of the commands in this assembly and load them.
            await _Commands.AddModulesAsync(Assembly.GetEntryAssembly(), _Services);
        }

        public async Task Log(LogMessage message)
        {
            //log messages to console
            Console.WriteLine(message);
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos))) return;
            // Create a Command Context
            var context = new CommandContext(Client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await _Commands.ExecuteAsync(context, argPos, _Services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
