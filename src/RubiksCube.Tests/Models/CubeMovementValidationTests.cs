using RubiksCube.Core.Models;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.Models;

/// <summary>
/// Mathematical validation of cube movements - no hard-coded expectations
/// Tests based on geometric principles and invariants
/// </summary>
public class CubeMovementValidationTests
{
    [Fact]
    public void XRotation_ShouldMoveExactly24Pieces()
    {
        // Mathematical fact: X rotation around X-axis (through Left-Right centers)
        // Only pieces with Y=0 AND Z=0 are on the X-axis and don't move
        // 26 total pieces - 2 pieces on X-axis = 24 pieces that must move
        
        var cube = Cube.CreateSolved();
        var piecesOnXAxis = cube.Pieces.Where(p => p.Position.Y == 0 && p.Position.Z == 0).ToList();
        var piecesOffXAxis = cube.Pieces.Where(p => !(p.Position.Y == 0 && p.Position.Z == 0)).ToList();
        
        // Verify our understanding of the geometry
        Assert.Equal(2, piecesOnXAxis.Count); // 2 centers at (±1,0,0)
        Assert.Equal(24, piecesOffXAxis.Count);
        
        // Store original positions
        var originalOnAxis = piecesOnXAxis.ToDictionary(p => p.SolvedPosition, p => p.Position);
        var originalOffAxis = piecesOffXAxis.ToDictionary(p => p.SolvedPosition, p => p.Position);
        
        // Apply X rotation
        var xMove = new Move('x');
        cube.ApplyReorientation(xMove);
        
        // Pieces on X-axis should NOT move
        foreach (var piece in piecesOnXAxis)
        {
            Assert.Equal(originalOnAxis[piece.SolvedPosition], piece.Position);
        }
        
        // Re-query pieces after move and count how many moved
        var movedCount = 0;
        foreach (var originalPiece in piecesOffXAxis)
        {
            var currentPiece = cube.Pieces.First(p => p.SolvedPosition.Equals(originalPiece.SolvedPosition));
            if (currentPiece.Position != originalOffAxis[originalPiece.SolvedPosition])
                movedCount++;
        }
        
        Assert.Equal(24, movedCount); // Exactly 24 pieces must move
    }
    
    [Fact]
    public void YRotation_ShouldMoveExactly24Pieces()
    {
        // Y rotation around Y-axis (through Top-Bottom centers)
        // Only pieces with X=0 AND Z=0 are on the Y-axis and don't move
        
        var cube = Cube.CreateSolved();
        var piecesOnYAxis = cube.Pieces.Where(p => p.Position.X == 0 && p.Position.Z == 0).ToList();
        var piecesOffYAxis = cube.Pieces.Where(p => !(p.Position.X == 0 && p.Position.Z == 0)).ToList();
        
        Assert.Equal(2, piecesOnYAxis.Count);
        Assert.Equal(24, piecesOffYAxis.Count);
        
        var originalOffAxis = piecesOffYAxis.ToDictionary(p => p.SolvedPosition, p => p.Position);
        
        var yMove = new Move('y');
        cube.ApplyReorientation(yMove);
        
        var movedCount = 0;
        foreach (var originalPiece in piecesOffYAxis)
        {
            var currentPiece = cube.Pieces.First(p => p.SolvedPosition.Equals(originalPiece.SolvedPosition));
            if (currentPiece.Position != originalOffAxis[originalPiece.SolvedPosition])
                movedCount++;
        }
        Assert.Equal(24, movedCount);
    }
    
    [Fact]
    public void ZRotation_ShouldMoveExactly24Pieces()
    {
        // Z rotation around Z-axis (through Front-Back centers)
        // Only pieces with X=0 AND Y=0 are on the Z-axis and don't move
        
        var cube = Cube.CreateSolved();
        var piecesOnZAxis = cube.Pieces.Where(p => p.Position.X == 0 && p.Position.Y == 0).ToList();
        var piecesOffZAxis = cube.Pieces.Where(p => !(p.Position.X == 0 && p.Position.Y == 0)).ToList();
        
        Assert.Equal(2, piecesOnZAxis.Count);
        Assert.Equal(24, piecesOffZAxis.Count);
        
        var originalOffAxis = piecesOffZAxis.ToDictionary(p => p.SolvedPosition, p => p.Position);
        
        var zMove = new Move('z');
        cube.ApplyReorientation(zMove);
        
        var movedCount = 0;
        foreach (var originalPiece in piecesOffZAxis)
        {
            var currentPiece = cube.Pieces.First(p => p.SolvedPosition.Equals(originalPiece.SolvedPosition));
            if (currentPiece.Position != originalOffAxis[originalPiece.SolvedPosition])
                movedCount++;
        }
        Assert.Equal(24, movedCount);
    }
    
    [Fact]
    public void RMove_ShouldMoveExactly8Pieces()
    {
        // R move rotates the right face (X = 1)
        // Should move: 1 center, 4 edges, 4 corners = 9 pieces total on the face
        // But the center doesn't actually change position, so 8 pieces move
        
        var cube = Cube.CreateSolved();
        var rightFacePieces = cube.Pieces.Where(p => p.Position.X == 1).ToList();
        
        // Verify face composition
        var center = rightFacePieces.Where(p => p.Type == PieceType.Center).ToList();
        var edges = rightFacePieces.Where(p => p.Type == PieceType.Edge).ToList();
        var corners = rightFacePieces.Where(p => p.Type == PieceType.Corner).ToList();
        
        Assert.Single(center);
        Assert.Equal(4, edges.Count);
        Assert.Equal(4, corners.Count);
        
        var originalPositions = rightFacePieces.ToDictionary(p => p.SolvedPosition, p => p.Position);
        
        var rMove = new Move('R');
        cube.ApplyMove(rMove);
        
        // Center should not move (it rotates in place)
        var centerPiece = center.First();
        Assert.Equal(originalPositions[centerPiece.SolvedPosition], centerPiece.Position);
        
        // Count moved pieces (center doesn't move) - re-query after move
        var movedCount = 0;
        foreach (var originalPiece in rightFacePieces)
        {
            var currentPiece = cube.Pieces.First(p => p.SolvedPosition.Equals(originalPiece.SolvedPosition));
            if (currentPiece.Position != originalPositions[originalPiece.SolvedPosition])
                movedCount++;
        }
        Assert.Equal(8, movedCount);
    }
    
    [Theory]
    [InlineData('R', 1, 0, 0)]  // Right face: X = 1
    [InlineData('L', -1, 0, 0)] // Left face: X = -1
    [InlineData('U', 0, 1, 0)]  // Up face: Y = 1
    [InlineData('D', 0, -1, 0)] // Down face: Y = -1
    [InlineData('F', 0, 0, 1)]  // Front face: Z = 1
    [InlineData('B', 0, 0, -1)] // Back face: Z = -1
    public void FaceMoves_ShouldMoveExactly8Pieces(char face, int x, int y, int z)
    {
        var cube = Cube.CreateSolved();
        var facePosition = new Position3D(x, y, z);
        
        // Get all pieces on the face
        var facePieces = cube.Pieces.Where(p => 
            (x != 0 && p.Position.X == x) ||
            (y != 0 && p.Position.Y == y) ||
            (z != 0 && p.Position.Z == z)
        ).ToList();
        
        Assert.Equal(9, facePieces.Count); // 1 center + 4 edges + 4 corners
        
        var originalPositions = facePieces.ToDictionary(p => p.SolvedPosition, p => p.Position);
        
        var move = new Move(face);
        cube.ApplyMove(move);
        
        // Count moved pieces (center doesn't move) - re-query after move
        var movedCount = 0;
        foreach (var originalPiece in facePieces)
        {
            var currentPiece = cube.Pieces.First(p => p.SolvedPosition.Equals(originalPiece.SolvedPosition));
            if (currentPiece.Position != originalPositions[originalPiece.SolvedPosition])
                movedCount++;
        }
        Assert.Equal(8, movedCount);
    }
    
    [Fact]
    public void AllMoves_ShouldPreserve26Pieces()
    {
        // State integrity: After any move, we should still have exactly 26 unique pieces
        var cube = Cube.CreateSolved();
        var moves = new[]
        {
            new Move('R'), new Move('L'), new Move('U'), 
            new Move('D'), new Move('F'), new Move('B'),
            new Move('x'), new Move('y'), new Move('z')
        };
        
        foreach (var move in moves)
        {
            var testCube = Cube.CreateSolved();
            
            if (move.Type == MoveType.Rotation)
                testCube.ApplyMove(move);
            else
                testCube.ApplyReorientation(move);
            
            // Verify piece count
            Assert.Equal(26, testCube.Pieces.Count);
            
            // Verify no duplicate positions
            var positions = testCube.Pieces.Select(p => p.Position).ToList();
            Assert.Equal(26, positions.Distinct().Count());
            
            // Verify piece types are preserved
            Assert.Equal(8, testCube.Corners.Count());
            Assert.Equal(12, testCube.Edges.Count());
            Assert.Equal(6, testCube.Centers.Count());
        }
    }
    
    [Fact]
    public void FourRotations_MathematicalInvariant()
    {
        // Mathematical fact: 4 rotations of 90° = 360° = identity
        // This tests the rotation group property
        
        var moves = new[] { 'R', 'L', 'U', 'D', 'F', 'B', 'x', 'y', 'z' };
        
        foreach (var moveChar in moves)
        {
            var cube = Cube.CreateSolved();
            var originalState = cube.Pieces.Select(p => new { p.SolvedPosition, p.Position, Colors = p.Colors.ToArray() }).ToList();
            
            var move = new Move(moveChar);
            
            // Apply same move 4 times
            for (int i = 0; i < 4; i++)
            {
                if (move.Type == MoveType.Rotation)
                    cube.ApplyMove(move);
                else
                    cube.ApplyReorientation(move);
            }
            
            // Should be back to original
            foreach (var original in originalState)
            {
                var current = cube.Pieces.First(p => p.SolvedPosition.Equals(original.SolvedPosition));
                Assert.Equal(original.Position, current.Position);
                Assert.Equal(original.Colors, current.Colors);
            }
        }
    }
    
    [Fact]
    public void MoveInverse_MathematicalProperty()
    {
        // Mathematical fact: Move × Move' = Identity
        // Already tested in other file but including for completeness
        
        var movePairs = new[]
        {
            ('R', MoveDirection.Clockwise, MoveDirection.CounterClockwise),
            ('L', MoveDirection.Clockwise, MoveDirection.CounterClockwise),
            ('U', MoveDirection.Clockwise, MoveDirection.CounterClockwise),
            ('D', MoveDirection.Clockwise, MoveDirection.CounterClockwise),
            ('F', MoveDirection.Clockwise, MoveDirection.CounterClockwise),
            ('B', MoveDirection.Clockwise, MoveDirection.CounterClockwise),
            ('x', MoveDirection.Clockwise, MoveDirection.CounterClockwise),
            ('y', MoveDirection.Clockwise, MoveDirection.CounterClockwise),
            ('z', MoveDirection.Clockwise, MoveDirection.CounterClockwise)
        };
        
        foreach (var (face, dir1, dir2) in movePairs)
        {
            var cube = Cube.CreateSolved();
            var originalCount = cube.Pieces.Count(p => p.Position.Equals(p.SolvedPosition));
            Assert.Equal(26, originalCount); // All solved
            
            var move1 = new Move(face, dir1);
            var move2 = new Move(face, dir2);
            
            if (move1.Type == MoveType.Rotation)
            {
                cube.ApplyMove(move1);
                cube.ApplyMove(move2);
            }
            else
            {
                cube.ApplyReorientation(move1);
                cube.ApplyReorientation(move2);
            }
            
            var finalCount = cube.Pieces.Count(p => p.Position.Equals(p.SolvedPosition));
            Assert.Equal(26, finalCount); // All back to solved
        }
    }
}