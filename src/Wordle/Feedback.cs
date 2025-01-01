namespace Wordle;

internal static class Feedback
{
    public const char Correct = 'c';
    public const char Misplaced = 'm';
    public const char NoMoreOccurrences = 'n';

    public static bool IsValid(char c) =>
        c switch
        {
            Correct or Misplaced or NoMoreOccurrences => true,
            _ => false,
        };
}
