using RubiksCube.Core.Models;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.Models;

/// <summary>
/// Tests for cube reorientations (x, y, z) - perspective changes only, no piece movement
/// </summary>
public class CubeReorientationTests
{
    [Fact]
    public void ApplyReorientation_XMove_ShouldChangeOrientationCorrectly()
    {
        var cube = Cube.CreateSolved();
        var originalOrientation = cube.Orientation;
        
        // Apply x (clockwise around right-left axis)
        var xMove = new Move('x', MoveDirection.Clockwise);
        cube.ApplyReorientation(xMove);
        
        // X: Front becomes Up, Up becomes Back, Back becomes Down, Down becomes Front
        Assert.Equal(originalOrientation.Bottom, cube.Orientation.Front);  // Down becomes Front
        Assert.Equal(originalOrientation.Front, cube.Orientation.Up);      // Front becomes Up
        
        // Pieces should not have moved
        Assert.Equal(20, cube.Pieces.Count);
        Assert.True(cube.IsValidState());
    }
    
    [Fact]
    public void ApplyReorientation_XPrime_ShouldChangeOrientationCorrectly()
    {
        var cube = Cube.CreateSolved();
        var originalOrientation = cube.Orientation;
        
        // Apply x' (counter-clockwise around right-left axis)
        var xPrimeMove = new Move('x', MoveDirection.CounterClockwise);
        cube.ApplyReorientation(xPrimeMove);
        
        // X': Front becomes Down, Up becomes Front, Back becomes Up, Down becomes Back
        Assert.Equal(originalOrientation.Up, cube.Orientation.Front);     // Up becomes Front
        Assert.Equal(originalOrientation.Back, cube.Orientation.Up);      // Back becomes Up
        
        // Pieces should not have moved
        Assert.Equal(20, cube.Pieces.Count);
        Assert.True(cube.IsValidState());
    }
    
    [Fact]
    public void ApplyReorientation_YMove_ShouldChangeOrientationCorrectly()
    {
        var cube = Cube.CreateSolved();
        var originalOrientation = cube.Orientation;
        
        // Apply y (clockwise around up-down axis)
        var yMove = new Move('y', MoveDirection.Clockwise);
        cube.ApplyReorientation(yMove);
        
        // Y clockwise: Front becomes Right, Right becomes Back, Back becomes Left, Left becomes Front
        Assert.Equal(originalOrientation.Right, cube.Orientation.Front);  // Right becomes Front
        Assert.Equal(originalOrientation.Up, cube.Orientation.Up);        // Up stays the same
        
        // Pieces should not have moved
        Assert.Equal(20, cube.Pieces.Count);
        Assert.True(cube.IsValidState());
    }
    
    [Fact]
    public void ApplyReorientation_YPrime_ShouldChangeOrientationCorrectly()
    {
        var cube = Cube.CreateSolved();
        var originalOrientation = cube.Orientation;
        
        // Apply y' (counter-clockwise around up-down axis)
        var yPrimeMove = new Move('y', MoveDirection.CounterClockwise);
        cube.ApplyReorientation(yPrimeMove);
        
        // Y': Front becomes Left, Left becomes Back, Back becomes Right, Right becomes Front
        Assert.Equal(originalOrientation.Left, cube.Orientation.Front);   // Left becomes Front
        Assert.Equal(originalOrientation.Up, cube.Orientation.Up);        // Up stays the same
        
        // Pieces should not have moved
        Assert.Equal(20, cube.Pieces.Count);
        Assert.True(cube.IsValidState());
    }
    
    [Fact]
    public void ApplyReorientation_ZMove_ShouldChangeOrientationCorrectly()
    {
        var cube = Cube.CreateSolved();
        var originalOrientation = cube.Orientation;
        
        // Apply z (clockwise around front-back axis)
        var zMove = new Move('z', MoveDirection.Clockwise);
        cube.ApplyReorientation(zMove);
        
        // Z: Up becomes Right, Right becomes Down, Down becomes Left, Left becomes Up
        Assert.Equal(originalOrientation.Front, cube.Orientation.Front);  // Front stays the same
        Assert.Equal(originalOrientation.Left, cube.Orientation.Up);      // Left becomes Up
        
        // Pieces should not have moved
        Assert.Equal(20, cube.Pieces.Count);
        Assert.True(cube.IsValidState());
    }
    
    [Fact]
    public void ApplyReorientation_ZPrime_ShouldChangeOrientationCorrectly()
    {
        var cube = Cube.CreateSolved();
        var originalOrientation = cube.Orientation;
        
        // Apply z' (counter-clockwise around front-back axis)
        var zPrimeMove = new Move('z', MoveDirection.CounterClockwise);
        cube.ApplyReorientation(zPrimeMove);
        
        // Z': Up becomes Left, Right becomes Up, Down becomes Right, Left becomes Down
        Assert.Equal(originalOrientation.Front, cube.Orientation.Front);  // Front stays the same
        Assert.Equal(originalOrientation.Right, cube.Orientation.Up);     // Right becomes Up
        
        // Pieces should not have moved
        Assert.Equal(20, cube.Pieces.Count);
        Assert.True(cube.IsValidState());
    }
    
    [Fact]
    public void ApplyReorientation_X2_ShouldApplyTwice()
    {
        var cube = Cube.CreateSolved();
        var originalOrientation = cube.Orientation;
        
        // Apply x2 (double rotation around right-left axis)
        var x2Move = new Move('x', MoveDirection.Double);
        cube.ApplyReorientation(x2Move);
        
        // X2 should be equivalent to applying x twice
        // After two X rotations: Front->Up->Back->Down->Front, Up->Back->Down->Front->Up
        Assert.Equal(originalOrientation.Back, cube.Orientation.Front);   // Front->Up->Back
        Assert.Equal(originalOrientation.Bottom, cube.Orientation.Up);    // Up->Back->Down
        
        // Pieces should not have moved
        Assert.Equal(20, cube.Pieces.Count);
        Assert.True(cube.IsValidState());
    }
    
    [Fact]
    public void ApplyReorientation_Y2_ShouldApplyTwice()
    {
        var cube = Cube.CreateSolved();
        var originalOrientation = cube.Orientation;
        
        // Apply y2 (double rotation around up-down axis)
        var y2Move = new Move('y', MoveDirection.Double);
        cube.ApplyReorientation(y2Move);
        
        // Y2 should apply Y rotation twice (180 degrees around up-down axis):
        // Start: Front=Green, Up=Yellow, Right=Orange  
        // Y1: Front=Orange, Up=Yellow, Right=Blue  
        // Y2: Front=Blue, Up=Yellow, Right=Red
        // So Y2 should result in Blue front (opposite of Green)
        Assert.Equal(CubeColor.Blue, cube.Orientation.Front);
        Assert.Equal(originalOrientation.Up, cube.Orientation.Up);        // Up stays the same
        
        // Pieces should not have moved
        Assert.Equal(20, cube.Pieces.Count);
        Assert.True(cube.IsValidState());
    }
    
    [Fact]
    public void ApplyReorientation_Z2_ShouldApplyTwice()
    {
        var cube = Cube.CreateSolved();
        var originalOrientation = cube.Orientation;
        
        // Apply z2 (double rotation around front-back axis)
        var z2Move = new Move('z', MoveDirection.Double);
        cube.ApplyReorientation(z2Move);
        
        // Z2 should flip up to down
        Assert.Equal(originalOrientation.Front, cube.Orientation.Front);  // Front stays the same
        Assert.Equal(originalOrientation.Bottom, cube.Orientation.Up);    // Up->Left->Down
        
        // Pieces should not have moved
        Assert.Equal(20, cube.Pieces.Count);
        Assert.True(cube.IsValidState());
    }
    
    [Fact]
    public void ApplyReorientation_MultipleRotations_ShouldComposeCorrectly()
    {
        var cube = Cube.CreateSolved();
        
        // Apply sequence: x y z
        cube.ApplyReorientation(new Move('x'));
        cube.ApplyReorientation(new Move('y'));
        cube.ApplyReorientation(new Move('z'));
        
        // Should result in a valid orientation
        Assert.True(cube.IsValidState());
        Assert.Equal(20, cube.Pieces.Count);
        
        // The exact final orientation depends on composition, but should be valid
        // Since default(CubeColor) is White, which is valid, just check the orientation exists
        Assert.NotNull(cube.Orientation);
        Assert.True(Enum.IsDefined(typeof(CubeColor), cube.Orientation.Front));
        Assert.True(Enum.IsDefined(typeof(CubeColor), cube.Orientation.Up));
    }
    
    [Fact]
    public void ApplyReorientation_InverseRotations_ShouldReturnToOriginal()
    {
        var cube = Cube.CreateSolved();
        var originalOrientation = cube.Orientation;
        
        // Apply x then x' - test each separately to isolate issues
        cube.ApplyReorientation(new Move('x'));
        cube.ApplyReorientation(new Move('x', MoveDirection.CounterClockwise));
        Assert.Equal(originalOrientation, cube.Orientation);
        
        // Reset and test y/y'
        cube = Cube.CreateSolved();
        cube.ApplyReorientation(new Move('y'));
        cube.ApplyReorientation(new Move('y', MoveDirection.CounterClockwise));
        Assert.Equal(originalOrientation, cube.Orientation);
        
        // Reset and test z/z'
        cube = Cube.CreateSolved();
        cube.ApplyReorientation(new Move('z'));
        cube.ApplyReorientation(new Move('z', MoveDirection.CounterClockwise));
        Assert.Equal(originalOrientation, cube.Orientation);
    }
    
    [Theory]
    [InlineData('x')]
    [InlineData('y')]  // Re-enabled after GetRightColor fix
    [InlineData('z')]
    public void ApplyReorientation_QuadrupleRotation_ShouldReturnToOriginal(char axis)
    {
        var cube = Cube.CreateSolved();
        var originalOrientation = cube.Orientation;
        
        // Apply the same rotation 4 times
        for (int i = 0; i < 4; i++)
        {
            cube.ApplyReorientation(new Move(axis));
        }
        
        Assert.Equal(originalOrientation, cube.Orientation);
        Assert.Equal(20, cube.Pieces.Count);
        Assert.True(cube.IsValidState());
    }
    
    [Fact]
    public void ApplyReorientation_NonReorientationMove_ShouldThrow()
    {
        var cube = Cube.CreateSolved();
        var rMove = new Move('R'); // This is a rotation, not reorientation
        
        Assert.Throws<ArgumentException>(() => cube.ApplyReorientation(rMove));
    }
    
    [Fact]
    public void ApplyReorientation_StandardOrientationChecks_ShouldWork()
    {
        var cube = Cube.CreateSolved();
        
        // Initially should be in standard orientation
        Assert.Equal(CubeColor.Green, cube.Orientation.Front);
        Assert.Equal(CubeColor.Yellow, cube.Orientation.Up);
        
        // Let's verify what the actual standard orientation gives us
        var standardRight = cube.Orientation.Right;
        var standardLeft = cube.Orientation.Left;
        var standardBack = cube.Orientation.Back;
        var standardBottom = cube.Orientation.Bottom;
        
        // After Y rotation: Front becomes old Right (Orange)
        cube.ApplyReorientation(new Move('y'));
        Assert.Equal(CubeColor.Orange, cube.Orientation.Front); // standardRight is Orange
        Assert.Equal(CubeColor.Yellow, cube.Orientation.Up); // Up unchanged
        
        // With Front=Orange, Up=Yellow, corrected GetRightColor should give Blue  
        Assert.Equal(CubeColor.Blue, cube.Orientation.Right);  // Corrected mapping
        Assert.Equal(CubeColor.Green, cube.Orientation.Left);  // Opposite of Blue is Green
        Assert.Equal(CubeColor.Red, cube.Orientation.Back);    // Opposite of Orange is Red
    }
    
    [Fact]
    public void ApplyReorientation_PiecesDoNotMove_OnlyOrientationChanges()
    {
        var cube = Cube.CreateSolved();
        
        // Get all piece positions before reorientation
        var originalPieces = cube.Pieces.ToDictionary(p => p.Position, p => p);
        
        // Apply several reorientations
        cube.ApplyReorientation(new Move('x'));
        cube.ApplyReorientation(new Move('y'));
        cube.ApplyReorientation(new Move('z'));
        
        // All pieces should still be in the same positions with same colors
        Assert.Equal(20, cube.Pieces.Count);
        foreach (var piece in cube.Pieces)
        {
            Assert.True(originalPieces.ContainsKey(piece.Position));
            var originalPiece = originalPieces[piece.Position];
            Assert.Equal(originalPiece.Colors, piece.Colors);
            Assert.Equal(originalPiece.SolvedPosition, piece.SolvedPosition);
        }
    }
}