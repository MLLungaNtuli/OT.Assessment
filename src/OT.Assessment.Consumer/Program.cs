using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using OT.Assessment.Consumer.Data;
using RabbitMQ.Client;
using OT.Assessment.Consumer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
    })
     .ConfigureServices((hostContext, services) =>
    {
        // Configure DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection")));

        // Configure RabbitMQ
        services.AddSingleton<IConnectionFactory>(sp =>
            new ConnectionFactory
            {
                HostName = hostContext.Configuration["RabbitMQ:HostName"],
                UserName = hostContext.Configuration["RabbitMQ:UserName"],
                Password = hostContext.Configuration["RabbitMQ:Password"]
            });

        services.AddSingleton<IConnection>(sp =>
        {
            var factory = sp.GetRequiredService<IConnectionFactory>();
            return factory.CreateConnection();
        });

        // Add the worker service
        services.AddHostedService<ConsumerService>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started {time:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

await host.RunAsync();

logger.LogInformation("Application ended {time:yyyy-MM-dd HH:mm:ss}", DateTime.Now);