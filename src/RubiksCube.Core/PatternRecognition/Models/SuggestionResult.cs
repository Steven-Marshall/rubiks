namespace RubiksCube.Core.PatternRecognition.Models;

/// <summary>
/// Represents an algorithm suggestion for making progress (Component 2: Algorithm Suggestion)
/// </summary>
public class SuggestionResult
{
    /// <summary>
    /// The suggested algorithm in Singmaster notation (empty if no moves needed)
    /// </summary>
    public string Algorithm { get; }
    
    /// <summary>
    /// Human-readable description of what the algorithm accomplishes
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// The stage that will be reached after applying this algorithm
    /// </summary>
    public string NextStage { get; }
    
    /// <summary>
    /// Additional suggestion-specific details (optional)
    /// </summary>
    public Dictionary<string, object> Details { get; }
    
    public SuggestionResult(
        string algorithm, 
        string description, 
        string nextStage,
        Dictionary<string, object>? details = null)
    {
        Algorithm = algorithm;
        Description = description;
        NextStage = nextStage;
        Details = details ?? new Dictionary<string, object>();
    }
}