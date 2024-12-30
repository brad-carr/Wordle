using Humanizer;

namespace Wordle
{
    public class ConsoleFeedbackProvider(IConsole console) : IFeedbackProvider
    {
        public string? GetFeedback(string suggestion, int remainingWordCount)
        {
            if (remainingWordCount == 1)
            {
                return Solver.SolvedFeedback;
            }

            while (true)
            {
                console.Write(
                    "Feedback - $yellow([C])orrect $yellow([M])isplaced $yellow([N])o more occurrences? "
                );
                var feedback = console.ReadLine();
                if (feedback == null)
                {
                    console.WriteLine("Null feedback; terminating.");
                    return null;
                }
                feedback = feedback.ToLower();
                if (feedback.Length != Solver.WordLength)
                {
                    console.WriteLine(
                        $"invalid feedback; expected {"character".ToQuantity(Solver.WordLength)}"
                    );
                    continue;
                }
                if (!feedback.All(Feedback.IsValid))
                {
                    console.WriteLine(
                        "invalid feedback; use only letters $yellow(C), $yellow(M) or $yellow(N)"
                    );
                    continue;
                }

                return feedback;
            }
        }
    }
}
