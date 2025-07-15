# Post-Cross-Refactor Plan

## Overview

After implementing the cross-solving functionality, we've identified critical duplication and maintainability issues that need to be addressed. This document outlines a comprehensive refactoring plan to eliminate code duplication, improve maintainability, and prepare the codebase for future enhancements like color-neutral solving.

## Current State Analysis

### Critical Issues Identified

1. **Hardcoded Cross Edge Arrays** (7+ locations)
   - Risk: Any change to edge solve order requires updates in multiple files
   - Recent Impact: preserveBottomLayer bugs occurred in 2 places due to this pattern

2. **Algorithm Move Counting Inconsistency**
   - CrossSolver: Uses proper Algorithm.Parse() with error handling
   - SuperhumanCrossSolver: Uses string splitting (no error handling)
   - Risk: Different results for malformed algorithms

3. **Color Neutrality Hardcoding** 
   - CubeColor.White hardcoded in ~10 CLI locations
   - Risk: Color-neutral solving will require extensive changes

4. **Parallel Solver Implementations**
   - SolveCompleteCross() and SuperhumanCrossSolver.EvaluatePermutation() have nearly identical logic
   - Risk: Future enhancements need changes in multiple places

5. **preserveBottomLayer Logic Duplication** (3 locations)
   - Same calculation logic repeated in multiple methods
   - Risk: Logic changes require updates in multiple places

## Refactoring Plan

### Phase 1: Foundation & Quick Wins (Priority: High)

**Estimated Effort:** 2-3 hours
**Risk:** Low
**Impact:** High (prevents future bugs)

#### 1.1 Extract Cross Constants
**File:** `/src/RubiksCube.Core/Constants/CrossConstants.cs`
```csharp
public static class CrossConstants 
{
    /// <summary>
    /// Standard cross edge solve order used across all cross solvers
    /// </summary>
    public static readonly CubeColor[] StandardEdgeColors = 
        { CubeColor.Green, CubeColor.Orange, CubeColor.Blue, CubeColor.Red };
    
    /// <summary>
    /// Default cross color for non-color-neutral solving
    /// </summary>
    public static readonly CubeColor DefaultCrossColor = CubeColor.White;
}
```

**Affected Files:**
- CrossSolver.cs (5 locations)
- SuperhumanCrossSolver.cs (2 locations)
- CubeExtensions.cs (1 location)

#### 1.2 Standardize Algorithm Utilities
**File:** `/src/RubiksCube.Core/Utilities/AlgorithmUtilities.cs`
```csharp
public static class AlgorithmUtilities
{
    /// <summary>
    /// Counts moves in an algorithm string using proper parsing
    /// Replaces inconsistent string splitting approaches
    /// </summary>
    public static int CountMoves(string algorithm)
    {
        if (string.IsNullOrWhiteSpace(algorithm)) 
            return 0;
            
        var parsed = Algorithm.Parse(algorithm);
        return parsed.IsSuccess ? parsed.Value.Length : 0;
    }
    
    /// <summary>
    /// Safely applies an algorithm to a cube with error handling
    /// </summary>
    public static Result ApplyAlgorithm(Cube cube, string algorithm)
    {
        var parsed = Algorithm.Parse(algorithm);
        if (parsed.IsFailure)
            return Result.Failure($"Invalid algorithm: {parsed.Error}");
            
        foreach (var move in parsed.Value.Moves)
        {
            cube.ApplyMove(move);
        }
        
        return Result.Success();
    }
}
```

**Affected Files:**
- SuperhumanCrossSolver.cs (replace string splitting)
- CrossSolver.cs (replace duplicate parsing logic)
- CLI Program.cs (algorithm application logic)

#### 1.3 Centralize Cross Color Configuration
**File:** `/src/RubiksCube.CLI/Configuration/CrossConfiguration.cs`
```csharp
public static class CrossConfiguration
{
    /// <summary>
    /// Get cross color based on CLI arguments or default
    /// Prepares for future color-neutral implementation
    /// </summary>
    public static CubeColor GetCrossColor(string[] args)
    {
        // Future: Parse --cross-color argument
        // For now: return default
        return CrossConstants.DefaultCrossColor;
    }
    
    /// <summary>
    /// Create cross solver with configured color
    /// </summary>
    public static CrossSolver CreateCrossSolver(string[] args)
    {
        return new CrossSolver(GetCrossColor(args));
    }
    
    /// <summary>
    /// Create cross analyzer with configured color
    /// </summary>
    public static CrossAnalyzer CreateCrossAnalyzer(string[] args)
    {
        return new CrossAnalyzer(GetCrossColor(args));
    }
}
```

**Affected Files:**
- CLI Program.cs (10+ hardcoded CubeColor.White references)

### Phase 2: Cross Edge Service Layer (Priority: Medium)

**Estimated Effort:** 3-4 hours
**Risk:** Medium
**Impact:** High (eliminates duplication, enables testing)

#### 2.1 Create Cross Edge Analysis Service
**File:** `/src/RubiksCube.Core/Services/CrossEdgeService.cs`
```csharp
public class EdgeAnalysisResult
{
    public CubeColor EdgeColor { get; init; }
    public string Algorithm { get; init; } = "";
    public int MoveCount { get; init; }
    public bool IsSolved { get; init; }
    public string Description { get; init; } = "";
}

public static class CrossEdgeService
{
    /// <summary>
    /// Centralized logic for determining preserve parameter
    /// Eliminates duplication across 3+ methods
    /// </summary>
    public static bool ShouldPreserveBottomLayer(Cube cube, CubeColor crossColor)
    {
        return cube.CountSolvedCrossEdges(crossColor) > 0;
    }
    
    /// <summary>
    /// Analyze a single cross edge with consistent logic
    /// Replaces duplicate analysis patterns in multiple classes
    /// </summary>
    public static EdgeAnalysisResult AnalyzeEdge(Cube cube, CubeColor crossColor, 
        CubeColor edgeColor, bool? preserveOverride = null)
    {
        var preserve = preserveOverride ?? ShouldPreserveBottomLayer(cube, crossColor);
        var edge = cube.FindCrossEdge(crossColor, edgeColor);
        
        if (edge?.IsSolved == true)
        {
            return new EdgeAnalysisResult 
            {
                EdgeColor = edgeColor,
                Algorithm = "",
                MoveCount = 0,
                IsSolved = true,
                Description = $"{crossColor.GetColorName()}-{edgeColor.GetColorName()} edge already solved"
            };
        }
        
        var solution = cube.SolveSingleCrossEdge(crossColor, edgeColor, preserve);
        return new EdgeAnalysisResult 
        {
            EdgeColor = edgeColor,
            Algorithm = solution?.Algorithm ?? "",
            MoveCount = AlgorithmUtilities.CountMoves(solution?.Algorithm),
            IsSolved = false,
            Description = solution?.Description ?? ""
        };
    }
    
    /// <summary>
    /// Analyze all cross edges with consistent logic
    /// Replaces similar loops in multiple solver classes
    /// </summary>
    public static List<EdgeAnalysisResult> AnalyzeAllEdges(Cube cube, CubeColor crossColor)
    {
        return CrossConstants.StandardEdgeColors
            .Select(edgeColor => AnalyzeEdge(cube, crossColor, edgeColor))
            .ToList();
    }
    
    /// <summary>
    /// Find edge with shortest algorithm using consistent logic
    /// </summary>
    public static CubeColor? FindShortestEdge(Cube cube, CubeColor crossColor)
    {
        return AnalyzeAllEdges(cube, crossColor)
            .Where(e => !e.IsSolved)
            .OrderBy(e => e.MoveCount)
            .FirstOrDefault()?.EdgeColor;
    }
    
    /// <summary>
    /// Find next edge in fixed order (original analyze behavior)
    /// </summary>
    public static CubeColor? FindNextEdgeInOrder(Cube cube, CubeColor crossColor)
    {
        return CrossConstants.StandardEdgeColors
            .FirstOrDefault(edgeColor => !cube.IsEdgeSolved(crossColor, edgeColor));
    }
}
```

**Affected Files:**
- CrossSolver.cs (eliminate FindNextEdgeToSolve, FindShortestEdgeToSolve, preserveBottomLayer logic)
- SuperhumanCrossSolver.cs (eliminate duplicate edge analysis loop)
- CLI analyze command (simplify edge selection logic)

#### 2.2 Consolidate Solver Logic
**File:** `/src/RubiksCube.Core/Services/CrossSolvingService.cs`
```csharp
public class CrossSolutionResult
{
    public string Algorithm { get; init; } = "";
    public string VerboseOutput { get; init; } = "";
    public Cube FinalCube { get; init; }
    public int TotalMoves { get; init; }
    public List<string> StepAlgorithms { get; init; } = new();
}

public static class CrossSolvingService
{
    /// <summary>
    /// Unified cross solving logic used by both CrossSolver and SuperhumanCrossSolver
    /// Eliminates duplication between SolveCompleteCross and EvaluatePermutation
    /// </summary>
    public static CrossSolutionResult SolveWithOrder(Cube cube, CubeColor crossColor, 
        List<CubeColor> edgeOrder, bool verbose = false)
    {
        var workingCube = cube.Clone();
        var stepAlgorithms = new List<string>();
        var verboseLines = new List<string>();
        
        if (verbose)
        {
            verboseLines.Add("Solving cross with specified edge order...");
            verboseLines.Add($"Order: {string.Join(" → ", edgeOrder.Select(c => c.GetColorName()))}");
            verboseLines.Add("");
        }
        
        int stepNumber = 1;
        foreach (var edgeColor in edgeOrder)
        {
            var analysis = CrossEdgeService.AnalyzeEdge(workingCube, crossColor, edgeColor);
            
            if (analysis.IsSolved)
                continue;
                
            if (verbose)
            {
                verboseLines.Add($"Step {stepNumber}: {crossColor.GetColorName()}-{edgeColor.GetColorName()} ({analysis.MoveCount} move{(analysis.MoveCount == 1 ? "" : "s")}) -> {analysis.Algorithm}");
            }
            
            var applyResult = AlgorithmUtilities.ApplyAlgorithm(workingCube, analysis.Algorithm);
            if (applyResult.IsFailure)
                throw new InvalidOperationException($"Failed to apply algorithm: {applyResult.Error}");
                
            stepAlgorithms.Add(analysis.Algorithm);
            
            if (verbose)
            {
                var newSolvedCount = workingCube.CountSolvedCrossEdges(crossColor);
                var remainingCount = 4 - newSolvedCount;
                verboseLines.Add($"Status: {newSolvedCount}/4 edges solved{(remainingCount > 0 ? $" ({remainingCount} remaining)" : "")}");
                verboseLines.Add("");
            }
            
            stepNumber++;
        }
        
        var finalAlgorithm = stepAlgorithms.Any() 
            ? AlgorithmCompressor.Compress(string.Join(" ", stepAlgorithms))
            : "";
            
        return new CrossSolutionResult
        {
            Algorithm = finalAlgorithm,
            VerboseOutput = string.Join("\n", verboseLines),
            FinalCube = workingCube,
            TotalMoves = AlgorithmUtilities.CountMoves(finalAlgorithm),
            StepAlgorithms = stepAlgorithms
        };
    }
    
    /// <summary>
    /// Solve using shortest-first order (current solve command behavior)
    /// </summary>
    public static CrossSolutionResult SolveOptimal(Cube cube, CubeColor crossColor, bool verbose = false)
    {
        var optimalOrder = GenerateOptimalOrder(cube, crossColor);
        return SolveWithOrder(cube, crossColor, optimalOrder, verbose);
    }
    
    /// <summary>
    /// Generate optimal solve order by repeatedly finding shortest edge
    /// </summary>
    private static List<CubeColor> GenerateOptimalOrder(Cube cube, CubeColor crossColor)
    {
        var workingCube = cube.Clone();
        var order = new List<CubeColor>();
        
        while (order.Count < 4)
        {
            var shortestEdge = CrossEdgeService.FindShortestEdge(workingCube, crossColor);
            if (shortestEdge == null)
                break;
                
            order.Add(shortestEdge.Value);
            
            // Apply this edge's solution to working cube for next iteration
            var analysis = CrossEdgeService.AnalyzeEdge(workingCube, crossColor, shortestEdge.Value);
            AlgorithmUtilities.ApplyAlgorithm(workingCube, analysis.Algorithm);
        }
        
        return order;
    }
}
```

### Phase 3: CLI Refactoring (Priority: Medium)

**Estimated Effort:** 2-3 hours
**Risk:** Low
**Impact:** Medium (improves maintainability)

#### 3.1 Abstract CLI Argument Parsing
**File:** `/src/RubiksCube.CLI/Utilities/ArgumentParser.cs`
```csharp
public class ParsedArguments
{
    public string? CubeName { get; set; }
    public bool Verbose { get; set; }
    public bool Json { get; set; }
    public Dictionary<string, string> CustomOptions { get; set; } = new();
}

public static class ArgumentParser
{
    public static ParsedArguments ParseCommonArguments(string[] args, int startIndex = 1)
    {
        var result = new ParsedArguments();
        
        for (int i = startIndex; i < args.Length; i++)
        {
            if (args[i] == "--verbose" || args[i] == "-v")
                result.Verbose = true;
            else if (args[i] == "--json" || args[i] == "-j")
                result.Json = true;
            else if (!args[i].StartsWith("-"))
                result.CubeName = args[i];
            // Additional common parsing...
        }
        
        return result;
    }
}
```

#### 3.2 Centralize JSON Output Generation
**File:** `/src/RubiksCube.CLI/Utilities/JsonOutputGenerator.cs`
```csharp
public static class JsonOutputGenerator
{
    public static string GenerateAnalysisJson(AnalysisResult result)
    {
        var jsonOutput = new
        {
            recognition = new
            {
                stage = result.Recognition.Stage,
                complete = result.Recognition.IsComplete,
                progress = result.Recognition.Progress,
                total = result.Recognition.Total,
                description = result.Recognition.Description,
                details = result.Recognition.Details
            },
            suggestion = result.Suggestion != null ? new
            {
                algorithm = result.Suggestion.Algorithm,
                description = result.Suggestion.Description,
                next_stage = result.Suggestion.NextStage,
                details = result.Suggestion.Details
            } : null
        };
        
        return JsonSerializer.Serialize(jsonOutput, new JsonSerializerOptions { WriteIndented = true });
    }
}
```

### Phase 4: Architecture Improvements (Priority: Low)

**Estimated Effort:** 4-6 hours
**Risk:** High
**Impact:** High (future-proofing)

#### 4.1 Cross Solver Unification
Consider creating a unified cross analysis engine that both CrossSolver and SuperhumanCrossSolver can use:

**File:** `/src/RubiksCube.Core/Engines/CrossAnalysisEngine.cs`
```csharp
public interface ICrossAnalysisEngine
{
    /// <summary>
    /// Analyze current cross state and suggest next move
    /// </summary>
    SuggestionResult SuggestNextMove(Cube cube, EdgeSelectionMode mode, CubeColor? specificEdge = null);
    
    /// <summary>
    /// Generate complete cross solution using specified strategy
    /// </summary>
    CrossSolutionResult SolveComplete(Cube cube, SolvingStrategy strategy, bool verbose = false);
    
    /// <summary>
    /// Find optimal cross solution by evaluating all permutations
    /// </summary>
    CrossSolutionResult FindOptimalSolution(Cube cube, bool verbose = false);
}

public enum SolvingStrategy
{
    FixedOrder,     // Green → Orange → Blue → Red
    ShortestFirst,  // Always pick shortest available
    Optimal         // Evaluate all permutations (superhuman)
}
```

## Implementation Strategy

### Rollout Plan

1. **Phase 1 (Session 1)**: Extract constants and utilities
   - Low risk, immediate benefit
   - Prevents future duplication bugs
   - Prepares foundation for subsequent phases

2. **Phase 2 (Session 2)**: Implement service layer
   - Replace duplicated analysis logic
   - Consolidate solver implementations
   - Comprehensive testing required

3. **Phase 3 (Session 3)**: CLI improvements
   - Lower priority, can be done incrementally
   - Improves maintainability

4. **Phase 4 (Future)**: Architecture unification
   - Only if more cross solvers are planned
   - Requires careful design consideration

### Testing Strategy

Each phase should include:
- Unit tests for new utility functions
- Integration tests to ensure equivalent behavior
- Regression tests for existing CLI commands
- Performance tests for refactored algorithms

### Validation Criteria

**Phase 1 Success:**
- All existing tests pass
- No hardcoded color arrays remain
- Consistent move counting across all solvers

**Phase 2 Success:** 
- CrossSolver and SuperhumanCrossSolver produce identical results
- No duplicate edge analysis logic remains
- Service layer has comprehensive test coverage

**Phase 3 Success:**
- CLI commands maintain identical user experience
- Argument parsing logic is centralized
- JSON output is consistent across commands

## Risk Mitigation

### High-Risk Areas
1. **Algorithm behavior changes**: Ensure refactored logic produces identical results
2. **CLI compatibility**: Maintain exact user experience
3. **Performance impact**: New abstractions shouldn't slow down solving

### Mitigation Strategies
1. **Extensive testing**: Compare before/after results on large test suites
2. **Incremental rollout**: Complete one phase before starting next
3. **Rollback plan**: Keep backup of original implementations until validation complete

## Benefits

### Immediate Benefits (Phase 1)
- Eliminates future preserveBottomLayer-style bugs
- Consistent algorithm handling across all components
- Prepares for color-neutral solving

### Long-term Benefits (All Phases)
- Single source of truth for cross solving logic
- Easier testing and validation
- Simplified addition of new solving strategies
- Better separation of concerns between CLI and Core
- Foundation for advanced features (color neutrality, new algorithms)

## Success Metrics

- **Duplication Elimination**: Zero hardcoded cross edge arrays
- **Consistency**: Identical results from CrossSolver and SuperhumanCrossSolver
- **Maintainability**: New cross features require changes in only one place
- **Test Coverage**: >95% coverage on new service layer
- **Performance**: No regression in solving speed
- **User Experience**: Zero changes to CLI behavior

## Future Considerations

This refactoring plan prepares the codebase for:
- **Color-Neutral Solving**: Easy addition of cross color selection
- **New Solving Methods**: Additional algorithms can use the same service layer  
- **Advanced Analysis**: Richer edge analysis with position tracking
- **Performance Optimization**: Centralized logic is easier to optimize
- **API Development**: Service layer can be exposed as REST API

The refactoring eliminates technical debt while building a foundation for future enhancements.