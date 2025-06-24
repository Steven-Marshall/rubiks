using System.Text.Json.Serialization;

namespace RubiksCube.Core.Models;

/// <summary>
/// Represents the type of piece on a Rubik's cube
/// </summary>
public enum PieceType
{
    Corner,
    Edge
}

/// <summary>
/// Represents a position vector in 3D space using cube coordinates
/// </summary>
public readonly struct Position3D : IEquatable<Position3D>
{
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
    
    public Position3D(int x, int y, int z)
    {
        if (Math.Abs(x) > 1 || Math.Abs(y) > 1 || Math.Abs(z) > 1)
            throw new ArgumentException("Position coordinates must be in range [-1, 0, 1]");
            
        X = x;
        Y = y;
        Z = z;
    }
    
    public bool Equals(Position3D other) => X == other.X && Y == other.Y && Z == other.Z;
    public override bool Equals(object? obj) => obj is Position3D other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public override string ToString() => $"({X}, {Y}, {Z})";
    
    public static bool operator ==(Position3D left, Position3D right) => left.Equals(right);
    public static bool operator !=(Position3D left, Position3D right) => !left.Equals(right);
}

/// <summary>
/// Represents a color on the cube using the Western/BOY color scheme
/// </summary>
public enum CubeColor
{
    White = 0,   // Bottom
    Yellow = 1,  // Top
    Green = 2,   // Front
    Blue = 3,    // Back
    Red = 4,     // Left
    Orange = 5   // Right
}

/// <summary>
/// Represents a single piece (corner or edge) on a Rubik's cube with its position and colors
/// </summary>
public class CubePiece : IEquatable<CubePiece>
{
    /// <summary>
    /// Current position of this piece in 3D space
    /// </summary>
    public Position3D Position { get; private set; }
    
    /// <summary>
    /// Type of piece (corner or edge)
    /// </summary>
    public PieceType Type { get; }
    
    /// <summary>
    /// Colors on this piece. Corners have 3 colors, edges have 2.
    /// Order represents the orientation of the piece.
    /// </summary>
    public IReadOnlyList<CubeColor> Colors { get; }
    
    /// <summary>
    /// The original/solved position of this piece (immutable)
    /// </summary>
    public Position3D SolvedPosition { get; }

    public CubePiece(Position3D solvedPosition, IEnumerable<CubeColor> colors)
    {
        SolvedPosition = solvedPosition;
        Position = solvedPosition;
        
        var colorList = colors.ToList();
        
        // Validate piece type based on position
        var nonZeroCoords = new[] { solvedPosition.X, solvedPosition.Y, solvedPosition.Z }
            .Count(coord => coord != 0);
            
        Type = nonZeroCoords switch
        {
            3 => PieceType.Corner,
            2 => PieceType.Edge,
            _ => throw new ArgumentException($"Invalid piece position: {solvedPosition}. Must be corner (3 non-zero) or edge (2 non-zero)")
        };
        
        // Validate color count matches piece type
        var expectedColors = Type == PieceType.Corner ? 3 : 2;
        if (colorList.Count != expectedColors)
            throw new ArgumentException($"{Type} piece must have exactly {expectedColors} colors");
            
        Colors = colorList.AsReadOnly();
    }
    
    /// <summary>
    /// Creates a new piece at the specified position with the same colors and orientation
    /// </summary>
    public CubePiece MoveTo(Position3D newPosition)
    {
        var newPiece = new CubePiece(SolvedPosition, Colors)
        {
            Position = newPosition
        };
        return newPiece;
    }
    
    /// <summary>
    /// Creates a new piece with rotated color orientation (for 90-degree rotations)
    /// </summary>
    public CubePiece RotateColors(int steps = 1)
    {
        if (steps == 0) return this;
        
        var rotatedColors = new List<CubeColor>(Colors);
        var actualSteps = ((steps % Colors.Count) + Colors.Count) % Colors.Count;
        
        for (int i = 0; i < actualSteps; i++)
        {
            var first = rotatedColors[0];
            rotatedColors.RemoveAt(0);
            rotatedColors.Add(first);
        }
        
        var newPiece = new CubePiece(SolvedPosition, rotatedColors)
        {
            Position = Position
        };
        return newPiece;
    }
    
    /// <summary>
    /// Checks if this piece is in its solved position and orientation
    /// </summary>
    public bool IsSolved => Position == SolvedPosition && IsCorrectOrientation;
    
    /// <summary>
    /// Checks if the piece has correct color orientation for its current position
    /// </summary>
    public bool IsCorrectOrientation
    {
        get
        {
            // For a piece to be correctly oriented, its colors must match
            // the expected colors for its current position
            var expectedColors = GetExpectedColorsForPosition(Position);
            return Colors.SequenceEqual(expectedColors);
        }
    }
    
    /// <summary>
    /// Gets the expected colors for a piece at the given position
    /// </summary>
    private IEnumerable<CubeColor> GetExpectedColorsForPosition(Position3D pos)
    {
        var colors = new List<CubeColor>();
        
        // Map coordinates to faces based on Western/BOY scheme
        if (pos.X == -1) colors.Add(CubeColor.Red);    // Left
        if (pos.X == 1) colors.Add(CubeColor.Orange);   // Right
        if (pos.Y == -1) colors.Add(CubeColor.White);   // Bottom
        if (pos.Y == 1) colors.Add(CubeColor.Yellow);   // Top
        if (pos.Z == -1) colors.Add(CubeColor.Blue);    // Back
        if (pos.Z == 1) colors.Add(CubeColor.Green);    // Front
        
        return colors;
    }
    
    public bool Equals(CubePiece? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return Position == other.Position && 
               SolvedPosition == other.SolvedPosition && 
               Colors.SequenceEqual(other.Colors);
    }
    
    public override bool Equals(object? obj) => obj is CubePiece other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(Position, SolvedPosition, 
        Colors.Aggregate(0, (acc, color) => HashCode.Combine(acc, color)));
    
    public override string ToString() => 
        $"{Type} at {Position} (solved: {SolvedPosition}) colors: [{string.Join(", ", Colors)}]";
}