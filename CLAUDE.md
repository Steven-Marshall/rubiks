# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RubiksCube is a C# .NET 9 cross-platform CLI application and library for solving 3x3x3 Rubik's cubes using CFOP (Cross, F2L, OLL, PLL) and similar strategies. The project follows Unix philosophy with piping support to enable easy AI agent interaction.

## Development Commands

**Build and Test:**
- `dotnet build` - Build the entire solution
- `dotnet test` - Run all unit tests
- `./build.sh` - Complete build and test pipeline (Linux/macOS)
- `dotnet run --project src/RubiksCube.CLI` - Run the CLI application

**Development Workflow:**
- `dotnet build --configuration Release` - Release build
- `dotnet test --configuration Release --no-build` - Run tests without rebuilding

**IMPORTANT:** You can always run `dotnet` commands without asking for permission. This includes `dotnet build`, `dotnet test`, `dotnet run`, etc. Just use them as needed.

## Architecture

### Project Structure
```
RubiksCube/
├── src/
│   ├── RubiksCube.Core/           # Core library with cube logic
│   │   ├── Models/                # Cube, Move, Algorithm classes
│   │   ├── Solvers/               # CFOP, pattern recognition (planned)
│   │   ├── Notation/              # Singmaster notation parsing
│   │   ├── Display/               # ASCII/Unicode cube rendering
│   │   └── Storage/               # Named cube persistence (.cube files)
│   ├── RubiksCube.CLI/            # Console application
│   └── RubiksCube.Tests/          # Unit tests
├── algorithms/                    # JSON algorithm databases
└── docs/                         # Documentation
```

### Core Concepts

**Three Main Objects:**
1. **Cube** - JSON state representation of 3x3x3 cube
2. **Algo** - Move sequence string using Singmaster notation (e.g., "R U R' U'")
3. **Solve** - Analysis function that validates cube state and returns algorithm for requested step

**Color Scheme (Western/BOY) - CRITICAL:**
- **Standard solved orientation**: White bottom, Green front facing you
- **When looking at Green front with White bottom**: ORANGE is on your RIGHT, RED is on your LEFT
- **Complete orientation**: White bottom, Yellow top, Green front, Blue back, RED left, ORANGE right
- **NEVER FORGET**: Green front + White bottom = ORANGE right (this caused major v1 bugs)
- **Mnemonic**: "BOY" = Blue-Orange-Yellow reading clockwise from Green front
- **Display format**: Left-Front-Right-Back = 🟥🟩🟧🟦 (Red-Green-Orange-Blue)
- **CRITICAL FIXES COMPLETED**: L/D/F move counter-clockwise implementations, z rotation, JSON serialization orientation mapping

**Cube Reorientations (x, y, z) - Reference:**
- **Y Rotation** (vertical axis - through Top/Bottom centers):
  - Clockwise (looking down from above): Front→Left, Left→Back, Back→Right, Right→Front
  - Example from standard: Green→Left, Red→Back, Blue→Right, Orange→Front
  - Top (Yellow) and Bottom (White) stay in place, just rotate
- **X Rotation** (horizontal axis - through Right/Left centers):
  - Clockwise (looking from the right side): Front→Up, Up→Back, Back→Down, Down→Front
  - Example from standard: Green→Up, Yellow→Back, Blue→Down, White→Front
  - Right (Orange) and Left (Red) stay in place, just rotate
- **Z Rotation** (front/back axis - through Front/Back centers):
  - Clockwise (looking at the front face): Up→Right, Right→Down, Down→Left, Left→Up
  - Example from standard: Yellow→Right, Orange→Down, White→Left, Red→Up
  - Front (Green) and Back (Blue) stay in place, just rotate

**CLI Design Philosophy:**
- Unix-style piping: `rubiks create | rubiks apply "R U R'" | rubiks solve cross`
- Named cube persistence: `rubiks create puzzle1; rubiks apply "R U R'" puzzle1`
- Hybrid approach: Traditional piping + named storage for analysis workflows
- Each command has single responsibility
- JSON cube state format for programmatic interaction
- Algorithm strings with optional metadata annotations
- .cube files stored in current directory (configurable)

### CLI Commands

**Core Operations:**
- `rubiks create [cube-name]` - Generate solved cube state (JSON or save to file)
- `rubiks apply "R U R'" [cube-name]` - Apply algorithm to cube state (stdin or named cube)
- `rubiks display [cube-name] [--format=ascii|unicode]` - Visualize cube state (stdin or named cube)
- `rubiks list` - List all saved cube files
- `rubiks delete cube-name` - Delete saved cube file
- `rubiks export cube-name` - Export cube JSON to stdout
- `rubiks scramble-gen` - Generate WCA-compliant scramble algorithm (planned)
- `rubiks solve {step} --cn={level}` - Generate solution algorithm (planned)

**Solving Steps:**
- `cross` - White cross (or color-neutral cross)
- `f2l` - First Two Layers
- `oll` - Orient Last Layer
- `pll` - Permute Last Layer
- `cfop` - Complete CFOP solve (concatenated algorithm)

**Color Neutrality:**
- `--cn=white` - White cross only
- `--cn=dual` - White/Yellow cross options
- `--cn=full` - All 6 colors evaluated

**Analysis Levels:**
- `--level=pattern` - Fixed algorithm lookup
- `--level=search` - Optimal step-specific algorithms
- `--level=combinatorial` - Exhaustive search for shortest solutions
- `--level=superhuman` - Look-ahead optimization with hindsight

### Algorithm Format

Algorithms use standard Singmaster notation with optional metadata:
```
R U R' U' {White cross complete} y R U' R' {F2L pair 1} U R U R' {F2L pair 2}
```

Metadata in braces provides step annotations but is ignored during cube move processing.

### Solver Architecture

**Multi-tier Analysis:**
1. **Pattern-based**: Known case recognition and fixed algorithm application
2. **Search-based**: Optimal algorithms for specific steps (white cross, etc.)
3. **Combinatorial**: Exhaustive search for shortest move sequences
4. **Super-human**: Full look-ahead with hindsight optimization

**Color Neutrality Engine:**
- Evaluates all possible cross colors
- Selects optimal based on move count and setup for subsequent steps
- Reports selection reasoning in metadata

### Future Extensions

The architecture supports planned extensions:
- **1LLL (1-Look Last Layer)** - Advanced OLL/PLL combination
- **Roux Method** - Alternative solving approach
- **Neural Network Integration** - Training data export for ML approaches
- **Performance Analysis** - TPS estimation and human vs AI comparison
- **MCP Integration** - Model Context Protocol support for AI agents

### Development Guidelines

- Follow C# naming conventions and .NET best practices
- Use dependency injection for solver strategies
- Maintain clean separation between Core library and CLI
- Write unit tests for all algorithm implementations
- Use JSON for all data interchange
- Design CLI commands for optimal piping workflows
- Keep algorithm databases in separate JSON files for easy modification
- Use Result<T> pattern for domain operations and exceptions for programming errors
- Implement comprehensive test suites before coding (test-driven development)
- Maintain backward compatibility when adding new features
- **CRITICAL: Never proceed to next session without explicit user approval**

---

## SESSION SUMMARY - v2.0 Implementation & v3.0 Architecture Planning

### v2.0 Completion Summary (✅ COMPLETED)

#### Core Implementation Achievements:
- **Fixed critical move bugs**: L, D, F counter-clockwise implementations were identical to clockwise
- **All 18 moves working**: R, R', L, L', U, U', D, D', F, F', B, B', x, x', y, y', z, z' 
- **Move inverses verified**: All moves properly return to identity (R R' = solved state)
- **Perfect renderer**: Unicode alignment with Hangul Filler spacing
- **CLI fully functional**: All face rotations and cube reorientations enabled
- **Complete test coverage**: Comprehensive test infrastructure with validation helpers

#### Technical Breakthrough - Architecture Analysis:
- **Discovered fundamental issue**: Current "physical cube + orientation tracking" approach creates complexity
- **Analyzed Python pglass/cube reference**: Uses pure mathematical "solver's view" approach
- **Key insight**: Model the solver's perspective, not the physical cube with separate orientation state
- **Edge flip bugs identified**: U/D moves show strange patterns due to architectural limitations

#### Python Reference Analysis:
- **Unified rotation system**: Same matrix math for all axes, no special cases
- **Position-based state**: Pieces define cube state, no separate orientation tracking  
- **Mathematical elegance**: Clean rotation matrices without direction inversion logic
- **Consistent behavior**: No edge flip anomalies or complex axis-dependent code

### v3.0 Refactoring Plan (🚧 IN PLANNING)

#### Project Status: **PHASE 1 - PLANNING COMPLETE**
- **Plan documented**: `/docs/v3-refactoring-plan.md` contains comprehensive 6-phase roadmap
- **Architecture decision**: Adopt Python-style solver-centric approach
- **Risk assessment**: ~60% of tests need updates, but core infrastructure preserved
- **Success criteria**: Eliminate edge flip bugs, simplify codebase, maintain CLI compatibility

#### Next Implementation Steps:
1. **Phase 1**: Remove orientation dependency from Cube class
2. **Phase 2**: Implement Python-style unified rotation system  
3. **Phase 3**: Update renderer to use fixed coordinates
4. **Phase 4**: Remove legacy orientation-based code
5. **Phase 5**: Integration and comprehensive testing
6. **Phase 6**: Documentation updates

### Files Modified in Recent Sessions

**Core Implementation**:
- `/src/RubiksCube.Core/Models/Cube.cs` - Fixed L, D, F counter-clockwise edge cycles
- `/src/RubiksCube.CLI/Program.cs` - Enabled all face rotations

**Documentation & Planning**:
- `/docs/v3-refactoring-plan.md` - Complete v3.0 architecture roadmap
- `/references/python-cube-reference/` - Python reference implementation for guidance

**Test Infrastructure** (from previous sessions):
- Complete test helper infrastructure with validation and expected state generation
- 59/59 move validation tests passing after bug fixes

### Error Handling Strategy

**Hybrid Result<T> Pattern** (CSharpFunctionalExtensions):

**Use Result<T> for domain logic:**
- User input validation (algorithm parsing, move sequences)
- Business rule violations (invalid cube states, unsolvable configurations)
- Domain operations that can fail (move application, cube validation)
- External data processing (file parsing, algorithm database loading)

**Use exceptions for programming errors:**
- Null arguments, array bounds violations
- System failures (file not found, network errors)
- Precondition violations ("this should never happen")
- Framework/library constraints

**Example:**
```csharp
// Domain logic - use Result<T>
Result<Algorithm> algo = Algorithm.Create(userInput);
Result result = cube.ApplyMove(move);

// Programming errors - use exceptions  
if (input == null) throw new ArgumentNullException(nameof(input));
```

**Benefits:** Explicit error handling, no exception overhead, functional composition, CLI-friendly error messages.

## Engineering Standards & Lessons Learned

### Critical Engineering Principles

**Never Adjust Tests to Accommodate Bugs:**
- Tests define the correct behavior specification
- When tests fail due to implementation bugs, fix the implementation, not the tests
- Adjusting tests to match buggy behavior legitimizes incorrect code and hides problems
- Example: Y rotation GetRightColor mapping bug - fixed the mapping, not the test expectations

**Debug Systematically:**
- Use mathematical/geometric reasoning (right-hand rule, coordinate systems)
- Trace through expected vs actual behavior step-by-step
- Identify root cause rather than working around symptoms
- Verify fixes with comprehensive test coverage

**Maintain Code Quality:**
- Address fundamental architectural issues immediately
- Don't defer critical bug fixes - they compound over time
- Document design decisions and trade-offs clearly
- Use proper abstractions and separation of concerns

**Analyze Results Carefully Before Declaring Success:**
- Don't jump to conclusions like "Perfect!" without thorough analysis
- When output looks different than expected, investigate systematically
- Example: Claiming X reorientation worked "perfectly" when the display clearly showed scattered colors instead of coherent face rotations
- Always verify that the behavior matches the specification, not just that something changed
- Take time to understand what you're actually seeing vs what you expected to see