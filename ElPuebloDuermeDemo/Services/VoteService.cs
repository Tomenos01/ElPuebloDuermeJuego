using ElPuebloDuermeDemo.Models;
using ElPuebloDuermeDemo.Domain;

namespace ElPuebloDuermeDemo.Services
{
    public interface IVoteService
    {
        Task<bool> CastVote(string gameId, string voterId, string targetId);
        Task<Dictionary<string, int>> GetVoteResults(string gameId);
        Task<bool> HasPlayerVoted(string gameId, string playerId);
        Task<bool> AreAllVotesIn(string gameId);
        Task<Player?> GetVotingResult(string gameId);
    }

    public class VoteService : IVoteService
    {
        private readonly IGameService _gameService;

        public VoteService(IGameService gameService)
        {
            _gameService = gameService;
        }

        public async Task<bool> CastVote(string gameId, string voterId, string targetId)
        {
            return await _gameService.SubmitVote(gameId, voterId, targetId);
        }

        public async Task<Dictionary<string, int>> GetVoteResults(string gameId)
        {
            var gameState = _gameService.GetGameState(gameId);
            if (gameState == null) return new Dictionary<string, int>();

            var results = new Dictionary<string, int>();
            
            foreach (var player in gameState.Players.Where(p => p.IsAlive))
            {
                results[player.Id] = player.VotesReceived;
            }

            await Task.CompletedTask;
            return results;
        }

        public async Task<bool> HasPlayerVoted(string gameId, string playerId)
        {
            var gameState = _gameService.GetGameState(gameId);
            if (gameState == null) return false;

            var player = gameState.GetPlayer(playerId);
            if (player == null) return false;

            await Task.CompletedTask;
            return !string.IsNullOrEmpty(player.VotedFor);
        }

        public async Task<bool> AreAllVotesIn(string gameId)
        {
            var gameState = _gameService.GetGameState(gameId);
            if (gameState == null) return false;

            var alivePlayers = gameState.Players.Where(p => p.IsAlive).ToList();
            var playersWhoVoted = alivePlayers.Where(p => !string.IsNullOrEmpty(p.VotedFor)).Count();

            await Task.CompletedTask;
            return playersWhoVoted == alivePlayers.Count;
        }

        public async Task<Player?> GetVotingResult(string gameId)
        {
            var gameState = _gameService.GetGameState(gameId);
            if (gameState == null) return null;

            await Task.CompletedTask;
            return gameState.GetMostVotedPlayer();
        }
    }
}