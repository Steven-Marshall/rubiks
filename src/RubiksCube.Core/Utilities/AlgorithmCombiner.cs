namespace RubiksCube.Core.Utilities;

/// <summary>
/// Utility class for combining normalization moves with solving algorithms
/// </summary>
public static class AlgorithmCombiner
{
    /// <summary>
    /// Combines normalization moves with a solving algorithm.
    /// Returns empty string if no real algorithm is needed, regardless of normalization.
    /// Normalization is an internal implementation detail - if no solving moves are needed,
    /// the user should get an empty result.
    /// </summary>
    /// <param name="normalizationMoves">The moves needed to normalize cube orientation (can be empty)</param>
    /// <param name="algorithm">The actual solving algorithm (can be empty if no moves needed)</param>
    /// <returns>Combined algorithm string, or empty if no solving moves are needed</returns>
    public static string CombineNormalizationWithAlgorithm(string normalizationMoves, string algorithm)
    {
        // If no solving algorithm is needed, return empty regardless of normalization
        if (string.IsNullOrEmpty(algorithm))
        {
            return "";
        }
        
        // If there's a real algorithm, combine with normalization if present
        return string.IsNullOrEmpty(normalizationMoves) 
            ? algorithm 
            : $"{normalizationMoves} {algorithm}";
    }
}