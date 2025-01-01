namespace Wordle
{
    /// <summary>
    /// Provides feedback on guesses.
    /// </summary>
    public interface IFeedbackProvider
    {
        /// <summary>
        /// Provides feedback on a guess.
        /// </summary>
        string? GetFeedback(string guess, int remainingWordCount);
    }
}
