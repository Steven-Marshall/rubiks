using RubiksCube.Core.Models;
using RubiksCube.Core.Extensions;
using RubiksCube.Core.Constants;
using RubiksCube.Core.Utilities;
using RubiksCube.Core.PatternRecognition.Models;
using RubiksCube.Core.Solving;

namespace RubiksCube.Core.Services;

/// <summary>
/// Result of analyzing a single cross edge
/// </summary>
public class EdgeAnalysisResult
{
    public CubeColor EdgeColor { get; init; }
    public string Algorithm { get; init; } = "";
    public int MoveCount { get; init; }
    public bool IsSolved { get; init; }
    public string Description { get; init; } = "";
}

/// <summary>
/// Centralized service for cross edge analysis and operations
/// Eliminates duplication across CrossSolver, SuperhumanCrossSolver, and extension methods
/// </summary>
public static class CrossEdgeService
{
    /// <summary>
    /// Centralized logic for determining preserve parameter
    /// Eliminates duplication across 3+ methods in different classes
    /// </summary>
    /// <param name="cube">The cube to analyze</param>
    /// <param name="crossColor">The cross color being solved</param>
    /// <returns>True if bottom layer should be preserved (when some edges are already solved)</returns>
    public static bool ShouldPreserveBottomLayer(Cube cube, CubeColor crossColor)
    {
        return cube.CountSolvedCrossEdges(crossColor) > 0;
    }
    
    /// <summary>
    /// Analyze a single cross edge with consistent logic
    /// Replaces duplicate analysis patterns in multiple classes
    /// </summary>
    /// <param name="cube">The cube to analyze</param>
    /// <param name="crossColor">The cross color being solved</param>
    /// <param name="edgeColor">The specific edge color to analyze</param>
    /// <param name="preserveOverride">Optional override for preserve parameter (for testing/debugging)</param>
    /// <returns>Analysis result with algorithm, move count, and status</returns>
    public static EdgeAnalysisResult AnalyzeEdge(Cube cube, CubeColor crossColor, 
        CubeColor edgeColor, bool? preserveOverride = null)
    {
        var preserve = preserveOverride ?? ShouldPreserveBottomLayer(cube, crossColor);
        var edge = cube.FindCrossEdge(crossColor, edgeColor);
        
        if (edge?.IsSolved == true)
        {
            return new EdgeAnalysisResult 
            {
                EdgeColor = edgeColor,
                Algorithm = "",
                MoveCount = 0,
                IsSolved = true,
                Description = $"{crossColor.GetColorName()}-{edgeColor.GetColorName()} edge already solved"
            };
        }
        
        var solution = cube.SolveSingleCrossEdge(crossColor, edgeColor, preserve);
        var algorithm = solution?.Algorithm ?? "";
        return new EdgeAnalysisResult 
        {
            EdgeColor = edgeColor,
            Algorithm = algorithm,
            MoveCount = AlgorithmUtilities.CountMoves(algorithm),
            IsSolved = false,
            Description = solution?.Description ?? ""
        };
    }
    
    /// <summary>
    /// Analyze all cross edges with consistent logic
    /// Replaces similar loops in multiple solver classes
    /// </summary>
    /// <param name="cube">The cube to analyze</param>
    /// <param name="crossColor">The cross color being solved</param>
    /// <returns>Analysis results for all 4 cross edges</returns>
    public static List<EdgeAnalysisResult> AnalyzeAllEdges(Cube cube, CubeColor crossColor)
    {
        return CrossConstants.StandardEdgeColors
            .Select(edgeColor => AnalyzeEdge(cube, crossColor, edgeColor))
            .ToList();
    }
    
    /// <summary>
    /// Find edge with shortest algorithm using consistent logic
    /// Replaces duplicate shortest-edge finding patterns
    /// </summary>
    /// <param name="cube">The cube to analyze</param>
    /// <param name="crossColor">The cross color being solved</param>
    /// <returns>Edge color with shortest algorithm, or null if all solved</returns>
    public static CubeColor? FindShortestEdge(Cube cube, CubeColor crossColor)
    {
        return AnalyzeAllEdges(cube, crossColor)
            .Where(e => !e.IsSolved)
            .OrderBy(e => e.MoveCount)
            .FirstOrDefault()?.EdgeColor;
    }
    
    /// <summary>
    /// Find next edge in fixed order (original analyze behavior)
    /// Replaces hardcoded edge finding logic
    /// </summary>
    /// <param name="cube">The cube to analyze</param>
    /// <param name="crossColor">The cross color being solved</param>
    /// <returns>First unsolved edge in standard order, or null if all solved</returns>
    public static CubeColor? FindNextEdgeInOrder(Cube cube, CubeColor crossColor)
    {
        return CrossConstants.StandardEdgeColors
            .FirstOrDefault(edgeColor => !cube.IsEdgeSolved(crossColor, edgeColor));
    }
    
    /// <summary>
    /// Get comprehensive analysis for a specific edge with debugging information
    /// Used for verbose outputs and detailed analysis
    /// </summary>
    /// <param name="cube">The cube to analyze</param>
    /// <param name="crossColor">The cross color being solved</param>
    /// <param name="edgeColor">The specific edge color</param>
    /// <returns>Detailed analysis result with case classification and algorithm details</returns>
    public static EdgeAnalysisResult AnalyzeEdgeWithDetails(Cube cube, CubeColor crossColor, CubeColor edgeColor)
    {
        var basicResult = AnalyzeEdge(cube, crossColor, edgeColor);
        
        if (basicResult.IsSolved)
        {
            return basicResult;
        }
        
        // Add detailed description with case information for debugging
        var caseType = CrossEdgeClassifier.ClassifyEdgePosition(cube, crossColor, edgeColor);
        var preserve = ShouldPreserveBottomLayer(cube, crossColor);
        
        var detailedDescription = $"{basicResult.Description} (Case: {caseType}, Preserve: {preserve})";
        
        return new EdgeAnalysisResult
        {
            EdgeColor = basicResult.EdgeColor,
            Algorithm = basicResult.Algorithm,
            MoveCount = basicResult.MoveCount,
            IsSolved = basicResult.IsSolved,
            Description = detailedDescription
        };
    }
    
    /// <summary>
    /// Count total solved cross edges
    /// Wrapper for consistent API
    /// </summary>
    /// <param name="cube">The cube to check</param>
    /// <param name="crossColor">The cross color</param>
    /// <returns>Number of solved cross edges (0-4)</returns>
    public static int CountSolvedEdges(Cube cube, CubeColor crossColor)
    {
        return cube.CountSolvedCrossEdges(crossColor);
    }
    
    /// <summary>
    /// Check if cross is complete (all 4 edges solved)
    /// </summary>
    /// <param name="cube">The cube to check</param>
    /// <param name="crossColor">The cross color</param>
    /// <returns>True if all cross edges are solved</returns>
    public static bool IsCrossComplete(Cube cube, CubeColor crossColor)
    {
        return CountSolvedEdges(cube, crossColor) == 4;
    }
}