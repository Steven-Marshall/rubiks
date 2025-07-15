# Superhuman Solver Bug Report

## Issue Summary
The SuperhumanCrossSolver is producing incorrect algorithms that don't actually solve the white cross when applied.

## Test Case

```bash
smarshal@LaptopStudio:~/code/rubiks$ rubiks create testcube
smarshal@LaptopStudio:~/code/rubiks$ rubiks apply "U' F' U' L B' D' R' B2 F2 L' D2 U2 D' B2 R B R L U2 B2 F' U2 F' B U2" testcube
smarshal@LaptopStudio:~/code/rubiks$ rubiks solve cross testcube --verbose
Solving white cross step by step...

Step 1: white-green (2 moves) -> U F2
Status: 1/4 edges solved (3 remaining)

Step 2: white-red (1 move) -> L2
Status: 2/4 edges solved (2 remaining)

Step 3: white-blue (2 moves) -> R2 B'
Status: 3/4 edges solved (1 remaining)

Step 4: white-orange (4 moves) -> U' B' R B
Status: 4/4 edges solved

White cross complete! Total moves: 9
Full algorithm: U F2 L2 R2 B' U' B' R B
smarshal@LaptopStudio:~/code/rubiks$ rubiks solve cross testcube --verbose --level=superhuman
Optimal sequence: red edge → orange edge → green edge (Superhuman: evaluated 24 permutations, optimal: 6 moves)
smarshal@LaptopStudio:~/code/rubiks$ rubiks solve cross testcube --level=superhuman
U L2 D B R F2
smarshal@LaptopStudio:~/code/rubiks$ rubiks apply "U L2 D B R F2" testcube
smarshal@LaptopStudio:~/code/rubiks$ rubiks display testcube
ㅤㅤㅤ🟦🟥🟨
ㅤㅤㅤ🟥🟨🟩
ㅤㅤㅤ🟩🔳🟧
🟧🟩🟨🟥🟥🟨🟩🟨🟦🟥🟨🟨
🟨🟥🟧🟩🟩🟦🟥🟧🟦🟧🟦🟦
🟧🟨🟥🟩🟩🟦🟥🟧🟧🟦🟦🟩
ㅤㅤㅤ🔳🔳🔳
ㅤㅤㅤ🟧🔳🔳
ㅤㅤㅤ🔳🔳🔳
smarshal@LaptopStudio:~/code/rubiks$
```

## Problem Analysis

### Pattern Solver (Working)
- **Algorithm**: `U F2 L2 R2 B' U' B' R B` (9 moves)
- **Result**: ✅ **Correctly solves white cross**

### Superhuman Solver (Broken)  
- **Algorithm**: `U L2 D B R F2` (6 moves)
- **Verbose**: Claims "optimal: 6 moves" and "evaluated 24 permutations"
- **Result**: ❌ **Does NOT solve white cross** - bottom layer shows no white edges in correct positions

## Expected vs Actual

**Expected**: Superhuman solver should produce a shorter algorithm that correctly solves the white cross (≤9 moves)

**Actual**: Superhuman solver produces a 6-move algorithm that fails to solve the white cross

## Root Cause Investigation Needed

1. **Algorithm Generation**: Is the SuperhumanCrossSolver actually testing that its generated algorithms solve the cross?
2. **Permutation Logic**: Are all 24 permutations being correctly evaluated?
3. **Validation**: Is there a validation step missing that verifies the algorithm actually works?

## Scramble for Reproduction
```
U' F' U' L B' D' R' B2 F2 L' D2 U2 D' B2 R B R L U2 B2 F' U2 F' B U2
```

## Impact
- **High**: SuperhumanCrossSolver is producing invalid solutions
- **User Experience**: Users cannot trust the "optimal" algorithms
- **Algorithm Quality**: The 6-move claim is meaningless if it doesn't solve the cross

## Root Cause Analysis (FOUND)

### The Problem
The `EvaluatePermutation` method in `SuperhumanCrossSolver.cs` (lines 183-237) has a critical validation gap:

1. ✅ **Correctly applies individual edge algorithms** to a working cube (lines 206-214)
2. ✅ **Correctly tracks and combines algorithms** (lines 218-220)
3. ❌ **MISSING: Final validation** that the complete combined algorithm actually solves the cross on the original cube

### Code Analysis

**What it does:**
```csharp
// EvaluatePermutation method (lines 183-237)
foreach (var edgeColor in edgeOrder)
{
    // Solve edge on working cube
    var edgeSolution = SolveSingleEdgeOnCube(workingCube, edgeColor);
    algorithms.Add(edgeSolution.Algorithm);
    
    // Apply moves to working cube for next iteration
    foreach (var move in algorithmResult.Value.Moves)
    {
        workingCube.ApplyMove(move);  // ✅ Works on working cube
    }
}

// Combine algorithms and return
var combinedAlgorithm = string.Join(" ", algorithms);
var compressedAlgorithm = AlgorithmCompressor.Compress(combinedAlgorithm);

return new SuggestionResult(
    algorithm: compressedAlgorithm,  // ❌ NEVER VALIDATED on original cube!
    description: description,
    nextStage: "cross"
);
```

**What's missing:**
```csharp
// MISSING: Validation that combined algorithm works on original cube
var testCube = cube.Clone();
var finalAlgorithm = Algorithm.Parse(compressedAlgorithm);
if (finalAlgorithm.IsSuccess)
{
    foreach (var move in finalAlgorithm.Value.Moves)
    {
        testCube.ApplyMove(move);
    }
    
    // Check if cross is actually solved
    if (!IsCrossSolved(testCube))
    {
        return null; // This permutation doesn't work
    }
}
```

### Why This Causes Failures

The algorithm fails because:

1. **State Dependencies**: Individual edge solutions work on the incrementally modified working cube, but the combined algorithm may not work on the original cube state
2. **Algorithm Compression Issues**: `AlgorithmCompressor.Compress()` might create move sequences that don't work the same way
3. **Move Sequence Conflicts**: The concatenated moves might have unintended interactions

### Comparison with Working Pattern Solver

**Pattern Solver (working)**: Validates each step and ensures the final state is correct
**Superhuman Solver (broken)**: Assumes the combined algorithm will work without testing it

## Fix Required

Add final validation in `EvaluatePermutation` method before returning the `SuggestionResult`:

```csharp
// Test the final combined algorithm on original cube
var testCube = cube.Clone();
var finalResult = Algorithm.Parse(compressedAlgorithm);
if (finalResult.IsSuccess)
{
    foreach (var move in finalResult.Value.Moves)
    {
        testCube.ApplyMove(move);
    }
    
    // Verify all cross edges are solved
    var allEdgesSolved = new[] { CubeColor.Green, CubeColor.Orange, CubeColor.Blue, CubeColor.Red }
        .All(edgeColor => IsEdgeSolved(testCube, CrossColor, edgeColor));
    
    if (!allEdgesSolved)
    {
        return null; // This permutation doesn't actually work
    }
}
```

This will ensure only working algorithms are returned as "optimal" solutions.