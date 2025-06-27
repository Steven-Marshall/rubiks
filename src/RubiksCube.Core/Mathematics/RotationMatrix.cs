namespace RubiksCube.Core.Mathematics;

using RubiksCube.Core.Models;

/// <summary>
/// Utility class for 3D rotation mathematics using 3x3 matrices
/// </summary>
public static class RotationMatrix
{
    /// <summary>
    /// Creates a 90-degree rotation matrix around the X-axis
    /// </summary>
    public static int[,] CreateXRotation(bool clockwise = true)
    {
        var sign = clockwise ? 1 : -1;
        return new int[,]
        {
            { 1,  0,     0    },
            { 0,  0,    -sign },
            { 0,  sign,  0    }
        };
    }
    
    /// <summary>
    /// Creates a 90-degree rotation matrix around the Y-axis
    /// </summary>
    public static int[,] CreateYRotation(bool clockwise = true)
    {
        var sign = clockwise ? 1 : -1;
        return new int[,]
        {
            { 0,     0,  sign },
            { 0,     1,  0    },
            { -sign, 0,  0    }
        };
    }
    
    /// <summary>
    /// Creates a 90-degree rotation matrix around the Z-axis
    /// </summary>
    public static int[,] CreateZRotation(bool clockwise = true)
    {
        var sign = clockwise ? 1 : -1;
        return new int[,]
        {
            { 0,    -sign, 0 },
            { sign,  0,    0 },
            { 0,     0,    1 }
        };
    }
    
    /// <summary>
    /// Creates a rotation matrix for any axis direction
    /// </summary>
    public static int[,] CreateRotationAroundAxis(Position3D axis, bool clockwise = true)
    {
        if (axis.X != 0 && axis.Y == 0 && axis.Z == 0)
        {
            // Rotation around X-axis, but consider direction
            var effectiveClockwise = (axis.X > 0) ? clockwise : !clockwise;
            return CreateXRotation(effectiveClockwise);
        }
        else if (axis.X == 0 && axis.Y != 0 && axis.Z == 0)
        {
            // Rotation around Y-axis, but consider direction
            var effectiveClockwise = (axis.Y > 0) ? clockwise : !clockwise;
            return CreateYRotation(effectiveClockwise);
        }
        else if (axis.X == 0 && axis.Y == 0 && axis.Z != 0)
        {
            // Rotation around Z-axis, but consider direction
            var effectiveClockwise = (axis.Z > 0) ? clockwise : !clockwise;
            return CreateZRotation(effectiveClockwise);
        }
        else
        {
            throw new ArgumentException($"Invalid rotation axis: {axis}. Must be along a single axis.");
        }
    }
    
    /// <summary>
    /// Applies a 3x3 rotation matrix to a position vector
    /// </summary>
    public static Position3D Apply(int[,] matrix, Position3D position)
    {
        if (matrix.GetLength(0) != 3 || matrix.GetLength(1) != 3)
            throw new ArgumentException("Matrix must be 3x3");
            
        var x = matrix[0, 0] * position.X + matrix[0, 1] * position.Y + matrix[0, 2] * position.Z;
        var y = matrix[1, 0] * position.X + matrix[1, 1] * position.Y + matrix[1, 2] * position.Z;
        var z = matrix[2, 0] * position.X + matrix[2, 1] * position.Y + matrix[2, 2] * position.Z;
        
        try 
        {
            return new Position3D(x, y, z);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException($"Failed to create Position3D({x}, {y}, {z}) from {position}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Applies a 3x3 rotation matrix to raw coordinates (without Position3D validation)
    /// </summary>
    public static (int x, int y, int z) ApplyRaw(int[,] matrix, int x, int y, int z)
    {
        if (matrix.GetLength(0) != 3 || matrix.GetLength(1) != 3)
            throw new ArgumentException("Matrix must be 3x3");
            
        var newX = matrix[0, 0] * x + matrix[0, 1] * y + matrix[0, 2] * z;
        var newY = matrix[1, 0] * x + matrix[1, 1] * y + matrix[1, 2] * z;
        var newZ = matrix[2, 0] * x + matrix[2, 1] * y + matrix[2, 2] * z;
        
        return (newX, newY, newZ);
    }
    
    /// <summary>
    /// Rotates multiple positions using the same matrix
    /// </summary>
    public static IEnumerable<Position3D> Apply(int[,] matrix, IEnumerable<Position3D> positions)
    {
        return positions.Select(pos => Apply(matrix, pos));
    }
    
    /// <summary>
    /// Multiplies two 3x3 matrices (for combining rotations)
    /// </summary>
    public static int[,] Multiply(int[,] matrixA, int[,] matrixB)
    {
        if (matrixA.GetLength(0) != 3 || matrixA.GetLength(1) != 3 ||
            matrixB.GetLength(0) != 3 || matrixB.GetLength(1) != 3)
            throw new ArgumentException("Both matrices must be 3x3");
            
        var result = new int[3, 3];
        
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                result[i, j] = 0;
                for (int k = 0; k < 3; k++)
                {
                    result[i, j] += matrixA[i, k] * matrixB[k, j];
                }
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Creates the identity matrix (no rotation)
    /// </summary>
    public static int[,] Identity => new int[,]
    {
        { 1, 0, 0 },
        { 0, 1, 0 },
        { 0, 0, 1 }
    };
    
    // V3.0 rotation matrices matching pglass/cube implementation
    // These are used for the unified v3.0 solver-centric approach
    
    /// <summary>
    /// 90 degree clockwise rotation in the XY plane (Z-axis rotation)
    /// </summary>
    public static int[,] ROT_XY_CW => new int[,]
    {
        { 0,  1, 0 },
        { -1, 0, 0 },
        { 0,  0, 1 }
    };
    
    /// <summary>
    /// 90 degree counter-clockwise rotation in the XY plane (Z-axis rotation)
    /// </summary>
    public static int[,] ROT_XY_CC => new int[,]
    {
        { 0, -1, 0 },
        { 1,  0, 0 },
        { 0,  0, 1 }
    };
    
    /// <summary>
    /// 90 degree clockwise rotation in the XZ plane (Y-axis rotation)
    /// </summary>
    public static int[,] ROT_XZ_CW => new int[,]
    {
        { 0, 0, -1 },
        { 0, 1,  0 },
        { 1, 0,  0 }
    };
    
    /// <summary>
    /// 90 degree counter-clockwise rotation in the XZ plane (Y-axis rotation)
    /// </summary>
    public static int[,] ROT_XZ_CC => new int[,]
    {
        { 0, 0,  1 },
        { 0, 1,  0 },
        { -1, 0, 0 }
    };
    
    /// <summary>
    /// 90 degree clockwise rotation in the YZ plane (X-axis rotation)
    /// </summary>
    public static int[,] ROT_YZ_CW => new int[,]
    {
        { 1,  0, 0 },
        { 0,  0, 1 },
        { 0, -1, 0 }
    };
    
    /// <summary>
    /// 90 degree counter-clockwise rotation in the YZ plane (X-axis rotation)
    /// </summary>
    public static int[,] ROT_YZ_CC => new int[,]
    {
        { 1, 0,  0 },
        { 0, 0, -1 },
        { 0, 1,  0 }
    };
    
    /// <summary>
    /// Checks if a matrix represents a valid rotation (determinant = ±1)
    /// </summary>
    public static bool IsValidRotation(int[,] matrix)
    {
        if (matrix.GetLength(0) != 3 || matrix.GetLength(1) != 3)
            return false;
            
        // Calculate determinant
        var det = matrix[0, 0] * (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1])
                - matrix[0, 1] * (matrix[1, 0] * matrix[2, 2] - matrix[1, 2] * matrix[2, 0])
                + matrix[0, 2] * (matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0]);
                
        return Math.Abs(det) == 1;
    }
    
    /// <summary>
    /// Pretty prints a matrix for debugging
    /// </summary>
    public static string ToString(int[,] matrix)
    {
        if (matrix.GetLength(0) != 3 || matrix.GetLength(1) != 3)
            return "Invalid matrix dimensions";
            
        var lines = new string[3];
        for (int i = 0; i < 3; i++)
        {
            lines[i] = $"[{matrix[i, 0],2} {matrix[i, 1],2} {matrix[i, 2],2}]";
        }
        
        return string.Join("\n", lines);
    }
}