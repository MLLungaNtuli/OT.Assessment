using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using OT.Assessment.App.Data;

namespace OT.Assessment.App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IConnection _rabbitConnection;
        private readonly ApplicationDbContext _dbContext;

        public HealthController(IConnection rabbitConnection, ApplicationDbContext dbContext)
        {
            _rabbitConnection = rabbitConnection;
            _dbContext = dbContext;
        }

        // GET api/health
        [HttpGet]
        public async Task<IActionResult> CheckHealth()
        {
            // Check RabbitMQ connection
            var rabbitMqHealthy = _rabbitConnection.IsOpen;
            if (!rabbitMqHealthy)
            {
                return StatusCode(500, new { status = "RabbitMQ connection failed" });
            }

            // Check Database connection
            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return StatusCode(500, new { status = "Database connection failed" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Database connection error", details = ex.Message });
            }

            return Ok(new { status = "Healthy", RabbitMQ = "Connected", Database = "Connected" });
        }
    }
}
