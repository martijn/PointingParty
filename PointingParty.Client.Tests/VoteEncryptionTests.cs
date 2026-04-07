using PointingParty.Domain;

namespace PointingParty.Client.Tests;

public class VoteEncryptionTests
{
    [Fact]
    public void Roundtrip_ScoredVote()
    {
        var vote = new Vote(8);
        var encrypted = VoteEncryption.Encrypt(vote);
        var decrypted = VoteEncryption.Decrypt(encrypted);

        Assert.Equal(vote, decrypted);
    }

    [Theory]
    [InlineData(VoteStatus.Coffee)]
    [InlineData(VoteStatus.Question)]
    [InlineData(VoteStatus.Pending)]
    public void Roundtrip_StatusVote(VoteStatus status)
    {
        var vote = new Vote(status);
        var encrypted = VoteEncryption.Encrypt(vote);
        var decrypted = VoteEncryption.Decrypt(encrypted);

        Assert.Equal(vote, decrypted);
    }

    [Fact]
    public void Encrypted_Vote_Hides_Score_And_Status()
    {
        var vote = new Vote(13);
        var encrypted = VoteEncryption.Encrypt(vote);

        Assert.Equal(0, encrypted.Score);
        Assert.Equal(VoteStatus.Pending, encrypted.Status);
        Assert.NotNull(encrypted.EncryptedPayload);
    }

    [Fact]
    public void Decrypt_Without_Payload_Returns_Unchanged()
    {
        var vote = new Vote(5);
        var result = VoteEncryption.Decrypt(vote);

        Assert.Equal(vote, result);
        Assert.Null(result.EncryptedPayload);
    }

    [Fact]
    public void Same_Vote_Encrypted_Twice_Produces_Different_Payloads()
    {
        var vote = new Vote(8);
        var enc1 = VoteEncryption.Encrypt(vote);
        var enc2 = VoteEncryption.Encrypt(vote);

        Assert.NotEqual(enc1.EncryptedPayload, enc2.EncryptedPayload);
    }
}
