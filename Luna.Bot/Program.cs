using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SharpLink;

namespace Luna.Bot;

class Program
{
    private static DiscordSocketClient _client;

    public static LavalinkManager lavalinkManager;
    
    private static InteractionService _interactionService;
    
    private static CommandService _commands;
    
    private static IServiceProvider _services;
    
    private static async Task Main() 
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent });
        _client.MessageReceived += 
            ClientOnMessageReceived;
        _client.Log += ClientOnLog;
        _client.Ready += ClientOnReady;
        _client.ReactionAdded += ClientOnReactionAdded;
        _client.SlashCommandExecuted += HandleInteraction;
        
        _commands = new CommandService();
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        
        var collection = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .BuildServiceProvider();
        
        _services = collection;
        
        string tokken = "MTM1MDQ1MjMxNzE5MDc1MDIxOA.Gdfm7c.8zYOhamt_udoTj7OXGDz2B2v7e3LGo4T-5pzjM";
        
        await _client.LoginAsync(TokenType.Bot, tokken);
        await _client.StartAsync();
        
        Console.WriteLine("Listening...");
        Console.ReadLine();
    }
    
    private static async Task HandleInteraction(SocketInteraction interaction)
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        var context = new SocketInteractionContext(_client, interaction);
        
        var slah = interaction as SocketSlashCommand;

        if (slah.CommandName == "ban")
        {
            await context.Guild.AddBanAsync((IUser)slah.Data.Options.FirstOrDefault(x => x.Name == "user").Value);
            await HandleListRoleCommand(slah);
        }
        if (slah.CommandName == "kick")
        {
            var user = ((IGuildUser)slah.Data.Options.FirstOrDefault(x => x.Name == "user").Value);
            await user.KickAsync();
            await HandleListRoleCommand(slah);
        }
    }

    private static async Task HandleListRoleCommand(SocketSlashCommand command)
    {
        var guildUser = (SocketGuildUser)command.Data.Options.First().Value;

        var roleList = string.Join(",\n", guildUser.Roles.Where(x => !x.IsEveryone).Select(x => x.Mention));

        var embedBuiler = new EmbedBuilder()
            .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
            .WithTitle("Roles")
            .WithDescription(roleList)
            .WithColor(Color.Green)
            .WithCurrentTimestamp();

        await command.RespondAsync(embed: embedBuiler.Build());
    }
    
    private static async Task ClientOnReady()
    {
        var ban = new SlashCommandBuilder() 
            .WithName("ban")   
            .WithDescription("забанить нахуй5656")
            .AddOption("user", ApplicationCommandOptionType.User, "выберите пользователя", isRequired: true);
        
        var kick = new SlashCommandBuilder() 
            .WithName("kick")   
            .WithDescription("кикнуть нахуй5656")
            .AddOption("user", ApplicationCommandOptionType.User, "выберите пользователя", isRequired: true);
        
        try
        {
            var guid = _client.GetGuild(983375413160058904);
            
            await guid.CreateApplicationCommandAsync(ban.Build());   // Создаём команду
            await guid.CreateApplicationCommandAsync(kick.Build());   // Создаём команду
        }
        catch (ApplicationCommandException exception)   // Обработчик каких-либо исключений
        {
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
        
            Console.WriteLine(json);
        }
    }

    private static async Task ClientOnReactionAdded(Cacheable<IUserMessage, ulong> userMessage, Cacheable<IMessageChannel, ulong> messageChannel, SocketReaction reaction)
    {
        var guild = ((SocketGuildUser)reaction.User.Value).Guild;
        var role = guild.Roles.FirstOrDefault(r => r.Name == "крутыш");

        if (reaction.Emote.Name == "\ud83d\udc40")
        {
            await ((SocketGuildUser)reaction.User).AddRoleAsync(role);
        }
        
        if (role != null)
        {
            await ((SocketGuildUser)userMessage.Value.Author).AddRoleAsync(role);
            await userMessage.Value.Channel.SendMessageAsync($"Роль {"roleName"} выдана пользователю {userMessage.Value.Author.Mention}");
        }
    }

    private static Task ClientOnLog(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        
        return Task.CompletedTask;
    }

    private static async Task ClientOnMessageReceived(SocketMessage message)
    {
        if (!message.Author.IsBot)
        {
            if (message.Content == "!буп")
            {
                
            }
            if (message.Content == "!создатель")
            {
                var user = _client.GetUser(message.Author.Username);
                
                message.Channel.SendMessageAsync($"{message.Author.Username} {string.Join(",", message.Author.Activities)}");
            }
        }
    }
}