using MassTransit;
using PointingParty.Events;

namespace PointingParty;

public class DebugConsumer(ILogger<DebugConsumer> logger) : IConsumer<PlayerJoinedGame>, IConsumer<VoteCast>
{
    public Task Consume(ConsumeContext<PlayerJoinedGame> context)
    {
        logger.LogInformation(
            "Player {playerName} joined game {GameId}",
            context.Message.PlayerName,
            context.Message.GameId);
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<VoteCast> context)
    {
        logger.LogInformation(
            "Player {playerName} voted {score} in {GameId}",
            context.Message.PlayerName,
            context.Message.Score,
            context.Message.GameId);
        return Task.CompletedTask;
    }
}
