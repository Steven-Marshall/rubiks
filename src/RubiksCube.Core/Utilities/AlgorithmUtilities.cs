using RubiksCube.Core.Algorithms;
using RubiksCube.Core.Models;
using CSharpFunctionalExtensions;

namespace RubiksCube.Core.Utilities;

/// <summary>
/// Centralized utilities for algorithm operations
/// Replaces inconsistent string splitting and parsing approaches across the codebase
/// </summary>
public static class AlgorithmUtilities
{
    /// <summary>
    /// Counts moves in an algorithm string using proper parsing
    /// Replaces inconsistent string splitting approaches
    /// </summary>
    /// <param name="algorithm">The algorithm string to count moves for</param>
    /// <returns>Number of moves, or 0 if algorithm is null/empty/invalid</returns>
    public static int CountMoves(string algorithm)
    {
        if (string.IsNullOrWhiteSpace(algorithm)) 
            return 0;
            
        var parsed = Algorithm.Parse(algorithm);
        return parsed.IsSuccess ? parsed.Value.Length : 0;
    }
    
    /// <summary>
    /// Safely applies an algorithm to a cube with error handling
    /// Standardizes algorithm application across the codebase
    /// </summary>
    /// <param name="cube">The cube to apply the algorithm to</param>
    /// <param name="algorithm">The algorithm string to apply</param>
    /// <returns>Success result, or failure with error message</returns>
    public static Result ApplyAlgorithm(Cube cube, string algorithm)
    {
        if (string.IsNullOrWhiteSpace(algorithm))
            return Result.Success(); // Empty algorithm is valid (no moves)
            
        var parsed = Algorithm.Parse(algorithm);
        if (parsed.IsFailure)
            return Result.Failure($"Invalid algorithm: {parsed.Error}");
            
        try
        {
            foreach (var move in parsed.Value.Moves)
            {
                cube.ApplyMove(move);
            }
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to apply algorithm: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Safely parses an algorithm string with consistent error handling
    /// </summary>
    /// <param name="algorithm">The algorithm string to parse</param>
    /// <returns>Parsed algorithm or failure result</returns>
    public static Result<Algorithm> ParseAlgorithm(string algorithm)
    {
        return Algorithm.Parse(algorithm);
    }
    
    /// <summary>
    /// Calculate total move count for a collection of algorithm strings
    /// Uses proper parsing for accurate counting
    /// </summary>
    /// <param name="algorithms">Collection of algorithm strings</param>
    /// <returns>Total move count across all algorithms</returns>
    public static int CalculateTotalMoves(IEnumerable<string> algorithms)
    {
        return algorithms.Sum(CountMoves);
    }
}