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
    [InlineData("F2", 3, "Place")]       // F2 move disrupts front edge
    [InlineData("R2", 3, "Place")]       // R2 move disrupts right edge
    [InlineData("L2", 3, "Place")]       // L2 move disrupts left edge
    [InlineData("B2", 3, "Place")]       // B2 move disrupts back edge
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
        // Arrange - Create disrupted cubes for each progress level
        var solver = new CrossSolver();
        var testScrambles = new[] { "F2 R2 L2 B2", "F2 R2 L2", "F2 R2", "F2" }; // 0, 1, 2, 3 edges solved

        for (int progress = 0; progress <= 3; progress++)
        {
            var cube = Cube.CreateSolved();
            // Apply scramble to disrupt edges
            foreach (var moveStr in testScrambles[progress].Split(' '))
            {
                cube.ApplyMove(new Move(moveStr[0]));
            }
            
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
            
            // Algorithm should be valid Singmaster notation (allow empty for edge cases)
            if (!string.IsNullOrEmpty(result.Algorithm))
            {
                Assert.Matches(@"^[RLUFDB]['2]?(\s[RLUFDB]['2]?)*$", result.Algorithm);
            }
        }
    }
}