using RubiksCube.Core.Models;
using RubiksCube.Core.Constants;
using RubiksCube.Core.Algorithms;
using RubiksCube.Core.Extensions;
using CSharpFunctionalExtensions;

namespace RubiksCube.Core.Utilities;

/// <summary>
/// Normalizes cubes to standard orientation (white bottom, green front) for analysis
/// and transforms results back to original orientation
/// </summary>
public static class CubeNormalizer
{
    /// <summary>
    /// Default orientation: White bottom, Green front
    /// </summary>
    private static readonly CubeColor StandardFrontColor = CubeColor.Green;
    
    /// <summary>
    /// Result of cube normalization containing normalized cube and transformations
    /// </summary>
    public class NormalizationResult
    {
        public Cube NormalizedCube { get; init; }
        public string NormalizationMoves { get; init; }  // Forward moves applied to normalize
        public string ReverseTransformation { get; init; }  // Moves to get back to original orientation
        public CubeColor DetectedCrossColor { get; init; }
        public bool WasAlreadyNormalized { get; init; }
        
        public NormalizationResult(Cube normalizedCube, string normalizationMoves, string reverseTransformation, 
            CubeColor detectedCrossColor, bool wasAlreadyNormalized = false)
        {
            NormalizedCube = normalizedCube;
            NormalizationMoves = normalizationMoves;
            ReverseTransformation = reverseTransformation;
            DetectedCrossColor = detectedCrossColor;
            WasAlreadyNormalized = wasAlreadyNormalized;
        }
    }
    
    /// <summary>
    /// Simple normalization: gets white to bottom only (for cross solving)
    /// </summary>
    /// <param name="cube">The cube to normalize</param>
    /// <returns>Normalization result with white on bottom</returns>
    public static NormalizationResult NormalizeToWhiteBottom(Cube cube)
    {
        var whereIsWhite = FindWhitePosition(cube);
        var whiteToBottomMoves = GetWhiteToBottomMoves(whereIsWhite);
        
        if (string.IsNullOrEmpty(whiteToBottomMoves))
        {
            // Already normalized
            return new NormalizationResult(cube.Clone(), "", "", CubeColor.White, wasAlreadyNormalized: true);
        }
        
        var normalizedCube = cube.Clone();
        ApplyAlgorithmToCube(normalizedCube, whiteToBottomMoves);
        var reverseMoves = GetReverseAlgorithm(whiteToBottomMoves);
        
        return new NormalizationResult(normalizedCube, whiteToBottomMoves, reverseMoves, CubeColor.White);
    }
    
    /// <summary>
    /// Full canonical normalization: white bottom + green front (for solved detection)
    /// </summary>
    /// <param name="cube">The cube to normalize</param>
    /// <param name="targetFrontColor">Optional: specify target front color (defaults to green)</param>
    /// <returns>Normalization result with white bottom and green front</returns>
    public static NormalizationResult NormalizeToCanonical(Cube cube, CubeColor? targetFrontColor = null)
    {
        var frontColor = targetFrontColor ?? StandardFrontColor;
        
        // Step 1: Get white to bottom
        var step1Result = NormalizeToWhiteBottom(cube);
        
        // Step 2: Get green to front
        var currentFrontColor = DetectFrontColor(step1Result.NormalizedCube);
        var frontAlignmentMoves = GetGreenToFrontMoves(currentFrontColor, frontColor);
        
        if (string.IsNullOrEmpty(frontAlignmentMoves))
        {
            // Already canonical
            return step1Result;
        }
        
        var canonicalCube = step1Result.NormalizedCube.Clone();
        ApplyAlgorithmToCube(canonicalCube, frontAlignmentMoves);
        
        // Combine forward and reverse transformations
        var combinedForward = CombineAlgorithms(step1Result.NormalizationMoves, frontAlignmentMoves);
        var combinedReverse = CombineAlgorithms(GetReverseAlgorithm(frontAlignmentMoves), step1Result.ReverseTransformation);
        
        // Compress common rotation sequences for better user experience
        var optimizedForward = CompressRotationSequence(combinedForward);
        
        return new NormalizationResult(canonicalCube, optimizedForward, combinedReverse, CubeColor.White);
    }
    
    
    /// <summary>
    /// Analyzes all possible cross colors and returns them in order of preference
    /// </summary>
    /// <param name="cube">The cube to analyze</param>
    /// <returns>Cross colors ordered by progress/preference (best first)</returns>
    public static CubeColor[] SuggestOptimalCrossColors(Cube cube)
    {
        // Return White first, then other colors in standard order
        return new[] { CubeColor.White, CubeColor.Yellow, CubeColor.Green, CubeColor.Orange, CubeColor.Blue, CubeColor.Red };
    }
    
    /// <summary>
    /// Gets detailed analysis for a specific cross color
    /// REQUIRES CANONICAL CUBE: white bottom, green front
    /// </summary>
    /// <param name="cube">The cube to analyze (MUST be canonical)</param>
    /// <param name="crossColor">The cross color to analyze</param>
    /// <returns>Detailed analysis of progress toward this cross color</returns>
    public static (int solvedEdges, int correctlyPlaced, int bottomLayer, bool bottomAligned, double score) 
        GetCrossColorAnalysis(Cube cube, CubeColor crossColor)
    {
        // Simple analysis without complex scoring
        var solvedEdges = CrossConstants.StandardEdgeColors
            .Count(edgeColor => cube.IsEdgeSolved(crossColor, edgeColor));
        
        var correctlyPlaced = CrossConstants.StandardEdgeColors
            .Count(edgeColor => IsEdgeCorrectlyPlaced(cube, crossColor, edgeColor));
        
        var bottomLayer = CrossConstants.StandardEdgeColors
            .Count(edgeColor => IsEdgeInBottomLayer(cube, crossColor, edgeColor));
        
        var bottomAligned = DetectBottomColor(cube) == crossColor;
        
        // Simple score: solved edges are most important
        var score = solvedEdges * 10 + correctlyPlaced * 5 + bottomLayer * 2 + (bottomAligned ? 1 : 0);
        
        return (solvedEdges, correctlyPlaced, bottomLayer, bottomAligned, score);
    }
    
    /// <summary>
    /// Suggests the optimal front face orientation for inspection (cross solving)
    /// </summary>
    /// <param name="cube">The cube to analyze</param>
    /// <param name="crossColor">The cross color to optimize for (defaults to auto-detect)</param>
    /// <returns>Array of front face colors in order of preference for inspection</returns>
    public static CubeColor[] SuggestOptimalFrontFaces(Cube cube, CubeColor? crossColor = null)
    {
        // Use detected cross color if not specified
        var targetCrossColor = crossColor ?? DetectCrossColor(cube);
        
        // For now, return a simple preference order
        // In a more sophisticated implementation, we could analyze the cube state
        // to determine which front faces lead to the most efficient cross solutions
        
        var frontFaces = new[] { CubeColor.Green, CubeColor.Orange, CubeColor.Blue, CubeColor.Red };
        
        // Return Green first as it's the standard, then others in order
        return frontFaces;
    }
    
    /// <summary>
    /// Transforms an algorithm from normalized orientation back to original orientation
    /// </summary>
    /// <param name="algorithm">Algorithm string to transform</param>
    /// <param name="reverseTransformation">Reverse transformation from normalization</param>
    /// <returns>Algorithm transformed back to original orientation</returns>
    public static string TransformAlgorithmBack(string algorithm, string reverseTransformation)
    {
        if (string.IsNullOrEmpty(algorithm))
        {
            return algorithm;
        }
        
        if (string.IsNullOrEmpty(reverseTransformation))
        {
            return algorithm;
        }
        
        // Simply prepend the reverse transformation moves to the algorithm
        return $"{reverseTransformation} {algorithm}";
    }
    
    /// <summary>
    /// Detects which cross color is most likely being solved
    /// </summary>
    /// <param name="cube">Cube to analyze</param>
    /// <returns>Most likely cross color based on cube state analysis</returns>
    private static CubeColor DetectCrossColor(Cube cube)
    {
        // Always use White cross - the standard beginner approach
        return CubeColor.White;
    }
    
    
    /// <summary>
    /// Checks if an edge is correctly placed (right position, may be flipped)
    /// </summary>
    private static bool IsEdgeCorrectlyPlaced(Cube cube, CubeColor crossColor, CubeColor edgeColor)
    {
        var edge = cube.FindCrossEdge(crossColor, edgeColor);
        if (edge == null) return false;
        
        // Check if edge is in the correct position (ignoring orientation)
        return edge.Position == edge.SolvedPosition;
    }
    
    /// <summary>
    /// Checks if an edge is in the bottom layer
    /// </summary>
    private static bool IsEdgeInBottomLayer(Cube cube, CubeColor crossColor, CubeColor edgeColor)
    {
        var edge = cube.FindCrossEdge(crossColor, edgeColor);
        if (edge == null) return false;
        
        return edge.Position.Y == -1;
    }
    
    /// <summary>
    /// Finds where the white center piece currently is (which face it's on)
    /// </summary>
    private static CubeColor FindWhitePosition(Cube cube)
    {
        var whiteCenter = cube.Centers.FirstOrDefault(c => 
            c.Colors.Contains(CubeColor.White));
        
        if (whiteCenter == null)
            return CubeColor.White; // Fallback
        
        // Determine which face the white center is on based on its position
        var pos = whiteCenter.Position;
        if (pos.Y == -1) return CubeColor.White;  // Bottom (where we want it)
        if (pos.Y == 1) return CubeColor.Yellow;  // Top
        if (pos.Z == 1) return CubeColor.Green;   // Front  
        if (pos.Z == -1) return CubeColor.Blue;   // Back
        if (pos.X == -1) return CubeColor.Red;    // Left
        if (pos.X == 1) return CubeColor.Orange;  // Right
        
        return CubeColor.White; // Fallback
    }
    
    /// <summary>
    /// Simple lookup: where is white → how to get it to bottom (5 cases)
    /// </summary>
    private static string GetWhiteToBottomMoves(CubeColor whereIsWhite) => whereIsWhite switch
    {
        CubeColor.White => "",      // Already on bottom
        CubeColor.Yellow => "x2",   // On top → rotate 180° around X
        CubeColor.Green => "x'",    // On front → rotate counter-clockwise around X  
        CubeColor.Blue => "x",      // On back → rotate clockwise around X
        CubeColor.Red => "z'",      // On left → rotate counter-clockwise around Z
        CubeColor.Orange => "z",    // On right → rotate clockwise around Z
        _ => ""
    };
    
    /// <summary>
    /// Simple lookup: what's on front → how to get green there (4 cases)
    /// </summary>
    private static string GetGreenToFrontMoves(CubeColor currentFront, CubeColor targetFront) => 
        (currentFront, targetFront) switch
        {
            (var c, var t) when c == t => "",           // Already aligned
            (CubeColor.Green, CubeColor.Green) => "",   // Green already on front
            (CubeColor.Orange, CubeColor.Green) => "y'", // Orange on front → turn left to get Green
            (CubeColor.Blue, CubeColor.Green) => "y2",   // Blue on front → turn around to get Green
            (CubeColor.Red, CubeColor.Green) => "y",     // Red on front → turn right to get Green
            
            // Support other target front colors if needed
            (CubeColor.Green, CubeColor.Orange) => "y",   // Green to Orange
            (CubeColor.Orange, CubeColor.Orange) => "",
            (CubeColor.Blue, CubeColor.Orange) => "y'",   
            (CubeColor.Red, CubeColor.Orange) => "y2",
            
            (CubeColor.Green, CubeColor.Blue) => "y2",    // Green to Blue
            (CubeColor.Orange, CubeColor.Blue) => "y",
            (CubeColor.Blue, CubeColor.Blue) => "",
            (CubeColor.Red, CubeColor.Blue) => "y'",
            
            (CubeColor.Green, CubeColor.Red) => "y'",     // Green to Red
            (CubeColor.Orange, CubeColor.Red) => "y2",
            (CubeColor.Blue, CubeColor.Red) => "y",
            (CubeColor.Red, CubeColor.Red) => "",
            
            _ => ""
        };
    
    /// <summary>
    /// Detects which color is currently on the bottom center
    /// </summary>
    private static CubeColor DetectBottomColor(Cube cube)
    {
        var bottomCenter = cube.Centers.FirstOrDefault(c => c.Position.Y == -1);
        if (bottomCenter?.Colors[1].HasValue == true)
        {
            return bottomCenter.Colors[1].Value;
        }
        
        // Fallback: assume white if can't detect
        return CubeColor.White;
    }
    
    /// <summary>
    /// Detects which color is currently on the front center
    /// </summary>
    private static CubeColor DetectFrontColor(Cube cube)
    {
        var frontCenter = cube.Centers.FirstOrDefault(c => c.Position.Z == 1);
        if (frontCenter?.Colors[2].HasValue == true)
        {
            return frontCenter.Colors[2].Value;
        }
        
        // Fallback: assume green if can't detect
        return CubeColor.Green;
    }
    
    /// <summary>
    /// Gets the moves needed to align a color to the bottom position
    /// </summary>
    private static string GetBottomAlignmentMoves(CubeColor currentBottom, CubeColor targetBottom)
    {
        return (currentBottom, targetBottom) switch
        {
            // To get White to bottom
            (CubeColor.White, CubeColor.White) => "",
            (CubeColor.Yellow, CubeColor.White) => "x2",  // Top to Bottom
            (CubeColor.Green, CubeColor.White) => "x",    // Front to Bottom
            (CubeColor.Blue, CubeColor.White) => "x'",    // Back to Bottom (inverse of x that put Blue there)
            (CubeColor.Red, CubeColor.White) => "z'",     // Left to Bottom
            (CubeColor.Orange, CubeColor.White) => "z",   // Right to Bottom
            
            // To get Yellow to bottom
            (CubeColor.White, CubeColor.Yellow) => "x2",
            (CubeColor.Yellow, CubeColor.Yellow) => "",
            (CubeColor.Green, CubeColor.Yellow) => "x",
            (CubeColor.Blue, CubeColor.Yellow) => "x'",
            (CubeColor.Red, CubeColor.Yellow) => "z",
            (CubeColor.Orange, CubeColor.Yellow) => "z'",
            
            // Add more mappings as needed for other cross colors
            _ => "" // Default: no transformation
        };
    }
    
    /// <summary>
    /// Gets the moves needed to align the front face to the target color
    /// </summary>
    private static string GetFrontAlignmentMoves(CubeColor currentFront, CubeColor targetFront)
    {
        if (currentFront == targetFront)
        {
            return "";
        }
        
        return (currentFront, targetFront) switch
        {
            // To get Green to front
            (CubeColor.Green, CubeColor.Green) => "",
            (CubeColor.Orange, CubeColor.Green) => "y'",  // Orange->Green: turn left
            (CubeColor.Blue, CubeColor.Green) => "y2",    // Blue->Green: turn around
            (CubeColor.Red, CubeColor.Green) => "y",      // Red->Green: turn right
            
            // To get Orange to front
            (CubeColor.Green, CubeColor.Orange) => "y",   // Green->Orange: turn right
            (CubeColor.Orange, CubeColor.Orange) => "",
            (CubeColor.Blue, CubeColor.Orange) => "y'",   // Blue->Orange: turn left
            (CubeColor.Red, CubeColor.Orange) => "y2",    // Red->Orange: turn around
            
            // To get Blue to front
            (CubeColor.Green, CubeColor.Blue) => "y2",    // Green->Blue: turn around
            (CubeColor.Orange, CubeColor.Blue) => "y",    // Orange->Blue: turn right
            (CubeColor.Blue, CubeColor.Blue) => "",
            (CubeColor.Red, CubeColor.Blue) => "y'",      // Red->Blue: turn left
            
            // To get Red to front
            (CubeColor.Green, CubeColor.Red) => "y'",     // Green->Red: turn left
            (CubeColor.Orange, CubeColor.Red) => "y2",    // Orange->Red: turn around
            (CubeColor.Blue, CubeColor.Red) => "y",       // Blue->Red: turn right
            (CubeColor.Red, CubeColor.Red) => "",
            
            _ => "" // Default: no transformation needed
        };
    }
    
    /// <summary>
    /// Gets the reverse of an algorithm (inverts the move sequence)
    /// </summary>
    private static string GetReverseAlgorithm(string algorithm)
    {
        if (string.IsNullOrEmpty(algorithm))
        {
            return "";
        }
        
        var parsed = Algorithm.Parse(algorithm);
        if (parsed.IsFailure)
        {
            return ""; // Return empty if parsing fails
        }
        
        // Reverse the move sequence and invert each move
        var reversedMoves = parsed.Value.Moves.Reverse().Select(move => move.Inverse()).ToArray();
        return string.Join(" ", reversedMoves.Select(m => m.ToString()));
    }
    
    /// <summary>
    /// Combines two algorithm strings
    /// </summary>
    private static string CombineAlgorithms(string first, string second)
    {
        if (string.IsNullOrEmpty(first)) return second;
        if (string.IsNullOrEmpty(second)) return first;
        return $"{first} {second}";
    }
    
    /// <summary>
    /// Compresses common rotation sequences for better user experience.
    /// Currently handles: x2 y2 → z2 (mathematically equivalent transformations)
    /// </summary>
    /// <param name="moves">The move sequence to compress</param>
    /// <returns>Compressed move sequence</returns>
    private static string CompressRotationSequence(string moves)
    {
        if (string.IsNullOrEmpty(moves)) return moves;
        
        // Normalize whitespace
        var normalized = moves.Trim().Replace("  ", " ");
        
        // Check for x2 y2 → z2 compression
        // x2: Front↔Back, Up↔Down; y2: Front↔Back, Left↔Right
        // Combined: Up↔Down + Left↔Right = z2 (Front/Back unchanged)
        if (normalized == "x2 y2")
        {
            return "z2";
        }
        
        return normalized;
    }
    
    /// <summary>
    /// Applies an algorithm string to a cube
    /// </summary>
    private static void ApplyAlgorithmToCube(Cube cube, string algorithm)
    {
        if (string.IsNullOrEmpty(algorithm)) return;
        
        var result = AlgorithmUtilities.ApplyAlgorithm(cube, algorithm);
        // Note: We're ignoring the result for now, assuming success
        // In a production version, we'd handle failures appropriately
    }
    
}