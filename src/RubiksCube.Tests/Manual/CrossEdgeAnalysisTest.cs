using Xunit;
using RubiksCube.Core.Models;
using RubiksCube.Core.Solving;
using RubiksCube.Core.Storage;

namespace RubiksCube.Tests.Manual;

/// <summary>
/// Manual test to analyze all 4 cross edges for a specific cube
/// Run this to see the analysis for white-green, white-orange, white-blue, white-red
/// </summary>
public class CrossEdgeAnalysisTest
{
    [Fact]
    public void AnalyzeAllCrossEdges_ScrambledCube()
    {
        // Load the scrambled cube
        var loadResult = CubeStorageService.Load("scrambled-cube");
        Assert.True(loadResult.IsSuccess, $"Failed to load cube: {loadResult.Error}");
        
        var cube = loadResult.Value;
        var crossColor = CubeColor.White;
        var edgeColors = new[] { CubeColor.Green, CubeColor.Orange, CubeColor.Blue, CubeColor.Red };

        foreach (var edgeColor in edgeColors)
        {
            try
            {
                // Classify this edge
                var caseType = CrossEdgeClassifier.ClassifyEdgePosition(cube, crossColor, edgeColor);
                
                // Get algorithm (assuming it's first edge)
                var algorithm = CrossEdgeAlgorithms.GetAlgorithm(caseType, cube, crossColor, isFirstEdge: true);
                
                // Output results
                var output = $"{crossColor}-{edgeColor} edge: {caseType} -> {(string.IsNullOrEmpty(algorithm) ? "None needed" : algorithm)}";
                
                // Use Assert to output the information
                Assert.True(true, output); // This will show in test output
            }
            catch (Exception ex)
            {
                Assert.True(false, $"{crossColor}-{edgeColor} edge: ERROR - {ex.Message}");
            }
        }
    }
}