using System.Text.Json.Serialization;

namespace RubiksCube.Core.Models;

/// <summary>
/// Represents the type of piece on a Rubik's cube
/// </summary>
public enum PieceType
{
    Corner,
    Edge,
    Center
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
    public Position3D Position { get; set; }
    
    /// <summary>
    /// Type of piece (corner, edge, or center)
    /// </summary>
    public PieceType Type { get; }
    
    /// <summary>
    /// Colors on this piece as 3-element array [X, Y, Z] with nulls for missing axes.
    /// This matches the Python pglass/cube implementation exactly.
    /// </summary>
    public CubeColor?[] Colors { get; set; }
    
    /// <summary>
    /// The original/solved position of this piece (immutable)
    /// </summary>
    public Position3D SolvedPosition { get; }

    public CubePiece(Position3D solvedPosition, CubeColor?[] colors)
    {
        SolvedPosition = solvedPosition;
        Position = solvedPosition;
        
        if (colors.Length != 3)
            throw new ArgumentException("Colors array must have exactly 3 elements [X, Y, Z]");
        
        // Validate piece type based on position
        var nonZeroCoords = new[] { solvedPosition.X, solvedPosition.Y, solvedPosition.Z }
            .Count(coord => coord != 0);
            
        Type = nonZeroCoords switch
        {
            3 => PieceType.Corner,
            2 => PieceType.Edge,
            1 => PieceType.Center,
            _ => throw new ArgumentException($"Invalid piece position: {solvedPosition}. Must be corner (3 non-zero), edge (2 non-zero), or center (1 non-zero)")
        };
        
        // Validate color count matches piece type (count non-null colors)
        var nonNullColors = colors.Count(c => c != null);
        var expectedColors = Type switch
        {
            PieceType.Corner => 3,
            PieceType.Edge => 2,
            PieceType.Center => 1,
            _ => throw new ArgumentException($"Unknown piece type: {Type}")
        };
        if (nonNullColors != expectedColors)
            throw new ArgumentException($"{Type} piece must have exactly {expectedColors} non-null colors");
            
        Colors = (CubeColor?[])colors.Clone();
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
    /// Swaps colors between two axes (Python-style rotation)
    /// </summary>
    public void SwapColors(int axis1, int axis2)
    {
        if (axis1 < 0 || axis1 > 2 || axis2 < 0 || axis2 > 2)
            throw new ArgumentException("Axis indices must be 0, 1, or 2");
            
        (Colors[axis1], Colors[axis2]) = (Colors[axis2], Colors[axis1]);
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
    /// Gets the expected colors for a piece at the given position as 3-element array
    /// </summary>
    private CubeColor?[] GetExpectedColorsForPosition(Position3D pos)
    {
        var colors = new CubeColor?[3];
        
        // Map coordinates to faces based on Western/BOY scheme
        if (pos.X == -1) colors[0] = CubeColor.Red;      // Left
        if (pos.X == 1) colors[0] = CubeColor.Orange;    // Right
        if (pos.Y == -1) colors[1] = CubeColor.White;    // Bottom
        if (pos.Y == 1) colors[1] = CubeColor.Yellow;    // Top
        if (pos.Z == -1) colors[2] = CubeColor.Blue;     // Back
        if (pos.Z == 1) colors[2] = CubeColor.Green;     // Front
        
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