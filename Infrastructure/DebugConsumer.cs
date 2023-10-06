using MassTransit;
using PointingParty.Domain.Events;

namespace PointingParty.Infrastructure;

public class DebugConsumer(ILogger<DebugConsumer> logger) : IConsumer<PlayerJoinedGame>, IConsumer<PlayerLeftGame>,
    IConsumer<GameReset>, IConsumer<VoteCast>
{
    public Task Consume(ConsumeContext<GameReset> context)
    {
        logger.LogInformation(
            "Game {GameId}: game was reset",
            context.Message.GameId);
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<PlayerJoinedGame> context)
    {
        logger.LogInformation(
            "Game {GameId}: {playerName} joined",
            context.Message.GameId,
            context.Message.PlayerName);
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<PlayerLeftGame> context)
    {
        logger.LogInformation(
            "Game {GameId}: {playerName} left",
            context.Message.GameId,
            context.Message.PlayerName);
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<VoteCast> context)
    {
        logger.LogInformation(
            "Player {playerName} voted {score} in {GameId}",
            context.Message.PlayerName,
            context.Message.Vote,
            context.Message.GameId);
        return Task.CompletedTask;
    }
}
