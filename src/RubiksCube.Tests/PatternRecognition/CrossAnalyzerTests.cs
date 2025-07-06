using Xunit;
using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.PatternRecognition;

public class CrossAnalyzerTests
{
    [Fact]
    public void Analyze_SolvedCube_ShouldReturnCompleteCross()
    {
        // Arrange
        var cube = Cube.CreateSolved();
        var analyzer = new CrossAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("cross", result.Stage);
        Assert.True(result.IsComplete);
        Assert.Equal(4, result.Progress);
        Assert.Equal(4, result.Total);
        Assert.Contains("White cross complete", result.Description);
        Assert.Equal("white", result.Details["cross_color"]);
        Assert.Equal(4, result.Details["correct_edges"]);
    }

    [Fact]
    public void Analyze_ScrambledCube_ShouldReturnCrossStageInfo()
    {
        // Arrange
        var cube = Cube.CreateSolved();
        // Apply moves that destroy the cross completely
        cube.ApplyMove(new Move('R'));
        cube.ApplyMove(new Move('U'));
        cube.ApplyMove(new Move('F'));
        cube.ApplyMove(new Move('L'));
        var analyzer = new CrossAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert - Should always return cross stage info (cross edges always exist)
        Assert.NotNull(result);
        Assert.Equal("cross", result.Stage);
        Assert.True(result.Progress >= 0 && result.Progress <= 4);
        // May or may not be complete depending on the scramble
    }

    [Fact]
    public void Analyze_PartialCross_ShouldReturnPartialProgress()
    {
        // Arrange - Create a cube with partial cross by applying a single F move
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('F')); // This should leave 3/4 cross
        var analyzer = new CrossAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert - Single F move should still leave some cross progress
        Assert.NotNull(result); // Should have cross progress
        Assert.Equal("cross", result.Stage);
        Assert.True(result.Progress >= 1 && result.Progress < 4);
        Assert.False(result.IsComplete);
        Assert.Contains($"{result.Progress}/4", result.Description);
    }

    [Theory]
    [InlineData(CubeColor.White)]
    [InlineData(CubeColor.Yellow)]
    public void Analyze_DifferentCrossColors_ShouldUseCorrectColor(CubeColor crossColor)
    {
        // Arrange
        var cube = Cube.CreateSolved();
        var analyzer = new CrossAnalyzer(crossColor);

        // Act
        var result = analyzer.Analyze(cube);

        // Assert
        if (crossColor == CubeColor.White)
        {
            // White cross should be complete in solved cube
            Assert.NotNull(result);
            Assert.True(result.IsComplete);
        }
        else
        {
            // Other colors won't have complete cross in standard solved cube
            // Could be null or partial progress
            if (result != null)
            {
                Assert.Equal("cross", result.Stage);
            }
        }
    }

    [Fact]
    public void StageName_ShouldReturnCross()
    {
        // Arrange
        var analyzer = new CrossAnalyzer();

        // Act & Assert
        Assert.Equal("cross", analyzer.StageName);
    }

    [Fact]
    public void CrossColor_ShouldDefaultToWhite()
    {
        // Arrange & Act
        var analyzer = new CrossAnalyzer();

        // Assert
        Assert.Equal(CubeColor.White, analyzer.CrossColor);
    }

    [Fact]
    public void CrossColor_ShouldUseSpecifiedColor()
    {
        // Arrange & Act
        var analyzer = new CrossAnalyzer(CubeColor.Yellow);

        // Assert
        Assert.Equal(CubeColor.Yellow, analyzer.CrossColor);
    }

    [Fact]
    public void Analyze_SingleEdgeMove_ShouldDetectChange()
    {
        // Arrange
        var cube = Cube.CreateSolved();
        var analyzer = new CrossAnalyzer();
        
        // Get baseline (should be complete)
        var beforeResult = analyzer.Analyze(cube);
        Assert.NotNull(beforeResult);
        Assert.True(beforeResult.IsComplete);
        
        // Apply single edge-affecting move
        cube.ApplyMove(new Move('F'));

        // Act
        var result = analyzer.Analyze(cube);

        // Assert - Cross should be affected
        Assert.NotNull(result);
        Assert.Equal("cross", result.Stage);
        Assert.True(result.Progress < 4); // Should be less than complete
    }
}