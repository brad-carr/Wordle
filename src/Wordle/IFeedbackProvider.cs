namespace Wordle
{
    public interface IFeedbackProvider
    {
        string? GetFeedback(string suggestion, int remainingWordCount);
    }
}
