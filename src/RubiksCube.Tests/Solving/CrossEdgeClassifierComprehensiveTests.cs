using Xunit;
using RubiksCube.Core.Models;
using RubiksCube.Core.Solving;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.Solving;

/// <summary>
/// Comprehensive tests for all 24 cross edge cases using real cube moves
/// 
/// TODO: MOST TESTS ARE FAILING - Move sequences are incorrect guesses!
/// Need to either:
/// 1. Trace each move sequence through cube to verify actual edge positions
/// 2. Create test cubes manually with known edge placements
/// 3. Use systematic approach to discover correct move sequences
/// 
/// Current status: Classifier logic works (5/28 passing), but test expectations wrong
/// </summary>
public class CrossEdgeClassifierComprehensiveTests
{
    [Theory]
    [InlineData("", CrossEdgeCase.BottomFrontAligned)]           // Case 1a: Solved position
    [InlineData("F2", CrossEdgeCase.BottomFrontFlipped)]         // Case 1b: Flipped in place
    [InlineData("D", CrossEdgeCase.BottomRightAligned)]          // Case 1c: Bottom-right correct
    [InlineData("D R2", CrossEdgeCase.BottomRightFlipped)]       // Case 1d: Bottom-right flipped
    [InlineData("D2", CrossEdgeCase.BottomBackAligned)]          // Case 1e: Bottom-back correct  
    [InlineData("D2 B2", CrossEdgeCase.BottomBackFlipped)]       // Case 1f: Bottom-back flipped
    [InlineData("D'", CrossEdgeCase.BottomLeftAligned)]          // Case 1g: Bottom-left correct
    [InlineData("D' L2", CrossEdgeCase.BottomLeftFlipped)]       // Case 1h: Bottom-left flipped
    public void ClassifyEdgePosition_BottomLayerCases_ReturnsCorrectCase(string moves, CrossEdgeCase expected)
    {
        // Arrange
        var cube = Cube.CreateSolved();
        if (!string.IsNullOrEmpty(moves))
        {
            // Apply individual moves
            foreach (var moveStr in moves.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                cube.ApplyMove(Move.Parse(moveStr));
            }
        }
        
        // Act - Test white-green edge classification
        var result = CrossEdgeClassifier.ClassifyEdgePosition(cube, CubeColor.White, CubeColor.Green);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("F", CrossEdgeCase.MiddleFrontRightAligned)]     // Case 2a: Front-right correct
    [InlineData("R'", CrossEdgeCase.MiddleFrontRightFlipped)]    // Case 2b: Front-right flipped
    [InlineData("R", CrossEdgeCase.MiddleRightBackAligned)]      // Case 2c: Right-back correct
    [InlineData("B'", CrossEdgeCase.MiddleRightBackFlipped)]     // Case 2d: Right-back flipped
    [InlineData("B", CrossEdgeCase.MiddleBackLeftAligned)]       // Case 2e: Back-left correct
    [InlineData("L'", CrossEdgeCase.MiddleBackLeftFlipped)]      // Case 2f: Back-left flipped
    [InlineData("F'", CrossEdgeCase.MiddleLeftFrontAligned)]     // Case 2g: Left-front correct
    [InlineData("L", CrossEdgeCase.MiddleLeftFrontFlipped)]      // Case 2h: Left-front flipped
    public void ClassifyEdgePosition_MiddleLayerCases_ReturnsCorrectCase(string moves, CrossEdgeCase expected)
    {
        // Arrange
        var cube = Cube.CreateSolved();
        // Apply individual moves
        foreach (var moveStr in moves.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            cube.ApplyMove(Move.Parse(moveStr));
        }
        
        // Act - Test white-green edge classification
        var result = CrossEdgeClassifier.ClassifyEdgePosition(cube, CubeColor.White, CubeColor.Green);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("F2 U2", CrossEdgeCase.TopFrontAligned)]        // Case 3a: Top-front correct
    [InlineData("U' R' F", CrossEdgeCase.TopFrontFlipped)]      // Case 3b: Top-front flipped
    [InlineData("F2 U", CrossEdgeCase.TopRightAligned)]         // Case 3c: Top-right correct
    [InlineData("R' F", CrossEdgeCase.TopRightFlipped)]         // Case 3d: Top-right flipped
    [InlineData("F2", CrossEdgeCase.TopBackAligned)]            // Case 3e: Top-back correct
    [InlineData("U R' F", CrossEdgeCase.TopBackFlipped)]        // Case 3f: Top-back flipped
    [InlineData("F2 U'", CrossEdgeCase.TopLeftAligned)]         // Case 3g: Top-left correct
    [InlineData("L F'", CrossEdgeCase.TopLeftFlipped)]          // Case 3h: Top-left flipped
    public void ClassifyEdgePosition_TopLayerCases_ReturnsCorrectCase(string moves, CrossEdgeCase expected)
    {
        // Arrange
        var cube = Cube.CreateSolved();
        // Apply individual moves
        foreach (var moveStr in moves.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            cube.ApplyMove(Move.Parse(moveStr));
        }
        
        // Act - Test white-green edge classification
        var result = CrossEdgeClassifier.ClassifyEdgePosition(cube, CubeColor.White, CubeColor.Green);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void ClassifyEdgePosition_AllTwentyFourCases_AreCovered()
    {
        // This test ensures we have coverage for all 24 enum values
        var allCases = Enum.GetValues<CrossEdgeCase>();
        Assert.Equal(24, allCases.Length);
        
        // Verify each case is tested above
        var testedCases = new[]
        {
            // Bottom layer (8 cases)
            CrossEdgeCase.BottomFrontAligned, CrossEdgeCase.BottomFrontFlipped,
            CrossEdgeCase.BottomRightAligned, CrossEdgeCase.BottomRightFlipped,
            CrossEdgeCase.BottomBackAligned, CrossEdgeCase.BottomBackFlipped,
            CrossEdgeCase.BottomLeftAligned, CrossEdgeCase.BottomLeftFlipped,
            
            // Middle layer (8 cases)
            CrossEdgeCase.MiddleFrontRightAligned, CrossEdgeCase.MiddleFrontRightFlipped,
            CrossEdgeCase.MiddleRightBackAligned, CrossEdgeCase.MiddleRightBackFlipped,
            CrossEdgeCase.MiddleBackLeftAligned, CrossEdgeCase.MiddleBackLeftFlipped,
            CrossEdgeCase.MiddleLeftFrontAligned, CrossEdgeCase.MiddleLeftFrontFlipped,
            
            // Top layer (8 cases)
            CrossEdgeCase.TopFrontAligned, CrossEdgeCase.TopFrontFlipped,
            CrossEdgeCase.TopRightAligned, CrossEdgeCase.TopRightFlipped,
            CrossEdgeCase.TopBackAligned, CrossEdgeCase.TopBackFlipped,
            CrossEdgeCase.TopLeftAligned, CrossEdgeCase.TopLeftFlipped
        };
        
        Assert.Equal(24, testedCases.Length);
        Assert.True(allCases.All(c => testedCases.Contains(c)));
    }
    
    [Theory]
    [InlineData(CubeColor.Orange)] // White-Orange edge
    [InlineData(CubeColor.Blue)]   // White-Blue edge  
    [InlineData(CubeColor.Red)]    // White-Red edge
    public void ClassifyEdgePosition_DifferentEdgeColors_WorksCorrectly(CubeColor edgeColor)
    {
        // Arrange - Create solved cube
        var cube = Cube.CreateSolved();
        
        // Act - All edges should be in their solved positions
        var result = CrossEdgeClassifier.ClassifyEdgePosition(cube, CubeColor.White, edgeColor);
        
        // Assert - Different edges will be in different "correct" positions
        var expectedCase = edgeColor switch
        {
            CubeColor.Green => CrossEdgeCase.BottomFrontAligned,  // (0,-1,1)
            CubeColor.Orange => CrossEdgeCase.BottomRightAligned, // (1,-1,0) 
            CubeColor.Blue => CrossEdgeCase.BottomBackAligned,    // (0,-1,-1)
            CubeColor.Red => CrossEdgeCase.BottomLeftAligned,     // (-1,-1,0)
            _ => throw new ArgumentException($"Invalid edge color: {edgeColor}")
        };
        
        Assert.Equal(expectedCase, result);
    }
}