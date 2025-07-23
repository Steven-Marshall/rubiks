using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition.Models;
using RubiksCube.Core.Utilities;

namespace RubiksCube.Core.PatternRecognition;

/// <summary>
/// Analyzes if the cube is completely solved
/// </summary>
public class SolvedAnalyzer : IStageAnalyzer
{
    public string StageName => "solved";
    
    public RecognitionResult? Analyze(Cube cube)
    {
        CubeValidator.ValidateCanonicalOrientation(cube, nameof(SolvedAnalyzer));
        
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