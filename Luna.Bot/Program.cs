using Discord;
using Discord.WebSocket;

namespace Luna.Bot;

class Program
{
    private static DiscordSocketClient _client;
    
    private static async Task Main() 
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent });
        _client.MessageReceived += 
            ClientOnMessageReceived;
        _client.Log += ClientOnLog;
        
        string tokken = "";
        
        await _client.LoginAsync(TokenType.Bot, tokken);
        await _client.StartAsync();

        Console.WriteLine("Listening...");
        Console.ReadLine();
    }
    
    private static Task ClientOnLog(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        
        return Task.CompletedTask;
    }

    private static Task ClientOnMessageReceived(SocketMessage message)
    {
        if (!message.Author.IsBot)
        {
            message.Channel.SendMessageAsync($"{message.Content}");
        }
        
        return Task.CompletedTask;
    }
}