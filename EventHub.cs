using MassTransit;
using PointingParty.Events;

namespace PointingParty;

public class EventHub : IConsumer<PlayerJoinedGame>, IConsumer<GameReset>, IConsumer<VoteCast>, IConsumer<VotesShown>
{
    public Task Consume(ConsumeContext<PlayerJoinedGame> context)
    {
        OnEvent?.Invoke(context.Message);
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<GameReset> context)
    {
        OnEvent?.Invoke(context.Message);
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<VoteCast> context)
    {
        OnEvent?.Invoke(context.Message);
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<VotesShown> context)
    {
        OnEvent?.Invoke(context.Message);
        return Task.CompletedTask;
    }

    public event Action<IGameEvent>? OnEvent;
}
