using RubiksCube.Core.Models;
using System.Text;

namespace RubiksCube.Core.Display;

/// <summary>
/// Clean renderer for Python-style cube architecture with 3-element color arrays.
/// No orientation complexity - direct coordinate to color mapping.
/// </summary>
public class PythonRenderer
{
    private readonly Cube _cube;

    public PythonRenderer(Cube cube)
    {
        _cube = cube ?? throw new ArgumentNullException(nameof(cube));
    }

    /// <summary>
    /// Render the cube using Unicode characters with compact format matching original renderer
    /// </summary>
    public string RenderUnicode()
    {
        return Render(DisplayFormat.Unicode);
    }

    /// <summary>
    /// Render the cube using ASCII letters
    /// </summary>
    public string RenderAscii()
    {
        return Render(DisplayFormat.ASCII);
    }

    /// <summary>
    /// Core rendering method that handles both Unicode and ASCII formats
    /// </summary>
    private string Render(DisplayFormat format)
    {
        var sb = new StringBuilder();
        
        // Get face colors using Python approach: pos.dot(axis) > 0
        var up = GetFaceColors(new Position3D(0, 1, 0), 1);    // pos.y > 0, Colors[1] 
        var down = GetFaceColors(new Position3D(0, -1, 0), 1);  // pos.y < 0, Colors[1]
        var left = GetFaceColors(new Position3D(-1, 0, 0), 0);  // pos.x < 0, Colors[0]
        var right = GetFaceColors(new Position3D(1, 0, 0), 0);  // pos.x > 0, Colors[0]
        var front = GetFaceColors(new Position3D(0, 0, 1), 2);  // pos.z > 0, Colors[2]
        var back = GetFaceColors(new Position3D(0, 0, -1), 2);  // pos.z < 0, Colors[2]
        
        // Use format-specific template for proper alignment
        var template = format == DisplayFormat.ASCII
            ? ("   {0}{1}{2}\n" +      // ASCII: regular spaces for single letters
               "   {3}{4}{5}\n" +
               "   {6}{7}{8}\n" +
               "{9}{10}{11}{12}{13}{14}{15}{16}{17}{18}{19}{20}\n" +
               "{21}{22}{23}{24}{25}{26}{27}{28}{29}{30}{31}{32}\n" +
               "{33}{34}{35}{36}{37}{38}{39}{40}{41}{42}{43}{44}\n" +
               "   {45}{46}{47}\n" +
               "   {48}{49}{50}\n" +
               "   {51}{52}{53}")
            : ("ㅤㅤㅤ{0}{1}{2}\n" +   // Unicode: Hangul Filler for wide squares
               "ㅤㅤㅤ{3}{4}{5}\n" +
               "ㅤㅤㅤ{6}{7}{8}\n" +
               "{9}{10}{11}{12}{13}{14}{15}{16}{17}{18}{19}{20}\n" +
               "{21}{22}{23}{24}{25}{26}{27}{28}{29}{30}{31}{32}\n" +
               "{33}{34}{35}{36}{37}{38}{39}{40}{41}{42}{43}{44}\n" +
               "ㅤㅤㅤ{45}{46}{47}\n" +
               "ㅤㅤㅤ{48}{49}{50}\n" +
               "ㅤㅤㅤ{51}{52}{53}");
                       
        // Build color array exactly like Python _color_list method
        var colorList = new List<CubeColor>();
        colorList.AddRange(up);                    // 9 colors for top
        colorList.AddRange(left.Take(3));          // 3 colors for left row 1
        colorList.AddRange(front.Take(3));         // 3 colors for front row 1  
        colorList.AddRange(right.Take(3));         // 3 colors for right row 1
        colorList.AddRange(back.Take(3));          // 3 colors for back row 1
        colorList.AddRange(left.Skip(3).Take(3));  // 3 colors for left row 2
        colorList.AddRange(front.Skip(3).Take(3)); // 3 colors for front row 2
        colorList.AddRange(right.Skip(3).Take(3)); // 3 colors for right row 2
        colorList.AddRange(back.Skip(3).Take(3));  // 3 colors for back row 2
        colorList.AddRange(left.Skip(6));          // 3 colors for left row 3
        colorList.AddRange(front.Skip(6));         // 3 colors for front row 3
        colorList.AddRange(right.Skip(6));         // 3 colors for right row 3
        colorList.AddRange(back.Skip(6));          // 3 colors for back row 3
        colorList.AddRange(down);                  // 9 colors for bottom
        
        var symbols = colorList.Select(color => GetColorSymbol(color, format)).ToArray();
        
        // Debug: Check face sizes
        var faceSizes = $"up:{up.Length}, down:{down.Length}, left:{left.Length}, right:{right.Length}, front:{front.Length}, back:{back.Length}";
        if (symbols.Length != 54)
        {
            throw new InvalidOperationException($"Expected 54 colors for template, got {symbols.Length}. Face sizes: {faceSizes}");
        }
        
        sb.Append(string.Format(template, symbols.Cast<object>().ToArray()));
        
        return sb.ToString();
    }

    /// <summary>
    /// Get face colors using Python approach: extract pieces with positive dot product
    /// </summary>
    private CubeColor[] GetFaceColors(Position3D axis, int colorIndex)
    {
        // Get all pieces on this face using Python logic: pos.dot(axis) > 0
        // This includes corners, edges, AND centers naturally
        var facePieces = _cube.Pieces
            .Where(p => DotProduct(p.Position, axis) > 0)
            .ToList();
            
        // Sort pieces according to Python sorting rules for each face
        facePieces = SortFacePieces(facePieces, axis);
        
        // Extract colors from the appropriate axis - now includes center naturally
        return facePieces.Select(p => p.Colors[colorIndex] ?? GetFallbackColor(axis))
                        .ToArray();
    }
    
    private double DotProduct(Position3D pos, Position3D axis)
    {
        return pos.X * axis.X + pos.Y * axis.Y + pos.Z * axis.Z;
    }
    
    private List<CubePiece> SortFacePieces(List<CubePiece> pieces, Position3D axis)
    {
        // Python sorting logic from _color_list method
        if (axis.X == 1) // RIGHT face
            return pieces.OrderBy(p => -p.Position.Y).ThenBy(p => -p.Position.Z).ToList();
        else if (axis.X == -1) // LEFT face  
            return pieces.OrderBy(p => -p.Position.Y).ThenBy(p => p.Position.Z).ToList();
        else if (axis.Y == 1) // UP face
            return pieces.OrderBy(p => p.Position.Z).ThenBy(p => p.Position.X).ToList();
        else if (axis.Y == -1) // DOWN face
            return pieces.OrderBy(p => -p.Position.Z).ThenBy(p => p.Position.X).ToList();
        else if (axis.Z == 1) // FRONT face
            return pieces.OrderBy(p => -p.Position.Y).ThenBy(p => p.Position.X).ToList();
        else if (axis.Z == -1) // BACK face
            return pieces.OrderBy(p => -p.Position.Y).ThenBy(p => -p.Position.X).ToList();
        else
            return pieces;
    }
    
    private CubeColor GetFallbackColor(Position3D axis)
    {
        // Simple fallback for missing piece colors (should rarely be used with centers)
        if (axis.X != 0) return axis.X > 0 ? CubeColor.Orange : CubeColor.Red;
        if (axis.Y != 0) return axis.Y > 0 ? CubeColor.Yellow : CubeColor.White; 
        if (axis.Z != 0) return axis.Z > 0 ? CubeColor.Green : CubeColor.Blue;
        return CubeColor.White;
    }


    /// <summary>
    /// Convert CubeColor to symbol (Unicode or ASCII)
    /// </summary>
    private string GetColorSymbol(CubeColor color, DisplayFormat format)
    {
        if (format == DisplayFormat.ASCII)
        {
            return color switch
            {
                CubeColor.White => "W",
                CubeColor.Yellow => "Y",
                CubeColor.Green => "G",
                CubeColor.Blue => "B",
                CubeColor.Red => "R",
                CubeColor.Orange => "O",
                _ => "?"
            };
        }
        else // Unicode
        {
            return color switch
            {
                CubeColor.White => "🔳",   // U+1F533 White Square Button
                CubeColor.Yellow => "🟨", // U+1F7E8 Yellow Square
                CubeColor.Green => "🟩",  // U+1F7E9 Green Square
                CubeColor.Blue => "🟦",   // U+1F7E6 Blue Square
                CubeColor.Red => "🟥",    // U+1F7E5 Red Square
                CubeColor.Orange => "🟧", // U+1F7E7 Orange Square
                _ => "❓"
            };
        }
    }
}