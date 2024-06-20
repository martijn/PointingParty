namespace PointingParty.Domain;

public enum VoteStatus
{
    Pending,
    Scored,
    Coffee,
    Question
}

public readonly struct Vote : IEquatable<Vote>, IEquatable<VoteStatus>, IEquatable<double>, IComparable<Vote>
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
            VoteStatus.Pending => "",
            _ => Score.ToString()
        };
    }

    public int CompareTo(Vote other)
    {
        if (Status == VoteStatus.Scored && other.Status != VoteStatus.Scored) return -1;

        if (Status != VoteStatus.Scored && other.Status == VoteStatus.Scored) return 1;

        return Score.CompareTo(other.Score);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vote vote && Equals(vote);
    }

    public bool Equals(Vote other)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return other.Score == Score && other.Status == Status;
    }

    public bool Equals(double score)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
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