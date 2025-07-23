using RubiksCube.Core.Models;

namespace RubiksCube.Core.Utilities;

/// <summary>
/// Provides validation methods to ensure cubes are in the correct format
/// for methods that have specific orientation requirements
/// </summary>
public static class CubeValidator
{
    /// <summary>
    /// Validates that the cube is in canonical orientation (white bottom, green front)
    /// </summary>
    /// <param name="cube">The cube to validate</param>
    /// <param name="methodName">Name of the method requiring validation (for error messages)</param>
    /// <exception cref="ArgumentException">Thrown when cube is not in canonical orientation</exception>
    public static void ValidateCanonicalOrientation(Cube cube, string methodName)
    {
        if (cube == null)
            throw new ArgumentNullException(nameof(cube));

        if (!IsWhiteOnBottom(cube))
        {
            throw new ArgumentException($"{methodName} requires canonical cube orientation: white must be on bottom face");
        }

        if (!IsGreenOnFront(cube))
        {
            throw new ArgumentException($"{methodName} requires canonical cube orientation: green must be on front face (white bottom + green front)");
        }
    }

    /// <summary>
    /// Validates that the cube has white on the bottom face
    /// </summary>
    /// <param name="cube">The cube to validate</param>
    /// <param name="methodName">Name of the method requiring validation (for error messages)</param>
    /// <exception cref="ArgumentException">Thrown when white is not on bottom</exception>
    public static void ValidateWhiteOnBottom(Cube cube, string methodName)
    {
        if (cube == null)
            throw new ArgumentNullException(nameof(cube));

        if (!IsWhiteOnBottom(cube))
        {
            throw new ArgumentException($"{methodName} requires white cross color on bottom face");
        }
    }

    /// <summary>
    /// Checks if white center is on the bottom face (Y = -1)
    /// </summary>
    private static bool IsWhiteOnBottom(Cube cube)
    {
        var bottomCenter = cube.GetPieceAt(new Position3D(0, -1, 0));
        return bottomCenter?.Colors[1] == CubeColor.White; // Y-axis color should be white
    }

    /// <summary>
    /// Checks if green center is on the front face (Z = 1)
    /// </summary>
    private static bool IsGreenOnFront(Cube cube)
    {
        var frontCenter = cube.GetPieceAt(new Position3D(0, 0, 1));
        return frontCenter?.Colors[2] == CubeColor.Green; // Z-axis color should be green
    }
}