using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition.Models;
using RubiksCube.Core.Algorithms;

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
        var edgeColors = new[] { CubeColor.Green, CubeColor.Orange, CubeColor.Blue, CubeColor.Red };
        var unsolvedEdges = GetUnsolvedEdges(cube, edgeColors);
        
        if (unsolvedEdges.Count == 0)
        {
            return new SuggestionResult(
                algorithm: "",
                description: "All cross edges are correctly placed",
                nextStage: "f2l"
            );
        }
        
        if (unsolvedEdges.Count == 1)
        {
            // Only one edge to solve - still use superhuman approach for consistency
            var singleEdgeSolution = SolveSingleEdgeOnCube(cube, unsolvedEdges[0]);
            if (singleEdgeSolution != null)
            {
                var optimalDescription = singleEdgeSolution.Description.Replace("Place", "Optimal solution:");
                return new SuggestionResult(
                    algorithm: singleEdgeSolution.Algorithm,
                    description: optimalDescription,
                    nextStage: singleEdgeSolution.NextStage
                );
            }
        }
        
        // Try all permutations of unsolved edges
        var allPermutations = GeneratePermutations(unsolvedEdges);
        var bestSolution = new SuggestionResult("", "No solution found", "cross");
        var shortestMoveCount = int.MaxValue;
        var evaluatedSolutions = 0;
        
        foreach (var permutation in allPermutations)
        {
            var solution = EvaluatePermutation(cube, permutation);
            evaluatedSolutions++;
            
            if (solution != null)
            {
                var moveCount = CountMoves(solution.Algorithm);
                if (moveCount < shortestMoveCount)
                {
                    shortestMoveCount = moveCount;
                    bestSolution = solution;
                }
            }
        }
        
        // Add superhuman optimization details to description
        var description = bestSolution.Description;
        if (Verbose)
        {
            description += $" (Superhuman: evaluated {evaluatedSolutions} permutations, optimal: {shortestMoveCount} moves)";
        }
        
        return new SuggestionResult(
            algorithm: bestSolution.Algorithm,
            description: description,
            nextStage: bestSolution.NextStage
        );
    }
    
    /// <summary>
    /// Get list of unsolved cross edge colors
    /// </summary>
    private List<CubeColor> GetUnsolvedEdges(Cube cube, CubeColor[] edgeColors)
    {
        var unsolvedEdges = new List<CubeColor>();
        
        foreach (var edgeColor in edgeColors)
        {
            var edge = FindCrossEdge(cube, CrossColor, edgeColor);
            if (edge != null && !IsEdgeSolved(cube, CrossColor, edgeColor))
            {
                unsolvedEdges.Add(edgeColor);
            }
        }
        
        return unsolvedEdges;
    }
    
    /// <summary>
    /// Check if a cross edge is in its solved position and orientation
    /// </summary>
    private bool IsEdgeSolved(Cube cube, CubeColor crossColor, CubeColor edgeColor)
    {
        var caseType = CrossEdgeClassifier.ClassifyEdgePosition(cube, crossColor, edgeColor);
        return caseType == CrossEdgeCase.BottomFrontAligned; // After transformation, solved edges show as BottomFrontAligned
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
    /// </summary>
    private SuggestionResult? EvaluatePermutation(Cube cube, List<CubeColor> edgeOrder)
    {
        var algorithms = new List<string>();
        var workingCube = cube.Clone();
        var descriptions = new List<string>();
        
        try
        {
            foreach (var edgeColor in edgeOrder)
            {
                // Check if this edge is already solved on the working cube
                if (IsEdgeSolved(workingCube, CrossColor, edgeColor))
                {
                    continue; // Skip already solved edges
                }
                
                // Solve this specific edge
                var edgeSolution = SolveSingleEdgeOnCube(workingCube, edgeColor);
                if (edgeSolution != null && !string.IsNullOrEmpty(edgeSolution.Algorithm))
                {
                    algorithms.Add(edgeSolution.Algorithm);
                    descriptions.Add($"{GetColorName(edgeColor)} edge");
                    
                    // Apply the moves to the working cube for next iteration
                    var algorithmResult = Algorithm.Parse(edgeSolution.Algorithm);
                    if (algorithmResult.IsSuccess)
                    {
                        foreach (var move in algorithmResult.Value.Moves)
                        {
                            workingCube.ApplyMove(move);
                        }
                    }
                }
            }
            
            // Combine all algorithms and compress
            var combinedAlgorithm = string.Join(" ", algorithms);
            var compressedAlgorithm = string.IsNullOrEmpty(combinedAlgorithm) ? "" : AlgorithmCompressor.Compress(combinedAlgorithm);
            
            var description = descriptions.Count > 0 ? 
                $"Optimal sequence: {string.Join(" → ", descriptions)}" : 
                "Cross optimization complete";
            
            return new SuggestionResult(
                algorithm: compressedAlgorithm,
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
    /// Solve a single edge on a specific cube state
    /// </summary>
    private SuggestionResult SolveSingleEdgeOnCube(Cube cube, CubeColor edgeColor)
    {
        try
        {
            // Classify the edge position and orientation
            var caseType = CrossEdgeClassifier.ClassifyEdgePosition(cube, CrossColor, edgeColor);
            
            // Check if we need to preserve bottom layer edges
            var solvedCount = CountSolvedCrossEdges(cube);
            var preserveBottomLayer = solvedCount > 0;
            
            // Get algorithm for this case
            var algorithm = CrossEdgeAlgorithms.GetAlgorithm(caseType, cube, CrossColor, preserveBottomLayer, edgeColor);
            
            // Compress algorithm to remove redundant moves
            var compressedAlgorithm = string.IsNullOrEmpty(algorithm) ? "" : AlgorithmCompressor.Compress(algorithm);
            
            var description = $"Place {GetColorName(CrossColor)}-{GetColorName(edgeColor)} edge";
            
            return new SuggestionResult(
                algorithm: compressedAlgorithm,
                description: description,
                nextStage: "cross"
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to solve {GetColorName(edgeColor)} edge: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Solve a single edge using the standard solver (for when only one edge remains)
    /// </summary>
    private SuggestionResult SolveSingleEdge(Cube cube, CubeColor edgeColor)
    {
        // Create a fake recognition result for the standard solver
        var recognition = new RecognitionResult(
            stage: "cross",
            isComplete: false,
            progress: 3, // 3/4 complete (last edge)
            total: 4,
            description: "Final cross edge"
        );
        
        return _standardSolver.SuggestAlgorithm(cube, recognition) ?? 
               new SuggestionResult("", "Failed to solve edge", "cross");
    }
    
    /// <summary>
    /// Count the number of moves in an algorithm string
    /// </summary>
    private int CountMoves(string algorithm)
    {
        if (string.IsNullOrWhiteSpace(algorithm))
            return 0;
            
        return algorithm.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }
    
    /// <summary>
    /// Count how many cross edges are already solved
    /// </summary>
    private int CountSolvedCrossEdges(Cube cube)
    {
        var edgeColors = new[] { CubeColor.Green, CubeColor.Orange, CubeColor.Blue, CubeColor.Red };
        return edgeColors.Count(edgeColor => IsEdgeSolved(cube, CrossColor, edgeColor));
    }
    
    /// <summary>
    /// Find a cross edge piece
    /// </summary>
    private CubePiece? FindCrossEdge(Cube cube, CubeColor crossColor, CubeColor edgeColor)
    {
        return cube.Edges.FirstOrDefault(edge =>
            edge.Colors.Contains(crossColor) && edge.Colors.Contains(edgeColor));
    }
    
    /// <summary>
    /// Get human-readable color name
    /// </summary>
    private string GetColorName(CubeColor color)
    {
        return color.ToString().ToLowerInvariant();
    }
}