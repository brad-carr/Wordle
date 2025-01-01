namespace Wordle.UnitTests;

using FluentAssertions;
using Wordle;

public class LinqTests
{
    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("abc")]
    [InlineData("abcd")]
    [InlineData("abcdefghijklmnopqrstuvwxyz")]
    public void RandomElement_ShouldMakeFairChoices(string distinctChars)
    {
        // Arrange
        var elements = distinctChars.Select(c => c.ToString()).ToArray();
        var distribution = elements.ToDictionary(s => s, _ => 0);
        var iters = distinctChars.Length * 2000;

        // Act
        for (var i = 0; i < iters; i++)
        {
            distribution[elements.RandomElement()!]++;
        }

        // Assert
        var expectedAllocation = 1.0 / distinctChars.Length;
        foreach (var s in elements)
        {
            (1.0 * distribution[s] / iters).Should().BeApproximately(expectedAllocation, 0.03);
        }
    }
}
