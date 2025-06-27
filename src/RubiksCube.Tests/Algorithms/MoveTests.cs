using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.Algorithms;

public class MoveTests
{
    [Theory]
    [InlineData('R')]
    [InlineData('L')]
    [InlineData('U')]
    [InlineData('D')]
    [InlineData('F')]
    [InlineData('B')]
    [InlineData('x')]
    [InlineData('y')]
    [InlineData('z')]
    public void Constructor_ValidFaces_ShouldCreateMove(char face)
    {
        var move = new Move(face);
        
        Assert.Equal(face, move.Face);
        Assert.Equal(MoveDirection.Clockwise, move.Direction);
    }
    
    [Theory]
    [InlineData('R', MoveDirection.Clockwise)]
    [InlineData('R', MoveDirection.CounterClockwise)]
    [InlineData('R', MoveDirection.Double)]
    [InlineData('x', MoveDirection.Clockwise)]
    [InlineData('x', MoveDirection.CounterClockwise)]
    [InlineData('x', MoveDirection.Double)]
    public void Constructor_AllDirections_ShouldSetCorrectly(char face, MoveDirection direction)
    {
        var move = new Move(face, direction);
        
        Assert.Equal(face, move.Face);
        Assert.Equal(direction, move.Direction);
    }
    
    [Theory]
    [InlineData('A')]
    [InlineData('M')]
    [InlineData('E')]
    [InlineData('S')]
    [InlineData('1')]
    [InlineData(' ')]
    public void Constructor_InvalidFaces_ShouldThrow(char invalidFace)
    {
        Assert.Throws<ArgumentException>(() => new Move(invalidFace));
    }
    
    [Theory]
    [InlineData("R", 'R', MoveDirection.Clockwise)]
    [InlineData("R'", 'R', MoveDirection.CounterClockwise)]
    [InlineData("R2", 'R', MoveDirection.Double)]
    [InlineData("U", 'U', MoveDirection.Clockwise)]
    [InlineData("U'", 'U', MoveDirection.CounterClockwise)]
    [InlineData("U2", 'U', MoveDirection.Double)]
    [InlineData("x", 'x', MoveDirection.Clockwise)]
    [InlineData("x'", 'x', MoveDirection.CounterClockwise)]
    [InlineData("x2", 'x', MoveDirection.Double)]
    [InlineData("  R  ", 'R', MoveDirection.Clockwise)] // Whitespace handling
    [InlineData("r", 'r', MoveDirection.Clockwise)] // Case sensitive
    public void Parse_ValidMoveStrings_ShouldParseCorrectly(string moveString, char expectedFace, MoveDirection expectedDirection)
    {
        var move = Move.Parse(moveString);
        
        Assert.Equal(expectedFace, move.Face);
        Assert.Equal(expectedDirection, move.Direction);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("R3")]
    [InlineData("R''")]
    [InlineData("RR")]
    [InlineData("A")]
    [InlineData("R2'")]
    public void Parse_InvalidMoveStrings_ShouldThrow(string invalidMoveString)
    {
        Assert.Throws<ArgumentException>(() => Move.Parse(invalidMoveString));
    }
    
    [Theory]
    [InlineData("R", MoveDirection.Clockwise, "R'", MoveDirection.CounterClockwise)]
    [InlineData("R'", MoveDirection.CounterClockwise, "R", MoveDirection.Clockwise)]
    [InlineData("R2", MoveDirection.Double, "R2", MoveDirection.Double)]
    [InlineData("x", MoveDirection.Clockwise, "x'", MoveDirection.CounterClockwise)]
    [InlineData("x'", MoveDirection.CounterClockwise, "x", MoveDirection.Clockwise)]
    [InlineData("x2", MoveDirection.Double, "x2", MoveDirection.Double)]
    public void Inverse_AllDirections_ShouldReturnCorrectInverse(string originalMove, MoveDirection _, string expectedInverse, MoveDirection expectedDirection)
    {
        var move = Move.Parse(originalMove);
        var inverse = move.Inverse();
        
        Assert.Equal(move.Face, inverse.Face);
        Assert.Equal(expectedDirection, inverse.Direction);
        Assert.Equal(expectedInverse, inverse.ToString());
    }
    
    [Theory]
    [InlineData('R', MoveDirection.Clockwise, "R")]
    [InlineData('R', MoveDirection.CounterClockwise, "R'")]
    [InlineData('R', MoveDirection.Double, "R2")]
    [InlineData('U', MoveDirection.Clockwise, "U")]
    [InlineData('U', MoveDirection.CounterClockwise, "U'")]
    [InlineData('U', MoveDirection.Double, "U2")]
    [InlineData('x', MoveDirection.Clockwise, "x")]
    [InlineData('x', MoveDirection.CounterClockwise, "x'")]
    [InlineData('x', MoveDirection.Double, "x2")]
    public void ToString_AllCombinations_ShouldFormatCorrectly(char face, MoveDirection direction, string expectedString)
    {
        var move = new Move(face, direction);
        Assert.Equal(expectedString, move.ToString());
    }
    
    [Fact]
    public void Equality_SameMoves_ShouldBeEqual()
    {
        var move1 = new Move('R', MoveDirection.Clockwise);
        var move2 = new Move('R', MoveDirection.Clockwise);
        var move3 = Move.Parse("R");
        
        Assert.Equal(move1, move2);
        Assert.Equal(move1, move3);
        Assert.True(move1 == move2);
        Assert.False(move1 != move2);
    }
    
    [Fact]
    public void Equality_DifferentMoves_ShouldNotBeEqual()
    {
        var moveR = new Move('R', MoveDirection.Clockwise);
        var moveRPrime = new Move('R', MoveDirection.CounterClockwise);
        var moveU = new Move('U', MoveDirection.Clockwise);
        
        Assert.NotEqual(moveR, moveRPrime);
        Assert.NotEqual(moveR, moveU);
        Assert.False(moveR == moveRPrime);
        Assert.True(moveR != moveRPrime);
    }
    
    [Fact]
    public void GetHashCode_SameMoves_ShouldHaveSameHashCode()
    {
        var move1 = new Move('R', MoveDirection.Clockwise);
        var move2 = new Move('R', MoveDirection.Clockwise);
        
        Assert.Equal(move1.GetHashCode(), move2.GetHashCode());
    }
    
    [Theory]
    [InlineData('R', false)]  // Single layer
    [InlineData('r', true)]   // Wide move
    [InlineData('U', false)]  // Single layer
    [InlineData('u', true)]   // Wide move
    [InlineData('x', false)]  // Reorientation (not wide)
    public void IsWide_VariousMoves_ShouldDetectCorrectly(char face, bool expectedIsWide)
    {
        var move = new Move(face);
        Assert.Equal(expectedIsWide, move.IsWide);
    }
    
    [Theory]
    [InlineData("r", 'r', MoveDirection.Clockwise, true)]
    [InlineData("r'", 'r', MoveDirection.CounterClockwise, true)]
    [InlineData("u2", 'u', MoveDirection.Double, true)]
    [InlineData("R", 'R', MoveDirection.Clockwise, false)]
    public void Parse_WideMoves_ShouldParseCorrectly(string moveString, char expectedFace, MoveDirection expectedDirection, bool expectedIsWide)
    {
        var move = Move.Parse(moveString);
        
        Assert.Equal(expectedFace, move.Face);
        Assert.Equal(expectedDirection, move.Direction);
        Assert.Equal(expectedIsWide, move.IsWide);
        // Note: Move type distinction has been removed in v3.0
    }
}