using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition.Models;

namespace RubiksCube.Core.PatternRecognition;

/// <summary>
/// Analyzes the white cross stage (default cross color)
/// </summary>
public class CrossAnalyzer : IStageAnalyzer
{
    public string StageName => "cross";
    
    /// <summary>
    /// The cross color this analyzer checks for
    /// </summary>
    public CubeColor CrossColor { get; }
    
    public CrossAnalyzer(CubeColor crossColor = CubeColor.White)
    {
        CrossColor = crossColor;
    }
    
    public RecognitionResult? Analyze(Cube cube)
    {
        var crossEdges = GetCrossEdges(cube);
        var correctEdges = 0;
        var misplacedEdges = new List<Dictionary<string, object>>();
        
        foreach (var edge in crossEdges)
        {
            if (IsEdgeCorrectlyPlaced(edge))
            {
                correctEdges++;
            }
            else
            {
                // Track details about misplaced edge
                var edgeInfo = new Dictionary<string, object>
                {
                    ["colors"] = string.Join("", edge.Colors.Where(c => c.HasValue).Select(c => GetColorChar(c!.Value))),
                    ["current_position"] = edge.Position.ToString(),
                    ["solved_position"] = edge.SolvedPosition.ToString(),
                    ["oriented"] = IsEdgeOriented(edge)
                };
                misplacedEdges.Add(edgeInfo);
            }
        }
        
        // Only return result if we have some cross progress or if it's complete
        if (correctEdges == 0)
        {
            return null; // No cross progress
        }
        
        var details = new Dictionary<string, object>
        {
            ["cross_color"] = CrossColor.ToString().ToLowerInvariant(),
            ["correct_edges"] = correctEdges,
            ["total_edges"] = 4
        };
        
        if (misplacedEdges.Any())
        {
            details["misplaced_edges"] = misplacedEdges;
        }
        
        var description = correctEdges == 4 
            ? $"White cross complete"
            : $"White cross {correctEdges}/4 complete";
            
        return new RecognitionResult(
            stage: "cross",
            isComplete: correctEdges == 4,
            progress: correctEdges,
            total: 4,
            description: description,
            details: details
        );
    }
    
    /// <summary>
    /// Gets all edges that should be part of the cross
    /// </summary>
    private List<CubePiece> GetCrossEdges(Cube cube)
    {
        return cube.Edges.Where(edge => 
            edge.Colors.Any(c => c == CrossColor)
        ).ToList();
    }
    
    /// <summary>
    /// Checks if an edge is correctly placed and oriented for the cross
    /// </summary>
    private bool IsEdgeCorrectlyPlaced(CubePiece edge)
    {
        return edge.IsSolved; // In solved position with correct orientation
    }
    
    /// <summary>
    /// Checks if an edge is correctly oriented (cross color on correct face)
    /// </summary>
    private bool IsEdgeOriented(CubePiece edge)
    {
        // For white cross, white should be on the bottom face (Y = -1)
        // The Y coordinate color should be white
        var yIndex = GetYColorIndex(edge.Position);
        return edge.Colors[yIndex] == CrossColor;
    }
    
    /// <summary>
    /// Gets the index in Colors array that corresponds to the Y axis for current position
    /// </summary>
    private int GetYColorIndex(Position3D position)
    {
        // In our v3.0 system, Colors[1] is always the Y-axis color
        return 1;
    }
    
    /// <summary>
    /// Gets a single character representation of a color
    /// </summary>
    private char GetColorChar(CubeColor color)
    {
        return color switch
        {
            CubeColor.White => 'W',
            CubeColor.Yellow => 'Y', 
            CubeColor.Green => 'G',
            CubeColor.Blue => 'B',
            CubeColor.Red => 'R',
            CubeColor.Orange => 'O',
            _ => '?'
        };
    }
}