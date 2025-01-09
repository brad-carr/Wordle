using System.Collections;
using System.Numerics;

namespace Wordle.Core;

public readonly struct BitMask : IReadOnlyCollection<int>
{
    private readonly ulong _value;

    public BitMask()
    {
    }
    
    private BitMask(ulong value) => _value = value;

    public bool IsEmpty => _value == 0;

    public int Count => BitOperations.PopCount(_value);

    public bool HasSetBits => _value > 0;
    
    public BitMask Set(int index) => new(_value | (1UL << index));

    public BitMask Clear(int index) => new (_value & ~(1UL << index));
    
    public bool IsSet(int index) => ((1UL << index) & _value) > 0;
    
    public IEnumerator<int> GetEnumerator()
    {
        var x = _value;
        while (x > 0)
        {
            var pos = BitOperations.TrailingZeroCount(x);
            yield return pos;
            x &= (x - 1);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}