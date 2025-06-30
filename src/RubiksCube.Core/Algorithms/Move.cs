namespace RubiksCube.Core.Algorithms;

/// <summary>
/// Represents the direction of a move
/// </summary>
public enum MoveDirection
{
    /// <summary>
    /// Clockwise rotation (default, no suffix)
    /// </summary>
    Clockwise,
    
    /// <summary>
    /// Counter-clockwise rotation (prime, ' suffix)
    /// </summary>
    CounterClockwise,
    
    /// <summary>
    /// 180-degree rotation (2 suffix)
    /// </summary>
    Double
}

/// <summary>
/// Represents the direction of a face rotation (for internal use)
/// </summary>
public enum RotationDirection
{
    /// <summary>
    /// Clockwise rotation when looking at the face
    /// </summary>
    Clockwise,
    
    /// <summary>
    /// Counter-clockwise rotation when looking at the face
    /// </summary>
    CounterClockwise
}

/// <summary>
/// Represents a single move in an algorithm
/// </summary>
public class Move : IEquatable<Move>
{
    /// <summary>
    /// The face or axis being moved (R, L, U, D, F, B, x, y, z)
    /// </summary>
    public char Face { get; }
    
    /// <summary>
    /// The direction of the move (clockwise, counter-clockwise, double)
    /// </summary>
    public MoveDirection Direction { get; }
    
    /// <summary>
    /// True if this is a wide move (lowercase letter, affects 2 layers)
    /// </summary>
    public bool IsWide { get; }
    
    /// <summary>
    /// Creates a new move
    /// </summary>
    public Move(char face, MoveDirection direction = MoveDirection.Clockwise)
    {
        Face = face; // Preserve case for Singmaster notation
        Direction = direction;
        IsWide = DetermineIfWide(Face);
        
        ValidateMove();
    }
    
    /// <summary>
    /// Determines if this is a wide move (lowercase face letter)
    /// </summary>
    private static bool DetermineIfWide(char face)
    {
        return face switch
        {
            'r' or 'l' or 'u' or 'd' or 'f' or 'b' => true,
            _ => false
        };
    }
    
    /// <summary>
    /// Validates that the move is well-formed
    /// </summary>
    private void ValidateMove()
    {
        var validFaces = new[] { 'R', 'L', 'U', 'D', 'F', 'B', 'r', 'l', 'u', 'd', 'f', 'b', 'x', 'y', 'z', 'M', 'E', 'S' };
        
        if (!validFaces.Contains(Face))
        {
            throw new ArgumentException($"Invalid move face: {Face}. Valid faces: {string.Join(", ", validFaces)}");
        }
    }
    
    /// <summary>
    /// Gets the inverse of this move
    /// </summary>
    public Move Inverse()
    {
        var inverseDirection = Direction switch
        {
            MoveDirection.Clockwise => MoveDirection.CounterClockwise,
            MoveDirection.CounterClockwise => MoveDirection.Clockwise,
            MoveDirection.Double => MoveDirection.Double, // Double moves are self-inverse
            _ => throw new InvalidOperationException($"Unknown direction: {Direction}")
        };
        
        return new Move(Face, inverseDirection);
    }
    
    /// <summary>
    /// Converts the move back to Singmaster notation
    /// </summary>
    public override string ToString()
    {
        var suffix = Direction switch
        {
            MoveDirection.Clockwise => "",
            MoveDirection.CounterClockwise => "'",
            MoveDirection.Double => "2",
            _ => throw new InvalidOperationException($"Unknown direction: {Direction}")
        };
        
        return $"{Face}{suffix}";
    }
    
    /// <summary>
    /// Parses a single move from Singmaster notation
    /// </summary>
    public static Move Parse(string moveString)
    {
        if (string.IsNullOrWhiteSpace(moveString))
            throw new ArgumentException("Move string cannot be null or empty");
            
        moveString = moveString.Trim();
        
        if (moveString.Length == 0)
            throw new ArgumentException("Move string cannot be empty");
            
        char face = moveString[0]; // Preserve case for Singmaster notation
        var direction = MoveDirection.Clockwise;
        
        // Parse suffix for direction
        if (moveString.Length > 1)
        {
            var suffix = moveString.Substring(1);
            direction = suffix switch
            {
                "'" => MoveDirection.CounterClockwise,
                "2" => MoveDirection.Double,
                "" => MoveDirection.Clockwise,
                _ => throw new ArgumentException($"Invalid move suffix: '{suffix}'. Valid suffixes: ', 2, or none")
            };
        }
        
        return new Move(face, direction);
    }
    
    public bool Equals(Move? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Face == other.Face && Direction == other.Direction;
    }
    
    public override bool Equals(object? obj) => obj is Move other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(Face, Direction);
    
    public static bool operator ==(Move? left, Move? right) => 
        ReferenceEquals(left, right) || (left?.Equals(right) ?? false);
        
    public static bool operator !=(Move? left, Move? right) => !(left == right);
}