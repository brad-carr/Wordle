using Wordle.Core;

namespace Wordle;

public sealed class Guesser : IGuesser
{
    private readonly Dictionary<byte, Word[]> _guessWordsBySingleOccurringLetter;
    
    public Guesser()
    {
        var guessWords = WordListReader.EnumerateGuessWords().Select(Word.Create).ToArray();
        _guessWordsBySingleOccurringLetter = Word.Alphabet.ToDictionary(
            c => c,
            c => guessWords.Where(w => w.ContainsOnce(c, out _)).ToArray());
    }
    
    public Word Guess(
        Random random, 
        Word partialSolution, 
        Word[] remainingWords, 
        int remainingAttempts)
    {
        if (remainingWords.Length == 1)
        {
            return remainingWords[0];
        }
        
        Word? maybeGuess = null;
        if ( // severe risk of exhaustion check
            remainingAttempts > 1 // this technique requires at least 2 attempts to work
            && partialSolution.ContainsOnce(0, out var unsolvedCharPosition)
            && remainingWords.Length > 2) // inefficient to try one letter at a time if more than two possibilities
        {
            maybeGuess = GuessForSinglePosition(partialSolution, remainingWords, unsolvedCharPosition, random);
        }

        // Fallback to the original approach: guess the word that eliminates the most possibilities
        return maybeGuess ?? GetNextWords(partialSolution, remainingWords).RandomElement(random);
    }

    private Word? GuessForSinglePosition(
        Word partialSolution, 
        Word[] remainingWords, 
        int unsolvedCharPosition, 
        Random random)
    {
        // Find a word that contains the most unsolved characters to maximize the number of words possibly eliminated
        var unsolvedCharMask = remainingWords
            .Where(word => !partialSolution.Contains(word[unsolvedCharPosition]))
            .Aggregate(
                BitMask.Empty,
                (current, word) => current.Set(word[unsolvedCharPosition]));
                
        var maybeGuess = unsolvedCharMask
            .SelectMany(c => _guessWordsBySingleOccurringLetter[c])
            .Distinct()
            .GroupBy(word => unsolvedCharMask.CountSetBitsWhere(word.Contains))
            .MaxBy(g => g.Key)? // group matching most criteria
            .RandomElement(random);
        return maybeGuess;
    }

    private static Word[] GetNextWords(Word solution, Word[] remainingWords)
    {
        var remainingIndexes = new BitMask();
        for (var i = 0; i < Solver.WordLength; i++)
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
}