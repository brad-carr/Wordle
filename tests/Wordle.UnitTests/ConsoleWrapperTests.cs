using FluentAssertions;

namespace Wordle.UnitTests;

using Wordle.Interaction;

[Collection(nameof(ConsoleTestCollection))]
public sealed class ConsoleWrapperTests
{
    [Fact]
    public void GetColorProperties_ShouldNotBeNegative()
    {
        //Arrange
        var sut = new ConsoleWrapper();

        // Act
        sut.ForegroundColor.Should().NotBe((ConsoleColor)(-1));
        sut.BackgroundColor.Should().NotBe((ConsoleColor)(-1));
    }

    [Fact]
    public void SetColorProperties_NoAssertions()
    {
        //Arrange
        var sut = new ConsoleWrapper();

        // Act
        sut.ForegroundColor = ConsoleColor.Red;
        sut.BackgroundColor = ConsoleColor.Red;
    }

    [Fact]
    public void Write_WhenCalled_ShouldWriteToStdOut()
    {
        // Arrange
        var testString = "Test String";
        var stringWriter = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(stringWriter);
        var sut = new ConsoleWrapper();

        // Act
        sut.Write(testString);

        // Assert
        stringWriter.ToString().Should().Be(testString);

        // Cleanup
        Console.SetOut(originalOut);
    }

    [Fact]
    public void WriteLine_WithNoParameters_ShouldWriteNewLine()
    {
        // Arrange
        var stringWriter = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(stringWriter);
        var sut = new ConsoleWrapper();

        // Act
        sut.WriteLine();

        // Assert
        stringWriter.ToString().Should().Be(Environment.NewLine);

        // Cleanup
        Console.SetOut(originalOut);
    }

    [Fact]
    public void WriteLine_WithText_ShouldWriteTextAndNewLine()
    {
        // Arrange
        var testString = "Test String";
        var stringWriter = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(stringWriter);
        var sut = new ConsoleWrapper();

        // Act
        sut.WriteLine(testString);

        // Assert
        stringWriter.ToString().Should().Be($"{testString}{Environment.NewLine}");

        // Cleanup
        Console.SetOut(originalOut);
    }

    [Fact]
    public void WriteLine_WithNullText_ShouldWriteNewLine()
    {
        // Arrange
        var stringWriter = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(stringWriter);
        var sut = new ConsoleWrapper();

        // Act
        sut.WriteLine(null!);

        // Assert
        stringWriter.ToString().Should().Be($"{Environment.NewLine}");

        // Cleanup
        Console.SetOut(originalOut);
    }

    [Fact]
    public void Clear_WhenOutputRedirected_ShouldNotWriteAnything()
    {
        // Arrange
        var stringWriter = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(stringWriter);
        var sut = new ConsoleWrapper();

        try
        {
            // Act
            sut.Clear();

            // Assert
            stringWriter.ToString().Should().BeEmpty();
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Theory]
    [InlineData('a', "a")]
    [InlineData('\n', "\n")]
    [InlineData('€', "€")]
    [InlineData(' ', " ")]
    public void Write_WhenCalledWithChar_ShouldWriteToConsole(char input, string expected)
    {
        // Arrange
        using var stringWriter = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(stringWriter);
        var sut = new ConsoleWrapper();

        try
        {
            // Act
            sut.Write(input);

            // Assert
            stringWriter.ToString().Should().Be(expected);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Theory]
    [InlineData("Hello World")]
    [InlineData("12345")]
    public void ReadLine_WhenInputProvided_ShouldReturnInput(string expected)
    {
        // Arrange
        using var stringReader = new StringReader(expected);
        var originalIn = Console.In;
        Console.SetIn(stringReader);
        var sut = new ConsoleWrapper();

        try
        {
            // Act
            var result = sut.ReadLine();

            // Assert
            result.Should().Be(expected);
        }
        finally
        {
            Console.SetIn(originalIn);
        }
    }

    [Fact]
    public void ReadLine_WhenEndOfStream_ShouldReturnNull()
    {
        // Arrange
        using var stringReader = new StringReader(string.Empty);
        var originalIn = Console.In;
        Console.SetIn(stringReader);
        var sut = new ConsoleWrapper();

        try
        {
            // Act
            var result = sut.ReadLine();

            // Assert
            result.Should().BeNull();
        }
        finally
        {
            Console.SetIn(originalIn);
        }
    }
}

[CollectionDefinition(nameof(ConsoleTestCollection), DisableParallelization = true)]
public sealed class ConsoleTestCollection { }
