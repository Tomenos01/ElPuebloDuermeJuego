using Microsoft.AspNetCore.SignalR;

namespace ElPuebloDuermeDemo.SignalR
{
    public class ChatHub : Hub
    {
        public async Task JoinGameChat(string gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{gameId}");
        }

        public async Task LeaveGameChat(string gameId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{gameId}");
        }

        public async Task SendMessage(string gameId, string playerName, string message)
        {
            // Solo durante el día se permite el chat
            await Clients.Group($"chat_{gameId}").SendAsync("ReceiveMessage", playerName, message);
        }

        public async Task SendPrivateMessage(string gameId, string fromPlayer, string toPlayerId, string message)
        {
            // Para mensajes privados (como el clérigo viendo roles)
            await Clients.Client(toPlayerId).SendAsync("ReceivePrivateMessage", fromPlayer, message);
        }
    }
}