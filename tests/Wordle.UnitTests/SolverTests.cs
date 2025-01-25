using Wordle.Core;

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
public sealed class SolverTests(SolverFixture fixture, ITestOutputHelper testHelper)
{
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
    [InlineData(1, "bacon")]
    [InlineData(1, "store")]
    [InlineData(1, "vocal")]
    [InlineData(1, "baker")]
    [InlineData(1, "wafer")]
    [InlineData(1, "couch")]
    [InlineData(1, "crave")]
    [InlineData(1, "craze")]
    [InlineData(1, "fanny")]
    [InlineData(1, "nanny")]
    [InlineData(1, "paper")]
    [InlineData(1, "parer")]
    [InlineData(1, "rarer")]
    [InlineData(1, "sassy")]
    [InlineData(1, "savvy")]
    [InlineData(1, "vouch")]
    [InlineData(1, "diver")]
    [InlineData(1, "rider")]

    public void Solve_DynamicFeedback_ProblematicSeeds_ShouldFindSolutionWithinSixAttempts(
        int problematicSeed,
        string solutionLiteral
    )
    {
        // Arrange
        var console = Mock.Of<IConsole>();
        var solution = Word.Create(solutionLiteral);
        var feedbackProvider = new DynamicFeedbackProvider(solution);
        var guesser = fixture.Guesser;
        var solver = new Solver(console, guesser, feedbackProvider);
        var random = new Random(problematicSeed);

        // Act
        var (solverSolution, guesses, failureReason) = solver.Solve(random);
        
        // Assert
        solverSolution.ToString()
            .Should()
            .Be(
                solution.ToString(),
                $"guesses were {RenderGuesses(guesses)} with failure reason: {failureReason}"
            );
        guesses
            .Count.Should()
            .BeLessOrEqualTo(Solver.DefaultMaxAttempts, $"guesses were {RenderGuesses(guesses)}");
        failureReason.Should().BeNull();
        testHelper.WriteLine($"Solved '{solution.ToString()}' in {guesses.Count} attempts. Guesses: {RenderGuesses(guesses)}");
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
        var solverStats = SolveAllSolutionWords(fixture.Solver, fixture.FeedbackProvider, () => new Random(seed));

        solverStats.GuessesBySolution.ForEach(x =>
            {
                var delimited = RenderGuesses(x.Guesses);
                x.Solved.Should().BeTrue(
                    $"expected solution for '{x.Solution.ToString()}' but failed after {"guess".ToQuantity(x.Guesses.Count)}: {delimited} with reason: {x.FailureReason}");

                testHelper.WriteLine(x.Guesses.Count > Solver.DefaultMaxAttempts
                    ? $"Failed for '{x.Solution.ToString()}' after {"guess".ToQuantity(x.Guesses.Count)}: {delimited}"
                    : $"Solved '{x.Solution.ToString()}' in {x.Guesses.Count} attempts ; guesses: {delimited}");
            });

        var totalGuessCount = solverStats.SuccessCount + solverStats.FailCount;
        var avgGuessCount = solverStats.TotalGuesses * 1d / totalGuessCount;
        testHelper.WriteLine(
            $"Completed with success: {solverStats.SuccessCount}; fail: {solverStats.FailCount}; total_guesses: {solverStats.TotalGuesses}; avg_guesses: {avgGuessCount:3F}");

        solverStats.FailCount.Should().BeLessOrEqualTo(9, "this is the benchmark set by the best run");
        totalGuessCount.Should().BeLessOrEqualTo(9057, "this is the benchmark set by the best run");
        //totalGuesses.Should().BeLessOrEqualTo(8905, "this is the benchmark set by the best run");
    }

    private record SolutionGuesses(
        Word Solution, 
        bool Solved,
        string? FailureReason,
        IReadOnlyCollection<Word> Guesses);

    private record AllWordsSolverStats(
        int SuccessCount,
        int FailCount,
        int TotalGuesses,
        List<SolutionGuesses> GuessesBySolution);

    [Fact(Skip="more of a test harness to find the optimal initial guess that has the most impact")]
    public void FindBestInitialWord()
    {
        var solutionWordLiterals = WordListReader
            .SolutionWordLiterals()
            .ToArray();
        var solutionWords = solutionWordLiterals
            .Select(Word.Create)
            .ToArray();
        var rankedInitialGuesses = 
            WordListReader
            .GuessWords
            .OrderByDescending(word => word.CalculateEliminationPower(Word.Empty, solutionWords))
            .ToArray();

        var guesser = new NewGuesser();
        var console = Mock.Of<IConsole>();
        var feedbackProvider = new DynamicFeedbackProvider();
        var solver = new Solver(console, guesser, feedbackProvider, solutionWordLiterals);        
        
        const int seed = 1;
        var bestAverage = double.MaxValue;
        foreach (var initialGuess in rankedInitialGuesses.Prepend("trace").Prepend("shade"))
        {
            guesser.InitialGuess = initialGuess;
            var stats = SolveAllSolutionWords(solver, feedbackProvider, () => new Random(seed));
            if (stats.FailCount > 0)
            {
                testHelper.WriteLine($"Initial: '{initialGuess.ToString()}'; fail_count={stats.FailCount}");
                continue;
            }

            if (stats.GuessesBySolution.Any(x => x.Guesses.Count > Solver.DefaultMaxAttempts))
            {
                testHelper.WriteLine($"Initial: '{initialGuess.ToString()}'; EXHAUSTION");
                continue;
            }

            var average = stats.TotalGuesses / (double)solutionWords.Length;
            if (average < bestAverage)
            {
                testHelper.WriteLine($"Initial: '{initialGuess.ToString()}'; avg: {average:F2} (NEW BEST)");
                bestAverage = average;
            }
            else
            {
                testHelper.WriteLine($"Initial: '{initialGuess.ToString()}'; avg: {average:F2}");
            }
        }
    }
    
    private static AllWordsSolverStats SolveAllSolutionWords(Solver solver, DynamicFeedbackProvider feedbackProvider, Func<Random> newRandom)
    {
        var (successCount, failCount) = (0, 0);
        var totalGuesses = 0;
        
        var solverStats = solver
            .SolutionWordList
            .Select(targetWord =>
            {
                var random = newRandom();
                feedbackProvider.Solution = targetWord;
                return (targetWord, result: solver.Solve(random, 1000));
            })
            .OrderBy(x => x.result.guesses.Count)
            .ToList();

        foreach (var x in solverStats)
        {
            if (x.result.solution == null)
            {
                failCount++;
            }
            else
            {
                successCount++;
                totalGuesses +=x.result.guesses.Count;    
            }
        }

        return new AllWordsSolverStats(
            successCount, 
            failCount, 
            totalGuesses, 
            solverStats.Select(x => 
                new SolutionGuesses(x.targetWord, x.result.solution.HasValue, x.result.failureReason, x.result.guesses.ToArray())).ToList());
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
        var solver = fixture.Solver;
        fixture.FeedbackProvider.Solution = solution;
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
                solverSolution.ToString()
                    .Should()
                    .Be(
                        solution.ToString(),
                        $"seed was {currentSeed}, guesses were {RenderGuesses(guesses)}, failure reason was {failureReason}"
                    );
                guesses
                    .Count.Should()
                    .BeLessOrEqualTo(
                        Solver.DefaultMaxAttempts,
                        $"seed was {currentSeed}, guesses were {RenderGuesses(guesses)}, failure reason was {failureReason}"
                    );
                failureReason.Should().BeNull();
                currentSeed = currentSeed == int.MaxValue ? 0 : currentSeed + 1;
                
                testHelper.WriteLine($"Solved '{solution.ToString()}' in {"guess".ToQuantity(guesses.Count)}: {RenderGuesses(guesses)}");
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

        var solver = new Solver(console, fixture.Guesser, feedbackProviderMock.Object);

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

        var solver = new Solver(console, fixture.Guesser, feedbackProviderMock.Object);

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
        { "2025-01-19", "rower" },
        { "2025-01-20", "squid" },
        { "2025-01-21", "icing" },
        { "2025-01-22", "reach" },
        { "2025-01-23", "upper" },
        { "2025-01-24", "crepe" },
        // { "2025-01-25", "" },
    };
    
    private static string RenderGuesses(IReadOnlyCollection<Word> guesses) => 
        string.Join($" {Unicode.RightArrow} ", guesses.Select(guess => $"'{guess.ToString()}'"));
}

[CollectionDefinition("SolverCollection")]
public sealed class SolverCollection : ICollectionFixture<SolverFixture> { }

public sealed class SolverFixture
{
    public SolverFixture()
    {
        SolutionWordList = WordListReader.SolutionWordLiterals().ToArray();
        var console = Mock.Of<IConsole>();
        FeedbackProvider = new DynamicFeedbackProvider();
        Guesser = new NewGuesser();
        Solver = new Solver(console, Guesser, FeedbackProvider, SolutionWordList);
    }

    public IGuesser Guesser { get; }

    public DynamicFeedbackProvider FeedbackProvider { get; }

    public Solver Solver { get; }
    public string[] SolutionWordList { get; }
}
