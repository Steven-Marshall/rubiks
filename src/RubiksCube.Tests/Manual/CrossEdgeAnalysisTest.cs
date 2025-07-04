using Xunit;
using RubiksCube.Core.Models;
using RubiksCube.Core.Solving;
using RubiksCube.Core.Storage;
using RubiksCube.Core.Algorithms;

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

    [Fact]
    public void TestFirstEdgeOptimization()
    {
        // Create a solved cube
        var cube = Cube.CreateSolved();
        
        // Apply scramble: F D2 R2 U D2 R2 U R2 D2 F2
        var scrambleResult = Algorithm.Parse("F D2 R2 U D2 R2 U R2 D2 F2");
        Assert.True(scrambleResult.IsSuccess, "Failed to parse scramble");
        
        foreach (var move in scrambleResult.Value.Moves)
        {
            cube.ApplyMove(move);
        }

        // Apply D' move
        var dPrimeResult = Algorithm.Parse("D'");
        Assert.True(dPrimeResult.IsSuccess, "Failed to parse D'");
        
        foreach (var move in dPrimeResult.Value.Moves)
        {
            cube.ApplyMove(move);
        }

        // Create cross solver and analyze
        var crossSolver = new CrossSolver(CubeColor.White);
        var analysis = crossSolver.AnalyzeAllCrossEdges(cube, verbose: true);
        
        // Output the analysis
        Console.WriteLine($"Analysis after scramble + D':\n{analysis}");
        
        // Also test specific edges to verify first edge optimization
        var edgeColors = new[] { CubeColor.Blue, CubeColor.Red };
        
        foreach (var edgeColor in edgeColors)
        {
            var caseType = CrossEdgeClassifier.ClassifyEdgePosition(cube, CubeColor.White, edgeColor);
            var isFirstEdge = crossSolver.IsFirstEdge(cube, edgeColor);
            var algorithm = CrossEdgeAlgorithms.GetAlgorithm(caseType, cube, CubeColor.White, isFirstEdge, edgeColor);
            
            Console.WriteLine($"white-{edgeColor}: {caseType}, isFirstEdge={isFirstEdge}, algorithm='{algorithm}'");
        }
    }
}