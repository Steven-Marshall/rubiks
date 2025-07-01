using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition.Models;
using RubiksCube.Core.Solving;

namespace RubiksCube.Core.PatternRecognition;

/// <summary>
/// Main orchestrator for cube analysis using backward detection
/// </summary>
public class CubeStateAnalyzer
{
    private readonly List<IStageAnalyzer> _analyzers;
    private readonly List<ISolver> _solvers;
    
    public CubeStateAnalyzer()
    {
        // Initialize analyzers in backward order (solved → ... → cross → unsolved)
        _analyzers = new List<IStageAnalyzer>
        {
            new SolvedAnalyzer(),
            new CrossAnalyzer()
            // TODO: Add PLLAnalyzer, OLLAnalyzer, F2LAnalyzer
        };
        
        // Initialize corresponding solvers
        _solvers = new List<ISolver>
        {
            new SolvedSolver(),
            new CrossSolver()
            // TODO: Add PLLSolver, OLLSolver, F2LSolver
        };
    }
    
    /// <summary>
    /// Analyzes the cube state and provides suggestions using backward detection
    /// </summary>
    /// <param name="cube">The cube to analyze</param>
    /// <returns>Combined analysis result with recognition and suggestion</returns>
    public AnalysisResult Analyze(Cube cube)
    {
        // Backward analysis - try each stage in order
        foreach (var analyzer in _analyzers)
        {
            var recognition = analyzer.Analyze(cube);
            if (recognition != null)
            {
                // Found a matching stage, now get suggestion
                var solver = _solvers.FirstOrDefault(s => s.StageName == recognition.Stage);
                var suggestion = solver?.SuggestAlgorithm(cube, recognition);
                
                return new AnalysisResult(recognition, suggestion);
            }
        }
        
        // No stage matched - fallback to "unsolved"
        var unsolvedRecognition = new RecognitionResult(
            stage: "unsolved",
            isComplete: false,
            progress: 0,
            total: 1,
            description: "No clear CFOP progress detected"
        );
        
        // TODO: Create UnsolvedSolver for basic suggestions
        var unsolvedSuggestion = new SuggestionResult(
            algorithm: "F D R F'",
            description: "Start with white cross",
            nextStage: "cross"
        );
        
        return new AnalysisResult(unsolvedRecognition, unsolvedSuggestion);
    }
}