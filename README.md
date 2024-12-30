# WordleSolver

## Overview

This C# program implements a basic algorithm for solving Wordle puzzles issued daily by the New York Times. See [link](https://www.nytimes.com/games/wordle/index.html).

## Algorithm

The program starts by taking a copy of the dictionary of possible words located in the file [wordlist.txt](https://github.com/brad-carr/WordleSolver/blob/master/wordlist.txt), which becomes the initial list of remaining words.

In each iteration, the program partitions the remaining words by the number of distinct characters and chooses one at random from the partition with the highest cardinality. This is done to maximize the number of words that can be eliminated from the remaining word list for the next iteration.

The program outputs this suggested word and asks the user to provide feedback for each of the positional letters. This feedback is then used to reduce the remaining word list in preparation for the next iteration.

The program stops when either the remaining word list reduces to a single element or it correctly selects the solution at random from the list of remaining words.

## Example Output

Solution to #1290 published on December 30, 2024; _correctly guessing the solution on the 2nd iteration_:

```csv
Suggestion 1: ROUSE - out of 2315 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? mnnmc
Suggestion 2: STARE - out of 10 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? ccccc
Solved on the 2nd attempt
```

Solution to #1289 published on December 29, 2024; _reducing to a single solution on the 4th iteration_:

```csv
Suggestion 1: FLINT - out of 2315 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? nnnnn
Suggestion 2: HOVER - out of 535 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? nmnnn
Suggestion 3: GUMBO - out of 11 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? nnccc
Suggestion 4: MAMBO - out of 1 possibility
Solved on the 4th attempt
```

Solution to #1288 published on December 28, 2024: _correctly guessing the solution on the 3rd iteration_:

```csv
Suggestion 1: WHINE - out of 2315 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? nnnnm
Suggestion 2: CAGEY - out of 306 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? mnnmc
Suggestion 3: DECRY - out of 3 possibilities
Feedback - [C]orrect [M]isplaced [N]o more occurrences? ccccc
Solved on the 3rd attempt
```
