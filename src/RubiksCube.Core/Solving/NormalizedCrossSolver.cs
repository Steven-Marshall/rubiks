using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition.Models;
using RubiksCube.Core.Utilities;
using RubiksCube.Core.Extensions;

namespace RubiksCube.Core.Solving;

/// <summary>
/// Wrapper around CrossSolver that handles cube normalization and provides
/// complete algorithms that work on the user's original cube orientation.
/// </summary>
public class NormalizedCrossSolver
{
    private readonly CrossSolver _solver;
    
    public NormalizedCrossSolver(CrossSolver solver)
    {
        _solver = solver;
    }
    
    /// <summary>
    /// Solves the complete cross with normalization handling.
    /// Returns algorithms that work on the user's original cube orientation.
    /// </summary>
    /// <param name="originalCube">The cube in any orientation</param>
    /// <param name="verbose">Whether to include verbose output</param>
    /// <returns>Solution with complete user-facing algorithm</returns>
    public (string Algorithm, string VerboseOutput, Cube FinalCube) SolveCompleteCross(Cube originalCube, bool verbose = false)
    {
        // Normalize cube for solving (white-bottom required)
        var normalizationResult = CubeNormalizer.NormalizeToWhiteBottom(originalCube);
        
        // Solve on normalized cube
        var solution = _solver.SolveCompleteCross(normalizationResult.NormalizedCube, verbose);
        
        // Combine normalization with solution algorithm using centralized logic
        var completeAlgorithm = AlgorithmCombiner.CombineNormalizationWithAlgorithm(
            normalizationResult.NormalizationMoves, 
            solution.Algorithm);
        
        // Enhanced verbose output if requested
        string enhancedVerboseOutput = solution.VerboseOutput;
        if (verbose && !string.IsNullOrEmpty(normalizationResult.NormalizationMoves))
        {
            var verboseLines = new List<string>();
            
            verboseLines.Add("Cube Normalization Applied");
            verboseLines.Add($"Rotation to canonical orientation: {normalizationResult.NormalizationMoves}");
            verboseLines.Add("(WORKAROUND: SuperhumanCrossSolver has green-front bug)");
            verboseLines.Add("");
            
            // Add original verbose output
            verboseLines.Add(solution.VerboseOutput);
            
            // Add final algorithm summary
            verboseLines.Add("");
            verboseLines.Add("Complete Algorithm for Your Cube");
            verboseLines.Add($"Final algorithm: {completeAlgorithm}");
            
            enhancedVerboseOutput = string.Join("\n", verboseLines);
        }
        
        return (
            completeAlgorithm,
            enhancedVerboseOutput,
            solution.FinalCube // Note: This is still the normalized cube's final state
        );
    }
    
    /// <summary>
    /// Suggests an algorithm for the current cross state with normalization handling.
    /// Returns algorithms that work on the user's original cube orientation.
    /// </summary>
    /// <param name="originalCube">The cube in any orientation</param>
    /// <param name="recognition">Recognition result for the cube state</param>
    /// <param name="selectionMode">Edge selection mode</param>
    /// <param name="specificEdge">Specific edge to target (optional)</param>
    /// <returns>Suggestion with complete user-facing algorithm</returns>
    public SuggestionResult? SuggestAlgorithm(Cube originalCube, RecognitionResult recognition, 
        CrossSolver.EdgeSelectionMode selectionMode = CrossSolver.EdgeSelectionMode.FixedOrder, 
        CubeColor? specificEdge = null)
    {
        // Normalize cube for solving (white-bottom required)
        var normalizationResult = CubeNormalizer.NormalizeToWhiteBottom(originalCube);
        
        // Get suggestion from solver
        var suggestion = _solver.SuggestAlgorithm(normalizationResult.NormalizedCube, recognition, selectionMode, specificEdge);
        
        if (suggestion == null)
        {
            return null; // No suggestion and no normalization-only results
        }
        
        // Combine normalization with suggestion algorithm using centralized logic
        var completeAlgorithm = AlgorithmCombiner.CombineNormalizationWithAlgorithm(
            normalizationResult.NormalizationMoves, 
            suggestion.Algorithm);
        
        return new SuggestionResult(
            completeAlgorithm,
            suggestion.Description,
            suggestion.NextStage,
            suggestion.Details
        );
    }
}

/// <summary>
/// Wrapper around SuperhumanCrossSolver that handles cube normalization.
/// </summary>
public class NormalizedSuperhumanCrossSolver
{
    private readonly SuperhumanCrossSolver _solver;
    
    public NormalizedSuperhumanCrossSolver(SuperhumanCrossSolver solver)
    {
        _solver = solver;
    }
    
    /// <summary>
    /// Suggests an algorithm using superhuman analysis with normalization handling.
    /// Returns algorithms that work on the user's original cube orientation.
    /// </summary>
    /// <param name="originalCube">The cube in any orientation</param>
    /// <param name="recognition">Recognition result for the cube state</param>
    /// <returns>Suggestion with complete user-facing algorithm</returns>
    public SuggestionResult? SuggestAlgorithm(Cube originalCube, RecognitionResult recognition)
    {
        // WORKAROUND: SuperhumanCrossSolver has a bug with non-green-front cubes
        // TODO: Fix SuperhumanCrossSolver to work with white-bottom only like CrossSolver
        var normalizationResult = CubeNormalizer.NormalizeToCanonical(originalCube);
        
        // Get suggestion from superhuman solver
        var suggestion = _solver.SuggestAlgorithm(normalizationResult.NormalizedCube, recognition);
        
        if (suggestion == null)
        {
            return null; // No suggestion and no normalization-only results
        }
        
        // Combine normalization with suggestion algorithm using centralized logic
        var completeAlgorithm = AlgorithmCombiner.CombineNormalizationWithAlgorithm(
            normalizationResult.NormalizationMoves, 
            suggestion.Algorithm);
        
        return new SuggestionResult(
            completeAlgorithm,
            suggestion.Description,
            suggestion.NextStage,
            suggestion.Details
        );
    }
    
    /// <summary>
    /// Suggests an algorithm with cross progress calculation handled internally.
    /// This method calculates the cross progress on the provided cube (assumed to be pre-normalized).
    /// </summary>
    /// <param name="normalizedCube">The cube in canonical orientation (white bottom, green front)</param>
    /// <param name="normalizationMoves">The moves that were applied to normalize the original cube</param>
    /// <param name="crossColor">The cross color to analyze</param>
    /// <param name="verbose">Whether to include enhanced verbose output</param>
    /// <returns>Suggestion with complete user-facing algorithm</returns>
    public SuggestionResult? SuggestAlgorithmWithProgress(Cube normalizedCube, string normalizationMoves, CubeColor crossColor, bool verbose = false)
    {
        // Cube is already normalized by HandleSolve() - no need to normalize again
        
        // Calculate cross progress on normalized cube
        var crossProgress = normalizedCube.CountSolvedCrossEdges(crossColor);
        
        // Create recognition result based on progress
        var recognition = new RecognitionResult(
            stage: "cross",
            isComplete: crossProgress == 4,
            progress: crossProgress,
            total: 4,
            description: $"Cross progress: {crossProgress}/4 edges"
        );
        
        // Get suggestion from superhuman solver
        var suggestion = _solver.SuggestAlgorithm(normalizedCube, recognition);
        
        if (suggestion == null)
        {
            return null; // No suggestion and no normalization-only results
        }
        
        // Combine normalization with suggestion algorithm using centralized logic
        var completeAlgorithm = AlgorithmCombiner.CombineNormalizationWithAlgorithm(
            normalizationMoves, 
            suggestion.Algorithm);
        
        // Enhance verbose output if requested
        string enhancedDescription = suggestion.Description;
        if (verbose && !string.IsNullOrEmpty(normalizationMoves))
        {
            var verboseLines = new List<string>();
            
            verboseLines.Add("Cube Normalization Applied");
            verboseLines.Add($"Rotation to canonical orientation: {normalizationMoves}");
            verboseLines.Add("(Applied by HandleSolve before superhuman analysis)");
            verboseLines.Add("");
            
            // Add original verbose output
            verboseLines.Add(suggestion.Description);
            
            // Add final algorithm summary
            verboseLines.Add("");
            verboseLines.Add("Complete Algorithm for Your Cube");
            verboseLines.Add($"Final algorithm: {completeAlgorithm}");
            
            enhancedDescription = string.Join("\n", verboseLines);
        }
        
        return new SuggestionResult(
            completeAlgorithm,
            enhancedDescription,
            suggestion.NextStage,
            suggestion.Details
        );
    }
}