namespace Wordle;

internal static class WordListReader
{
    public static IEnumerable<string> EnumerateGuessWords() => EnumerateLines("guess-word-list.txt");
    public static IEnumerable<string> EnumerateSolutionWords() => EnumerateLines("solution-word-list.txt");

    public static IEnumerable<string> EnumerateLines(string fileName)
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
