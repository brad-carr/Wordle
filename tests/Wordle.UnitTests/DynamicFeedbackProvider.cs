namespace Wordle.UnitTests;

public sealed class DynamicFeedbackProvider(string Solution) : IFeedbackProvider
{
    public string? GetFeedback(string guess, int remainingWordCount)
    {
        guess = guess ?? throw new ArgumentNullException(nameof(guess));

        if (guess.Length != Solution.Length)
        {
            throw new ArgumentException(
                $"Invalid guess length; expected {Solution.Length}, got {guess.Length}.",
                nameof(guess)
            );
        }

        var feedbackArray = new char[guess.Length];
        for (var i = 0; i < guess.Length; i++)
        {
            if (guess[i] == Solution[i])
            {
                feedbackArray[i] = 'c';
            }
            else if (Solution.Contains(guess[i]))
            {
                feedbackArray[i] = 'm';
            }
            else
            {
                feedbackArray[i] = 'n';
            }
        }

        return new string(feedbackArray);
    }
}
