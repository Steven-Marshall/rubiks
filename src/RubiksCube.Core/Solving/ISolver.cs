using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition.Models;

namespace RubiksCube.Core.Solving;

/// <summary>
/// Base interface for suggesting algorithms to solve specific CFOP stages
/// </summary>
public interface ISolver
{
    /// <summary>
    /// The CFOP stage this solver handles
    /// </summary>
    string StageName { get; }
    
    /// <summary>
    /// Suggests an algorithm to make progress from the current recognition result
    /// </summary>
    /// <param name="cube">The cube in its current state</param>
    /// <param name="recognition">The recognition result from analysis</param>
    /// <returns>Suggestion result with algorithm and description</returns>
    SuggestionResult? SuggestAlgorithm(Cube cube, RecognitionResult recognition);
}