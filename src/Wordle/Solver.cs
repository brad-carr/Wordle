using Humanizer;

namespace Wordle;

public sealed class Solver(IConsole console, IFeedbackProvider feedbackProvider)
{
    public const int WordLength = 5;
    private readonly IConsole _console = console;
    internal static readonly string SolvedFeedback = new('c', WordLength);

    public (string? solution, IReadOnlyCollection<string> guesses) Solve(DateOnly publicationDate)
    {
        var seed = GetSeed(publicationDate);
        var random = new Random(seed);
        return Solve(random);
    }

    public (string? solution, IReadOnlyCollection<string> guesses) Solve(Random random)
    {
        var remainingWords = WordListReader.EnumerateLines().ToArray();
        var solution = Enumerable.Repeat(' ', WordLength).ToArray();
        var guesses = new List<string>(10);
        var numAttempts = 0;

        while (true)
        {
            numAttempts++;

            var guess =
                remainingWords.Length == 1
                    ? remainingWords[0]
                    : remainingWords
                        .GroupBy(word => word.Distinct().Count())
                        .OrderByDescending(g => g.Key)
                        .First()
                        .RandomElement(random)!;

            guesses.Add(guess);

            _console.WriteLine(
                $"Suggestion $magenta({numAttempts}): $green({guess.ToUpper()}) - out of $magenta({"possibility".ToQuantity(remainingWords.Length)})"
            );

            var feedback = feedbackProvider.GetFeedback(guess, remainingWords.Length);
            if (feedback == null)
            {
                return (null, guesses);
            }
            if (feedback == SolvedFeedback)
            {
                return (guess, guesses);
            }

            var operations = feedback
                .Zip(guess)
                .Select((x, i) => (f: x.First, c: x.Second, i))
                .Where(x => solution[x.i] != x.c) // skip already solved positional indexes
                .OrderBy(x => x.f); // ensures processing order 'c' -> 'm' -> 'n'

            var misplacedLetters = new HashSet<char>();

            foreach (var (f, c, i) in operations)
            {
                switch (f)
                {
                    case Feedback.Correct:
                        solution[i] = c;
                        remainingWords = remainingWords.Where(w => w[i] == c).ToArray();
                        break;
                    case Feedback.Misplaced:
                        misplacedLetters.Add(c);
                        var unsolvedIndexes = Enumerable
                            .Range(0, WordLength)
                            .Where(j => j != i && solution[j] == ' ');
                        remainingWords = remainingWords
                            .Where(w => w[i] != c && unsolvedIndexes.Any(u => w[u] == c))
                            .ToArray();
                        break;
                    case Feedback.NoMoreOccurrences:
                        if (misplacedLetters.Contains(c))
                        {
                            // skip if same character misplaced elsewhere
                            // required for seed test to pass: [InlineData(20241295, "mambo")]
                            break;
                        }

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
                return (null, guesses);
            }
        }
    }

    internal static int GetSeed(DateOnly publicationDate) =>
        publicationDate.Year * 10000 + publicationDate.Month * 100 + publicationDate.Day;
}
