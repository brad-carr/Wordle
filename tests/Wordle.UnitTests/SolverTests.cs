namespace Wordle.UnitTests;

using FluentAssertions;
using Moq;
using Wordle;

public class SolverTests
{
    [Theory]
    [InlineData(
        "2024-12-28", "decry", 3, new[] { "whine|2315|nnnnm", "cagey|306|mnnmc", "decry|3|ccccc" }
    )]
    [InlineData(
        "2024-12-29",
        "mambo",
        4,
        new[] { "flint|2315|nnnnn", "hover|535|nmnnn", "gumbo|11|nnccc", "mambo|1|ccccc" }
    )]
    [InlineData("2024-12-30", "stare", 2, new[] { "rouse|2315|mnnmc", "stare|10|ccccc" })]
    public void Solver_ReturnsCorrectSolutionAndGuesses(
        string publicationDateString,
        string expectedSolution,
        int expectedGuesses,
        string[] feedbackDataArray
    )
    {
        // Arrange
        var publicationDate = DateOnly.Parse(publicationDateString);
        var feedbackData = feedbackDataArray
            .Select(entry =>
            {
                var parts = entry.Split('|');
                return (word: parts[0], remaining: int.Parse(parts[1]), feedback: parts[2]);
            })
            .ToList();

        var console = Mock.Of<IConsole>();
        var feedbackProviderMock = new Mock<IFeedbackProvider>(MockBehavior.Strict);

        foreach (var (word, remaining, feedback) in feedbackData)
        {
            feedbackProviderMock.Setup(mock => mock.GetFeedback(word, remaining)).Returns(feedback);
        }

        var solver = new Solver(console, feedbackProviderMock.Object);

        // Act
        var (solution, numGuesses) = solver.Solve(publicationDate);

        // Assert
        solution.Should().Be(expectedSolution);
        numGuesses.Should().Be(expectedGuesses);
        feedbackProviderMock.VerifyAll();
    }
}
