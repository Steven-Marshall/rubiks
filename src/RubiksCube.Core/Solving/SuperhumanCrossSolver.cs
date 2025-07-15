using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition.Models;
using RubiksCube.Core.Algorithms;
using RubiksCube.Core.Extensions;
using RubiksCube.Core.Constants;
using RubiksCube.Core.Utilities;
using RubiksCube.Core.Services;
using System.Text;

namespace RubiksCube.Core.Solving;

/// <summary>
/// Superhuman cross solver that tries all possible edge solve orders to find the optimal solution
/// Uses brute force optimization to achieve truly optimal cross solutions
/// </summary>
public class SuperhumanCrossSolver : ISolver
{
    public string StageName => "cross";
    
    /// <summary>
    /// Cross color this solver works on
    /// </summary>
    public CubeColor CrossColor { get; }
    
    /// <summary>
    /// Whether to include verbose optimization details in descriptions
    /// </summary>
    public bool Verbose { get; set; } = false;
    
    /// <summary>
    /// The standard cross solver used for individual edge solutions
    /// </summary>
    private readonly CrossSolver _standardSolver;
    
    public SuperhumanCrossSolver(CubeColor crossColor = CubeColor.White)
    {
        CrossColor = crossColor;
        _standardSolver = new CrossSolver(crossColor);
    }
    
    public SuggestionResult? SuggestAlgorithm(Cube cube, RecognitionResult recognition)
    {
        if (recognition.Stage != "cross")
        {
            return null; // Not our stage
        }
        
        if (recognition.IsComplete)
        {
            // Cross is complete, suggest moving to F2L
            return new SuggestionResult(
                algorithm: "",
                description: "Cross complete! Look for F2L pairs (superhuman optimization)",
                nextStage: "f2l"
            );
        }
        
        // Find the optimal solution using all permutations
        var optimalSolution = FindOptimalCrossSolution(cube);
        
        return optimalSolution;
    }
    
    /// <summary>
    /// Find the optimal cross solution by trying all possible edge solve orders
    /// </summary>
    private SuggestionResult FindOptimalCrossSolution(Cube cube)
    {
        var edgeColors = CrossConstants.StandardEdgeColors;
        
        // Check if cross is already complete
        if (CrossEdgeService.IsCrossComplete(cube, CrossColor))
        {
            return new SuggestionResult(
                algorithm: "",
                description: "All cross edges are correctly placed",
                nextStage: "f2l"
            );
        }
        
        // Always try all permutations of all 4 edges for true optimization
        var allEdges = edgeColors.ToList();
        var allPermutations = GeneratePermutations(allEdges);
        var bestSolution = new SuggestionResult("", "No solution found", "cross");
        var shortestMoveCount = int.MaxValue;
        var evaluatedSolutions = 0;
        
        // Track all permutation results for verbose output
        var permutationResults = new List<PermutationResult>();
        
        foreach (var permutation in allPermutations)
        {
            var solution = EvaluatePermutation(cube, permutation);
            evaluatedSolutions++;
            
            if (solution != null)
            {
                var moveCount = AlgorithmUtilities.CountMoves(solution.Algorithm);
                
                // Track this permutation's result
                permutationResults.Add(new PermutationResult
                {
                    Permutation = permutation,
                    Solution = solution,
                    MoveCount = moveCount,
                    PermutationNumber = evaluatedSolutions
                });
                
                if (moveCount < shortestMoveCount)
                {
                    shortestMoveCount = moveCount;
                    bestSolution = solution;
                }
            }
        }
        
        // Generate description based on verbose mode
        var description = GenerateDescription(bestSolution, permutationResults, shortestMoveCount);
        
        return new SuggestionResult(
            algorithm: bestSolution.Algorithm,
            description: description,
            nextStage: bestSolution.NextStage
        );
    }
    
    
    /// <summary>
    /// Generate all permutations of edge colors to try different solve orders
    /// </summary>
    private IEnumerable<List<CubeColor>> GeneratePermutations(List<CubeColor> edges)
    {
        if (edges.Count <= 1)
        {
            yield return new List<CubeColor>(edges);
            yield break;
        }
        
        for (int i = 0; i < edges.Count; i++)
        {
            var current = edges[i];
            var remaining = edges.Where((_, index) => index != i).ToList();
            
            foreach (var permutation in GeneratePermutations(remaining))
            {
                var result = new List<CubeColor> { current };
                result.AddRange(permutation);
                yield return result;
            }
        }
    }
    
    /// <summary>
    /// Evaluate a specific permutation of edge solve order
    /// Now uses CrossSolvingService for unified logic with CrossSolver
    /// </summary>
    private SuggestionResult? EvaluatePermutation(Cube cube, List<CubeColor> edgeOrder)
    {
        try
        {
            // Use the unified CrossSolvingService to solve with this specific edge order
            var solution = CrossSolvingService.SolveWithOrder(cube, CrossColor, edgeOrder, verbose: false);
            
            // Validate the solution actually solves the cross
            if (!CrossSolvingService.ValidateSolution(cube, CrossColor, solution.Algorithm))
            {
                return null; // This permutation doesn't actually work
            }
            
            // Create description from the solved edges
            var solvedEdges = solution.EdgeAnalysis
                .Where(e => !e.IsSolved && !string.IsNullOrEmpty(e.Algorithm))
                .Select(e => $"{e.EdgeColor.GetColorName()} edge")
                .ToList();
            
            var description = solvedEdges.Count > 0 ? 
                $"Optimal sequence: {string.Join(" → ", solvedEdges)}" : 
                "Cross optimization complete";
            
            return new SuggestionResult(
                algorithm: solution.Algorithm,
                description: description,
                nextStage: "cross"
            );
        }
        catch
        {
            // If this permutation fails, return null to skip it
            return null;
        }
    }
    
    
    
    
    /// <summary>
    /// Generate description based on verbose mode and permutation results
    /// </summary>
    private string GenerateDescription(SuggestionResult bestSolution, List<PermutationResult> permutationResults, int shortestMoveCount)
    {
        if (!Verbose)
        {
            // Non-verbose: just return the basic description
            return bestSolution.Description;
        }
        
        // Verbose: show detailed permutation analysis
        var description = new StringBuilder();
        description.AppendLine($"Evaluating all 24 permutations for optimal cross solution...");
        description.AppendLine();
        
        var sortedResults = permutationResults.OrderBy(r => r.PermutationNumber).ToList();
        var bestMoveCount = permutationResults.Min(r => r.MoveCount);
        
        foreach (var result in sortedResults)
        {
            var permutationStr = string.Join(" → ", result.Permutation.Select(color => color.GetColorName()));
            var isOptimal = result.MoveCount == bestMoveCount ? " ⭐ OPTIMAL" : "";
            
            description.AppendLine($"Permutation {result.PermutationNumber}/24: {permutationStr}{isOptimal}");
            description.AppendLine($"  Total: {result.MoveCount} moves | Combined: {result.Solution.Algorithm}");
            description.AppendLine();
        }
        
        // Summary statistics
        description.AppendLine("Summary:");
        description.AppendLine($"- Best solution: {bestMoveCount} moves");
        if (permutationResults.Count > 0)
        {
            var worstMoveCount = permutationResults.Max(r => r.MoveCount);
            var averageMoveCount = permutationResults.Average(r => r.MoveCount);
            description.AppendLine($"- Worst solution: {worstMoveCount} moves");
            description.AppendLine($"- Average: {averageMoveCount:F1} moves");
        }
        
        var bestPermutation = sortedResults.FirstOrDefault(r => r.MoveCount == bestMoveCount);
        if (bestPermutation != null)
        {
            var optimalSequence = string.Join(" → ", bestPermutation.Permutation.Select(color => color.GetColorName()));
            description.AppendLine($"- Optimal sequence: {optimalSequence}");
            description.AppendLine($"- Selected algorithm: {bestSolution.Algorithm}");
        }
        
        return description.ToString().TrimEnd();
    }
    
    /// <summary>
    /// Helper class to track permutation results
    /// </summary>
    private class PermutationResult
    {
        public List<CubeColor> Permutation { get; set; } = new();
        public SuggestionResult Solution { get; set; } = new("", "", "");
        public int MoveCount { get; set; }
        public int PermutationNumber { get; set; }
    }
}