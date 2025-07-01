using Xunit;
using RubiksCube.Core.Models;
using RubiksCube.Core.Solving;
using RubiksCube.Core.PatternRecognition.Models;

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
    [InlineData(0, "Start building white cross")]
    [InlineData(1, "Continue white cross - 1/4 edges placed")]
    [InlineData(2, "Half cross complete - 2/4 edges placed")]
    [InlineData(3, "Almost done - 3/4 edges placed")]
    public void SuggestAlgorithm_PartialCross_ShouldProvideAlgorithm(int progress, string expectedDescription)
    {
        // Arrange
        var cube = Cube.CreateSolved();
        var solver = new CrossSolver();
        var recognition = new RecognitionResult(
            stage: "cross",
            isComplete: false,
            progress: progress,
            total: 4,
            description: $"White cross {progress}/4 complete"
        );

        // Act
        var result = solver.SuggestAlgorithm(cube, recognition);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Algorithm); // Should provide some moves
        Assert.Contains(expectedDescription, result.Description);
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