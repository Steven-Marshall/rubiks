using RubiksCube.Core.Models;

namespace RubiksCube.Tests.Models;

public class CubePieceV3Tests
{
    [Fact]
    public void Constructor_CornerPiece_ShouldSetCorrectType()
    {
        var position = new Position3D(1, 1, 1);
        var colors = new CubeColor?[] { CubeColor.Orange, CubeColor.Yellow, CubeColor.Green };
        
        var piece = new CubePiece(position, colors);
        
        Assert.Equal(PieceType.Corner, piece.Type);
        Assert.Equal(3, piece.Colors.Count(c => c != null));
    }
    
    [Fact]
    public void Constructor_EdgePiece_ShouldSetCorrectType()
    {
        var position = new Position3D(1, 0, 1);
        var colors = new CubeColor?[] { CubeColor.Orange, null, CubeColor.Green };
        
        var piece = new CubePiece(position, colors);
        
        Assert.Equal(PieceType.Edge, piece.Type);
        Assert.Equal(2, piece.Colors.Count(c => c != null));
    }
    
    [Fact]
    public void Constructor_CenterPiece_ShouldSetCorrectType()
    {
        var position = new Position3D(1, 0, 0);
        var colors = new CubeColor?[] { CubeColor.Orange, null, null };
        
        var piece = new CubePiece(position, colors);
        
        Assert.Equal(PieceType.Center, piece.Type);
        Assert.Equal(1, piece.Colors.Count(c => c != null));
    }
    
    [Theory]
    [InlineData(0, 0, 0)] // Invalid - center with no colors
    [InlineData(4, 0, 0)] // Invalid - too many colors
    public void Constructor_InvalidColorCount_ShouldThrow(int xColor, int yColor, int zColor)
    {
        var position = new Position3D(1, 1, 1);
        var colors = new CubeColor?[] 
        { 
            xColor > 0 ? CubeColor.Red : null,
            yColor > 0 ? CubeColor.Yellow : null,
            zColor > 0 ? CubeColor.Green : null
        };
        
        Assert.Throws<ArgumentException>(() => new CubePiece(position, colors));
    }
    
    [Fact]
    public void Colors_ShouldAlwaysHaveThreeElements()
    {
        var position = new Position3D(1, 1, 1);
        var colors = new CubeColor?[] { CubeColor.Orange, CubeColor.Yellow, CubeColor.Green };
        
        var piece = new CubePiece(position, colors);
        
        Assert.Equal(3, piece.Colors.Length);
    }
    
    [Fact]
    public void Equality_SamePieces_ShouldBeEqual()
    {
        var position1 = new Position3D(1, 1, 1);
        var position2 = new Position3D(0, 0, 0);
        var colors = new CubeColor?[] { CubeColor.Orange, CubeColor.Yellow, CubeColor.Green };
        
        var piece1 = new CubePiece(position1, colors);
        var piece2 = new CubePiece(position1, colors);
        
        Assert.Equal(piece1, piece2);
    }
    
    [Fact]
    public void Equality_DifferentPositions_ShouldNotBeEqual()
    {
        var position1 = new Position3D(1, 1, 1);
        var position2 = new Position3D(1, 1, -1);
        var colors = new CubeColor?[] { CubeColor.Orange, CubeColor.Yellow, CubeColor.Green };
        
        var piece1 = new CubePiece(position1, colors);
        var piece2 = new CubePiece(position2, colors);
        
        Assert.NotEqual(piece1, piece2);
    }
    
    [Fact]
    public void Equality_DifferentColors_ShouldNotBeEqual()
    {
        var position = new Position3D(1, 1, 1);
        var colors1 = new CubeColor?[] { CubeColor.Orange, CubeColor.Yellow, CubeColor.Green };
        var colors2 = new CubeColor?[] { CubeColor.Red, CubeColor.Yellow, CubeColor.Green };
        
        var piece1 = new CubePiece(position, colors1);
        var piece2 = new CubePiece(position, colors2);
        
        Assert.NotEqual(piece1, piece2);
    }
    
    [Fact]
    public void ToString_ShouldIncludeTypeAndPosition()
    {
        var position = new Position3D(1, 1, 1);
        var colors = new CubeColor?[] { CubeColor.Orange, CubeColor.Yellow, CubeColor.Green };
        
        var piece = new CubePiece(position, colors);
        var str = piece.ToString();
        
        Assert.Contains("Corner", str);
        Assert.Contains("(1, 1, 1)", str);
    }
    
    [Theory]
    [InlineData(1, 1, 1, PieceType.Corner)]  // Corner
    [InlineData(1, 0, 1, PieceType.Edge)]    // Edge
    [InlineData(1, 0, 0, PieceType.Center)]  // Center
    [InlineData(-1, -1, -1, PieceType.Corner)] // Corner
    [InlineData(0, 1, 0, PieceType.Center)]  // Center
    public void TypeDetection_ShouldWorkForAllValidPositions(int x, int y, int z, PieceType expectedType)
    {
        var position = new Position3D(x, y, z);
        CubeColor?[] colors;
        
        switch (expectedType)
        {
            case PieceType.Corner:
                colors = new CubeColor?[] { CubeColor.Red, CubeColor.Yellow, CubeColor.Green };
                break;
            case PieceType.Edge:
                colors = new CubeColor?[] 
                { 
                    x != 0 ? CubeColor.Red : null,
                    y != 0 ? CubeColor.Yellow : null,
                    z != 0 ? CubeColor.Green : null
                };
                break;
            case PieceType.Center:
                colors = new CubeColor?[] 
                { 
                    x != 0 ? CubeColor.Red : null,
                    y != 0 ? CubeColor.Yellow : null,
                    z != 0 ? CubeColor.Green : null
                };
                break;
            default:
                throw new ArgumentException("Invalid piece type");
        }
        
        var piece = new CubePiece(position, colors);
        
        Assert.Equal(expectedType, piece.Type);
    }
}