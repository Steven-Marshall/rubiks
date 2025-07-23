# Comprehensive Cube Format Expectations Audit

**Date**: July 22, 2025  
**Purpose**: Document what cube format each method expects in analyze and solving workflows  
**Status**: Post-reset analysis of commit `ecc0b73`

## **FUNDAMENTAL BUILDING BLOCKS** 

These are the lowest-level properties and methods that everything else depends on:

### **Core IsSolved Properties:**
- `CubePiece.IsSolved` (CubePiece.cs:143) - **CANONICAL (white-bottom + green-front)** - Most fundamental building block
- `Cube.IsSolved` (Cube.cs:145) - **CANONICAL** - Uses `_pieces.Values.All(p => p.IsSolved)` - inherits canonical requirement
- `EdgeAnalysisResult.IsSolved` (CrossEdgeService.cs:18) - **[DATA STRUCTURE]** - Property in result object

### **Core Orientation Methods:**
- `CubePiece.IsCorrectOrientation` (CubePiece.cs:148) - **CANONICAL** - Used by `CubePiece.IsSolved`
- `CubePiece.GetExpectedColorsForPosition()` (CubePiece.cs:162) - **CANONICAL** - Hardcoded Western/BOY color scheme

### **Dependency Chain Analysis:**
```
CLI Entry → Analyzers → Cube.IsSolved → All CubePiece.IsSolved → CubePiece.IsCorrectOrientation → GetExpectedColorsForPosition
```

**CONFIRMED CASCADE EFFECT**: `CubePiece.IsSolved` REQUIRES CANONICAL orientation. Analysis vs Solving distinction:

**ANALYSIS CHAIN (Requires CANONICAL):**
- `CubePiece.IsSolved` → **CANONICAL** (hardcoded Western/BOY color scheme)
- `Cube.IsSolved` → **CANONICAL** (uses `_pieces.Values.All(p => p.IsSolved)`)  
- `SolvedAnalyzer.Analyze()` → **CANONICAL** (uses `cube.IsSolved`)
- `CrossAnalyzer.Analyze()` → **CANONICAL** (uses `IsEdgeCorrectlyPlaced()` → `edge.IsSolved`)

**SOLVING CHAIN (Requires WHITE-BOTTOM only):**
- `CrossEdgeClassifier.ClassifyEdgePosition()` → **WHITE-BOTTOM** (perspective-based, transforms view not cube)
- `CrossSolver.SuggestAlgorithm()` → **WHITE-BOTTOM** (uses perspective-based classification)
- `CrossEdgeAlgorithms.GetAlgorithm()` → **WHITE-BOTTOM** (assumes white bottom, any front face orientation)

**KEY INSIGHT**: Analysis and solving have different format requirements!

---

## **CATEGORY 1: Any Cube Format (Format-Agnostic)**

These methods work with cubes in any orientation and don't assume specific color positions:

### CLI Entry Points:
- `Program.HandleAnalyze()` - Accepts any cube format from stdin/storage
- `Program.HandleSolve()` - Accepts any cube format from stdin/storage  
- `Program.HandleDisplay()` - Works with any cube format
- `Program.HandleApply()` - Applies moves to any cube format

### Core Cube Operations:
- `Cube.ApplyMove()` - Applies moves regardless of orientation
- `Cube.Clone()` - Copies any cube state
- `Cube.ToJson()`/`FromJson()` - Serializes any cube state

### Storage & Utilities:
- `CubeStorageService.*` - All storage operations work with any format
- `AlgorithmUtilities.ApplyAlgorithm()` - Applies algorithms to any cube
- `AlgorithmUtilities.CountMoves()` - Counts moves in algorithm strings
- `AlgorithmCompressor.Compress()` - Compresses any algorithm

## **CATEGORY 2: White-Bottom Normalized (Partial Normalization)**

These methods require white to be on the bottom but don't care about front face orientation:

### Cross Analysis Methods:
- `CrossEdgeClassifier.ClassifyEdgePosition()` - **WHITE-BOTTOM** - Perspective-based classification, white must be on bottom
- `CrossEdgeClassifier.IsEdgeCorrectlyOriented()` - Checks Y-axis for bottom layer (line 249)
- `CubeExtensions.CountSolvedCrossEdges()` - Uses cross edge classification
- `CubeExtensions.IsEdgeSolved()` - Uses classification that requires white bottom
- `CrossEdgeService.*` - All methods use edge classification requiring white bottom

### Solving Methods:
- `CrossEdgeAlgorithms.GetAlgorithm()` - Algorithms assume white bottom orientation
- `CrossEdgeAlgorithms.ProcessConditionalRestorations()` - Tests on white-bottom cubes
- `CrossSolver.SuggestAlgorithm()` - Uses white-bottom classification
- `SuperhumanCrossSolver.SuggestAlgorithm()` - Uses white-bottom methods
- `CrossSolvingService.*` - All methods use white-bottom classification

## **CATEGORY 3: Canonical (White-Bottom + Green-Front)**

These methods require full canonical orientation with white bottom AND green front:

### Cross Analysis (Confirmed):
- `CrossAnalyzer.Analyze()` - **CANONICAL** - Uses `IsEdgeCorrectlyPlaced()` → `edge.IsSolved` (line 93)
- `CrossAnalyzer.IsEdgeCorrectlyPlaced()` - **CANONICAL** - Directly uses `edge.IsSolved`

### Cross Solving (Corrected):
- `CrossEdgeClassifier.ClassifyEdgePosition()` - **WHITE-BOTTOM** - Transforms perspective, not cube orientation requirement
- `CrossSolver.SuggestAlgorithm()` - **WHITE-BOTTOM** - Uses perspective-based classification

### Normalization Analysis:
- `CubeNormalizer.GetCrossColorAnalysis()` - **LINE 114 COMMENT: "REQUIRES CANONICAL CUBE: white bottom, green front"**
- `CubeNormalizer.IsEdgeCorrectlyPlaced()` - Uses canonical position assumptions
- `CubeNormalizer.DetectBottomColor()` - Expects canonical center positions
- `CubeNormalizer.DetectFrontColor()` - Expects canonical center positions

## **CATEGORY 4: Unknown/Needs Investigation**

Methods that don't clearly specify format requirements but may have implicit assumptions:

### Pattern Recognition:
- `CubeStateAnalyzer.Analyze()` - **[NEEDS INVESTIGATION]** - Orchestrates analyzers with mixed requirements

---

## **KEY FINDINGS:**

### **Critical Architecture Issue:**
The codebase has **mixed format expectations throughout the call stack**:

1. **CLI accepts any format** → passes to analyzers
2. **Analyzers require white-bottom normalization** → but don't normalize input
3. **Some utilities require full canonical orientation** → without explicit normalization

### **Specific Problem Areas:**

1. **Cross Edge Classification Chain:**
   ```
   HandleAnalyze() [any format] 
   → CrossAnalyzer.Analyze() [requires white-bottom]
   → CrossEdgeClassifier.ClassifyEdgePosition() [requires white-bottom]
   ```

2. **Solving Chain:**
   ```
   HandleSolve() [any format]
   → CrossSolver.SuggestAlgorithm() [requires white-bottom]  
   → CrossEdgeAlgorithms.GetAlgorithm() [requires white-bottom]
   ```

3. **Hidden Dependencies:**
   - `CrossEdgeClassifier.IsEdgeCorrectlyOriented()` explicitly checks Y-axis assuming white bottom
   - All cross edge methods use this classification without normalization

### **Normalization Gap:**
- `CubeNormalizer` exists but is **never used in the main analyze/solve workflows**
- Methods require normalized input but don't normalize it themselves
- No automatic normalization at CLI entry points

---

## **ROOT CAUSE ANALYSIS**

### **The Core Problem:**
The current `x B'` bug demonstrates the analysis vs solving architecture distinction:

1. User runs: `create | apply "x B'" | analyze`
2. CLI passes rotated cube (white still on bottom, but faces rotated) to `CrossAnalyzer`
3. `CrossAnalyzer.Analyze()` uses **CANONICAL-dependent** `edge.IsSolved` chain
4. `GetExpectedColorsForPosition()` has hardcoded canonical expectations
5. After `x B'`, front face changed so `IsCorrectOrientation` returns false for all edges
6. **Result**: Analysis shows 0/4 cross complete (wrong)

**But if user ran**: `create | apply "x B'" | solve cross`
1. `CrossSolver` would use **WHITE-BOTTOM-only** perspective-based classification  
2. Would correctly detect cross is complete and return no moves
3. **Result**: Solver would work correctly

**The bug is analysis-specific, not solving-specific.**

### **The Expectation Mismatch:**
- **Expected**: Cross analysis should recognize cross completion in any orientation  
- **Reality**: Cross analysis requires canonical orientation, solving only needs white-bottom
- **User Experience**: Analysis shows wrong results, but solving would work correctly

---

## **RECOMMENDED SOLUTION APPROACH**

### **CRITICAL: Entry Point Only Changes + Low-Level Validation**

**🚨 IMPLEMENTATION RULE: DO NOT MODIFY LOW-LEVEL CODE LOGIC**
- ✅ Add validation checks to low-level methods (throw errors if wrong format)
- ❌ Do NOT change solving/analysis logic in low-level methods  
- ❌ Do NOT add normalization inside solvers/analyzers
- **Reason**: Previous attempts at this caused massive complexity and bugs

---

### **Differentiated Normalization by Command Type**

**For Analysis Commands** (`analyze`):
```
CLI Entry → Normalize to Canonical → Analyze → Return Results  
```
- Use `CubeNormalizer.NormalizeToCanonical()` 
- Required because analysis chain uses `edge.IsSolved` → canonical expectations

**For Solving Commands** (`solve cross`) - DETAILED WORKFLOW:

**Current Broken Workflow:**
1. User: `rubiks solve cross` (cube has yellow on bottom)
2. `HandleSolve()` loads cube (any orientation) 
3. Calls `CrossSolver.SuggestAlgorithm(cube)` directly
4. ❌ **FAILS**: Solver expects white-bottom, gets yellow-bottom

**Fixed Workflow:**
1. User: `rubiks solve cross` (cube has yellow on bottom)
2. `HandleSolve()` detects white is not on bottom
3. **Normalize**: `CubeNormalizer.NormalizeToWhiteBottom()`
   - Returns: `{ NormalizedCube: <white-bottom-cube>, Algorithm: "x2", ReverseAlgorithm: "x2" }`
4. **Solve**: `CrossSolver.SuggestAlgorithm(normalizedCube)`
   - Returns: `"F R U' R' F'"` (for normalized cube)
5. **Prepend**: Combine `"x2" + "F R U' R' F'"` = `"x2 F R U' R' F'"`
6. **Return**: User gets algorithm that works on their original cube

**Benefits:**
- ✅ Works with any input cube orientation
- ✅ Solver gets expected format (white-bottom)
- ✅ User gets correct algorithm for their cube
- ✅ No changes to solving logic
- ✅ Uses existing proven normalization code

### **Option 2: Handle All Orientations Throughout**
```
CLI Entry → Pass Raw Cube → Each Method Handles Orientation
```

**Drawbacks:**
- Requires massive refactoring of classification system
- High risk of introducing new bugs
- Duplicate logic across many methods

### **Option 3: Hybrid Approach**
```
CLI Entry → Detect Need → Conditionally Normalize → Analyze
```

**Drawbacks:**
- Complex detection logic
- Still requires extensive testing across orientations

---

## **IMPLEMENTATION PLAN**

### **Phase 1: Add Low-Level Validation (Safety Net) ✅ COMPLETED**
1. ✅ Add format validation to `CubePiece.IsSolved` - throw if not canonical
2. ✅ Add format validation to `CrossAnalyzer.Analyze()` - throw if not canonical  
3. ✅ Add format validation to `CrossSolver.SuggestAlgorithm()` - throw if not white-bottom
4. ✅ **Goal**: Clear error messages when methods get wrong format

### **Phase 2: Build Normalization Infrastructure ✅ COMPLETED**
1. ✅ Verify existing `CubeNormalizer.NormalizeToWhiteBottom()` works correctly
2. ✅ Verify existing `CubeNormalizer.NormalizeToCanonical()` works correctly  
3. ✅ Test normalization functions with various cube orientations
4. ✅ Ensure `NormalizationResult` includes Algorithm for prepending

### **Phase 3: Update CLI Entry Points 🚧 IN PROGRESS**

#### **HandleSolve() Analysis Complete:**
- **Two solver paths identified**:
  - `--level=superhuman`: Lines 731-772, calls `SuperhumanCrossSolver.SuggestAlgorithm()` directly
  - `--level=pattern` (default): Lines 773-781, calls `CrossSolver.SolveCompleteCross()` directly  
- **Both paths bypass validation**: Load cubes from stdin/named files, pass directly to solvers
- **Validation gaps confirmed**: Neither path uses normalization or validation

#### **Required CLI Updates:**
1. **HandleAnalyze()**: Add `CubeNormalizer.NormalizeToCanonical()` before analysis
2. **HandleSolve()**: Add `CubeNormalizer.NormalizeToWhiteBottom()` + prepend workflow for both solver paths

#### **HandleSolve() Prepend Implementation:**
```csharp
// 1. Load cube (any format)
// 2. Normalize: var result = CubeNormalizer.NormalizeToWhiteBottom(cube);  
// 3. Solve on normalized cube
// 4. Prepend normalization moves: result.NormalizationMoves + " " + solutionAlgorithm
// 5. Return complete algorithm
```

### **Phase 4: Integration Testing**
1. Test various cube orientations: x, x', x2, y, y', y2, z, z', z2, combinations
2. Verify analysis shows correct results for all orientations  
3. Verify solving produces correct algorithms for all orientations
4. Add regression tests for the `x B'` bug specifically

**This approach uses existing, proven normalization code while maintaining the principle of "entry point transforms + low-level validation" to prevent future regressions.**