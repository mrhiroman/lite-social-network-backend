namespace lite_social_network_backend.Signalr;

using Microsoft.AspNetCore.SignalR;
public class SignalrHub : Hub
{
    public async Task NewMessage(string user, string message)
    {
        await Clients.All.SendAsync("messageReceived", user, message);
    }
}