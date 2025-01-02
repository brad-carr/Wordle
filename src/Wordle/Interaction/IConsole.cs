namespace Wordle.Interaction;

public interface IConsole
{
    ConsoleColor BackgroundColor { get; set; }

    ConsoleColor ForegroundColor { get; set; }

    void Clear();

    string? ReadLine();

    void WriteLine();

    void WriteLine(string text);

    void Write(char c);

    void Write(string text);
}
