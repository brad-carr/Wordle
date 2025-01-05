using FluentAssertions;
using Wordle.Core;

namespace Wordle.UnitTests;

public sealed class BitMaskTests
{
    [Fact]
    public void InitialValue_Empty()
    {
        var sut = new BitMask();
        sut.IsEmpty.Should().BeTrue();
        for (var i = 0; i < 32; i++)
        {
            sut[i].Should().BeFalse();
        }
    }

    [Fact]
    public void Set_CreatesANewBitMask()
    {
        var bm1 = new BitMask();
        var bm2 = bm1.Set(7).Set(3).Set(63);
        
        bm1.IsEmpty.Should().BeTrue();
        bm2.IsEmpty.Should().BeFalse();
        
        for (var i = 0; i < 64; i++)
        {
            bm1[i].Should().BeFalse();

            switch (i)
            {
                case 3:
                case 7:
                case 63:
                    bm2[i].Should().BeTrue();
                    break;
                default:
                    bm2[i].Should().BeFalse();
                    break;
            }
        }
    }
}