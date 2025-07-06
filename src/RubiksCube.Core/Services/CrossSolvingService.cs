using RubiksCube.Core.Models;
using RubiksCube.Core.Extensions;
using RubiksCube.Core.Constants;
using RubiksCube.Core.Utilities;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Core.Services;

/// <summary>
/// Result of a complete cross solution with detailed information
/// </summary>
public class CrossSolutionResult
{
    public string Algorithm { get; init; } = "";
    public string VerboseOutput { get; init; } = "";
    public required Cube FinalCube { get; init; }
    public int TotalMoves { get; init; }
    public List<string> StepAlgorithms { get; init; } = new();
    public List<EdgeAnalysisResult> EdgeAnalysis { get; init; } = new();
}

/// <summary>
/// Centralized service for cross solving operations
/// Eliminates duplication between CrossSolver.SolveCompleteCross() and SuperhumanCrossSolver.EvaluatePermutation()
/// </summary>
public static class CrossSolvingService
{
    /// <summary>
    /// Unified cross solving logic used by both CrossSolver and SuperhumanCrossSolver
    /// Eliminates duplication between SolveCompleteCross and EvaluatePermutation
    /// </summary>
    /// <param name="cube">The cube to solve</param>
    /// <param name="crossColor">The cross color to solve for</param>
    /// <param name="edgeOrder">The order in which to solve edges</param>
    /// <param name="verbose">Whether to generate verbose output</param>
    /// <returns>Complete solution result with algorithm and diagnostic information</returns>
    public static CrossSolutionResult SolveWithOrder(Cube cube, CubeColor crossColor, 
        List<CubeColor> edgeOrder, bool verbose = false)
    {
        var workingCube = cube.Clone();
        var stepAlgorithms = new List<string>();
        var verboseLines = new List<string>();
        var edgeAnalysis = new List<EdgeAnalysisResult>();
        
        if (verbose)
        {
            verboseLines.Add("Solving cross with specified edge order...");
            verboseLines.Add($"Order: {string.Join(" → ", edgeOrder.Select(c => c.GetColorName()))}");
            verboseLines.Add("");
        }
        
        int stepNumber = 1;
        foreach (var edgeColor in edgeOrder)
        {
            var analysis = CrossEdgeService.AnalyzeEdge(workingCube, crossColor, edgeColor);
            edgeAnalysis.Add(analysis);
            
            if (analysis.IsSolved)
            {
                if (verbose)
                {
                    verboseLines.Add($"Step {stepNumber}: {crossColor.GetColorName()}-{edgeColor.GetColorName()} already solved");
                }
                continue;
            }
                
            if (verbose)
            {
                verboseLines.Add($"Step {stepNumber}: {crossColor.GetColorName()}-{edgeColor.GetColorName()} ({analysis.MoveCount} move{(analysis.MoveCount == 1 ? "" : "s")}) -> {analysis.Algorithm}");
            }
            
            var applyResult = AlgorithmUtilities.ApplyAlgorithm(workingCube, analysis.Algorithm);
            if (applyResult.IsFailure)
            {
                throw new InvalidOperationException($"Failed to apply algorithm '{analysis.Algorithm}': {applyResult.Error}");
            }
                
            stepAlgorithms.Add(analysis.Algorithm);
            
            if (verbose)
            {
                var newSolvedCount = CrossEdgeService.CountSolvedEdges(workingCube, crossColor);
                var remainingCount = 4 - newSolvedCount;
                verboseLines.Add($"Status: {newSolvedCount}/4 edges solved{(remainingCount > 0 ? $" ({remainingCount} remaining)" : "")}");
                verboseLines.Add("");
            }
            
            stepNumber++;
        }
        
        var finalAlgorithm = stepAlgorithms.Any() 
            ? AlgorithmCompressor.Compress(string.Join(" ", stepAlgorithms))
            : "";
        
        // Add final summary for verbose output
        if (verbose)
        {
            var totalMoves = AlgorithmUtilities.CountMoves(finalAlgorithm);
            var stepsUsed = stepAlgorithms.Count;
            
            if (stepsUsed == 0)
            {
                verboseLines.Add("Cross already solved!");
            }
            else
            {
                verboseLines.Add($"White cross complete! Total moves: {totalMoves}");
                if (!string.IsNullOrEmpty(finalAlgorithm))
                {
                    verboseLines.Add($"Full algorithm: {finalAlgorithm}");
                }
            }
        }
            
        return new CrossSolutionResult
        {
            Algorithm = finalAlgorithm,
            VerboseOutput = string.Join("\n", verboseLines),
            FinalCube = workingCube,
            TotalMoves = AlgorithmUtilities.CountMoves(finalAlgorithm),
            StepAlgorithms = stepAlgorithms,
            EdgeAnalysis = edgeAnalysis
        };
    }
    
    /// <summary>
    /// Solve using shortest-first order (current solve command behavior)
    /// Dynamically finds the shortest algorithm at each step
    /// </summary>
    /// <param name="cube">The cube to solve</param>
    /// <param name="crossColor">The cross color to solve for</param>
    /// <param name="verbose">Whether to generate verbose output</param>
    /// <returns>Solution using optimal shortest-first ordering</returns>
    public static CrossSolutionResult SolveOptimal(Cube cube, CubeColor crossColor, bool verbose = false)
    {
        var optimalOrder = GenerateOptimalOrder(cube, crossColor);
        return SolveWithOrder(cube, crossColor, optimalOrder, verbose);
    }
    
    /// <summary>
    /// Solve using fixed order (original analyze command behavior)
    /// Uses Green → Orange → Blue → Red order
    /// </summary>
    /// <param name="cube">The cube to solve</param>
    /// <param name="crossColor">The cross color to solve for</param>
    /// <param name="verbose">Whether to generate verbose output</param>
    /// <returns>Solution using fixed standard ordering</returns>
    public static CrossSolutionResult SolveFixedOrder(Cube cube, CubeColor crossColor, bool verbose = false)
    {
        var fixedOrder = CrossConstants.StandardEdgeColors.ToList();
        return SolveWithOrder(cube, crossColor, fixedOrder, verbose);
    }
    
    /// <summary>
    /// Generate optimal solve order by repeatedly finding shortest edge
    /// Used by SolveOptimal to create dynamic shortest-first ordering
    /// </summary>
    /// <param name="cube">The cube to analyze</param>
    /// <param name="crossColor">The cross color to solve for</param>
    /// <returns>List of edge colors in optimal solve order</returns>
    private static List<CubeColor> GenerateOptimalOrder(Cube cube, CubeColor crossColor)
    {
        var workingCube = cube.Clone();
        var order = new List<CubeColor>();
        
        while (order.Count < 4)
        {
            var shortestEdge = CrossEdgeService.FindShortestEdge(workingCube, crossColor);
            if (shortestEdge == null)
                break; // All edges are solved
                
            order.Add(shortestEdge.Value);
            
            // Apply this edge's solution to working cube for next iteration
            var analysis = CrossEdgeService.AnalyzeEdge(workingCube, crossColor, shortestEdge.Value);
            if (!string.IsNullOrEmpty(analysis.Algorithm))
            {
                var applyResult = AlgorithmUtilities.ApplyAlgorithm(workingCube, analysis.Algorithm);
                if (applyResult.IsFailure)
                {
                    // If we can't apply the algorithm, break to avoid infinite loop
                    break;
                }
            }
        }
        
        // Fill in any missing edges in standard order (edge case handling)
        foreach (var edgeColor in CrossConstants.StandardEdgeColors)
        {
            if (!order.Contains(edgeColor))
            {
                order.Add(edgeColor);
            }
        }
        
        return order;
    }
    
    /// <summary>
    /// Validate that a solution algorithm actually solves the cross
    /// Used for verification in superhuman solver
    /// </summary>
    /// <param name="originalCube">The original cube state</param>
    /// <param name="crossColor">The cross color</param>
    /// <param name="algorithm">The algorithm to validate</param>
    /// <returns>True if the algorithm solves the cross completely</returns>
    public static bool ValidateSolution(Cube originalCube, CubeColor crossColor, string algorithm)
    {
        if (string.IsNullOrWhiteSpace(algorithm))
        {
            // Empty algorithm is valid if cross is already complete
            return CrossEdgeService.IsCrossComplete(originalCube, crossColor);
        }
        
        var testCube = originalCube.Clone();
        var applyResult = AlgorithmUtilities.ApplyAlgorithm(testCube, algorithm);
        
        if (applyResult.IsFailure)
        {
            return false; // Algorithm couldn't be applied
        }
        
        return CrossEdgeService.IsCrossComplete(testCube, crossColor);
    }
    
    /// <summary>
    /// Get summary statistics for a solution
    /// Used for verbose output and optimization analysis
    /// </summary>
    /// <param name="solution">The solution to analyze</param>
    /// <returns>Human-readable summary string</returns>
    public static string GetSolutionSummary(CrossSolutionResult solution)
    {
        var solvedEdges = solution.EdgeAnalysis.Count(e => e.IsSolved);
        var unsolvedEdges = solution.EdgeAnalysis.Where(e => !e.IsSolved).ToList();
        
        if (unsolvedEdges.Count == 0)
        {
            return $"Cross already complete! All {solvedEdges} edges solved.";
        }
        
        var totalMoves = solution.TotalMoves;
        var stepCount = unsolvedEdges.Count;
        var averageMovesPerStep = stepCount > 0 ? (double)totalMoves / stepCount : 0;
        
        return $"Cross solved in {totalMoves} moves across {stepCount} steps (avg: {averageMovesPerStep:F1} moves/step)";
    }
}