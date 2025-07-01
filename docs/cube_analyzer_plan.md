# Cube Analyzer Plan

## Overview
Two-component system for analyzing Rubik's cube state and suggesting next moves using **backward analysis** for accurate state detection.

## Two-Component Architecture

### Component 1: State Recognition
- Analyzes current cube state using backward detection flow
- Identifies stage (solved/PLL/OLL/F2L/cross/unsolved)
- Provides detailed progress information
- Detects specific problems (misoriented pieces, missing pairs, etc.)

### Component 2: Algorithm Suggestion
- Based on recognition results, suggests specific moves to make progress
- Outputs algorithm strings only (never executes moves)
- User decides whether to apply suggestions via `rubiks apply`
- Follows Unix philosophy: analyzer suggests, user executes

**Critical Principle**: The analyzer NEVER applies moves to cubes - it only produces algorithm suggestions.

## Unix Workflow Example
```bash
# 1. Analyze current state and get suggestion
rubiks analyze cube-name
# Output: "Cross: White cross 3/4 complete, missing WG edge (at UF, flipped)"
#         "Suggested: F D R F' D' R'"

# 2. User decides to apply the suggestion
rubiks apply "F D R F' D' R'" cube-name

# 3. Check progress after applying moves
rubiks analyze cube-name  
# Output: "Cross: White cross complete!"
#         "Suggested: Start F2L - look for WR-WG pair"

# 4. Continue the cycle
rubiks apply "R U R' U' F R F'" cube-name
rubiks analyze cube-name
```

This separation allows users to:
- Get advice without forced execution
- Learn by seeing suggestions before applying
- Make strategic decisions about which moves to use
- Maintain full control over cube state

## Analysis Flow (Backward Detection)
The analyzer works backwards through CFOP stages for accurate detection:

1. **solved?** → "Cube completely solved"
2. **PLL stage?** → "PLL: OLL complete, permuting last layer" 
3. **OLL stage?** → "OLL: F2L complete, orienting last layer"
4. **F2L stage?** → "F2L: Cross complete, working on pairs"
5. **Cross stage?** → "Cross: Building foundation (white by default)"
6. **Otherwise** → "Unsolved: No clear CFOP progress"

**Rationale:** Each stage includes all previous requirements (solved cube has cross complete, PLL stage has F2L complete, etc.). This prevents misclassification and provides accurate progress assessment.

## Stage 2: Detailed Progress

### Cross details:
- `cross(white, 4)` - White cross with 4 edges correctly placed
- `cross(white, 2)` - White cross with 2 edges correctly placed
- `cross(yellow, 3)` - Yellow cross with 3 edges correctly placed

### F2L details:
- `F2L(0)` - Cross complete, no F2L pairs
- `F2L(2)` - 2 F2L pairs completed
- `F2L(4)` - All F2L pairs completed (ready for OLL)

### OLL details:
- `OLL(cross)` - Yellow cross formed on top
- `OLL(L-shape)` - Specific OLL pattern detected
- `OLL(case-21)` - Specific OLL case number

### PLL details:
- `PLL(T-perm)` - T permutation case
- `PLL(U-perm)` - U permutation case
- `PLL(corners)` - Only corners need permuting

## CLI Interface

### Basic Usage:
```bash
# Direct analysis (white cross by default)
rubiks analyze cube-name

# Piping support  
rubiks export cube-name | rubiks analyze

# Cross color options
rubiks analyze cube-name --cross=white          # White cross (default)
rubiks analyze cube-name --cross=yellow         # Yellow cross only
rubiks analyze cube-name --cross=dual           # White + Yellow options  
rubiks analyze cube-name --cross=neutral        # All 6 colors (color-neutral)

# Output formats
rubiks analyze cube-name --verbose              # Detailed explanations
rubiks analyze cube-name --json                 # JSON for automation

# Force specific stage analysis (debugging)
rubiks analyze cube-name --stage=cross          # Force cross analysis only
rubiks analyze cube-name --stage=f2l            # Force F2L analysis only
```

### Output Format Examples (Human):
```bash
# Component 1: Recognition + Component 2: Suggestion
"Solved: Cube completely solved"
"No moves needed"

"PLL: T-perm detected"  
"Suggested: R U R' U' R' F R2 U' R' U'"

"OLL: F2L complete, yellow cross formed, case #21 detected"
"Suggested: R U2 R' U' R U R' U' R U' R'"

"F2L: White cross complete, 2/4 pairs done"
"Suggested: Look for WR-WG pair - R U R' U' F R F'"

"Cross: White cross 3/4 complete, missing WG edge (at UF, flipped)"
"Suggested: F D R F' D' R'"

"Unsolved: No clear CFOP progress"
"Suggested: Start white cross - F D R F'"
```

### Output Format (JSON for piping):
```json
{
  "recognition": {
    "stage": "cross",
    "cross_color": "white", 
    "progress": 3,
    "total": 4,
    "complete": false,
    "details": {
      "missing": [{
        "edge": "WG",
        "current_position": "UF",
        "oriented": false,
        "problem": "misoriented_and_misplaced"
      }]
    }
  },
  "suggestion": {
    "algorithm": "F D R F' D' R'",
    "description": "Fix WG edge orientation and placement",
    "next_stage": "f2l"
  }
}
```

## Architecture

### Core Library Structure:
```
RubiksCube.Core/
├── PatternRecognition/              # Component 1: Recognition
│   ├── IStageAnalyzer.cs           # Base interface for stage detection
│   ├── CubeStateAnalyzer.cs        # Main orchestrator (backward analysis)
│   ├── SolvedAnalyzer.cs           # Check if cube is solved
│   ├── PLLAnalyzer.cs              # PLL case recognition  
│   ├── OLLAnalyzer.cs              # OLL case recognition
│   ├── F2LAnalyzer.cs              # F2L pair detection
│   ├── CrossAnalyzer.cs            # Cross detection (white default)
│   └── UnsolvedAnalyzer.cs         # Fallback for no progress
├── Solving/                         # Component 2: Algorithm Suggestions
│   ├── ISolver.cs                  # Base interface for move suggestions
│   ├── CrossSolver.cs              # Suggests cross completion moves
│   ├── F2LSolver.cs                # Suggests F2L pair algorithms
│   ├── OLLSolver.cs                # Suggests OLL algorithms
│   └── PLLSolver.cs                # Suggests PLL algorithms
├── PatternRecognition/Models/
│   ├── RecognitionResult.cs        # Component 1 output
│   ├── SuggestionResult.cs         # Component 2 output
│   ├── AnalysisResult.cs           # Combined result (recognition + suggestion)
│   └── PatternCase.cs              # For OLL/PLL cases
└── PatternRecognition/Data/
    ├── OLLPatterns.json            # 57 OLL cases with algorithms
    └── PLLPatterns.json            # 21 PLL cases with algorithms
```

## Cross Recognition Features

### CrossAnalyzer:
1. **White cross by default** - Primary analysis target
2. **Cross color options:**
   - `--cross=white` - White cross only (default)
   - `--cross=yellow` - Yellow cross only  
   - `--cross=dual` - White + Yellow options
   - `--cross=neutral` - All 6 colors (color-neutral)
3. **Edge tracking:**
   - Position (correct/incorrect)
   - Orientation (flipped/correct)
   - Location if misplaced
4. **Scoring system:**
   - 0/4 = No cross progress
   - 4/4 = Cross complete
   - Track partial progress

### Key Decisions:
- Consider cross "complete" only when edges are both positioned and oriented correctly
- Track edges in cross layer but wrong position separately  
- Default to white cross for beginner-friendly experience
- Only runs during backward analysis if higher stages (F2L/OLL/PLL) are not detected

## Pattern Matching System (OLL/PLL)

### JSON Pattern Format:
```json
{
  "name": "T-Perm",
  "category": "PLL", 
  "pattern": {
    "corners": ["solved"],
    "edges": ["UF-UR", "UR-UB"]
  },
  "algorithm": "R U R' U' R' F R2 U' R' U' R U R' F'",
  "probability": 0.0556
}
```

### Benefits:
- Import community pattern databases
- Easy updates without recompiling
- Support for multiple algorithms per case
- Color-neutral pattern matching
- Extensible for new solving methods

## Implementation Order
1. **SolvedAnalyzer** (simplest check - all pieces in solved positions)
2. **CrossAnalyzer** (foundation of CFOP, white cross default)
3. **CubeStateAnalyzer** (orchestrator with backward analysis flow)
4. **F2LAnalyzer** (builds on cross detection)
5. **OLL/PLL pattern matching system** (advanced pattern recognition)
6. **JSON pattern database import** (extensible case library)

## Future Considerations
- Support for other solving methods (Roux, ZZ)
- Advanced metrics (efficiency scores, move count estimates)
- Training mode suggestions
- Integration with solving algorithms