using RubiksCube.Core.Mathematics;
using RubiksCube.Core.Models;

namespace RubiksCube.Tests.Mathematics;

/// <summary>
/// Mathematical validation of rotation matrices using properties and invariants
/// No hard-coded expected results - only mathematical principles
/// </summary>
public class RotationMatrixMathTests
{
    [Fact]
    public void RotationMatrix_ShouldPreserveDeterminant()
    {
        // Mathematical fact: Rotation matrices must have determinant = ±1
        // This ensures they preserve volume and handedness
        
        var matrices = new[]
        {
            RotationMatrix.CreateXRotation(true),
            RotationMatrix.CreateXRotation(false),
            RotationMatrix.CreateYRotation(true),
            RotationMatrix.CreateYRotation(false),
            RotationMatrix.CreateZRotation(true),
            RotationMatrix.CreateZRotation(false),
            RotationMatrix.Identity
        };
        
        foreach (var matrix in matrices)
        {
            var det = CalculateDeterminant(matrix);
            Assert.True(Math.Abs(det) == 1, $"Determinant should be ±1, but was {det}");
        }
    }
    
    [Fact]
    public void RotationMatrix_InverseShouldEqualTranspose()
    {
        // Mathematical fact: For rotation matrices, inverse = transpose
        // This is because rotation matrices are orthogonal
        
        var matrices = new[]
        {
            RotationMatrix.CreateXRotation(true),
            RotationMatrix.CreateYRotation(true),
            RotationMatrix.CreateZRotation(true)
        };
        
        foreach (var matrix in matrices)
        {
            var transpose = Transpose(matrix);
            var product = RotationMatrix.Multiply(matrix, transpose);
            
            // Product should be identity
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var expected = (i == j) ? 1 : 0;
                    Assert.Equal(expected, product[i, j]);
                }
            }
        }
    }
    
    [Fact]
    public void ClockwiseCounterClockwise_ShouldBeInverses()
    {
        // Mathematical fact: R × R' = Identity
        // Clockwise and counterclockwise rotations cancel out
        
        var rotationPairs = new[]
        {
            (RotationMatrix.CreateXRotation(true), RotationMatrix.CreateXRotation(false)),
            (RotationMatrix.CreateYRotation(true), RotationMatrix.CreateYRotation(false)),
            (RotationMatrix.CreateZRotation(true), RotationMatrix.CreateZRotation(false))
        };
        
        foreach (var (clockwise, counterClockwise) in rotationPairs)
        {
            var product = RotationMatrix.Multiply(clockwise, counterClockwise);
            AssertIsIdentity(product);
        }
    }
    
    [Fact]
    public void FourRotations_ShouldEqualIdentity()
    {
        // Mathematical fact: 4 × 90° rotations = 360° = identity
        // Tests cyclic group property
        
        var baseMatrices = new[]
        {
            RotationMatrix.CreateXRotation(true),
            RotationMatrix.CreateYRotation(true),
            RotationMatrix.CreateZRotation(true)
        };
        
        foreach (var baseMatrix in baseMatrices)
        {
            var result = RotationMatrix.Identity;
            
            // Apply 4 times
            for (int i = 0; i < 4; i++)
            {
                result = RotationMatrix.Multiply(result, baseMatrix);
            }
            
            AssertIsIdentity(result);
        }
    }
    
    [Fact]
    public void RotationMatrix_ShouldPreserveVectorLength()
    {
        // Mathematical fact: Rotations preserve vector length
        // |Rv| = |v| for all vectors v
        
        var testVectors = new[]
        {
            new Position3D(1, 0, 0),
            new Position3D(0, 1, 0),
            new Position3D(0, 0, 1),
            new Position3D(1, 1, 0),
            new Position3D(1, 1, 1)
        };
        
        var matrices = new[]
        {
            RotationMatrix.CreateXRotation(true),
            RotationMatrix.CreateYRotation(true),
            RotationMatrix.CreateZRotation(true)
        };
        
        foreach (var vector in testVectors)
        {
            var originalLength = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
            
            foreach (var matrix in matrices)
            {
                var rotated = RotationMatrix.Apply(matrix, vector);
                var rotatedLength = Math.Sqrt(rotated.X * rotated.X + rotated.Y * rotated.Y + rotated.Z * rotated.Z);
                
                Assert.Equal(originalLength, rotatedLength, precision: 10);
            }
        }
    }
    
    [Fact]
    public void RotationComposition_ShouldBeAssociative()
    {
        // Mathematical fact: Matrix multiplication is associative
        // (AB)C = A(BC)
        
        var A = RotationMatrix.CreateXRotation(true);
        var B = RotationMatrix.CreateYRotation(true);
        var C = RotationMatrix.CreateZRotation(true);
        
        var AB_C = RotationMatrix.Multiply(RotationMatrix.Multiply(A, B), C);
        var A_BC = RotationMatrix.Multiply(A, RotationMatrix.Multiply(B, C));
        
        // Should be equal
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Assert.Equal(AB_C[i, j], A_BC[i, j]);
            }
        }
    }
    
    [Theory]
    [InlineData(1, 0, 0)]  // X-axis
    [InlineData(0, 1, 0)]  // Y-axis
    [InlineData(0, 0, 1)]  // Z-axis
    public void AxisRotation_ShouldNotMoveAxisVector(int x, int y, int z)
    {
        // Mathematical fact: Rotating around an axis doesn't move vectors on that axis
        // X rotation leaves (1,0,0) unchanged, etc.
        
        var axisVector = new Position3D(x, y, z);
        int[,] matrix;
        
        if (x != 0)
            matrix = RotationMatrix.CreateXRotation(true);
        else if (y != 0)
            matrix = RotationMatrix.CreateYRotation(true);
        else
            matrix = RotationMatrix.CreateZRotation(true);
        
        var rotated = RotationMatrix.Apply(matrix, axisVector);
        Assert.Equal(axisVector, rotated);
    }
    
    [Fact]
    public void RotationMatrix_ShouldFollowRightHandRule()
    {
        // Mathematical validation: Cross product of rotated basis vectors
        // For a proper rotation, (e1 × e2) · e3 = 1 (right-handed)
        
        var matrices = new[]
        {
            RotationMatrix.CreateXRotation(true),
            RotationMatrix.CreateYRotation(true),
            RotationMatrix.CreateZRotation(true)
        };
        
        var e1 = new Position3D(1, 0, 0);
        var e2 = new Position3D(0, 1, 0);
        var e3 = new Position3D(0, 0, 1);
        
        foreach (var matrix in matrices)
        {
            var r1 = RotationMatrix.Apply(matrix, e1);
            var r2 = RotationMatrix.Apply(matrix, e2);
            var r3 = RotationMatrix.Apply(matrix, e3);
            
            // Cross product r1 × r2
            var cross = new Position3D(
                r1.Y * r2.Z - r1.Z * r2.Y,
                r1.Z * r2.X - r1.X * r2.Z,
                r1.X * r2.Y - r1.Y * r2.X
            );
            
            // Dot product with r3
            var dot = cross.X * r3.X + cross.Y * r3.Y + cross.Z * r3.Z;
            
            // Should be 1 for right-handed system
            Assert.Equal(1, dot);
        }
    }
    
    // Helper methods
    private int CalculateDeterminant(int[,] matrix)
    {
        return matrix[0, 0] * (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1])
             - matrix[0, 1] * (matrix[1, 0] * matrix[2, 2] - matrix[1, 2] * matrix[2, 0])
             + matrix[0, 2] * (matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0]);
    }
    
    private int[,] Transpose(int[,] matrix)
    {
        var result = new int[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                result[i, j] = matrix[j, i];
            }
        }
        return result;
    }
    
    private void AssertIsIdentity(int[,] matrix)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var expected = (i == j) ? 1 : 0;
                Assert.Equal(expected, matrix[i, j]);
            }
        }
    }
}