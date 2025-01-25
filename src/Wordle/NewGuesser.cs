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

        var highestScoringWords = _guessWords
            .GroupBy(word => CalculateScore(word, partialSolution, knowledge))
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

    private int CalculateScore(Word word, Word partialSolution, Knowledge knowledge)
    {
        if (word.ToString() == "flava")
        {
            Console.WriteLine();
        }
        var score = 0;
        var charsAlreadySeen = new BitMask();
        for (var i = 0; i < word.Length; i++)
        {
            var c = word[i];

            if (partialSolution[i] == c)
            {
                continue; // Already solved at that slot, no new information gained
            }

            if (charsAlreadySeen.IsSet(c))
            {
                continue; // prefer uniqueness
            }
            charsAlreadySeen = charsAlreadySeen.Set(c);
            
            if (knowledge.CharsNotInSolution.IsSet(c))
            {
                continue; // Char not in solution, no new information to be gained by including it
            }

            if (knowledge.ForbiddenCharsBySlot[i].IsSet(c))
            {
                continue; // Already forbidden here, no new information gained
            }

            if (knowledge.MaybeCharsBySlot[i].IsSet(c))
            {
                score += 5; // real possibility letter could be in this slot => greater weight
            }
            else if (!partialSolution.Contains(c))
            {
                score += 3; // favour letters not yet seen
            }
            else
            {
                score += 1; // char in solution
            }

            if (!Vowels.IsSet(c))
            {
                score += 1; // favour consonants
            }
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