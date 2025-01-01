namespace Wordle;

internal sealed class ConsoleWrapper : IConsole
{
    public void Clear()
    {
        if (!Console.IsOutputRedirected)
        {
            Console.Clear();
        }
    }

    public ConsoleColor BackgroundColor
    {
        get => Console.BackgroundColor;
        set => Console.BackgroundColor = value;
    }

    public ConsoleColor ForegroundColor
    {
        get => Console.ForegroundColor;
        set => Console.ForegroundColor = value;
    }

    public string? ReadLine() => Console.ReadLine();

    public void WriteLine() => Console.WriteLine();

    public void WriteLine(string text) => Console.WriteLine(text);

    public void Write(char c) => Console.Write(c);

    public void Write(string text) => Console.Write(text);
}
