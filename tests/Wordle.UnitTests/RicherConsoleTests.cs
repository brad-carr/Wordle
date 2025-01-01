using FluentAssertions;
using Moq;

namespace Wordle.UnitTests
{
    public class RicherConsoleTests
    {
        private const ConsoleColor DefaultBackgroundColor = ConsoleColor.Black;
        private const ConsoleColor DefaultForegroundColor = ConsoleColor.Gray;

        [Fact]
        public void GetBackgroundColor_SourcedFromInnerConsole()
        {
            var inner = new Mock<IConsole>(MockBehavior.Strict);
            var sut = CreateAndVerifySUT(inner);

            inner.SetupGet(mock => mock.BackgroundColor).Returns(ConsoleColor.Yellow).Verifiable();

            sut.BackgroundColor.Should().Be(ConsoleColor.Yellow);

            inner.VerifyGet(mock => mock.BackgroundColor, Times.Once());
            inner.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetForegroundColor_SourcedFromInnerConsole()
        {
            var inner = new Mock<IConsole>(MockBehavior.Strict);
            var sut = CreateAndVerifySUT(inner);

            inner.SetupGet(mock => mock.ForegroundColor).Returns(ConsoleColor.Yellow).Verifiable();

            sut.ForegroundColor.Should().Be(ConsoleColor.Yellow);

            inner.VerifyGet(mock => mock.ForegroundColor, Times.Once());
            inner.VerifyNoOtherCalls();
        }

        [Fact]
        public void SetBackgroundColor_PropagatesToInnerConsole()
        {
            var inner = new Mock<IConsole>(MockBehavior.Strict);
            var sut = CreateAndVerifySUT(inner);

            inner.SetupSet(mock => mock.BackgroundColor = ConsoleColor.Yellow).Verifiable();

            sut.BackgroundColor = ConsoleColor.Yellow;

            inner.VerifySet(mock => mock.BackgroundColor = ConsoleColor.Yellow, Times.Once());
            inner.VerifyNoOtherCalls();
        }

        [Fact]
        public void SetForegroundColor_PropagatesToInnerConsole()
        {
            var inner = new Mock<IConsole>(MockBehavior.Strict);
            var sut = CreateAndVerifySUT(inner);

            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.Yellow).Verifiable();
            sut.ForegroundColor = ConsoleColor.Yellow;

            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.Yellow, Times.Once());
            inner.VerifyNoOtherCalls();
        }

        [Fact]
        public void ReadLine_ReturnsInputFromInnerConsole()
        {
            var inner = new Mock<IConsole>(MockBehavior.Strict);
            var sut = CreateAndVerifySUT(inner);

            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.Yellow).Verifiable();
            inner
                .SetupGet(mock => mock.ForegroundColor)
                .Returns(DefaultForegroundColor)
                .Verifiable();
            inner.Setup(mock => mock.ReadLine()).Returns("Hello world!").Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = DefaultForegroundColor).Verifiable();

            var result = sut.ReadLine();

            result.Should().Be("Hello world!");
            inner.Verify(mock => mock.ReadLine(), Times.Once());
            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.Yellow, Times.Once());
            inner.VerifySet(mock => mock.ForegroundColor = DefaultForegroundColor, Times.Once());
            inner.VerifyGet(mock => mock.ForegroundColor, Times.Once());
            inner.VerifyNoOtherCalls();
        }

        [Fact]
        public void Write_PlainText()
        {
            var inner = new Mock<IConsole>(MockBehavior.Strict);
            var sut = CreateAndVerifySUT(inner);

            inner.Setup(mock => mock.Write("Hello world!")).Verifiable();

            sut.Write("Hello world!");

            inner.Verify(mock => mock.Write("Hello world!"), Times.Once());
            inner.VerifyNoOtherCalls();
        }

        [Fact]
        public void WriteLine_PlainText()
        {
            var inner = new Mock<IConsole>(MockBehavior.Strict);
            var sut = CreateAndVerifySUT(inner);

            inner.Setup(mock => mock.Write("Hello world!")).Verifiable();
            inner.Setup(mock => mock.WriteLine()).Verifiable();

            sut.WriteLine("Hello world!");

            inner.Verify(mock => mock.Write("Hello world!"), Times.Once());
            inner.Verify(mock => mock.WriteLine(), Times.Once());
            inner.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("(not markup)")]
        [InlineData("(not markup) Hello world! (not markup)")]
        public void Write_PlainTextBracesNotConsideredMarkup(string plainText)
        {
            var inner = new Mock<IConsole>(MockBehavior.Strict);
            var sut = CreateAndVerifySUT(inner);

            inner.Setup(mock => mock.Write(plainText)).Verifiable();

            sut.Write(plainText);

            inner.Verify(mock => mock.Write(plainText), Times.Once());
            inner.VerifyNoOtherCalls();
        }

        [Fact]
        public void Write_Markup()
        {
            var inner = new Mock<IConsole>(MockBehavior.Strict);
            var sut = CreateAndVerifySUT(inner);

            inner
                .SetupGet(mock => mock.ForegroundColor)
                .Returns(DefaultForegroundColor)
                .Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.Green).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = DefaultForegroundColor).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.Red).Verifiable();
            inner.Setup(mock => mock.Write("Hello")).Verifiable();
            inner.Setup(mock => mock.Write(" ")).Verifiable();
            inner.Setup(mock => mock.Write("world!")).Verifiable();

            sut.Write("$green(Hello) $red(world!)");

            inner.Verify(mock => mock.Write("Hello"), Times.Once());
            inner.Verify(mock => mock.Write(" "), Times.Once());
            inner.Verify(mock => mock.Write("world!"), Times.Once());
            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.Green, Times.Once());
            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.Red, Times.Once());
            inner.VerifySet(
                mock => mock.ForegroundColor = DefaultForegroundColor,
                Times.Exactly(2)
            );
            inner.VerifyGet(mock => mock.ForegroundColor, Times.Exactly(2));
            inner.VerifyNoOtherCalls();
        }

        [Fact]
        public void Write_MarkupNotTerminated()
        {
            var inner = new Mock<IConsole>(MockBehavior.Strict);
            var sut = CreateAndVerifySUT(inner);

            inner
                .SetupGet(mock => mock.ForegroundColor)
                .Returns(DefaultForegroundColor)
                .Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.Green).Verifiable();

            sut.Invoking(x => x.Write("$green(Not terminated")) // missing closing parenthesis
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("$<color>(...) expression not terminated.");

            inner.VerifyGet(mock => mock.ForegroundColor, Times.Once());
            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.Green, Times.Once());
            inner.VerifyNoOtherCalls();
        }

        [Fact]
        public void Write_MarkupRainbowCyclesColors()
        {
            var inner = new Mock<IConsole>(MockBehavior.Strict);
            var sut = CreateAndVerifySUT(inner);

            inner
                .SetupGet(mock => mock.ForegroundColor)
                .Returns(DefaultForegroundColor)
                .Verifiable();

            inner.Setup(mock => mock.Write("plain text ")).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.DarkBlue).Verifiable();
            inner.Setup(mock => mock.Write('R')).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.DarkGreen).Verifiable();
            inner.Setup(mock => mock.Write('a')).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.DarkCyan).Verifiable();
            inner.Setup(mock => mock.Write('i')).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.DarkRed).Verifiable();
            inner.Setup(mock => mock.Write('n')).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.DarkMagenta).Verifiable();
            inner.Setup(mock => mock.Write('b')).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.DarkYellow).Verifiable();
            inner.Setup(mock => mock.Write('o')).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.Gray).Verifiable();
            inner.Setup(mock => mock.Write('w')).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.DarkGray).Verifiable();
            inner.Setup(mock => mock.Write(' ')).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.Blue).Verifiable();
            inner.Setup(mock => mock.Write('T')).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.Green).Verifiable();
            inner.Setup(mock => mock.Write('e')).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.Cyan).Verifiable();
            inner.Setup(mock => mock.Write('x')).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.Red).Verifiable();
            inner.Setup(mock => mock.Write('t')).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = ConsoleColor.Magenta).Verifiable();
            inner.Setup(mock => mock.Write('!')).Verifiable();
            inner.SetupSet(mock => mock.ForegroundColor = DefaultForegroundColor).Verifiable();
            inner.Setup(mock => mock.Write(" more plain text")).Verifiable();

            sut.Write("plain text $rainbow(Rainbow Text!) more plain text");

            inner.Verify(mock => mock.Write("plain text "), Times.Once());
            inner.Verify(mock => mock.Write('R'));
            inner.Verify(mock => mock.Write('a'));
            inner.Verify(mock => mock.Write('i'));
            inner.Verify(mock => mock.Write('n'));
            inner.Verify(mock => mock.Write('b'));
            inner.Verify(mock => mock.Write('o'));
            inner.Verify(mock => mock.Write('w'));
            inner.Verify(mock => mock.Write(' '));
            inner.Verify(mock => mock.Write('T'));
            inner.Verify(mock => mock.Write('e'));
            inner.Verify(mock => mock.Write('x'));
            inner.Verify(mock => mock.Write('t'));
            inner.Verify(mock => mock.Write('!'));
            inner.Verify(mock => mock.Write(" more plain text"));

            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.DarkBlue, Times.Once());
            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.DarkGreen, Times.Once());
            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.DarkCyan, Times.Once());
            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.DarkRed, Times.Once());
            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.DarkMagenta, Times.Once());
            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.DarkYellow, Times.Once());
            // inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.Gray, Times.Once()); conflicts with default foreground color
            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.DarkGray, Times.Once());
            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.Blue, Times.Once());
            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.Green, Times.Once());
            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.Cyan, Times.Once());
            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.Red, Times.Once());
            inner.VerifySet(mock => mock.ForegroundColor = ConsoleColor.Magenta, Times.Once());
            inner.VerifySet(
                mock => mock.ForegroundColor = DefaultForegroundColor,
                Times.Exactly(2)
            );
            inner.VerifyGet(mock => mock.ForegroundColor, Times.Once());
            inner.VerifyGet(mock => mock.BackgroundColor, Times.Exactly(15));
            inner.VerifyNoOtherCalls();
        }

        private static RicherConsole CreateAndVerifySUT(Mock<IConsole> inner)
        {
            inner
                .SetupGet(mock => mock.ForegroundColor)
                .Returns(DefaultForegroundColor)
                .Verifiable();
            inner
                .SetupGet(mock => mock.BackgroundColor)
                .Returns(DefaultBackgroundColor)
                .Verifiable();
            inner.Setup(mock => mock.Clear()).Verifiable();

            var sut = new RicherConsole(inner.Object);

            inner.Verify(
                mock => mock.Clear(),
                Times.Once(),
                "should clear the console on initialization"
            );
            inner.VerifyNoOtherCalls();
            inner.Invocations.Clear();

            return sut;
        }
    }
}
