namespace Wordle;

internal static class WordListReader
{
    public static IEnumerable<string> GuessWordLiterals() => EnumerateLines("guess-word-list.txt");
    public static IEnumerable<string> SolutionWordLiterals() => EnumerateLines("solution-word-list.txt");
    public static Word[] GuessWords { get; } = GuessWordLiterals().Select(Word.Create).ToArray();
    public static Word[] SolutionWords { get; } = SolutionWordLiterals().Select(Word.Create).ToArray();

    public static Word DeriveOptimalStartWord() => GuessWords
        .GroupBy(word => word.CalculateEliminationPower(Word.Empty, SolutionWords))!
        .MaxBy(g => g.Key)!
        .First();

    private static IEnumerable<string> EnumerateLines(string fileName)
    {
        var assembly =
            System.Reflection.Assembly.GetExecutingAssembly()
            ?? throw new Exception("assembly is null");
        var resourceName = $"Wordle.Resources.{fileName}";

        using var stream =
            assembly.GetManifestResourceStream(resourceName)
            ?? throw new Exception("resource not found");
        using var reader = new StreamReader(stream);

        while (true)
        {
            var content = reader.ReadLine();
            if (content == null)
            {
                yield break;
            }

            content = content.Trim();
            if (content.Length > 0)
            {
                yield return content;
            }
        }
    }
}
