namespace Wordle
{
    public interface IFeedbackProvider
    {
        string? GetFeedback(int remainingWordCount);
    }
}
