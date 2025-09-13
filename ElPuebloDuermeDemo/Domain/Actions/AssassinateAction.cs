using ElPuebloDuermeDemo.Models;
using ElPuebloDuermeDemo.Domain.Actions;

namespace ElPuebloDuermeDemo.Domain.Actions
{
    public class AssassinateAction : IRoleAction
    {
        public string ActionId { get; } = Guid.NewGuid().ToString();
        public string PlayerId { get; }
        public string? TargetId { get; }
        public ActionPriority Priority { get; } = ActionPriority.Medium;

        public AssassinateAction(string playerId, string targetId)
        {
            PlayerId = playerId;
            TargetId = targetId;
        }

        public bool CanExecute(IGameState gameState)
        {
            var player = gameState.GetPlayer(PlayerId);
            var target = gameState.GetPlayer(TargetId);

            if (player == null || target == null) return false;
            if (player.Role != Role.Asesino) return false;
            if (!player.IsAlive || !target.IsAlive) return false;
            if (PlayerId == TargetId) return false; // No puede matarse a sí mismo

            return true;
        }

        public ActionResult Execute(IGameState gameState)
        {
            if (!CanExecute(gameState)) return ActionResult.Invalid;

            var target = gameState.GetPlayer(TargetId)!;

            // Verificar si el target está protegido por el caballero
            if (target.IsProtected)
            {
                gameState.AddGlobalMessage($"Un jugador fue protegido durante la noche.");
                return ActionResult.Blocked;
            }

            // Asesinar al target
            target.IsAlive = false;
            gameState.AddDeadPlayer(TargetId!);
            gameState.AddGlobalMessage($"{target.Name} fue encontrado muerto esta mañana.");

            return ActionResult.Success;
        }

        public string GetDescription()
        {
            return "Asesinar a un jugador durante la noche";
        }
    }
}