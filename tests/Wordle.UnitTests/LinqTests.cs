namespace Wordle.UnitTests;

using FluentAssertions;
using Wordle;

public sealed class LinqTests
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
        var random = new Random(9183247);

        // Act
        for (var i = 0; i < iters; i++)
        {
            distribution[elements.RandomElement(random)!]++;
        }

        // Assert
        var expectedAllocation = 1.0 / distinctChars.Length;
        foreach (var s in elements)
        {
            (1.0 * distribution[s] / iters).Should().BeApproximately(expectedAllocation, 0.04);
        }
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3, 4, 5 }, 3, true, 2)]
    [InlineData(new[] { 1, 2, 3, 3, 5 }, 3, false, -1)]
    [InlineData(new[] { 1, 2, 4, 5 }, 3, false, -1)]
    [InlineData(new[] { 1, 1, 1 }, 1, false, -1)]
    [InlineData(new int[] { }, 1, false, -1)]
    public void ContainsOnce_ShouldReturnExpectedResult(
        int[] items,
        int value,
        bool expected,
        int expectedIndex
    )
    {
        // Act
        var result = items.ContainsOnce(value, out var index);

        // Assert
        result.Should().Be(expected);
        index.Should().Be(expectedIndex);
    }
}
