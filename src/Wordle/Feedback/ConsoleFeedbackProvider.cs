using System.Net;
using Wordle.Interaction;

namespace Wordle.Feedback;

internal sealed class ConsoleFeedbackProvider(IConsole console) : IFeedbackProvider
{
    public string? GetFeedback(Word guess, int remainingWordCount)
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
                    $"Invalid feedback '$red({feedback})'; expected {Solver.WordLength} chars, got {feedback.Length}."
                );
                continue;
            }

            var invalidCharInfo = feedback
                .Select(PositionalChar.Create)
                .FirstOrDefault(x => FeedbackOption.IsInvalid(x.Char));

            if (!invalidCharInfo.IsValid())
            {
                return feedback;
            }
            
            console.WriteLine(
                $"Invalid feedback '$red({feedback})'; contains invalid char '$red({invalidCharInfo.Char})' at position {invalidCharInfo.Position + 1}. Use only letters $yellow(C), $yellow(M) or $yellow(N)."
            );

            var padding = new string(' ', invalidCharInfo.Position + 18);
            console.WriteLine($"{padding}$red({Unicode.UpArrow})");
        }
    }

    private readonly struct PositionalChar(char c, int position)
    {
        public char Char { get; } = c;
        public int Position { get; } = position;

        public static PositionalChar Create(char c, int i) => new(c, i);

        public bool IsValid() => char.IsLetter(Char);
    }
}
