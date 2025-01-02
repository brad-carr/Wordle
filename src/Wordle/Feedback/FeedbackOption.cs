namespace Wordle.Feedback;

internal static class FeedbackOption
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

    public static bool IsInvalid(char c) => !IsValid(c);
}
