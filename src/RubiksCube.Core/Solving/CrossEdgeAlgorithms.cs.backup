using RubiksCube.Core.Models;

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
    /// <param name="isFirstEdge">True if solving the first cross edge (enables optimizations)</param>
    /// <param name="edgeColor">Edge color for algorithm transformation</param>
    /// <returns>Algorithm string with conditionals resolved and transformed for edge perspective</returns>
    public static string GetAlgorithm(CrossEdgeCase caseType, Cube? cube = null, CubeColor crossColor = CubeColor.White, bool isFirstEdge = false, CubeColor edgeColor = CubeColor.Green)
    {
        var rawAlgorithm = GetRawAlgorithm(caseType, isFirstEdge);
        
        // Process conditional restoration brackets [...]
        var processedAlgorithm = ProcessConditionalRestorations(rawAlgorithm, cube, crossColor);
        
        // Transform algorithm based on edge color perspective
        return TransformAlgorithmForEdgeColor(processedAlgorithm, edgeColor);
    }
    
    /// <summary>
    /// Gets the raw algorithm with conditional notation intact
    /// </summary>
    private static string GetRawAlgorithm(CrossEdgeCase caseType, bool isFirstEdge)
    {
        return caseType switch
        {
            // Bottom Layer Cases (Y = -1)
            CrossEdgeCase.BottomFrontAligned => "",                                    // Case 1a: Front position, aligned
            CrossEdgeCase.BottomFrontFlipped => "D R [D' restores bottom layer] F",   // Case 1b: Front position, flipped
            
            // Case 1c: First edge optimization
            CrossEdgeCase.BottomRightAligned => isFirstEdge ? "D'" : "R2 U F2",       // Case 1c: Right position, aligned
            CrossEdgeCase.BottomRightFlipped => "R F",                                 // Case 1d: Right position, flipped
            
            // Case 1e: First edge optimization  
            CrossEdgeCase.BottomBackAligned => isFirstEdge ? "D2" : "B2 U2 F2",       // Case 1e: Back position, aligned
            CrossEdgeCase.BottomBackFlipped => "D' R [D restores bottom layer] F",    // Case 1f: Back position, flipped
            
            // Case 1g: First edge optimization
            CrossEdgeCase.BottomLeftAligned => isFirstEdge ? "D" : "L2 U' F2",        // Case 1g: Left position, aligned
            CrossEdgeCase.BottomLeftFlipped => "L' F'",                                // Case 1h: Left position, flipped
            
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
    /// For now: simple approach that includes all restoration moves
    /// TODO: Implement smart conditional checking
    /// </summary>
    private static string ProcessConditionalRestorations(string rawAlgorithm, Cube? cube, CubeColor crossColor)
    {
        if (string.IsNullOrEmpty(rawAlgorithm))
            return rawAlgorithm;
            
        // TODO: Implement proper conditional logic
        // For now: simple approach - strip brackets and include all moves
        var processed = rawAlgorithm;
        
        // Remove bracket notation but keep the moves
        // Pattern: [move restores position] -> just move
        processed = System.Text.RegularExpressions.Regex.Replace(
            processed, 
            @"\[([^]]+) restores [^]]+\]", 
            "$1");
        
        // Clean up extra spaces
        processed = System.Text.RegularExpressions.Regex.Replace(processed, @"\s+", " ").Trim();
        
        return processed;
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
        
        // Split algorithm into individual moves
        var moves = algorithm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var transformedMoves = new List<string>();
        
        foreach (var move in moves)
        {
            transformedMoves.Add(TransformMove(move, edgeColor));
        }
        
        return string.Join(" ", transformedMoves);
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
    
    /// <summary>
    /// TODO: Smart conditional restoration checking
    /// Checks if a position contains a solved cross edge that would be displaced
    /// </summary>
    private static bool NeedsRestoration(Cube cube, Position3D position, CubeColor crossColor)
    {
        // TODO: Implement proper checking logic
        // Check if there's a solved cross edge at this position
        // Return true if restoration move is needed
        
        throw new NotImplementedException("Smart conditional restoration not yet implemented");
    }
    
    /// <summary>
    /// TODO: Parse position references from restoration notation
    /// Extracts position coordinates from strings like "(1,-1,0)" or "bottom layer"
    /// </summary>
    private static Position3D ParsePositionReference(string positionRef)
    {
        // TODO: Parse position references like:
        // "(1,-1,0)" -> Position3D(1, -1, 0)
        // "bottom layer" -> analyze which position
        
        throw new NotImplementedException("Position reference parsing not yet implemented");
    }
}