using FluentAssertions;

namespace Wordle.UnitTests;

public sealed class WordTests
{
    [Theory]
    [InlineData("abcde")]
    [InlineData(" bcd ")]
    [InlineData("     ")]
    public void Indexer_ReturnsExpectedValue(string literal)
    {
        var zipped = Word.Create(literal).Zip(literal).ToArray();
        zipped.Length.Should().Be(literal.Length);
        foreach (var x in zipped)
        {
            x.First.Should().Be(ToByte(x.Second));
        }
    }

    [Theory]
    [InlineData("abcde")]
    [InlineData(" bcd ")]
    [InlineData("     ")]
    public void ToString_ReturnsCorrectRepresentation(string literal)
    {
        Word.Create(literal).ToString().Should().Be(literal);
    }

    [Theory]
    [InlineData("abcde", 'a', true)]
    [InlineData("abcde", 'b', true)]
    [InlineData("abcde", 'c', true)]
    [InlineData("abcde", 'd', true)]
    [InlineData("abcde", 'e', true)]
    [InlineData("abcde", 'f', false)]
    [InlineData("abcde", ' ', false)]
    public void Contains_ReturnsExpectedResult(string literal, char c, bool expected)
    {
        Word.Create(literal).Contains(ToByte(c)).Should().Be(expected);
    }

    [Theory]
    [InlineData("abcde", 'z', 0, "zbcde")]
    [InlineData("abcde", 'z', 1, "azcde")]
    [InlineData("abcde", 'z', 2, "abzde")]
    [InlineData("abcde", 'z', 3, "abcze")]
    [InlineData("abcde", 'z', 4, "abcdz")]
    [InlineData("abcde", ' ', 0, " bcde")]
    [InlineData("abcde", ' ', 1, "a cde")]
    [InlineData("abcde", ' ', 2, "ab de")]
    [InlineData("abcde", ' ', 3, "abc e")]
    [InlineData("abcde", ' ', 4, "abcd ")]
    public void SetCharAtPos_ReturnsExpectedResult(string literal, char c, int pos, string expected)
    {
        Word.Create(literal).SetCharAtPos(ToByte(c), pos).ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData("abcde", 'a', true, 0)]
    [InlineData("abcde", 'b', true, 1)]
    [InlineData("abcde", 'c', true, 2)]
    [InlineData("abcde", 'd', true, 3)]
    [InlineData("abcde", 'e', true, 4)]
    [InlineData("abcde", 'f', false, -1)]
    [InlineData("abcde", ' ', false, -1)]
    [InlineData("aacde", 'a', false, -1)]
    [InlineData("aacde", 'c', true, 2)]
    [InlineData("aacde", 'd', true, 3)]
    [InlineData("aacde", 'e', true, 4)]
    [InlineData("aacee", 'e', false, -1)]
    [InlineData("aacee", 'a', false, -1)]
    [InlineData("aacee", 'c', true, 2)]
    public void ContainsOnce_ReturnsExpectedResult(string literal, char c, bool expected, int expectedPosition)
    {
        Word.Create(literal).ContainsOnce(ToByte(c), out var pos).Should().Be(expected);
        if (expected)
        {
            pos.Should().Be(expectedPosition);    
        }
    }

    private static byte ToByte(char c) => c == ' ' ? (byte)0 : (byte)(c - 'a' + 1);
}