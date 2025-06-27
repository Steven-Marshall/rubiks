using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.Algorithms;

/// <summary>
/// Demonstrates proper Singmaster notation case sensitivity and wide move support
/// </summary>
public class CaseSensitivityDemoTests
{
    [Fact]
    public void SingmasterNotation_CaseSensitivity_ShouldBeRespected()
    {
        // Standard single-layer moves (uppercase)
        var R = Move.Parse("R");
        Assert.Equal('R', R.Face);
        // Note: Move type distinction removed in v3.0
        Assert.False(R.IsWide);
        
        // Wide moves (lowercase) - affects 2 layers
        var r = Move.Parse("r");
        Assert.Equal('r', r.Face);
        // Note: Move type distinction removed in v3.0
        Assert.True(r.IsWide);
        
        // Cube rotations (lowercase)
        var x = Move.Parse("x");
        Assert.Equal('x', x.Face);
        // Note: Move type distinction removed in v3.0
        Assert.False(x.IsWide);
        
        // They are different moves!
        Assert.NotEqual(R, r);
    }
    
    [Fact]
    public void Algorithm_MixedCaseSequence_ShouldParseCorrectly()
    {
        // Complex algorithm with mixed case notation
        var algorithm = Algorithm.Parse("x r U R' u' x'").Value;
        
        Assert.Equal(6, algorithm.Length);
        
        // Verify each move is parsed with correct case
        Assert.Equal("x", algorithm.Moves[0].ToString());  // Reorientation
        Assert.Equal("r", algorithm.Moves[1].ToString());  // Wide move
        Assert.Equal("U", algorithm.Moves[2].ToString());  // Single layer
        Assert.Equal("R'", algorithm.Moves[3].ToString()); // Single layer prime
        Assert.Equal("u'", algorithm.Moves[4].ToString()); // Wide move prime
        Assert.Equal("x'", algorithm.Moves[5].ToString()); // Reorientation prime
        
        // Note: Move type distinction removed in v3.0 - all moves handled uniformly
        
        // Verify wide move detection
        Assert.False(algorithm.Moves[0].IsWide); // x
        Assert.True(algorithm.Moves[1].IsWide);  // r
        Assert.False(algorithm.Moves[2].IsWide); // U
        Assert.False(algorithm.Moves[3].IsWide); // R'
        Assert.True(algorithm.Moves[4].IsWide);  // u'
        Assert.False(algorithm.Moves[5].IsWide); // x'
    }
    
    [Fact]
    public void Algorithm_FilterByWideStatus_ShouldWork()
    {
        var algorithm = Algorithm.Parse("R r U u' F f2").Value;
        
        var wideMoves = algorithm.Moves.Where(m => m.IsWide).ToList();
        var singleMoves = algorithm.Moves.Where(m => !m.IsWide).ToList();
        
        Assert.Equal(3, wideMoves.Count);  // r, u', f2
        Assert.Equal(3, singleMoves.Count); // R, U, F
        
        Assert.All(wideMoves, move => Assert.True(char.IsLower(move.Face)));
        Assert.All(singleMoves, move => Assert.True(char.IsUpper(move.Face)));
    }
    
    [Theory]
    [InlineData("R U R' U'", "Standard CFOP trigger")]
    [InlineData("r U r' U'", "Wide move CFOP trigger")]
    [InlineData("x y z", "Cube reorientations")]
    [InlineData("R r U u D d", "Mixed single and wide")]
    public void RealWorldAlgorithms_ShouldParseWithCorrectCasing(string algorithm, string description)
    {
        var result = Algorithm.Parse(algorithm);
        
        Assert.True(result.IsSuccess, $"Failed to parse {description}: {(result.IsFailure ? result.Error : "N/A")}");
        Assert.Equal(algorithm, result.Value.ToString());
    }
}