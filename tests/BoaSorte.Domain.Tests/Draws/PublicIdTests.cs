using BoaSorte.Domain.Draws;
using BoaSorte.Domain.Tests.TestDoubles;

namespace BoaSorte.Domain.Tests.Draws;

public class PublicIdTests
{
    [Fact]
    public void New_builds_twelve_characters_from_the_base62_alphabet()
    {
        var random = new SequenceRandomSource(0, 9, 10, 35, 36, 61, 0, 0, 0, 0, 0, 1);

        var id = PublicId.New(random);

        Assert.Equal("09azAZ000001", id.Value);
    }

    [Fact]
    public void New_with_real_randomness_is_always_valid()
    {
        var random = new SeededRandomSource(7);

        for (var i = 0; i < 100; i++)
        {
            Assert.True(PublicId.Parse(PublicId.New(random).Value).IsSuccess);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("aZ3k9QpL0xYt1")]
    [InlineData("aZ3k9QpL0xY-")]
    [InlineData("aZ3k9QpL0xYé")]
    public void Parse_rejects_malformed_values(string? raw)
    {
        Assert.True(PublicId.Parse(raw).IsFailure);
    }

    [Fact]
    public void Parse_accepts_a_well_formed_value()
    {
        Assert.Equal("aZ3k9QpL0xYt", PublicId.Parse("aZ3k9QpL0xYt").Value.Value);
    }
}
