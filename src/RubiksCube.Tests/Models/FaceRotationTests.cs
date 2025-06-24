using RubiksCube.Core.Models;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.Models;

/// <summary>
/// Tests for face rotations (R, L, U, D, F, B) - piece movement operations
/// These tests validate that pieces move to correct positions and orientations
/// </summary>
public class FaceRotationTests
{
    #region Individual Move Validation Tests

    [Fact]
    public void ApplyMove_R_ShouldMoveCorrectPieces()
    {
        var cube = Cube.CreateSolved();
        var originalPieces = cube.Pieces.ToDictionary(p => p.Position, p => p);
        
        var rMove = new Move('R', MoveDirection.Clockwise);
        cube.ApplyMove(rMove);
        
        // R move should only affect pieces on the right face (X = 1)
        // 4 corners: (1,1,1), (1,1,-1), (1,-1,1), (1,-1,-1)
        // 4 edges: (1,1,0), (1,0,1), (1,0,-1), (1,-1,0)
        
        var rightFacePieces = cube.Pieces.Where(p => p.Position.X == 1).ToList();
        Assert.Equal(8, rightFacePieces.Count); // 4 corners + 4 edges
        
        // All right face pieces should have moved (position or orientation changed)
        foreach (var piece in rightFacePieces)
        {
            var originalPiece = originalPieces[piece.SolvedPosition];
            bool positionChanged = piece.Position != originalPiece.Position;
            bool orientationChanged = !piece.Colors.SequenceEqual(originalPiece.Colors);
            
            Assert.True(positionChanged || orientationChanged, 
                $"Right face piece at {piece.SolvedPosition} should have moved or rotated");
        }
        
        // Pieces not on right face should be unchanged
        var nonRightFacePieces = cube.Pieces.Where(p => p.Position.X != 1).ToList();
        foreach (var piece in nonRightFacePieces)
        {
            var originalPiece = originalPieces[piece.SolvedPosition];
            Assert.Equal(originalPiece.Position, piece.Position);
            Assert.Equal(originalPiece.Colors, piece.Colors);
        }
        
        // Cube should remain valid
        Assert.True(cube.IsValidState());
        Assert.Equal(20, cube.Pieces.Count);
    }
    
    [Fact]
    public void ApplyMove_L_ShouldMoveCorrectPieces()
    {
        var cube = Cube.CreateSolved();
        var originalPieces = cube.Pieces.ToDictionary(p => p.Position, p => p);
        
        var lMove = new Move('L', MoveDirection.Clockwise);
        cube.ApplyMove(lMove);
        
        // L move should only affect pieces on the left face (X = -1)
        var leftFacePieces = cube.Pieces.Where(p => p.Position.X == -1).ToList();
        Assert.Equal(8, leftFacePieces.Count); // 4 corners + 4 edges
        
        // All left face pieces should have moved
        foreach (var piece in leftFacePieces)
        {
            var originalPiece = originalPieces[piece.SolvedPosition];
            bool positionChanged = piece.Position != originalPiece.Position;
            bool orientationChanged = !piece.Colors.SequenceEqual(originalPiece.Colors);
            
            Assert.True(positionChanged || orientationChanged,
                $"Left face piece at {piece.SolvedPosition} should have moved or rotated");
        }
        
        // Non-left face pieces should be unchanged
        var nonLeftFacePieces = cube.Pieces.Where(p => p.Position.X != -1).ToList();
        foreach (var piece in nonLeftFacePieces)
        {
            var originalPiece = originalPieces[piece.SolvedPosition];
            Assert.Equal(originalPiece.Position, piece.Position);
            Assert.Equal(originalPiece.Colors, piece.Colors);
        }
        
        Assert.True(cube.IsValidState());
    }
    
    [Fact]
    public void ApplyMove_U_ShouldMoveCorrectPieces()
    {
        var cube = Cube.CreateSolved();
        var originalPieces = cube.Pieces.ToDictionary(p => p.Position, p => p);
        
        var uMove = new Move('U', MoveDirection.Clockwise);
        cube.ApplyMove(uMove);
        
        // U move should only affect pieces on the up face (Y = 1)
        var upFacePieces = cube.Pieces.Where(p => p.Position.Y == 1).ToList();
        Assert.Equal(8, upFacePieces.Count); // 4 corners + 4 edges
        
        foreach (var piece in upFacePieces)
        {
            var originalPiece = originalPieces[piece.SolvedPosition];
            bool positionChanged = piece.Position != originalPiece.Position;
            bool orientationChanged = !piece.Colors.SequenceEqual(originalPiece.Colors);
            
            Assert.True(positionChanged || orientationChanged,
                $"Up face piece at {piece.SolvedPosition} should have moved or rotated");
        }
        
        // Non-up face pieces should be unchanged
        var nonUpFacePieces = cube.Pieces.Where(p => p.Position.Y != 1).ToList();
        foreach (var piece in nonUpFacePieces)
        {
            var originalPiece = originalPieces[piece.SolvedPosition];
            Assert.Equal(originalPiece.Position, piece.Position);
            Assert.Equal(originalPiece.Colors, piece.Colors);
        }
        
        Assert.True(cube.IsValidState());
    }
    
    [Fact]
    public void ApplyMove_D_ShouldMoveCorrectPieces()
    {
        var cube = Cube.CreateSolved();
        var originalPieces = cube.Pieces.ToDictionary(p => p.Position, p => p);
        
        var dMove = new Move('D', MoveDirection.Clockwise);
        cube.ApplyMove(dMove);
        
        // D move should only affect pieces on the down face (Y = -1)
        var downFacePieces = cube.Pieces.Where(p => p.Position.Y == -1).ToList();
        Assert.Equal(8, downFacePieces.Count);
        
        foreach (var piece in downFacePieces)
        {
            var originalPiece = originalPieces[piece.SolvedPosition];
            bool positionChanged = piece.Position != originalPiece.Position;
            bool orientationChanged = !piece.Colors.SequenceEqual(originalPiece.Colors);
            
            Assert.True(positionChanged || orientationChanged,
                $"Down face piece at {piece.SolvedPosition} should have moved or rotated");
        }
        
        var nonDownFacePieces = cube.Pieces.Where(p => p.Position.Y != -1).ToList();
        foreach (var piece in nonDownFacePieces)
        {
            var originalPiece = originalPieces[piece.SolvedPosition];
            Assert.Equal(originalPiece.Position, piece.Position);
            Assert.Equal(originalPiece.Colors, piece.Colors);
        }
        
        Assert.True(cube.IsValidState());
    }
    
    [Fact]
    public void ApplyMove_F_ShouldMoveCorrectPieces()
    {
        var cube = Cube.CreateSolved();
        var originalPieces = cube.Pieces.ToDictionary(p => p.Position, p => p);
        
        var fMove = new Move('F', MoveDirection.Clockwise);
        cube.ApplyMove(fMove);
        
        // F move should only affect pieces on the front face (Z = 1)
        var frontFacePieces = cube.Pieces.Where(p => p.Position.Z == 1).ToList();
        Assert.Equal(8, frontFacePieces.Count);
        
        foreach (var piece in frontFacePieces)
        {
            var originalPiece = originalPieces[piece.SolvedPosition];
            bool positionChanged = piece.Position != originalPiece.Position;
            bool orientationChanged = !piece.Colors.SequenceEqual(originalPiece.Colors);
            
            Assert.True(positionChanged || orientationChanged,
                $"Front face piece at {piece.SolvedPosition} should have moved or rotated");
        }
        
        var nonFrontFacePieces = cube.Pieces.Where(p => p.Position.Z != 1).ToList();
        foreach (var piece in nonFrontFacePieces)
        {
            var originalPiece = originalPieces[piece.SolvedPosition];
            Assert.Equal(originalPiece.Position, piece.Position);
            Assert.Equal(originalPiece.Colors, piece.Colors);
        }
        
        Assert.True(cube.IsValidState());
    }
    
    [Fact]
    public void ApplyMove_B_ShouldMoveCorrectPieces()
    {
        var cube = Cube.CreateSolved();
        var originalPieces = cube.Pieces.ToDictionary(p => p.Position, p => p);
        
        var bMove = new Move('B', MoveDirection.Clockwise);
        cube.ApplyMove(bMove);
        
        // B move should only affect pieces on the back face (Z = -1)
        var backFacePieces = cube.Pieces.Where(p => p.Position.Z == -1).ToList();
        Assert.Equal(8, backFacePieces.Count);
        
        foreach (var piece in backFacePieces)
        {
            var originalPiece = originalPieces[piece.SolvedPosition];
            bool positionChanged = piece.Position != originalPiece.Position;
            bool orientationChanged = !piece.Colors.SequenceEqual(originalPiece.Colors);
            
            Assert.True(positionChanged || orientationChanged,
                $"Back face piece at {piece.SolvedPosition} should have moved or rotated");
        }
        
        var nonBackFacePieces = cube.Pieces.Where(p => p.Position.Z != -1).ToList();
        foreach (var piece in nonBackFacePieces)
        {
            var originalPiece = originalPieces[piece.SolvedPosition];
            Assert.Equal(originalPiece.Position, piece.Position);
            Assert.Equal(originalPiece.Colors, piece.Colors);
        }
        
        Assert.True(cube.IsValidState());
    }

    #endregion

    #region Mathematical Property Tests

    [Theory]
    [InlineData('R')]
    [InlineData('L')]
    [InlineData('U')]
    [InlineData('D')]
    [InlineData('F')]
    [InlineData('B')]
    public void ApplyMove_InverseMoves_ShouldReturnToSolved(char face)
    {
        var cube = Cube.CreateSolved();
        var originalState = cube.Clone();
        
        // Apply move then its inverse
        cube.ApplyMove(new Move(face, MoveDirection.Clockwise));
        cube.ApplyMove(new Move(face, MoveDirection.CounterClockwise));
        
        // Should return to original solved state
        Assert.Equal(originalState.Pieces.Count, cube.Pieces.Count);
        
        foreach (var piece in cube.Pieces)
        {
            var originalPiece = originalState.Pieces.First(p => p.SolvedPosition == piece.SolvedPosition);
            Assert.Equal(originalPiece.Position, piece.Position);
            Assert.Equal(originalPiece.Colors, piece.Colors);
        }
        
        Assert.True(cube.IsSolved);
        Assert.True(cube.IsValidState());
    }
    
    [Theory]
    [InlineData('R')]
    [InlineData('L')]
    [InlineData('U')]
    [InlineData('D')]
    [InlineData('F')]
    [InlineData('B')]
    public void ApplyMove_FourFoldSymmetry_ShouldReturnToSolved(char face)
    {
        var cube = Cube.CreateSolved();
        var originalState = cube.Clone();
        
        // Apply same move 4 times (full 360° rotation)
        for (int i = 0; i < 4; i++)
        {
            cube.ApplyMove(new Move(face, MoveDirection.Clockwise));
        }
        
        // Should return to original solved state
        Assert.Equal(originalState.Pieces.Count, cube.Pieces.Count);
        
        foreach (var piece in cube.Pieces)
        {
            var originalPiece = originalState.Pieces.First(p => p.SolvedPosition == piece.SolvedPosition);
            Assert.Equal(originalPiece.Position, piece.Position);
            Assert.Equal(originalPiece.Colors, piece.Colors);
        }
        
        Assert.True(cube.IsSolved);
        Assert.True(cube.IsValidState());
    }
    
    [Theory]
    [InlineData('R')]
    [InlineData('L')]
    [InlineData('U')]
    [InlineData('D')]
    [InlineData('F')]
    [InlineData('B')]
    public void ApplyMove_DoubleMoves_ShouldBeEquivalentToTwoSingles(char face)
    {
        var cube1 = Cube.CreateSolved();
        var cube2 = Cube.CreateSolved();
        
        // Apply double move to cube1
        cube1.ApplyMove(new Move(face, MoveDirection.Double));
        
        // Apply two single moves to cube2
        cube2.ApplyMove(new Move(face, MoveDirection.Clockwise));
        cube2.ApplyMove(new Move(face, MoveDirection.Clockwise));
        
        // Results should be identical
        Assert.Equal(cube1.Pieces.Count, cube2.Pieces.Count);
        
        foreach (var piece1 in cube1.Pieces)
        {
            var piece2 = cube2.Pieces.First(p => p.SolvedPosition == piece1.SolvedPosition);
            Assert.Equal(piece1.Position, piece2.Position);
            Assert.Equal(piece1.Colors, piece2.Colors);
        }
        
        Assert.Equal(cube1.IsValidState(), cube2.IsValidState());
    }
    
    [Fact]
    public void ApplyMove_StateIntegrity_ShouldAlwaysRemainValid()
    {
        var cube = Cube.CreateSolved();
        var moves = new[] { 'R', 'L', 'U', 'D', 'F', 'B' };
        var directions = new[] { MoveDirection.Clockwise, MoveDirection.CounterClockwise, MoveDirection.Double };
        
        // Apply random sequence of moves
        var random = new Random(42); // Fixed seed for reproducibility
        for (int i = 0; i < 20; i++)
        {
            var face = moves[random.Next(moves.Length)];
            var direction = directions[random.Next(directions.Length)];
            var move = new Move(face, direction);
            
            cube.ApplyMove(move);
            
            // After each move, cube should remain valid
            Assert.True(cube.IsValidState(), $"Cube became invalid after move {i + 1}: {move}");
            Assert.True(cube.Pieces.Count == 20, $"Wrong piece count after move {i + 1}: {move}");
            
            // Should have exactly 9 stickers of each color (8 movable + 1 center)
            var movableColors = cube.Pieces.SelectMany(p => p.Colors).ToList();
            var centerColors = new[] { 
                cube.Orientation.Up, cube.Orientation.Bottom, 
                cube.Orientation.Left, cube.Orientation.Right,
                cube.Orientation.Front, cube.Orientation.Back 
            };
            var allColors = movableColors.Concat(centerColors).ToList();
            
            foreach (CubeColor color in Enum.GetValues<CubeColor>())
            {
                var count = allColors.Count(c => c == color);
                Assert.True(count == 9, $"Wrong count for {color} after move {i + 1}: {move}");
            }
        }
    }
    
    [Fact]
    public void ApplyMove_OrientationUnaffected_ByFaceRotations()
    {
        var cube = Cube.CreateSolved();
        var originalOrientation = cube.Orientation;
        
        // Apply various face rotations
        cube.ApplyMove(new Move('R'));
        cube.ApplyMove(new Move('U'));
        cube.ApplyMove(new Move('F'));
        cube.ApplyMove(new Move('L', MoveDirection.CounterClockwise));
        
        // Orientation should remain unchanged
        Assert.Equal(originalOrientation.Front, cube.Orientation.Front);
        Assert.Equal(originalOrientation.Up, cube.Orientation.Up);
        Assert.Equal(originalOrientation.Right, cube.Orientation.Right);
        Assert.Equal(originalOrientation.Left, cube.Orientation.Left);
        Assert.Equal(originalOrientation.Back, cube.Orientation.Back);
        Assert.Equal(originalOrientation.Bottom, cube.Orientation.Bottom);
    }

    #endregion

    #region Mixed Operations Tests
    
    [Fact]
    public void ApplyMove_MixedReorientationsAndRotations_ShouldWorkCorrectly()
    {
        var cube = Cube.CreateSolved();
        
        // Apply reorientation, then rotation, then another reorientation
        cube.ApplyReorientation(new Move('x'));  // Front becomes Up
        cube.ApplyMove(new Move('R'));           // Face rotation
        cube.ApplyReorientation(new Move('y'));  // Rotate viewing angle
        
        // Cube should remain valid throughout
        Assert.True(cube.IsValidState());
        Assert.Equal(20, cube.Pieces.Count);
        
        // Should be able to continue with more moves
        cube.ApplyMove(new Move('U', MoveDirection.CounterClockwise));
        cube.ApplyReorientation(new Move('z', MoveDirection.CounterClockwise));
        
        Assert.True(cube.IsValidState());
    }
    
    [Fact]
    public void ApplyMove_RendererStillWorks_AfterFaceRotations()
    {
        var cube = Cube.CreateSolved();
        
        // Apply some face rotations to scramble pieces
        cube.ApplyMove(new Move('R'));
        cube.ApplyMove(new Move('U'));
        cube.ApplyMove(new Move('F'));
        
        // Renderer should still work (no exceptions)
        var renderer = new RubiksCube.Core.Display.CubeRenderer(
            new RubiksCube.Core.Display.DisplayConfig());
        var result = renderer.Render(cube);
        
        Assert.True(result.IsSuccess, $"Renderer failed: {(result.IsFailure ? result.Error : "N/A")}");
        Assert.NotNull(result.Value);
        Assert.True(result.Value.Length > 0);
        
        // Should contain valid color symbols (no empty faces)
        Assert.DoesNotContain("   ", result.Value); // No empty 3-character sequences
    }

    #endregion
}