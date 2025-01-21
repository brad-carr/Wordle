using Wordle.Core;

namespace Wordle;

public sealed class NewGuesser : IGuesser
{
    private readonly Word[] _guessWords = WordListReader.EnumerateGuessWords().Select(Word.Create).ToArray();
    private readonly Word _startingGuess = Word.Create("aurei");
    
    public Word Guess(
        Random random, 
        Word partialSolution, 
        Word[] remainingWords, 
        Knowledge knowledge,
        int attempt,
        int remainingAttempts)
    {
        if (attempt == 1)
        {
            return _startingGuess;
        }
        
        IEnumerable<Word> reduced = _guessWords;

        if (knowledge.CharsNotInSolution.HasSetBits)
        {
            reduced = reduced.Where(word => !word.ContainsAny(knowledge.CharsNotInSolution)).ToArray();
        }
        
        if (partialSolution != Word.Empty)
        {
            var unsolvedChars = partialSolution
                .UnsolvedPositions()
                .Aggregate(
                    new BitMask(), 
                    (mask, i) => remainingWords.Aggregate(mask, (current, word) => current.Set(word[i])));

            // Include all words having any unsolved characters
            reduced = reduced.Where(word => word.Any(c => unsolvedChars.IsSet(c))).ToArray();
        }
        
        // determine the group of all guess words having the greatest elimination power
        reduced = reduced
            .GroupBy(
                guessWord => remainingWords.Count(word => word.HasCommonChars(guessWord)),
                (_, words) => words.ToArray())
            .MaxBy(words => words.Length)!
            .ToArray();
        
        // from this group extract the subset of words that share the least number of letters with partialSolution
        reduced = reduced.GroupBy(word => word.CountCommonChars(partialSolution)).MinBy(g => g.Key)!.ToArray();

        // now reduce further choosing words having the most distinct characters
        reduced = reduced.GroupBy(word => word.Distinct().Count()).MaxBy(g => g.Key)!.ToArray();

        // TODO: exclude words having forbidden chars in specified slots
        reduced = reduced.Where(word => word.DoesNotContainForbiddenChars(knowledge.ForbiddenCharsBySlot)).ToArray();

        // Reduce to words having the fewest number of banned chars in unsolved slots
        reduced = reduced
            .GroupBy(word => partialSolution.UnsolvedPositions().Count(i => knowledge.ForbiddenCharsBySlot[i].IsSet(word[i])))
            .MinBy(g => g.Key)!
            .ToArray();
        
        var guess = reduced.RandomElement(random);
        return guess;
    }
}