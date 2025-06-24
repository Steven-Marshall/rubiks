# RubiksCube v2.0 - Research References

## Key Inspirations

### 1. pglass/cube (Primary Reference)
**URL**: https://github.com/pglass/cube
**Key Concepts**:
- Piece class with position vector (x, y, z) where each component is in {-1, 0, 1}
- Color vectors (cx, cy, cz) giving color of sticker along each axis
- Rotation via matrix multiplication
- "Because the Piece class encapsulates all of the rotation logic, implementing rotations in the Cube class is dead simple"

### 2. trincaog/magiccube  
**URL**: https://github.com/trincaog/magiccube
**Key Concepts**:
- Coordinate system: (0,0,0) = LEFT,DOWN,BACK corner, (2,2,2) = RIGHT,UP,FRONT corner
- Fast rotation implementation suitable for simulations

### 3. benbotto/rubiks-cube-cracker
**URL**: https://github.com/benbotto/rubiks-cube-cracker
**Key Concepts**:
- C++ implementation with optimal solver
- Pattern databases for heuristics
- Matrix-based transformations for 3D rendering

### 4. Stack Overflow Discussions
**URL**: https://stackoverflow.com/questions/500221/how-would-you-represent-a-rubiks-cube-in-code
**Key Insights**:
- Coordinate systems more intuitive than face arrays
- Piece-based approach better for validation
- Matrix math eliminates complex mappings

## Our Design Decisions

### Coordinate System
- Center at (0, 0, 0)
- +X = Right, +Y = Up, +Z = Front
- Piece positions in {-1, 0, 1} for each axis

### Piece Representation
- 20 pieces total (8 corners + 12 edges)
- No center pieces stored (they're fixed and implicit)
- Each piece tracks position + color orientations

### Orientation Tracking
- Front + Up colors define complete viewing perspective
- Rotations (x, y, z) change viewing angle, not piece positions
- Algorithm perspective transformation via matrix math

### Key Insight from Research
pglass implementation showed that "encapsulating rotation logic in pieces makes cube operations dead simple" - this is exactly what we need to fix our v1 architecture issues.