using System.Security.Cryptography;

namespace PointingParty.Domain;

public static class VoteEncryption
{
    private const int SaltLength = 12;

    public static Vote Encrypt(Vote vote)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltLength);

        var scoreBytes = BitConverter.GetBytes(vote.Score);
        var statusBytes = BitConverter.GetBytes((int)vote.Status);

        var encrypted = new byte[SaltLength + 12];
        salt.CopyTo(encrypted, 0);
        for (var i = 0; i < 8; i++)
            encrypted[SaltLength + i] = (byte)(scoreBytes[i] ^ salt[i % SaltLength]);
        for (var i = 0; i < 4; i++)
            encrypted[SaltLength + 8 + i] = (byte)(statusBytes[i] ^ salt[(8 + i) % SaltLength]);

        return new Vote
        {
            Score = 0,
            Status = VoteStatus.Pending,
            EncryptedPayload = Convert.ToBase64String(encrypted)
        };
    }

    public static Vote Decrypt(Vote vote)
    {
        if (vote.EncryptedPayload is null)
            return vote;

        var data = Convert.FromBase64String(vote.EncryptedPayload);
        var salt = data[..SaltLength];

        var scoreBytes = new byte[8];
        var statusBytes = new byte[4];
        for (var i = 0; i < 8; i++)
            scoreBytes[i] = (byte)(data[SaltLength + i] ^ salt[i % SaltLength]);
        for (var i = 0; i < 4; i++)
            statusBytes[i] = (byte)(data[SaltLength + 8 + i] ^ salt[(8 + i) % SaltLength]);

        var score = BitConverter.ToDouble(scoreBytes, 0);
        var status = (VoteStatus)BitConverter.ToInt32(statusBytes, 0);

        return status == VoteStatus.Scored ? new Vote(score) : new Vote(status);
    }
}
