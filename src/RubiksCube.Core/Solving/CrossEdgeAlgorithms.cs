using System.Linq;
using System.Text;
using RubiksCube.Core.Models;
using RubiksCube.Core.Algorithms;
using RubiksCube.Core.Constants;

namespace RubiksCube.Core.Solving;

/// <summary>
/// Provides algorithms for all 24 cross edge cases with conditional restoration logic
/// Algorithms stored exactly as documented in crossalgo.md with [restoration] notation
/// </summary>
public static class CrossEdgeAlgorithms
{
    /// <summary>
    /// Gets the algorithm for a specific cross edge case
    /// </summary>
    /// <param name="caseType">The classified edge case</param>
    /// <param name="cube">Current cube state (for conditional restoration checking)</param>
    /// <param name="crossColor">Cross color being solved</param>
    /// <param name="preserveBottomLayer">True if bottom layer edges should be preserved (uses U-layer algorithms)</param>
    /// <param name="edgeColor">Edge color for algorithm transformation</param>
    /// <returns>Algorithm string with conditionals resolved and transformed for edge perspective</returns>
    public static string GetAlgorithm(CrossEdgeCase caseType, Cube? cube = null, CubeColor crossColor = CubeColor.White, bool preserveBottomLayer = false, CubeColor edgeColor = CubeColor.Green)
    {
        var rawAlgorithm = GetRawAlgorithm(caseType, preserveBottomLayer);
        
        // DEBUG: Uncomment to trace algorithm processing
        // Console.Error.WriteLine($"[DEBUG] Raw: {rawAlgorithm}");
        
        // Transform algorithm based on edge color perspective (keeping brackets intact)
        var transformedAlgorithm = TransformAlgorithmForEdgeColor(rawAlgorithm, edgeColor);
        
        // DEBUG: Uncomment to trace algorithm processing
        // Console.Error.WriteLine($"[DEBUG] Transformed: {transformedAlgorithm}");
        
        // Process conditional restoration brackets [...] AFTER transformation
        var finalAlgorithm = ProcessConditionalRestorations(transformedAlgorithm, cube, crossColor);
        
        // DEBUG: Uncomment to trace algorithm processing
        // Console.Error.WriteLine($"[DEBUG] Final: {finalAlgorithm}");
        
        return finalAlgorithm;
    }
    
    /// <summary>
    /// Gets the raw algorithm for debugging purposes (without conditional processing)
    /// </summary>
    public static string GetRawAlgorithmForDebugging(CrossEdgeCase caseType)
    {
        return GetRawAlgorithm(caseType, preserveBottomLayer: false);
    }
    
    /// <summary>
    /// Gets the raw algorithm with conditional notation intact
    /// </summary>
    private static string GetRawAlgorithm(CrossEdgeCase caseType, bool preserveBottomLayer)
    {
        return caseType switch
        {
            // Bottom Layer Cases (Y = -1)
            CrossEdgeCase.BottomFrontAligned => "",                                    // Case 1a: Front position, aligned
            CrossEdgeCase.BottomFrontFlipped => "D R [D' restores bottom layer] F",   // Case 1b: Front position, flipped
            
            // Case 1c: Right position - use U-layer algorithm if preserving bottom
            CrossEdgeCase.BottomRightAligned => preserveBottomLayer ? "R2 U F2" : "D'", // Case 1c: Right position, aligned
            CrossEdgeCase.BottomRightFlipped => "R F",                                  // Case 1d: Right position, flipped
            
            // Case 1e: Back position - use U-layer algorithm if preserving bottom
            CrossEdgeCase.BottomBackAligned => preserveBottomLayer ? "B2 U2 F2" : "D2", // Case 1e: Back position, aligned
            CrossEdgeCase.BottomBackFlipped => "D' R [D restores bottom layer] F",     // Case 1f: Back position, flipped
            
            // Case 1g: Left position - use U-layer algorithm if preserving bottom
            CrossEdgeCase.BottomLeftAligned => preserveBottomLayer ? "L2 U' F2" : "D",  // Case 1g: Left position, aligned
            CrossEdgeCase.BottomLeftFlipped => "L' F'",                                 // Case 1h: Left position, flipped
            
            // Middle Layer Cases (Y = 0)
            CrossEdgeCase.MiddleFrontRightAligned => "F",                              // Case 2a: Front-right aligned
            CrossEdgeCase.MiddleFrontRightFlipped => "R U [R' restores (1,-1,0)] F2", // Case 2b: Front-right flipped
            CrossEdgeCase.MiddleRightBackAligned => "R2 F [R2 restores (1,0,1)]",     // Case 2c: Right-back aligned
            CrossEdgeCase.MiddleRightBackFlipped => "R' U [R restores (1,1,0)] F2",   // Case 2d: Right-back flipped
            CrossEdgeCase.MiddleBackLeftAligned => "L2 F' [L2 restores (-1,0,1)]",    // Case 2e: Back-left aligned
            CrossEdgeCase.MiddleBackLeftFlipped => "L U' [L' restores (-1,1,0)] F2",  // Case 2f: Back-left flipped
            CrossEdgeCase.MiddleLeftFrontAligned => "F'",                              // Case 2g: Left-front aligned
            CrossEdgeCase.MiddleLeftFrontFlipped => "L' U' [L restores (-1,1,0)] F2", // Case 2h: Left-front flipped
            
            // Top Layer Cases (Y = 1)
            CrossEdgeCase.TopFrontAligned => "F2",                                     // Case 3a: Top-front aligned
            CrossEdgeCase.TopFrontFlipped => "U' R' F [R restores (1,0,1)]",          // Case 3b: Top-front flipped
            CrossEdgeCase.TopRightAligned => "U F2",                                   // Case 3c: Top-right aligned
            CrossEdgeCase.TopRightFlipped => "R' F [R restores (1,0,1)]",             // Case 3d: Top-right flipped
            CrossEdgeCase.TopBackAligned => "U2 F2",                                   // Case 3e: Top-back aligned
            CrossEdgeCase.TopBackFlipped => "U R' F [R restores (1,0,1)]",            // Case 3f: Top-back flipped
            CrossEdgeCase.TopLeftAligned => "U' F2",                                   // Case 3g: Top-left aligned
            CrossEdgeCase.TopLeftFlipped => "L F' [L' restores (-1,0,1)]",            // Case 3h: Top-left flipped
            
            _ => throw new ArgumentException($"Unknown cross edge case: {caseType}")
        };
    }
    
    /// <summary>
    /// Processes conditional restoration notation [...]
    /// Tries both with and without restoration moves, picks the one that preserves more edges
    /// </summary>
    private static string ProcessConditionalRestorations(string rawAlgorithm, Cube? cube, CubeColor crossColor)
    {
        if (string.IsNullOrEmpty(rawAlgorithm))
            return rawAlgorithm;
            
        // If no cube provided, default to removing brackets (simple approach)
        if (cube == null)
            return ExtractWithRestoration(rawAlgorithm);
            
        // Check if algorithm has any conditional restorations
        if (!rawAlgorithm.Contains("["))
            return rawAlgorithm;
        
        // Extract two versions: with and without restoration moves
        var withRestoration = ExtractWithRestoration(rawAlgorithm);
        var withoutRestoration = ExtractWithoutRestoration(rawAlgorithm);
        
        // If they're the same, no choice to make
        if (withRestoration == withoutRestoration)
            return withRestoration;
        
        // Try both and see which preserves more cross edges
        var edgesBeforeSolve = CountSolvedCrossEdges(cube, crossColor);
        
        // Try with restoration
        var cubeWithRestoration = cube.Clone();
        var algo1 = Algorithm.Parse(withRestoration);
        if (algo1.IsSuccess)
        {
            foreach (var move in algo1.Value.Moves)
            {
                cubeWithRestoration.ApplyMove(move);
            }
        }
        var edgesWithRestoration = CountSolvedCrossEdges(cubeWithRestoration, crossColor);
        
        // Try without restoration
        var cubeWithoutRestoration = cube.Clone();
        var algo2 = Algorithm.Parse(withoutRestoration);
        if (algo2.IsSuccess)
        {
            foreach (var move in algo2.Value.Moves)
            {
                cubeWithoutRestoration.ApplyMove(move);
            }
        }
        var edgesWithoutRestoration = CountSolvedCrossEdges(cubeWithoutRestoration, crossColor);
        
        // Pick the better option
        if (edgesWithRestoration > edgesWithoutRestoration)
        {
            // DEBUG: Uncomment to see restoration decisions
            // Console.Error.WriteLine($"[DEBUG] Choosing WITH restoration: {edgesWithRestoration} edges vs {edgesWithoutRestoration}");
            return withRestoration;
        }
        else if (edgesWithoutRestoration > edgesWithRestoration)
        {
            // DEBUG: Uncomment to see restoration decisions
            // Console.Error.WriteLine($"[DEBUG] Choosing WITHOUT restoration: {edgesWithoutRestoration} edges vs {edgesWithRestoration}");
            return withoutRestoration;
        }
        else
        {
            // Tied - pick the shorter algorithm
            var movesWithRestoration = withRestoration.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var movesWithoutRestoration = withoutRestoration.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            
            // DEBUG: Uncomment to see restoration decisions
            // Console.Error.WriteLine($"[DEBUG] Tied at {edgesWithRestoration} edges - choosing shorter: {(movesWithoutRestoration <= movesWithRestoration ? "without" : "with")} restoration");
            
            return movesWithoutRestoration <= movesWithRestoration ? withoutRestoration : withRestoration;
        }
    }
    
    /// <summary>
    /// Extracts algorithm with restoration moves (removes brackets but keeps moves)
    /// </summary>
    private static string ExtractWithRestoration(string rawAlgorithm)
    {
        // Pattern: [move restores position] -> just move
        var processed = System.Text.RegularExpressions.Regex.Replace(
            rawAlgorithm, 
            @"\[([^]]+) restores [^]]+\]", 
            "$1");
        
        // Clean up extra spaces
        return System.Text.RegularExpressions.Regex.Replace(processed, @"\s+", " ").Trim();
    }
    
    /// <summary>
    /// Extracts algorithm without restoration moves (removes entire bracket contents)
    /// </summary>
    private static string ExtractWithoutRestoration(string rawAlgorithm)
    {
        // Pattern: [anything] -> remove entirely
        var processed = System.Text.RegularExpressions.Regex.Replace(
            rawAlgorithm, 
            @"\[[^]]+\]", 
            "");
        
        // Clean up extra spaces
        return System.Text.RegularExpressions.Regex.Replace(processed, @"\s+", " ").Trim();
    }
    
    /// <summary>
    /// Find a specific cross edge by its two colors
    /// </summary>
    private static CubePiece? FindCrossEdge(Cube cube, CubeColor crossColor, CubeColor edgeColor)
    {
        return cube.Edges.FirstOrDefault(edge =>
            edge.Colors.Contains(crossColor) && edge.Colors.Contains(edgeColor));
    }
    
    /// <summary>
    /// Counts how many cross edges are solved (in correct position with cross color on bottom)
    /// </summary>
    private static int CountSolvedCrossEdges(Cube cube, CubeColor crossColor)
    {
        var crossEdgeColors = CrossConstants.StandardEdgeColors;
        var solvedCount = 0;
        
        foreach (var edgeColor in crossEdgeColors)
        {
            var edge = FindCrossEdge(cube, crossColor, edgeColor);
            if (edge != null && IsEdgeSolved(edge, crossColor, edgeColor))
            {
                solvedCount++;
            }
        }
        
        return solvedCount;
    }
    
    /// <summary>
    /// Checks if a cross edge is in its solved position
    /// </summary>
    private static bool IsEdgeSolved(CubePiece edge, CubeColor crossColor, CubeColor edgeColor)
    {
        // Edge is solved if:
        // 1. It's on the bottom layer (Y = -1)
        // 2. Cross color (white) is facing down
        // 3. It's in the correct position relative to its center
        
        if (edge.Position.Y != -1)
            return false;
        
        // Check if it's in the correct position relative to center
        var expectedPosition = GetExpectedCrossEdgePosition(edgeColor);
        if (!edge.Position.Equals(expectedPosition))
            return false;
        
        // Check if cross color is facing down
        // For an edge on the bottom layer, the cross color should be in the Y slot (index 1)
        // and should match the expected color for that position
        if (edge.Colors[1] != crossColor)
            return false;
        
        // Also verify the edge color is facing its correct direction
        // Based on the position, we can determine which index should have the edge color
        var edgeColorIndex = edge.Position.Z != 0 ? 2 : 0; // Front/Back use Z index, Left/Right use X index
        if (edge.Colors[edgeColorIndex] != edgeColor)
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Gets the expected position for a cross edge based on its non-cross color
    /// </summary>
    private static Position3D GetExpectedCrossEdgePosition(CubeColor edgeColor)
    {
        return edgeColor switch
        {
            CubeColor.Green => new Position3D(0, -1, 1),   // Front edge
            CubeColor.Orange => new Position3D(1, -1, 0),  // Right edge
            CubeColor.Blue => new Position3D(0, -1, -1),   // Back edge
            CubeColor.Red => new Position3D(-1, -1, 0),    // Left edge
            _ => throw new ArgumentException($"Invalid edge color for cross: {edgeColor}")
        };
    }
    
    /// <summary>
    /// Transforms an algorithm string based on edge color perspective
    /// </summary>
    private static string TransformAlgorithmForEdgeColor(string algorithm, CubeColor edgeColor)
    {
        if (string.IsNullOrEmpty(algorithm) || edgeColor == CubeColor.Green)
        {
            return algorithm; // No transformation needed for canonical green
        }
        
        // Handle algorithms with conditional restoration brackets
        // Pattern: "move1 move2 [restoration_move restores position] move3"
        
        var result = new StringBuilder();
        var tokens = algorithm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        bool inBracket = false;
        
        foreach (var token in tokens)
        {
            if (token.StartsWith("["))
            {
                inBracket = true;
                if (result.Length > 0) result.Append(" ");
                
                // Handle bracket token that may contain a move
                if (token.Length > 1)
                {
                    // Extract move from [R or [R] format
                    var moveStart = 1; // Skip the [
                    var moveEnd = token.IndexOf(' ', moveStart);
                    if (moveEnd == -1) moveEnd = token.IndexOf(']', moveStart);
                    if (moveEnd == -1) moveEnd = token.Length;
                    
                    var moveContent = token.Substring(moveStart, moveEnd - moveStart);
                    if (IsMoveToken(moveContent))
                    {
                        // Transform the move and reconstruct
                        var transformedMove = TransformMove(moveContent, edgeColor);
                        result.Append("[").Append(transformedMove).Append(token.Substring(moveEnd));
                    }
                    else
                    {
                        result.Append(token);
                    }
                }
                else
                {
                    result.Append(token);
                }
                
                if (token.EndsWith("]"))
                {
                    inBracket = false;
                }
            }
            else if (inBracket)
            {
                // Inside brackets - transform moves but preserve other text
                if (IsMoveToken(token))
                {
                    result.Append(" ").Append(TransformMove(token, edgeColor));
                }
                else
                {
                    result.Append(" ").Append(token);
                }
                
                if (token.EndsWith("]"))
                {
                    inBracket = false;
                }
            }
            else
            {
                // Outside brackets - transform as normal move
                if (result.Length > 0) result.Append(" ");
                result.Append(TransformMove(token, edgeColor));
            }
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// Checks if a token looks like a move (starts with face letter)
    /// </summary>
    private static bool IsMoveToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        var firstChar = token[0];
        return "RLUDFBMESXYZ".Contains(char.ToUpper(firstChar));
    }
    
    /// <summary>
    /// Transforms a single move based on edge color perspective
    /// </summary>
    private static string TransformMove(string move, CubeColor edgeColor)
    {
        // Extract the base move (letter) and modifiers (', 2)
        var baseMoveChar = move[0];
        var modifiers = move.Length > 1 ? move.Substring(1) : "";
        
        var transformedBase = edgeColor switch
        {
            CubeColor.Orange => ApplyYRotationToMove(baseMoveChar),      // y: F→R→B→L→F
            CubeColor.Blue => ApplyY2RotationToMove(baseMoveChar),       // y2: F↔B, R↔L
            CubeColor.Red => ApplyYPrimeRotationToMove(baseMoveChar),    // y': F→L→B→R→F
            _ => baseMoveChar // Green or other (no transformation)
        };
        
        return transformedBase + modifiers;
    }
    
    /// <summary>
    /// Apply Y rotation transformation to a move character (F→R→B→L→F)
    /// </summary>
    private static char ApplyYRotationToMove(char moveChar)
    {
        return moveChar switch
        {
            'F' => 'R',
            'R' => 'B', 
            'B' => 'L',
            'L' => 'F',
            'U' => 'U', // U and D don't change with Y rotation
            'D' => 'D',
            'M' => 'S', // Middle slices also rotate
            'S' => 'M',
            'E' => 'E', // E slice doesn't change
            'x' => 'z', // Cube rotations also transform
            'z' => 'x',
            'y' => 'y', // Y rotation doesn't change
            _ => moveChar // Unknown moves pass through unchanged
        };
    }
    
    /// <summary>
    /// Apply Y2 rotation transformation to a move character (F↔B, R↔L)
    /// </summary>
    private static char ApplyY2RotationToMove(char moveChar)
    {
        return moveChar switch
        {
            'F' => 'B',
            'B' => 'F',
            'R' => 'L',
            'L' => 'R', 
            'U' => 'U', // U and D don't change
            'D' => 'D',
            'M' => 'M', // M slice becomes its opposite direction (handled by modifiers)
            'S' => 'S', // S slice becomes its opposite direction  
            'E' => 'E', // E slice doesn't change
            'x' => 'x', // x rotation becomes its opposite direction
            'y' => 'y', // y rotation becomes y2 (handled elsewhere)
            'z' => 'z', // z rotation becomes its opposite direction
            _ => moveChar // Unknown moves pass through unchanged
        };
    }
    
    /// <summary>
    /// Apply Y' rotation transformation to a move character (F→L→B→R→F)
    /// </summary>
    private static char ApplyYPrimeRotationToMove(char moveChar)
    {
        return moveChar switch
        {
            'F' => 'L',
            'L' => 'B',
            'B' => 'R', 
            'R' => 'F',
            'U' => 'U', // U and D don't change with Y rotation
            'D' => 'D',
            'M' => 'S', // Middle slices rotate opposite to Y
            'S' => 'M',
            'E' => 'E', // E slice doesn't change
            'x' => 'z', // Cube rotations transform opposite to Y
            'z' => 'x',
            'y' => 'y', // Y rotation doesn't change
            _ => moveChar // Unknown moves pass through unchanged
        };
    }
}