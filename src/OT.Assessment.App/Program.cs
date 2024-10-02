using Microsoft.EntityFrameworkCore;
using OT.Assessment.App.Data;
using RabbitMQ.Client;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure RabbitMQ
builder.Services.AddSingleton<IConnectionFactory>(sp =>
    new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:HostName"] ?? throw new ArgumentNullException("RabbitMQ HostName is missing"),
        UserName = builder.Configuration["RabbitMQ:UserName"] ?? throw new ArgumentNullException("RabbitMQ UserName is missing"),
        Password = builder.Configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ Password is missing")
    });

builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = sp.GetRequiredService<IConnectionFactory>();
    return factory.CreateConnection();
});

builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OT.Assessment.App API", Version = "v1" });
});

var app = builder.Build();

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log the error and handle migration failure
        Console.WriteLine($"Migration failed: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OT.Assessment.App API V1");
    });
}
else
{
    // Consider disabling Swagger in production
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
