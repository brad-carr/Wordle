namespace Wordle.UnitTests;

using FluentAssertions;
using Moq;
using Wordle;
using Wordle.Feedback;
using Wordle.Interaction;

[Collection("SolverCollection")]
public sealed class SolverTests
{
    private readonly SolverFixture _fixture;

    public SolverTests(SolverFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(20241260, "mambo")] // fixed in commit 386c6c442ba2f515b08c769c53d6c253ba1c0b37
    [InlineData(20241295, "mambo")] // fixed in commit 2134bc918dab9cc7c39e1bf81fe0c59bfe605d24
    [InlineData(20241269, "stare")] // fixed in commit 6e3740e631d36a23d2daa6e6865dbdb3adf3b4e3
    [InlineData(20241247, "stare")] // fixed in commit 680aa9c14d4678d20ed4a6467a653e1026c4685c
    [InlineData(20241434, "stare")] // fixed in commit c5bf081769ed5395ecb7a77dc0ad74e10836625e
    [InlineData(20250237, "nerve")] // fixed in commit 386c6c442ba2f515b08c769c53d6c253ba1c0b37
    [InlineData(20241916, "lemur")] // fixed in commit 6e3740e631d36a23d2daa6e6865dbdb3adf3b4e3
    [InlineData(20241413, "grain")] // fixed in commit 6e3740e631d36a23d2daa6e6865dbdb3adf3b4e3
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
        var (solverSolution, guesses, failureReason) = solver.Solve(random);

        // Assert
        solverSolution
            .Should()
            .Be(
                solution,
                $"guesses were {string.Join($" {Unicode.RightArrow} ", guesses)} and failure reason was {failureReason}"
            );
        guesses
            .Count.Should()
            .BeLessOrEqualTo(6, $"guesses were {string.Join($" {Unicode.RightArrow} ", guesses)}");
        failureReason.Should().BeNull();
    }

    [Theory(Skip = "Use this test to find problematic seeds for which the solver fails")]
    [InlineData("2024-12-27", "grain")]
    [InlineData("2024-12-28", "decry")]
    [InlineData("2024-12-29", "mambo")]
    [InlineData("2024-12-30", "stare")]
    [InlineData("2024-12-31", "lemur")]
    [InlineData("2025-01-01", "nerve")]
    [InlineData("2025-01-02", "chose")]
    [InlineData("2025-01-03", "cheap")]
    [InlineData("2025-01-04", "relax")]
    [InlineData("2025-01-05", "cyber")]
    public void Solve_DynamicFeedback_MultipleSeeds_ShouldFindSolutionWithinSixAttempts(
        string publicationDateLiteral,
        string solution
    )
    {
        RunScenario(publicationDateLiteral, solution, 5000);
    }

    [Theory]
    [InlineData("2024-12-27", "grain")]
    [InlineData("2024-12-28", "decry")]
    [InlineData("2024-12-29", "mambo")]
    [InlineData("2024-12-30", "stare")]
    [InlineData("2024-12-31", "lemur")]
    [InlineData("2025-01-01", "nerve")]
    [InlineData("2025-01-02", "chose")]
    [InlineData("2025-01-03", "cheap")]
    [InlineData("2025-01-04", "relax")]
    [InlineData("2025-01-05", "cyber")]
    public void Solve_DynamicFeedback_NaturalSeed_ShouldFindSolutionWithinSixAttempts(
        string publicationDateLiteral,
        string solution
    )
    {
        RunScenario(publicationDateLiteral, solution, 1);
    }

    private void RunScenario(
        string publicationDateLiteral,
        string solution,
        int numConsecutiveSeedsToTest
    )
    {
        // Arrange
        var publicationDate = DateOnly.Parse(publicationDateLiteral);
        var console = Mock.Of<IConsole>();
        var feedbackProvider = new DynamicFeedbackProvider(solution);
        var solver = new Solver(console, feedbackProvider, _fixture.WordList);
        var initialSeed = Solver.GetSeed(publicationDate);
        var currentSeed = initialSeed;

        Enumerable
            .Range(0, numConsecutiveSeedsToTest)
            .ToList()
            .ForEach(_ =>
            {
                // Arrange
                var random = new Random(currentSeed);

                // Act
                var (solverSolution, guesses, failureReason) = solver.Solve(random);

                // Assert
                solverSolution
                    .Should()
                    .Be(
                        solution,
                        $"seed was {currentSeed}, guesses were {string.Join($" {Unicode.RightArrow} ", guesses)}, failure reason was {failureReason}"
                    );
                guesses
                    .Count.Should()
                    .BeLessOrEqualTo(
                        6,
                        $"seed was {currentSeed}, guesses were {string.Join($" {Unicode.RightArrow} ", guesses)}, failure reason was {failureReason}"
                    );
                failureReason.Should().BeNull();
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
        var (solution, guesses, failureReason) = solver.Solve(publicationDate);

        // Assert
        solution.Should().BeNull();
        guesses.Count.Should().Be(Solver.MaxAttempts);
        feedbackProviderMock.VerifyAll();
        failureReason.Should().Be("maximum attempts reached without solution");
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
        var (solution, guesses, failureReason) = solver.Solve(publicationDate);

        // Assert
        solution.Should().BeNull();
        guesses.Count.Should().Be(1);
        feedbackProviderMock.VerifyAll();
        failureReason.Should().Be("failed to acquire feedback for guess");
    }
}

[CollectionDefinition("SolverCollection")]
public sealed class SolverCollection : ICollectionFixture<SolverFixture> { }

public sealed class SolverFixture
{
    public SolverFixture() => WordList = WordListReader.EnumerateLines().ToArray();

    public string[] WordList { get; }
}
