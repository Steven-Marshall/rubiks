# WCA Scrambler Implementation Plan

## Overview
Port the existing v1 WCA scramble generator to v3 architecture with updated defaults and WCA compliance.

## Requirements Based on Official WCA Regulations

### WCA Compliance:
- **Random state requirement**: Must require at least 2 moves to solve (Regulation 4b3)
- **Equal probability**: Each valid state should have equal probability (Regulation 4b3)
- **Standard notation**: F, B, R, L, U, D with ', 2 modifiers (Regulation 12a1)
- **No filtering**: Cannot inspect or select scrambles (Regulation 4b1)

### Implementation Approach:
- **Phase 1**: Random move sequence generator with WCA rules (immediate implementation)
- **Phase 2**: True random state generation (future enhancement after solving algorithms)

## Default Configuration

### Move Count:
- **Default**: 25 moves (updated from v1's 20 moves)
- **Configurable**: `--moves` parameter for custom lengths
- **Range**: Typically 18-30 moves for practical use

### WCA Rule Compliance:
- **No consecutive same-face moves**: R R forbidden
- **No opposite faces within 2 moves**: R L R forbidden in strict mode
- **Face moves only**: R, L, U, D, F, B with ', 2 modifiers
- **No slice moves**: M, E, S not used in official scrambles
- **No rotations**: x, y, z not used in official scrambles

## Architecture

### Core Library Structure:
```
RubiksCube.Core/
├── Scrambling/
│   ├── IScrambleGenerator.cs       # Interface for future extensibility
│   ├── WCAScrambleGenerator.cs     # Main implementation
│   ├── ScrambleOptions.cs          # Configuration options
│   └── Models/
│       ├── ScrambleResult.cs       # Contains algorithm and metadata
│       └── ScrambleValidation.cs   # Rule validation utilities
```

### Existing v1 Implementation to Port:
**Source**: `/archive/v1/src/RubiksCube.Core/Scrambling/ScrambleGenerator.cs`

**Key Features to Maintain:**
- ✅ Configurable move count (update default 20 → 25)
- ✅ Seed-based reproducibility 
- ✅ Strict WCA rule validation
- ✅ Result<T> error handling pattern
- ✅ Comprehensive test coverage (15 test cases)

## CLI Interface

### Command Structure:
```bash
# Basic scramble generation
rubiks scramble-gen
> R U2 F' D2 B L F2 U' L2 D' R2 F2 U' B2 U L2 U' F2 D' R B2 L2 U R' U

# Custom move count
rubiks scramble-gen --moves=20
rubiks scramble-gen --moves=30

# Reproducible scrambles with seed
rubiks scramble-gen --seed=12345

# Apply scramble to cube
rubiks scramble-gen | rubiks create scrambled-cube --apply-stdin

# Chain with other operations
rubiks scramble-gen | rubiks create temp | rubiks display
```

### Output Format:
- **Default**: Plain algorithm string for easy piping
- **Verbose**: Include metadata about move count, seed used
- **JSON**: Structured output for programmatic use (future)

## Implementation Details

### Core Components:

#### 1. **WCAScrambleGenerator.cs**
```csharp
public class WCAScrambleGenerator
{
    public Result<ScrambleResult> Generate(ScrambleOptions options)
    
    // Key methods from v1:
    private bool IsValidNextMove(Move lastMove, Move currentMove)
    private Move GenerateRandomMove(Random random, Move? previousMove)
    private Algorithm GenerateRandomAlgorithm(int moveCount, int? seed)
}
```

#### 2. **ScrambleOptions.cs**
```csharp
public class ScrambleOptions
{
    public int MoveCount { get; set; } = 25;  // Updated default
    public int? Seed { get; set; }
    public bool StrictWCAMode { get; set; } = true;
}
```

#### 3. **ScrambleResult.cs**
```csharp
public class ScrambleResult
{
    public Algorithm Algorithm { get; set; }
    public int MoveCount { get; set; }
    public int SeedUsed { get; set; }
    public TimeSpan GenerationTime { get; set; }
}
```

### WCA Rule Implementation:

#### Move Validation Rules:
```csharp
// From v1 implementation - proven working
private bool IsValidNextMove(Move lastMove, Move currentMove)
{
    // Rule 1: No consecutive moves on same face (R R)
    if (lastMove.Face == currentMove.Face) return false;
    
    // Rule 2: No opposite faces within 2 moves (R L R in strict mode)
    if (StrictMode && AreOppositeFaces(lastMove.Face, currentMove.Face))
        return false;
        
    return true;
}

private bool AreOppositeFaces(char face1, char face2)
{
    return (face1, face2) switch
    {
        ('R', 'L') or ('L', 'R') => true,
        ('U', 'D') or ('D', 'U') => true,
        ('F', 'B') or ('B', 'F') => true,
        _ => false
    };
}
```

#### Move Generation:
```csharp
private Move GenerateRandomMove(Random random, Move? previousMove)
{
    var faces = new[] { 'R', 'L', 'U', 'D', 'F', 'B' };
    var directions = new[] { 
        MoveDirection.Clockwise, 
        MoveDirection.CounterClockwise, 
        MoveDirection.Double 
    };
    
    // Generate valid moves following WCA rules
    // Implementation from v1 with proven logic
}
```

## Testing Strategy

### Port Existing Tests:
- **15 comprehensive test cases** from v1 implementation
- **Rule validation tests**: Same-face, opposite-face restrictions
- **Seed reproducibility**: Same seed produces same scramble
- **Move count validation**: Correct number of moves generated
- **Algorithm parsing**: Generated scrambles parse correctly

### New Tests for v3:
- **Integration with Move/Algorithm classes**
- **CLI command functionality** 
- **Piping compatibility**
- **Error handling with Result<T> pattern**

### Test Coverage:
```csharp
[Fact] public void Generate_DefaultOptions_Produces25Moves()
[Fact] public void Generate_WithSeed_ProducesReproducibleResults()
[Fact] public void Generate_ValidatesWCARules_NoConsecutiveSameFace()
[Fact] public void Generate_ValidatesWCARules_NoOppositeFacesInStrictMode()
[Theory] public void Generate_CustomMoveCount_ProducesCorrectLength(int moves)
```

## Migration Steps

### 1. **Port Core Classes**
- Copy `ScrambleGenerator.cs` from v1 archive
- Update namespace and dependencies for v3
- Update default move count: 20 → 25
- Integrate with v3 Move/Algorithm classes

### 2. **Update Architecture**
- Implement `IScrambleGenerator` interface
- Add proper dependency injection support
- Update error handling to match v3 patterns

### 3. **CLI Integration**
- Add `scramble-gen` command to CLI
- Implement argument parsing for `--moves` and `--seed`
- Add to help text and examples

### 4. **Testing**
- Port existing test suite
- Update tests for v3 architecture
- Add CLI integration tests
- Verify WCA rule compliance

### 5. **Documentation**
- Update CLAUDE.md with scramble-gen command
- Add examples to help text
- Document WCA compliance features

## Future Enhancements (Phase 2)

### True Random State Generation:
- **Requirement**: Implement cube solving algorithms first
- **Method**: Generate random valid cube state, solve optimally
- **Benefit**: True WCA compliance with equal probability distribution
- **Implementation**: Use Kociemba algorithm or similar two-phase solver

### Advanced Features:
- **Multiple scramble output**: Generate sets of scrambles
- **Scramble verification**: Validate scramble produces solvable state
- **Performance optimization**: Faster generation for bulk usage
- **Competition mode**: Additional WCA compliance features

## Success Criteria

### Immediate (Phase 1):
- ✅ 25-move scrambles generated with WCA rule compliance
- ✅ Seed-based reproducibility working
- ✅ CLI integration with piping support
- ✅ All existing v1 tests passing in v3 architecture
- ✅ No regression in functionality from v1

### Future (Phase 2):
- ✅ True random state generation implemented
- ✅ Optimal scramble sequences computed
- ✅ Full WCA tournament compliance achieved