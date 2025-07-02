using Xunit;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.Algorithms;

public class AlgorithmCompressorTests
{
    [Fact]
    public void Compress_EmptyString_ShouldReturnEmpty()
    {
        // Act
        var result = AlgorithmCompressor.Compress("");
        
        // Assert
        Assert.Equal("", result);
    }
    
    [Fact]
    public void Compress_SingleMove_ShouldReturnSame()
    {
        // Act
        var result = AlgorithmCompressor.Compress("R");
        
        // Assert
        Assert.Equal("R", result);
    }
    
    [Theory]
    [InlineData("R R'", "")] // Move and inverse cancel
    [InlineData("R' R", "")] // Inverse and move cancel
    [InlineData("R2 R2", "")] // Double moves cancel
    [InlineData("U U U", "U'")] // Three moves = inverse
    [InlineData("F F", "F2")] // Two moves = double
    [InlineData("L' L' L'", "L")] // Three inverses = single
    public void Compress_BasicCombinations_ShouldSimplify(string input, string expected)
    {
        // Act
        var result = AlgorithmCompressor.Compress(input);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void Compress_CrossSolverExample_ShouldSimplify()
    {
        // Our R move case: R' R2 should become R
        // Act
        var result = AlgorithmCompressor.Compress("R' R2");
        
        // Assert
        Assert.Equal("R", result);
    }
    
    [Fact]
    public void Compress_MultiplePatterns_ShouldSimplifyAll()
    {
        // Act
        var result = AlgorithmCompressor.Compress("R R' U U F F F' L L");
        
        // Assert - R R' cancels, U U becomes U2, F F F' = F2 F' = F, L L becomes L2
        Assert.Equal("U2 F L2", result);
    }
    
    [Fact]
    public void Compress_MixedFaces_ShouldOnlyCombineSameFace()
    {
        // Act
        var result = AlgorithmCompressor.Compress("R U R U'");
        
        // Assert - Only consecutive same-face moves are combined, so no compression occurs
        Assert.Equal("R U R U'", result);
    }
    
    [Fact]
    public void Compress_ComplexSequence_ShouldSimplifyCorrectly()
    {
        // Act
        var result = AlgorithmCompressor.Compress("R U R' U' R U R' U' R U' R'");
        
        // Assert - Should only compress consecutive same-face moves
        Assert.NotNull(result);
        // With simple compression, only consecutive moves are combined, so most of the sequence remains
        Assert.Equal("R U R' U' R U R' U' R U' R'", result);
    }
    
    [Theory]
    [InlineData("R R R R", "")] // Four quarter turns = full rotation = nothing
    [InlineData("R R R R R", "R")] // Five quarter turns = one quarter turn
    [InlineData("R2 R2 R2 R2", "")] // Four half turns = nothing
    [InlineData("R' R' R' R'", "")] // Four inverse quarter turns = nothing
    public void Compress_FullRotations_ShouldCancel(string input, string expected)
    {
        // Act
        var result = AlgorithmCompressor.Compress(input);
        
        // Assert
        Assert.Equal(expected, result);
    }
}