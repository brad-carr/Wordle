namespace Wordle;

public interface IConsole
{
    string? ReadLine();

    void WriteLine(string text);

    void Write(string text);
}
