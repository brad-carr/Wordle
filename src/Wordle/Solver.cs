using System.Diagnostics.CodeAnalysis;
using Humanizer;
using Wordle.Core;
using Wordle.Feedback;
using Wordle.Interaction;

namespace Wordle;

public sealed class Solver
{
    public const int WordLength = 5;
    public const int DefaultMaxAttempts = 6;

    internal static readonly string SolvedFeedback = new('c', WordLength);

    private readonly Dictionary<byte, Word[]> _guessWordsBySingleOccurringLetter;
    private readonly IFeedbackProvider _feedbackProvider;
    private readonly Word[] _solutionWordList;
    private readonly IConsole _console;

    public Solver(
        IConsole console,
        IFeedbackProvider feedbackProvider,
        string[] solutionWordList,
        string[] guessWordList
    )
    {
        var guessWords = guessWordList.Select(Word.Create).ToArray();
        
        (_console, _feedbackProvider, _solutionWordList, _guessWordsBySingleOccurringLetter) = (
            console,
            feedbackProvider,
            solutionWordList.Select(Word.Create).ToArray(),
            Word.Alphabet.ToDictionary(
                c => c,
                c => guessWords.Where(w => w.ContainsOnce(c, out _)).ToArray())
        );
    }

    public Solver(IConsole console, IFeedbackProvider feedbackProvider)
        : this(console, feedbackProvider, WordListReader.EnumerateSolutionWords().ToArray(), WordListReader.EnumerateGuessWords().ToArray()) { }

    public (Word? solution, IReadOnlyCollection<Word> guesses, string? reason) Solve(
        DateOnly publicationDate, 
        int maxAttempts = DefaultMaxAttempts
    )
    {
        var seed = GetSeed(publicationDate);
        var random = new Random(seed);
        return Solve(random, maxAttempts);
    }

    public (Word? solution, IReadOnlyCollection<Word> guesses, string? failureReason) Solve(
        Random random, 
        int maxAttempts = DefaultMaxAttempts
    )
    {
        var remainingWords = _solutionWordList;
        var solution = Word.Empty;
        var guesses = new List<Word>(maxAttempts);
        var numAttempts = 0;
        var isDynamicFeedbackProvider = _feedbackProvider is DynamicFeedbackProvider;

        while (numAttempts < maxAttempts)
        {
            var remainingAttempts = maxAttempts - numAttempts++;
            var (guess, feedbackIndexesToProcess) = Guess(random, remainingWords, isDynamicFeedbackProvider, remainingAttempts, solution);
            guesses.Add(guess);

            _console.WriteLine(
                $"Suggestion $magenta({numAttempts}): $green({guess}) - out of $magenta({"possibility".ToQuantity(remainingWords.Length)})"
            );

            var feedback = _feedbackProvider.GetFeedback(guess, remainingWords.Length);
            if (feedback == null)
            {
                return (null, guesses, "failed to acquire feedback for guess");
            }
            if (feedback == SolvedFeedback)
            {
                return (guess, guesses, null);
            }

            var solutionCopy = solution; 
            var operations = feedback
                .Zip(guess)
                .Select((x, i) => (f: x.First, c: x.Second, i))
                .Where(x =>
                    feedbackIndexesToProcess.IsEmpty
                        ? solutionCopy[x.i] != x.c // skip already solved positional indexes
                        : feedbackIndexesToProcess.IsSet(x.i) // process only specific indexes 
                )
                .OrderBy(x => x.f); // ensures processing order 'c' -> 'm' -> 'n'

            var misplacedCharIndexes = new BitMask();
            foreach (var (f, c, i) in operations)
            {
                switch (f)
                {
                    case FeedbackOption.Correct:
                        solution = solution.SetCharAtPos(c, i);
                        remainingWords = remainingWords.Where(w => w[i] == c).ToArray();
                        break;
                    case FeedbackOption.Misplaced:
                        misplacedCharIndexes = misplacedCharIndexes.Set(c - 'a');
                        var unsolvedIndexes = Enumerable
                            .Range(0, WordLength)
                            .Where(j => j != i && solution[j] == 0)
                            .ToArray();
                        remainingWords = remainingWords
                            .Where(w => w[i] != c && unsolvedIndexes.Any(u => w[u] == c))
                            .ToArray();
                        break;
                    case FeedbackOption.NoMoreOccurrences:
                        if (misplacedCharIndexes.IsSet(c - 'a'))
                        {
                            // skip if same character misplaced elsewhere
                            // required for seed test to pass: [InlineData(20241295, "mambo")]
                            break;
                        }

                        for (var j = 0; j < WordLength && remainingWords.Length > 0; j++)
                        {
                            if (solution[j] == 0)
                            {
                                remainingWords = remainingWords.Where(w => w[j] != c).ToArray();
                            }
                        }
                        break;
                }

                if (remainingWords.Length == 0)
                {
                    break; // word missing from dictionary or potential bug
                }
            }

            switch (remainingWords.Length)
            {
                case 0:
                    _console.WriteLine("$red(No remaining words, check input)");
                    return (null, guesses, "algorithm failure, no remaining words available");
                case 1:
                    continue;
            }

            AddCommonPositionalCharsToSolution(remainingWords, ref solution);
        }

        return (null, guesses, "maximum attempts reached without solution");
    }

    [return: NotNull]
    private (Word, BitMask) Guess(Random random, Word[] remainingWords, bool isDynamicFeedbackProvider, int remainingAttempts,
        Word solution)
    {
        Word? maybeGuess = null;
        if (remainingWords.Length == 1)
        {
            return (remainingWords[0], BitMask.Empty);
        }
        
        var feedbackIndexesToProcess = new BitMask();
        if ( // severe risk of exhaustion check
            isDynamicFeedbackProvider
            && remainingAttempts > 1 // this technique requires at least 2 attempts to work
            && solution.ContainsOnce(0, out var unsolvedCharPosition)
            && remainingWords.Length > 2) // inefficient to try one letter at a time if more than two possibilities
        {
            // Find a word that contains the most unsolved characters to maximize the number of words possibly eliminated
            var unsolvedCharMask = remainingWords
                .Where(word => !solution.Contains(word[unsolvedCharPosition]))
                .Aggregate(
                    BitMask.Empty,
                    (current, word) => current.Set(word[unsolvedCharPosition]));
                
            maybeGuess = unsolvedCharMask
                .SelectMany(c => _guessWordsBySingleOccurringLetter[c])
                .Distinct()
                .GroupBy(word => unsolvedCharMask.CountSetBitsWhere(word.Contains))
                .MaxBy(g => g.Key)? // group matching most criteria
                .RandomElement(random);

            if (maybeGuess.HasValue)
            {
                for (var i = 0; i < WordLength; i++)
                {
                    if (unsolvedCharMask.IsSet(maybeGuess.Value[i]))
                    {
                        feedbackIndexesToProcess = feedbackIndexesToProcess.Set(i);
                    }
                }
            }
        }

        // Fallback to the original approach: guess the word that eliminates the most possibilities
        var guess = maybeGuess ?? GetNextWords(solution, remainingWords).RandomElement(random);
        return (guess, feedbackIndexesToProcess);
    }

    private static void AddCommonPositionalCharsToSolution(Word[] remainingWords, ref Word solution)
    {
        unchecked
        {
            var firstRemainingWord = remainingWords[0];

            for (var i = 0; i < WordLength; i++)
            {
                if (solution[i] > 0)
                {
                    continue;
                }

                var allMatch = true;
                var charToMatch = firstRemainingWord[i];
                for (var j = 1; j < remainingWords.Length; j++)
                {
                    if (remainingWords[j][i] == charToMatch) continue;
                    allMatch = false;
                    break;
                }

                if (allMatch)
                {
                    solution = solution.SetCharAtPos(charToMatch, i);
                }
            }
        }
    }

    private static Word[] GetNextWords(Word solution, Word[] remainingWords)
    {
        var remainingIndexes = new BitMask();
        for (var i = 0; i < WordLength; i++)
        {
            if (solution[i] == 0)
            {
                remainingIndexes = remainingIndexes.Set(i);
            }
        }

        while (remainingIndexes.HasSetBits && remainingWords.Length > 1)
        {
            var next = remainingIndexes
                .Select(i => // find words matching the most commonly occurring character in remainingWords at position i
                    (
                        i,
                        matches: remainingWords
                            .GroupBy(w => w[i], (_, words) => words.ToArray())
                            .MaxBy(words => words.Length)!
                    )
                )
                .MaxBy(y => y.matches.Length); // find the most common character across all positions

            remainingWords = next.matches;
            remainingIndexes = remainingIndexes.Clear(next.i); // mark position as visited and repeat
        }

        return remainingWords;
    }

    internal static int GetSeed(DateOnly publicationDate) =>
        publicationDate.Year * 10000 + publicationDate.Month * 100 + publicationDate.Day;
}