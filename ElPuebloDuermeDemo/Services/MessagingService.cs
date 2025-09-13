using ElPuebloDuermeDemo.Models;

namespace ElPuebloDuermeDemo.Services
{
    public interface IMessagingService
    {
        Task SendMessageToAll(string gameId, string message);
        Task SendMessageToPlayer(string gameId, string playerId, string message);
        Task SendRoleToPlayer(string gameId, string playerId, Role role);
        Task SendGameStateUpdate(string gameId);
        Task SendPhaseChange(string gameId, GamePhase newPhase);
        Task SendPlayerDeath(string gameId, List<string> deadPlayerIds);
        Task SendVotingResults(string gameId, string? eliminatedPlayerId);
        Task SendGameEnd(string gameId, string winner, Dictionary<string, Role> allRoles);
        Task SendRoleReveal(string playerId, string targetId, Role revealedRole);
    }

    public class MessagingService : IMessagingService
    {
        // Esta clase será implementada cuando agreguemos SignalR
        // Por ahora solo definimos la interfaz

        public async Task SendMessageToAll(string gameId, string message)
        {
            // Implementar con SignalR Hub
            await Task.CompletedTask;
        }

        public async Task SendMessageToPlayer(string gameId, string playerId, string message)
        {
            // Implementar con SignalR Hub - mensaje dirigido
            await Task.CompletedTask;
        }

        public async Task SendRoleToPlayer(string gameId, string playerId, Role role)
        {
            // Enviar rol privadamente al jugador
            await Task.CompletedTask;
        }

        public async Task SendGameStateUpdate(string gameId)
        {
            // Enviar estado actual del juego a todos los jugadores
            await Task.CompletedTask;
        }

        public async Task SendPhaseChange(string gameId, GamePhase newPhase)
        {
            // Notificar cambio de fase
            await Task.CompletedTask;
        }

        public async Task SendPlayerDeath(string gameId, List<string> deadPlayerIds)
        {
            // Notificar muertes
            await Task.CompletedTask;
        }

        public async Task SendVotingResults(string gameId, string? eliminatedPlayerId)
        {
            // Enviar resultados de votación
            await Task.CompletedTask;
        }

        public async Task SendGameEnd(string gameId, string winner, Dictionary<string, Role> allRoles)
        {
            // Enviar final del juego con roles revelados
            await Task.CompletedTask;
        }

        public async Task SendRoleReveal(string playerId, string targetId, Role revealedRole)
        {
            // Enviar rol revelado solo al clérigo que lo solicitó
            await Task.CompletedTask;
        }
    }
}