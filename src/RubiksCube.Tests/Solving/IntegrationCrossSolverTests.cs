using Xunit;
using RubiksCube.Core.Models;
using RubiksCube.Core.Solving;
using RubiksCube.Core.PatternRecognition;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.Solving;

/// <summary>
/// Integration tests for the new case-based CrossSolver implementation
/// </summary>
public class IntegrationCrossSolverTests
{
    [Fact]
    public void CrossSolver_RMoveScramble_ShouldSuggestCorrectInverse()
    {
        // Arrange - R move leaves 3/4 cross with OW edge misplaced
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('R'));
        
        var analyzer = new CubeStateAnalyzer();
        var result = analyzer.Analyze(cube);
        
        // Act - Get solver suggestion
        var suggestion = result.Suggestion;
        
        // Assert - Should suggest R' to fix the orange edge
        Assert.NotNull(suggestion);
        Assert.Equal("R'", suggestion.Algorithm);
        Assert.Contains("orange", suggestion.Description.ToLowerInvariant());
        Assert.Equal("cross", suggestion.NextStage);
    }

    [Fact] 
    public void CrossSolver_FMoveScramble_ShouldSuggestCorrectInverse()
    {
        // Arrange - F move leaves 3/4 cross with GW edge misplaced
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('F'));
        
        var analyzer = new CubeStateAnalyzer();
        var result = analyzer.Analyze(cube);
        
        // Act - Get solver suggestion
        var suggestion = result.Suggestion;
        
        // Assert - Should suggest F' to fix the green edge
        Assert.NotNull(suggestion);
        Assert.Equal("F'", suggestion.Algorithm);
        Assert.Contains("green", suggestion.Description.ToLowerInvariant());
        Assert.Equal("cross", suggestion.NextStage);
    }

    [Fact]
    public void CrossSolver_CompleteCross_ShouldSuggestF2L()
    {
        // Arrange - U move preserves cross completely
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('U'));
        
        var analyzer = new CubeStateAnalyzer();
        var result = analyzer.Analyze(cube);
        
        // Act - Get solver suggestion
        var suggestion = result.Suggestion;
        
        // Assert - Should suggest moving to F2L
        Assert.NotNull(suggestion);
        Assert.Equal("", suggestion.Algorithm); // No moves needed
        Assert.Contains("F2L", suggestion.Description);
        Assert.Equal("f2l", suggestion.NextStage);
    }

    [Fact]
    public void CrossSolver_LFRScramble_ShouldProvideIncrementalSolution()
    {
        // Arrange - "L F R" scramble leaves 1/4 cross
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('L'));
        cube.ApplyMove(new Move('F'));
        cube.ApplyMove(new Move('R'));
        
        var analyzer = new CubeStateAnalyzer();
        var result = analyzer.Analyze(cube);
        
        // Act - Get solver suggestion
        var suggestion = result.Suggestion;
        
        // Assert - Should suggest specific edge move
        Assert.NotNull(suggestion);
        Assert.NotEmpty(suggestion.Algorithm);
        Assert.Contains("edge", suggestion.Description.ToLowerInvariant());
        Assert.Equal("cross", suggestion.NextStage);
    }

    [Fact]
    public void CrossSolver_IncrementalSolving_ShouldMakeProgress()
    {
        // Arrange - Start with L F R scramble (1/4 cross)
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move('L'));
        cube.ApplyMove(new Move('F')); 
        cube.ApplyMove(new Move('R'));
        
        var analyzer = new CubeStateAnalyzer();
        var initialResult = analyzer.Analyze(cube);
        
        // Act - Apply the suggested algorithm
        var suggestion = initialResult.Suggestion;
        Assert.NotNull(suggestion);
        Assert.NotEmpty(suggestion.Algorithm);
        
        var algorithm = Algorithm.Parse(suggestion.Algorithm);
        Assert.True(algorithm.IsSuccess);
        
        foreach (var move in algorithm.Value.Moves)
        {
            cube.ApplyMove(move);
        }
        
        // Check progress after applying suggestion
        var afterResult = analyzer.Analyze(cube);
        
        // Assert - Should have made progress on cross
        Assert.True(afterResult.Recognition.Progress > initialResult.Recognition.Progress ||
                   afterResult.Recognition.Stage == "solved");
    }

    [Theory]
    [InlineData("R", "R'")]
    [InlineData("L", "L'")]
    [InlineData("F", "F'")]
    [InlineData("B", "B'")]
    public void CrossSolver_SingleFaceMoves_ShouldSuggestInverse(string scrambleMove, string expectedSolution)
    {
        // Arrange
        var cube = Cube.CreateSolved();
        cube.ApplyMove(new Move(scrambleMove[0]));
        
        var analyzer = new CubeStateAnalyzer();
        var result = analyzer.Analyze(cube);
        
        // Act
        var suggestion = result.Suggestion;
        
        // Assert - Should suggest the inverse move
        Assert.NotNull(suggestion);
        Assert.Equal(expectedSolution, suggestion.Algorithm);
    }

    [Fact]
    public void CrossSolver_EdgeFindingLogic_ShouldIdentifyAllCrossEdges()
    {
        // Arrange - Test that we can find all cross edges in solved cube
        var cube = Cube.CreateSolved();
        var solver = new CrossSolver();
        
        // Act - Use reflection to test private FindCrossEdge method
        var findCrossEdgeMethod = typeof(CrossSolver).GetMethod("FindCrossEdge", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(findCrossEdgeMethod);
        
        // Assert - Should find all four cross edges
        var whiteGreen = findCrossEdgeMethod.Invoke(solver, new object[] { cube, CubeColor.White, CubeColor.Green });
        var whiteOrange = findCrossEdgeMethod.Invoke(solver, new object[] { cube, CubeColor.White, CubeColor.Orange });
        var whiteBlue = findCrossEdgeMethod.Invoke(solver, new object[] { cube, CubeColor.White, CubeColor.Blue });
        var whiteRed = findCrossEdgeMethod.Invoke(solver, new object[] { cube, CubeColor.White, CubeColor.Red });
        
        Assert.NotNull(whiteGreen);
        Assert.NotNull(whiteOrange);
        Assert.NotNull(whiteBlue);
        Assert.NotNull(whiteRed);
    }
}