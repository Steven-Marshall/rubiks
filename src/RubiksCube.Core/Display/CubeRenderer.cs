using RubiksCube.Core.Models;
using CSharpFunctionalExtensions;
using System.Text;

namespace RubiksCube.Core.Display;

public class CubeRenderer
{
    private readonly DisplayConfig _config;

    public CubeRenderer(DisplayConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public Result<string> Render(Cube cube)
    {
        if (cube == null)
            return Result.Failure<string>("Cube cannot be null");

        try
        {
            return _config.Format switch
            {
                DisplayFormat.Unicode => RenderUnicode(cube),
                DisplayFormat.ASCII => RenderAscii(cube),
                _ => Result.Failure<string>($"Unsupported display format: {_config.Format}")
            };
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"Rendering failed: {ex.Message}");
        }
    }

    private Result<string> RenderUnicode(Cube cube)
    {
        var sb = new StringBuilder();
        
        // Get the symbol for each color
        var symbols = GetColorSymbols();
        
        // Get face data based on current orientation
        var up = GetFaceColors(cube, FaceDirection.Up);
        var left = GetFaceColors(cube, FaceDirection.Left);
        var front = GetFaceColors(cube, FaceDirection.Front);
        var right = GetFaceColors(cube, FaceDirection.Right);
        var back = GetFaceColors(cube, FaceDirection.Back);
        var down = GetFaceColors(cube, FaceDirection.Down);

        // Render top face (Up) - aligned with front
        for (int row = 0; row < 3; row++)
        {
            sb.Append($"{(char)0x3164}{(char)0x3164}{(char)0x3164}"); // 3 Hangul Filler (U+3164) to center over front face
            for (int col = 0; col < 3; col++)
            {
                var position = row * 3 + col;
                sb.Append(symbols[up[position]]);
            }
            sb.AppendLine();
        }

        // Render middle row (Left, Front, Right, Back)
        for (int row = 0; row < 3; row++)
        {
            // Left face
            for (int col = 0; col < 3; col++)
            {
                var position = row * 3 + col;
                sb.Append(symbols[left[position]]);
            }

            // Front face  
            for (int col = 0; col < 3; col++)
            {
                var position = row * 3 + col;
                sb.Append(symbols[front[position]]);
            }

            // Right face
            for (int col = 0; col < 3; col++)
            {
                var position = row * 3 + col;
                sb.Append(symbols[right[position]]);
            }

            // Back face
            for (int col = 0; col < 3; col++)
            {
                var position = row * 3 + col;
                sb.Append(symbols[back[position]]);
            }
            sb.AppendLine();
        }

        // Render bottom face (Down) - aligned with front
        for (int row = 0; row < 3; row++)
        {
            sb.Append($"{(char)0x3164}{(char)0x3164}{(char)0x3164}"); // 3 Hangul Filler (U+3164) to center over front face
            for (int col = 0; col < 3; col++)
            {
                var position = row * 3 + col;
                sb.Append(symbols[down[position]]);
            }
            sb.AppendLine();
        }

        return Result.Success(sb.ToString());
    }

    private Result<string> RenderAscii(Cube cube)
    {
        var sb = new StringBuilder();
        
        // Get the symbol for each color  
        var symbols = GetColorSymbols();
        
        // Same layout as Unicode but with ASCII characters
        var up = GetFaceColors(cube, FaceDirection.Up);
        var left = GetFaceColors(cube, FaceDirection.Left);
        var front = GetFaceColors(cube, FaceDirection.Front);
        var right = GetFaceColors(cube, FaceDirection.Right);
        var back = GetFaceColors(cube, FaceDirection.Back);
        var down = GetFaceColors(cube, FaceDirection.Down);

        // Render top face (Up)
        for (int row = 0; row < 3; row++)
        {
            sb.Append("       "); // 7 spaces to align with front face (5 left chars + 2 spaces)
            for (int col = 0; col < 3; col++)
            {
                var position = row * 3 + col;
                sb.Append(symbols[up[position]]);
                if (col < 2) sb.Append(' '); // Space between letters
            }
            sb.AppendLine();
        }

        // Render middle row
        for (int row = 0; row < 3; row++)
        {
            // Left face
            for (int col = 0; col < 3; col++)
            {
                var position = row * 3 + col;
                sb.Append(symbols[left[position]]);
                if (col < 2) sb.Append(' ');
            }
            sb.Append("  "); // 2 spaces between faces

            // Front face
            for (int col = 0; col < 3; col++)
            {
                var position = row * 3 + col;
                sb.Append(symbols[front[position]]);
                if (col < 2) sb.Append(' ');
            }
            sb.Append("  "); // 2 spaces between faces

            // Right face
            for (int col = 0; col < 3; col++)
            {
                var position = row * 3 + col;
                sb.Append(symbols[right[position]]);
                if (col < 2) sb.Append(' ');
            }
            sb.Append("  "); // 2 spaces between faces

            // Back face
            for (int col = 0; col < 3; col++)
            {
                var position = row * 3 + col;
                sb.Append(symbols[back[position]]);
                if (col < 2) sb.Append(' ');
            }
            sb.AppendLine();
        }

        // Render bottom face (Down)
        for (int row = 0; row < 3; row++)
        {
            sb.Append("       "); // 7 spaces to align with front face
            for (int col = 0; col < 3; col++)
            {
                var position = row * 3 + col;
                sb.Append(symbols[down[position]]);
                if (col < 2) sb.Append(' ');
            }
            sb.AppendLine();
        }

        return Result.Success(sb.ToString());
    }

    private Dictionary<CubeColor, string> GetColorSymbols()
    {
        return _config.Format switch
        {
            DisplayFormat.Unicode => new Dictionary<CubeColor, string>
            {
                [CubeColor.White] = _config.Squares.White,
                [CubeColor.Red] = _config.Squares.Red,
                [CubeColor.Blue] = _config.Squares.Blue,
                [CubeColor.Orange] = _config.Squares.Orange,
                [CubeColor.Green] = _config.Squares.Green,
                [CubeColor.Yellow] = _config.Squares.Yellow
            },
            DisplayFormat.ASCII => new Dictionary<CubeColor, string>
            {
                [CubeColor.White] = _config.Letters.White,
                [CubeColor.Red] = _config.Letters.Red,
                [CubeColor.Blue] = _config.Letters.Blue,
                [CubeColor.Orange] = _config.Letters.Orange,
                [CubeColor.Green] = _config.Letters.Green,
                [CubeColor.Yellow] = _config.Letters.Yellow
            },
            _ => throw new InvalidOperationException($"Unsupported display format: {_config.Format}")
        };
    }

    /// <summary>
    /// Gets the 3x3 color array for a face in display orientation
    /// Position mapping: 0=top-left, 1=top-center, 2=top-right, 3=middle-left, etc.
    /// </summary>
    private CubeColor[] GetFaceColors(Cube cube, FaceDirection faceDirection)
    {
        var colors = new CubeColor[9];
        
        // Get the direction vector for this face in current orientation
        var faceColor = GetFaceColor(cube, faceDirection);
        var faceVector = cube.Orientation.GetDirectionForFace(faceColor);
        
        // For each position in the 3x3 grid, find what color should be displayed
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                var gridPosition = row * 3 + col;
                var worldPosition = GetWorldPositionForFaceGrid(faceVector, row, col);
                var color = GetColorAtPosition(cube, worldPosition, faceVector);
                colors[gridPosition] = color;
            }
        }
        
        return colors;
    }

    /// <summary>
    /// Gets the world position (x,y,z) for a specific row/column in a face's 3x3 grid
    /// </summary>
    private Position3D GetWorldPositionForFaceGrid(Position3D faceVector, int row, int col)
    {
        // Convert row/col (0-2) to local coordinates (-1 to +1)
        var localRow = 1 - row; // row 0 = +1, row 1 = 0, row 2 = -1
        var localCol = col - 1; // col 0 = -1, col 1 = 0, col 2 = +1
        
        // Convert local 2D coordinates to 3D world position based on face orientation
        if (faceVector.Y != 0) // Up or Down face
        {
            // Up/Down: X=left-right, Z=back-front
            return new Position3D(localCol, faceVector.Y, -localRow);
        }
        else if (faceVector.Z != 0) // Front or Back face
        {
            // Front/Back: X=left-right, Y=down-up
            return new Position3D(localCol, localRow, faceVector.Z);
        }
        else // Left or Right face
        {
            // Left/Right: Z=back-front, Y=down-up
            return new Position3D(faceVector.X, localRow, -localCol);
        }
    }

    /// <summary>
    /// Gets the color that should be displayed at a world position on a specific face
    /// </summary>
    private CubeColor GetColorAtPosition(Cube cube, Position3D position, Position3D faceVector)
    {
        // Check if this is a center position (centers don't move, just show the face color)
        if (IsCenterPosition(position))
        {
            return cube.Orientation.GetFaceForDirection(faceVector);
        }
        
        // Find the piece at this position
        var piece = cube.GetPieceAt(position);
        if (piece == null)
        {
            // This shouldn't happen in a valid cube, but return face color as fallback
            return cube.Orientation.GetFaceForDirection(faceVector);
        }
        
        // Determine which color of the piece should be showing on this face
        return GetPieceColorForFace(piece, faceVector, cube.Orientation);
    }

    /// <summary>
    /// Determines which color of a piece should be visible on a specific face
    /// Based on pglass/cube approach: each piece shows specific colors on specific axes
    /// </summary>
    private CubeColor GetPieceColorForFace(CubePiece piece, Position3D faceVector, CubeOrientation orientation)
    {
        // Determine which axis this face vector represents
        if (faceVector.X != 0)
        {
            // This is an X-axis face (Left/Right)
            return GetPieceColorForAxis(piece, 'X', orientation);
        }
        else if (faceVector.Y != 0)
        {
            // This is a Y-axis face (Up/Down)  
            return GetPieceColorForAxis(piece, 'Y', orientation);
        }
        else // faceVector.Z != 0
        {
            // This is a Z-axis face (Front/Back)
            return GetPieceColorForAxis(piece, 'Z', orientation);
        }
    }
    
    /// <summary>
    /// Gets the color that a piece shows on a specific axis
    /// STRICT MODE: Uses direct color indexing based on X→Y→Z ordering
    /// </summary>
    private CubeColor GetPieceColorForAxis(CubePiece piece, char axis, CubeOrientation orientation)
    {
        var position = piece.Position;
        
        return axis switch
        {
            'X' => position.X switch
            {
                -1 => GetStrictColorForAxis(piece, 'X'), // Left face - use piece's X-axis color
                1 => GetStrictColorForAxis(piece, 'X'),  // Right face - use piece's X-axis color  
                0 => GetMiddleEdgeColorForX(piece, orientation), // Middle edge piece
                _ => throw new InvalidOperationException($"Invalid X coordinate: {position.X}")
            },
            'Y' => position.Y switch
            {
                -1 => GetStrictColorForAxis(piece, 'Y'), // Bottom face - use piece's Y-axis color
                1 => GetStrictColorForAxis(piece, 'Y'),  // Top face - use piece's Y-axis color
                0 => GetMiddleEdgeColorForY(piece, orientation), // Middle edge piece
                _ => throw new InvalidOperationException($"Invalid Y coordinate: {position.Y}")
            },
            'Z' => position.Z switch
            {
                -1 => GetStrictColorForAxis(piece, 'Z'), // Back face - use piece's Z-axis color
                1 => GetStrictColorForAxis(piece, 'Z'),  // Front face - use piece's Z-axis color
                0 => GetMiddleEdgeColorForZ(piece, orientation), // Middle edge piece  
                _ => throw new InvalidOperationException($"Invalid Z coordinate: {position.Z}")
            },
            _ => throw new ArgumentException($"Invalid axis: {axis}")
        };
    }
    
    /// <summary>
    /// Gets the color for a specific axis using strict X→Y→Z ordering
    /// This will expose orientation bugs if colors are stored incorrectly
    /// </summary>
    private CubeColor GetStrictColorForAxis(CubePiece piece, char axis)
    {
        var pos = piece.Position;
        var colorIndex = 0;
        
        // Calculate color index based on X→Y→Z ordering
        switch (char.ToUpper(axis))
        {
            case 'X':
                if (pos.X == 0) throw new InvalidOperationException($"Piece at {pos} has no X-axis color");
                return piece.Colors[0]; // X-axis color is always first
                
            case 'Y':
                if (pos.Y == 0) throw new InvalidOperationException($"Piece at {pos} has no Y-axis color");
                if (pos.X != 0) colorIndex++; // Skip X-axis color if present
                return piece.Colors[colorIndex];
                
            case 'Z':
                if (pos.Z == 0) throw new InvalidOperationException($"Piece at {pos} has no Z-axis color");
                if (pos.X != 0) colorIndex++; // Skip X-axis color if present
                if (pos.Y != 0) colorIndex++; // Skip Y-axis color if present
                return piece.Colors[colorIndex];
                
            default:
                throw new ArgumentException($"Invalid axis: {axis}");
        }
    }
    
    /// <summary>
    /// For middle edge pieces on X-axis, determine which color shows on X faces
    /// STRICT MODE: Assumes colors are stored in X→Y→Z order
    /// </summary>
    private CubeColor GetMiddleEdgeColorForX(CubePiece piece, CubeOrientation orientation)
    {
        // Middle edges have 2 colors: X-axis color should be first (index 0)
        // This will expose orientation bugs if colors are in wrong order
        return piece.Colors.Count > 0 ? piece.Colors[0] : orientation.Left;
    }
    
    /// <summary>
    /// For middle edge pieces on Y-axis, determine which color shows on Y faces
    /// STRICT MODE: Assumes colors are stored in X→Y→Z order
    /// </summary>
    private CubeColor GetMiddleEdgeColorForY(CubePiece piece, CubeOrientation orientation)
    {
        // For Y-axis edges, determine which color index corresponds to Y-axis
        // If piece is at (0, Y, Z): Y-axis color is first (only Y and Z present)
        // If piece is at (X, Y, 0): Y-axis color is second (X first, then Y)
        var pos = piece.Position;
        if (pos.X == 0) // YZ edge: Y-axis color is first
            return piece.Colors.Count > 0 ? piece.Colors[0] : orientation.Up;
        else // XY edge: Y-axis color is second
            return piece.Colors.Count > 1 ? piece.Colors[1] : orientation.Up;
    }
    
    /// <summary>
    /// For middle edge pieces on Z-axis, determine which color shows on Z faces
    /// STRICT MODE: Assumes colors are stored in X→Y→Z order
    /// </summary>
    private CubeColor GetMiddleEdgeColorForZ(CubePiece piece, CubeOrientation orientation)
    {
        // For Z-axis edges, determine which color index corresponds to Z-axis
        // If piece is at (0, 0, Z): impossible (would be center)
        // If piece is at (X, 0, Z): Z-axis color is second (X first, then Z)
        // If piece is at (0, Y, Z): Z-axis color is second (Y first, then Z)
        var pos = piece.Position;
        if (pos.X == 0) // YZ edge: Z-axis color is second
            return piece.Colors.Count > 1 ? piece.Colors[1] : orientation.Front;
        else // XZ edge: Z-axis color is second
            return piece.Colors.Count > 1 ? piece.Colors[1] : orientation.Front;
    }

    /// <summary>
    /// Checks if a position is a center position (only one non-zero coordinate)
    /// </summary>
    private static bool IsCenterPosition(Position3D position)
    {
        var nonZeroCount = 0;
        if (position.X != 0) nonZeroCount++;
        if (position.Y != 0) nonZeroCount++;
        if (position.Z != 0) nonZeroCount++;
        
        return nonZeroCount == 1;
    }

    /// <summary>
    /// Gets the face color for a given direction in the cube's current orientation
    /// </summary>
    private CubeColor GetFaceColor(Cube cube, FaceDirection direction)
    {
        return direction switch
        {
            FaceDirection.Up => cube.Orientation.Up,
            FaceDirection.Down => cube.Orientation.Bottom,
            FaceDirection.Left => cube.Orientation.Left,
            FaceDirection.Right => cube.Orientation.Right,
            FaceDirection.Front => cube.Orientation.Front,
            FaceDirection.Back => cube.Orientation.Back,
            _ => throw new ArgumentException($"Unknown face direction: {direction}")
        };
    }
}

public enum FaceDirection
{
    Up,
    Down,
    Left,
    Right,
    Front,
    Back
}