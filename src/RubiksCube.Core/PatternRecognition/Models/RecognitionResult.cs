namespace RubiksCube.Core.PatternRecognition.Models;

/// <summary>
/// Represents the result of analyzing a cube's current stage (Component 1: Recognition)
/// </summary>
public class RecognitionResult
{
    /// <summary>
    /// The CFOP stage identified (solved, pll, oll, f2l, cross, unsolved)
    /// </summary>
    public string Stage { get; }
    
    /// <summary>
    /// Whether this stage is complete
    /// </summary>
    public bool IsComplete { get; }
    
    /// <summary>
    /// Progress within the stage (e.g., 3/4 for cross)
    /// </summary>
    public int Progress { get; }
    
    /// <summary>
    /// Total possible progress for this stage
    /// </summary>
    public int Total { get; }
    
    /// <summary>
    /// Human-readable description of current state
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// Additional stage-specific details (optional)
    /// </summary>
    public Dictionary<string, object> Details { get; }
    
    public RecognitionResult(
        string stage, 
        bool isComplete, 
        int progress, 
        int total, 
        string description,
        Dictionary<string, object>? details = null)
    {
        Stage = stage;
        IsComplete = isComplete;
        Progress = progress;
        Total = total;
        Description = description;
        Details = details ?? new Dictionary<string, object>();
    }
}