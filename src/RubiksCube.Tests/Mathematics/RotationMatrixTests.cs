using RubiksCube.Core.Mathematics;
using RubiksCube.Core.Models;

namespace RubiksCube.Tests.Mathematics;

public class RotationMatrixTests
{
    [Fact]
    public void CreateXRotation_Clockwise_ShouldRotateCorrectly()
    {
        var matrix = RotationMatrix.CreateXRotation(clockwise: true);
        
        // Test known rotations around X-axis (clockwise when looking down positive X)
        // Y -> Z, Z -> -Y, -Y -> -Z, -Z -> Y
        var originalY = new Position3D(0, 1, 0);  // Up
        var originalZ = new Position3D(0, 0, 1);  // Front
        var originalNegY = new Position3D(0, -1, 0);  // Down
        var originalNegZ = new Position3D(0, 0, -1);  // Back
        
        var rotatedY = RotationMatrix.Apply(matrix, originalY);
        var rotatedZ = RotationMatrix.Apply(matrix, originalZ);
        var rotatedNegY = RotationMatrix.Apply(matrix, originalNegY);
        var rotatedNegZ = RotationMatrix.Apply(matrix, originalNegZ);
        
        Assert.Equal(new Position3D(0, 0, 1), rotatedY);    // Y -> Z
        Assert.Equal(new Position3D(0, -1, 0), rotatedZ);   // Z -> -Y
        Assert.Equal(new Position3D(0, 0, -1), rotatedNegY); // -Y -> -Z
        Assert.Equal(new Position3D(0, 1, 0), rotatedNegZ);  // -Z -> Y
    }
    
    [Fact]
    public void CreateYRotation_Clockwise_ShouldRotateCorrectly()
    {
        var matrix = RotationMatrix.CreateYRotation(clockwise: true);
        
        // Test known rotations around Y-axis (clockwise when looking down positive Y)
        // X -> -Z, Z -> X, -X -> Z, -Z -> -X
        var originalX = new Position3D(1, 0, 0);   // Right
        var originalZ = new Position3D(0, 0, 1);   // Front
        var originalNegX = new Position3D(-1, 0, 0); // Left
        var originalNegZ = new Position3D(0, 0, -1); // Back
        
        var rotatedX = RotationMatrix.Apply(matrix, originalX);
        var rotatedZ = RotationMatrix.Apply(matrix, originalZ);
        var rotatedNegX = RotationMatrix.Apply(matrix, originalNegX);
        var rotatedNegZ = RotationMatrix.Apply(matrix, originalNegZ);
        
        Assert.Equal(new Position3D(0, 0, -1), rotatedX);   // X -> -Z
        Assert.Equal(new Position3D(1, 0, 0), rotatedZ);    // Z -> X
        Assert.Equal(new Position3D(0, 0, 1), rotatedNegX); // -X -> Z
        Assert.Equal(new Position3D(-1, 0, 0), rotatedNegZ); // -Z -> -X
    }
    
    [Fact]
    public void CreateZRotation_Clockwise_ShouldRotateCorrectly()
    {
        var matrix = RotationMatrix.CreateZRotation(clockwise: true);
        
        // Test known rotations around Z-axis (clockwise when looking down positive Z)
        // X -> Y, Y -> -X, -X -> -Y, -Y -> X
        var originalX = new Position3D(1, 0, 0);   // Right
        var originalY = new Position3D(0, 1, 0);   // Up
        var originalNegX = new Position3D(-1, 0, 0); // Left
        var originalNegY = new Position3D(0, -1, 0); // Down
        
        var rotatedX = RotationMatrix.Apply(matrix, originalX);
        var rotatedY = RotationMatrix.Apply(matrix, originalY);
        var rotatedNegX = RotationMatrix.Apply(matrix, originalNegX);
        var rotatedNegY = RotationMatrix.Apply(matrix, originalNegY);
        
        Assert.Equal(new Position3D(0, 1, 0), rotatedX);    // X -> Y
        Assert.Equal(new Position3D(-1, 0, 0), rotatedY);   // Y -> -X
        Assert.Equal(new Position3D(0, -1, 0), rotatedNegX); // -X -> -Y
        Assert.Equal(new Position3D(1, 0, 0), rotatedNegY);  // -Y -> X
    }
    
    [Fact]
    public void CreateRotationAroundAxis_ShouldMatchSpecificRotations()
    {
        // Test that axis-based rotation matches specific axis rotations
        var xAxis = new Position3D(1, 0, 0);
        var yAxis = new Position3D(0, 1, 0);
        var zAxis = new Position3D(0, 0, 1);
        
        var xRotMatrix = RotationMatrix.CreateRotationAroundAxis(xAxis, true);
        var yRotMatrix = RotationMatrix.CreateRotationAroundAxis(yAxis, true);
        var zRotMatrix = RotationMatrix.CreateRotationAroundAxis(zAxis, true);
        
        var expectedX = RotationMatrix.CreateXRotation(true);
        var expectedY = RotationMatrix.CreateYRotation(true);
        var expectedZ = RotationMatrix.CreateZRotation(true);
        
        Assert.Equal(expectedX, xRotMatrix);
        Assert.Equal(expectedY, yRotMatrix);
        Assert.Equal(expectedZ, zRotMatrix);
    }
    
    [Fact]
    public void Identity_ShouldNotChangePositions()
    {
        var identity = RotationMatrix.Identity;
        var positions = new[]
        {
            new Position3D(1, 0, 0),
            new Position3D(0, 1, 0),
            new Position3D(0, 0, 1),
            new Position3D(-1, -1, -1),
            new Position3D(1, 1, 1)
        };
        
        foreach (var position in positions)
        {
            var result = RotationMatrix.Apply(identity, position);
            Assert.Equal(position, result);
        }
    }
    
    [Fact]
    public void FourRotations_ShouldReturnToOriginal()
    {
        var testPosition = new Position3D(1, 0, 1);
        
        // Test X rotations (4 × 90° = 360° = identity)
        var xMatrix = RotationMatrix.Identity;
        for (int i = 0; i < 4; i++)
        {
            xMatrix = RotationMatrix.Multiply(xMatrix, RotationMatrix.CreateXRotation(true));
        }
        var xResult = RotationMatrix.Apply(xMatrix, testPosition);
        Assert.Equal(testPosition, xResult);
        
        // Test Y rotations
        var yMatrix = RotationMatrix.Identity;
        for (int i = 0; i < 4; i++)
        {
            yMatrix = RotationMatrix.Multiply(yMatrix, RotationMatrix.CreateYRotation(true));
        }
        var yResult = RotationMatrix.Apply(yMatrix, testPosition);
        Assert.Equal(testPosition, yResult);
        
        // Test Z rotations
        var zMatrix = RotationMatrix.Identity;
        for (int i = 0; i < 4; i++)
        {
            zMatrix = RotationMatrix.Multiply(zMatrix, RotationMatrix.CreateZRotation(true));
        }
        var zResult = RotationMatrix.Apply(zMatrix, testPosition);
        Assert.Equal(testPosition, zResult);
    }
    
    [Fact]
    public void MatrixMultiplication_ShouldCombineRotations()
    {
        var rotation1 = RotationMatrix.CreateYRotation(true);
        var rotation2 = RotationMatrix.CreateYRotation(true);
        var combined = RotationMatrix.Multiply(rotation1, rotation2);
        
        // Two Y rotations should equal manually combining two Y rotations
        var manualDouble = RotationMatrix.Multiply(RotationMatrix.CreateYRotation(true), RotationMatrix.CreateYRotation(true));
        Assert.Equal(manualDouble, combined);
        
        // Test with a position
        var testPos = new Position3D(1, 0, 0);
        var step1 = RotationMatrix.Apply(rotation1, testPos);
        var step2 = RotationMatrix.Apply(rotation2, step1);
        var direct = RotationMatrix.Apply(combined, testPos);
        
        Assert.Equal(step2, direct);
    }
    
    [Fact]
    public void IsValidRotation_ShouldDetectValidMatrices()
    {
        // All our rotation matrices should be valid (determinant = ±1)
        Assert.True(RotationMatrix.IsValidRotation(RotationMatrix.Identity));
        Assert.True(RotationMatrix.IsValidRotation(RotationMatrix.CreateXRotation(true)));
        Assert.True(RotationMatrix.IsValidRotation(RotationMatrix.CreateYRotation(true)));
        Assert.True(RotationMatrix.IsValidRotation(RotationMatrix.CreateZRotation(true)));
        Assert.True(RotationMatrix.IsValidRotation(RotationMatrix.CreateXRotation(false)));
        
        // Invalid matrix should fail
        var invalid = new int[,] { { 1, 0, 0 }, { 0, 2, 0 }, { 0, 0, 1 } }; // determinant = 2
        Assert.False(RotationMatrix.IsValidRotation(invalid));
    }
    
    [Fact]
    public void CounterClockwise_ShouldBeInverseOfClockwise()
    {
        var testPosition = new Position3D(1, 1, 0);
        
        // X rotation
        var xClock = RotationMatrix.CreateXRotation(true);
        var xCounter = RotationMatrix.CreateXRotation(false);
        var xCombined = RotationMatrix.Multiply(xClock, xCounter);
        var xResult = RotationMatrix.Apply(xCombined, testPosition);
        Assert.Equal(testPosition, xResult);
        
        // Y rotation
        var yClock = RotationMatrix.CreateYRotation(true);
        var yCounter = RotationMatrix.CreateYRotation(false);
        var yCombined = RotationMatrix.Multiply(yClock, yCounter);
        var yResult = RotationMatrix.Apply(yCombined, testPosition);
        Assert.Equal(testPosition, yResult);
        
        // Z rotation
        var zClock = RotationMatrix.CreateZRotation(true);
        var zCounter = RotationMatrix.CreateZRotation(false);
        var zCombined = RotationMatrix.Multiply(zClock, zCounter);
        var zResult = RotationMatrix.Apply(zCombined, testPosition);
        Assert.Equal(testPosition, zResult);
    }
    
    [Fact]
    public void ApplyMultiplePositions_ShouldWorkCorrectly()
    {
        var matrix = RotationMatrix.CreateYRotation(true);
        var positions = new[]
        {
            new Position3D(1, 0, 0),   // Right -> Back
            new Position3D(0, 0, 1),   // Front -> Right
            new Position3D(-1, 0, 0),  // Left -> Front
            new Position3D(0, 0, -1)   // Back -> Left
        };
        
        var rotated = RotationMatrix.Apply(matrix, positions).ToArray();
        
        Assert.Equal(new Position3D(0, 0, -1), rotated[0]);  // Right -> Back
        Assert.Equal(new Position3D(1, 0, 0), rotated[1]);   // Front -> Right
        Assert.Equal(new Position3D(0, 0, 1), rotated[2]);   // Left -> Front
        Assert.Equal(new Position3D(-1, 0, 0), rotated[3]);  // Back -> Left
    }
}