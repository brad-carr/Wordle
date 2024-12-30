namespace Wordle;

public static class Feedback
{
    public const char Correct = 'c';
    public const char Misplaced = 'm';
    public const char NoMoreOccurrences = 'n';

    internal static bool IsValid(char c) =>
        c switch
        {
            Correct or Misplaced or NoMoreOccurrences => true,
            _ => false,
        };
}
