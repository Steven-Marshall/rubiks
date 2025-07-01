using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition.Models;

namespace RubiksCube.Core.PatternRecognition;

/// <summary>
/// Analyzes if the cube is completely solved
/// </summary>
public class SolvedAnalyzer : IStageAnalyzer
{
    public string StageName => "solved";
    
    public RecognitionResult? Analyze(Cube cube)
    {
        if (!cube.IsSolved)
        {
            return null; // Not in solved stage
        }
        
        return new RecognitionResult(
            stage: "solved",
            isComplete: true,
            progress: 1,
            total: 1,
            description: "Cube completely solved"
        );
    }
}