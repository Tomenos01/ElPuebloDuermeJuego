using ElPuebloDuermeDemo.Models;
using ElPuebloDuermeDemo.Domain.Actions;

namespace ElPuebloDuermeDemo.Domain
{
    public interface IGameRulesValidator
    {
        bool IsValidAction(IRoleAction action, IGameState gameState);
        bool CanPlayerVote(string playerId, IGameState gameState);
        bool IsGameStartable(IGameState gameState);
        List<string> GetValidationErrors(IGameState gameState);
    }

    public class GameRulesValidator : IGameRulesValidator
    {
        public bool IsValidAction(IRoleAction action, IGameState gameState)
        {
            try
            {
                return action.CanExecute(gameState);
            }
            catch
            {
                return false;
            }
        }

        public bool CanPlayerVote(string playerId, IGameState gameState)
        {
            var player = gameState.GetPlayer(playerId);
            if (player == null) return false;
            if (!player.IsAlive) return false;
            if (gameState.CurrentPhase != GamePhase.Acusacion) return false;

            return true;
        }

        public bool IsGameStartable(IGameState gameState)
        {
            var players = gameState.Players;
            
            // Mínimo 4 jugadores para empezar
            if (players.Count < 4) return false;
            
            // Máximo 12 jugadores
            if (players.Count > 12) return false;

            // Todos los jugadores deben estar vivos al inicio
            if (players.Any(p => !p.IsAlive)) return false;

            return true;
        }

        public List<string> GetValidationErrors(IGameState gameState)
        {
            var errors = new List<string>();
            var players = gameState.Players;

            if (players.Count < 4)
                errors.Add("Se necesitan al menos 4 jugadores para comenzar el juego.");

            if (players.Count > 12)
                errors.Add("No se pueden tener más de 12 jugadores en el juego.");

            if (players.Any(p => string.IsNullOrWhiteSpace(p.Name)))
                errors.Add("Todos los jugadores deben tener un nombre válido.");

            if (players.GroupBy(p => p.Name).Any(g => g.Count() > 1))
                errors.Add("No puede haber jugadores con nombres duplicados.");

            if (players.Any(p => !p.IsAlive))
                errors.Add("Todos los jugadores deben estar vivos al inicio del juego.");

            return errors;
        }
    }
}