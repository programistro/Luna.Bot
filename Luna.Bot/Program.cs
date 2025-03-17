using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using SharpLink;

namespace Luna.Bot;

class Program
{
    private static DiscordSocketClient _client;

    public static LavalinkManager lavalinkManager;
    
    private static InteractionService _interactionService;
    
    private static async Task Main() 
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent });
        _client.MessageReceived += 
            ClientOnMessageReceived;
        _client.Log += ClientOnLog;
        _client.ReactionAdded += ClientOnReactionAdded;
        
        lavalinkManager = new LavalinkManager(_client, new LavalinkManagerConfig
        {
            RESTHost = "localhost",
            RESTPort = 2333,
            WebSocketHost = "localhost",
            WebSocketPort = 2333,
            Authorization = "YOUR_SECRET_AUTHORIZATION_KEY",
            TotalShards = 1 
        });

        _client.Ready += async () =>
        {
            await lavalinkManager.StartAsync();
        };
        
        string tokken = "MTM1MDQ1MjMxNzE5MDc1MDIxOA.Gdfm7c.8zYOhamt_udoTj7OXGDz2B2v7e3LGo4T-5pzjM";
        
        await _client.LoginAsync(TokenType.Bot, tokken);
        await _client.StartAsync();

        Console.WriteLine("Listening...");
        Console.ReadLine();
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

public class MusicModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("play", "Воспроизвести музыку")]
    public async Task PlayAsync()
    {
        // Context автоматически доступен здесь
        var guild = Context.Guild;
        var channel = Context.Channel;
        var user = Context.User;
        
        var player = Program.lavalinkManager.GetPlayer(Context.Guild.Id) ?? 
                     await Program.lavalinkManager.JoinAsync(Context.Guild.GetVoiceChannel(Context.User.Id));
        LoadTracksResponse response = await Program.lavalinkManager.GetTracksAsync("https://www.youtube.com/watch?v=4mF7h5WRs6A&list=RD4mF7h5WRs6A&start_radio=1&ab_channel=%E8%8F%AF%E7%B4%8D%E9%9F%B3%E6%A8%82%E8%A5%BF%E6%B4%8B%E6%97%A5%E9%9F%93%E9%A0%BB%E9%81%93");
        LavalinkTrack track = response.Tracks.First();
        await player.PlayAsync(track);
    }
}