namespace Wordle.UnitTests;

using FluentAssertions;
using Moq;
using Wordle;

public sealed class SolverTests
{
    [Theory]
    [InlineData(
        "2024-12-27",
        "grain",
        3,
        new[] { "baker|2315|nmnnm", "cramp|88|nccnn", "grain|19|ccccc" }
    )]
    [InlineData(
        "2024-12-28",
        "decry",
        3,
        new[] { "whine|2315|nnnnm", "cagey|306|mnnmc", "decry|3|ccccc" }
    )]
    [InlineData(
        "2024-12-29",
        "mambo",
        4,
        new[] { "flint|2315|nnnnn", "hover|535|nmnnn", "buxom|11|mnnmm", "mambo|1|ccccc" }
    )]
    [InlineData("2024-12-30", "stare", 2, new[] { "rouse|2315|mnnmc", "stare|10|ccccc" })]
    [InlineData(
        "2024-12-31",
        "lemur",
        3,
        new[] { "dealt|2315|ncnmn", "helix|25|ncmnn", "lemur|13|ccccc" }
    )]
    [InlineData(
        "2025-01-01",
        "nerve",
        3,
        new[] { "saner|2315|nnmmm", "borne|15|nncmc", "nerve|1|ccccc" }
    )]
    public void Solve_ExplicitFeedback_ReturnsCorrectSolutionAndGuesses(
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

        foreach (var (guess, remaining, feedback) in feedbackData)
        {
            feedbackProviderMock
                .Setup(mock => mock.GetFeedback(guess, remaining))
                .Returns(feedback);
        }

        var solver = new Solver(console, feedbackProviderMock.Object);

        // Act
        var (solution, guesses) = solver.Solve(publicationDate);

        // Assert
        solution.Should().Be(expectedSolution);
        guesses.Count.Should().Be(expectedGuesses);
        feedbackProviderMock.VerifyAll();
    }

    [Theory]
    [InlineData(20241260, "mambo")]
    [InlineData(20241269, "stare")]
    public void Solve_DynamicFeedback_ProblematicSeeds_ShouldFindSolutionWithinSixAttempts(
        int problematicSeed,
        string solution
    )
    {
        // Arrange
        var console = Mock.Of<IConsole>();
        var feedbackProvider = new DynamicFeedbackProvider(solution);
        var solver = new Solver(console, feedbackProvider);
        var random = new Random(problematicSeed);

        // Act
        var (solverSolution, guesses) = solver.Solve(random);

        // Assert
        solverSolution
            .Should()
            .Be(solution, $"guesses were {string.Join($" {Unicode.RightArrow} ", guesses)}");
        guesses
            .Count.Should()
            .BeLessOrEqualTo(6, $"guesses were {string.Join($" {Unicode.RightArrow} ", guesses)}");
    }

    [Theory]
    [InlineData("2024-12-27", "grain")]
    [InlineData("2024-12-28", "decry")]
    [InlineData("2024-12-29", "mambo")]
    [InlineData("2024-12-30", "stare")]
    [InlineData("2024-12-31", "lemur")]
    [InlineData("2025-01-01", "nerve")]
    public void Solve_DynamicFeedback_MultipleSeeds_ShouldFindSolutionWithinSixAttempts(
        string publicationDateLiteral,
        string solution
    )
    {
        // Arrange
        const int NumConsecutiveSeedsToTest = 50;
        var publicationDate = DateOnly.Parse(publicationDateLiteral);
        var console = Mock.Of<IConsole>();
        var feedbackProvider = new DynamicFeedbackProvider(solution);
        var solver = new Solver(console, feedbackProvider);
        var initialSeed = Solver.GetSeed(publicationDate);
        var currentSeed = initialSeed;

        Enumerable
            .Range(0, NumConsecutiveSeedsToTest)
            .ToList()
            .ForEach(_ =>
            {
                // Arrange
                var random = new Random(currentSeed);

                // Act
                var (solverSolution, guesses) = solver.Solve(random);

                // Assert
                solverSolution
                    .Should()
                    .Be(
                        solution,
                        $"seed was {currentSeed}, guesses were {string.Join($" {Unicode.RightArrow} ", guesses)}"
                    );
                guesses
                    .Count.Should()
                    .BeLessOrEqualTo(
                        6,
                        $"seed was {currentSeed}, guesses were {string.Join($" {Unicode.RightArrow} ", guesses)}"
                    );

                currentSeed = currentSeed == int.MaxValue ? 0 : currentSeed + 1;
            });
    }

    [Fact]
    public void Solve_ReturnsNullWhenNoSolutionFound()
    {
        // Arrange
        var publicationDate = DateOnly.Parse("2024-12-31");

        var console = Mock.Of<IConsole>();
        var feedbackProviderMock = new Mock<IFeedbackProvider>(MockBehavior.Strict);
        feedbackProviderMock
            .Setup(mock => mock.GetFeedback(It.IsAny<string>(), It.IsAny<int>()))
            .Returns("nnnnn");

        var solver = new Solver(console, feedbackProviderMock.Object);

        // Act
        var (solution, guesses) = solver.Solve(publicationDate);

        // Assert
        solution.Should().BeNull();
        guesses.Count.Should().Be(3);
        feedbackProviderMock.VerifyAll();
    }

    [Fact]
    public void Solve_ReturnsNullWhenNullFeedbackProvided()
    {
        // Arrange
        var publicationDate = DateOnly.Parse("2024-12-31");

        var console = Mock.Of<IConsole>();
        var feedbackProviderMock = new Mock<IFeedbackProvider>(MockBehavior.Strict);
        feedbackProviderMock
            .Setup(mock => mock.GetFeedback(It.IsAny<string>(), It.IsAny<int>()))
            .Returns((string)null!);

        var solver = new Solver(console, feedbackProviderMock.Object);

        // Act
        var (solution, guesses) = solver.Solve(publicationDate);

        // Assert
        solution.Should().BeNull();
        guesses.Count.Should().Be(1);
        feedbackProviderMock.VerifyAll();
    }
}
