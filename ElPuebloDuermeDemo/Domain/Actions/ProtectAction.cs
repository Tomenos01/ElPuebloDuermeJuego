using ElPuebloDuermeDemo.Models;
using ElPuebloDuermeDemo.Domain.Actions;

namespace ElPuebloDuermeDemo.Domain.Actions
{
    public class ProtectAction : IRoleAction
    {
        public string ActionId { get; } = Guid.NewGuid().ToString();
        public string PlayerId { get; }
        public string? TargetId { get; }
        public ActionPriority Priority { get; } = ActionPriority.High; // Prioridad alta para proteger antes del asesinato

        public ProtectAction(string playerId, string targetId)
        {
            PlayerId = playerId;
            TargetId = targetId;
        }

        public bool CanExecute(IGameState gameState)
        {
            var player = gameState.GetPlayer(PlayerId);
            var target = gameState.GetPlayer(TargetId);

            if (player == null || target == null) return false;
            if (player.Role != Role.Caballero) return false;
            if (!player.IsAlive || !target.IsAlive) return false;
            if (PlayerId == TargetId) return false; // No puede protegerse a sí mismo
            if (!player.CanUseAbility(gameState.CurrentRound)) return false;

            return true;
        }

        public ActionResult Execute(IGameState gameState)
        {
            if (!CanExecute(gameState)) return ActionResult.Invalid;

            var player = gameState.GetPlayer(PlayerId)!;
            var target = gameState.GetPlayer(TargetId)!;

            // Proteger al target
            target.IsProtected = true;
            player.UseAbility(gameState.CurrentRound);

            return ActionResult.Success;
        }

        public string GetDescription()
        {
            return "Proteger a un jugador de ser asesinado (una vez cada dos turnos)";
        }
    }
}