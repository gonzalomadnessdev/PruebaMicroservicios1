using Common.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Common.Infrastructure
{
    public class RabbitMQEventBus : IEventBus
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly string _exchangeName = "event_bus";

        private RabbitMQEventBus(IConnection connection, IChannel channel)
        {
            _connection = connection;
            _channel = channel;
        }

        public static async Task<RabbitMQEventBus> CreateAsync()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/",
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(exchange: "event_bus", type: ExchangeType.Fanout);

            return new RabbitMQEventBus(connection, channel);
        }

        public async Task PublishAsync<T>(T @event) where T : class
        {
            var json = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties
            {
                ContentType = "application/json",
                Persistent = true
            };

            await _channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: string.Empty,
                mandatory: false,
                basicProperties: props,
                body: body
            );
        }

        public async Task SubscribeAsync<T>(Action<T> handler) where T : class
        {
            var q = await _channel.QueueDeclareAsync();
            await _channel.QueueBindAsync(queue: q.QueueName, exchange: _exchangeName, routingKey: string.Empty);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var ev = JsonSerializer.Deserialize<T>(json);
                if (ev != null)
                    await Task.Run(() => handler(ev));
            };

            await _channel.BasicConsumeAsync(queue: q.QueueName, autoAck: true, consumer: consumer);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel.IsOpen)
                await _channel.CloseAsync();
            if (_connection.IsOpen)
                await _connection.CloseAsync();
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}
