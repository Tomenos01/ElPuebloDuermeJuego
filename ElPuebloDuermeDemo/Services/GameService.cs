using ElPuebloDuermeDemo.Models;
using ElPuebloDuermeDemo.Domain;
using ElPuebloDuermeDemo.Domain.Actions;

namespace ElPuebloDuermeDemo.Services
{
    public interface IGameService
    {
        Task<string> CreateGame();
        Task<bool> JoinGame(string gameId, string playerName, string connectionId);
        Task<bool> LeaveGame(string gameId, string playerId);
        Task<bool> StartGame(string gameId);
        Task<bool> SubmitAction(string gameId, IRoleAction action);
        Task<bool> SubmitVote(string gameId, string voterId, string targetId);
        Task<bool> AdvancePhase(string gameId);
        GameState? GetGameState(string gameId);
        Task<List<GameState>> GetAllGames();
        Task<bool> EndGame(string gameId);
    }

    public class GameService : IGameService
    {
        private readonly Dictionary<string, GameState> _games = new();
        private readonly IRoleAssignmentService _roleAssignmentService;
        private readonly IPhaseManager _phaseManager;
        private readonly IGameRulesValidator _rulesValidator;

        public GameService(
            IRoleAssignmentService roleAssignmentService,
            IPhaseManager phaseManager,
            IGameRulesValidator rulesValidator)
        {
            _roleAssignmentService = roleAssignmentService;
            _phaseManager = phaseManager;
            _rulesValidator = rulesValidator;
        }

        public async Task<string> CreateGame()
        {
            var gameId = Guid.NewGuid().ToString();
            var gameState = new GameState
            {
                GameId = gameId,
                CurrentPhase = GamePhase.Preparacion,
                CurrentRound = 1,
                IsStarted = false
            };

            _games[gameId] = gameState;
            
            await Task.CompletedTask;
            return gameId;
        }

        public async Task<bool> JoinGame(string gameId, string playerName, string connectionId)
        {
            if (!_games.TryGetValue(gameId, out var gameState))
                return false;

            if (gameState.IsStarted)
                return false;

            if (gameState.Players.Count >= 12)
                return false;

            if (gameState.Players.Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
                return false;

            var player = new Player
            {
                Name = playerName,
                ConnectionId = connectionId
            };

            gameState.AddPlayer(player);
            gameState.AddGlobalMessage($"{playerName} se ha unido al juego.");

            await Task.CompletedTask;
            return true;
        }

        public async Task<bool> LeaveGame(string gameId, string playerId)
        {
            if (!_games.TryGetValue(gameId, out var gameState))
                return false;

            var player = gameState.GetPlayer(playerId);
            if (player == null)
                return false;

            if (!gameState.IsStarted)
            {
                gameState.RemovePlayer(playerId);
                gameState.AddGlobalMessage($"{player.Name} ha abandonado el juego.");
            }
            else
            {
                // Durante el juego, marcar como desconectado pero no remover
                player.IsAlive = false;
                gameState.AddDeadPlayer(playerId);
                gameState.AddGlobalMessage($"{player.Name} se ha desconectado y ha sido eliminado del juego.");
            }

            await Task.CompletedTask;
            return true;
        }

        public async Task<bool> StartGame(string gameId)
        {
            if (!_games.TryGetValue(gameId, out var gameState))
                return false;

            if (gameState.IsStarted)
                return false;

            if (!_rulesValidator.IsGameStartable(gameState))
                return false;

            // Asignar roles
            _roleAssignmentService.AssignRoles(gameState.Players);
            
            gameState.IsStarted = true;
            gameState.CurrentPhase = GamePhase.Preparacion;
            gameState.AddGlobalMessage("¡El juego ha comenzado! Todos los jugadores han recibido sus roles.");

            await Task.CompletedTask;
            return true;
        }

        public async Task<bool> SubmitAction(string gameId, IRoleAction action)
        {
            if (!_games.TryGetValue(gameId, out var gameState))
                return false;

            if (gameState.CurrentPhase != GamePhase.Noche)
                return false;

            if (!_rulesValidator.IsValidAction(action, gameState))
                return false;

            gameState.AddAction(action);

            await Task.CompletedTask;
            return true;
        }

        public async Task<bool> SubmitVote(string gameId, string voterId, string targetId)
        {
            if (!_games.TryGetValue(gameId, out var gameState))
                return false;

            if (!_rulesValidator.CanPlayerVote(voterId, gameState))
                return false;

            var target = gameState.GetPlayer(targetId);
            if (target == null || !target.IsAlive)
                return false;

            gameState.AddVote(voterId, targetId);

            await Task.CompletedTask;
            return true;
        }

        public async Task<bool> AdvancePhase(string gameId)
        {
            if (!_games.TryGetValue(gameId, out var gameState))
                return false;

            var result = await _phaseManager.AdvancePhase(gameState);
            
            // Verificar condiciones especiales de victoria
            if (gameState.CurrentPhase == GamePhase.Acusacion)
            {
                var mostVoted = gameState.GetMostVotedPlayer();
                if (mostVoted?.Role == Role.Bufon)
                {
                    gameState.CurrentPhase = GamePhase.Finalizado;
                    gameState.AddGlobalMessage("¡El Bufón ha ganado al ser eliminado por votación!");
                }
            }

            return result;
        }

        public GameState? GetGameState(string gameId)
        {
            _games.TryGetValue(gameId, out var gameState);
            return gameState;
        }

        public async Task<List<GameState>> GetAllGames()
        {
            await Task.CompletedTask;
            return _games.Values.ToList();
        }

        public async Task<bool> EndGame(string gameId)
        {
            if (!_games.TryGetValue(gameId, out var gameState))
                return false;

            gameState.CurrentPhase = GamePhase.Finalizado;
            gameState.AddGlobalMessage("El juego ha sido terminado por el administrador.");

            await Task.CompletedTask;
            return true;
        }
    }
}