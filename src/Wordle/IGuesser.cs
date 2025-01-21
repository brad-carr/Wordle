namespace Wordle;

public interface IGuesser
{
    Word Guess(
        Random random, 
        Word partialSolution, 
        Word[] remainingWords, 
        Knowledge knowledge,
        int attempt, 
        int remainingAttempts);
}