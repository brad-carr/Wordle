namespace Wordle.UnitTests;

using FluentAssertions;
using Humanizer;
using Moq;
using Wordle;
using Feedback;
using Interaction;
using Xunit;
using Xunit.Abstractions;

[Collection("SolverCollection")]
public sealed class SolverTests
{
    private readonly SolverFixture _fixture;
    private readonly ITestOutputHelper _testHelper;

    public SolverTests(SolverFixture fixture, ITestOutputHelper testHelper)
    {
        _fixture = fixture;
        _testHelper = testHelper;
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
    [InlineData(1, "wight")]
    public void Solve_DynamicFeedback_ProblematicSeeds_ShouldFindSolutionWithinSixAttempts(
        int problematicSeed,
        string solutionLiteral
    )
    {
        // Arrange
        var console = Mock.Of<IConsole>();
        var solution = Word.Create(solutionLiteral);
        var feedbackProvider = new DynamicFeedbackProvider(solution);
        var guesser = _fixture.Guesser;
        var solver = new Solver(console, guesser, feedbackProvider);
        var random = new Random(problematicSeed);

        // Act
        var (solverSolution, guesses, failureReason) = solver.Solve(random);
        
        // Assert
        solverSolution.ToString()
            .Should()
            .Be(
                solution.ToString(),
                $"guesses were {string.Join($" {Unicode.RightArrow} ", guesses.Select(guess => $"'{guess.ToString()}'"))} with failure reason: {failureReason}"
            );
        guesses
            .Count.Should()
            .BeLessOrEqualTo(Solver.DefaultMaxAttempts, $"guesses were {string.Join($" {Unicode.RightArrow} ", guesses)}");
        failureReason.Should().BeNull();
        _testHelper.WriteLine($"Solved '{solution.ToString()}' in {guesses.Count} attempts. Guesses: {string.Join($" {Unicode.RightArrow} ", guesses)}");
    }

    [Theory(Skip = "Use this test to find problematic seeds for which the solver fails")]
    [MemberData(nameof(NytDailySolutions))]
    public void Solve_DynamicFeedback_MultipleSeeds_ShouldFindSolutionWithinSixAttempts(
        string publicationDateLiteral,
        string solution
    )
    {
        Thread.CurrentThread.Priority = ThreadPriority.Highest;
        RunScenario(publicationDateLiteral, solution, 5000);
    }

    [Theory]
    [MemberData(nameof(NytDailySolutions))]
    public void Solve_DynamicFeedback_NaturalSeed_ShouldFindSolutionWithinSixAttempts(
        string publicationDateLiteral,
        string solution
    )
    {
        RunScenario(publicationDateLiteral, solution, 1);
    }

    [Fact]
    public void Solve_DynamicFeedback_AllSolutionsTested()
    {
        Thread.CurrentThread.Priority = ThreadPriority.Highest;

        const int seed = 1;
        var solver = _fixture.Solver;
        var (successCount, failCount) = (0, 0);
        
        _fixture
            .SolutionWordList
            .Select(literal =>
            {
                var solution =  Word.Create(literal);
                var random = new Random(seed);
                _fixture.FeedbackProvider.Solution = solution;
                return (solution, result: solver.Solve(random, 1000));
            })
            .OrderBy(x => x.result.guesses.Count)
            .ToList()
            .ForEach(x =>
            {
                var delimited = string.Join(", ", x.result.guesses.Select(g => $"'{g.ToString()}'"));
                x.result.solution.Should().NotBeNull(
                    $"expected solution for '{x.solution.ToString()}' but failed after {"guess".ToQuantity(x.result.guesses.Count)}: {delimited} with reason: {x.result.failureReason}");
                
                if (x.result.guesses.Count > Solver.DefaultMaxAttempts)
                {
                    failCount++;
                    _testHelper.WriteLine($"Failed for '{x.solution.ToString()}' after {"guess".ToQuantity(x.result.guesses.Count)}: {delimited}");
                }
                else
                {
                    successCount++;
                    _testHelper.WriteLine($"Solved '{x.solution.ToString()}' in {x.result.guesses.Count} attempts ; guesses: {delimited}");
                }
            });
        
        _testHelper.WriteLine($"Completed. success: {successCount}; fail: {failCount}");
    }

    private void RunScenario(
        string publicationDateLiteral,
        string solutionLiteral,
        int numConsecutiveSeedsToTest
    )
    {
        // Arrange
        var publicationDate = DateOnly.Parse(publicationDateLiteral);
        var solution = Word.Create(solutionLiteral);
        var initialSeed = Solver.GetSeed(publicationDate);
        var solver = _fixture.Solver;
        _fixture.FeedbackProvider.Solution = solution;
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
                        Solver.DefaultMaxAttempts,
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
            .Setup(mock => mock.GetFeedback(It.IsAny<Word>(), It.IsAny<int>()))
            .Returns("nnnnn");

        var solver = new Solver(console, _fixture.Guesser, feedbackProviderMock.Object);

        // Act
        var (solution, guesses, failureReason) = solver.Solve(publicationDate);

        // Assert
        solution.Should().BeNull();
        guesses.Count.Should().BeGreaterThan(0);
        guesses.Count.Should().BeLessOrEqualTo(Solver.DefaultMaxAttempts);
        feedbackProviderMock.VerifyAll();
        failureReason.Should().Be("algorithm failure, no remaining words available");
    }

    [Fact]
    public void Solve_ReturnsNullWhenNullFeedbackProvided()
    {
        // Arrange
        var publicationDate = DateOnly.Parse("2024-12-31");

        var console = Mock.Of<IConsole>();
        var feedbackProviderMock = new Mock<IFeedbackProvider>(MockBehavior.Strict);
        feedbackProviderMock
            .Setup(mock => mock.GetFeedback(It.IsAny<Word>(), It.IsAny<int>()))
            .Returns((string)null!);

        var solver = new Solver(console, _fixture.Guesser, feedbackProviderMock.Object);

        // Act
        var (solution, guesses, failureReason) = solver.Solve(publicationDate);

        // Assert
        solution.Should().BeNull();
        guesses.Count.Should().Be(1);
        feedbackProviderMock.VerifyAll();
        failureReason.Should().Be("failed to acquire feedback for guess");
    }
    
    public static TheoryData<string, string> NytDailySolutions => new() {
        { "2024-12-27", "grain" },
        { "2024-12-28", "decry" },
        { "2024-12-29", "mambo" },
        { "2024-12-30", "stare" },
        { "2024-12-31", "lemur" },
        { "2025-01-01", "nerve" },
        { "2025-01-02", "chose" },
        { "2025-01-03", "cheap" },
        { "2025-01-04", "relax" },
        { "2025-01-05", "cyber" },
        { "2025-01-06", "sprig" },
        { "2025-01-07", "atlas" },
        { "2025-01-08", "draft" },
        { "2025-01-09", "wafer" },
        { "2025-01-10", "crawl" },
        { "2025-01-11", "dingy" },
        { "2025-01-12", "total" },
        { "2025-01-13", "cloak" },
        { "2025-01-14", "fancy" },
        { "2025-01-15", "knack" },
        { "2025-01-16", "flint" },
        { "2025-01-17", "prose" },
        { "2025-01-18", "silly" },
    };
}

[CollectionDefinition("SolverCollection")]
public sealed class SolverCollection : ICollectionFixture<SolverFixture> { }

public sealed class SolverFixture
{
    public SolverFixture()
    {
        SolutionWordList = WordListReader.EnumerateSolutionWords().ToArray();
        var console = Mock.Of<IConsole>();
        FeedbackProvider = new DynamicFeedbackProvider();
        Guesser = new Guesser();
        Solver = new Solver(console, Guesser, FeedbackProvider, SolutionWordList);
    }

    public Guesser Guesser { get; }

    public DynamicFeedbackProvider FeedbackProvider { get; }

    public Solver Solver { get; }
    public string[] SolutionWordList { get; }
}
