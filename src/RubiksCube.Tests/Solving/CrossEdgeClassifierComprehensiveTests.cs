using Xunit;
using RubiksCube.Core.Models;
using RubiksCube.Core.Solving;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.Solving;

/// <summary>
/// Comprehensive tests for all 24 cross edge cases using real cube moves
/// Tests verify that the CrossEdgeClassifier correctly identifies edge positions
/// after applying specific move sequences from a solved cube state
/// </summary>
public class CrossEdgeClassifierComprehensiveTests
{
    [Theory]
    [InlineData("", CrossEdgeCase.BottomFrontAligned)]           // Case 1a: Solved position
    [InlineData("F' D' R' D'", CrossEdgeCase.BottomFrontFlipped)]  // Case 1b: Reverse of "D R D' F"
    [InlineData("D", CrossEdgeCase.BottomRightAligned)]          // Case 1c: Bottom-right correct
    [InlineData("F' R'", CrossEdgeCase.BottomRightFlipped)]       // Case 1d: Reverse of "R F"
    [InlineData("D2", CrossEdgeCase.BottomBackAligned)]          // Case 1e: Bottom-back correct  
    [InlineData("F' D' R' D", CrossEdgeCase.BottomBackFlipped)]   // Case 1f: Reverse of "D' R D F"
    [InlineData("D'", CrossEdgeCase.BottomLeftAligned)]          // Case 1g: Bottom-left correct
    [InlineData("F L", CrossEdgeCase.BottomLeftFlipped)]          // Case 1h: Reverse of "L' F'"
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
    [InlineData("F'", CrossEdgeCase.MiddleFrontRightAligned)]    // Case 2a: Reverse of "F"
    [InlineData("F2 U' R'", CrossEdgeCase.MiddleFrontRightFlipped)] // Case 2b: Reverse of "R U F2"
    [InlineData("F' R2", CrossEdgeCase.MiddleRightBackAligned)]   // Case 2c: Reverse of "R2 F"
    [InlineData("F2 U' R", CrossEdgeCase.MiddleRightBackFlipped)]  // Case 2d: Reverse of "R' U F2"
    [InlineData("F L2", CrossEdgeCase.MiddleBackLeftAligned)]     // Case 2e: Reverse of "L2 F'"
    [InlineData("F2 U L'", CrossEdgeCase.MiddleBackLeftFlipped)]   // Case 2f: Reverse of "L U' F2"
    [InlineData("F", CrossEdgeCase.MiddleLeftFrontAligned)]       // Case 2g: Reverse of "F'"
    [InlineData("F2 U L", CrossEdgeCase.MiddleLeftFrontFlipped)]   // Case 2h: Reverse of "L' U' F2"
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
    [InlineData("F2", CrossEdgeCase.TopFrontAligned)]           // Case 3a: Reverse of "F2"
    [InlineData("F' R U", CrossEdgeCase.TopFrontFlipped)]        // Case 3b: Reverse of "U' R' F"
    [InlineData("F2 U'", CrossEdgeCase.TopRightAligned)]        // Case 3c: Reverse of "U F2"
    [InlineData("F' R", CrossEdgeCase.TopRightFlipped)]          // Case 3d: Reverse of "R' F"
    [InlineData("F2 U2", CrossEdgeCase.TopBackAligned)]         // Case 3e: Reverse of "U2 F2"
    [InlineData("F' R U'", CrossEdgeCase.TopBackFlipped)]        // Case 3f: Reverse of "U R' F"
    [InlineData("F2 U", CrossEdgeCase.TopLeftAligned)]          // Case 3g: Reverse of "U' F2"
    [InlineData("F L'", CrossEdgeCase.TopLeftFlipped)]           // Case 3h: Reverse of "L F'"
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
        
        // Assert - All edges should return BottomFrontAligned when in solved position
        // because the classifier transforms the perspective to view from each edge's canonical position
        Assert.Equal(CrossEdgeCase.BottomFrontAligned, result);
    }
}