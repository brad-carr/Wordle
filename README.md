# WordleSolver

## Overview

This C# program implements an algorithm for solving the daily Wordle puzzles published by the New York Times. See the game at [link](https://www.nytimes.com/games/wordle/index.html).

## Algorithm

The program begins by loading a copy of possible words from [wordlist.txt](https://github.com/brad-carr/WordleSolver/blob/master/wordlist.txt) as the initial solution set.

The program uses two strategies to determine the next guess word:

### Main Strategy

- Identifies the most frequently occurring character in each unsolved position
- Filters words to those containing these most common positional characters
- Repeats this process for remaining unsolved positions until finding a suitable word

### Alternative Strategy

This strategy activates when all but one position is solved and remaining attempts are fewer than remaining possible words. This prevents exhausting attempts by trying each remaining letter individually.

Instead, the program finds a 5-letter word containing the most possible characters and uses it as the next guess, maximizing word elimination potential.

### Process Flow

After each guess, the program:

1. Requests user feedback for each letter position
2. Uses this feedback to filter the remaining word list
3. Continues to the next iteration

### Termination Conditions

The program ends when any of these conditions is met:

- Only one word remains (solution found)
- The correct solution is guessed by chance
- Six attempts are exhausted without finding the solution
- No valid words remain (indicating possible feedback errors or outdated word list)

## Example Output

Solution for Wordle #1290 (December 30, 2024) - solved in 2 attempts:

```csv
Suggestion 1: ROUSE - out of 2315 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? mnnmc
Suggestion 2: STARE - out of 10 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? ccccc
Solved on the 2nd attempt
```

Solution for Wordle #1289 (December 29, 2024) - solved in 4 attempts:

```csv
Suggestion 1: FLINT - out of 2315 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? nnnnn
Suggestion 2: HOVER - out of 535 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? nmnnn
Suggestion 3: BUXOM - out of 11 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? mnnmm
Suggestion 4: MAMBO - out of 1 possibility
Solved on the 4th attempt
```

Solution for Wordle #1288 (December 28, 2024) - solved in 3 attempts:

```csv
Suggestion 1: WHINE - out of 2315 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? nnnnm
Suggestion 2: CAGEY - out of 306 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? mnnmc
Suggestion 3: DECRY - out of 3 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? ccccc
Solved on the 3rd attempt
```

## Notes

Challenging seeds can be identified using the test `Solve_DynamicFeedback_MultipleSeeds_ShouldFindSolutionWithinSixAttempts`. Discovered problematic cases can be added to `Solve_DynamicFeedback_ProblematicSeeds_ShouldFindSolutionWithinSixAttempts` for investigation and algorithm optimization.

## Credits

1. [solution-word-list.txt](https://github.com/brad-carr/WordleSolver/blob/master/src/Wordle/solution-word-list.txt) was cloned from [Cyrus Freshman's](https://gist.github.com/cfreshman/a03ef2cba789d8cf00c08f767e0fad7b) word list with the following additions:
   - `atlas`, spotted on  7-Jan-25
   - `squid`, spotted on 20-Jan-25
1. [guess-word-list.txt](https://github.com/brad-carr/WordleSolver/blob/master/src/Wordle/guess-word-list.txt) was cloned from [M Somerville's](https://gist.github.com/dracos/dd0668f281e685bad51479e5acaadb93) word list.
