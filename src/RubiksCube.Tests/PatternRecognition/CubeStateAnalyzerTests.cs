using Xunit;
using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.PatternRecognition;

public class CubeStateAnalyzerTests
{
    [Fact]
    public void Analyze_SolvedCube_ShouldReturnSolvedAnalysis()
    {
        // Arrange
        var cube = Cube.CreateSolved();
        var analyzer = new CubeStateAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Recognition);
        Assert.Equal("solved", result.Recognition.Stage);
        Assert.True(result.Recognition.IsComplete);
        Assert.Equal("Cube completely solved", result.Recognition.Description);
        
        Assert.NotNull(result.Suggestion);
        Assert.Equal("", result.Suggestion.Algorithm); // No moves needed
        Assert.Equal("solved", result.Suggestion.NextStage);
        Assert.Contains("No moves needed", result.Suggestion.Description);
    }

    [Fact]
    public void Analyze_ScrambledCube_ShouldDetectAppropriateStage()
    {
        // Arrange
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('R')); // Make it unsolved but might still have cross progress
        var analyzer = new CubeStateAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Recognition);
        
        // Could be "cross" (if cross progress remains) or "unsolved" (if no progress)
        Assert.True(result.Recognition.Stage == "cross" || result.Recognition.Stage == "unsolved");
        
        if (result.Recognition.Stage == "cross")
        {
            Assert.True(result.Recognition.Progress >= 1 && result.Recognition.Progress <= 4);
        }
        else if (result.Recognition.Stage == "unsolved")
        {
            Assert.False(result.Recognition.IsComplete);
            Assert.Equal("No clear CFOP progress detected", result.Recognition.Description);
        }
        
        Assert.NotNull(result.Suggestion);
    }

    [Fact]
    public void Analyze_GetSummary_ShouldReturnFormattedString()
    {
        // Arrange
        var cube = Cube.CreateSolved();
        var analyzer = new CubeStateAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);
        var summary = result.GetSummary();

        // Assert
        Assert.Contains("Cube completely solved", summary);
        Assert.Contains("Suggested:", summary);
    }

    [Theory]
    [InlineData('R')]
    [InlineData('F')]
    public void Analyze_CrossAffectingMoves_ShouldDetectCrossStage(char move)
    {
        // Arrange
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move(move));
        var analyzer = new CubeStateAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert
        Assert.NotNull(result);
        
        // Moves like R and F affect the cross, so should be "cross" stage
        Assert.Equal("cross", result.Recognition.Stage);
        Assert.True(result.Recognition.Progress >= 1 && result.Recognition.Progress < 4);
        Assert.False(result.Recognition.IsComplete); // Should not be complete
        
        Assert.NotNull(result.Suggestion);
    }
    
    [Fact]
    public void Analyze_UpMove_ShouldPreserveCrossComplete()
    {
        // Arrange - U move doesn't affect white cross (bottom layer)
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('U'));
        var analyzer = new CubeStateAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert
        Assert.NotNull(result);
        
        // U move preserves white cross, so cross should be complete
        Assert.Equal("cross", result.Recognition.Stage);
        Assert.True(result.Recognition.IsComplete);
        Assert.Equal(4, result.Recognition.Progress);
        
        Assert.NotNull(result.Suggestion);
        Assert.Equal("f2l", result.Suggestion.NextStage);
    }
}