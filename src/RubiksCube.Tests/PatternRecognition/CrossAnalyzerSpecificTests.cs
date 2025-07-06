using Xunit;
using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.PatternRecognition;

/// <summary>
/// Specific tests for CrossAnalyzer using known scrambles that produce specific cross states
/// </summary>
public class CrossAnalyzerSpecificTests
{
    [Fact]
    public void Analyze_SingleRMove_ShouldProduce3Of4Cross()
    {
        // Arrange - R move leaves 3/4 cross with OW edge misplaced
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('R'));
        var analyzer = new CrossAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("cross", result.Stage);
        Assert.False(result.IsComplete);
        Assert.Equal(3, result.Progress);
        Assert.Equal(4, result.Total);
        Assert.Equal("White cross 3/4 complete", result.Description);
        
        // Check details
        Assert.Equal("white", result.Details["cross_color"]);
        Assert.Equal(3, result.Details["correct_edges"]);
        Assert.True(result.Details.ContainsKey("misplaced_edges"));
    }

    [Fact]
    public void Analyze_SingleLMove_ShouldProduce3Of4Cross()
    {
        // Arrange - L move leaves 3/4 cross with RW edge misplaced
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('L'));
        var analyzer = new CrossAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("cross", result.Stage);
        Assert.False(result.IsComplete);
        Assert.Equal(3, result.Progress);
        Assert.Equal(4, result.Total);
        Assert.Equal("White cross 3/4 complete", result.Description);
    }

    [Fact]
    public void Analyze_FMoveOnSolved_ShouldProduce3Of4Cross()
    {
        // Arrange - F move affects front cross edge, leaving 3/4 cross
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('F'));
        var analyzer = new CrossAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("cross", result.Stage);
        Assert.False(result.IsComplete);
        Assert.Equal(3, result.Progress);
        Assert.Equal(4, result.Total);
        Assert.Equal("White cross 3/4 complete", result.Description);
    }

    [Fact]
    public void Analyze_LFRScramble_ShouldProduce1Of4Cross()
    {
        // Arrange - "L F R" scramble leaves only 1/4 cross
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('L'));
        cube.ApplyMove(new Move('F'));
        cube.ApplyMove(new Move('R'));
        var analyzer = new CrossAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("cross", result.Stage);
        Assert.False(result.IsComplete);
        Assert.Equal(1, result.Progress);
        Assert.Equal(4, result.Total);
        Assert.Equal("White cross 1/4 complete", result.Description);
    }

    [Fact]
    public void Analyze_DMoveDestroysAllCross_ShouldReturnCrossStage()
    {
        // Arrange - D move completely destroys white cross
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('D'));
        var analyzer = new CrossAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert - Should return cross stage with 0/4 progress (cross edges exist but none solved)
        Assert.NotNull(result);
        Assert.Equal("cross", result.Stage);
        Assert.False(result.IsComplete);
        Assert.Equal(0, result.Progress);
        Assert.Equal(4, result.Total);
    }

    [Fact]
    public void Analyze_FDScrambleDestroysAllCross_ShouldReturnCrossStage()
    {
        // Arrange - "F D" scramble completely destroys white cross
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('F'));
        cube.ApplyMove(new Move('D'));
        var analyzer = new CrossAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert - Should return cross stage with 0/4 progress (cross edges exist but none solved)
        Assert.NotNull(result);
        Assert.Equal("cross", result.Stage);
        Assert.False(result.IsComplete);
        Assert.Equal(0, result.Progress);
        Assert.Equal(4, result.Total);
    }

    [Fact]
    public void Analyze_UMovePreservesCross_ShouldReturnComplete()
    {
        // Arrange - U move only affects top layer, preserves white cross
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('U'));
        var analyzer = new CrossAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("cross", result.Stage);
        Assert.True(result.IsComplete);
        Assert.Equal(4, result.Progress);
        Assert.Equal(4, result.Total);
        Assert.Equal("White cross complete", result.Description);
    }

    [Theory]
    [InlineData("R", 3)]
    [InlineData("L", 3)]
    [InlineData("F", 3)]
    [InlineData("B", 3)]
    [InlineData("U", 4)]
    [InlineData("L F R", 1)]
    public void Analyze_VariousScrambles_ShouldProduceExpectedCrossProgress(string moveSequence, int expectedProgress)
    {
        // Arrange
        var cube = Cube.CreateSolved();
        var moves = moveSequence.Split(' ');
        foreach (var moveStr in moves)
        {
            cube.ApplyMove(new Move(moveStr[0]));
        }
        var analyzer = new CrossAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert
        if (expectedProgress > 0)
        {
            Assert.NotNull(result);
            Assert.Equal("cross", result.Stage);
            Assert.Equal(expectedProgress, result.Progress);
            Assert.Equal(expectedProgress == 4, result.IsComplete);
        }
        else
        {
            Assert.Null(result); // No cross progress
        }
    }

    [Fact]
    public void Analyze_MisplacedEdgeDetails_ShouldIncludePositionAndOrientation()
    {
        // Arrange - R move creates a specific misplaced edge pattern
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('R'));
        var analyzer = new CrossAnalyzer();

        // Act
        var result = analyzer.Analyze(cube);

        // Assert
        Assert.NotNull(result);
        var misplacedEdges = result.Details["misplaced_edges"] as List<Dictionary<string, object>>;
        Assert.NotNull(misplacedEdges);
        Assert.Single(misplacedEdges); // Should have 1 misplaced edge

        var edge = misplacedEdges[0];
        Assert.True(edge.ContainsKey("colors"));
        Assert.True(edge.ContainsKey("current_position"));
        Assert.True(edge.ContainsKey("solved_position"));
        Assert.True(edge.ContainsKey("oriented"));

        // OW edge should be misplaced after R move
        Assert.Equal("OW", edge["colors"]);
    }
}