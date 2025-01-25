using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Wordle.Core;

[DebuggerDisplay("{ToString()} ({_value})")]
public readonly struct BitMask : IReadOnlyCollection<byte>, IEquatable<BitMask>
{
    public static BitMask Empty { get; } = new();

    private readonly uint _value;

    internal BitMask(uint value) => _value = value;

    public bool IsEmpty => _value == 0;

    public int Count => BitOperations.PopCount(_value);

    public bool HasSetBits => _value != 0;

    public BitMask Set(int index) => new(_value | (1U << index));

    public BitMask Clear(int index) => new(~(1U << index) & _value); // Bmi1.X64.AndNot

    public bool IsSet(int index) => ((1U << index) & _value) > 0;

    public IEnumerator<byte> GetEnumerator()
    {
        for (var x = _value; x != 0;)
        {
            yield return (byte)BitOperations.TrailingZeroCount(x);
            x = ResetLowestSetBit(x);
        }
    }

    // TODO test
    public override string ToString() => string.Create(Count, _value, (span, x) =>
    {
        var i = 0;
        while (x != 0)
        {
            span[i++] = (char)('a' + BitOperations.TrailingZeroCount(x) - 1);
            x = ResetLowestSetBit(x);
        }
    });

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ResetLowestSetBit(uint x) =>
        x & (x - 1U); // Bmi1.X64.ResetLowestSetBit - leverages wraparound if x==0

    public static BitMask operator ~(BitMask a) => new(~a._value);

    public static BitMask operator |(BitMask a, BitMask b) => new(a._value | b._value);

    public static BitMask operator &(BitMask a, BitMask b) => new(a._value & b._value);

    public bool Equals(BitMask other) => _value == other._value;

    public override bool Equals(object? obj) => obj is BitMask other && Equals(other);

    public override int GetHashCode() => _value.GetHashCode();

    public static bool operator ==(BitMask left, BitMask right) => left.Equals(right);

    public static bool operator !=(BitMask left, BitMask right) => !left.Equals(right);
}