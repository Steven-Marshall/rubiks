using RubiksCube.Core.Models;
using RubiksCube.Core.PatternRecognition.Models;
using RubiksCube.Core.Algorithms;
using RubiksCube.Core.Extensions;
using RubiksCube.Core.Constants;
using RubiksCube.Core.Utilities;
using RubiksCube.Core.Services;

namespace RubiksCube.Core.Solving;

/// <summary>
/// Cross solver using systematic 24-case approach
/// Replaces old 6-case logic with comprehensive position-based analysis
/// </summary>
public class CrossSolver : ISolver
{
    public string StageName => "cross";
    
    /// <summary>
    /// Cross color this solver works on
    /// </summary>
    public CubeColor CrossColor { get; }
    
    /// <summary>
    /// Whether to include verbose case information in descriptions
    /// </summary>
    public bool Verbose { get; set; } = false;
    
    public CrossSolver(CubeColor crossColor = CubeColor.White)
    {
        CrossColor = crossColor;
    }
    
    public SuggestionResult? SuggestAlgorithm(Cube cube, RecognitionResult recognition)
    {
        return SuggestAlgorithm(cube, recognition, EdgeSelectionMode.FixedOrder, null);
    }
    
    /// <summary>
    /// Edge selection modes for analyze command
    /// </summary>
    public enum EdgeSelectionMode
    {
        FixedOrder,    // Original behavior: Green → Orange → Blue → Red
        Shortest       // New behavior: Pick edge with shortest algorithm
    }
    
    /// <summary>
    /// Suggest algorithm with customizable edge selection
    /// </summary>
    public SuggestionResult? SuggestAlgorithm(Cube cube, RecognitionResult recognition, EdgeSelectionMode selectionMode, CubeColor? specificEdge = null)
    {
        CubeValidator.ValidateWhiteOnBottom(cube, nameof(CrossSolver));
        
        if (recognition.Stage != "cross")
        {
            return null; // Not our stage
        }
        
        if (recognition.IsComplete)
        {
            // Cross is complete, suggest moving to F2L
            return new SuggestionResult(
                algorithm: "",
                description: "Cross complete! Look for F2L pairs",
                nextStage: "f2l"
            );
        }
        
        // Find the target edge based on selection mode
        CubeColor? targetEdgeColor;
        
        if (specificEdge.HasValue)
        {
            // Check if the specific edge is already solved
            var specificEdgePiece = cube.FindCrossEdge(CrossColor, specificEdge.Value);
            if (specificEdgePiece != null && specificEdgePiece.IsSolved)
            {
                return new SuggestionResult(
                    algorithm: "",
                    description: $"{CrossColor.GetColorName()}-{specificEdge.Value.GetColorName()} edge is already solved",
                    nextStage: "cross"
                );
            }
            targetEdgeColor = specificEdge.Value;
        }
        else if (selectionMode == EdgeSelectionMode.Shortest)
        {
            targetEdgeColor = CrossEdgeService.FindShortestEdge(cube, CrossColor);
        }
        else
        {
            targetEdgeColor = CrossEdgeService.FindNextEdgeInOrder(cube, CrossColor);
        }
        
        if (targetEdgeColor == null)
        {
            return new SuggestionResult(
                algorithm: "",
                description: "All cross edges are correctly placed",
                nextStage: "f2l"
            );
        }
        
        // Solve this specific edge using new 24-case system
        var solution = SolveSingleCrossEdge(cube, targetEdgeColor.Value);
        
        return solution;
    }
    
    
    
    /// <summary>
    /// Solve a single cross edge using the new 24-case systematic approach
    /// </summary>
    private SuggestionResult SolveSingleCrossEdge(Cube cube, CubeColor edgeColor)
    {
        try
        {
            // Step 1: Classify the edge position and orientation
            var caseType = CrossEdgeClassifier.ClassifyEdgePosition(cube, CrossColor, edgeColor);
            
            // Step 2: Check if we need to preserve bottom layer edges
            var preserveBottomLayer = CrossEdgeService.ShouldPreserveBottomLayer(cube, CrossColor);
            
            // Step 3: Get algorithm for this case
            var algorithm = CrossEdgeAlgorithms.GetAlgorithm(caseType, cube, CrossColor, preserveBottomLayer, edgeColor);
            
            // Step 4: Compress algorithm to remove redundant moves
            var compressedAlgorithm = string.IsNullOrEmpty(algorithm) ? "" : AlgorithmCompressor.Compress(algorithm);
            
            // Step 5: Create description
            var description = CreateDescription(edgeColor, caseType, preserveBottomLayer);
            
            return new SuggestionResult(
                algorithm: compressedAlgorithm,
                description: description,
                nextStage: "cross"
            );
        }
        catch (Exception ex)
        {
            // If classification fails, throw the exception rather than giving generic advice
            throw new InvalidOperationException($"Failed to classify {edgeColor.GetColorName()} edge: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Check if this is the first cross edge being solved (enables D-move optimizations)
    /// </summary>
    public bool IsFirstEdge(Cube cube, CubeColor currentEdgeColor)
    {
        var crossEdgeColors = CrossConstants.StandardEdgeColors;
        
        // Count how many edges are already solved
        int solvedCount = 0;
        foreach (var edgeColor in crossEdgeColors)
        {
            if (edgeColor == currentEdgeColor) continue; // Don't count current edge
            
            var edge = cube.FindCrossEdge(CrossColor, edgeColor);
            if (edge != null && edge.IsSolved)
            {
                solvedCount++;
            }
        }
        
        // First edge if no other edges are solved
        return solvedCount == 0;
    }
    
    /// <summary>
    /// Create human-readable description for the solution
    /// </summary>
    private string CreateDescription(CubeColor edgeColor, CrossEdgeCase caseType, bool preserveBottomLayer)
    {
        var edgeName = $"{CrossColor.GetColorName()}-{edgeColor.GetColorName()}";
        var baseDescription = $"Place {edgeName} edge";
        
        if (!Verbose)
        {
            return baseDescription;
        }
        
        // Verbose mode: include case information
        var caseDescription = GetCaseDescription(caseType);
        var preservation = preserveBottomLayer ? " (preserving bottom layer)" : "";
        
        return $"{baseDescription} - {caseDescription}{preservation}";
    }
    
    /// <summary>
    /// Get human-readable description of the case type
    /// </summary>
    private string GetCaseDescription(CrossEdgeCase caseType)
    {
        return caseType switch
        {
            // Bottom layer cases
            CrossEdgeCase.BottomFrontAligned => "bottom front, aligned",
            CrossEdgeCase.BottomFrontFlipped => "bottom front, flipped",
            CrossEdgeCase.BottomRightAligned => "bottom right, aligned", 
            CrossEdgeCase.BottomRightFlipped => "bottom right, flipped",
            CrossEdgeCase.BottomBackAligned => "bottom back, aligned",
            CrossEdgeCase.BottomBackFlipped => "bottom back, flipped",
            CrossEdgeCase.BottomLeftAligned => "bottom left, aligned",
            CrossEdgeCase.BottomLeftFlipped => "bottom left, flipped",
            
            // Middle layer cases
            CrossEdgeCase.MiddleFrontRightAligned => "middle front-right, aligned",
            CrossEdgeCase.MiddleFrontRightFlipped => "middle front-right, flipped",
            CrossEdgeCase.MiddleRightBackAligned => "middle right-back, aligned",
            CrossEdgeCase.MiddleRightBackFlipped => "middle right-back, flipped", 
            CrossEdgeCase.MiddleBackLeftAligned => "middle back-left, aligned",
            CrossEdgeCase.MiddleBackLeftFlipped => "middle back-left, flipped",
            CrossEdgeCase.MiddleLeftFrontAligned => "middle left-front, aligned",
            CrossEdgeCase.MiddleLeftFrontFlipped => "middle left-front, flipped",
            
            // Top layer cases
            CrossEdgeCase.TopFrontAligned => "top front, aligned",
            CrossEdgeCase.TopFrontFlipped => "top front, flipped",
            CrossEdgeCase.TopRightAligned => "top right, aligned", 
            CrossEdgeCase.TopRightFlipped => "top right, flipped",
            CrossEdgeCase.TopBackAligned => "top back, aligned",
            CrossEdgeCase.TopBackFlipped => "top back, flipped",
            CrossEdgeCase.TopLeftAligned => "top left, aligned",
            CrossEdgeCase.TopLeftFlipped => "top left, flipped",
            
            _ => $"case {caseType}"
        };
    }
    
    
    /// <summary>
    /// Debug method to analyze all 4 cross edges
    /// </summary>
    public string AnalyzeAllCrossEdges(Cube cube, bool verbose = false)
    {
        var results = new List<string>();
        var edgeColors = CrossConstants.StandardEdgeColors;
        
        foreach (var edgeColor in edgeColors)
        {
            try
            {
                // Find the actual edge position
                var edge = cube.FindCrossEdge(CrossColor, edgeColor);
                if (edge == null)
                {
                    results.Add($"{CrossColor.GetColorName()}-{edgeColor.GetColorName()}: NOT FOUND");
                    continue;
                }
                
                // Get the classification (from edge color's perspective)
                var caseType = CrossEdgeClassifier.ClassifyEdgePosition(cube, CrossColor, edgeColor);
                var solvedCount = cube.CountSolvedCrossEdges(CrossColor);
                var preserveBottomLayer = solvedCount > 0;
                var baseAlgorithm = CrossEdgeAlgorithms.GetRawAlgorithmForDebugging(caseType);
                var algorithm = CrossEdgeAlgorithms.GetAlgorithm(caseType, cube, CrossColor, preserveBottomLayer, edgeColor);
                var algorithmText = string.IsNullOrEmpty(algorithm) ? "None needed" : algorithm;
                
                // Calculate move count
                int moveCount = 0;
                if (!string.IsNullOrEmpty(algorithm))
                {
                    var parsedAlgo = Algorithm.Parse(algorithm);
                    if (parsedAlgo.IsSuccess)
                    {
                        moveCount = parsedAlgo.Value.Length;
                    }
                }
                
                // Describe actual position in human-readable form
                var actualPosition = DescribeEdgePosition(edge);
                
                if (verbose)
                {
                    // Verbose mode: show canonical position, face view classification, preserve info, and algorithm transformation
                    var moveInfo = moveCount > 0 ? $" ({moveCount} move{(moveCount == 1 ? "" : "s")})" : "";
                    var preserveInfo = preserveBottomLayer ? $" [Preserve: Yes ({solvedCount} solved)]" : " [Preserve: No]";
                    results.Add($"{CrossColor.GetColorName()}-{edgeColor.GetColorName()}: Canonical {actualPosition} (Face view {caseType}){preserveInfo} -> Base: \"{baseAlgorithm}\" -> Final: {algorithmText}{moveInfo}");
                }
                else
                {
                    // Basic mode: just show canonical position
                    results.Add($"{CrossColor.GetColorName()}-{edgeColor.GetColorName()}: {actualPosition} -> {algorithmText}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"{CrossColor.GetColorName()}-{edgeColor.GetColorName()}: ERROR - {ex.Message}");
            }
        }
        
        return string.Join("\n", results);
    }
    
    /// <summary>
    /// Describes an edge's position in human-readable form from canonical (green front) perspective
    /// </summary>
    private string DescribeEdgePosition(CubePiece edge)
    {
        var pos = edge.Position;
        var description = "";
        
        // Determine layer
        if (pos.Y == 1)
            description = "top-";
        else if (pos.Y == -1)
            description = "bottom-";
        else
            description = "middle-";
        
        // Determine position within layer
        if (pos.Y == 0)
        {
            // Middle layer edges
            if (pos.X == 1 && pos.Z == 1) description += "front-right";
            else if (pos.X == 1 && pos.Z == -1) description += "back-right";
            else if (pos.X == -1 && pos.Z == -1) description += "back-left";
            else if (pos.X == -1 && pos.Z == 1) description += "front-left";
        }
        else
        {
            // Top/Bottom layer edges
            if (pos.Z == 1) description += "front";
            else if (pos.Z == -1) description += "back";
            else if (pos.X == 1) description += "right";
            else if (pos.X == -1) description += "left";
        }
        
        // Check orientation for cross edges
        if (edge.Colors.Contains(CrossColor))
        {
            var crossColorIndex = Array.IndexOf(edge.Colors, CrossColor);
            bool isAligned = false;
            
            // For bottom layer, white should be facing down (Y index)
            if (pos.Y == -1 && crossColorIndex == 1)
                isAligned = true;
            // For top layer, white should be facing up (Y index)  
            else if (pos.Y == 1 && crossColorIndex == 1)
                isAligned = true;
            // For middle layer, orientation depends on position
            else if (pos.Y == 0)
            {
                // Check if cross color is on the correct face
                if ((pos.X != 0 && crossColorIndex == 0) || (pos.Z != 0 && crossColorIndex == 2))
                    isAligned = false; // Cross color is on side
                else
                    isAligned = true; // Cross color is on top/bottom
            }
            
            description += isAligned ? " (aligned)" : " (flipped)";
        }
        
        return description;
    }
    
    /// <summary>
    /// Solve the complete cross by applying algorithms until all edges are solved
    /// Now uses CrossSolvingService for unified logic with SuperhumanCrossSolver
    /// </summary>
    /// <param name="cube">The cube to solve</param>
    /// <param name="verbose">Whether to include verbose step-by-step output</param>
    /// <returns>Tuple containing final algorithm, verbose output, and resulting cube state</returns>
    public (string Algorithm, string VerboseOutput, Cube FinalCube) SolveCompleteCross(Cube cube, bool verbose = false)
    {
        // Use the new unified CrossSolvingService for shortest-first solving
        var solution = CrossSolvingService.SolveOptimal(cube, CrossColor, verbose);
        
        return (solution.Algorithm, solution.VerboseOutput, solution.FinalCube);
    }
}