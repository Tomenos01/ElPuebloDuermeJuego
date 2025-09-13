using Microsoft.AspNetCore.Mvc;
using ElPuebloDuermeDemo.Services;
using ElPuebloDuermeDemo.Models;
using ActionResult = Microsoft.AspNetCore.Mvc.ActionResult;

namespace ElPuebloDuermeDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly IVoteService _voteService;

        public GameController(IGameService gameService, IVoteService voteService)
        {
            _gameService = gameService;
            _voteService = voteService;
        }

        [HttpPost("create")]
        public async Task<ActionResult<string>> CreateGame()
        {
            try
            {
                var gameId = await _gameService.CreateGame();
                return Ok(new { GameId = gameId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("{gameId}")]
        public ActionResult<object> GetGame(string gameId)
        {
            try
            {
                var gameState = _gameService.GetGameState(gameId);
                if (gameState == null)
                    return NotFound(new { Error = "Juego no encontrado" });

                // Devolver versión pública del estado
                var publicState = new
                {
                    gameState.GameId,
                    gameState.CurrentPhase,
                    gameState.CurrentRound,
                    gameState.IsStarted,
                    Players = gameState.Players.Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.IsAlive,
                        p.VotesReceived
                    }),
                    PlayerCount = gameState.Players.Count,
                    AlivePlayerCount = gameState.Players.Count(p => p.IsAlive),
                    Messages = gameState.GlobalMessages.TakeLast(10)
                };

                return Ok(publicState);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<object>>> GetAllGames()
        {
            try
            {
                var games = await _gameService.GetAllGames();
                var publicGames = games.Select(g => new
                {
                    g.GameId,
                    g.CurrentPhase,
                    g.IsStarted,
                    PlayerCount = g.Players.Count,
                    AlivePlayerCount = g.Players.Count(p => p.IsAlive)
                });

                return Ok(publicGames);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("{gameId}/start")]
        public async Task<ActionResult> StartGame(string gameId)
        {
            try
            {
                var success = await _gameService.StartGame(gameId);
                if (success)
                    return Ok(new { Message = "Juego iniciado correctamente" });
                else
                    return BadRequest(new { Error = "No se pudo iniciar el juego" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("{gameId}/advance")]
        public async Task<ActionResult> AdvancePhase(string gameId)
        {
            try
            {
                var success = await _gameService.AdvancePhase(gameId);
                if (success)
                    return Ok(new { Message = "Fase avanzada correctamente" });
                else
                    return BadRequest(new { Error = "No se pudo avanzar la fase" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("{gameId}/end")]
        public async Task<ActionResult> EndGame(string gameId)
        {
            try
            {
                var success = await _gameService.EndGame(gameId);
                if (success)
                    return Ok(new { Message = "Juego terminado" });
                else
                    return BadRequest(new { Error = "No se pudo terminar el juego" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("{gameId}/votes")]
        public async Task<ActionResult> GetVoteResults(string gameId)
        {
            try
            {
                var results = await _voteService.GetVoteResults(gameId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}