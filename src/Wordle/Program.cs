using Humanizer;
using Wordle;

var console = new RicherConsole(new ConsoleWrapper());
var feedbackProvider = new ConsoleFeedbackProvider(console);
var solver = new Solver(console, feedbackProvider);
var (solution, numGuesses) = solver.Solve(DateOnly.FromDateTime(DateTime.Today));

if (solution != null)
{
    console.WriteLine($"$rainbow(Solved after {"guess".ToQuantity(numGuesses)})");
}
else
{
    console.WriteLine("No solution found");
}
