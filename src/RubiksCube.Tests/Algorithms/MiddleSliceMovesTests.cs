using RubiksCube.Core.Algorithms;
using RubiksCube.Core.Models;

namespace RubiksCube.Tests.Algorithms;

/// <summary>
/// Tests for middle slice moves: M (Middle), E (Equatorial), S (Standing)
/// These moves affect the center slices between opposite faces
/// </summary>
public class MiddleSliceMovesTests
{
    [Theory]
    [InlineData('M')]
    [InlineData('E')]
    [InlineData('S')]
    public void Constructor_ValidMiddleSliceMoves_ShouldCreateMove(char face)
    {
        var move = new Move(face);
        
        Assert.Equal(face, move.Face);
        Assert.Equal(MoveDirection.Clockwise, move.Direction);
        Assert.False(move.IsWide); // M, E, S are not wide moves
    }
    
    [Theory]
    [InlineData('M', MoveDirection.Clockwise)]
    [InlineData('M', MoveDirection.CounterClockwise)]
    [InlineData('M', MoveDirection.Double)]
    [InlineData('E', MoveDirection.Clockwise)]
    [InlineData('E', MoveDirection.CounterClockwise)]
    [InlineData('E', MoveDirection.Double)]
    [InlineData('S', MoveDirection.Clockwise)]
    [InlineData('S', MoveDirection.CounterClockwise)]
    [InlineData('S', MoveDirection.Double)]
    public void Constructor_AllMiddleSliceDirections_ShouldSetCorrectly(char face, MoveDirection direction)
    {
        var move = new Move(face, direction);
        
        Assert.Equal(face, move.Face);
        Assert.Equal(direction, move.Direction);
    }
    
    [Theory]
    [InlineData("M", 'M', MoveDirection.Clockwise)]
    [InlineData("M'", 'M', MoveDirection.CounterClockwise)]
    [InlineData("M2", 'M', MoveDirection.Double)]
    [InlineData("E", 'E', MoveDirection.Clockwise)]
    [InlineData("E'", 'E', MoveDirection.CounterClockwise)]
    [InlineData("E2", 'E', MoveDirection.Double)]
    [InlineData("S", 'S', MoveDirection.Clockwise)]
    [InlineData("S'", 'S', MoveDirection.CounterClockwise)]
    [InlineData("S2", 'S', MoveDirection.Double)]
    public void Parse_ValidMiddleSliceMoveStrings_ShouldParseCorrectly(string moveString, char expectedFace, MoveDirection expectedDirection)
    {
        var move = Move.Parse(moveString);
        
        Assert.Equal(expectedFace, move.Face);
        Assert.Equal(expectedDirection, move.Direction);
    }
    
    [Theory]
    [InlineData("M", "M'")]
    [InlineData("M'", "M")]
    [InlineData("M2", "M2")]
    [InlineData("E", "E'")]
    [InlineData("E'", "E")]
    [InlineData("E2", "E2")]
    [InlineData("S", "S'")]
    [InlineData("S'", "S")]
    [InlineData("S2", "S2")]
    public void Inverse_AllMiddleSliceMoves_ShouldReturnCorrectInverse(string originalMove, string expectedInverse)
    {
        var move = Move.Parse(originalMove);
        var inverse = move.Inverse();
        
        Assert.Equal(move.Face, inverse.Face);
        Assert.Equal(expectedInverse, inverse.ToString());
    }
    
    [Fact]
    public void MMove_ShouldAffectMiddleSliceBetweenLAndR()
    {
        // M move affects pieces with X=0 (between L face X=-1 and R face X=1)
        var cube = Cube.CreateSolved();
        
        // Track specific pieces that should move with M rotation
        // Edge piece at (0, 1, 1) - top-front edge of middle slice
        var topFrontEdge = cube.Pieces.First(p => 
            p.Position.X == 0 && p.Position.Y == 1 && p.Position.Z == 1 && p.Type == PieceType.Edge);
        
        var originalPosition = topFrontEdge.Position;
        var originalColors = topFrontEdge.Colors.ToArray();
        
        // Apply M move
        cube.ApplyMove(new Move('M'));
        
        // The piece should have moved to a new position
        var movedPiece = cube.Pieces.First(p => p.SolvedPosition.Equals(topFrontEdge.SolvedPosition));
        Assert.NotEqual(originalPosition, movedPiece.Position);
        
        // Verify the cube is still in a valid state
        Assert.True(cube.IsValidState());
        Assert.Equal(26, cube.Pieces.Count);
    }
    
    [Fact]
    public void EMove_ShouldAffectMiddleSliceBetweenUAndD()
    {
        // E move affects pieces with Y=0 (between U face Y=1 and D face Y=-1)
        var cube = Cube.CreateSolved();
        
        // Track specific pieces that should move with E rotation
        // Edge piece at (1, 0, 1) - right-front edge of middle slice
        var rightFrontEdge = cube.Pieces.First(p => 
            p.Position.X == 1 && p.Position.Y == 0 && p.Position.Z == 1 && p.Type == PieceType.Edge);
        
        var originalPosition = rightFrontEdge.Position;
        
        // Apply E move
        cube.ApplyMove(new Move('E'));
        
        // The piece should have moved to a new position
        var movedPiece = cube.Pieces.First(p => p.SolvedPosition.Equals(rightFrontEdge.SolvedPosition));
        Assert.NotEqual(originalPosition, movedPiece.Position);
        
        // Verify the cube is still in a valid state
        Assert.True(cube.IsValidState());
        Assert.Equal(26, cube.Pieces.Count);
    }
    
    [Fact]
    public void SMove_ShouldAffectMiddleSliceBetweenFAndB()
    {
        // S move affects pieces with Z=0 (between F face Z=1 and B face Z=-1)
        var cube = Cube.CreateSolved();
        
        // Track specific pieces that should move with S rotation
        // Edge piece at (1, 1, 0) - right-top edge of middle slice
        var rightTopEdge = cube.Pieces.First(p => 
            p.Position.X == 1 && p.Position.Y == 1 && p.Position.Z == 0 && p.Type == PieceType.Edge);
        
        var originalPosition = rightTopEdge.Position;
        
        // Apply S move
        cube.ApplyMove(new Move('S'));
        
        // The piece should have moved to a new position
        var movedPiece = cube.Pieces.First(p => p.SolvedPosition.Equals(rightTopEdge.SolvedPosition));
        Assert.NotEqual(originalPosition, movedPiece.Position);
        
        // Verify the cube is still in a valid state
        Assert.True(cube.IsValidState());
        Assert.Equal(26, cube.Pieces.Count);
    }
    
    [Theory]
    [InlineData('M')]
    [InlineData('E')]
    [InlineData('S')]
    public void FourMiddleSliceMoves_ShouldReturnToIdentity(char moveChar)
    {
        // Mathematical property: 4 × 90° = 360° = identity
        var cube = Cube.CreateSolved();
        var originalState = cube.Pieces.Select(p => new { p.SolvedPosition, p.Position, Colors = p.Colors.ToArray() }).ToList();
        
        var move = new Move(moveChar);
        
        // Apply same move 4 times
        for (int i = 0; i < 4; i++)
        {
            cube.ApplyMove(move);
        }
        
        // Should be back to original state
        foreach (var original in originalState)
        {
            var current = cube.Pieces.First(p => p.SolvedPosition.Equals(original.SolvedPosition));
            Assert.Equal(original.Position, current.Position);
            Assert.Equal(original.Colors, current.Colors);
        }
    }
    
    [Theory]
    [InlineData('M')]
    [InlineData('E')]
    [InlineData('S')]
    public void MiddleSliceMoveInverse_ShouldReturnToIdentity(char moveChar)
    {
        // Mathematical property: Move × Move' = Identity
        var cube = Cube.CreateSolved();
        var originalCount = cube.Pieces.Count(p => p.Position.Equals(p.SolvedPosition));
        Assert.Equal(26, originalCount); // All solved
        
        var move = new Move(moveChar, MoveDirection.Clockwise);
        var inverse = new Move(moveChar, MoveDirection.CounterClockwise);
        
        cube.ApplyMove(move);
        cube.ApplyMove(inverse);
        
        var finalCount = cube.Pieces.Count(p => p.Position.Equals(p.SolvedPosition));
        Assert.Equal(26, finalCount); // All back to solved
    }
    
    [Fact]
    public void MiddleSliceMoves_ShouldNotAffectCenterPieces()
    {
        // Center pieces should never move, regardless of the rotation
        var cube = Cube.CreateSolved();
        var centers = cube.Centers.ToList();
        
        // Store original center positions and colors
        var originalCenters = centers.ToDictionary(
            c => c.SolvedPosition,
            c => new { Position = c.Position, Colors = c.Colors.ToArray() }
        );
        
        // Apply various middle slice moves
        var moves = new[] { 'M', 'E', 'S' };
        foreach (var moveChar in moves)
        {
            var testCube = Cube.CreateSolved();
            var move = new Move(moveChar);
            
            testCube.ApplyMove(move);
            
            // Check all centers remain unchanged
            foreach (var center in testCube.Centers)
            {
                var original = originalCenters[center.SolvedPosition];
                
                // Center should not move
                Assert.Equal(original.Position, center.Position);
                
                // Center colors should not change
                Assert.Equal(original.Colors, center.Colors);
            }
        }
    }
    
    [Fact]
    public void Algorithm_MixedMiddleSliceSequence_ShouldParseCorrectly()
    {
        // Test parsing complex sequences with middle slice moves
        var algorithm = Algorithm.Parse("M E S M' E' S'").Value;
        
        Assert.Equal(6, algorithm.Length);
        
        // Verify each move is parsed correctly
        Assert.Equal("M", algorithm.Moves[0].ToString());
        Assert.Equal("E", algorithm.Moves[1].ToString());
        Assert.Equal("S", algorithm.Moves[2].ToString());
        Assert.Equal("M'", algorithm.Moves[3].ToString());
        Assert.Equal("E'", algorithm.Moves[4].ToString());
        Assert.Equal("S'", algorithm.Moves[5].ToString());
    }
    
    [Theory]
    [InlineData("M E M' E'", "Test middle slice commutator")]
    [InlineData("S M S' M'", "Another middle slice sequence")]
    [InlineData("M2 E2 S2", "Double middle slice moves")]
    public void RealWorldMiddleSliceAlgorithms_ShouldParseWithCorrectMoves(string algorithm, string description)
    {
        var result = Algorithm.Parse(algorithm);
        
        Assert.True(result.IsSuccess, $"Failed to parse {description}: {(result.IsFailure ? result.Error : "N/A")}");
        Assert.Equal(algorithm, result.Value.ToString());
    }
}