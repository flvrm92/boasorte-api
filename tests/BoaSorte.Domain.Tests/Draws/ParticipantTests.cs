using BoaSorte.Domain.Draws;

namespace BoaSorte.Domain.Tests.Draws;

public class ParticipantTests
{
    [Theory]
    [InlineData("Ana", "Ana")]
    [InlineData("  Ana  ", "Ana")]
    [InlineData("Maria  Clara", "Maria  Clara")]
    public void Create_trims_the_value(string raw, string expected)
    {
        Assert.Equal(expected, Participant.Create(raw).Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_empty_values(string? raw)
    {
        var error = Assert.Single(Participant.Create(raw).Errors);
        Assert.Equal("participant.empty", error.Code);
    }

    [Fact]
    public void Create_accepts_exactly_the_maximum_length()
    {
        Assert.True(Participant.Create(new string('a', Participant.MaxLength)).IsSuccess);
    }

    [Fact]
    public void Create_rejects_values_longer_than_the_maximum_after_trimming()
    {
        var error = Assert.Single(Participant.Create(new string('a', Participant.MaxLength + 1)).Errors);
        Assert.Equal("participant.too_long", error.Code);
    }

    [Fact]
    public void Dedup_key_ignores_case_but_not_accents()
    {
        Assert.Equal(Participant.Create("ANA").Value.DedupKey, Participant.Create("ana").Value.DedupKey);
        Assert.NotEqual(Participant.Create("José").Value.DedupKey, Participant.Create("Jose").Value.DedupKey);
    }

    [Fact]
    public void Participants_with_same_value_are_equal()
    {
        Assert.Equal(Participant.Create("Ana").Value, Participant.Create(" Ana ").Value);
    }
}
