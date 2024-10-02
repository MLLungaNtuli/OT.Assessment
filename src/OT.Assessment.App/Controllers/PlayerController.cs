using Microsoft.AspNetCore.Mvc;
using OT.Assessment.App.Models;
using OT.Assessment.App.Data;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;


namespace OT.Assessment.App.Controllers
{
  
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly IConnection _rabbitConnection;
        private readonly ApplicationDbContext _dbContext;

        public PlayerController(IConnection rabbitConnection, ApplicationDbContext dbContext)
        {
            _rabbitConnection = rabbitConnection;
            _dbContext = dbContext;
        }
        //POST api/player/casinowager
        [HttpPost("casinowager")]
        public async Task<IActionResult> SendCasinoWager([FromBody] CasinoWager wager)
        {
            if (wager == null)
            {
                return BadRequest("Wager data is null.");
            }

            // Ensure or check if Player exists
            var player = await _dbContext.Players.FindAsync(wager.AccountId);
            if (player == null)
            {
                player = new Player
                {
                    AccountId = wager.AccountId,
                    Username = wager.Username
                };
                _dbContext.Players.Add(player);
                await _dbContext.SaveChangesAsync();
            }

            // Here we Publish to RabbitMQ
            using var channel = _rabbitConnection.CreateModel();
            channel.QueueDeclare(queue: "casino_wager",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: new Dictionary<string, object>  //Implement a Dead Letter Queue (DLQ) for handling failed messages in RabbitMQ.
                                {
                                { "x-dead-letter-exchange", "casino_wager_dead" }
                                });

            var message = JsonSerializer.Serialize(wager);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: "",
                                 routingKey: "casino_wager",
                                 basicProperties: properties,
                                 body: body);

            return Accepted(new { message = "Wager received and queued." });
        }

        //GET api/player/{playerId}/wagers
        [HttpGet("{playerId}/casino")]
        public async Task<IActionResult> GetPlayerWagers(Guid playerId, int pageSize = 10, int page = 1)
        {
            //The use of pagination in the GetPlayerWagers endpoint ensures that the data is retrieved in an optimized way
            var total = await _dbContext.CasinoWagers.CountAsync(w => w.AccountId == playerId);
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var wagers = await _dbContext.CasinoWagers
                .Where(w => w.AccountId == playerId)
                .OrderByDescending(w => w.CreatedDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new
                {
                    w.WagerId,
                    game = w.GameName,
                    w.Provider,
                    amount = w.Amount,
                    createdDate = w.CreatedDateTime
                })
                .ToListAsync();

            var response = new
            {
                data = wagers,
                page,
                pageSize,
                total,
                totalPages
            };

            return Ok(response);
        }

        //GET api/player/topSpenders?count=10        
        [HttpGet("topSpenders")]
        public async Task<IActionResult> GetTopSpenders(int count = 10)
        {
        // Retrieves the top spenders data from the CasinoWagers table.
          // The query groups the wagers by AccountId and Username,
            // then calculates the total amount spent for each player.    
            var topSpenders = await _dbContext.CasinoWagers
                .GroupBy(w => new { w.AccountId, w.Username })
                .Select(g => new
                {
                    g.Key.AccountId,
                    g.Key.Username,
                    totalAmountSpend = g.Sum(w => w.Amount)
                })
                .OrderByDescending(s => s.totalAmountSpend)
                .Take(count)
                .ToListAsync();

            return Ok(topSpenders);
        }
    }
}
