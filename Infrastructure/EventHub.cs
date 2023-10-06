using MassTransit;
using PointingParty.Domain.Events;

namespace PointingParty.Infrastructure;

public class EventHub : IConsumer<PlayerJoinedGame>, IConsumer<PlayerLeftGame>, IConsumer<GameReset>,
    IConsumer<VoteCast>, IConsumer<VotesShown>
{
    public Task Consume(ConsumeContext<GameReset> context)
    {
        OnEvent?.Invoke(context.Message);
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<PlayerJoinedGame> context)
    {
        OnEvent?.Invoke(context.Message);
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<PlayerLeftGame> context)
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
