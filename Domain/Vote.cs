namespace PointingParty.Domain;

public enum VoteStatus
{
    Pending,
    Scored,
    Coffee,
    Question
}

public readonly struct Vote : IEquatable<Vote>, IEquatable<VoteStatus>, IEquatable<double>
{
    public double Score { get; init; }
    public VoteStatus Status { get; init; }

    public Vote()
    {
        Status = VoteStatus.Pending;
    }

    public Vote(double score)
    {
        Score = score;
        Status = VoteStatus.Scored;
    }

    public Vote(VoteStatus status)
    {
        Status = status;
    }

    public override string ToString()
    {
        return Status switch
        {
            VoteStatus.Coffee => "☕️",
            VoteStatus.Question => "❓",
            VoteStatus.Pending => "-",
            _ => Score.ToString()
        };
    }

    public override bool Equals(object? obj)
    {
        return obj is Vote && Equals(obj);
    }

    public bool Equals(Vote other)
    {
        return other.Score == Score && other.Status == Status;
    }

    public bool Equals(double score)
    {
        return Status == VoteStatus.Scored && Score == score;
    }

    public bool Equals(VoteStatus status)
    {
        return Status == status;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Score, Status);
    }

    public static bool operator ==(Vote left, Vote right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vote left, Vote right)
    {
        return !(left == right);
    }

    public static implicit operator Vote(double score)
    {
        return new Vote(score);
    }

    public static implicit operator Vote(VoteStatus status)
    {
        return new Vote(status);
    }
}
