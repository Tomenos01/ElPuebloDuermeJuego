using ElPuebloDuermeDemo.Models;
using ElPuebloDuermeDemo.Domain.Actions;

namespace ElPuebloDuermeDemo.Domain.Actions
{
    public class HealAction : IRoleAction
    {
        public string ActionId { get; } = Guid.NewGuid().ToString();
        public string PlayerId { get; }
        public string? TargetId { get; }
        public ActionPriority Priority { get; } = ActionPriority.Lowest; // Prioridad baja para curar después de todo

        public HealAction(string playerId, string targetId)
        {
            PlayerId = playerId;
            TargetId = targetId;
        }

        public bool CanExecute(IGameState gameState)
        {
            var player = gameState.GetPlayer(PlayerId);
            var target = gameState.GetPlayer(TargetId);

            if (player == null || target == null) return false;
            if (player.Role != Role.Medico) return false;
            if (!player.IsAlive) return false;
            if (target.IsAlive) return false; // Solo puede curar muertos
            if (!player.CanUseAbility(gameState.CurrentRound)) return false;

            return true;
        }

        public ActionResult Execute(IGameState gameState)
        {
            if (!CanExecute(gameState)) return ActionResult.Invalid;

            var player = gameState.GetPlayer(PlayerId)!;
            var target = gameState.GetPlayer(TargetId)!;

            // Curar al target (revivirlo)
            target.IsAlive = true;
            player.UseAbility(gameState.CurrentRound);
            
            // Remover de la lista de muertos de esta ronda si está ahí
            gameState.DeadPlayersThisRound.Remove(TargetId!);
            gameState.AddGlobalMessage($"{target.Name} fue misteriosamente salvado de la muerte.");

            return ActionResult.Success;
        }

        public string GetDescription()
        {
            return "Curar (revivir) a una persona muerta (solo una vez por juego)";
        }
    }
}