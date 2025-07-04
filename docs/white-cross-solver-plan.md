# White Cross Solver Implementation Plan

## Overview
Implement two solving approaches: "Human Level" and "Super Human" for white cross completion.

## Human Level Solver Logic

### Move Prioritization Strategy
When selecting which edge to solve next, use the following priority order:

1. **Primary**: Fewest total moves to solve the edge
2. **Tiebreaker 1**: Moves that solve multiple pieces simultaneously 
   - Example: D' solving both white-blue and white-red in one move
   - This is maximum efficiency - prioritize moves that affect multiple target edges
3. **Tiebreaker 2**: Prefer shorter individual algorithms (1 move > 2 moves > 3 moves)

### Implementation Requirements
The human solver needs to:
1. Detect when multiple edges can be solved by the same move
2. Group and prioritize moves that solve multiple edges simultaneously  
3. Build optimal sequence based on multi-edge efficiency
4. Apply compression to the final concatenated algorithm

### Example from Testing
Current scramble state:
- white-blue: bottom-left → D' (1 move)
- white-red: bottom-front → D' (1 move)  
- white-green: top-front → F2 (1 move)
- white-orange: top-right → U' B' R (3 moves)

Optimal human order:
1. D' (solves 2 edges: white-blue + white-red) → **Priority 1**
2. F2 (solves white-green) → Priority 2
3. U' B' R (solves white-orange) → Priority 3

Result: 5 moves instead of naive 6 moves

## Super Human Solver Logic
- Generate all possible solve orders (4! = 24 permutations)
- For each order, concatenate algorithms and compress
- Return the shortest compressed result

## Current Status
- [x] Smart conditional restoration implemented
- [x] Actual position display with verbose mode
- [x] Move counting in verbose output
- [ ] Human level solver implementation
- [ ] Super human solver implementation
- [ ] CLI integration for solve command