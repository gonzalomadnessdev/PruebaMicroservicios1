using BasketApi.Application;
using Common.Contracts;
using Common.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var eventBus = await RabbitMQEventBus.CreateAsync();

eventBus.SubscribeAsync<IntegrationEvent>(async (e) =>
{
    Console.WriteLine($"Este es mi mensajito, {e.Id} - {e.CreationDate}");
});

builder.Services.AddSingleton<IEventBus>(eventBus);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/health", () =>
{
    return Results.Ok();
})
.WithName("Health")
.WithOpenApi();



app.Run();