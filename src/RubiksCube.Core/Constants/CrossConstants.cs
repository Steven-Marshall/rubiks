using RubiksCube.Core.Models;

namespace RubiksCube.Core.Constants;

/// <summary>
/// Centralized constants for cross-solving operations
/// Eliminates hardcoded cross edge arrays and cross color references across the codebase
/// </summary>
public static class CrossConstants
{
    /// <summary>
    /// Standard cross edge solve order used across all cross solvers
    /// Order: Green → Orange → Blue → Red (matches current behavior)
    /// </summary>
    public static readonly CubeColor[] StandardEdgeColors = 
        { CubeColor.Green, CubeColor.Orange, CubeColor.Blue, CubeColor.Red };
    
    /// <summary>
    /// Default cross color for non-color-neutral solving
    /// Currently white, but centralized for future color-neutral implementation
    /// </summary>
    public static readonly CubeColor DefaultCrossColor = CubeColor.White;
    
    /// <summary>
    /// All possible cross colors for color-neutral solving (future use)
    /// </summary>
    public static readonly CubeColor[] AllCrossColors = 
        { CubeColor.White, CubeColor.Yellow, CubeColor.Green, CubeColor.Blue, CubeColor.Red, CubeColor.Orange };
}