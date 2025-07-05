using Xunit;
using RubiksCube.Core.Models;
using RubiksCube.Core.Solving;

namespace RubiksCube.Tests.Solving;

/// <summary>
/// Tests for the CrossEdgeAlgorithms lookup system
/// These test the algorithm retrieval and basic conditional processing
/// </summary>
public class CrossEdgeAlgorithmsTests
{
    [Fact]
    public void GetAlgorithm_BottomFrontCorrect_ReturnsEmpty()
    {
        // Arrange & Act
        var result = CrossEdgeAlgorithms.GetAlgorithm(CrossEdgeCase.BottomFrontAligned);
        
        // Assert - Already solved, no moves needed
        Assert.Equal("", result);
    }
    
    [Fact]
    public void GetAlgorithm_BottomFrontFlipped_ReturnsProcessedAlgorithm()
    {
        // Arrange & Act  
        var result = CrossEdgeAlgorithms.GetAlgorithm(CrossEdgeCase.BottomFrontFlipped);
        
        // Assert - Should process "[D' restores bottom layer]" -> "D'"
        Assert.Equal("D R D' F", result);
    }
    
    [Fact]
    public void GetAlgorithm_BottomRightCorrect_ReturnsExpectedAlgorithm()
    {
        // Arrange & Act
        var result = CrossEdgeAlgorithms.GetAlgorithm(CrossEdgeCase.BottomRightAligned);
        
        // Assert - Default algorithm without preservation
        Assert.Equal("D'", result);
    }
    
    [Theory]
    [InlineData(CrossEdgeCase.MiddleFrontRightAligned, "F")]
    [InlineData(CrossEdgeCase.MiddleLeftFrontAligned, "F'")]
    [InlineData(CrossEdgeCase.TopFrontAligned, "F2")]
    [InlineData(CrossEdgeCase.TopRightAligned, "U F2")]
    [InlineData(CrossEdgeCase.BottomRightFlipped, "R F")]
    [InlineData(CrossEdgeCase.BottomLeftFlipped, "L' F'")]
    public void GetAlgorithm_SimpleCase_ReturnsExpectedAlgorithm(CrossEdgeCase caseType, string expected)
    {
        // Arrange & Act
        var result = CrossEdgeAlgorithms.GetAlgorithm(caseType);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData(CrossEdgeCase.MiddleFrontRightFlipped, "R U R' F2")]  // Should strip "[R' restores (1,-1,0)]" -> "R'"
    [InlineData(CrossEdgeCase.MiddleRightBackAligned, "R2 F R2")]      // Should strip "[R2 restores (1,0,1)]" -> "R2"
    [InlineData(CrossEdgeCase.TopFrontFlipped, "U' R' F R")]           // Should strip "[R restores (1,0,1)]" -> "R"
    public void GetAlgorithm_ConditionalRestoration_ProcessesBrackets(CrossEdgeCase caseType, string expected)
    {
        // Arrange & Act
        var result = CrossEdgeAlgorithms.GetAlgorithm(caseType);
        
        // Assert - Conditional restoration moves should be included (simple approach)
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void GetAlgorithm_AllCases_NoneThrowException()
    {
        // Arrange - Get all enum values
        var allCases = Enum.GetValues<CrossEdgeCase>();
        
        // Act & Assert - None should throw
        foreach (var caseType in allCases)
        {
            var exception = Record.Exception(() => CrossEdgeAlgorithms.GetAlgorithm(caseType));
            Assert.Null(exception);
        }
    }
    
    [Fact]
    public void GetAlgorithm_BottomLayerPreservation_ReturnsDifferentAlgorithms()
    {
        // Arrange - Cases that have preservation variants
        var preservationCases = new[]
        {
            CrossEdgeCase.BottomRightAligned,
            CrossEdgeCase.BottomBackAligned, 
            CrossEdgeCase.BottomLeftAligned
        };
        
        // Act & Assert
        foreach (var caseType in preservationCases)
        {
            var regular = CrossEdgeAlgorithms.GetAlgorithm(caseType, preserveBottomLayer: false);
            var preserved = CrossEdgeAlgorithms.GetAlgorithm(caseType, preserveBottomLayer: true);
            
            // Should be different algorithms when preservation is enabled
            Assert.NotEqual(regular, preserved);
        }
    }
}