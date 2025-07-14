using RubiksCube.Core.Models;
using RubiksCube.Core.Algorithms;

namespace RubiksCube.Tests.Models;

/// <summary>
/// Tests that cube rotations (x, y, z) follow the specification documented in CLAUDE.md
/// These tests validate the DIRECTION of rotations, not just mathematical correctness
/// </summary>
public class CubeRotationSpecificationTests
{
    /// <summary>
    /// Helper method to find a center piece by its color
    /// </summary>
    private static CubePiece GetCenterByColor(Cube cube, CubeColor color)
    {
        return cube.Centers.First(c => c.Colors.Contains(color));
    }

    /// <summary>
    /// Helper method to check if two cubes have identical piece positions and colors
    /// </summary>
    private static bool CubesAreEqual(Cube cube1, Cube cube2)
    {
        if (cube1.Pieces.Count != cube2.Pieces.Count)
            return false;

        foreach (var piece1 in cube1.Pieces)
        {
            var piece2 = cube2.Pieces.FirstOrDefault(p => p.SolvedPosition == piece1.SolvedPosition);
            if (piece2 == null || piece1.Position != piece2.Position || !piece1.Colors.SequenceEqual(piece2.Colors))
                return false;
        }
        return true;
    }

    #region X Rotation Tests

    [Fact]
    public void XRotation_ShouldFollowSpecification()
    {
        // CLAUDE.md specification: X Rotation (clockwise looking from right side):
        // Front→Up, Up→Back, Back→Down, Down→Front
        // Example: Green→Up, Yellow→Back, Blue→Down, White→Front
        
        var cube = Cube.CreateSolved();
        
        // Get initial center positions
        var greenCenter = GetCenterByColor(cube, CubeColor.Green);   // Front
        var whiteCenter = GetCenterByColor(cube, CubeColor.White);   // Bottom
        var yellowCenter = GetCenterByColor(cube, CubeColor.Yellow); // Top
        var blueCenter = GetCenterByColor(cube, CubeColor.Blue);     // Back
        var redCenter = GetCenterByColor(cube, CubeColor.Red);       // Left (should not move)
        var orangeCenter = GetCenterByColor(cube, CubeColor.Orange); // Right (should not move)
        
        // Verify initial solved state
        Assert.Equal(new Position3D(0, 0, 1), greenCenter.Position);   // Green at front (Z+1)
        Assert.Equal(new Position3D(0, -1, 0), whiteCenter.Position);  // White at bottom (Y-1)
        Assert.Equal(new Position3D(0, 1, 0), yellowCenter.Position);  // Yellow at top (Y+1)
        Assert.Equal(new Position3D(0, 0, -1), blueCenter.Position);   // Blue at back (Z-1)
        Assert.Equal(new Position3D(-1, 0, 0), redCenter.Position);    // Red at left (X-1)
        Assert.Equal(new Position3D(1, 0, 0), orangeCenter.Position);  // Orange at right (X+1)
        
        // Apply X rotation
        cube.ApplyMove(new Move('x'));
        
        // Re-get centers after rotation (references may have changed)
        greenCenter = GetCenterByColor(cube, CubeColor.Green);
        whiteCenter = GetCenterByColor(cube, CubeColor.White);
        yellowCenter = GetCenterByColor(cube, CubeColor.Yellow);
        blueCenter = GetCenterByColor(cube, CubeColor.Blue);
        redCenter = GetCenterByColor(cube, CubeColor.Red);
        orangeCenter = GetCenterByColor(cube, CubeColor.Orange);
        
        // After X rotation: Front→Up, Up→Back, Back→Down, Down→Front
        // Green (front) should move to up position
        Assert.Equal(new Position3D(0, 1, 0), greenCenter.Position);   // Green moved to top
        // Yellow (top) should move to back position  
        Assert.Equal(new Position3D(0, 0, -1), yellowCenter.Position); // Yellow moved to back
        // Blue (back) should move to down position
        Assert.Equal(new Position3D(0, -1, 0), blueCenter.Position);   // Blue moved to bottom
        // White (bottom) should move to front position
        Assert.Equal(new Position3D(0, 0, 1), whiteCenter.Position);   // White moved to front
        
        // Left and Right centers should not move (they're on the X-axis)
        Assert.Equal(new Position3D(-1, 0, 0), redCenter.Position);    // Red stays at left
        Assert.Equal(new Position3D(1, 0, 0), orangeCenter.Position);  // Orange stays at right
    }

    [Fact]
    public void XPrimeRotation_ShouldFollowSpecification()
    {
        // X' rotation should be counter-clockwise (opposite of X)
        // Front→Down, Down→Back, Back→Up, Up→Front
        // Example: Green→Down, White→Back, Blue→Up, Yellow→Front
        
        var cube = Cube.CreateSolved();
        
        var greenCenter = GetCenterByColor(cube, CubeColor.Green);   // Front
        var whiteCenter = GetCenterByColor(cube, CubeColor.White);   // Bottom  
        var yellowCenter = GetCenterByColor(cube, CubeColor.Yellow); // Top
        var blueCenter = GetCenterByColor(cube, CubeColor.Blue);     // Back
        
        // Apply X' rotation
        cube.ApplyMove(new Move('x', MoveDirection.CounterClockwise));
        
        // Re-get centers after rotation
        greenCenter = GetCenterByColor(cube, CubeColor.Green);
        whiteCenter = GetCenterByColor(cube, CubeColor.White);
        yellowCenter = GetCenterByColor(cube, CubeColor.Yellow);
        blueCenter = GetCenterByColor(cube, CubeColor.Blue);
        
        // After X' rotation: Front→Down, Down→Back, Back→Up, Up→Front
        Assert.Equal(new Position3D(0, -1, 0), greenCenter.Position);  // Green moved to bottom
        Assert.Equal(new Position3D(0, 0, -1), whiteCenter.Position);  // White moved to back
        Assert.Equal(new Position3D(0, 1, 0), blueCenter.Position);    // Blue moved to top
        Assert.Equal(new Position3D(0, 0, 1), yellowCenter.Position);  // Yellow moved to front
    }

    #endregion

    #region Y Rotation Tests

    [Fact]
    public void YRotation_ShouldFollowSpecification()
    {
        // CLAUDE.md specification: Y Rotation (clockwise looking down from above):
        // Front→Left, Left→Back, Back→Right, Right→Front
        // Example: Green→Left, Red→Back, Blue→Right, Orange→Front
        
        var cube = Cube.CreateSolved();
        
        var greenCenter = GetCenterByColor(cube, CubeColor.Green);   // Front
        var redCenter = GetCenterByColor(cube, CubeColor.Red);       // Left
        var blueCenter = GetCenterByColor(cube, CubeColor.Blue);     // Back
        var orangeCenter = GetCenterByColor(cube, CubeColor.Orange); // Right
        var whiteCenter = GetCenterByColor(cube, CubeColor.White);   // Bottom (should not move)
        var yellowCenter = GetCenterByColor(cube, CubeColor.Yellow); // Top (should not move)
        
        // Apply Y rotation
        cube.ApplyMove(new Move('y'));
        
        // Re-get centers after rotation
        greenCenter = GetCenterByColor(cube, CubeColor.Green);
        redCenter = GetCenterByColor(cube, CubeColor.Red);
        blueCenter = GetCenterByColor(cube, CubeColor.Blue);
        orangeCenter = GetCenterByColor(cube, CubeColor.Orange);
        whiteCenter = GetCenterByColor(cube, CubeColor.White);
        yellowCenter = GetCenterByColor(cube, CubeColor.Yellow);
        
        // After Y rotation: Front→Left, Left→Back, Back→Right, Right→Front
        Assert.Equal(new Position3D(-1, 0, 0), greenCenter.Position);  // Green moved to left
        Assert.Equal(new Position3D(0, 0, -1), redCenter.Position);    // Red moved to back
        Assert.Equal(new Position3D(1, 0, 0), blueCenter.Position);    // Blue moved to right
        Assert.Equal(new Position3D(0, 0, 1), orangeCenter.Position);  // Orange moved to front
        
        // Top and Bottom centers should not move (they're on the Y-axis)
        Assert.Equal(new Position3D(0, -1, 0), whiteCenter.Position);  // White stays at bottom
        Assert.Equal(new Position3D(0, 1, 0), yellowCenter.Position);  // Yellow stays at top
    }

    [Fact]
    public void YPrimeRotation_ShouldFollowSpecification()
    {
        // Y' rotation should be counter-clockwise (opposite of Y)
        // Front→Right, Right→Back, Back→Left, Left→Front
        
        var cube = Cube.CreateSolved();
        
        var greenCenter = GetCenterByColor(cube, CubeColor.Green);   // Front
        var redCenter = GetCenterByColor(cube, CubeColor.Red);       // Left
        var blueCenter = GetCenterByColor(cube, CubeColor.Blue);     // Back
        var orangeCenter = GetCenterByColor(cube, CubeColor.Orange); // Right
        
        // Apply Y' rotation
        cube.ApplyMove(new Move('y', MoveDirection.CounterClockwise));
        
        // Re-get centers after rotation
        greenCenter = GetCenterByColor(cube, CubeColor.Green);
        redCenter = GetCenterByColor(cube, CubeColor.Red);
        blueCenter = GetCenterByColor(cube, CubeColor.Blue);
        orangeCenter = GetCenterByColor(cube, CubeColor.Orange);
        
        // After Y' rotation: Front→Right, Right→Back, Back→Left, Left→Front
        Assert.Equal(new Position3D(1, 0, 0), greenCenter.Position);   // Green moved to right
        Assert.Equal(new Position3D(0, 0, 1), redCenter.Position);     // Red moved to front
        Assert.Equal(new Position3D(-1, 0, 0), blueCenter.Position);   // Blue moved to left
        Assert.Equal(new Position3D(0, 0, -1), orangeCenter.Position); // Orange moved to back
    }

    #endregion

    #region Z Rotation Tests

    [Fact]
    public void ZRotation_ShouldFollowSpecification()
    {
        // CLAUDE.md specification: Z Rotation (clockwise looking at front face):
        // Up→Right, Right→Down, Down→Left, Left→Up
        // Example: Yellow→Right, Orange→Down, White→Left, Red→Up
        
        var cube = Cube.CreateSolved();
        
        var yellowCenter = GetCenterByColor(cube, CubeColor.Yellow); // Top
        var orangeCenter = GetCenterByColor(cube, CubeColor.Orange); // Right
        var whiteCenter = GetCenterByColor(cube, CubeColor.White);   // Bottom
        var redCenter = GetCenterByColor(cube, CubeColor.Red);       // Left
        var greenCenter = GetCenterByColor(cube, CubeColor.Green);   // Front (should not move)
        var blueCenter = GetCenterByColor(cube, CubeColor.Blue);     // Back (should not move)
        
        // Apply Z rotation
        cube.ApplyMove(new Move('z'));
        
        // Re-get centers after rotation
        yellowCenter = GetCenterByColor(cube, CubeColor.Yellow);
        orangeCenter = GetCenterByColor(cube, CubeColor.Orange);
        whiteCenter = GetCenterByColor(cube, CubeColor.White);
        redCenter = GetCenterByColor(cube, CubeColor.Red);
        greenCenter = GetCenterByColor(cube, CubeColor.Green);
        blueCenter = GetCenterByColor(cube, CubeColor.Blue);
        
        // After Z rotation: Up→Right, Right→Down, Down→Left, Left→Up
        Assert.Equal(new Position3D(1, 0, 0), yellowCenter.Position);  // Yellow moved to right
        Assert.Equal(new Position3D(0, -1, 0), orangeCenter.Position); // Orange moved to bottom
        Assert.Equal(new Position3D(-1, 0, 0), whiteCenter.Position);  // White moved to left
        Assert.Equal(new Position3D(0, 1, 0), redCenter.Position);     // Red moved to top
        
        // Front and Back centers should not move (they're on the Z-axis)
        Assert.Equal(new Position3D(0, 0, 1), greenCenter.Position);   // Green stays at front
        Assert.Equal(new Position3D(0, 0, -1), blueCenter.Position);   // Blue stays at back
    }

    [Fact]
    public void ZPrimeRotation_ShouldFollowSpecification()
    {
        // Z' rotation should be counter-clockwise (opposite of Z)
        // Up→Left, Left→Down, Down→Right, Right→Up
        
        var cube = Cube.CreateSolved();
        
        var yellowCenter = GetCenterByColor(cube, CubeColor.Yellow); // Top
        var orangeCenter = GetCenterByColor(cube, CubeColor.Orange); // Right
        var whiteCenter = GetCenterByColor(cube, CubeColor.White);   // Bottom
        var redCenter = GetCenterByColor(cube, CubeColor.Red);       // Left
        
        // Apply Z' rotation
        cube.ApplyMove(new Move('z', MoveDirection.CounterClockwise));
        
        // Re-get centers after rotation
        yellowCenter = GetCenterByColor(cube, CubeColor.Yellow);
        orangeCenter = GetCenterByColor(cube, CubeColor.Orange);
        whiteCenter = GetCenterByColor(cube, CubeColor.White);
        redCenter = GetCenterByColor(cube, CubeColor.Red);
        
        // After Z' rotation: Up→Left, Left→Down, Down→Right, Right→Up
        Assert.Equal(new Position3D(-1, 0, 0), yellowCenter.Position); // Yellow moved to left
        Assert.Equal(new Position3D(0, 1, 0), orangeCenter.Position);  // Orange moved to top
        Assert.Equal(new Position3D(1, 0, 0), whiteCenter.Position);   // White moved to right
        Assert.Equal(new Position3D(0, -1, 0), redCenter.Position);    // Red moved to bottom
    }

    #endregion

    #region Inverse Relationship Tests

    [Fact]
    public void XAndXPrime_ShouldBeInverses()
    {
        var cube = Cube.CreateSolved();
        var originalCube = cube.Clone();
        
        // Apply X then X'
        cube.ApplyMove(new Move('x'));
        cube.ApplyMove(new Move('x', MoveDirection.CounterClockwise));
        
        // Should return to original state
        Assert.True(CubesAreEqual(cube, originalCube));
    }

    [Fact]
    public void YAndYPrime_ShouldBeInverses()
    {
        var cube = Cube.CreateSolved();
        var originalCube = cube.Clone();
        
        // Apply Y then Y'
        cube.ApplyMove(new Move('y'));
        cube.ApplyMove(new Move('y', MoveDirection.CounterClockwise));
        
        // Should return to original state
        Assert.True(CubesAreEqual(cube, originalCube));
    }

    [Fact]
    public void ZAndZPrime_ShouldBeInverses()
    {
        var cube = Cube.CreateSolved();
        var originalCube = cube.Clone();
        
        // Apply Z then Z'
        cube.ApplyMove(new Move('z'));
        cube.ApplyMove(new Move('z', MoveDirection.CounterClockwise));
        
        // Should return to original state
        Assert.True(CubesAreEqual(cube, originalCube));
    }

    [Fact]
    public void FourRotations_ShouldReturnToOriginal()
    {
        // Four 90-degree rotations around any axis should return to original position
        
        var cube = Cube.CreateSolved();
        var originalCube = cube.Clone();
        
        // Test X rotations
        cube.ApplyMove(new Move('x'));
        cube.ApplyMove(new Move('x'));
        cube.ApplyMove(new Move('x'));
        cube.ApplyMove(new Move('x'));
        Assert.True(CubesAreEqual(cube, originalCube));
        
        // Test Y rotations
        cube.ApplyMove(new Move('y'));
        cube.ApplyMove(new Move('y'));
        cube.ApplyMove(new Move('y'));
        cube.ApplyMove(new Move('y'));
        Assert.True(CubesAreEqual(cube, originalCube));
        
        // Test Z rotations
        cube.ApplyMove(new Move('z'));
        cube.ApplyMove(new Move('z'));
        cube.ApplyMove(new Move('z'));
        cube.ApplyMove(new Move('z'));
        Assert.True(CubesAreEqual(cube, originalCube));
    }

    #endregion

    #region Cross-Axis Validation Tests

    [Fact]
    public void XRotation_ShouldOnlyMoveYZPlane()
    {
        // X rotation should only move pieces that are not on the X-axis
        var cube = Cube.CreateSolved();
        
        // Get pieces on X-axis (should not move)
        var xAxisPieces = cube.Pieces.Where(p => p.Position.Y == 0 && p.Position.Z == 0).ToList();
        var originalXPositions = xAxisPieces.ToDictionary(p => p.SolvedPosition, p => p.Position);
        
        cube.ApplyMove(new Move('x'));
        
        // Verify X-axis pieces didn't move
        foreach (var piece in xAxisPieces)
        {
            Assert.Equal(originalXPositions[piece.SolvedPosition], piece.Position);
        }
    }

    [Fact]
    public void YRotation_ShouldOnlyMoveXZPlane()
    {
        // Y rotation should only move pieces that are not on the Y-axis
        var cube = Cube.CreateSolved();
        
        // Get pieces on Y-axis (should not move)
        var yAxisPieces = cube.Pieces.Where(p => p.Position.X == 0 && p.Position.Z == 0).ToList();
        var originalYPositions = yAxisPieces.ToDictionary(p => p.SolvedPosition, p => p.Position);
        
        cube.ApplyMove(new Move('y'));
        
        // Verify Y-axis pieces didn't move
        foreach (var piece in yAxisPieces)
        {
            Assert.Equal(originalYPositions[piece.SolvedPosition], piece.Position);
        }
    }

    [Fact]
    public void ZRotation_ShouldOnlyMoveXYPlane()
    {
        // Z rotation should only move pieces that are not on the Z-axis
        var cube = Cube.CreateSolved();
        
        // Get pieces on Z-axis (should not move)
        var zAxisPieces = cube.Pieces.Where(p => p.Position.X == 0 && p.Position.Y == 0).ToList();
        var originalZPositions = zAxisPieces.ToDictionary(p => p.SolvedPosition, p => p.Position);
        
        cube.ApplyMove(new Move('z'));
        
        // Verify Z-axis pieces didn't move
        foreach (var piece in zAxisPieces)
        {
            Assert.Equal(originalZPositions[piece.SolvedPosition], piece.Position);
        }
    }

    #endregion
}