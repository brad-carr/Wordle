using System.Collections;
using System.Diagnostics;

namespace Wordle;

[DebuggerDisplay("{ToString()} ({_bits})")]
public readonly struct Word : IReadOnlyList<byte>, IEquatable<Word>
{
    private const byte CharCount = 26;
    internal const byte FirstChar = 1;
    private const byte BitsPerChar = 5;
    private const byte CharMask = (1 << BitsPerChar) - 1;
    private const uint ULCharMask = CharMask;
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

    public static Word Empty { get; } = new(0U);

    private readonly uint _bits;

    private Word(uint bits) => _bits = bits;

    public int Count => Solver.WordLength;

    public int Length => Solver.WordLength;

    public static Word Create(string word)
    {
        var bits = 0U;
        var i = Solver.WordLength;
        unchecked
        {
            while (i-- > 0)
            {
                var c = word[i];
                bits <<= BitsPerChar;

                if (c != ' ')
                {
                    bits |= c & ULCharMask; // case-insensitive
                }
            }
        }

        return new Word(bits);
    }

    public bool ContainsLetterAtPositionsOtherThan(byte findChar, int positionToIgnore)
    {
        var bits = _bits;
        unchecked
        {
            for (var i = 0; i < Solver.WordLength; i++)
            {
                if (i != positionToIgnore && (bits & ULCharMask) == findChar)
                {
                    return true;
                }

                bits >>= BitsPerChar;
            }
        }

        return false;
    }

    public bool Contains(byte findChar)
    {
        var bits = _bits;
        unchecked
        {
            for (var i = 0; i < Solver.WordLength; i++)
            {
                if ((bits & ULCharMask) == findChar)
                {
                    return true;
                }

                bits >>= BitsPerChar;
            }
        }

        return false;
    }

    public bool ContainsOnce(byte findChar, out int foundPos)
    {
        foundPos = -1;
        var bits = _bits;

        unchecked
        {
            for (var i = 0; i < Solver.WordLength; i++)
            {
                if ((bits & ULCharMask) == findChar)
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
        var clearMask = ~(ULCharMask << shift);
        return new Word(_bits & clearMask | positionalChar);
    }

    public override string ToString()
    {
        return string.Create(Solver.WordLength, _bits, (span, bits) =>
        {
            for (var i = 0; i < Solver.WordLength; i++)
            {
                var b = bits & ULCharMask;
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
}