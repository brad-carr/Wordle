using Wordle.Core;

namespace Wordle;

public readonly struct Knowledge(
    BitMask charsNotInSolution,
    BitMask[] forbiddenCharsBySlot,
    BitMask[] maybeCharsBySlot)
{
    public BitMask CharsNotInSolution { get; } = charsNotInSolution;
    public BitMask[] ForbiddenCharsBySlot { get; } = forbiddenCharsBySlot;
    public BitMask[] MaybeCharsBySlot { get; } = maybeCharsBySlot;
}