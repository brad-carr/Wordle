using System.Collections;
using System.Diagnostics;
using Wordle.Core;

namespace Wordle;

[DebuggerDisplay("{ToString()} ({_bits})")]
public readonly struct Word : IReadOnlyList<byte>, IEquatable<Word>
{
    private const byte CharCount = 26;
    private const byte FirstChar = 1;
    private const byte BitsPerChar = 5;
    private const byte CharMask = (1 << BitsPerChar) - 1;
    private const uint UCharMask = CharMask;
    private const char Space = ' ';
    public static IReadOnlyCollection<byte> Alphabet { get; } = GenerateAlphabet();

    private static byte[] GenerateAlphabet()
    {
        var result = new byte[CharCount];
        var next = FirstChar;
        for (var i = 0; i < CharCount; i++)
        {
            result[i] = next++;
        }

        return result;
    }

    public static Word Empty { get; } = new();

    private readonly uint _bits;
    private readonly BitMask _uniqueChars;

    private Word(uint bits, BitMask uniqueChars)
    {
        _bits = bits;
        _uniqueChars = uniqueChars;
    }
    
    public BitMask UniqueChars => _uniqueChars;
    public int Count => Solver.WordLength;

    public int Length => Solver.WordLength;

    public static Word Create(string word)
    {
        var bits = 0U;
        var mask = new BitMask();
        var i = Solver.WordLength;
        unchecked
        {
            while (i-- > 0)
            {
                var c = word[i];
                bits <<= BitsPerChar;

                if (c == ' ') // space
                {
                    continue;
                }
                
                var b = c & UCharMask; // case-insensitive
                bits |= b;
                mask = mask.Set((int)b);
            }
        }

        return new Word(bits, mask);
    }

    // TODO: test
    public bool ContainsAny(BitMask charMask) => (_uniqueChars & charMask).HasSetBits;

    public bool Contains(byte findChar) => _uniqueChars.IsSet(findChar);

    public bool ContainsOnce(byte findChar, out int foundPos)
    {
        foundPos = -1;
        var bits = _bits;

        unchecked
        {
            for (var i = 0; i < Solver.WordLength; i++)
            {
                if ((bits & CharMask) == findChar)
                {
                    if (foundPos >= 0)
                    {
                        return false;
                    }

                    foundPos = i;
                }

                bits >>= BitsPerChar;
            }
        }

        return foundPos >= 0;
    }

    public byte this[int pos] 
    {
        get
        {
            unchecked
            {
                return (byte)((_bits >> pos * BitsPerChar) & CharMask);    
            }
        }
    }

    public Word SetCharAtPos(byte charToSet, int pos)
    {
        var shift = pos * BitsPerChar;
        var positionalChar = (uint)charToSet << shift;
        var clearMask = ~(UCharMask << shift);
        return new Word(_bits & clearMask | positionalChar, _uniqueChars.Set(charToSet));
    }

    public override string ToString()
    {
        return string.Create(Solver.WordLength, _bits, (span, bits) =>
        {
            for (var i = 0; i < Solver.WordLength; i++)
            {
                var b = bits & CharMask;
                span[i] = b == 0 ? Space : (char)(b + 'a' -1);
                bits >>= BitsPerChar;
            }
        });
    }

    public IEnumerator<byte> GetEnumerator()
    {
        var bits = _bits;
        for (var i = 0; i < Solver.WordLength; i++)
        {
            yield return (byte)(bits & CharMask);
            bits >>= BitsPerChar;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(Word other) => _bits == other._bits;

    public override bool Equals(object? obj) => obj is Word other && Equals(other);

    public override int GetHashCode() => _bits.GetHashCode();

    public static bool operator ==(Word first, Word second) => first.Equals(second);

    public static bool operator !=(Word first, Word second) => !(first == second);

    public bool HasCharsInCommon(Word other) => ContainsAny(other._uniqueChars);

    public int CountCommonChars(Word other) => CountCommonChars(other._uniqueChars);

    public int CountCommonChars(BitMask charSet) => (_uniqueChars & charSet).Count;

    // TODO: add tests
    public BitMask UnsolvedPositions()
    {
        var result = new BitMask();
        var bits = _bits;
        for (var i = 0; i < Solver.WordLength; i++)
        {
            if ((bits & CharMask) == 0)
            {
                result = result.Set(i);
            }

            bits >>= BitsPerChar;
        }

        return result;
    }
}