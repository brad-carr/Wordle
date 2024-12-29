namespace Wordle;

public static class Linq{
    
    /// <summary>Fisher-Yates Reservoir Sampling</summary>
    public static T? RandomElement<T>(this IEnumerable<T> items, Random? rand = null) where T : class{
        var i = 0;
        rand ??= new Random();
        T? selection = null;
        foreach (var item in items){
            if (rand.Next(i++) == 0){
                selection = item;
            }
        }
        return selection;
    }
}
