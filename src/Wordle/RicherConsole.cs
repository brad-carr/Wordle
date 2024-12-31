using System.Text;

namespace Wordle;

public class RicherConsole : IConsole
{
    private static readonly Dictionary<string, ConsoleColor> ColorMap =
        Enum.GetNames<ConsoleColor>()
            .Zip(Enum.GetValues<ConsoleColor>())
            .ToDictionary(z => z.First.ToLower(), z => z.Second, StringComparer.OrdinalIgnoreCase);

    private IConsole _inner;

    public RicherConsole(IConsole inner)
    {
        _inner = inner;
        Clear();
        ResetColors();
    }

    public ConsoleColor BackgroundColor
    {
        get => _inner.BackgroundColor;
        set => _inner.BackgroundColor = value;
    }

    public ConsoleColor ForegroundColor
    {
        get => _inner.ForegroundColor;
        set => _inner.ForegroundColor = value;
    }

    public void Clear() => _inner.Clear();

    public string? ReadLine()
    {
        ForegroundColor = ConsoleColor.Yellow;
        var input = _inner.ReadLine();
        ResetColors();
        return input;
    }

    public void Write(string text)
    {
        ResetColors();
        var rainbow = false;
        var buf = new StringBuilder();
        ConsoleColor nextColor;
        foreach (var c in text)
        {
            if (c == '$')
            {
                _inner.Write(buf.ToString());
                buf.Clear();
            }
            else if (c == '(')
            {
                var colorLiteral = buf.ToString().ToLower();
                rainbow = string.Equals(
                    colorLiteral,
                    "rainbow",
                    StringComparison.OrdinalIgnoreCase
                );
                if (!rainbow)
                {
                    ForegroundColor = nextColor = ColorMap[buf.ToString()];
                }

                buf.Clear();
            }
            else if (c == ')')
            {
                if (rainbow)
                {
                    WriteRainbow(buf);
                }
                else
                {
                    _inner.Write(buf.ToString());
                }

                buf.Clear();
                ResetColors();
            }
            else
            {
                buf.Append(c);
            }
        }
        _inner.Write(buf.ToString());
    }

    public void Write(char c) => _inner.Write(c);

    public void WriteLine() => _inner.WriteLine();

    public void WriteLine(string text)
    {
        Write(text);
        WriteLine();
    }

    private void WriteRainbow(StringBuilder buf)
    {
        _inner.ForegroundColor = BackgroundColor;
        RotateForegroundColor();

        foreach (var k in buf.ToString())
        {
            Write(k);
            RotateForegroundColor();

            if (ForegroundColor == BackgroundColor)
            {
                RotateForegroundColor();
            }
        }
    }

    private void RotateForegroundColor() =>
        ForegroundColor =
            ForegroundColor == ConsoleColor.White ? ConsoleColor.Black : ForegroundColor + 1;

    public void ResetColors()
    {
        BackgroundColor = ConsoleColor.Black;
        ForegroundColor = ConsoleColor.Gray;
    }
}
