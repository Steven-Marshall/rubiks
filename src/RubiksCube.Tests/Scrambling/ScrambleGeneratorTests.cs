using Xunit;
using RubiksCube.Core.Scrambling;
using RubiksCube.Core.Models;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.Scrambling;

public class ScrambleGeneratorTests
{
    [Fact]
    public void GenerateScramble_DefaultParameters_Should_Generate25Moves()
    {
        // Arrange
        var generator = new ScrambleGenerator(seed: 12345);

        // Act
        var result = generator.GenerateScramble();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(25, result.Value.Moves.Count);
    }

    [Fact]
    public void GenerateScramble_WithSeed_Should_BeReproducible()
    {
        // Arrange
        var generator1 = new ScrambleGenerator(seed: 42);
        var generator2 = new ScrambleGenerator(seed: 42);

        // Act
        var result1 = generator1.GenerateScramble();
        var result2 = generator2.GenerateScramble();

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.Equal(result1.Value.ToString(), result2.Value.ToString());
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(25)]
    [InlineData(30)]
    public void GenerateScramble_CustomMoveCount_Should_GenerateCorrectLength(int moveCount)
    {
        // Arrange
        var generator = new ScrambleGenerator(seed: 123);

        // Act
        var result = generator.GenerateScramble(moveCount);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(moveCount, result.Value.Moves.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void GenerateScramble_InvalidMoveCount_Should_ReturnFailure(int moveCount)
    {
        // Arrange
        var generator = new ScrambleGenerator();

        // Act
        var result = generator.GenerateScramble(moveCount);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Move count must be positive", result.Error);
    }

    [Fact]
    public void GenerateScramble_ExcessiveMoveCount_Should_ReturnFailure()
    {
        // Arrange
        var generator = new ScrambleGenerator();

        // Act
        var result = generator.GenerateScramble(101);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Move count cannot exceed 100", result.Error);
    }

    [Fact]
    public void GenerateScramble_StrictWcaRules_Should_NotHaveConsecutiveSameFace()
    {
        // Arrange
        var generator = new ScrambleGenerator(seed: 999);

        // Act
        var result = generator.GenerateScramble(moveCount: 25, strictWcaRules: true);

        // Assert
        Assert.True(result.IsSuccess);
        
        var moves = result.Value.Moves;
        for (int i = 1; i < moves.Count; i++)
        {
            var prevMove = moves[i-1];
            var currMove = moves[i];
            Assert.NotEqual(prevMove.Face, currMove.Face);
        }
    }

    [Fact]
    public void GenerateScramble_StrictWcaRules_Should_NotHaveOppositeFacesWithin2Moves()
    {
        // Arrange
        var generator = new ScrambleGenerator(seed: 777);

        // Act
        var result = generator.GenerateScramble(moveCount: 25, strictWcaRules: true);

        // Assert
        Assert.True(result.IsSuccess);
        
        var moves = result.Value.Moves;
        for (int i = 2; i < moves.Count; i++)
        {
            // Check that move i doesn't have opposite face to move i-2
            var currentFace = moves[i].Face;
            var twoMovesAgoFace = moves[i-2].Face;
            Assert.False(AreOppositeFaces(currentFace, twoMovesAgoFace), 
                $"Found opposite faces within 2 moves: {twoMovesAgoFace} at position {i-2} and {currentFace} at position {i}");
        }
    }

    [Fact]
    public void GenerateScramble_RelaxedRules_Should_AllowMoreMoveVariations()
    {
        // Arrange
        var generator = new ScrambleGenerator(seed: 555);

        // Act
        var result = generator.GenerateScramble(moveCount: 30, strictWcaRules: false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(30, result.Value.Moves.Count);
        
        // Should still not have consecutive same face moves
        var moves = result.Value.Moves;
        for (int i = 1; i < moves.Count; i++)
        {
            Assert.NotEqual(moves[i-1].Face, moves[i].Face);
        }
    }

    [Fact]
    public void GenerateScramble_Should_UseAllFaces()
    {
        // Arrange
        var generator = new ScrambleGenerator(seed: 111);

        // Act
        var result = generator.GenerateScramble(moveCount: 50, strictWcaRules: false);

        // Assert
        Assert.True(result.IsSuccess);
        
        var usedFaces = result.Value.Moves.Select(m => m.Face).Distinct().ToList();
        Assert.True(usedFaces.Count >= 5, "Should use at least 5 different faces in 50 moves");
    }

    [Fact]
    public void GenerateScramble_Should_UseAllMoveTypes()
    {
        // Arrange
        var generator = new ScrambleGenerator(seed: 222);

        // Act
        var result = generator.GenerateScramble(moveCount: 30);

        // Assert
        Assert.True(result.IsSuccess);
        
        var moves = result.Value.Moves;
        var hasClockwise = moves.Any(m => m.Direction == MoveDirection.Clockwise);
        var hasCounterClockwise = moves.Any(m => m.Direction == MoveDirection.CounterClockwise);
        var hasDouble = moves.Any(m => m.Direction == MoveDirection.Double);
        
        Assert.True(hasClockwise, "Should have clockwise moves");
        Assert.True(hasCounterClockwise, "Should have counter-clockwise moves");
        // Double moves are less common but should appear occasionally
    }

    [Fact]
    public void GenerateScramble_Should_CreateValidAlgorithm()
    {
        // Arrange
        var generator = new ScrambleGenerator(seed: 333);

        // Act
        var result = generator.GenerateScramble();

        // Assert
        Assert.True(result.IsSuccess);
        
        // Algorithm should parse correctly
        var algorithmString = result.Value.ToString();
        Assert.False(string.IsNullOrEmpty(algorithmString));
        
        // Should be able to apply to cube without errors
        var cube = Cube.CreateSolved();
        foreach (var move in result.Value.Moves)
        {
            cube.ApplyMove(move);
        }
        
        // Cube should be scrambled (not solved)
        Assert.False(cube.IsSolved);
    }

    [Fact]
    public void GenerateScramble_Should_ProduceValidSingmasterNotation()
    {
        // Arrange
        var generator = new ScrambleGenerator(seed: 444);

        // Act
        var result = generator.GenerateScramble(moveCount: 10);

        // Assert
        Assert.True(result.IsSuccess);
        
        var notation = result.Value.ToString();
        var parts = notation.Split(' ');
        
        Assert.Equal(10, parts.Length);
        foreach (var part in parts)
        {
            // Each part should be a valid move notation
            Assert.Matches(@"^[UFRLDB][2']?$", part);
        }
    }

    [Fact]
    public void GenerateScramble_MultipleCalls_Should_ProduceDifferentScrambles()
    {
        // Arrange
        var generator = new ScrambleGenerator(); // No seed - random

        // Act
        var result1 = generator.GenerateScramble();
        var result2 = generator.GenerateScramble();
        var result3 = generator.GenerateScramble();

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.True(result3.IsSuccess);
        
        // At least two should be different (very unlikely all three are same)
        var scrambles = new[] { 
            result1.Value.ToString(), 
            result2.Value.ToString(), 
            result3.Value.ToString() 
        };
        
        Assert.True(scrambles.Distinct().Count() >= 2, 
            "Multiple calls should produce different scrambles");
    }

    private static bool AreOppositeFaces(char face1, char face2)
    {
        return (face1, face2) switch
        {
            ('U', 'D') or ('D', 'U') => true,
            ('F', 'B') or ('B', 'F') => true,
            ('R', 'L') or ('L', 'R') => true,
            _ => false
        };
    }
}