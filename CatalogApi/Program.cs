using CatalogApi.Application;
using Common.Contracts;
using Common.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var eventBus = await RabbitMQEventBus.CreateAsync();
builder.Services.AddSingleton<IEventBus>(eventBus);

var app = builder.Build();

// Configure the HTTP request pipeline.

// Enable Swagger middleware in dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

object value = app.MapGet("/api/items", async (IEventBus eventBus) =>
{
    var items = new List<Item>
    {
        new Item { Id = Guid.NewGuid(), Name = "Item1", Description = "Description1", Price = 10.99M },
        new Item { Id = Guid.NewGuid(), Name = "Item2", Description = "Description2", Price = 20.99M },
        new Item { Id = Guid.NewGuid(), Name = "Item3", Description = "Description3", Price = 30.99M }
    };

    await eventBus.PublishAsync(new IntegrationEvent());
    return items;
});

app.Run();

public class Item
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}
