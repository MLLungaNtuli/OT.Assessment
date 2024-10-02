using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using OT.Assessment.Consumer.Services;
using OT.Assessment.App.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
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
                HostName = hostContext.Configuration["RabbitMQ:HostName"] 
                    ?? throw new ArgumentNullException("RabbitMQ HostName is missing"),
                UserName = hostContext.Configuration["RabbitMQ:UserName"] 
                    ?? throw new ArgumentNullException("RabbitMQ UserName is missing"),
                Password = hostContext.Configuration["RabbitMQ:Password"] 
                    ?? throw new ArgumentNullException("RabbitMQ Password is missing")
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
    .Build(); 

// Run the host
await host.RunAsync();
