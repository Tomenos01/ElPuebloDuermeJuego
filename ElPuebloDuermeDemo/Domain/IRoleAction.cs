using ElPuebloDuermeDemo.Models;
using System.Security.Cryptography;

namespace ElPuebloDuermeDemo.Domain.Actions
{
    public interface IRoleAction
    {
        string ActionId { get; }
        string PlayerId { get; }
        string? TargetId { get; }
        ActionPriority Priority { get; }
        ActionResult Execute(IGameState gameState);
        bool CanExecute(IGameState gameState);
        string GetDescription();
    }

    public interface IGameState
    {
        List<Player> Players { get; }
        GamePhase CurrentPhase { get; set; }
        int CurrentRound { get; set; }
        List<string> DeadPlayersThisRound { get; }
        List<string> GlobalMessages { get; }
        Player? GetPlayer(string playerId);
        void AddGlobalMessage(string message);
        void AddDeadPlayer(string playerId);
        void ResetForNewRound();
        bool IsGameFinished();
        string? GetWinner();
    }
}