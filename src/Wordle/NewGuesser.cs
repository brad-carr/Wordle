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
        int attempt,
        int remainingAttempts)
    {
        if (attempt == 1)
        {
            return _startingGuess;
        }
        
        var wordsByEliminationPower = new Dictionary<int, List<Word>>();

        IEnumerable<Word> guessWords;
        if (partialSolution == Word.Empty)
        {
            guessWords = _guessWords;
        }
        else
        {
            var unsolvedChars = partialSolution
                .UnsolvedPositions()
                .Aggregate(
                    new BitMask(), 
                    (mask, i) => remainingWords.Aggregate(mask, (current, word) => current.Set(word[i])));

            guessWords = _guessWords.Where(word => word.Any(c => unsolvedChars.IsSet(c))).ToArray();
            
            // TODO: remove words containing characters known not to be present in the solution
        }
        
        foreach (var guessWord in guessWords)
        {
            var countEliminated = remainingWords.Count(word => word.Any(guessWord.Contains));
            if (!wordsByEliminationPower.TryGetValue(countEliminated, out var words))
            {
                words = [];
                wordsByEliminationPower.Add(countEliminated, words);
            } 
            
            words.Add(guessWord);
        }

        var optimalEliminationGroup = wordsByEliminationPower.MaxBy(kvp => kvp.Key);
        
        // from this group extract the subset of words that share the least number of letters with partialSolution
        var reduced = optimalEliminationGroup.Value.GroupBy(word => word.CountCommonChars(partialSolution)).MinBy(g => g.Key)!.ToArray();
        
        // now reduce further choosing words having the most distinct characters
        reduced = reduced.GroupBy(word => word.Distinct().Count()).MaxBy(g => g.Key)!.ToArray();
        
        // TODO: exclude words having misplaced letters in known slots
        
        // TODO: prefer words having misplaced letters in unsolved slots
        
        return reduced.RandomElement(random);
    }
}