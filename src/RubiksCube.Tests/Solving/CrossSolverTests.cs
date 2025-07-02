using Xunit;
using RubiksCube.Core.Models;
using RubiksCube.Core.Solving;
using RubiksCube.Core.PatternRecognition.Models;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.Solving;

public class CrossSolverTests
{
    [Fact]
    public void SuggestAlgorithm_CompleteCross_ShouldSuggestF2L()
    {
        // Arrange
        var cube = Cube.CreateSolved();
        var solver = new CrossSolver();
        var recognition = new RecognitionResult(
            stage: "cross",
            isComplete: true,
            progress: 4,
            total: 4,
            description: "White cross complete"
        );

        // Act
        var result = solver.SuggestAlgorithm(cube, recognition);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("", result.Algorithm); // No moves needed
        Assert.Equal("f2l", result.NextStage);
        Assert.Contains("F2L", result.Description);
    }

    [Theory]
    [InlineData("R", 3, "Final")]        // R move leaves 3/4 cross
    [InlineData("L F R", 1, "Place")]    // L F R leaves 1/4 cross  
    [InlineData("F", 3, "Final")]        // F move leaves 3/4 cross
    [InlineData("L", 3, "Final")]        // L move leaves 3/4 cross
    public void SuggestAlgorithm_PartialCross_ShouldProvideEdgeSpecificSuggestion(string scramble, int expectedProgress, string expectedDescriptionStart)
    {
        // Arrange - Create cube with actual cross disruption
        var cube = Cube.CreateSolved();
        var moves = scramble.Split(' ');
        foreach (var moveStr in moves)
        {
            cube.ApplyMove(new Move(moveStr[0]));
        }
        
        var solver = new CrossSolver();
        var recognition = new RecognitionResult(
            stage: "cross",
            isComplete: false,
            progress: expectedProgress,
            total: 4,
            description: $"White cross {expectedProgress}/4 complete"
        );

        // Act
        var result = solver.SuggestAlgorithm(cube, recognition);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Algorithm); // Should provide some moves
        Assert.Contains(expectedDescriptionStart, result.Description);
        Assert.Contains("edge", result.Description.ToLowerInvariant());
        Assert.Equal("cross", result.NextStage); // Still working on cross
    }

    [Fact]
    public void SuggestAlgorithm_WrongStage_ShouldReturnNull()
    {
        // Arrange
        var cube = Cube.CreateSolved();
        var solver = new CrossSolver();
        var recognition = new RecognitionResult(
            stage: "solved", // Wrong stage
            isComplete: true,
            progress: 1,
            total: 1,
            description: "Cube solved"
        );

        // Act
        var result = solver.SuggestAlgorithm(cube, recognition);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void StageName_ShouldReturnCross()
    {
        // Arrange
        var solver = new CrossSolver();

        // Act & Assert
        Assert.Equal("cross", solver.StageName);
    }

    [Fact]
    public void SuggestAlgorithm_AllProgressLevels_ShouldProvideValidAlgorithms()
    {
        // Arrange
        var cube = Cube.CreateSolved();
        var solver = new CrossSolver();

        for (int progress = 0; progress <= 3; progress++)
        {
            var recognition = new RecognitionResult(
                stage: "cross",
                isComplete: false,
                progress: progress,
                total: 4,
                description: $"Cross {progress}/4 complete"
            );

            // Act
            var result = solver.SuggestAlgorithm(cube, recognition);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Algorithm);
            Assert.NotEmpty(result.Description);
            Assert.Equal("cross", result.NextStage);
            
            // Algorithm should be valid Singmaster notation
            Assert.Matches(@"^[RLUFDB]['2]?(\s[RLUFDB]['2]?)*$", result.Algorithm);
        }
    }
}