using ElPuebloDuermeDemo.Models;
using ElPuebloDuermeDemo.Domain.Actions;

namespace ElPuebloDuermeDemo.Domain
{
    public interface IPhaseManager
    {
        Task<bool> AdvancePhase(IGameState gameState);
        bool CanAdvancePhase(IGameState gameState);
        TimeSpan GetPhaseTimeLimit(GamePhase phase);
    }

    public class PhaseManager : IPhaseManager
    {
        private readonly IActionResolver _actionResolver;

        public PhaseManager(IActionResolver actionResolver)
        {
            _actionResolver = actionResolver;
        }

        public async Task<bool> AdvancePhase(IGameState gameState)
        {
            if (!CanAdvancePhase(gameState)) return false;

            switch (gameState.CurrentPhase)
            {
                case GamePhase.Preparacion:
                    gameState.CurrentPhase = GamePhase.Noche;
                    gameState.AddGlobalMessage("La noche ha comenzado. Los jugadores con habilidades especiales pueden actuar.");
                    break;

                case GamePhase.Noche:
                    // Resolver todas las acciones nocturnas
                    await _actionResolver.ResolveNightActions(gameState);
                    gameState.CurrentPhase = GamePhase.Dia;
                    gameState.AddGlobalMessage("Amanece en el pueblo. Es hora de discutir lo que pasó durante la noche.");
                    break;

                case GamePhase.Dia:
                    gameState.CurrentPhase = GamePhase.Acusacion;
                    gameState.AddGlobalMessage("Es hora de votar. El chat se ha cerrado y comienza la votación anónima.");
                    break;

                case GamePhase.Acusacion:
                    // Procesar votación
                    ProcessVotingResults(gameState);
                    
                    if (gameState.IsGameFinished())
                    {
                        gameState.CurrentPhase = GamePhase.Finalizado;
                        var winner = gameState.GetWinner();
                        gameState.AddGlobalMessage($"¡El juego ha terminado! Los {winner} han ganado.");
                    }
                    else
                    {
                        // Continuar el ciclo
                        gameState.CurrentRound++;
                        gameState.ResetForNewRound();
                        gameState.CurrentPhase = GamePhase.Noche;
                        gameState.AddGlobalMessage($"Comienza la ronda {gameState.CurrentRound}. La noche vuelve a caer sobre el pueblo.");
                    }
                    break;

                case GamePhase.Finalizado:
                    // El juego ya terminó
                    return false;
            }

            return true;
        }

        public bool CanAdvancePhase(IGameState gameState)
        {
            return gameState.CurrentPhase != GamePhase.Finalizado;
        }

        public TimeSpan GetPhaseTimeLimit(GamePhase phase)
        {
            return phase switch
            {
                GamePhase.Preparacion => TimeSpan.FromMinutes(2),
                GamePhase.Noche => TimeSpan.FromMinutes(3),
                GamePhase.Dia => TimeSpan.FromMinutes(5),
                GamePhase.Acusacion => TimeSpan.FromMinutes(3),
                GamePhase.Finalizado => TimeSpan.Zero,
                _ => TimeSpan.FromMinutes(3)
            };
        }

        private void ProcessVotingResults(IGameState gameState)
        {
            if (gameState is not GameState state) return;

            var mostVoted = state.GetMostVotedPlayer();
            
            if (mostVoted != null)
            {
                mostVoted.IsAlive = false;
                state.AddDeadPlayer(mostVoted.Id);
                
                if (mostVoted.Role == Role.Bufon)
                {
                    // El bufón gana si es votado
                    state.AddGlobalMessage($"{mostVoted.Name} fue eliminado por votación y resultó ser el Bufón. ¡El Bufón ha ganado el juego!");
                    // Marcar que el bufón ganó (esto se manejará en el GameService)
                }
                else
                {
                    state.AddGlobalMessage($"{mostVoted.Name} fue eliminado por votación del pueblo.");
                }
            }
            else
            {
                state.AddGlobalMessage("Hubo empate en la votación. Nadie fue eliminado.");
            }

            state.ClearVotes();
        }
    }
}