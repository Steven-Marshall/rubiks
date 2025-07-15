using RubiksCube.Core.Models;
using RubiksCube.Core.Constants;
using RubiksCube.Core.Solving;
using RubiksCube.Core.PatternRecognition;

namespace RubiksCube.CLI.Configuration;

/// <summary>
/// Centralized cross configuration for CLI commands
/// Eliminates hardcoded CubeColor.White references and prepares for color-neutral solving
/// </summary>
public static class CrossConfiguration
{
    /// <summary>
    /// Get cross color based on CLI arguments or default
    /// Prepares for future color-neutral implementation with --cross-color argument
    /// </summary>
    /// <param name="args">CLI arguments to parse</param>
    /// <returns>Cross color to use for solving</returns>
    public static CubeColor GetCrossColor(string[] args)
    {
        // Future: Parse --cross-color argument for color-neutral solving
        // For now: return default white
        return CrossConstants.DefaultCrossColor;
    }
    
    /// <summary>
    /// Create cross solver with configured color
    /// Centralizes solver creation to eliminate hardcoded color references
    /// </summary>
    /// <param name="args">CLI arguments for configuration</param>
    /// <returns>CrossSolver configured with appropriate cross color</returns>
    public static CrossSolver CreateCrossSolver(string[] args)
    {
        return new CrossSolver(GetCrossColor(args));
    }
    
    /// <summary>
    /// Create cross analyzer with configured color
    /// Centralizes analyzer creation to eliminate hardcoded color references
    /// </summary>
    /// <param name="args">CLI arguments for configuration</param>
    /// <returns>CrossAnalyzer configured with appropriate cross color</returns>
    public static CrossAnalyzer CreateCrossAnalyzer(string[] args)
    {
        return new CrossAnalyzer(GetCrossColor(args));
    }
    
    /// <summary>
    /// Create superhuman cross solver with configured color
    /// Centralizes superhuman solver creation
    /// </summary>
    /// <param name="args">CLI arguments for configuration</param>
    /// <returns>SuperhumanCrossSolver configured with appropriate cross color</returns>
    public static SuperhumanCrossSolver CreateSuperhumanCrossSolver(string[] args)
    {
        return new SuperhumanCrossSolver(GetCrossColor(args));
    }
    
    /// <summary>
    /// Check if color-neutral solving is enabled in CLI arguments
    /// Future enhancement for --color-neutral or --cn flags
    /// </summary>
    /// <param name="args">CLI arguments to check</param>
    /// <returns>True if color-neutral solving is requested</returns>
    public static bool IsColorNeutralEnabled(string[] args)
    {
        // Future: Check for --color-neutral or --cn flags
        return false;
    }
}