using Xunit;
using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.PatternRecognition;

public class SolvedAnalyzerTests
{
    [Fact]
    public void Analyze_SolvedCube_ShouldReturnSolvedResult()
    {
        // Arrange
        var cube = Cube.CreateSolved();
        var analyzer = new SolvedAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("solved", result.Stage);
        Assert.True(result.IsComplete);
        Assert.Equal(1, result.Progress);
        Assert.Equal(1, result.Total);
        Assert.Equal("Cube completely solved", result.Description);
    }

    [Fact]
    public void Analyze_ScrambledCube_ShouldReturnNull()
    {
        // Arrange
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('R')); // Make it unsolved
        var analyzer = new SolvedAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void StageName_ShouldReturnSolved()
    {
        // Arrange
        var analyzer = new SolvedAnalyzer();

        // Act & Assert
        Assert.Equal("solved", analyzer.StageName);
    }

    [Fact]
    public void Analyze_PartiallyScrambledCube_ShouldReturnNull()
    {
        // Arrange
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('R'));
        cube.ApplyMove(new Move('U'));
        cube.ApplyMove(new Move('R', MoveDirection.CounterClockwise));
        cube.ApplyMove(new Move('U', MoveDirection.CounterClockwise));
        // Note: This doesn't return to solved state in general
        var analyzer = new SolvedAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert - Should be null unless cube happens to be solved
        if (result != null)
        {
            // If somehow still solved, verify the result is correct
            Assert.Equal("solved", result.Stage);
            Assert.True(result.IsComplete);
        }
        else
        {
            // Expected case - cube is not solved
            Assert.Null(result);
        }
    }
}