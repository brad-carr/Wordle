namespace Wordle.UnitTests;

using FluentAssertions;
using Wordle.Feedback;

public sealed class FeedbackTests
{
    [Theory]
    [InlineData('a', false)]
    [InlineData('b', false)]
    [InlineData(FeedbackOption.Correct, true)]
    [InlineData('d', false)]
    [InlineData('e', false)]
    [InlineData('f', false)]
    [InlineData('g', false)]
    [InlineData('h', false)]
    [InlineData('i', false)]
    [InlineData('j', false)]
    [InlineData('k', false)]
    [InlineData('l', false)]
    [InlineData(FeedbackOption.Misplaced, true)]
    [InlineData(FeedbackOption.NoMoreOccurrences, true)]
    [InlineData('o', false)]
    [InlineData('p', false)]
    [InlineData('q', false)]
    [InlineData('r', false)]
    [InlineData('s', false)]
    [InlineData('t', false)]
    [InlineData('u', false)]
    [InlineData('v', false)]
    [InlineData('w', false)]
    [InlineData('x', false)]
    [InlineData('y', false)]
    [InlineData('z', false)]
    public void IsValid_ReturnsTrueForRecognisedCharacters(char test, bool isValid)
    {
        // Act
        var actual = FeedbackOption.IsValid(test);

        // Assert
        actual.Should().Be(isValid);
    }
}
