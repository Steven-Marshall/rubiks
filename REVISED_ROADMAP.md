# RubiksCube Project - Revised Development Roadmap

## Executive Summary

This document outlines the development roadmap for completing the RubiksCube CFOP solver. The foundation is complete with CLI, cube mechanics, and persistence implemented.

## Current Status: Foundation Complete ✅
- **221 tests passing**
- Core cube mechanics, display system, algorithm parsing ready
- CLI interface with Unix piping support implemented
- Named cube persistence system operational

---

 **Initial Context Setup**
  The conversation started with the user asking me to review the project state and CLAUDE.md file. I reviewed the RubiksCube project structure, identifying it as a C# .NET 9 CLI
  application with 480/518 tests passing after recent bug fixes for L, D, and F moves.

  **Testing and Bug Discovery Phase**
  The user systematically tested cube moves through the CLI:
  - Started with "R L D'" which revealed D' was doing clockwise instead of counter-clockwise rotation
  - Through careful testing, we discovered D and D' implementations were swapped
  - F and F' were both rotating in the wrong direction
  - Y rotation was going opposite to U move direction
  - Z rotation had direction issues

  **Bug Fixes Implemented**
  I fixed several critical bugs:
  1. Swapped D/D' implementations in Cube.cs
  2. Fixed F/F' implementations (both were backwards)
  3. Created comprehensive prime move tests
  4. Fixed Y rotation to match U move direction
  5. Fixed Z rotation to match F move direction
  6. Updated test expectations throughout

  **Major Architecture Issue Discovery**
  The user identified through "R z'" testing that the system doesn't properly handle piece orientation within faces after rotations. The current system only tracks which face is in
  which position, not how faces are oriented/rotated within their positions.

  **Architecture Analysis and Planning**
  We analyzed that the move application was already orientation-aware but rendering was not. The user proposed a center-based rendering approach instead of tracking accumulated
  rotations. We created a comprehensive plan to replace the orientation system with center-based approach.

  **Center-Based Implementation (Phases 1-4)**
  I implemented:
  1. CenterBasedOrientation class tracking center positions
  2. PieceLookup utilities for rendering
  3. Updated Cube class with dual orientation systems
  4. Created CenterBasedRenderer alongside existing renderer

  **Critical Realization**
  While implementing piece lookup, the user pointed out I was misunderstanding the fundamental approach - I should be finding what piece is currently at a physical location based on
  center orientations, not trying to match pieces by their colors.

  **Architecture Fragility Discussion**
  The user expressed concern that the core architecture was fragile, not reflecting physical reality of a cube with fixed pieces around an immovable center core. This led to
  researching other implementations.

  **Research Phase**
  I researched GitHub implementations finding:
  - pglass/cube: Elegant piece-based system with position vectors and matrix rotations
  - trincaog/magiccube: Fast coordinate system implementation
  - Key insight: Piece-based coordinate systems are cleaner and more robust

  **Complete Refactoring Decision**
  The user decided to archive the entire v1 codebase and start fresh with a piece-based coordinate system. Key requirements:
  - Keep the excellent CLI interface
  - Use piece-based data structures (20 pieces: 8 corners + 12 edges)
  - Centers are fixed and only indicate cube orientation
  - Need Front+Up to fully define orientation for algorithm application

  **v2.0 Implementation Started**
  I archived v1 to /archive/v1/, created research references, and set up new project structure with .NET 9 projects.

  Summary:
  1. Primary Request and Intent:
     - Initially: Review project state and catch up on RubiksCube C# .NET 9 project progress
     - Test and fix all cube moves (basic, prime, double, rotations) through systematic CLI verification
     - Fix discovered bugs in move implementations where some were swapped or rotating wrong directions
     - Create comprehensive test coverage for all move types
     - Discover and address fundamental issue with rotation display where sticker orientation within faces is not tracked
     - Research better cube data structures on GitHub
     - Archive entire v1 codebase and start fresh with piece-based coordinate system
     - Design new architecture that reflects physical cube reality with fixed center pieces
     - Implement v2.0 with piece-based approach while preserving excellent CLI interface

  2. Key Technical Concepts:
     - Rubik's Cube move notation (R, L, U, D, F, B and variants R', R2, etc.)
     - Cube rotations (x, y, z) that change viewing perspective
     - Western/BOY color scheme (Yellow top, Green front, Red left, Orange right, Blue back, White bottom)
     - Clockwise vs counter-clockwise rotations from face perspective
     - Center-based vs face-based orientation tracking
     - Piece-based coordinate systems with position vectors (x,y,z) in {-1,0,1}
     - Matrix mathematics for rotations
     - Front + Up orientation defining complete viewing perspective
     - Algorithm perspective transformation
     - Physical cube constraints (20 movable pieces: 8 corners + 12 edges)

  3. Files and Code Sections:
     - `/archive/v1/src/RubiksCube.Core/Models/Cube.cs`
        - Original core cube implementation with face-based storage
        - Fixed D/D' swap (lines 529-589) and F/F' implementations (lines 407-467)
        - Contains ApplyMove method that was already orientation-aware

     - `/archive/v1/src/RubiksCube.Core/Models/CubeOrientation.cs`
        - Handled cube rotations but only tracked face positions, not orientations
        - Fixed Y rotation to match U move direction:
        ```csharp
        case RotationDirection.CounterClockwise:
            // y': F → R → B → L → F (counter-clockwise when viewed from above)
            _positionToFace[CubeFace.Front] = oldMapping[CubeFace.Left];
            _positionToFace[CubeFace.Right] = oldMapping[CubeFace.Front];
            _positionToFace[CubeFace.Back] = oldMapping[CubeFace.Left];
            _positionToFace[CubeFace.Right] = oldMapping[CubeFace.Back];
        ```

     - `/src/RubiksCube.Core/Models/CenterBasedOrientation.cs` (v1 attempt)
        - New class tracking center colors in each viewing position
        - Fixed y rotation direction after user feedback

     - `/v2-references.md`
        - Documents key research findings from GitHub implementations
        - References pglass/cube as primary inspiration

     - `/archive/v1/README.md`
        - Documents lessons learned from v1
        - Lists strengths and architecture issues discovered

     - New v2.0 project structure created:
        - `RubiksCube.sln` - New solution file
        - `src/RubiksCube.Core/` - Core library project
        - `src/RubiksCube.CLI/` - Console application
        - `src/RubiksCube.Tests/` - Test project

  4. Errors and fixes:
     - D and D' moves were swapped:
       - Fixed by swapping their implementations in Cube.cs
       - User feedback: "ok. this is better"

     - F and F' moves both rotating backwards:
       - Fixed by correcting their implementations
       - User identified through careful analysis: "i think this is wrong. and i think it's F'"

     - Y rotation going opposite to U move:
       - Initially implemented wrong direction
       - User feedback: "BINGO! you're learning"
       - Fixed to match U move direction (F → L → B → R → F for clockwise)

     - Center-based Y rotation was backwards:
       - User correction: "y' you expect orange to be in front? it should be red"
       - Fixed counter-clockwise to be F → R → B → L → F

     - Piece lookup conceptual error:
       - I was trying to match pieces by color instead of finding what's at a location
       - User clarification: "you're looking for what cube is currently at that location"

     - My spatial reasoning errors:
       - User feedback: "you struggle making assessments which require spacial awareness"

  5. Problem Solving:
     - Systematically tested each move type through CLI to verify behavior
     - Created visual comparisons between expected and actual cube states
     - Discovered fundamental orientation tracking issue through R z' test case
     - Researched existing implementations to find better architectural approaches
     - Decided on complete refactoring to piece-based system for robustness

  6. All user messages:
     - "right, catch up where we are on this project in directory. review claude.md and all relevant files"
     - "right. let's do some quick tests. using CLI commands. from an unscrambled cube, apply R L D' and show me the output."
     - "ok. i think that's wrong. i think the D' move you have run is a clockwise one. let's do a simple test that just runs D' on an unscrambled cube to check. just show me cli
  display output. no need for json."
     - "ok. why don't we do one by one. show me D."
     - "agreed. now do we have tests for D and D' and are they passing?"
     - "ok. let's re-run the R L D' algo and see if we are correct now"
     - "ok. this is better. let's do R2 L2 U2 D2 F2 B2 on a clean cube"
     - "right. let's do B F' D."
     - "i think this is wrong. and i think it's F' run that alone and think critically about it. look at the tests we have written for F and F'. are they logically correct?"
     - "ok. let's be super slow about this. let's look at F' and F' alone. again. think critically about output. do not think about how in interacts with F at the moment."
     - "i said. do NOT think about F at the moment. just think about F'. do you think F' is doing an anti-clockwise rotation or clockwise?"
     - "correct. F' is going in the wrong direction. now hold that thought, and do the same analysis on F. and only F."
     - "ok. in terms of the tests we are doing.... do F and F' currently pass tests or not?"
     - "right, show me an F' move now in the CLI"
     - "show me an F move (from clean cube)"
     - "ok. now let's take things slowly. let's one by one look at each of B F R L D U moves. firstly - does the CLI move give us what we expect? secondly - does the test do what we
  think it should do? thirdly - does this all align. consider each one, one at a time. do not consider prime moves yet at all."
     - "show me a U move and then show me a D move."
     - "both U and D are correct and your analysis is wrong. it appears that you struggle making assessments which require spacial awareness concepts such as clockwisse and
  anticlockwise when viewing objects from a different direction."
     - "once we have the tests fully working then that will help avoid mistakes like this. right. let's now go through each of the prime moves. do we have individual tests for each
  one, along with combnations with the non-prime moves to check they are inverses?"
     - "yes please"
     - "ok, please proceed. let me know along the way if you need my help in reviewing"
     - "show me F' and only F'"
     - "ok. let's fix the test to match your new expectation"
     - "do we have tests for the double moves? so D2 = D D for example?"
     - "right. let's look at rotations. let's start at x. show me x in CLI first and only that."
     - "that looks correct. does it pass the single test and the test with it's inverse (i.e. return to base state)"
     - "right. show me y"
     - "ok, let's think about this. show me a U move."
     - "BINGO! you're learning. let's also check the tests for y and y' to see if they are testing what is expected. then let's fix the code for y (likely y') and the test code."
     - "stop"
     - "stop stop stop"
     - "ok, please proceed on the rotation test code, but stop after finishing it and we will manually check everything to be 100% sure"
     - "show me R z'"
     - "ok. let's just do z' and nothing else"
     - "ah got you. yes. that makes sense to me now. we just move the faces but lose orientation. in fact... we don't really need to know it though. the renderer could get all the
  information it needs knowing that the physical structure of the cube remains the same in term of centre spots. so we could fix it without tracking each rotation. but the notation at
   the moment is confusing. we should be thinking .... where are the coloured center spots (orientation). then when we render, take into account the orientation as we determine where
  to put the stickers in the 3x3 part of the display. does that make sense?"
     - "ok. that seems to make sense. do we know how many 90 degree rotations it's taken (+ or -)? how do we store rotation information?"
     - "explain to me what we currently do with rotations. what do we store?"
     - "right. now let's do R z'"
     - "ok. there is something wrong in how we're handling rotations. it might be in the display code rather than the actual rotation code which only changes the viewpoint. but let's
  take the front face... you have WWW on the right hand side. BUT... it should now be on the top row due to the rotation. similar to the other 3 faces around the sides. the rotation
  does not just change where the faces sit, but the orientation in how we view them on the display mapping. when we have solved faces we don't see it. but with a mixed face it's more
  obvious. for example. look at the front right edge... it's all white... how can that be?"
     - "this will affect all rotations and primes as well. think deeply about what we need to do and how to fix this. do not change any code yet without my approval. analysis first."
     - "yes. outline how you plan to do 1 (maybe using some psuedo code) first. make no actual code changes"
     - "maybe we are over complicating this. we know that the cube stickers do not move themselves with rotations. with a rotation we should maybe track purely cube aligment as a
  visual aid. we know (front center spot, top center spot, right center spot) - so GYO when we start. we use this information when we render and render from that viewpoint. so for the
   top face first row we know that we start at the back/top edge from left/right back is blue, top is yellow... so we use that to render the row. using center spots to specify
  alignment. does that make sense?"
     - "1. technically we only need to track 3 given we know the opposite one... but it might be easier to to keep 6? 2. alignment tells you where each of the center cubes is. when
  you render... let's take the top left sticker we render on solved cube with no rotation... that's the TOP sticker of the yellow (top) in the corner with the left side (red) and back
   side (blue). the next one along is the edge piece ...top side (yellow), back side (blue). does that make sense? we can clarify more if needed.... 3. i think it should replace it.
  so we track where the center spots are from a visual perspective"
     - "this provides orthogonal data for rotations versus movements. we track cube orientation and where the pieces have moved to separately. we render based on the orientation. and
  we apply our algos based on the orientation. we need to remember that. we will need to review that bit of movement code as well."
     - "let's confirm the existing code. the current code may indeed ignore rotations as we've not really tested it yet combined with orientations. take a look before we do anything
  so we get a sense of if that suspicion is true or not"
     - "yes. i think this is much much clearer. but i guess it means making a lot of changes. let's make a clear plan of action before proceeding. look at all the places we need to
  change. initial orientation. changing orientation. what orientation means when making rotations. then rendering. is that all?"
     - "are we doing this so that we can compare one against the other as we go along? the pattern recognition at the moment is not really used heavily so if we have to redo that from
   scratch that's not a problem. please proceed with phases 1-3 and then we should look at updating the tests. a lot should still work regarding movement etc. only stop and ask if you
   need clarification or you hit a problem"
     - "stop for a moment. you mention on y' you expect orange to be in front? it should be red...."
     - "y' (counter-clockwise rotation when viewed from above (lookying at yellow face unsolved))... you are moving them all the wrong way."
     - "can you show me CLI for x for y and then for z please. one at a time."
     - "ok you seem to say that x,y and z matches is that the case? do we need to retest?"
     - "don't do full scramble. look at the 1 move + 1 rotation example we used before. like R z'"
     - "yes :0"
     - "ok, have a think about how you need to fix this. if you are unsure then we can go through in detail. do not to any coding just yet. give me your thoughts"
     - "you are correct in your last comment. take a look at how we currently do things. we know that stickers are also fixed so the current data structures may be almost too rich for
   what the reality is. take a look at the data structures and come up with a proposal for what we should do, or an analysis of each approach. do not code anything yet."
     - "let's go with A. to protect ourselves... it may also be worth writing some code at some stage that validates a cube to check we're not moving stickers around in a way that is
  not physically allowed by an actual cube. but we can put that on the to do list for later."
     - "yes please"
     - "no please proceed"
     - "i feel that maybe we're exposing that the core architecture is fragile. in some ways we are not reflecting the physical reality of a cube. we have a number of stickers when
  really we have pieces that a fixed around a center core which is immovable. our moves are complex with orientations as we need to know what the "front" colour is. but if we had a
  data structure of the cube that represented concepts like front(color) then we could execute all our algos on front() by referring to the cube which has a fixed center piece.
  rotating edge and corner pieces around that front center piece. display then would be totally separate. all i need to know is to have an alignment of front,bottom and that dictates
  everything else. i love all the CLI interface. it fits very well with the ultimate aim to build a tool that an AI can interface with to aid a human solver. but it feels more and
  more like we're building on the wrong foundations. can you do a bit of research on what other people have done around cube data structures like this. maybe visit github?"
     - "i like this. the cube is "fixed" relative to some concept (like yellow top, green front). BUT when we apply an algo, we need to apply it from the perspective of looking at a
  face. does this data structure allow us to do it in an elegant code way?"
     - "yes, include this in the plan. we should track how the cube is orientated for us though and adjust for applying x,y,z rotations, so we always know which way to look at.
  also... the issue is not just "red at front" but we need to know at least one other face to fully define orientation. do the archiving, create the new plan. make sure we keep
  reference of sources we're looking at. write the plan in details with progress indicators in it. when you've done that stop. write no code at this stage,"
     - "let's go!"

### Session 7: Scramble Generation
**Goal**: Generate WCA-compliant scrambles for testing and practice

**Deliverables**:
- ScrambleGenerator class with WCA rules:
  - 20-25 move scrambles
  - No consecutive same-face moves (R R)
  - No opposite faces within 2 moves (R L R)
- CLI command: `rubiks scramble-gen [--moves=20] [--seed=12345]`
- Integration with apply/display pipeline
- Validation that scrambles actually scramble the cube

**Acceptance Criteria**:
```bash
rubiks create | rubiks apply "$(rubiks scramble-gen)" | rubiks display
rubiks create scramble1 && rubiks apply "$(rubiks scramble-gen)" scramble1
```

**Effort**: 1 session  
**Dependencies**: CLI and persistence complete ✅
**Note**: Will use only face moves (R, L, U, D, F, B) initially

### Session 8: Rotation Support Implementation
**Goal**: Implement x, y, z rotations for cube reorientation

**Deliverables**:
- Rotation class/interface to represent whole-cube rotations
- Parser updates to recognize x, y, z notation with modifiers (', 2)
- Cube rotation logic for all three axes
- Comprehensive test coverage for rotations
- Integration with existing Algorithm class

**Acceptance Criteria**:
```bash
rubiks create | rubiks apply "x y' z2" | rubiks display
rubiks create | rubiks apply "y' R U R' U' R' F R F'" | rubiks display  # T-perm
```

**Effort**: 1 session
**Dependencies**: Session 7 complete ✅
**Reference**: See `/docs/references/rotation-implementation-notes.md`

### Session 9: Pattern Recognition Design Discussion
**Goal**: Design pattern recognition system for cube state analysis

**Discussion Topics**:
- High-level state recognition (scrambled, solved, OLL, F2L stages)
- Specific case identification (OLL #21, PLL T-perm, etc.)
- API design for pattern queries
- Integration with future solver components
- Performance considerations for pattern matching

**Deliverables**:
- Design document for pattern recognition API
- Decision on implementation approach
- Test scenarios for pattern matching
- Integration plan with algorithm database

**Example API Concepts**:
```bash
rubiks pattern cube1  # Returns: "scrambled" or "OLL" or "F2L(1)" etc.
rubiks pattern cube1 --detailed  # Returns: "PLL: T-perm" or "OLL: Sune" etc.
```

**Effort**: 0.5 session (design discussion)
**Dependencies**: Understanding of rotation system ✅

---

## Phase 2: Algorithm Foundation

### Session 10: Algorithm Database Foundation
**Goal**: Create JSON algorithm storage and basic OLL/PLL data

**Deliverables**:
- JSON schema for algorithm storage
- `algorithms/oll.json` with basic OLL cases (subset of 57)
- `algorithms/pll.json` with basic PLL cases (subset of 21)  
- AlgorithmDatabase service to load/query algorithms
- Basic pattern matching framework

**Acceptance Criteria**:
- Can load algorithms from JSON files
- Can match simple cube patterns to stored algorithms

**Effort**: 0.5-1 session
**Dependencies**: Rotation support and pattern design complete ✅

---

## Phase 3: Basic CFOP Solving

### Session 11: Pattern Recognition Implementation
**Goal**: Implement the pattern recognition system designed in Session 9

**Deliverables**:
- PatternMatcher service implementation
- Cube state analysis for solving stages
- Specific case recognition (OLL/PLL patterns)
- Integration with algorithm database
- CLI command: `rubiks pattern [cube-name] [--detailed]`

**Acceptance Criteria**:
```bash
rubiks create | rubiks apply "R U R' U'" | rubiks pattern
# Returns: "F2L incomplete" or similar

rubiks pattern cube1 --detailed
# Returns: "OLL: Sune (Case 27)" or "PLL: T-perm" etc.
```

**Effort**: 1 session
**Dependencies**: Sessions 9, 10 complete ✅

### Session 12: Cross Solver
**Goal**: Detect and solve white cross

**Deliverables**:
- Cross pattern detection
- Generate algorithms to solve white cross
- CLI command: `rubiks solve cross`
- Integration with scramble→solve pipeline

**Acceptance Criteria**:
```bash
rubiks create | rubiks apply "$(rubiks scramble-gen)" | rubiks solve cross
# Returns algorithm to solve cross

rubiks create scramble1 && rubiks apply "$(rubiks scramble-gen)" scramble1
rubiks solve cross scramble1 | rubiks apply - scramble1
rubiks pattern scramble1  # Should show "cross solved"
```

**Effort**: 1-1.5 sessions
**Dependencies**: Pattern recognition complete ✅

### Session 13: MVP Integration & Testing
**Goal**: End-to-end testing of scramble→solve pipeline

**Deliverables**:
- Integration tests for full workflow
- Performance testing
- Documentation updates
- Bug fixes and refinements

**Effort**: 0.5-1 session
**Dependencies**: Cross solver complete ✅

---

## Phase 4: Complete CFOP System

### Session 14: F2L Solver
**Goal**: First Two Layers pattern recognition and solving

**Deliverables**:
- F2L pair detection and tracking
- Algorithm selection for 42 basic F2L cases
- CLI command: `rubiks solve f2l`
- Integration with cross→F2L pipeline

**Effort**: 1-2 sessions
**Dependencies**: Cross solver complete ✅

### Session 15: OLL Solver  
**Goal**: Orient Last Layer (57 cases)

**Deliverables**:
- OLL pattern recognition for all 57 cases
- Algorithm selection from database
- CLI command: `rubiks solve oll`
- Integration with F2L→OLL pipeline

**Effort**: 1 session
**Dependencies**: F2L solver complete ✅

### Session 16: PLL Solver
**Goal**: Permute Last Layer (21 cases)

**Deliverables**:
- PLL pattern recognition for all 21 cases
- Algorithm selection from database
- CLI command: `rubiks solve pll`
- Full CFOP pipeline: `rubiks solve cfop`

**Effort**: 1 session
**Dependencies**: OLL solver complete ✅

### Session 17: Color Neutrality & Analysis Levels
**Goal**: Multi-color analysis and optimization levels

**Deliverables**:
- Evaluate all 6 cross colors
- Implement analysis levels (pattern, search, combinatorial, superhuman)
- CLI options: `--cn=white|dual|full` and `--level=pattern|search|etc`
- Performance metrics and reporting

**Effort**: 1-2 sessions
**Dependencies**: Full CFOP complete ✅

---

## Phase 5: Advanced Features (Future)

- 1LLL (1-Look Last Layer)
- Roux Method
- Neural Network Integration  
- Performance Analysis
- MCP Integration

---

## Summary & Timeline

### Completed (Sessions 1-6.5) ✅
- Core cube mechanics with all moves
- Algorithm parsing and execution
- Display system (ASCII/Unicode)
- CLI with Unix piping
- Named cube persistence
- 221 tests passing

### Upcoming Work (Sessions 7-17)
**Phase 1: Pre-Algorithm Foundation (Sessions 7-9)**
- Scramble generation
- Rotation support (x, y, z)
- Pattern recognition design

**Phase 2: Algorithm Foundation (Session 10)**
- JSON algorithm database
- Basic OLL/PLL algorithms

**Phase 3: Basic CFOP (Sessions 11-13)**
- Pattern recognition implementation
- Cross solver
- MVP integration

**Phase 4: Complete CFOP (Sessions 14-17)**
- F2L solver
- OLL solver (57 cases)
- PLL solver (21 cases)
- Color neutrality & analysis levels

### Key Implementation Notes

**Technical Considerations:**
- Maintain Result<T> pattern for error handling
- Ensure JSON format consistency across commands
- Design for Unix philosophy (small tools, piping)
- Support both piped and named cube workflows

**Quality Gates:**
- All tests pass before proceeding
- Each phase requires user approval
- Documentation updated with each session
- No regressions in existing functionality

**Critical Reminder:**
- **NEVER proceed to next session without explicit user approval**
- Each major feature requires discussion and sign-off
- Maintain backward compatibility throughout