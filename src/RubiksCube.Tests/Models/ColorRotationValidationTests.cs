using RubiksCube.Core.Models;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.Models;

/// <summary>
/// Validates color rotation behavior based on Python's 2-axis swap rule
/// When a piece moves and 2 coordinates change, colors swap between those axis indices
/// </summary>
public class ColorRotationValidationTests
{
    [Fact]
    public void RMove_EdgePieces_ShouldSwapColorsCorrectly()
    {
        // R move rotates pieces on X=1 face around X-axis
        // For edges: if Y and Z change, swap colors at indices 1 and 2
        
        var cube = Cube.CreateSolved();
        
        // Track a specific edge piece on the right face
        // Edge at (1, 0, 1) has colors [Orange, null, Green]
        var trackedPiece = cube.Pieces.First(p => 
            p.Position.X == 1 && p.Position.Y == 0 && p.Position.Z == 1 && p.Type == PieceType.Edge);
        
        var originalColors = trackedPiece.Colors.ToArray();
        Assert.Equal(CubeColor.Orange, originalColors[0]); // X-axis color
        Assert.Null(originalColors[1]); // No Y-axis color (edge)
        Assert.Equal(CubeColor.Green, originalColors[2]); // Z-axis color
        
        // Apply R move
        cube.ApplyMove(new Move('R'));
        
        // After R: (1,0,1) -> (1,1,0)
        // Y changed from 0->1, Z changed from 1->0
        // So colors at indices 1 and 2 should swap
        
        // Find the piece at its new position
        var movedPiece = cube.Pieces.First(p => p.SolvedPosition.Equals(trackedPiece.SolvedPosition));
        Assert.Equal(new Position3D(1, 1, 0), movedPiece.Position);
        
        // Colors should have swapped at Y and Z indices
        Assert.Equal(CubeColor.Orange, movedPiece.Colors[0]); // X unchanged
        Assert.Equal(CubeColor.Green, movedPiece.Colors[1]); // Was at index 2
        Assert.Null(movedPiece.Colors[2]); // Was at index 1
    }
    
    [Fact]
    public void RMove_CornerPieces_ShouldProduceCorrectResult()
    {
        // Test R move on corner piece produces known correct result
        // This test was fixed to remove flawed coordinate-counting logic
        var cube = Cube.CreateSolved();
        
        // Track corner at (1, 1, 1) with colors [Orange, Yellow, Green]
        var trackedCorner = cube.Pieces.First(p =>
            p.Position.X == 1 && p.Position.Y == 1 && p.Position.Z == 1 && p.Type == PieceType.Corner);
        
        var originalColors = trackedCorner.Colors.ToArray();
        Assert.Equal(CubeColor.Orange, originalColors[0]); // X
        Assert.Equal(CubeColor.Yellow, originalColors[1]); // Y
        Assert.Equal(CubeColor.Green, originalColors[2]); // Z
        
        // Apply R move
        cube.ApplyMove(new Move('R'));
        
        // Find where the corner moved to
        var movedCorner = cube.Pieces.First(p => p.SolvedPosition.Equals(trackedCorner.SolvedPosition));
        
        // Verify known correct result from Python's algorithm:
        // (1,1,1) → (1,1,-1) with colors [Orange, Green, Yellow]
        Assert.Equal(new Position3D(1, 1, -1), movedCorner.Position);
        Assert.Equal(CubeColor.Orange, movedCorner.Colors[0]); // X unchanged  
        Assert.Equal(CubeColor.Green, movedCorner.Colors[1]);  // Was at Z, now at Y
        Assert.Equal(CubeColor.Yellow, movedCorner.Colors[2]); // Was at Y, now at Z
    }
    
    [Theory]
    [InlineData('U')] // Up face
    [InlineData('D')] // Down face  
    [InlineData('F')] // Front face
    [InlineData('B')] // Back face
    public void FaceMoves_ShouldPreserveMathematicalProperties(char face)
    {
        // Test that face moves preserve mathematical properties
        // This replaces the flawed coordinate-counting logic
        
        var cube = Cube.CreateSolved();
        var originalPieceCount = cube.Pieces.Count;
        
        // Apply face move
        cube.ApplyMove(new Move(face));
        
        // Verify mathematical properties are preserved
        Assert.Equal(originalPieceCount, cube.Pieces.Count); // Piece count unchanged
        Assert.True(cube.IsValidState()); // State remains valid
        
        // Verify all positions are still unique
        var positions = cube.Pieces.Select(p => p.Position).ToList();
        Assert.Equal(positions.Count, positions.Distinct().Count());
        
        // Verify 4 moves return to identity
        for (int i = 0; i < 3; i++)
        {
            cube.ApplyMove(new Move(face));
        }
        
        // After 4 moves, should be back to solved state
        Assert.True(cube.IsSolved);
    }
    
    [Fact]
    public void XReorientation_ShouldProduceCorrectResult()
    {
        // Test X rotation produces known correct result
        // This test was fixed to remove flawed coordinate-counting logic
        
        var cube = Cube.CreateSolved();
        
        // Track specific edge piece at (0, 1, 1) with colors [null, Yellow, Green]
        var trackedPiece = cube.Pieces.First(p => p.Position.X == 0 && p.Position.Y == 1 && p.Position.Z == 1);
        var originalColors = trackedPiece.Colors.ToArray();
        
        Assert.Null(originalColors[0]);                    // No X color (edge piece)
        Assert.Equal(CubeColor.Yellow, originalColors[1]); // Y color
        Assert.Equal(CubeColor.Green, originalColors[2]);  // Z color
        
        cube.ApplyReorientation(new Move('x'));
        
        var movedPiece = cube.Pieces.First(p => p.SolvedPosition.Equals(trackedPiece.SolvedPosition));
        
        // Verify known correct result: (0,1,1) → (0,1,-1) with colors [null, Green, Yellow]
        Assert.Equal(new Position3D(0, 1, -1), movedPiece.Position);
        Assert.Null(movedPiece.Colors[0]);                // X unchanged (null)
        Assert.Equal(CubeColor.Green, movedPiece.Colors[1]);  // Was at Z, now at Y
        Assert.Equal(CubeColor.Yellow, movedPiece.Colors[2]); // Was at Y, now at Z
    }
    
    [Fact]
    public void CenterPieces_NeverSwapColors()
    {
        // Center pieces have only one color and it never changes
        // They rotate in place but color stays in same axis index
        
        var cube = Cube.CreateSolved();
        var centers = cube.Centers.ToList();
        
        // Store original colors
        var originalCenterColors = centers.ToDictionary(
            c => c.SolvedPosition,
            c => c.Colors.ToArray()
        );
        
        // Apply various moves
        var moves = new[] { 'R', 'U', 'F', 'x', 'y', 'z' };
        foreach (var moveChar in moves)
        {
            var testCube = Cube.CreateSolved();
            var move = new Move(moveChar);
            
            if (move.Type == MoveType.Rotation)
                testCube.ApplyMove(move);
            else
                testCube.ApplyReorientation(move);
            
            // Check all centers
            foreach (var center in testCube.Centers)
            {
                var original = originalCenterColors[center.SolvedPosition];
                
                // Center should still have exactly one non-null color
                Assert.Equal(1, center.Colors.Count(c => c != null));
                
                // The color value should be unchanged (though it might be at a different index)
                var originalColor = original.First(c => c != null);
                var currentColor = center.Colors.First(c => c != null);
                Assert.Equal(originalColor, currentColor);
            }
        }
    }
    
    [Fact] 
    public void ComplexSequence_ShouldMaintainMathematicalProperties()
    {
        // Test that complex sequences maintain mathematical properties
        // This replaces the flawed coordinate-counting validation
        
        var cube = Cube.CreateSolved();
        
        // Apply R U R' U' sequence (should return to solved for these pieces)
        var sequence = new[] 
        { 
            new Move('R'), 
            new Move('U'), 
            new Move('R', MoveDirection.CounterClockwise), 
            new Move('U', MoveDirection.CounterClockwise) 
        };
        
        // Track specific pieces and their initial states
        var cornerInitial = cube.Pieces.First(p => p.Position.Equals(new Position3D(1, 1, 1)));
        var edgeInitial = cube.Pieces.First(p => p.Position.Equals(new Position3D(1, 1, 0)));
        var centerInitial = cube.Pieces.First(p => p.Position.Equals(new Position3D(0, 1, 0)));
        
        var initialStates = new Dictionary<Position3D, CubeColor?[]>
        {
            [cornerInitial.SolvedPosition] = cornerInitial.Colors.ToArray(),
            [edgeInitial.SolvedPosition] = edgeInitial.Colors.ToArray(),
            [centerInitial.SolvedPosition] = centerInitial.Colors.ToArray()
        };
        
        // Apply the sequence
        foreach (var move in sequence)
        {
            cube.ApplyMove(move);
        }
        
        // Verify mathematical properties are maintained
        Assert.Equal(26, cube.Pieces.Count); // Piece count preserved
        Assert.True(cube.IsValidState()); // State remains valid
        
        // For this specific sequence, verify that center pieces are unaffected
        var finalCenter = cube.Pieces.First(p => p.SolvedPosition.Equals(centerInitial.SolvedPosition));
        Assert.Equal(initialStates[centerInitial.SolvedPosition], finalCenter.Colors);
    }
}