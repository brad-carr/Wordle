namespace Wordle.Core;

public readonly struct BitMask
{
    private readonly ulong _value;

    public BitMask()
    {
    }
    
    private BitMask(ulong value) => _value = value;

    public bool IsEmpty => _value == 0;

    public BitMask Set(int index) => new (_value | (1UL << index));
    
    public bool this[int index] => ((1UL << index) & _value) > 0;
}