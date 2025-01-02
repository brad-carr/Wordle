namespace Wordle.UnitTests;

using FluentAssertions;
using Moq;
using Wordle;
using Wordle.Feedback;
using Wordle.Interaction;

public sealed class ConsoleFeedbackProviderTests
{
    [Fact]
    public void GetFeedback_SkipsConsoleInteractionsWhenOnlyOneOption()
    {
        // Arrange
        var consoleMock = new Mock<IConsole>(MockBehavior.Strict);
        var feedbackProvider = new ConsoleFeedbackProvider(consoleMock.Object);

        // Act
        var actual = feedbackProvider.GetFeedback("comic", 1);

        // Assert
        actual.Should().Be(Solver.SolvedFeedback);
        consoleMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void GetFeedback_GivesUpIfConsoleReadIsEOF()
    {
        // Arrange
        var consoleMock = new Mock<IConsole>(MockBehavior.Strict);
        consoleMock
            .Setup(mock =>
                mock.Write(
                    "Feedback - $yellow([C])orrect $yellow([M])isplaced $yellow([N])o more occurrences? "
                )
            )
            .Verifiable();
        consoleMock.Setup(mock => mock.ReadLine()).Returns((string?)null).Verifiable();
        consoleMock.Setup(mock => mock.WriteLine("Null feedback; terminating.")).Verifiable();
        var feedbackProvider = new ConsoleFeedbackProvider(consoleMock.Object);

        // Act
        var actual = feedbackProvider.GetFeedback("comic", 10);

        // Assert
        actual.Should().BeNull();
        consoleMock.Verify(
            mock =>
                mock.Write(
                    "Feedback - $yellow([C])orrect $yellow([M])isplaced $yellow([N])o more occurrences? "
                ),
            Times.Once
        );
        consoleMock.Verify(mock => mock.ReadLine(), Times.Once);
        consoleMock.Verify(mock => mock.WriteLine("Null feedback; terminating."), Times.Once);
        consoleMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("c", "Invalid feedback '$red(c)'; expected 5 chars, got 1.", -1)]
    [InlineData("cc", "Invalid feedback '$red(cc)'; expected 5 chars, got 2.", -1)]
    [InlineData("ccc", "Invalid feedback '$red(ccc)'; expected 5 chars, got 3.", -1)]
    [InlineData("cccc", "Invalid feedback '$red(cccc)'; expected 5 chars, got 4.", -1)]
    [InlineData("cccccc", "Invalid feedback '$red(cccccc)'; expected 5 chars, got 6.", -1)]
    [InlineData(
        "acccc",
        "Invalid feedback '$red(acccc)'; contains invalid char '$red(a)' at position 1. Use only letters $yellow(C), $yellow(M) or $yellow(N).",
        18
    )]
    [InlineData(
        "caccc",
        "Invalid feedback '$red(caccc)'; contains invalid char '$red(a)' at position 2. Use only letters $yellow(C), $yellow(M) or $yellow(N).",
        19
    )]
    [InlineData(
        "ccacc",
        "Invalid feedback '$red(ccacc)'; contains invalid char '$red(a)' at position 3. Use only letters $yellow(C), $yellow(M) or $yellow(N).",
        20
    )]
    [InlineData(
        "cccac",
        "Invalid feedback '$red(cccac)'; contains invalid char '$red(a)' at position 4. Use only letters $yellow(C), $yellow(M) or $yellow(N).",
        21
    )]
    [InlineData(
        "cccca",
        "Invalid feedback '$red(cccca)'; contains invalid char '$red(a)' at position 5. Use only letters $yellow(C), $yellow(M) or $yellow(N).",
        22
    )]
    public void GetFeedback_ReattemptsIfConsoleFeedbackIsInvalid(
        string invalidFeedback,
        string reason,
        int redUpArrowPosition
    )
    {
        // Arrange
        var consoleMock = new Mock<IConsole>(MockBehavior.Strict);
        consoleMock.Setup(mock => mock.WriteLine(reason)).Verifiable();
        consoleMock
            .Setup(mock =>
                mock.Write(
                    "Feedback - $yellow([C])orrect $yellow([M])isplaced $yellow([N])o more occurrences? "
                )
            )
            .Verifiable();
        consoleMock
            .SetupSequence(mock => mock.ReadLine())
            .Returns(invalidFeedback)
            .Returns(Solver.SolvedFeedback);

        var expectedArrowOutput = string.Empty;
        if (redUpArrowPosition != -1)
        {
            var expectedPadding = new string(' ', redUpArrowPosition);
            expectedArrowOutput = $"{expectedPadding}$red({Unicode.UpArrow})";
            consoleMock.Setup(mock => mock.WriteLine(expectedArrowOutput)).Verifiable();
        }
        var feedbackProvider = new ConsoleFeedbackProvider(consoleMock.Object);

        // Act
        var actual = feedbackProvider.GetFeedback("comic", 5);

        // Assert
        actual.Should().Be(Solver.SolvedFeedback, reason);
        consoleMock.Verify(
            mock =>
                mock.Write(
                    "Feedback - $yellow([C])orrect $yellow([M])isplaced $yellow([N])o more occurrences? "
                ),
            Times.Exactly(2)
        );
        consoleMock.Verify(mock => mock.ReadLine(), Times.Exactly(2));
        consoleMock.Verify(mock => mock.WriteLine(reason), Times.Once);
        if (redUpArrowPosition != -1)
        {
            consoleMock.Verify(mock => mock.WriteLine(expectedArrowOutput), Times.Once);
        }
        consoleMock.VerifyNoOtherCalls();
    }
}
