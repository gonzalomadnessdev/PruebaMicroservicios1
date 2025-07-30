namespace Common.Contracts
{
    public interface IEventBus: IAsyncDisposable
    {
        Task PublishAsync<T>(T @event) where T : class;
        Task SubscribeAsync<T>(Action<T> handler) where T : class;
    }
}
