using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OT.Assessment.App.Models; // Use the CasinoWager from App.Models
using OT.Assessment.App.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace OT.Assessment.Consumer.Services
{
    public class ConsumerService : BackgroundService
    {
        private readonly ILogger<ConsumerService> _logger;
        private readonly IConnection _rabbitConnection;
        private readonly ApplicationDbContext _dbContext;
        private IModel _channel;

        public ConsumerService(ILogger<ConsumerService> logger, IConnection rabbitConnection, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _rabbitConnection = rabbitConnection;
            _dbContext = dbContext;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitConnection.CreateModel();
            _channel.QueueDeclare(queue: "casino_wager",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false); // Adjust prefetch count as needed
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var wager = JsonSerializer.Deserialize<CasinoWager>(message); // Use the CasinoWager from OT.Assessment.App.Models

                    if (wager != null)
                    {
                        // Check if wager already exists to prevent duplication
                        var exists = await _dbContext.CasinoWagers.AnyAsync(w => w.WagerId == wager.WagerId, stoppingToken);
                        if (!exists)
                        {
                            _dbContext.CasinoWagers.Add(wager);
                            await _dbContext.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"Stored Wager ID: {wager.WagerId}");
                        }
                        else
                        {
                            _logger.LogWarning($"Duplicate Wager ID: {wager.WagerId} ignored.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message.");
                    // Optionally, implement retry logic or move message to a dead-letter queue
                }

                // Acknowledge message
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: "casino_wager",
                                 autoAck: false, // Manual acknowledgment
                                 consumer: consumer);

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _channel?.Close();
            _channel?.Dispose();
            return base.StopAsync(cancellationToken);
        }
    }
}
