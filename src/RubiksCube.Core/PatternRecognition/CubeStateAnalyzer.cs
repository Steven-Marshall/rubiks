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
        
        // No stage matched - this indicates a problem with the analysis system
        var unsolvedRecognition = new RecognitionResult(
            stage: "unknown",
            isComplete: false,
            progress: 0,
            total: 1,
            description: "Unable to identify current solving stage"
        );
        
        // Don't provide random moves - report the analysis failure instead
        var unsolvedSuggestion = new SuggestionResult(
            algorithm: "",
            description: "Analysis failed: No stage analyzer could identify the cube state. The cube may be in an invalid state or missing stage analyzers.",
            nextStage: "unknown"
        );
        
        return new AnalysisResult(unsolvedRecognition, unsolvedSuggestion);
    }
    
    /// <summary>
    /// Analyzes the cube state with detailed verbose information about the analysis process
    /// </summary>
    /// <param name="cube">The cube to analyze</param>
    /// <returns>Analysis result with detailed diagnostic information</returns>
    public (AnalysisResult Result, string VerboseDetails) AnalyzeWithDetails(Cube cube)
    {
        var details = new List<string>();
        details.Add("Cube Analysis - Backward Detection Process");
        details.Add("============================================");
        details.Add("");
        
        // Try each analyzer and record the results
        foreach (var analyzer in _analyzers)
        {
            details.Add($"Trying {analyzer.GetType().Name}...");
            
            var recognition = analyzer.Analyze(cube);
            if (recognition != null)
            {
                details.Add($"  ✓ SUCCESS: {recognition.Description}");
                details.Add($"  Stage: {recognition.Stage}");
                details.Add($"  Progress: {recognition.Progress}/{recognition.Total}");
                details.Add($"  Complete: {(recognition.IsComplete ? "Yes" : "No")}");
                
                // Add detailed information from recognition
                if (recognition.Details != null && recognition.Details.Any())
                {
                    details.Add("  Details:");
                    foreach (var detail in recognition.Details)
                    {
                        if (detail.Key == "misplaced_edges" && detail.Value is List<Dictionary<string, object>> edges)
                        {
                            details.Add($"    Misplaced edges: {edges.Count}");
                            foreach (var edge in edges)
                            {
                                var colors = edge.ContainsKey("colors") ? edge["colors"].ToString() : "??";
                                var position = edge.ContainsKey("current_position") ? edge["current_position"].ToString() : "??";
                                var oriented = edge.ContainsKey("oriented") ? edge["oriented"].ToString() : "??";
                                details.Add($"      {colors} at {position} (oriented: {oriented})");
                            }
                        }
                        else
                        {
                            details.Add($"    {detail.Key}: {detail.Value}");
                        }
                    }
                }
                details.Add("");
                
                // Found a matching stage, now get suggestion
                var solver = _solvers.FirstOrDefault(s => s.StageName == recognition.Stage);
                if (solver != null)
                {
                    details.Add($"Using {solver.GetType().Name} for suggestions...");
                    var suggestion = solver.SuggestAlgorithm(cube, recognition);
                    
                    if (suggestion != null)
                    {
                        details.Add($"  Algorithm: {(string.IsNullOrEmpty(suggestion.Algorithm) ? "None needed" : suggestion.Algorithm)}");
                        details.Add($"  Description: {suggestion.Description}");
                        details.Add($"  Next stage: {suggestion.NextStage}");
                        
                        // Add solver-specific details if available
                        if (suggestion.Details != null && suggestion.Details.Any())
                        {
                            details.Add("  Solver details:");
                            foreach (var detail in suggestion.Details)
                            {
                                details.Add($"    {detail.Key}: {detail.Value}");
                            }
                        }
                    }
                    else
                    {
                        details.Add("  No suggestion generated");
                    }
                    
                    return (new AnalysisResult(recognition, suggestion), string.Join("\n", details));
                }
                else
                {
                    details.Add($"  ✗ ERROR: No solver found for stage '{recognition.Stage}'");
                    return (new AnalysisResult(recognition, null), string.Join("\n", details));
                }
            }
            else
            {
                details.Add($"  ✗ FAILED: No match for this stage");
            }
            details.Add("");
        }
        
        // No stage matched
        details.Add("Analysis Result: No stage analyzers matched");
        details.Add("This indicates either:");
        details.Add("  - Invalid cube state");
        details.Add("  - Missing stage analyzer implementations");
        details.Add("  - Bug in stage detection logic");
        
        var unsolvedRecognition = new RecognitionResult(
            stage: "unknown",
            isComplete: false,
            progress: 0,
            total: 1,
            description: "Unable to identify current solving stage"
        );
        
        var unsolvedSuggestion = new SuggestionResult(
            algorithm: "",
            description: "Analysis failed: No stage analyzer could identify the cube state. The cube may be in an invalid state or missing stage analyzers.",
            nextStage: "unknown"
        );
        
        return (new AnalysisResult(unsolvedRecognition, unsolvedSuggestion), string.Join("\n", details));
    }
}