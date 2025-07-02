# Cross Algorithm Systematic Approach

## Overview
Replace the current 6-case ad-hoc system with a systematic position-based approach that uses cube symmetry to solve any white cross edge.

## Design Principles
1. **No D moves** during cross solving (except potentially as first move if edge already in correct position)
2. **Preserve solved edges** - all algorithms must not disturb already-placed cross edges
3. **Simple cases get simple solutions** - single move scrambles should have single move solutions
4. **Use symmetry** - define algorithms for one edge (White-Green) and transform for others
5. **Restoration principle** - when algorithms temporarily displace pieces, include restoration moves unless the displaced piece was incorrect anyway

## Canonical Edge: White-Green
- Target position: Bottom-front (0, -1, 1)
- Target orientation: White facing down, Green facing front

## Case Classification System

### Case 1: Bottom Layer (Y = -1)

#### Case 1a: Bottom-front position, correct orientation
- **Position**: (0, -1, 1) [bottom-front edge]
- **State**: White down, green front (correct orientation)
- **Algorithm**: **SOLVED** ✓

#### Case 1b: Bottom-front position, flipped orientation
- **Position**: (0, -1, 1) [bottom-front edge]
- **State**: White down, green back (flipped orientation)
- **Algorithm**: `D R [D' restores bottom layer] F`
- **Explanation**: D moves edge to bottom-right, R brings to front-right, D' restores bottom layer, F inserts

#### Case 1c: Bottom-right position, correct orientation
- **Position**: (1, -1, 0) [bottom-right edge]
- **State**: White down, green right (correct orientation)
- **Algorithm**: `R2 U F2`
- **Explanation**: R2 lifts to top-right, U aligns above target, F2 inserts

#### Case 1d: Bottom-right position, flipped orientation
- **Position**: (1, -1, 0) [bottom-right edge]
- **State**: White facing right (flipped orientation)
- **Algorithm**: `R F`
- **Explanation**: R moves to front-right, F rotates down to target

#### Case 1e: Bottom-back position, correct orientation
- **Position**: (0, -1, -1) [bottom-back edge]
- **State**: White down, green back (correct orientation)
- **Algorithm**: `B2 U2 F2`
- **Explanation**: B2 lifts to top-back, U2 aligns above target, F2 inserts

#### Case 1f: Bottom-back position, flipped orientation
- **Position**: (0, -1, -1) [bottom-back edge]
- **State**: White facing back (flipped orientation)
- **Algorithm**: `D' R [D restores bottom layer] F`
- **Explanation**: D' moves edge to bottom-right, R brings to front-right, D restores bottom layer, F inserts

#### Case 1g: Bottom-left position, correct orientation
- **Position**: (-1, -1, 0) [bottom-left edge]
- **State**: White down, green left (correct orientation)
- **Algorithm**: `L2 U' F2`
- **Explanation**: L2 lifts to top-left, U' aligns above target, F2 inserts

#### Case 1h: Bottom-left position, flipped orientation
- **Position**: (-1, -1, 0) [bottom-left edge]
- **State**: White facing left (flipped orientation)
- **Algorithm**: `L' F'`
- **Explanation**: L' moves to front-left, F' rotates down to target

### Case 2: Middle Layer (Y = 0)

#### Case 2a: Front-right position, correct orientation
- **Position**: (1, 0, 1) [front-right edge]
- **State**: White facing front (correct orientation)
- **Algorithm**: `F`
- **Explanation**: Single front face rotation places edge correctly

#### Case 2b: Front-right position, flipped orientation
- **Position**: (1, 0, 1) [front-right edge]
- **State**: White facing right (flipped orientation)
- **Algorithm**: `R U [R' restores (1,-1,0)] F2`
- **Explanation**: R moves edge to bottom-right (displacing piece there), U moves to top-front, conditional R' restores displaced piece, F2 inserts

#### Case 2c: Right-back position, correct orientation
- **Position**: (1, 0, -1) [right-back edge]
- **State**: White facing right (correct orientation)
- **Algorithm**: `R2 F [R2 restores (1,0,1)]`
- **Explanation**: R2 moves edge to front-right (displacing piece there), F places edge, conditional R2 restores displaced piece

#### Case 2d: Right-back position, flipped orientation
- **Position**: (1, 0, -1) [right-back edge]
- **State**: White facing back (flipped orientation)
- **Algorithm**: `R' U [R restores (1,1,0)] F2`
- **Explanation**: R' moves edge to right-top (displacing piece there), U moves to top-front, conditional R restores displaced piece, F2 inserts

#### Case 2e: Back-left position, correct orientation
- **Position**: (-1, 0, -1) [back-left edge]
- **State**: White facing back (correct orientation)
- **Algorithm**: `L2 F' [L2 restores (-1,0,1)]`
- **Explanation**: L2 moves edge to front-left (displacing piece there), F' places edge, conditional L2 restores displaced piece

#### Case 2f: Back-left position, flipped orientation
- **Position**: (-1, 0, -1) [back-left edge]
- **State**: White facing left (flipped orientation)
- **Algorithm**: `L U' [L' restores (-1,1,0)] F2`
- **Explanation**: L moves edge to left-top (displacing piece there), U' moves to top-front, conditional L' restores displaced piece, F2 inserts

#### Case 2g: Left-front position, correct orientation
- **Position**: (-1, 0, 1) [left-front edge]
- **State**: White facing left (correct orientation)
- **Algorithm**: `F'`
- **Explanation**: Single counter-clockwise front face rotation places edge correctly

#### Case 2h: Left-front position, flipped orientation
- **Position**: (-1, 0, 1) [left-front edge]
- **State**: White facing front (flipped orientation)
- **Algorithm**: `L' U' [L restores (-1,1,0)] F2`
- **Explanation**: L' moves edge to left-top (displacing piece there), U' moves to top-front, conditional L restores displaced piece, F2 inserts

### Case 3: Top Layer (Y = 1)

#### Case 3a: Top-front position, correct orientation
- **Position**: (0, 1, 1) [top-front edge]
- **State**: White facing up (correct orientation)
- **Algorithm**: `F2`
- **Explanation**: Simple double move brings edge straight down

#### Case 3b: Top-front position, flipped orientation
- **Position**: (0, 1, 1) [top-front edge]
- **State**: White facing front (flipped orientation)
- **Algorithm**: `U' R' F [R restores (1,0,1)]`
- **Explanation**: U' moves to top-right, R' moves to front-right, F inserts, R restores displaced piece

#### Case 3c: Top-right position, correct orientation
- **Position**: (1, 1, 0) [top-right edge]
- **State**: White facing up (correct orientation)
- **Algorithm**: `U F2`
- **Explanation**: U aligns above target, F2 inserts

#### Case 3d: Top-right position, flipped orientation
- **Position**: (1, 1, 0) [top-right edge]
- **State**: White facing right (flipped orientation)
- **Algorithm**: `R' F [R restores (1,0,1)]`
- **Explanation**: R' moves to front-right, F inserts, R restores displaced piece

#### Case 3e: Top-back position, correct orientation
- **Position**: (0, 1, -1) [top-back edge]
- **State**: White facing up (correct orientation)
- **Algorithm**: `U2 F2`
- **Explanation**: U2 aligns above target, F2 inserts

#### Case 3f: Top-back position, flipped orientation
- **Position**: (0, 1, -1) [top-back edge]
- **State**: White facing back (flipped orientation)
- **Algorithm**: `U R' F [R restores (1,0,1)]`
- **Explanation**: U moves to top-right, R' moves to front-right, F inserts, R restores displaced piece

#### Case 3g: Top-left position, correct orientation
- **Position**: (-1, 1, 0) [top-left edge]
- **State**: White facing up (correct orientation)
- **Algorithm**: `U' F2`
- **Explanation**: U' aligns above target, F2 inserts

#### Case 3h: Top-left position, flipped orientation
- **Position**: (-1, 1, 0) [top-left edge]
- **State**: White facing left (flipped orientation)
- **Algorithm**: `L F' [L' restores (-1,0,1)]`
- **Explanation**: L moves to front-left, F' inserts, L' restores displaced piece

## Symmetry Transformations

For other target edges, apply these move transformations:

### White-Orange (target: bottom-right)
- F → R
- R → B  
- B → L
- L → F
- U → U (unchanged)
- D → D (unchanged)

### White-Blue (target: bottom-back)
- F → B
- R → L
- B → F
- L → R
- U → U (unchanged)
- D → D (unchanged)

### White-Red (target: bottom-left)
- F → L
- R → F
- B → R
- L → B
- U → U (unchanged)
- D → D (unchanged)

## Special Cases / Exceptions

### First Edge Exception
When solving the very first cross edge, optimization opportunities exist since no other edges need preservation.

#### Conditional Restoration Skip
All conditional restoration moves `[...]` can be skipped when solving the first edge:
- **Case 1b**: `D R F` (3 moves vs 4)
- **Case 1f**: `D' R F` (3 moves vs 4) 
- **Cases 2b,2c,2d,2e,2f,2h**: Skip all restoration moves
- **Cases 3b,3d,3f,3h**: Skip all restoration moves

#### First Edge Optimization - Single Move Solutions
For correctly oriented bottom layer edges, use simple D moves:

**Case 1c (first edge)**: Bottom-right, correct orientation
- **Regular algorithm**: `R2 U F2` (3 moves)
- **First edge optimization**: `D'` (1 move)
- **Explanation**: Rotates bottom-right directly to bottom-front target

**Case 1e (first edge)**: Bottom-back, correct orientation  
- **Regular algorithm**: `B2 U2 F2` (3 moves)
- **First edge optimization**: `D2` (1 move)
- **Explanation**: Rotates bottom-back directly to bottom-front target

**Case 1g (first edge)**: Bottom-left, correct orientation
- **Regular algorithm**: `L2 U' F2` (3 moves)
- **First edge optimization**: `D` (1 move)
- **Explanation**: Rotates bottom-left directly to bottom-front target

#### Implementation Logic
```
if (isFirstCrossEdge) {
    // Use optimized algorithms with no restoration
    // Use D moves for cases 1c, 1e, 1g
} else {
    // Use full algorithms with conditional restoration
}
```

## Symmetry Transformation System ✅

### Key Insight: Case Identification + Move Transformation
Rather than storing 96 algorithms (24 cases × 4 edges), we use:
1. **Identify the case** relative to the target edge color
2. **Apply canonical algorithm** for that case
3. **Transform moves** using rotation mapping

### Rotation Transformations
**White-Green** (front target): No transformation needed
**White-Blue** (back target): y2 rotation → F↔B, R↔L, U↔U, D↔D
**White-Orange** (right target): y rotation → F→R→B→L→F
**White-Red** (left target): y' rotation → F→L→B→R→F

### Example: Case 1d for Different Targets
- **White-Green**: Bottom-right, flipped → Case 1d → `R F`
- **White-Red**: Bottom-front, flipped → Case 1d → `R F` → Transform to `F L`
- **White-Blue**: Bottom-left, flipped → Case 1d → `R F` → Transform to `L B`
- **White-Orange**: Bottom-back, flipped → Case 1d → `R F` → Transform to `B R`

### Implementation Benefits
- **24 canonical algorithms** instead of 96
- **Mathematical consistency** across all edge colors
- **Systematic approach** scales to Cases 1, 2, and 3
- **Clean code structure** with transformation layer

## Implementation Status ✅

### ✅ COMPLETED (v2.1 - Systematic Cross Solver)
1. **CrossSolver API**: ✅ Unified `SolveEdge()` method with 24-case system
2. **Edge Classification**: ✅ `CrossEdgeClassifier.ClassifyEdgePosition()` - returns exact case from 24 possibilities
3. **Algorithm Database**: ✅ `CrossEdgeAlgorithms` - all 24 canonical algorithms with conditional restoration
4. **Symmetry Transformation**: ✅ Position AND color transformation for edge perspective consistency
5. **First Edge Optimization**: ✅ D-move shortcuts for first edge in bottom layer
6. **CLI Integration**: ✅ `--all-edges` debug flag for full cross analysis

### 🔧 Core Architecture Breakthrough
**Problem Solved**: Color transformation mismatch - position coordinates were transformed but colors array wasn't, causing orientation misclassification.

**Solution**: Transform both position AND colors array using Y/Y'/Y2 rotations to maintain coordinate system consistency.

**Result**: Accurate case classification - complex 4-move algorithms reduced to simple 1-move solutions where appropriate.

### 🎯 Polish Phase - Remaining Tasks

#### **1. Conditional Restoration Logic** (Priority: Medium)
- **Current**: All `[bracketed moves]` included regardless
- **Needed**: Smart checking if position actually contains solved cross edge
- **Files**: `CrossEdgeAlgorithms.ProcessConditionalRestorations()`
- **Benefit**: Eliminate unnecessary restoration moves

#### **2. Full Cross Solution Compression** (Priority: Medium)  
- **Current**: Individual algorithms compressed via `AlgorithmCompressor`
- **Needed**: Compress entire cross sequence (e.g., `R` + `R'` cancellation)
- **Implementation**: New `CrossSolutionCompressor` class
- **Benefit**: Shorter total move counts

#### **3. Cube Rotation Compatibility** (Priority: High)
- **Current**: Assumes fixed green-front perspective
- **Challenge**: After x/y'/z moves, "canonical front" changes
- **Needed**: Track cube orientation state in solver context
- **Test Case**: Apply `x y'` then solve cross - should still work
- **Impact**: Critical for integration with full solve sequences

#### **4. Superhuman Cross Optimization** (Priority: Low)
- **Current**: Fixed solve order (Green→Orange→Blue→Red)
- **Needed**: Try all 24 permutations (4! = 24), return shortest
- **Algorithm**: Generate permutations, solve each, pick best
- **Benefit**: Truly optimal cross solutions
- **Implementation**: New `SuperhumanCrossSolver` class

#### **5. Enhanced Test Coverage** (Priority: High)
- **Transformation Tests**: Verify Y/Y'/Y2 logic for all edge colors
- **Systematic Case Tests**: All 24 cases with known correct algorithms  
- **Rotation Compatibility**: Cross solving after various cube orientations
- **Integration Tests**: Full scramble-to-solution verification
- **Edge Cases**: First edge optimization, conditional restoration logic

## Testing Strategy

1. **Single move tests**: Ensure R→R', F→F', L→L', B→B'
2. **Complex scrambles**: Verify multi-move solutions work
3. **Incremental solving**: Ensure later edges don't disturb earlier ones
4. **All positions**: Test all 20 possible edge positions for each target

## Open Questions

1. Case 2b/2c algorithms need optimization
2. Case 3b needs testing for most efficient algorithm  
3. Case 1c and 3d need full definition for all positions
4. Consider if we need any 4-move or longer algorithms