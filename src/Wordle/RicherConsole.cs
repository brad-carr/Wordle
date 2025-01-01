using System.Text;

namespace Wordle;

/// <summary>Wraps an existing console and adds support for colored text and rainbow text.</summary>
public class RicherConsole : IConsole
{
    private static readonly Dictionary<string, ConsoleColor> ColorMap =
        Enum.GetNames<ConsoleColor>()
            .Zip(Enum.GetValues<ConsoleColor>())
            .ToDictionary(z => z.First.ToLower(), z => z.Second, StringComparer.OrdinalIgnoreCase);

    private IConsole _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="RicherConsole"/> class.
    /// </summary>
    /// <param name="inner">The inner console to wrap.</param>
    public RicherConsole(IConsole inner)
    {
        _inner = inner;
        Clear();
    }

    /// <summary>
    /// Gets or sets the background color of the inner console.
    /// </summary>
    public ConsoleColor BackgroundColor
    {
        get => _inner.BackgroundColor;
        set => _inner.BackgroundColor = value;
    }

    /// <summary>
    /// Gets or sets the foreground color of the inner console.
    /// </summary>
    public ConsoleColor ForegroundColor
    {
        get => _inner.ForegroundColor;
        set => _inner.ForegroundColor = value;
    }

    /// <summary>
    /// Clears the inner console.
    /// </summary>
    public void Clear() => _inner.Clear();

    /// <summary>
    /// Reads the next line of characters from the inner console.
    /// </summary>
    /// <returns>
    /// The next line of characters from the inner console, or null if the end of the input stream has been reached.
    /// </returns>
    public string? ReadLine()
    {
        var restoreForegroundColor = ForegroundColor;
        ForegroundColor = ConsoleColor.Yellow;
        var input = _inner.ReadLine();
        if (restoreForegroundColor != ConsoleColor.Yellow)
        {
            ForegroundColor = restoreForegroundColor;
        }
        return input;
    }

    /// <summary>
    /// Writes markup text to the console, with support for colored text and rainbow text.
    /// </summary>
    /// <param name="markup">
    /// The text to write. This text can contain color markers in the form of $colorName(...).
    /// </param>
    /// <remarks>
    /// The color markers are:
    /// - $colorName(...) - sets the color of the text that follows. The colorName is case-insensitive.
    /// - $rainbow(...) - sets the color of each character in the text that follows to a different color.
    /// </remarks>
    /// <example>
    ///   <code>
    ///     console.Write("$green(Hello), $red(world)! $rainbow(Rainbow text!)");
    ///   </code>
    /// </example>
    public void Write(string markup)
    {
        var rainbow = false;
        var buf = new StringBuilder();
        var colorStack = new Stack<ConsoleColor>();
        foreach (var c in markup)
        {
            if (c == '(' && TryExtractColorChangeMarkup(buf, out var colorLiteral))
            {
                FlushBuffer(buf);

                colorStack.Push(ForegroundColor);

                if (!(rainbow = colorLiteral == "rainbow"))
                {
                    ForegroundColor = ColorMap[colorLiteral!];
                }

                buf.Clear();
            }
            else if (c == ')' && colorStack.Count > 0)
            {
                if (rainbow)
                {
                    WriteRainbow(buf);
                }
                else
                {
                    FlushBuffer(buf);
                }

                buf.Clear();
                ForegroundColor = colorStack.Pop();
            }
            else
            {
                buf.Append(c);
            }
        }

        if (colorStack.Count > 0)
        {
            throw new InvalidOperationException("$<color>(...) expression not terminated.");
        }

        FlushBuffer(buf);
    }

    private void FlushBuffer(StringBuilder buf)
    {
        if (buf.Length > 0)
        {
            _inner.Write(buf.ToString());
            buf.Clear();
        }
    }

    private static bool TryExtractColorChangeMarkup(StringBuilder buf, out string? colorLiteral)
    {
        var i = buf.Length - 1;
        while (i >= 0 && buf[i] != '$')
        {
            i--;
        }

        if (i < 0)
        {
            colorLiteral = null;
            return false;
        }

        colorLiteral = buf.ToString(i + 1, buf.Length - i - 1).ToLower();
        buf.Remove(i, buf.Length - i);
        return true;
    }

    /// <summary>
    /// Writes a character to the inner console.
    /// </summary>
    public void Write(char c) => _inner.Write(c);

    /// <summary>
    /// Writes a line terminator to the inner console.
    /// </summary>
    public void WriteLine() => _inner.WriteLine();

    /// <summary>
    /// Writes markup text followed by a line terminator to the inner console.
    /// </summary>
    /// <param name="markup">
    /// The text to write. This text can contain color markers in the form of $colorName(...).
    /// </param>
    /// <remarks>
    /// The color markers are:
    /// - $colorName(...) - sets the color of the text that follows. The colorName is case-insensitive.
    /// - $rainbow(...) - sets the color of each character in the text that follows to a different color.
    /// </remarks>
    /// <example>
    ///   <code>
    ///     console.Write("$green(Hello), $red(world)! $rainbow(Rainbow text!)");
    ///   </code>
    /// </example>
    public void WriteLine(string markup)
    {
        Write(markup);
        WriteLine();
    }

    private void WriteRainbow(StringBuilder buf)
    {
        var nextForegroundColor = RotateForegroundColor(BackgroundColor);

        foreach (var k in buf.ToString())
        {
            ForegroundColor = nextForegroundColor;
            Write(k);
            nextForegroundColor = RotateForegroundColor(nextForegroundColor);
        }
    }

    private ConsoleColor RotateForegroundColor(ConsoleColor foregroundColor)
    {
        var next = foregroundColor == ConsoleColor.White ? ConsoleColor.Black : foregroundColor + 1;
        return next == BackgroundColor ? RotateForegroundColor(next) : next;
    }
}
