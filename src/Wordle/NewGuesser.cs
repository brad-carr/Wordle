using Wordle.Core;

namespace Wordle;

public sealed class NewGuesser : IGuesser
{
    private readonly Word[] _guessWords = WordListReader.GuessWords;
    private readonly HashSet<Word> _solutionWords = WordListReader.SolutionWords.ToHashSet();
    private static readonly Word DefaultInitialGuess = "trace"; // "aurei";
    private static readonly BitMask Vowels = Word.Create("aeiou").UniqueChars;
    private static readonly BitMask Consonants = ~Vowels;

    public Word InitialGuess { get; set; } = DefaultInitialGuess;
    
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
            return InitialGuess;
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

        return highestScoringWords
            .GroupBy(word => word.CalculateEliminationPower(partialSolution, remainingWords))
            .MaxBy(k => k.Key)!
            .RandomElement(random);
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

            score += 5;
            
            if (c == mostCommonCharPerSlot[i])
            {
                score += 43; // maximum elimination potential
            }

            if (!charsAlreadySeen.IsSet(c))
            {
                score += 30; // favour new letters
            }
            
            if (knowledge.MaybeCharsBySlot[i].IsSet(c))
            {
                score += 15; // improved chance of success
            }

            if (!partialSolution.Contains(c))
            {
                score += 11; // favour unique chars in solution
            }

            if (Consonants.IsSet(c))
            {
                score += 3; // favour consonants
            }

            charsAlreadySeen = charsAlreadySeen.Set(c);
        }

        if (_solutionWords.Contains(word))
        {
            score += 10; // favour words in solution list => improve chances if near to a solution
        }

        return score;
    }
}