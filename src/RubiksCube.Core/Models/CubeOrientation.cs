namespace RubiksCube.Core.Models;

/// <summary>
/// Defines the orientation of the cube for viewing and algorithm application.
/// Only tracks Front and Up faces - all other faces are derived from these two.
/// </summary>
public class CubeOrientation : IEquatable<CubeOrientation>
{
    /// <summary>
    /// The color currently facing forward (toward the viewer)
    /// </summary>
    public CubeColor Front { get; private set; }
    
    /// <summary>
    /// The color currently facing upward
    /// </summary>
    public CubeColor Up { get; private set; }
    
    /// <summary>
    /// Standard solved orientation: Green front, Yellow up (White bottom implied)
    /// Western/BOY scheme: When Green front + Yellow up, then Orange right, Red left, Blue back, White bottom
    /// </summary>
    public static CubeOrientation Standard => new(CubeColor.Green, CubeColor.Yellow);
    
    public CubeOrientation(CubeColor front, CubeColor up)
    {
        if (!IsValidOrientation(front, up))
            throw new ArgumentException($"Invalid orientation: {front} front, {up} up. Faces must be adjacent.");
            
        Front = front;
        Up = up;
    }
    
    /// <summary>
    /// Validates that two faces can be adjacent (not opposite each other)
    /// </summary>
    private static bool IsValidOrientation(CubeColor front, CubeColor up)
    {
        // Check if the faces are opposite each other (invalid)
        var oppositePairs = new[]
        {
            (CubeColor.White, CubeColor.Yellow),
            (CubeColor.Green, CubeColor.Blue),
            (CubeColor.Red, CubeColor.Orange)
        };
        
        return !oppositePairs.Any(pair => 
            (front == pair.Item1 && up == pair.Item2) || 
            (front == pair.Item2 && up == pair.Item1));
    }
    
    /// <summary>
    /// Gets the color that would be on the right side for this orientation
    /// </summary>
    public CubeColor Right => GetRightColor(Front, Up);
    
    /// <summary>
    /// Gets the color that would be on the left side for this orientation
    /// </summary>
    public CubeColor Left => GetOpposite(Right);
    
    /// <summary>
    /// Gets the color that would be on the back for this orientation
    /// </summary>
    public CubeColor Back => GetOpposite(Front);
    
    /// <summary>
    /// Gets the color that would be on the bottom for this orientation
    /// </summary>
    public CubeColor Bottom => GetOpposite(Up);
    
    /// <summary>
    /// Returns the opposite color for any given color
    /// </summary>
    private static CubeColor GetOpposite(CubeColor color) => color switch
    {
        CubeColor.White => CubeColor.Yellow,
        CubeColor.Yellow => CubeColor.White,
        CubeColor.Green => CubeColor.Blue,
        CubeColor.Blue => CubeColor.Green,
        CubeColor.Red => CubeColor.Orange,
        CubeColor.Orange => CubeColor.Red,
        _ => throw new ArgumentException($"Unknown color: {color}")
    };
    
    /// <summary>
    /// Calculates the right-side color given front and up colors
    /// Uses right-hand rule: if you point your right hand toward front with fingers up, thumb points right
    /// </summary>
    private static CubeColor GetRightColor(CubeColor front, CubeColor up)
    {
        return (front, up) switch
        {
            // Green front orientations
            (CubeColor.Green, CubeColor.Yellow) => CubeColor.Orange,
            (CubeColor.Green, CubeColor.White) => CubeColor.Red,
            (CubeColor.Green, CubeColor.Red) => CubeColor.Yellow,
            (CubeColor.Green, CubeColor.Orange) => CubeColor.White,
            
            // Blue front orientations  
            (CubeColor.Blue, CubeColor.Yellow) => CubeColor.Red,
            (CubeColor.Blue, CubeColor.White) => CubeColor.Orange,
            (CubeColor.Blue, CubeColor.Red) => CubeColor.White,
            (CubeColor.Blue, CubeColor.Orange) => CubeColor.Yellow,
            
            // Red front orientations
            (CubeColor.Red, CubeColor.Yellow) => CubeColor.Green,
            (CubeColor.Red, CubeColor.White) => CubeColor.Green,
            (CubeColor.Red, CubeColor.Green) => CubeColor.Yellow,
            (CubeColor.Red, CubeColor.Blue) => CubeColor.White,
            
            // Orange front orientations
            (CubeColor.Orange, CubeColor.Yellow) => CubeColor.Blue,
            (CubeColor.Orange, CubeColor.White) => CubeColor.Blue,
            (CubeColor.Orange, CubeColor.Green) => CubeColor.White,
            (CubeColor.Orange, CubeColor.Blue) => CubeColor.Yellow,
            
            // Yellow front orientations
            (CubeColor.Yellow, CubeColor.Green) => CubeColor.Orange,
            (CubeColor.Yellow, CubeColor.Blue) => CubeColor.Red,
            (CubeColor.Yellow, CubeColor.Red) => CubeColor.Green,
            (CubeColor.Yellow, CubeColor.Orange) => CubeColor.Blue,
            
            // White front orientations
            (CubeColor.White, CubeColor.Green) => CubeColor.Red,
            (CubeColor.White, CubeColor.Blue) => CubeColor.Orange,
            (CubeColor.White, CubeColor.Red) => CubeColor.Blue,
            (CubeColor.White, CubeColor.Orange) => CubeColor.Green,
            
            _ => throw new ArgumentException($"Invalid orientation: {front} front, {up} up")
        };
    }
    
    /// <summary>
    /// Applies an X rotation (around the right-left axis)
    /// X: Front becomes Up, Up becomes Back, Back becomes Down, Down becomes Front
    /// </summary>
    public CubeOrientation ApplyXRotation(bool clockwise = true)
    {
        var newFront = clockwise ? Bottom : Up;
        var newUp = clockwise ? Front : Back;
        return new CubeOrientation(newFront, newUp);
    }
    
    /// <summary>
    /// Applies a Y rotation (around the up-down axis)  
    /// Y clockwise: Front becomes Right, Right becomes Back, Back becomes Left, Left becomes Front
    /// Y': Front becomes Left, Left becomes Back, Back becomes Right, Right becomes Front
    /// </summary>
    public CubeOrientation ApplyYRotation(bool clockwise = true)
    {
        var newFront = clockwise ? Right : Left;
        var newUp = Up; // Up stays the same for Y rotations
        return new CubeOrientation(newFront, newUp);
    }
    
    /// <summary>
    /// Applies a Z rotation (around the front-back axis)
    /// Z: Up becomes Right, Right becomes Down, Down becomes Left, Left becomes Up
    /// </summary>
    public CubeOrientation ApplyZRotation(bool clockwise = true)
    {
        var newFront = Front; // Front stays the same for Z rotations
        var newUp = clockwise ? Left : Right;
        return new CubeOrientation(newFront, newUp);
    }
    
    /// <summary>
    /// Gets the 3D coordinate direction vector for a given face color in this orientation
    /// </summary>
    public Position3D GetDirectionForFace(CubeColor face)
    {
        if (face == Front) return new Position3D(0, 0, 1);   // Front: +Z
        if (face == Back) return new Position3D(0, 0, -1);   // Back: -Z
        if (face == Up) return new Position3D(0, 1, 0);      // Up: +Y
        if (face == Bottom) return new Position3D(0, -1, 0); // Bottom: -Y
        if (face == Right) return new Position3D(1, 0, 0);   // Right: +X
        if (face == Left) return new Position3D(-1, 0, 0);   // Left: -X
        
        throw new ArgumentException($"Unknown face: {face}");
    }
    
    /// <summary>
    /// Gets the face color for a given 3D coordinate direction in this orientation
    /// </summary>
    public CubeColor GetFaceForDirection(Position3D direction)
    {
        return direction switch
        {
            { X: 1, Y: 0, Z: 0 } => Right,
            { X: -1, Y: 0, Z: 0 } => Left,
            { X: 0, Y: 1, Z: 0 } => Up,
            { X: 0, Y: -1, Z: 0 } => Bottom,
            { X: 0, Y: 0, Z: 1 } => Front,
            { X: 0, Y: 0, Z: -1 } => Back,
            _ => throw new ArgumentException($"Invalid direction: {direction}")
        };
    }
    
    public bool Equals(CubeOrientation? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Front == other.Front && Up == other.Up;
    }
    
    public override bool Equals(object? obj) => obj is CubeOrientation other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Front, Up);
    public override string ToString() => $"Front: {Front}, Up: {Up}";
    
    public static bool operator ==(CubeOrientation? left, CubeOrientation? right) => 
        ReferenceEquals(left, right) || (left?.Equals(right) ?? false);
        
    public static bool operator !=(CubeOrientation? left, CubeOrientation? right) => 
        !(left == right);
}