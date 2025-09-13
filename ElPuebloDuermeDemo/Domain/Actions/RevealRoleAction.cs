using ElPuebloDuermeDemo.Models;
using ElPuebloDuermeDemo.Domain.Actions;

namespace ElPuebloDuermeDemo.Domain.Actions
{
    public class RevealRoleAction : IRoleAction
    {
        public string ActionId { get; } = Guid.NewGuid().ToString();
        public string PlayerId { get; }
        public string? TargetId { get; }
        public ActionPriority Priority { get; } = ActionPriority.Low;

        public RevealRoleAction(string playerId, string targetId)
        {
            PlayerId = playerId;
            TargetId = targetId;
        }

        public bool CanExecute(IGameState gameState)
        {
            var player = gameState.GetPlayer(PlayerId);
            var target = gameState.GetPlayer(TargetId);

            if (player == null || target == null) return false;
            if (player.Role != Role.Clerigo) return false;
            if (!player.IsAlive) return false;
            if (!player.CanUseAbility(gameState.CurrentRound)) return false;

            return true;
        }

        public ActionResult Execute(IGameState gameState)
        {
            if (!CanExecute(gameState)) return ActionResult.Invalid;

            var player = gameState.GetPlayer(PlayerId)!;
            var target = gameState.GetPlayer(TargetId)!;

            // Revelar el rol al clérigo (esto se manejará en el frontend/SignalR)
            player.UseAbility(gameState.CurrentRound);
            
            // El rol revelado se enviará solo al clérigo a través de SignalR
            gameState.AddGlobalMessage($"El clérigo ha usado su habilidad de visión.");

            return ActionResult.Success;
        }

        public string GetDescription()
        {
            return "Revelar el rol de un jugador vivo o muerto (solo una vez por juego)";
        }
    }
}