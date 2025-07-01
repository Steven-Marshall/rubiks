using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition.Models;

namespace RubiksCube.Core.Solving;

/// <summary>
/// Provides suggestions when the cube is already solved
/// </summary>
public class SolvedSolver : ISolver
{
    public string StageName => "solved";
    
    public SuggestionResult? SuggestAlgorithm(Cube cube, RecognitionResult recognition)
    {
        if (recognition.Stage != "solved")
        {
            return null; // Not our stage
        }
        
        return new SuggestionResult(
            algorithm: "", // No moves needed
            description: "No moves needed - cube is solved!",
            nextStage: "solved"
        );
    }
}