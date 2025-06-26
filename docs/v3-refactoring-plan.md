# v3.0 Refactoring Plan: Python-Style Solver-Centric Architecture

## Project Status: **PHASE 1 - PLANNING COMPLETE**

## Overview
Transform from "physical cube + orientation tracking" to "solver's view only" approach, following the proven Python pglass/cube implementation pattern.

**Key Insight**: Model the solver's perspective, not the physical cube. Eliminate orientation tracking by making piece positions the state.

## ✅ Phase 0: Analysis & Planning (COMPLETED)
- ✅ Analyzed Python pglass/cube implementation 
- ✅ Identified architectural differences
- ✅ Confirmed data structures are compatible
- ✅ Created detailed refactoring plan

## 🚧 Phase 1: Data Model Cleanup (IN PROGRESS)
**Goal**: Prepare data structures for solver-centric approach

### 1.1 Remove Orientation Dependency (PENDING)
- [ ] **Remove**: `CubeOrientation Orientation` property from Cube class
- [ ] **Remove**: All references to `cube.Orientation` in codebase
- [ ] **Keep**: `CubePiece` and `Position3D` classes (already compatible)
- [ ] **Update**: Cube constructor to not initialize orientation

### 1.2 Update Interfaces (PENDING)
- [ ] **Update**: `IsSolved` property to use position-based logic instead of orientation
- [ ] **Remove**: `ApplyReorientation` method (will be replaced with unified approach)
- [ ] **Keep**: `ApplyMove` method signature (will update implementation)

## 📋 Phase 2: Replace Rotation Logic (PENDING)
**Goal**: Implement Python-style unified rotation system

### 2.1 Create Python-Style Rotation Matrices (PENDING)
- [ ] **Add**: Static rotation matrices matching Python:
  - `ROT_XY_CW`, `ROT_XY_CC` (Z-axis rotations)  
  - `ROT_XZ_CW`, `ROT_XZ_CC` (Y-axis rotations)
  - `ROT_YZ_CW`, `ROT_YZ_CC` (X-axis rotations)
- [ ] **Verify**: Matrix math matches Python implementation

### 2.2 Implement Python-Style Piece Rotation (PENDING)
- [ ] **Replace**: `RotatePieceColors` method with Python approach:
  ```csharp
  private static CubePiece RotatePiece(CubePiece piece, int[,] matrix)
  {
      var oldPos = piece.Position;
      var newPos = RotationMatrix.Apply(matrix, oldPos);
      var posChange = new Position3D(newPos.X - oldPos.X, newPos.Y - oldPos.Y, newPos.Z - oldPos.Z);
      
      // Find which 2 axes changed and swap colors
      var changedAxes = GetChangedAxes(posChange);
      var newColors = SwapColorsOnChangedAxes(piece.Colors, changedAxes);
      
      return new CubePiece(piece.SolvedPosition, newColors).MoveTo(newPos);
  }
  ```

### 2.3 Create Unified Move System (PENDING)
- [ ] **Replace**: `ApplyFaceRotation` with Python-style approach:
  ```csharp
  private void ApplyRotation(Position3D face, int[,] matrix)
  {
      var affectedPieces = GetPiecesOnFace(face);
      foreach (var piece in affectedPieces)
      {
          var rotatedPiece = RotatePiece(piece, matrix);
          _pieces[rotatedPiece.Position] = rotatedPiece;
      }
  }
  ```

### 2.4 Define All Moves Python-Style (PENDING)
- [ ] **Replace**: Face moves with fixed coordinate definitions:
  ```csharp
  public void R() => ApplyRotation(new Position3D(1, 0, 0), ROT_YZ_CW);
  public void L() => ApplyRotation(new Position3D(-1, 0, 0), ROT_YZ_CC);
  public void U() => ApplyRotation(new Position3D(0, 1, 0), ROT_XZ_CW);
  public void D() => ApplyRotation(new Position3D(0, -1, 0), ROT_XZ_CC);
  public void F() => ApplyRotation(new Position3D(0, 0, 1), ROT_XY_CW);
  public void B() => ApplyRotation(new Position3D(0, 0, -1), ROT_XY_CC);
  ```

- [ ] **Add**: Cube reorientations as whole-cube rotations:
  ```csharp
  public void X() => ApplyRotationToAllPieces(ROT_YZ_CW);
  public void Y() => ApplyRotationToAllPieces(ROT_XZ_CW);
  public void Z() => ApplyRotationToAllPieces(ROT_XY_CW);
  ```

## 📋 Phase 3: Update Renderer (PENDING)
**Goal**: Make renderer position-based instead of orientation-based

### 3.1 Fixed Coordinate Rendering (PENDING)
- [ ] **Replace**: `GetFaceColors(cube, FaceDirection.Up)` with `GetFaceColors(cube, Y_POSITIVE)`
- [ ] **Remove**: All `cube.Orientation` usage from renderer
- [ ] **Implement**: Fixed coordinate face detection:
  ```csharp
  private CubeColor[] GetTopFace(Cube cube) => GetColorsAt(cube, y: 1);
  private CubeColor[] GetBottomFace(Cube cube) => GetColorsAt(cube, y: -1);
  private CubeColor[] GetFrontFace(Cube cube) => GetColorsAt(cube, z: 1);
  // etc.
  ```

### 3.2 Update Display Logic (PENDING)
- [ ] **Update**: Renderer to assume fixed solver perspective:
  - Top = Y+1, Bottom = Y-1
  - Right = X+1, Left = X-1  
  - Front = Z+1, Back = Z-1

## 📋 Phase 4: Remove Legacy Code (PENDING)
**Goal**: Clean up old orientation-based code

### 4.1 Remove Orientation Classes (PENDING)
- [ ] **Delete**: `CubeOrientation.cs`
- [ ] **Remove**: Orientation-related methods and properties
- [ ] **Remove**: Complex direction inversion logic (no longer needed)

### 4.2 Update Tests (PENDING)
- [ ] **Update**: All tests to use position-based expectations
- [ ] **Remove**: Orientation-dependent test assertions
- [ ] **Add**: Tests for new Python-style rotation logic

## 📋 Phase 5: Integration & Validation (PENDING)
**Goal**: Ensure everything works together

### 5.1 CLI Integration (PENDING)
- [ ] **Update**: CLI move parsing to call new methods
- [ ] **Verify**: Named cube storage still works
- [ ] **Test**: All move combinations work correctly

### 5.2 Comprehensive Testing (PENDING)
- [ ] **Test**: All 18 basic moves (R, R', L, L', U, U', D, D', F, F', B, B', x, x', y, y', z, z')
- [ ] **Verify**: Move inverses work (R R' = identity)
- [ ] **Test**: Complex sequences like "R U R' U'"
- [ ] **Verify**: Reorientations work (x R != R)

## 📋 Phase 6: Documentation Update (PENDING)
- [ ] **Update**: CLAUDE.md to reflect new architecture
- [ ] **Document**: Solver-centric coordinate system
- [ ] **Update**: API examples and usage patterns

## Success Criteria
- ✅ All 18 moves work correctly
- ✅ Move inverses return to identity  
- ✅ Reorientations change subsequent move behavior
- ✅ No edge flip bugs on any moves
- ✅ Renderer displays correctly for all cube states
- ✅ CLI maintains backward compatibility
- ✅ Existing cube files still load/save correctly

## Risk Mitigation
- **Backup**: Current working implementation before starting
- **Incremental**: Implement and test each phase separately
- **Validation**: Compare behavior with Python reference implementation
- **Rollback**: Plan to revert if critical issues arise

## Test Strategy Assessment

### Tests That Will Break (Need Rewrite):
- **Orientation Tests**: Any test using `cube.Orientation`
- **Reorientation Tests**: Tests for x, y, z moves (logic changing)
- **Renderer Tests**: Tests expecting orientation-based rendering
- **Move Tests**: Some tests may have wrong expectations for Y-axis moves

### Tests That Should Survive:
- **Piece Position Tests**: Basic piece movement tracking
- **Algorithm Parsing Tests**: String parsing logic unchanged
- **Storage Tests**: JSON serialization of piece positions
- **CLI Tests**: Interface tests (though some move behavior may change)

### New Tests Needed:
- **Python-Style Rotation Tests**: Verify matrix math matches Python
- **Unified Move Tests**: All 18 moves with proper expectations
- **Position-Based Rendering Tests**: Fixed coordinate display logic

**Estimated Test Impact**: ~60% of existing tests will need updates or rewrites, but test infrastructure can be preserved.

## References
- Python implementation: `/references/python-cube-reference/`
- Current v2.0 issues: U/D edge flip bugs, complex orientation logic
- Architecture inspiration: pglass/cube unified rotation approach