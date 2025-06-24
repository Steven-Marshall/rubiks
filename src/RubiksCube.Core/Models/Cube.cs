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
    /// Current viewing orientation of the cube (front and up faces)
    /// </summary>
    public CubeOrientation Orientation { get; private set; }
    
    /// <summary>
    /// All pieces on the cube (8 corners + 12 edges)
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
        Orientation = CubeOrientation.Standard;
        
        InitializeSolvedPieces();
    }
    
    /// <summary>
    /// Copy constructor for creating cube copies
    /// </summary>
    private Cube(Dictionary<Position3D, CubePiece> pieces, CubeOrientation orientation)
    {
        _pieces = new Dictionary<Position3D, CubePiece>(pieces);
        Orientation = orientation;
    }
    
    /// <summary>
    /// Initializes all 20 pieces in their solved positions with correct colors
    /// </summary>
    private void InitializeSolvedPieces()
    {
        // 8 Corner pieces (3 colors each)
        var corners = new[]
        {
            // Top corners (Y = 1)
            (new Position3D(-1, 1, 1), new[] { CubeColor.Red, CubeColor.Yellow, CubeColor.Green }),    // Top-Left-Front
            (new Position3D(1, 1, 1), new[] { CubeColor.Orange, CubeColor.Yellow, CubeColor.Green }),  // Top-Right-Front  
            (new Position3D(-1, 1, -1), new[] { CubeColor.Red, CubeColor.Yellow, CubeColor.Blue }),    // Top-Left-Back
            (new Position3D(1, 1, -1), new[] { CubeColor.Orange, CubeColor.Yellow, CubeColor.Blue }),  // Top-Right-Back
            
            // Bottom corners (Y = -1)
            (new Position3D(-1, -1, 1), new[] { CubeColor.Red, CubeColor.White, CubeColor.Green }),    // Bottom-Left-Front
            (new Position3D(1, -1, 1), new[] { CubeColor.Orange, CubeColor.White, CubeColor.Green }),  // Bottom-Right-Front
            (new Position3D(-1, -1, -1), new[] { CubeColor.Red, CubeColor.White, CubeColor.Blue }),    // Bottom-Left-Back  
            (new Position3D(1, -1, -1), new[] { CubeColor.Orange, CubeColor.White, CubeColor.Blue })   // Bottom-Right-Back
        };
        
        // 12 Edge pieces (2 colors each)
        var edges = new[]
        {
            // Top edges (Y = 1)
            (new Position3D(0, 1, 1), new[] { CubeColor.Yellow, CubeColor.Green }),    // Top-Front
            (new Position3D(0, 1, -1), new[] { CubeColor.Yellow, CubeColor.Blue }),    // Top-Back
            (new Position3D(-1, 1, 0), new[] { CubeColor.Yellow, CubeColor.Red }),     // Top-Left
            (new Position3D(1, 1, 0), new[] { CubeColor.Yellow, CubeColor.Orange }),   // Top-Right
            
            // Middle edges (Y = 0)
            (new Position3D(-1, 0, 1), new[] { CubeColor.Red, CubeColor.Green }),      // Middle-Left-Front
            (new Position3D(1, 0, 1), new[] { CubeColor.Orange, CubeColor.Green }),    // Middle-Right-Front
            (new Position3D(-1, 0, -1), new[] { CubeColor.Red, CubeColor.Blue }),      // Middle-Left-Back
            (new Position3D(1, 0, -1), new[] { CubeColor.Orange, CubeColor.Blue }),    // Middle-Right-Back
            
            // Bottom edges (Y = -1)
            (new Position3D(0, -1, 1), new[] { CubeColor.White, CubeColor.Green }),    // Bottom-Front
            (new Position3D(0, -1, -1), new[] { CubeColor.White, CubeColor.Blue }),    // Bottom-Back
            (new Position3D(-1, -1, 0), new[] { CubeColor.White, CubeColor.Red }),     // Bottom-Left
            (new Position3D(1, -1, 0), new[] { CubeColor.White, CubeColor.Orange })    // Bottom-Right
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
    public bool IsSolved => _pieces.Values.All(p => p.IsSolved) && Orientation == CubeOrientation.Standard;
    
    /// <summary>
    /// Creates a deep copy of this cube
    /// </summary>
    public Cube Clone()
    {
        var clonedPieces = _pieces.ToDictionary(
            kvp => kvp.Key,
            kvp => new CubePiece(kvp.Value.SolvedPosition, kvp.Value.Colors).MoveTo(kvp.Value.Position)
        );
        
        return new Cube(clonedPieces, new CubeOrientation(Orientation.Front, Orientation.Up));
    }
    
    /// <summary>
    /// Applies a cube rotation (x, y, z) changing the viewing orientation
    /// </summary>
    public void ApplyRotation(char axis, bool clockwise = true)
    {
        switch (char.ToLower(axis))
        {
            case 'x':
                Orientation = Orientation.ApplyXRotation(clockwise);
                break;
            case 'y':
                Orientation = Orientation.ApplyYRotation(clockwise);
                break;
            case 'z':
                Orientation = Orientation.ApplyZRotation(clockwise);
                break;
            default:
                throw new ArgumentException($"Invalid rotation axis: {axis}. Must be x, y, or z.");
        }
    }
    
    /// <summary>
    /// Applies a reorientation move (x, y, z) to the cube
    /// </summary>
    public void ApplyReorientation(Move move)
    {
        if (move.Type != MoveType.Reorientation)
            throw new ArgumentException($"Move {move} is not a reorientation. Use ApplyMove for rotations.");
            
        var clockwise = move.Direction switch
        {
            MoveDirection.Clockwise => true,
            MoveDirection.CounterClockwise => false,
            MoveDirection.Double => true, // Apply twice below
            _ => throw new InvalidOperationException($"Unknown direction: {move.Direction}")
        };
        
        // Apply the rotation
        ApplyRotation(move.Face, clockwise);
        
        // For double moves, apply again
        if (move.Direction == MoveDirection.Double)
        {
            ApplyRotation(move.Face, clockwise);
        }
    }
    
    /// <summary>
    /// Applies a face turn (R, L, U, D, F, B) by rotating pieces around that face
    /// </summary>
    public void ApplyFaceMove(char face, bool clockwise = true, int turns = 1)
    {
        // Normalize turns to 0-3 range
        turns = ((turns % 4) + 4) % 4;
        if (turns == 0) return;
        
        // Apply the move the specified number of times
        for (int i = 0; i < turns; i++)
        {
            ApplySingleFaceMove(face, clockwise);
        }
    }
    
    /// <summary>
    /// Applies a single 90-degree face turn
    /// </summary>
    private void ApplySingleFaceMove(char face, bool clockwise)
    {
        // Get the face direction in current orientation
        var faceColor = GetFaceColorFromNotation(face);
        var faceDirection = Orientation.GetDirectionForFace(faceColor);
        
        // Find all pieces that touch this face
        var affectedPieces = _pieces.Values
            .Where(piece => TouchesFace(piece.Position, faceDirection))
            .ToList();
        
        // Create rotation matrix for 90-degree turn around the face axis
        var rotationMatrix = CreateRotationMatrix(faceDirection, clockwise);
        
        // Apply rotation to each piece
        var newPositions = new Dictionary<Position3D, CubePiece>();
        
        foreach (var piece in affectedPieces)
        {
            // Calculate new position
            var newPosition = ApplyRotationMatrix(piece.Position, rotationMatrix);
            
            // Calculate color rotation based on the piece type and axis
            var colorRotationSteps = CalculateColorRotation(piece, faceDirection, clockwise);
            var rotatedPiece = piece.RotateColors(colorRotationSteps).MoveTo(newPosition);
            
            newPositions[newPosition] = rotatedPiece;
        }
        
        // Update the cube with new positions
        foreach (var (position, piece) in newPositions)
        {
            _pieces[position] = piece;
        }
    }
    
    /// <summary>
    /// Converts face notation (R, L, U, D, F, B) to color based on current orientation
    /// </summary>
    private CubeColor GetFaceColorFromNotation(char face)
    {
        return char.ToUpper(face) switch
        {
            'R' => Orientation.Right,
            'L' => Orientation.Left,
            'U' => Orientation.Up,
            'D' => Orientation.Bottom,
            'F' => Orientation.Front,
            'B' => Orientation.Back,
            _ => throw new ArgumentException($"Invalid face notation: {face}")
        };
    }
    
    /// <summary>
    /// Checks if a piece at the given position touches the specified face
    /// </summary>
    private static bool TouchesFace(Position3D position, Position3D faceDirection)
    {
        // A piece touches a face if it has the same coordinate as the face direction
        // and that coordinate is at the extreme (+1 or -1)
        return (faceDirection.X != 0 && position.X == faceDirection.X) ||
               (faceDirection.Y != 0 && position.Y == faceDirection.Y) ||
               (faceDirection.Z != 0 && position.Z == faceDirection.Z);
    }
    
    /// <summary>
    /// Creates a rotation matrix for 90-degree rotation around the given axis
    /// </summary>
    private static int[,] CreateRotationMatrix(Position3D axis, bool clockwise)
    {
        return RotationMatrix.CreateRotationAroundAxis(axis, clockwise);
    }
    
    /// <summary>
    /// Applies rotation matrix to a position vector
    /// </summary>
    private static Position3D ApplyRotationMatrix(Position3D position, int[,] matrix)
    {
        return RotationMatrix.Apply(matrix, position);
    }
    
    /// <summary>
    /// Calculates how many steps to rotate piece colors based on move
    /// </summary>
    private static int CalculateColorRotation(CubePiece piece, Position3D faceDirection, bool clockwise)
    {
        // For now, return 0 (no color rotation) - this is a simplification
        // In a full implementation, this would calculate the proper color rotation
        // based on how the piece's orientation changes relative to the fixed coordinate system
        return 0;
    }
    
    /// <summary>
    /// Validates that the cube state is physically possible
    /// </summary>
    public bool IsValidState()
    {
        // Check that we have exactly 20 pieces
        if (_pieces.Count != 20) return false;
        
        // Check that we have 8 corners and 12 edges
        var corners = _pieces.Values.Count(p => p.Type == PieceType.Corner);
        var edges = _pieces.Values.Count(p => p.Type == PieceType.Edge);
        if (corners != 8 || edges != 12) return false;
        
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
    /// Checks if a position is valid for a piece (corner or edge)
    /// </summary>
    private static bool IsValidPiecePosition(Position3D position)
    {
        var coords = new[] { position.X, position.Y, position.Z };
        var nonZeroCount = coords.Count(c => c != 0);
        
        // Corners have 3 non-zero coordinates, edges have 2
        return nonZeroCount == 2 || nonZeroCount == 3;
    }
    
    /// <summary>
    /// Serializes the cube to JSON format for storage and piping
    /// </summary>
    public string ToJson()
    {
        var dto = new CubeDto
        {
            Version = "2.0",
            Orientation = new OrientationDto 
            { 
                Front = Orientation.Front, 
                Up = Orientation.Up 
            },
            Pieces = _pieces.Values.Select(p => new PieceDto
            {
                SolvedPosition = new PositionDto { X = p.SolvedPosition.X, Y = p.SolvedPosition.Y, Z = p.SolvedPosition.Z },
                CurrentPosition = new PositionDto { X = p.Position.X, Y = p.Position.Y, Z = p.Position.Z },
                Colors = p.Colors.ToArray(),
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

        if (dto.Version != "2.0")
            throw new ArgumentException($"Unsupported cube format version: {dto.Version}");

        // Recreate pieces dictionary
        var pieces = new Dictionary<Position3D, CubePiece>();
        
        foreach (var pieceDto in dto.Pieces)
        {
            var solvedPos = new Position3D(pieceDto.SolvedPosition.X, pieceDto.SolvedPosition.Y, pieceDto.SolvedPosition.Z);
            var currentPos = new Position3D(pieceDto.CurrentPosition.X, pieceDto.CurrentPosition.Y, pieceDto.CurrentPosition.Z);
            
            var piece = new CubePiece(solvedPos, pieceDto.Colors).MoveTo(currentPos);
            pieces[currentPos] = piece;
        }

        // Recreate orientation
        var orientation = new CubeOrientation(dto.Orientation.Front, dto.Orientation.Up);

        return new Cube(pieces, orientation);
    }

    public override string ToString()
    {
        return $"Cube (Orientation: {Orientation}, Pieces: {_pieces.Count}, Solved: {IsSolved})";
    }

    // DTO classes for JSON serialization
    private class CubeDto
    {
        public string Version { get; set; } = "";
        public OrientationDto Orientation { get; set; } = new();
        public PieceDto[] Pieces { get; set; } = Array.Empty<PieceDto>();
    }

    private class OrientationDto
    {
        public CubeColor Front { get; set; }
        public CubeColor Up { get; set; }
    }

    private class PieceDto
    {
        public PositionDto SolvedPosition { get; set; } = new();
        public PositionDto CurrentPosition { get; set; } = new();
        public CubeColor[] Colors { get; set; } = Array.Empty<CubeColor>();
        public PieceType Type { get; set; }
    }

    private class PositionDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }
}