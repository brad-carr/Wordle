namespace Wordle;

public static class Linq
{
    public static T? RandomElement<T>(this IEnumerable<T> items)
        where T : class
    {
        return items.RandomElement(new Random());
    }

    /// <summary>Chooses a random element from a sequence, using only a single pass of the sequence.</summary>
    /// <remarks>Uses the Fisher-Yates reservoir sampling algorithm.</remarks>
    public static T? RandomElement<T>(this IEnumerable<T> items, Random rand)
        where T : class
    {
        var i = 0;
        T? selection = null;
        foreach (var item in items)
        {
            var next = rand.Next(++i);
            if (next == 0)
            {
                selection = item;
            }
        }
        return selection;
    }
}
