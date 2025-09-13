using ElPuebloDuermeDemo.Models;
using ElPuebloDuermeDemo.Domain.Actions;

namespace ElPuebloDuermeDemo.Domain
{
    public class GameState : IGameState
    {
        public List<Player> Players { get; private set; } = new();
        public GamePhase CurrentPhase { get; set; } = GamePhase.Preparacion;
        public int CurrentRound { get; set; } = 1;
        public List<string> DeadPlayersThisRound { get; private set; } = new();
        public List<string> GlobalMessages { get; private set; } = new();
        public List<IRoleAction> PendingActions { get; private set; } = new();
        public Dictionary<string, int> Votes { get; private set; } = new();
        public bool IsStarted { get; set; } = false;
        public string? GameId { get; set; } = Guid.NewGuid().ToString();

        public Player? GetPlayer(string? playerId)
        {
            if (string.IsNullOrEmpty(playerId)) return null;
            return Players.FirstOrDefault(p => p.Id == playerId);
        }

        public void AddPlayer(Player player)
        {
            if (!Players.Any(p => p.Id == player.Id))
            {
                Players.Add(player);
            }
        }

        public void RemovePlayer(string playerId)
        {
            Players.RemoveAll(p => p.Id == playerId);
        }

        public void AddGlobalMessage(string message)
        {
            GlobalMessages.Add($"[Ronda {CurrentRound}] {message}");
        }

        public void AddDeadPlayer(string playerId)
        {
            if (!DeadPlayersThisRound.Contains(playerId))
            {
                DeadPlayersThisRound.Add(playerId);
            }
        }

        public void AddAction(IRoleAction action)
        {
            PendingActions.Add(action);
        }

        public void ClearActions()
        {
            PendingActions.Clear();
        }

        public void ClearVotes()
        {
            Votes.Clear();
            foreach (var player in Players)
            {
                player.VotesReceived = 0;
                player.VotedFor = null;
            }
        }

        public void AddVote(string voterId, string targetId)
        {
            var voter = GetPlayer(voterId);
            if (voter == null || !voter.IsAlive) return;

            // Remover voto anterior si existe
            if (!string.IsNullOrEmpty(voter.VotedFor))
            {
                var previousTarget = GetPlayer(voter.VotedFor);
                if (previousTarget != null)
                {
                    previousTarget.VotesReceived = Math.Max(0, previousTarget.VotesReceived - (voter.Role == Role.Detective ? 2 : 1));
                }
            }

            // Agregar nuevo voto
            voter.VotedFor = targetId;
            var target = GetPlayer(targetId);
            if (target != null)
            {
                // El detective vota doble
                int voteWeight = voter.Role == Role.Detective ? 2 : 1;
                target.VotesReceived += voteWeight;
            }
        }

        public void ResetForNewRound()
        {
            DeadPlayersThisRound.Clear();
            ClearActions();
            
            // Resetear protecciones
            foreach (var player in Players)
            {
                player.ResetProtection();
            }
        }

        public bool IsGameFinished()
        {
            var alivePlayers = Players.Where(p => p.IsAlive).ToList();
            var aliveAssassins = alivePlayers.Where(p => p.Role == Role.Asesino).Count();
            var aliveNonAssassins = alivePlayers.Count - aliveAssassins;

            // Condición 1: Los asesinos son igual o mayor número que el resto
            if (aliveAssassins >= aliveNonAssassins) return true;

            // Condición 2: No quedan asesinos vivos
            if (aliveAssassins == 0) return true;

            // Condición 3: El bufón fue votado (se verifica en VoteService)
            return false;
        }

        public string? GetWinner()
        {
            var alivePlayers = Players.Where(p => p.IsAlive).ToList();
            var aliveAssassins = alivePlayers.Where(p => p.Role == Role.Asesino).Count();

            if (aliveAssassins >= alivePlayers.Count - aliveAssassins)
            {
                return "Asesinos";
            }
            
            if (aliveAssassins == 0)
            {
                return "Aldeanos";
            }

            return null;
        }

        public Player? GetMostVotedPlayer()
        {
            var alivePlayers = Players.Where(p => p.IsAlive).ToList();
            if (!alivePlayers.Any()) return null;

            var maxVotes = alivePlayers.Max(p => p.VotesReceived);
            if (maxVotes == 0) return null;

            var mostVoted = alivePlayers.Where(p => p.VotesReceived == maxVotes).ToList();
            
            // Si hay empate, nadie es eliminado
            if (mostVoted.Count > 1) return null;

            return mostVoted.First();
        }
    }
}