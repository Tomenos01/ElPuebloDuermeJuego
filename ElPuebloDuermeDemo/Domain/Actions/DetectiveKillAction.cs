using ElPuebloDuermeDemo.Models;
using ElPuebloDuermeDemo.Domain.Actions;

namespace ElPuebloDuermeDemo.Domain.Actions
{
    public class DetectiveKillAction : IRoleAction
    {
        public string ActionId { get; } = Guid.NewGuid().ToString();
        public string PlayerId { get; }
        public string? TargetId { get; }
        public ActionPriority Priority { get; } = ActionPriority.High;

        public DetectiveKillAction(string playerId, string targetId)
        {
            PlayerId = playerId;
            TargetId = targetId;
        }

        public bool CanExecute(IGameState gameState)
        {
            var player = gameState.GetPlayer(PlayerId);
            var target = gameState.GetPlayer(TargetId);

            if (player == null || target == null) return false;
            if (player.Role != Role.Detective) return false;
            if (!player.IsAlive || !target.IsAlive) return false;
            if (PlayerId == TargetId) return false;
            if (!player.CanUseAbility(gameState.CurrentRound)) return false;

            return true;
        }

        public ActionResult Execute(IGameState gameState)
        {
            if (!CanExecute(gameState)) return ActionResult.Invalid;

            var player = gameState.GetPlayer(PlayerId)!;
            var target = gameState.GetPlayer(TargetId)!;

            player.UseAbility(gameState.CurrentRound);

            if (target.Role == Role.Asesino)
            {
                // Éxito: mata al asesino
                target.IsAlive = false;
                gameState.AddDeadPlayer(TargetId!);
                gameState.AddGlobalMessage($"{target.Name} fue ejecutado por el detective y resultó ser el asesino.");
                return ActionResult.Success;
            }
            else
            {
                // Fallo: mata al civil y se mata a sí mismo
                target.IsAlive = false;
                player.IsAlive = false;
                gameState.AddDeadPlayer(TargetId!);
                gameState.AddDeadPlayer(PlayerId);
                gameState.AddGlobalMessage($"{target.Name} fue ejecutado por el detective, pero era inocente. El detective se suicidó por su error.");
                return ActionResult.Failed;
            }
        }

        public string GetDescription()
        {
            return "Intentar ejecutar al asesino (si falla, mata a un civil y se suicida)";
        }
    }
}