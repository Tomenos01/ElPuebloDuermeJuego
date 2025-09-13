using Microsoft.AspNetCore.SignalR;
using ElPuebloDuermeDemo.Services;
using ElPuebloDuermeDemo.Models;
using ElPuebloDuermeDemo.Domain.Actions;

namespace ElPuebloDuermeDemo.SignalR
{
    public class GameHub : Hub
    {
        private readonly IGameService _gameService;
        private readonly IVoteService _voteService;
        private readonly IMessagingService _messagingService;

        public GameHub(
            IGameService gameService, 
            IVoteService voteService,
            IMessagingService messagingService)
        {
            _gameService = gameService;
            _voteService = voteService;
            _messagingService = messagingService;
        }

        public async Task JoinGame(string gameId, string playerName)
        {
            var success = await _gameService.JoinGame(gameId, playerName, Context.ConnectionId);
            
            if (success)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
                await Clients.Group(gameId).SendAsync("PlayerJoined", playerName);
                await SendGameStateUpdate(gameId);
            }
            else
            {
                await Clients.Caller.SendAsync("JoinFailed", "No se pudo unir al juego");
            }
        }

        public async Task LeaveGame(string gameId, string playerId)
        {
            var success = await _gameService.LeaveGame(gameId, playerId);
            
            if (success)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
                await Clients.Group(gameId).SendAsync("PlayerLeft", playerId);
                await SendGameStateUpdate(gameId);
            }
        }

        public async Task StartGame(string gameId)
        {
            var success = await _gameService.StartGame(gameId);
            
            if (success)
            {
                var gameState = _gameService.GetGameState(gameId);
                if (gameState != null)
                {
                    // Enviar roles privadamente a cada jugador
                    foreach (var player in gameState.Players)
                    {
                        await Clients.Client(player.ConnectionId).SendAsync("RoleAssigned", player.Role);
                    }
                    
                    await Clients.Group(gameId).SendAsync("GameStarted");
                    await SendGameStateUpdate(gameId);
                }
            }
            else
            {
                await Clients.Caller.SendAsync("StartGameFailed", "No se pudo iniciar el juego");
            }
        }

        public async Task SubmitNightAction(string gameId, string actionType, string playerId, string? targetId = null)
        {
            try
            {
                IRoleAction? action = actionType.ToLower() switch
                {
                    "assassinate" => new AssassinateAction(playerId, targetId!),
                    "protect" => new ProtectAction(playerId, targetId!),
                    "heal" => new HealAction(playerId, targetId!),
                    "reveal" => new RevealRoleAction(playerId, targetId!),
                    "detectivekill" => new DetectiveKillAction(playerId, targetId!),
                    _ => null
                };

                if (action != null)
                {
                    var success = await _gameService.SubmitAction(gameId, action);
                    
                    if (success)
                    {
                        await Clients.Caller.SendAsync("ActionSubmitted", actionType);
                    }
                    else
                    {
                        await Clients.Caller.SendAsync("ActionFailed", "No se pudo ejecutar la acción");
                    }
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ActionFailed", ex.Message);
            }
        }

        public async Task SubmitVote(string gameId, string voterId, string targetId)
        {
            var success = await _voteService.CastVote(gameId, voterId, targetId);
            
            if (success)
            {
                await Clients.Caller.SendAsync("VoteSubmitted");
                
                // Verificar si todos han votado
                var allVotesIn = await _voteService.AreAllVotesIn(gameId);
                if (allVotesIn)
                {
                    await Clients.Group(gameId).SendAsync("AllVotesReceived");
                }
            }
            else
            {
                await Clients.Caller.SendAsync("VoteFailed", "No se pudo registrar el voto");
            }
        }

        public async Task AdvancePhase(string gameId)
        {
            var success = await _gameService.AdvancePhase(gameId);
            
            if (success)
            {
                var gameState = _gameService.GetGameState(gameId);
                if (gameState != null)
                {
                    await Clients.Group(gameId).SendAsync("PhaseChanged", gameState.CurrentPhase);
                    await SendGameStateUpdate(gameId);
                    
                    // Enviar mensajes específicos de la nueva fase
                    foreach (var message in gameState.GlobalMessages.TakeLast(1))
                    {
                        await Clients.Group(gameId).SendAsync("GlobalMessage", message);
                    }

                    // Si el juego terminó, revelar todos los roles
                    if (gameState.CurrentPhase == GamePhase.Finalizado)
                    {
                        var allRoles = gameState.Players.ToDictionary(p => p.Name, p => p.Role);
                        await Clients.Group(gameId).SendAsync("GameEnded", gameState.GetWinner(), allRoles);
                    }
                }
            }
        }

        public async Task SendChatMessage(string gameId, string playerName, string message)
        {
            var gameState = _gameService.GetGameState(gameId);
            
            // Solo permitir chat durante el día
            if (gameState?.CurrentPhase == GamePhase.Dia)
            {
                await Clients.Group(gameId).SendAsync("ChatMessage", playerName, message);
            }
        }

        public async Task GetGameState(string gameId)
        {
            await SendGameStateUpdate(gameId);
        }

        private async Task SendGameStateUpdate(string gameId)
        {
            var gameState = _gameService.GetGameState(gameId);
            if (gameState != null)
            {
                // Crear una versión "sanitizada" del estado para los clientes
                var publicGameState = new
                {
                    gameState.GameId,
                    gameState.CurrentPhase,
                    gameState.CurrentRound,
                    gameState.IsStarted,
                    Players = gameState.Players.Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.IsAlive,
                        p.VotesReceived,
                        // NO enviamos el rol, solo al final del juego
                        HasVoted = !string.IsNullOrEmpty(p.VotedFor)
                    }),
                    PlayerCount = gameState.Players.Count,
                    AlivePlayerCount = gameState.Players.Count(p => p.IsAlive)
                };

                await Clients.Group(gameId).SendAsync("GameStateUpdate", publicGameState);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Manejar desconexión del jugador
            // En un caso real, buscarías el gameId por el ConnectionId
            await base.OnDisconnectedAsync(exception);
        }
    }
}