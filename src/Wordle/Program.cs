using Humanizer;
using Wordle;

var console = new RicherConsole();
var feedbackProvider = new ConsoleFeedbackProvider(console);
var solver = new Solver(console, feedbackProvider);
var (solution, numGuesses) = solver.Solve(DateOnly.Parse("2024-12-29"));

if (solution != null)
{
    console.WriteLine($"$rainbow(Solved after {"guess".ToQuantity(numGuesses)})");
}
else
{
    console.WriteLine("No solution found");
}
