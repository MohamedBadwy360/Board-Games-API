
using BoardGamesAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using System.Linq.Dynamic.Core;
using BoardGamesAPI.Attributes;
using System.Reflection.Metadata;

namespace BoardGames.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardGamesController : ControllerBase
    {
        private readonly ILogger<BoardGamesController> _logger;
        private readonly ApplicationDbContext _context;
        public BoardGamesController(ILogger<BoardGamesController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetBoardGames")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<BoardGame[]>> Get([FromQuery] RequestDTO<BoardGameDTO> input)
        {
            _logger.LogInformation(CustomLogEvents.BoardGamesController_Get, "Get Method Starts.");

            var query = _context.BoardGames.AsQueryable();

            if (!string.IsNullOrEmpty(input.FilterQuery))
            {
                query = query.Where(b => b.Name.Contains(input.FilterQuery));
            }
            var recordCount = await query.CountAsync();

             query = query
                .OrderBy($"{input.SortColumn} {input.SortOrder}")
                .Skip(input.PageIndex * input.PageSize)
                .Take(input.PageSize);

            return new RestDTO<BoardGame[]>
            {
                Data = await query.ToArrayAsync(),
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                RecordCount = recordCount,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(Url.Action(null, "BoardGames", new{input.PageIndex, input.PageSize}, Request.Scheme)!,
                                "self",
                                "GET")
                }
            };
        }


        [HttpPut(Name = "UpdateBoardGame")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<BoardGame?>> Put(BoardGameDTO model)
        {
            var boardGame = await _context.BoardGames.Where(b => b.Id == model.Id).FirstOrDefaultAsync();

            if (boardGame is not null)
            {
                if (!string.IsNullOrEmpty(model.Name))
                {
                    boardGame.Name = model.Name;
                }
                if (model.Year.HasValue && model.Year.Value > 0)
                {
                    boardGame.Year = model.Year.Value;
                }
                boardGame.LastModifiedDate = DateTime.Now;
                _context.BoardGames.Update(boardGame);
                await _context.SaveChangesAsync();
            }

            return new RestDTO<BoardGame?>() {
                Data = boardGame,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(null, "BoardGames", model, Request.Scheme)!, "self", "POST") 
                }
            };
        }


        [HttpDelete(Name = "DeleteBoardGame")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<BoardGame?>> Delete(int id)
        {
            var boardGame = await _context.BoardGames.Where(b => b.Id == id).FirstOrDefaultAsync();

            if (boardGame is not null)
            {
                _context.BoardGames.Remove(boardGame);
                await _context.SaveChangesAsync();
            }

            return new RestDTO<BoardGame?>()
            {
                Data = boardGame,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                            Url.Action(
                                null,
                                "BoardGames",
                                id,
                                Request.Scheme)!,
                            "self",
                            "DELETE"),
                }
            };
        }
    }
}
