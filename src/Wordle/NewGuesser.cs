using Wordle.Core;

namespace Wordle;

public sealed class NewGuesser : IGuesser
{
    private readonly Word[] _guessWords = WordListReader.EnumerateGuessWords().Select(Word.Create).ToArray();
    private readonly HashSet<Word> _solutionWords = WordListReader.EnumerateSolutionWords().Select(Word.Create).ToHashSet();
    private readonly Word _startingGuess = Word.Create("trace"); // Word.Create("aurei");
    private static readonly BitMask Vowels = Word.Create("aeiou").UniqueChars;
    
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

        var mostCommonCharPerSlot = partialSolution
            .UnsolvedPositions()
            .Aggregate(Word.Empty, (w, i) => 
                w.SetCharAtPos(remainingWords.CountBy(x => x[i]).MaxBy(kvp => kvp.Value).Key, i));
        
        var highestScoringWords = _guessWords
            .GroupBy(word => CalculateScore(word, partialSolution, mostCommonCharPerSlot, knowledge))
            .MaxBy(g => g.Key)!
            .ToArray();

        if (highestScoringWords.Length == 1)
        {
            return highestScoringWords[0];
        }

        var guessWordsByEliminationPower = highestScoringWords
            .GroupBy(word => CalculateEliminationPower(word, partialSolution, remainingWords))
            .ToArray();

        var powerfulGroup = guessWordsByEliminationPower
            .MaxBy(k => k.Key)!
            .ToArray();

        var winner = powerfulGroup.RandomElement(random);
        return winner;
    }

    private int CalculateScore(Word word, Word partialSolution, Word mostCommonCharPerSlot, Knowledge knowledge)
    {
        if (word.ToString() == "flava")
        {
            Console.WriteLine();
        }
        var score = 0;
        var charsAlreadySeen = knowledge.CharsAlreadySeen;
        for (var i = 0; i < word.Length; i++)
        {
            var c = word[i];

            if (partialSolution[i] == c)
            {
                continue; // Already solved at that slot, no new information gained
            }

            if (knowledge.CharsNotInSolution.IsSet(c))
            {
                continue; // Char not in solution, no new information to be gained by including it
            }

            if (knowledge.ForbiddenCharsBySlot[i].IsSet(c))
            {
                continue; // Already forbidden here, no new information gained
            }

            score += 1;
            
            if (!charsAlreadySeen.IsSet(c))
            {
                score += 3; // favour new letters
            }
            
            if (knowledge.MaybeCharsBySlot[i].IsSet(c))
            {
                score += 2; // improved chance of success
            }

            if (c == mostCommonCharPerSlot[i])
            {
                score += 4; // maximum elimination potential
            }

            if (!partialSolution.Contains(c))
            {
                score += 1; // favour unique chars in solution
            }

            if (!Vowels.IsSet(c))
            {
                score += 1; // favour consonants
            }

            charsAlreadySeen = charsAlreadySeen.Set(c);
        }

        if (_solutionWords.Contains(word))
        {
            score += 1; // favour words in solution list => improve chances if near to a solution
        }

        return score;
    }
    
    private static int CalculateEliminationPower(Word word, Word partialSolution, Word[] remainingWords)
    {
        IEnumerable<Word> reduced = remainingWords;
        foreach (var i in partialSolution.UnsolvedPositions())
        {
            var c = word[i];
            reduced = reduced.Where(w => w[i] != c);
        }

        return remainingWords.Length - reduced.Count();
    }
}