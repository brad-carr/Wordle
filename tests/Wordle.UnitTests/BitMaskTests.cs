using FluentAssertions;
using Microsoft.VisualBasic;
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
        var bm2 = bm1.Set(7).Set(0).Set(31);
        
        bm1.IsEmpty.Should().BeTrue();
        bm1.HasSetBits.Should().BeFalse();
        
        bm2.IsEmpty.Should().BeFalse();
        bm2.HasSetBits.Should().BeTrue();
        bm2.Count.Should().Be(3);
        
        for (var i = 0; i < 32; i++)
        {
            bm1.IsSet(i).Should().BeFalse();

            switch (i)
            {
                case 0:
                case 7:
                case 31:
                    bm2.IsSet(i).Should().BeTrue();
                    break;
                default:
                    bm2.IsSet(i).Should().BeFalse();
                    break;
            }
        }

        bm2.ToArray().Should().Equal(0, 7, 31);
    }

    [Theory]
    [InlineData(0xFFFF, 0xFFFF, 0xFFFF)]
    [InlineData(0xFFF0, 0x0FFF, 0x0FF0)]
    [InlineData(0xFF00, 0x00FF, 0x0000)]
    public void BitwiseAnd_ReturnsExpectedValue(uint first, uint second, uint expected)
    {
        (new BitMask(first) & new BitMask(second)).Should().Equal(new BitMask(expected));
    }
    
}