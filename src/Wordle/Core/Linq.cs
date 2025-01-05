namespace Wordle;

internal static class Linq
{
    /// <summary>Chooses a random element from an array.</summary>
    /// <remarks>Uses the Fisher-Yates reservoir sampling algorithm.</remarks>
    public static T? RandomElement<T>(this T[] items, Random rand)
        where T : class =>
        items.Length switch
        {
            0 => null,
            1 => items[0],
            _ => RandomElement((IEnumerable<T>)items, rand),
        };

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

    /// <summary>Checks if a sequence contains a single instance of a given item.</summary>
    public static bool ContainsOnce<T>(this IEnumerable<T> items, T comparand, out int index)
        where T : IEquatable<T>
    {
        index = -1;
        int i = 0;
        foreach (var item in items)
        {
            if (item.Equals(comparand))
            {
                if (index >= 0)
                {
                    index = -1;
                    return false;
                }

                index = i;
            }
            i++;
        }

        return index >= 0;
    }
}
