using Humanizer;

namespace Wordle;

public class Solver(IConsole console, IFeedbackProvider feedbackProvider)
{
    public const int WordLength = 5;
    private IConsole _console = console;
    internal static readonly string SolvedFeedback = new('c', WordLength);

    public (string? solution, int numGuesses) Solve(DateOnly publicationDate)
    {
        var remainingWords = WordListReader.EnumerateLines().ToArray();
        var solution = Enumerable.Repeat(' ', WordLength).ToArray();
        var numAttempts = 0;
        var rand = new Random(
            publicationDate.Year * 10000 + publicationDate.Month * 100 + publicationDate.Day
        );

        while (true)
        {
            numAttempts++;

            var suggestion =
                remainingWords.Length == 1
                    ? remainingWords[0]
                    : remainingWords
                        .GroupBy(word => word.Distinct().Count())
                        .OrderByDescending(g => g.Key)
                        .First()
                        .RandomElement(rand)!;

            _console.WriteLine(
                $"Suggestion $magenta({numAttempts}): $green({suggestion.ToUpper()}) - out of $magenta({"possibility".ToQuantity(remainingWords.Length)})"
            );

            var feedback = feedbackProvider.GetFeedback(remainingWords.Length);
            if (feedback == null)
            {
                return (null, numAttempts);
            }
            if (feedback == SolvedFeedback)
            {
                return (suggestion, numAttempts);
            }

            var operations = feedback
                .Zip(suggestion)
                .Select((x, i) => (f: x.First, c: x.Second, i))
                .Where(x => solution[x.i] != x.c) // skip already solved positional indexes
                .OrderBy(x => x.f); // ensures processing order 'c' -> 'm' -> 'n'

            foreach (var (f, c, i) in operations)
            {
                switch (f)
                {
                    case Feedback.Correct:
                        solution[i] = c;
                        remainingWords = remainingWords.Where(w => w[i] == c).ToArray();
                        break;
                    case Feedback.Misplaced:
                        var unsolvedIndexes = Enumerable
                            .Range(0, WordLength)
                            .Where(j => j != i && solution[j] == ' ');
                        remainingWords = remainingWords
                            .Where(w => w[i] != c && unsolvedIndexes.Any(u => w[u] == c))
                            .ToArray();
                        break;
                    case Feedback.NoMoreOccurrences:
                        for (var j = 0; j < WordLength; j++)
                        {
                            if (solution[j] == ' ')
                            {
                                remainingWords = remainingWords.Where(w => w[j] != c).ToArray();
                            }
                        }
                        break;
                }
            }

            if (remainingWords.Length == 0)
            {
                _console.WriteLine("$red(No remaining words, check input)");
                return (null, numAttempts);
            }
        }
    }
}
