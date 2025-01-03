using System.Collections.Immutable;
using Humanizer;
using Wordle.Feedback;
using Wordle.Interaction;

namespace Wordle;

public sealed class Solver(IConsole console, IFeedbackProvider feedbackProvider)
{
    public const int WordLength = 5;
    public const int MaxAttempts = 6;

    private readonly IConsole _console = console;
    internal static readonly string SolvedFeedback = new('c', WordLength);

    public (string? solution, IReadOnlyCollection<string> guesses, string? reason) Solve(
        DateOnly publicationDate
    )
    {
        var seed = GetSeed(publicationDate);
        var random = new Random(seed);
        return Solve(random);
    }

    public (string? solution, IReadOnlyCollection<string> guesses, string? failureReason) Solve(
        Random random
    )
    {
        var allWords = WordListReader.EnumerateLines().ToArray();
        var remainingWords = allWords.ToArray();
        var solution = Enumerable.Repeat(' ', WordLength).ToArray();
        var guesses = new List<string>(MaxAttempts);
        var numAttempts = 0;

        while (numAttempts < MaxAttempts)
        {
            var remainingAttempts = MaxAttempts - numAttempts++;
            int[]? feedbackIndexesToProcess = null;

            string? guess = null;
            if (remainingWords.Length == 1)
            {
                guess = remainingWords.First();
            }
            else if ( // severe risk of exhaustion check
                feedbackProvider is DynamicFeedbackProvider
                && WordLength - solution.Count(c => c != ' ') == 1 // only one character left to solve
                && remainingWords.Length > remainingAttempts // exhaustion of attempts possible
                && remainingAttempts > 1 // this technique requires at least 2 attempts to work
            )
            {
                // Find a word that contains the most unsolved characters to maximize the number of words eliminated
                var unsolvedCharPosition = Array.IndexOf(solution, ' ');
                var unsolvedCharCandidates = remainingWords
                    .Select(w => w[unsolvedCharPosition])
                    .Distinct()
                    .ToArray();

                guess = allWords //TODO: use full wordle guess list here, not just the solution list
                    .GroupBy(word =>
                        unsolvedCharCandidates.Count(u =>
                            !solution.Contains(u) // favour characters not yet in the solution
                            && word.Count(c => c == u) == 1 // favour characters with unique occurrences in the word to maximize elimination scope
                        )
                    )
                    .MaxBy(g => g.Key)! // group matching most criteria
                    .RandomElement(random);

                if (guess != null)
                {
                    feedbackIndexesToProcess = Enumerable
                        .Range(0, guess.Length)
                        .Where(i => unsolvedCharCandidates.Contains(guess[i]))
                        .ToArray();
                }
            }

            // Fallback to the original approach: guess the word that eliminates the most possibilities
            guess ??= GetNextWords(solution, remainingWords).RandomElement(random)!;

            guesses.Add(guess);

            _console.WriteLine(
                $"Suggestion $magenta({numAttempts}): $green({guess.ToUpper()}) - out of $magenta({"possibility".ToQuantity(remainingWords.Length)})"
            );

            var feedback = feedbackProvider.GetFeedback(guess, remainingWords.Length);
            if (feedback == null)
            {
                return (null, guesses, "failed to acquire feedback for guess");
            }
            if (feedback == SolvedFeedback)
            {
                return (guess, guesses, null);
            }

            var operations = feedback
                .Zip(guess)
                .Select((x, i) => (f: x.First, c: x.Second, i))
                .Where(x =>
                    feedbackIndexesToProcess == null
                        ? solution[x.i] != x.c // skip already solved positional indexes
                        : feedbackIndexesToProcess.Contains(x.i) // process only specific indexes
                )
                .OrderBy(x => x.f); // ensures processing order 'c' -> 'm' -> 'n'

            var misplacedChars = new HashSet<char>();

            foreach (var (f, c, i) in operations)
            {
                switch (f)
                {
                    case FeedbackOption.Correct:
                        solution[i] = c;
                        remainingWords = remainingWords.Where(w => w[i] == c).ToArray();
                        break;
                    case FeedbackOption.Misplaced:
                        misplacedChars.Add(c);
                        var unsolvedIndexes = Enumerable
                            .Range(0, WordLength)
                            .Where(j => j != i && solution[j] == ' ');
                        remainingWords = remainingWords
                            .Where(w => w[i] != c && unsolvedIndexes.Any(u => w[u] == c))
                            .ToArray();
                        break;
                    case FeedbackOption.NoMoreOccurrences:
                        if (misplacedChars.Contains(c))
                        {
                            // skip if same character misplaced elsewhere
                            // required for seed test to pass: [InlineData(20241295, "mambo")]
                            break;
                        }

                        for (var j = 0; j < WordLength && remainingWords.Length > 1; j++)
                        {
                            if (solution[j] == ' ')
                            {
                                remainingWords = remainingWords.Where(w => w[j] != c).ToArray();
                            }
                        }
                        break;
                }

                if (remainingWords.Length <= 1)
                {
                    break; // no need to process further feedback as either solution now known or no solution exists
                }
            }

            if (remainingWords.Length == 0)
            {
                _console.WriteLine("$red(No remaining words, check input)");
                return (null, guesses, "algorithm failure, no remaining words available");
            }

            if (remainingWords.Length == 1)
            {
                continue;
            }

            // Scan remaining words to see if there are any common characters at unsolved positional indexes
            var firstRemainingWord = remainingWords[0];
            Enumerable
                .Range(0, WordLength)
                .Where(i =>
                    solution[i] == ' ' && remainingWords.All(w => w[i] == firstRemainingWord[i])
                )
                .ToList()
                .ForEach(i => solution[i] = remainingWords.First()[i]); // mark common positional character as solved
        }

        return (null, guesses, "maximum attempts reached without solution");
    }

    private static string[] GetNextWords(
        char[] solution,
        IReadOnlyCollection<string> remainingWords
    )
    {
        var filteredWords = remainingWords.ToArray();

        var remainingIndexes = solution
            .Select((c, i) => (c, i))
            .Where(x => x.c == ' ')
            .Select(x => x.i)
            .ToHashSet();

        while (remainingIndexes.Count > 0 && filteredWords.Length > 1)
        {
            var next = remainingIndexes
                .Select(i => // find the most commonly occurring character in filteredWords at position i
                    (
                        i,
                        x: filteredWords
                            .GroupBy(w => w[i], (key, words) => (i, c: key, n: words.Count()))
                            .MaxBy(x => x.n)
                    )
                )
                .MaxBy(y => y.x.n) // find the most common character across all positions
                .x;

            // filter remaining words by those having the most popular character
            filteredWords = filteredWords.Where(w => w[next.i] == next.c).ToArray();

            remainingIndexes.Remove(next.i); // mark position as visited and repeat
        }

        return filteredWords;
    }

    internal static int GetSeed(DateOnly publicationDate) =>
        publicationDate.Year * 10000 + publicationDate.Month * 100 + publicationDate.Day;
}
