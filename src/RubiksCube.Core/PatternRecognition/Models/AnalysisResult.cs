namespace RubiksCube.Core.PatternRecognition.Models;

/// <summary>
/// Combined result containing both recognition and suggestion components
/// </summary>
public class AnalysisResult
{
    /// <summary>
    /// Component 1: Recognition result
    /// </summary>
    public RecognitionResult Recognition { get; }
    
    /// <summary>
    /// Component 2: Algorithm suggestion  
    /// </summary>
    public SuggestionResult? Suggestion { get; }
    
    public AnalysisResult(RecognitionResult recognition, SuggestionResult? suggestion = null)
    {
        Recognition = recognition;
        Suggestion = suggestion;
    }
    
    /// <summary>
    /// Gets a human-readable summary of the analysis
    /// </summary>
    public string GetSummary()
    {
        var summary = Recognition.Description;
        
        if (Suggestion != null)
        {
            summary += $"\nSuggested: {Suggestion.Algorithm}";
            if (!string.IsNullOrEmpty(Suggestion.Description))
            {
                summary += $" ({Suggestion.Description})";
            }
        }
        
        return summary;
    }
}