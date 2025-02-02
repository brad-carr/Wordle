using FluentAssertions;

namespace Wordle.UnitTests;

public class WordListReaderTests
{
    [Fact]
    public void SolutionWords_ShouldBeASubsetOf_GuessWords()
    {
        // Arrange
        var solutionWords = WordListReader.SolutionWordLiterals().ToHashSet();
        var guessWords = WordListReader.GuessWordLiterals().ToHashSet();
        
        // Act
        var missingWords = solutionWords.Except(guessWords).OrderBy(word => word).ToArray();
        
        // Assert
        missingWords.Should().BeEmpty($"expected these solution words to be in guess words: {string.Join(", ", missingWords)}");
    }

    [Theory]
    [InlineData("sooey")]
    public void ShouldDeriveOptimalStartWord(string expected) =>
        WordListReader.DeriveOptimalStartWord().ToString().Should().Be(expected);
}