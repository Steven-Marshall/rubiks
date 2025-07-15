using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition.Models;
using RubiksCube.Core.Algorithms;
using RubiksCube.Core.Solving;
using RubiksCube.Core.Constants;

namespace RubiksCube.Core.Extensions;

/// <summary>
/// Extension methods for Cube operations, particularly cross-solving utilities
/// </summary>
public static class CubeExtensions
{
    /// <summary>
    /// Find a specific cross edge by its two colors
    /// </summary>
    /// <param name="cube">The cube to search</param>
    /// <param name="crossColor">The cross color (typically white)</param>
    /// <param name="edgeColor">The edge color (green, orange, blue, red)</param>
    /// <returns>The cross edge piece, or null if not found</returns>
    public static CubePiece? FindCrossEdge(this Cube cube, CubeColor crossColor, CubeColor edgeColor)
    {
        return cube.Edges.FirstOrDefault(edge =>
            edge.Colors.Contains(crossColor) && edge.Colors.Contains(edgeColor));
    }
    
    /// <summary>
    /// Count how many cross edges are currently solved (in correct position with cross color on bottom)
    /// </summary>
    /// <param name="cube">The cube to analyze</param>
    /// <param name="crossColor">The cross color (typically white)</param>
    /// <returns>Number of solved cross edges (0-4)</returns>
    public static int CountSolvedCrossEdges(this Cube cube, CubeColor crossColor)
    {
        var crossEdgeColors = CrossConstants.StandardEdgeColors;
        var solvedCount = 0;
        
        foreach (var edgeColor in crossEdgeColors)
        {
            if (cube.IsEdgeSolved(crossColor, edgeColor))
            {
                solvedCount++;
            }
        }
        
        return solvedCount;
    }
    
    /// <summary>
    /// Check if a specific cross edge is in its solved position and orientation
    /// </summary>
    /// <param name="cube">The cube to check</param>
    /// <param name="crossColor">The cross color (typically white)</param>
    /// <param name="edgeColor">The edge color (green, orange, blue, red)</param>
    /// <returns>True if the edge is correctly placed and oriented</returns>
    public static bool IsEdgeSolved(this Cube cube, CubeColor crossColor, CubeColor edgeColor)
    {
        var caseType = CrossEdgeClassifier.ClassifyEdgePosition(cube, crossColor, edgeColor);
        return caseType == CrossEdgeCase.BottomFrontAligned; // After transformation, solved edges show as BottomFrontAligned
    }
    
    /// <summary>
    /// Solve a single cross edge using the systematic 24-case approach
    /// </summary>
    /// <param name="cube">The cube state to solve on</param>
    /// <param name="crossColor">The cross color being solved</param>
    /// <param name="edgeColor">The specific edge color to solve</param>
    /// <param name="preserveBottomLayer">Whether to preserve already-solved bottom layer edges</param>
    /// <returns>Solution result with algorithm and description</returns>
    public static SuggestionResult SolveSingleCrossEdge(this Cube cube, CubeColor crossColor, CubeColor edgeColor, bool preserveBottomLayer = false)
    {
        try
        {
            // Step 1: Classify the edge position and orientation
            var caseType = CrossEdgeClassifier.ClassifyEdgePosition(cube, crossColor, edgeColor);
            
            // Step 2: Get algorithm for this case
            var algorithm = CrossEdgeAlgorithms.GetAlgorithm(caseType, cube, crossColor, preserveBottomLayer, edgeColor);
            
            // Step 3: Compress algorithm to remove redundant moves
            var compressedAlgorithm = string.IsNullOrEmpty(algorithm) ? "" : AlgorithmCompressor.Compress(algorithm);
            
            // Step 4: Create description
            var description = $"Place {crossColor.ToString().ToLowerInvariant()}-{edgeColor.ToString().ToLowerInvariant()} edge";
            
            return new SuggestionResult(
                algorithm: compressedAlgorithm,
                description: description,
                nextStage: "cross"
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to solve {edgeColor.ToString().ToLowerInvariant()} edge: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Get human-readable color name
    /// </summary>
    /// <param name="color">The color to convert</param>
    /// <returns>Lowercase color name</returns>
    public static string GetColorName(this CubeColor color)
    {
        return color.ToString().ToLowerInvariant();
    }
}