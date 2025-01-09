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
        sut.HasSetBits.Should().BeFalse();
        for (var i = 0; i < 32; i++)
        {
            sut.IsSet(i).Should().BeFalse();
        }
    }

    [Fact]
    public void Set_CreatesANewBitMask()
    {
        var bm1 = new BitMask();
        var bm2 = bm1.Set(7).Set(3).Set(63);
        
        bm1.IsEmpty.Should().BeTrue();
        bm1.HasSetBits.Should().BeFalse();
        
        bm2.IsEmpty.Should().BeFalse();
        bm2.HasSetBits.Should().BeTrue();
        bm2.Count.Should().Be(3);
        
        for (var i = 0; i < 64; i++)
        {
            bm1.IsSet(i).Should().BeFalse();

            switch (i)
            {
                case 3:
                case 7:
                case 63:
                    bm2.IsSet(i).Should().BeTrue();
                    break;
                default:
                    bm2.IsSet(i).Should().BeFalse();
                    break;
            }
        }

        bm2.ToArray().Should().Equal(3, 7, 63);
    }
}