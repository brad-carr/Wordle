using Humanizer;
using Wordle;
using Wordle.Feedback;
using Wordle.Interaction;

var console = new RicherConsole(new ConsoleWrapper());
var feedbackProvider = new ConsoleFeedbackProvider(console);
var guesser = new Guesser();
var solver = new Solver(console, guesser, feedbackProvider);
var (solution, guesses, failureReason) = 
    solver.Solve(DateOnly.FromDateTime(DateTime.Today));

console.WriteLine(
    solution == null
        ? $"No solution found because: $red({failureReason})"
        : $"$rainbow(Solved after {"guess".ToQuantity(guesses.Count)})"
);
