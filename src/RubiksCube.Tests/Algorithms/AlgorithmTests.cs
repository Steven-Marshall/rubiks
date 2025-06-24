using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.Algorithms;

public class AlgorithmTests
{
    [Fact]
    public void Constructor_EmptyAlgorithm_ShouldCreateEmpty()
    {
        var algorithm = new Algorithm();
        
        Assert.True(algorithm.IsEmpty);
        Assert.Equal(0, algorithm.Length);
        Assert.Empty(algorithm.Moves);
    }
    
    [Fact]
    public void Constructor_SingleMove_ShouldCreateWithOneMove()
    {
        var move = new Move('R');
        var algorithm = new Algorithm(move);
        
        Assert.False(algorithm.IsEmpty);
        Assert.Equal(1, algorithm.Length);
        Assert.Single(algorithm.Moves);
        Assert.Equal(move, algorithm.Moves[0]);
    }
    
    [Fact]
    public void Constructor_MultipleMoves_ShouldCreateWithAllMoves()
    {
        var moves = new[]
        {
            new Move('R'),
            new Move('U'),
            new Move('R', MoveDirection.CounterClockwise),
            new Move('U', MoveDirection.CounterClockwise)
        };
        
        var algorithm = new Algorithm(moves);
        
        Assert.False(algorithm.IsEmpty);
        Assert.Equal(4, algorithm.Length);
        Assert.Equal(moves, algorithm.Moves);
    }
    
    [Theory]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    [InlineData("R", 1)]
    [InlineData("R U R' U'", 4)]
    [InlineData("x R U R' U' x'", 6)]
    [InlineData("R2 U2 R2", 3)]
    public void Parse_ValidAlgorithms_ShouldParseCorrectly(string algorithmString, int expectedMoveCount)
    {
        var result = Algorithm.Parse(algorithmString);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedMoveCount, result.Value.Length);
    }
    
    [Theory]
    [InlineData("R U R' U'")]
    [InlineData("x R U R' U' x'")]
    [InlineData("R2 U2 R' U' R U' R' U2 R2")]
    [InlineData("(R U R' U') (R U R' U')")]  // Parentheses should be ignored
    public void Parse_ComplexAlgorithms_ShouldParseSuccessfully(string algorithmString)
    {
        var result = Algorithm.Parse(algorithmString);
        
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsEmpty);
    }
    
    [Theory]
    [InlineData("R U R' U'", "R", "U", "R'", "U'")]
    [InlineData("x R U R' U' x'", "x", "R", "U", "R'", "U'", "x'")]
    [InlineData("R2 U2 F2", "R2", "U2", "F2")]
    public void Parse_SpecificSequences_ShouldParseInCorrectOrder(string algorithmString, params string[] expectedMoves)
    {
        var result = Algorithm.Parse(algorithmString);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedMoves.Length, result.Value.Length);
        
        for (int i = 0; i < expectedMoves.Length; i++)
        {
            Assert.Equal(expectedMoves[i], result.Value.Moves[i].ToString());
        }
    }
    
    [Theory]
    [InlineData("R U R' U'", "U R U' R'")]
    [InlineData("x R U R' U' x'", "x U R U' R' x'")]
    [InlineData("R2 U2", "U2 R2")]
    public void Inverse_VariousAlgorithms_ShouldInvertCorrectly(string original, string expectedInverse)
    {
        var algorithm = Algorithm.Parse(original).Value;
        var inverse = algorithm.Inverse();
        
        Assert.Equal(expectedInverse, inverse.ToString());
    }
    
    [Fact]
    public void Inverse_EmptyAlgorithm_ShouldReturnEmpty()
    {
        var algorithm = new Algorithm();
        var inverse = algorithm.Inverse();
        
        Assert.True(inverse.IsEmpty);
    }
    
    [Fact]
    public void Concat_TwoAlgorithms_ShouldCombineCorrectly()
    {
        var algo1 = Algorithm.Parse("R U").Value;
        var algo2 = Algorithm.Parse("R' U'").Value;
        var combined = algo1.Concat(algo2);
        
        Assert.Equal("R U R' U'", combined.ToString());
        Assert.Equal(4, combined.Length);
    }
    
    [Fact]
    public void Repeat_Algorithm_ShouldRepeatCorrectly()
    {
        var algorithm = Algorithm.Parse("R U").Value;
        var repeated = algorithm.Repeat(3);
        
        Assert.Equal("R U R U R U", repeated.ToString());
        Assert.Equal(6, repeated.Length);
    }
    
    [Fact]
    public void Repeat_ZeroTimes_ShouldReturnEmpty()
    {
        var algorithm = Algorithm.Parse("R U").Value;
        var repeated = algorithm.Repeat(0);
        
        Assert.True(repeated.IsEmpty);
    }
    
    [Fact]
    public void GetReorientations_MixedAlgorithm_ShouldReturnOnlyReorientations()
    {
        var algorithm = Algorithm.Parse("x R U R' U' x'").Value;
        var reorientations = algorithm.GetReorientations();
        
        Assert.Equal("x x'", reorientations.ToString());
        Assert.Equal(2, reorientations.Length);
        Assert.All(reorientations.Moves, move => Assert.Equal(MoveType.Reorientation, move.Type));
    }
    
    [Fact]
    public void GetRotations_MixedAlgorithm_ShouldReturnOnlyRotations()
    {
        var algorithm = Algorithm.Parse("x R U R' U' x'").Value;
        var rotations = algorithm.GetRotations();
        
        Assert.Equal("R U R' U'", rotations.ToString());
        Assert.Equal(4, rotations.Length);
        Assert.All(rotations.Moves, move => Assert.Equal(MoveType.Rotation, move.Type));
    }
    
    [Theory]
    [InlineData("R U R' U'")]
    [InlineData("x R U R' U' x'")]
    [InlineData("")]
    public void ToString_VariousAlgorithms_ShouldFormatCorrectly(string algorithmString)
    {
        var algorithm = Algorithm.Parse(algorithmString).Value;
        var formattedString = algorithm.ToString();
        
        // Empty algorithm should return empty string
        if (string.IsNullOrWhiteSpace(algorithmString))
        {
            Assert.Equal("", formattedString);
        }
        else
        {
            // Should be able to parse the formatted string back to the same algorithm
            var reparsed = Algorithm.Parse(formattedString).Value;
            Assert.Equal(algorithm, reparsed);
        }
    }
    
    [Fact]
    public void ToDetailedString_MixedAlgorithm_ShouldShowMoveTypes()
    {
        var algorithm = Algorithm.Parse("x R U R' U' x'").Value;
        var detailed = algorithm.ToDetailedString();
        
        Assert.Contains("(Reorientation)", detailed);
        Assert.Contains("(Rotation)", detailed);
        Assert.Contains("x", detailed);
        Assert.Contains("R", detailed);
    }
    
    [Fact]
    public void Equality_SameAlgorithms_ShouldBeEqual()
    {
        var algo1 = Algorithm.Parse("R U R' U'").Value;
        var algo2 = Algorithm.Parse("R U R' U'").Value;
        
        Assert.Equal(algo1, algo2);
        Assert.True(algo1 == algo2);
        Assert.False(algo1 != algo2);
    }
    
    [Fact]
    public void Equality_DifferentAlgorithms_ShouldNotBeEqual()
    {
        var algo1 = Algorithm.Parse("R U R' U'").Value;
        var algo2 = Algorithm.Parse("R U R'").Value;
        
        Assert.NotEqual(algo1, algo2);
        Assert.False(algo1 == algo2);
        Assert.True(algo1 != algo2);
    }
    
    [Fact]
    public void ImplicitConversion_FromString_ShouldWork()
    {
        Algorithm algorithm = "R U R' U'";
        
        Assert.Equal(4, algorithm.Length);
        Assert.Equal("R U R' U'", algorithm.ToString());
    }
    
    [Fact]
    public void ImplicitConversion_InvalidString_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => 
        {
            Algorithm algorithm = "INVALID";
        });
    }
    
    [Theory]
    [InlineData("R U R' U'", "R")]
    [InlineData("x R U R' U' x'", "x")]
    [InlineData("R2 U2 F2", "R2")]
    public void Parse_FirstMove_ShouldBeCorrect(string algorithmString, string expectedFirstMove)
    {
        var algorithm = Algorithm.Parse(algorithmString).Value;
        
        Assert.Equal(expectedFirstMove, algorithm.Moves[0].ToString());
    }
    
    [Theory]
    [InlineData("( R U R' U' )")]    // Spaces inside parentheses
    [InlineData("(R U R' U')")]      // No spaces inside parentheses
    [InlineData("R (U R') U'")]      // Partial parentheses
    [InlineData("((R U)) ((R' U'))")]// Nested parentheses
    public void Parse_Parentheses_ShouldBeIgnored(string algorithmWithParentheses)
    {
        var result = Algorithm.Parse(algorithmWithParentheses);
        
        Assert.True(result.IsSuccess);
        
        // Should parse the same as without parentheses
        var expected = Algorithm.Parse("R U R' U'").Value;
        Assert.Equal(expected, result.Value);
    }
}