using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Wordle.Core;

public readonly struct BitMask : IReadOnlyCollection<byte>
{
    public static BitMask Empty { get; } = new();

    private readonly ulong _value;

    public BitMask()
    {
    }

    private BitMask(ulong value) => _value = value;

    public bool IsEmpty => _value == 0;

    public int Count => BitOperations.PopCount(_value);

    public bool HasSetBits => _value != 0;

    public BitMask Set(int index) => new(_value | (1UL << index));

    public BitMask Clear(int index) => new(~(1UL << index) & _value); // Bmi1.X64.AndNot

    public bool IsSet(int index) => ((1UL << index) & _value) > 0;

    public int CountSetBitsWhere(Predicate<byte> criteria)
    {
        var count = 0;
        for (var x = _value; x != 0;)
        {
            if (criteria((byte)BitOperations.TrailingZeroCount(x)))
            {
                count++;
            }

            x = ResetLowestSetBit(x);
        }

        return count;
    }

    public IEnumerator<byte> GetEnumerator()
    {
        for (var x = _value; x != 0;)
        {
            yield return (byte)BitOperations.TrailingZeroCount(x);
            x = ResetLowestSetBit(x);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ResetLowestSetBit(ulong x) =>
        x & (x - 1UL); // Bmi1.X64.ResetLowestSetBit - leverages wraparound if x==0
}