using System.Text.Json;
using System.Text.Json.Serialization;
using RubiksCube.Core.Mathematics;
using RubiksCube.Core.Algorithms;
using CSharpFunctionalExtensions;

namespace RubiksCube.Core.Models;

/// <summary>
/// Represents a 3x3x3 Rubik's cube using a piece-based coordinate system.
/// Contains 20 movable pieces (8 corners + 12 edges) and tracks orientation.
/// Centers are fixed and only used for orientation reference.
/// </summary>
public class Cube
{
    private readonly Dictionary<Position3D, CubePiece> _pieces;
    
    /// <summary>
    /// All pieces on the cube (8 corners + 12 edges + 6 centers)
    /// </summary>
    public IReadOnlyCollection<CubePiece> Pieces => _pieces.Values;
    
    /// <summary>
    /// Corner pieces only
    /// </summary>
    public IEnumerable<CubePiece> Corners => _pieces.Values.Where(p => p.Type == PieceType.Corner);
    
    /// <summary>
    /// Edge pieces only  
    /// </summary>
    public IEnumerable<CubePiece> Edges => _pieces.Values.Where(p => p.Type == PieceType.Edge);
    
    /// <summary>
    /// Center pieces only
    /// </summary>
    public IEnumerable<CubePiece> Centers => _pieces.Values.Where(p => p.Type == PieceType.Center);
    
    /// <summary>
    /// Creates a new cube in solved state with standard orientation
    /// </summary>
    public static Cube CreateSolved()
    {
        return new Cube();
    }
    
    /// <summary>
    /// Private constructor creates solved cube
    /// </summary>
    private Cube()
    {
        _pieces = new Dictionary<Position3D, CubePiece>();
        InitializeSolvedPieces();
    }
    
    /// <summary>
    /// Copy constructor for creating cube copies
    /// </summary>
    private Cube(Dictionary<Position3D, CubePiece> pieces)
    {
        _pieces = new Dictionary<Position3D, CubePiece>(pieces);
    }
    
    /// <summary>
    /// Initializes all 26 pieces in their solved positions with correct colors
    /// </summary>
    private void InitializeSolvedPieces()
    {
        // 8 Corner pieces (3 colors each) - V3.0 [X, Y, Z] arrays
        var corners = new[]
        {
            // Top corners (Y = 1)
            (new Position3D(-1, 1, 1), new CubeColor?[] { CubeColor.Red, CubeColor.Yellow, CubeColor.Green }),    // Top-Left-Front
            (new Position3D(1, 1, 1), new CubeColor?[] { CubeColor.Orange, CubeColor.Yellow, CubeColor.Green }),  // Top-Right-Front  
            (new Position3D(-1, 1, -1), new CubeColor?[] { CubeColor.Red, CubeColor.Yellow, CubeColor.Blue }),    // Top-Left-Back
            (new Position3D(1, 1, -1), new CubeColor?[] { CubeColor.Orange, CubeColor.Yellow, CubeColor.Blue }),  // Top-Right-Back
            
            // Bottom corners (Y = -1)
            (new Position3D(-1, -1, 1), new CubeColor?[] { CubeColor.Red, CubeColor.White, CubeColor.Green }),    // Bottom-Left-Front
            (new Position3D(1, -1, 1), new CubeColor?[] { CubeColor.Orange, CubeColor.White, CubeColor.Green }),  // Bottom-Right-Front
            (new Position3D(-1, -1, -1), new CubeColor?[] { CubeColor.Red, CubeColor.White, CubeColor.Blue }),    // Bottom-Left-Back  
            (new Position3D(1, -1, -1), new CubeColor?[] { CubeColor.Orange, CubeColor.White, CubeColor.Blue })   // Bottom-Right-Back
        };
        
        // 12 Edge pieces - V3.0 [X, Y, Z] arrays with nulls for missing axes
        var edges = new[]
        {
            // Top edges (Y = 1)
            (new Position3D(0, 1, 1), new CubeColor?[] { null, CubeColor.Yellow, CubeColor.Green }),    // Top-Front
            (new Position3D(0, 1, -1), new CubeColor?[] { null, CubeColor.Yellow, CubeColor.Blue }),    // Top-Back
            (new Position3D(-1, 1, 0), new CubeColor?[] { CubeColor.Red, CubeColor.Yellow, null }),     // Top-Left
            (new Position3D(1, 1, 0), new CubeColor?[] { CubeColor.Orange, CubeColor.Yellow, null }),   // Top-Right
            
            // Middle edges (Y = 0)
            (new Position3D(-1, 0, 1), new CubeColor?[] { CubeColor.Red, null, CubeColor.Green }),      // Middle-Left-Front
            (new Position3D(1, 0, 1), new CubeColor?[] { CubeColor.Orange, null, CubeColor.Green }),    // Middle-Right-Front
            (new Position3D(-1, 0, -1), new CubeColor?[] { CubeColor.Red, null, CubeColor.Blue }),      // Middle-Left-Back
            (new Position3D(1, 0, -1), new CubeColor?[] { CubeColor.Orange, null, CubeColor.Blue }),    // Middle-Right-Back
            
            // Bottom edges (Y = -1)
            (new Position3D(0, -1, 1), new CubeColor?[] { null, CubeColor.White, CubeColor.Green }),    // Bottom-Front
            (new Position3D(0, -1, -1), new CubeColor?[] { null, CubeColor.White, CubeColor.Blue }),    // Bottom-Back
            (new Position3D(-1, -1, 0), new CubeColor?[] { CubeColor.Red, CubeColor.White, null }),     // Bottom-Left
            (new Position3D(1, -1, 0), new CubeColor?[] { CubeColor.Orange, CubeColor.White, null })    // Bottom-Right
        };
        
        // Center pieces (6 total) - V3.0 style with single color and 2 nulls
        var centers = new[]
        {
            (new Position3D(1, 0, 0), new CubeColor?[] { CubeColor.Orange, null, null }),    // Right center
            (new Position3D(-1, 0, 0), new CubeColor?[] { CubeColor.Red, null, null }),      // Left center
            (new Position3D(0, 1, 0), new CubeColor?[] { null, CubeColor.Yellow, null }),    // Up center
            (new Position3D(0, -1, 0), new CubeColor?[] { null, CubeColor.White, null }),    // Down center
            (new Position3D(0, 0, 1), new CubeColor?[] { null, null, CubeColor.Green }),     // Front center
            (new Position3D(0, 0, -1), new CubeColor?[] { null, null, CubeColor.Blue })      // Back center
        };
        
        // Add all pieces to the cube
        foreach (var (position, colors) in corners)
        {
            _pieces[position] = new CubePiece(position, colors);
        }
        
        foreach (var (position, colors) in edges)
        {
            _pieces[position] = new CubePiece(position, colors);
        }
        
        foreach (var (position, colors) in centers)
        {
            _pieces[position] = new CubePiece(position, colors);
        }
    }
    
    /// <summary>
    /// Gets the piece at the specified position, or null if no piece exists there
    /// </summary>
    public CubePiece? GetPieceAt(Position3D position)
    {
        return _pieces.TryGetValue(position, out var piece) ? piece : null;
    }
    
    /// <summary>
    /// Checks if the cube is in a solved state
    /// </summary>
    public bool IsSolved => _pieces.Values.All(p => p.IsSolved);
    
    /// <summary>
    /// Creates a deep copy of this cube
    /// </summary>
    public Cube Clone()
    {
        var clonedPieces = _pieces.ToDictionary(
            kvp => kvp.Key,
            kvp => new CubePiece(kvp.Value.SolvedPosition, kvp.Value.Colors).MoveTo(kvp.Value.Position)
        );
        
        return new Cube(clonedPieces);
    }
    
    /// <summary>
    /// X rotation - rotates entire cube around X-axis (like rotating along Right-Left axis)
    /// Uses Python ROT_YZ_CC matrix to rotate all pieces
    /// </summary>
    public void X()
    {
        var matrix = new int[,] {
            { 1, 0, 0 },
            { 0, 0, -1 },
            { 0, 1, 0 }
        };
        RotateAllPieces(matrix);
    }
    
    /// <summary>
    /// X' rotation - counter-clockwise X rotation
    /// </summary>
    public void Xi()
    {
        var matrix = new int[,] {
            { 1, 0, 0 },
            { 0, 0, 1 },
            { 0, -1, 0 }
        };
        RotateAllPieces(matrix);
    }
    
    /// <summary>
    /// Y rotation - rotates entire cube around Y-axis (like rotating along Up-Down axis)
    /// Uses Python ROT_XZ_CW matrix to rotate all pieces
    /// </summary>
    public void Y()
    {
        var matrix = new int[,] {
            { 0, 0, -1 },
            { 0, 1, 0 },
            { 1, 0, 0 }
        };
        RotateAllPieces(matrix);
    }
    
    /// <summary>
    /// Y' rotation - counter-clockwise Y rotation
    /// </summary>
    public void Yi()
    {
        var matrix = new int[,] {
            { 0, 0, 1 },
            { 0, 1, 0 },
            { -1, 0, 0 }
        };
        RotateAllPieces(matrix);
    }
    
    /// <summary>
    /// Z rotation - rotates entire cube around Z-axis (like rotating along Front-Back axis)
    /// Uses Python ROT_XY_CW matrix to rotate all pieces
    /// </summary>
    public void Z()
    {
        var matrix = new int[,] {
            { 0, 1, 0 },
            { -1, 0, 0 },
            { 0, 0, 1 }
        };
        RotateAllPieces(matrix);
    }
    
    /// <summary>
    /// Z' rotation - counter-clockwise Z rotation
    /// </summary>
    public void Zi()
    {
        var matrix = new int[,] {
            { 0, -1, 0 },
            { 1, 0, 0 },
            { 0, 0, 1 }
        };
        RotateAllPieces(matrix);
    }
    
    /// <summary>
    /// Rotates all pieces in the cube using the given rotation matrix (v3.0 style)
    /// </summary>
    private void RotateAllPieces(int[,] matrix)
    {
        var newPositions = new Dictionary<Position3D, CubePiece>();
        
        foreach (var piece in _pieces.Values)
        {
            var rotatedPiece = RotatePiece(piece, matrix);
            newPositions[rotatedPiece.Position] = rotatedPiece;
        }
        
        // Update cube with new positions
        _pieces.Clear();
        foreach (var (position, piece) in newPositions)
        {
            _pieces[position] = piece;
        }
    }
    
    /// <summary>
    /// Applies any move (R, L, U, D, F, B, x, y, z) to the cube
    /// </summary>
    public void ApplyMove(Move move)
    {
        var direction = move.Direction == MoveDirection.CounterClockwise 
            ? RotationDirection.CounterClockwise 
            : RotationDirection.Clockwise;
            
        var times = move.Direction == MoveDirection.Double ? 2 : 1;
        
        for (int i = 0; i < times; i++)
        {
            // Check move type: axis rotation (x, y, z), slice move (M, E, S), or face rotation (R, L, U, D, F, B)
            if (move.Face == 'x' || move.Face == 'y' || move.Face == 'z')
            {
                ApplyAxisRotation(move.Face, direction);
            }
            else if (move.Face == 'M' || move.Face == 'E' || move.Face == 'S')
            {
                ApplySliceRotation(move.Face, direction);
            }
            else
            {
                ApplyFaceRotation(move.Face, direction);
            }
        }
    }
    
    // ===== V3.0 ROTATION METHODS =====
    // These implement the unified solver-centric approach (based on pglass/cube)
    
    /// <summary>
    /// V3.0 piece rotation - applies matrix to piece position and swaps colors on changed axes
    /// </summary>
    private static CubePiece RotatePiece(CubePiece piece, int[,] matrix)
    {
        var oldPos = piece.Position;
        var newPos = RotationMatrix.Apply(matrix, oldPos);
        
        // Check if newPos is valid before creating Position3D
        if (Math.Abs(newPos.X) > 1 || Math.Abs(newPos.Y) > 1 || Math.Abs(newPos.Z) > 1)
        {
            throw new InvalidOperationException($"Matrix rotation produced invalid position: {newPos} from {oldPos}");
        }
        
        // Clone the piece colors for modification
        var newColors = (CubeColor?[])piece.Colors.Clone();
        
        var deltaX = newPos.X - oldPos.X;
        var deltaY = newPos.Y - oldPos.Y; 
        var deltaZ = newPos.Z - oldPos.Z;
        
        // Python: if not any(rot): return  # no change occurred
        if (deltaX == 0 && deltaY == 0 && deltaZ == 0)
        {
            return new CubePiece(piece.SolvedPosition, newColors) { Position = newPos };
        }
        
        // Python: if rot.count(0) == 2: rot += matrix * rot
        var zeroCount = (deltaX == 0 ? 1 : 0) + (deltaY == 0 ? 1 : 0) + (deltaZ == 0 ? 1 : 0);
        if (zeroCount == 2)
        {
            var (rotX, rotY, rotZ) = RotationMatrix.ApplyRaw(matrix, deltaX, deltaY, deltaZ);
            deltaX += rotX;
            deltaY += rotY;
            deltaZ += rotZ;
        }
        
        // Find which 2 axes changed and swap colors (Python approach)
        var changedAxes = GetChangedAxes(deltaX, deltaY, deltaZ);
        
        // Python: self.colors[i], self.colors[j] = self.colors[j], self.colors[i]
        if (changedAxes.axis1 < 3 && changedAxes.axis2 < 3)
        {
            (newColors[changedAxes.axis1], newColors[changedAxes.axis2]) = 
                (newColors[changedAxes.axis2], newColors[changedAxes.axis1]);
        }
        
        return new CubePiece(piece.SolvedPosition, newColors) { Position = newPos };
    }
    
    
    /// <summary>
    /// Gets the two axes that changed during rotation (for color swapping)
    /// </summary>
    private static (int axis1, int axis2) GetChangedAxes(int deltaX, int deltaY, int deltaZ)
    {
        var changedAxes = new List<int>();
        
        if (deltaX != 0) changedAxes.Add(0); // X-axis
        if (deltaY != 0) changedAxes.Add(1); // Y-axis
        if (deltaZ != 0) changedAxes.Add(2); // Z-axis
        
        if (changedAxes.Count != 2)
        {
            throw new InvalidOperationException($"Expected exactly 2 axes to change during rotation, got {changedAxes.Count}. Position change: ({deltaX}, {deltaY}, {deltaZ})");
        }
        
        return (changedAxes[0], changedAxes[1]);
    }
    
    
    /// <summary>
    /// V3.0 face rotation - applies rotation to all pieces on a face
    /// </summary>
    private void ApplyFaceRotationMatrix(Position3D face, int[,] matrix)
    {
        var affectedPieces = GetPiecesOnFace(face);
        var updates = new Dictionary<Position3D, CubePiece>();
        
        // First, remove all affected pieces from their current positions
        foreach (var piece in affectedPieces)
        {
            _pieces.Remove(piece.Position);
        }
        
        // Then, rotate and place them in their new positions
        foreach (var piece in affectedPieces)
        {
            var rotatedPiece = RotatePiece(piece, matrix);
            _pieces[rotatedPiece.Position] = rotatedPiece;
        }
    }
    
    /// <summary>
    /// Gets pieces that belong to a specific face using v3.0 coordinate system
    /// </summary>
    private List<CubePiece> GetPiecesOnFace(Position3D face)
    {
        return _pieces.Values.Where(piece =>
        {
            var pos = piece.Position;
            // A piece is on a face if it has the same coordinate as the face
            return (face.X != 0 && pos.X == face.X) ||
                   (face.Y != 0 && pos.Y == face.Y) ||
                   (face.Z != 0 && pos.Z == face.Z);
        }).ToList();
    }
    
    
    /// <summary>
    /// Applies a single face rotation using our mathematical approach
    /// </summary>
    private void ApplyFaceRotation(char face, RotationDirection direction)
    {
        // Use v3.0 face rotation
        var isClockwise = direction == RotationDirection.Clockwise;
        
        // Get the appropriate rotation matrix based on face and direction
        int[,] matrix;
        Position3D facePosition;
        
        switch (char.ToUpper(face))
        {
            case 'R': // Right face (X = 1)
                matrix = isClockwise ? RotationMatrix.ROT_YZ_CW : RotationMatrix.ROT_YZ_CC;
                facePosition = new Position3D(1, 0, 0);
                break;
            case 'L': // Left face (X = -1)
                matrix = isClockwise ? RotationMatrix.ROT_YZ_CC : RotationMatrix.ROT_YZ_CW;
                facePosition = new Position3D(-1, 0, 0);
                break;
            case 'U': // Up face (Y = 1)
                matrix = isClockwise ? RotationMatrix.ROT_XZ_CW : RotationMatrix.ROT_XZ_CC;
                facePosition = new Position3D(0, 1, 0);
                break;
            case 'D': // Down face (Y = -1)
                matrix = isClockwise ? RotationMatrix.ROT_XZ_CC : RotationMatrix.ROT_XZ_CW;
                facePosition = new Position3D(0, -1, 0);
                break;
            case 'F': // Front face (Z = 1)
                matrix = isClockwise ? RotationMatrix.ROT_XY_CW : RotationMatrix.ROT_XY_CC;
                facePosition = new Position3D(0, 0, 1);
                break;
            case 'B': // Back face (Z = -1)
                matrix = isClockwise ? RotationMatrix.ROT_XY_CC : RotationMatrix.ROT_XY_CW;
                facePosition = new Position3D(0, 0, -1);
                break;
            default:
                throw new ArgumentException($"Invalid face: {face}");
        }
        
        // Apply v3.0 face rotation
        ApplyFaceRotationMatrix(facePosition, matrix);
    }
    
    /// <summary>
    /// Applies axis rotation (x, y, z) to the entire cube
    /// </summary>
    private void ApplyAxisRotation(char axis, RotationDirection direction)
    {
        var isClockwise = direction == RotationDirection.Clockwise;
        
        // Get the appropriate rotation matrix for the entire cube
        int[,] matrix = axis switch
        {
            'x' => isClockwise ? RotationMatrix.ROT_YZ_CW : RotationMatrix.ROT_YZ_CC,
            'y' => isClockwise ? RotationMatrix.ROT_XZ_CW : RotationMatrix.ROT_XZ_CC,
            'z' => isClockwise ? RotationMatrix.ROT_XY_CW : RotationMatrix.ROT_XY_CC,
            _ => throw new ArgumentException($"Invalid axis: {axis}")
        };
        
        // Apply rotation to ALL pieces (this is an axis rotation, not a face rotation)
        ApplyAxisRotationMatrix(matrix);
    }
    
    /// <summary>
    /// Applies slice rotation (M, E, S) to the middle layer pieces
    /// </summary>
    private void ApplySliceRotation(char slice, RotationDirection direction)
    {
        var isClockwise = direction == RotationDirection.Clockwise;
        
        // Get the appropriate rotation matrix and slice position for each slice
        int[,] matrix;
        Position3D slicePosition;
        
        switch (slice)
        {
            case 'M': // Middle slice between L and R faces (X = 0)
                // M move uses ROT_YZ_CC (same as L face direction) for standard M
                matrix = isClockwise ? RotationMatrix.ROT_YZ_CC : RotationMatrix.ROT_YZ_CW;
                slicePosition = new Position3D(0, 0, 0); // X = 0 plane
                break;
            case 'E': // Equatorial slice between U and D faces (Y = 0) 
                // E move uses XZ rotation (same as U/D faces) but counter-clockwise matches D direction
                matrix = isClockwise ? RotationMatrix.ROT_XZ_CC : RotationMatrix.ROT_XZ_CW;
                slicePosition = new Position3D(0, 0, 0); // Y = 0 plane
                break;
            case 'S': // Standing slice between F and B faces (Z = 0)
                // S move uses XY rotation (same as F/B faces) and clockwise matches F direction
                matrix = isClockwise ? RotationMatrix.ROT_XY_CW : RotationMatrix.ROT_XY_CC;
                slicePosition = new Position3D(0, 0, 0); // Z = 0 plane
                break;
            default:
                throw new ArgumentException($"Invalid slice: {slice}");
        }
        
        // Apply v3.0 slice rotation
        ApplySliceRotationMatrix(slice, matrix);
    }
    
    /// <summary>
    /// V3.0 slice rotation - applies rotation to pieces in the specified slice plane
    /// </summary>
    private void ApplySliceRotationMatrix(char slice, int[,] matrix)
    {
        var affectedPieces = GetPiecesOnSlice(slice);
        var updates = new Dictionary<Position3D, CubePiece>();
        
        // First, remove all affected pieces from their current positions
        foreach (var piece in affectedPieces)
        {
            _pieces.Remove(piece.Position);
        }
        
        // Then, rotate and place them in their new positions
        foreach (var piece in affectedPieces)
        {
            var rotatedPiece = RotatePiece(piece, matrix);
            _pieces[rotatedPiece.Position] = rotatedPiece;
        }
    }
    
    /// <summary>
    /// Gets pieces that belong to a specific slice using v3.0 coordinate system
    /// Includes ALL pieces on the slice plane (edges AND centers)
    /// </summary>
    private List<CubePiece> GetPiecesOnSlice(char slice)
    {
        return _pieces.Values.Where(piece =>
        {
            var pos = piece.Position;
            return slice switch
            {
                'M' => pos.X == 0, // Middle slice: pieces with X = 0 (between L and R faces)
                'E' => pos.Y == 0, // Equatorial slice: pieces with Y = 0 (between U and D faces)
                'S' => pos.Z == 0, // Standing slice: pieces with Z = 0 (between F and B faces)
                _ => throw new ArgumentException($"Invalid slice: {slice}")
            };
        }).ToList();
    }
    
    /// <summary>
    /// V3.0 axis rotation - applies rotation to ALL pieces in the cube
    /// </summary>
    private void ApplyAxisRotationMatrix(int[,] matrix)
    {
        var updates = new Dictionary<Position3D, CubePiece>();
        
        // First, remove all pieces from their current positions
        var allPieces = _pieces.Values.ToList();
        _pieces.Clear();
        
        // Then, rotate and place them in their new positions
        foreach (var piece in allPieces)
        {
            var rotatedPiece = RotatePiece(piece, matrix);
            _pieces[rotatedPiece.Position] = rotatedPiece;
        }
    }
    
    /// <summary>
    /// Validates that the cube state is physically possible
    /// </summary>
    public bool IsValidState()
    {
        // Check that we have exactly 26 pieces (8 corners + 12 edges + 6 centers)
        if (_pieces.Count != 26) return false;
        
        // Check that we have 8 corners, 12 edges, and 6 centers
        var corners = _pieces.Values.Count(p => p.Type == PieceType.Corner);
        var edges = _pieces.Values.Count(p => p.Type == PieceType.Edge);
        var centers = _pieces.Values.Count(p => p.Type == PieceType.Center);
        if (corners != 8 || edges != 12 || centers != 6) return false;
        
        // Check that all positions are valid
        foreach (var piece in _pieces.Values)
        {
            if (!IsValidPiecePosition(piece.Position)) return false;
        }
        
        // Check that no two pieces occupy the same position
        var positions = _pieces.Values.Select(p => p.Position).ToList();
        if (positions.Count != positions.Distinct().Count()) return false;
        
        return true;
    }
    
    /// <summary>
    /// Checks if a position is valid for a piece (corner, edge, or center)
    /// </summary>
    private static bool IsValidPiecePosition(Position3D position)
    {
        var coords = new[] { position.X, position.Y, position.Z };
        var nonZeroCount = coords.Count(c => c != 0);
        
        // Corners have 3 non-zero coordinates, edges have 2, centers have 1
        return nonZeroCount == 1 || nonZeroCount == 2 || nonZeroCount == 3;
    }
    
    /// <summary>
    /// Serializes the cube to JSON format for storage and piping
    /// </summary>
    public string ToJson()
    {
        var dto = new CubeDto
        {
            Version = "3.0",
            Pieces = _pieces.Values.Select(p => new PieceDto
            {
                SolvedPosition = new PositionDto { X = p.SolvedPosition.X, Y = p.SolvedPosition.Y, Z = p.SolvedPosition.Z },
                CurrentPosition = new PositionDto { X = p.Position.X, Y = p.Position.Y, Z = p.Position.Z },
                Colors = p.Colors,
                Type = p.Type
            }).ToArray()
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        return JsonSerializer.Serialize(dto, options);
    }

    /// <summary>
    /// Deserializes a cube from JSON format
    /// </summary>
    public static Cube FromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        var dto = JsonSerializer.Deserialize<CubeDto>(json, options);
        if (dto == null)
            throw new ArgumentException("Invalid JSON format");

        if (dto.Version != "3.0" && dto.Version != "2.0")
            throw new ArgumentException($"Unsupported cube format version: {dto.Version}");

        // Recreate pieces dictionary
        var pieces = new Dictionary<Position3D, CubePiece>();
        
        foreach (var pieceDto in dto.Pieces)
        {
            var solvedPos = new Position3D(pieceDto.SolvedPosition.X, pieceDto.SolvedPosition.Y, pieceDto.SolvedPosition.Z);
            var currentPos = new Position3D(pieceDto.CurrentPosition.X, pieceDto.CurrentPosition.Y, pieceDto.CurrentPosition.Z);
            
            // Use the Colors array directly (already CubeColor?[])
            var colorsArray = pieceDto.Colors;
            var piece = new CubePiece(solvedPos, colorsArray).MoveTo(currentPos);
            pieces[currentPos] = piece;
        }

        return new Cube(pieces);
    }

    public override string ToString()
    {
        return $"Cube (Pieces: {_pieces.Count}, Solved: {IsSolved})";
    }

    // DTO classes for JSON serialization
    private class CubeDto
    {
        public string Version { get; set; } = "";
        public PieceDto[] Pieces { get; set; } = Array.Empty<PieceDto>();
    }

    private class PieceDto
    {
        public PositionDto SolvedPosition { get; set; } = new();
        public PositionDto CurrentPosition { get; set; } = new();
        public CubeColor?[] Colors { get; set; } = Array.Empty<CubeColor?>();
        public PieceType Type { get; set; }
    }

    private class PositionDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }
}