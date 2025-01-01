namespace Wordle.UnitTests;

using FluentAssertions;
using Wordle;

public sealed class FeedbackTests
{
    [Theory]
    [InlineData('a', false)]
    [InlineData('b', false)]
    [InlineData(Feedback.Correct, true)]
    [InlineData('d', false)]
    [InlineData('e', false)]
    [InlineData('f', false)]
    [InlineData('g', false)]
    [InlineData('h', false)]
    [InlineData('i', false)]
    [InlineData('j', false)]
    [InlineData('k', false)]
    [InlineData('l', false)]
    [InlineData(Feedback.Misplaced, true)]
    [InlineData(Feedback.NoMoreOccurrences, true)]
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
        var actual = Feedback.IsValid(test);

        // Assert
        actual.Should().Be(isValid);
    }
}
