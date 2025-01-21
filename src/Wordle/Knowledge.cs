using Wordle.Core;

namespace Wordle;

public readonly struct Knowledge(
    BitMask charsNotInSolution,
    BitMask[] forbiddenCharsBySlot)
{
    public BitMask CharsNotInSolution { get; } = charsNotInSolution;
    public BitMask[] ForbiddenCharsBySlot { get; } = forbiddenCharsBySlot;
}