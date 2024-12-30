using System.Text;

namespace Wordle;

public class RicherConsole : IConsole
{
    static readonly Dictionary<string, ConsoleColor> ColorMap = Enum.GetNames<ConsoleColor>()
        .Zip(Enum.GetValues<ConsoleColor>())
        .ToDictionary(z => z.First.ToLower(), z => z.Second, StringComparer.OrdinalIgnoreCase);

    public RicherConsole()
    {
        if (!Console.IsOutputRedirected)
        {
            Console.Clear();
        }
        ResetColors();
    }

    public string? ReadLine()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        var input = Console.ReadLine();
        ResetColors();
        return input;
    }

    public void WriteLine(string text)
    {
        Write(text);
        Console.WriteLine();
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
                Console.Write(buf.ToString());
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
                    Console.ForegroundColor = nextColor = ColorMap[buf.ToString()];
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
                    Console.Write(buf.ToString());
                }

                buf.Clear();
                ResetColors();
            }
            else
            {
                buf.Append(c);
            }
        }
        Console.Write(buf);
    }

    private void WriteRainbow(StringBuilder buf)
    {
        Console.ForegroundColor = Console.BackgroundColor;
        RotateForegroundColor();

        foreach (var k in buf.ToString())
        {
            Console.Write(k);

            RotateForegroundColor();

            if (Console.ForegroundColor == Console.BackgroundColor)
            {
                RotateForegroundColor();
            }
        }
    }

    private void RotateForegroundColor() =>
        Console.ForegroundColor =
            Console.ForegroundColor == ConsoleColor.White
                ? ConsoleColor.Black
                : Console.ForegroundColor + 1;

    public void ResetColors() => Console.ForegroundColor = ConsoleColor.Gray;
}
