# v3.0 Python Architecture Refactoring Plan

## Status: **READY TO EXECUTE**

## Overview
Complete the transition to Python pglass/cube architecture with full 3-element color arrays and solver-centric approach. Build on existing progress from previous session.

## What We're Keeping
- ✅ Python-style rotation matrices (ROT_XY_CW, ROT_YZ_CC, etc.)
- ✅ Fixed coordinate renderer
- ✅ `RotatePiecePython()` method structure
- ✅ CLI testing capability
- ✅ Basic cube initialization pattern

## What We're Fixing
- ❌ CubePiece compact color arrays → Full 3-element arrays
- ❌ Color conversion logic → Direct axis swapping
- ❌ Edge flip bugs → Proper color axis tracking

---

## Phase 1: Core Data Model (PRIORITY 1)
**Goal**: Switch to Python-style 3-element color arrays

### 1.1 Update CubePiece Class ⭐
```csharp
public class CubePiece 
{
    public CubeColor?[] Colors { get; set; } // Always [X, Y, Z] with nulls
    // Remove: IReadOnlyList<CubeColor> Colors
    // Update: Constructor to accept CubeColor?[3]
}
```

### 1.2 Update Cube Initialization ⭐
```csharp
// Change from:
(new Position3D(0, 1, 1), new[] { CubeColor.Yellow, CubeColor.Green })

// To:
(new Position3D(0, 1, 1), new CubeColor?[] { null, CubeColor.Yellow, CubeColor.Green })
```

### 1.3 Simplify RotatePiecePython() ⭐
```csharp
// Remove conversion methods, direct swap:
(newColors[axis1], newColors[axis2]) = (newColors[axis2], newColors[axis1]);
```

**Test After Phase 1**: L L' should return to solved state

---

## Phase 2: Renderer Updates (PRIORITY 2)
**Goal**: Handle nullable colors in display

### 2.1 Update CubeRenderer ⭐
- Modify color lookup to handle `CubeColor?[]` arrays
- Update `GetPieceColorForFaceFixed()` to index into 3-element arrays
- Handle null colors gracefully

### 2.2 Test Rendering ⭐
- Verify solved cube displays correctly
- Test single moves show correct patterns

**Test After Phase 2**: U move should show correct pattern without edge flips

---

## Phase 3: Serialization & Storage (PRIORITY 3)
**Goal**: JSON serialization works with new format

### 3.1 Update JSON Serialization ⭐
- Ensure `CubeColor?[]` serializes/deserializes correctly
- Test round-trip: create → save → load → display

### 3.2 CLI Compatibility ⭐
- Verify all `python-apply` commands work
- Test piping and file operations

**Test After Phase 3**: Full CLI workflow functions

---

## Phase 4: Move Set Completion (PRIORITY 4)
**Goal**: All basic moves work correctly

### 4.1 Verify All Moves ⭐
- Test: R R', L L', U U', D D', F F', B B'
- Verify: No edge flips, proper color swapping

### 4.2 Add Missing Moves ⭐
- Implement: F, F', B, B' using Python matrices
- Test: All 6 face moves + inverses

**Test After Phase 4**: Complete move set validation

---

## Phase 5: Test Suite Updates (PRIORITY 5)
**Goal**: Update existing tests for new architecture

### 5.1 Core Model Tests ⭐
- Update `CubePiece` tests for 3-element arrays
- Fix serialization tests
- Update move validation tests

### 5.2 Integration Tests ⭐
- CLI command tests
- Display tests
- Storage tests

---

## Phase 6: Cleanup & Optimization (PRIORITY 6)
**Goal**: Remove old code, optimize performance

### 6.1 Remove Legacy Code ⭐
- Remove old rotation methods
- Remove conversion helpers
- Clean up unused orientation code

### 6.2 Documentation ⭐
- Update code comments
- Update CLI help text
- Document Python compatibility

---

## Success Criteria

✅ **Phase 1 Complete**: L L' returns to solved state  
✅ **Phase 2 Complete**: U move shows correct colors (no edge flips)  
✅ **Phase 3 Complete**: Full CLI workflow operational  
✅ **Phase 4 Complete**: All 12 basic moves work correctly  
✅ **Phase 5 Complete**: Test suite passes  
✅ **Phase 6 Complete**: Clean, maintainable codebase  

---

## Implementation Strategy

1. **Start Small**: Phase 1 only, test immediately
2. **Test Frequently**: After each phase, verify CLI functionality
3. **Rollback Ready**: Keep git commits small for easy reversion
4. **CLI First**: Maintain `python-apply` command throughout

**Next Action**: Execute Phase 1.1 - Update CubePiece to use 3-element arrays