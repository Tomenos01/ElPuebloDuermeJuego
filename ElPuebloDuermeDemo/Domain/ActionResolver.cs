using ElPuebloDuermeDemo.Models;
using ElPuebloDuermeDemo.Domain.Actions;

namespace ElPuebloDuermeDemo.Domain
{
    public interface IActionResolver
    {
        Task<List<ActionResult>> ResolveNightActions(IGameState gameState);
    }

    public class ActionResolver : IActionResolver
    {
        public async Task<List<ActionResult>> ResolveNightActions(IGameState gameState)
        {
            var results = new List<ActionResult>();

            if (gameState is not GameState state) 
                return results;

            // Ordenar acciones por prioridad (1 = mayor prioridad)
            var sortedActions = state.PendingActions
                .OrderBy(a => (int)a.Priority)
                .ToList();

            foreach (var action in sortedActions)
            {
                try
                {
                    var result = action.Execute(gameState);
                    results.Add(result);
                    
                    await Task.Delay(50); // Pequeña pausa para simular procesamiento
                }
                catch (Exception ex)
                {
                    // Log error (en un caso real usarías ILogger)
                    results.Add(ActionResult.Failed);
                }
            }

            state.ClearActions();
            return results;
        }
    }
}