using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition.Models;

namespace RubiksCube.Core.PatternRecognition;

/// <summary>
/// Base interface for analyzing specific CFOP stages
/// </summary>
public interface IStageAnalyzer
{
    /// <summary>
    /// The CFOP stage this analyzer handles
    /// </summary>
    string StageName { get; }
    
    /// <summary>
    /// Analyzes if the cube is in this stage and provides details
    /// </summary>
    /// <param name="cube">The cube to analyze</param>
    /// <returns>Recognition result with stage details, or null if not in this stage</returns>
    RecognitionResult? Analyze(Cube cube);
}