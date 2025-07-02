using RubiksCube.Core.Models;

namespace RubiksCube.Core.Solving;

/// <summary>
/// Classifies cross edge positions into one of 24 systematic cases
/// </summary>
public static class CrossEdgeClassifier
{
    /// <summary>
    /// Classifies the position and orientation of a cross edge piece
    /// </summary>
    /// <param name="cube">Current cube state</param>
    /// <param name="crossColor">Cross color being solved (default white)</param>
    /// <param name="edgeColor">The other color of the edge piece (e.g., green for white-green edge)</param>
    /// <returns>The classified case for this edge</returns>
    public static CrossEdgeCase ClassifyEdgePosition(Cube cube, CubeColor crossColor, CubeColor edgeColor)
    {
        // Find the edge piece
        var edge = FindCrossEdge(cube, crossColor, edgeColor);
        if (edge == null)
        {
            throw new InvalidOperationException($"Edge with colors {crossColor} and {edgeColor} not found");
        }
        
        // Transform both position and colors to be viewed from the edge color's perspective
        var transformedPosition = TransformPositionForEdgeColor(edge.Position, edgeColor);
        var transformedColors = TransformColorsForEdgeColor(edge.Colors, edgeColor);
        
        // Create a transformed edge with both transformed position AND colors
        var transformedEdge = new CubePiece(edge.SolvedPosition, transformedColors)
        {
            Position = transformedPosition
        };
        
        // Classify based on transformed position (always relative to canonical front)
        var targetPosition = new Position3D(0, -1, 1); // Always canonical front
        
        return ClassifyEdge(transformedEdge, targetPosition, crossColor, edgeColor);
    }
    
    /// <summary>
    /// Transforms colors array to be viewed from the specified edge color's perspective
    /// </summary>
    private static CubeColor?[] TransformColorsForEdgeColor(CubeColor?[] colors, CubeColor edgeColor)
    {
        return edgeColor switch
        {
            CubeColor.Green => colors, // No transformation needed - canonical view
            CubeColor.Orange => ApplyYRotationToColors(colors), // y rotation: X→Z, Z→-X
            CubeColor.Blue => ApplyY2RotationToColors(colors), // y2 rotation: X→-X, Z→-Z
            CubeColor.Red => ApplyYPrimeRotationToColors(colors), // y' rotation: X→-Z, Z→X
            _ => throw new ArgumentException($"Invalid edge color: {edgeColor}")
        };
    }
    
    /// <summary>
    /// Transforms a position to be viewed from the specified edge color's perspective
    /// </summary>
    private static Position3D TransformPositionForEdgeColor(Position3D position, CubeColor edgeColor)
    {
        return edgeColor switch
        {
            CubeColor.Green => position, // No transformation needed - canonical view
            CubeColor.Orange => ApplyYRotation(position), // y rotation: F→R→B→L→F
            CubeColor.Blue => ApplyY2Rotation(position), // y2 rotation: F↔B, R↔L
            CubeColor.Red => ApplyYPrimeRotation(position), // y' rotation: F→L→B→R→F
            _ => throw new ArgumentException($"Invalid edge color: {edgeColor}")
        };
    }
    
    /// <summary>
    /// Apply Y rotation transformation (clockwise from above)
    /// F→R, R→B, B→L, L→F
    /// </summary>
    private static Position3D ApplyYRotation(Position3D pos)
    {
        // Y rotation swaps X and Z coordinates with sign changes
        // (0,y,1) → (1,y,0), (1,y,0) → (0,y,-1), (0,y,-1) → (-1,y,0), (-1,y,0) → (0,y,1)
        return new Position3D(-pos.Z, pos.Y, pos.X);
    }
    
    /// <summary>
    /// Apply Y2 rotation transformation (180 degrees from above)
    /// F↔B, R↔L
    /// </summary>
    private static Position3D ApplyY2Rotation(Position3D pos)
    {
        // Y2 rotation negates both X and Z coordinates
        // (0,y,1) → (0,y,-1), (1,y,0) → (-1,y,0), etc.
        return new Position3D(-pos.X, pos.Y, -pos.Z);
    }
    
    /// <summary>
    /// Apply Y' rotation transformation (counter-clockwise from above)  
    /// F→L, R→F, B→R, L→B
    /// </summary>
    private static Position3D ApplyYPrimeRotation(Position3D pos)
    {
        // Y' rotation is opposite of Y rotation
        // (0,y,1) → (-1,y,0), (1,y,0) → (0,y,1), (0,y,-1) → (1,y,0), (-1,y,0) → (0,y,-1)
        return new Position3D(pos.Z, pos.Y, -pos.X);
    }
    
    /// <summary>
    /// Apply Y rotation transformation to colors array (X,Y,Z) → (-Z,Y,X)
    /// </summary>
    private static CubeColor?[] ApplyYRotationToColors(CubeColor?[] colors)
    {
        // Y rotation: (x, y, z) → (-z, y, x)
        // Colors array [X, Y, Z] → [-Z, Y, X] = [Z, Y, X] (since -Z has same color as Z)
        return new CubeColor?[] { colors[2], colors[1], colors[0] };
    }
    
    /// <summary>
    /// Apply Y2 rotation transformation to colors array (X,Y,Z) → (-X,Y,-Z)
    /// </summary>
    private static CubeColor?[] ApplyY2RotationToColors(CubeColor?[] colors)
    {
        // Y2 rotation: (x, y, z) → (-x, y, -z)
        // Colors array [X, Y, Z] → [-X, Y, -Z] = [X, Y, Z] (since -X/-Z have same colors as X/Z)
        return new CubeColor?[] { colors[0], colors[1], colors[2] };
    }
    
    /// <summary>
    /// Apply Y' rotation transformation to colors array (X,Y,Z) → (Z,Y,-X)
    /// </summary>
    private static CubeColor?[] ApplyYPrimeRotationToColors(CubeColor?[] colors)
    {
        // Y' rotation: (x, y, z) → (z, y, -x)
        // Colors array [X, Y, Z] → [Z, Y, -X] = [Z, Y, X] (since -X has same color as X)
        return new CubeColor?[] { colors[2], colors[1], colors[0] };
    }
    
    /// <summary>
    /// Finds the edge piece with the specified colors
    /// </summary>
    private static CubePiece? FindCrossEdge(Cube cube, CubeColor crossColor, CubeColor edgeColor)
    {
        return cube.Edges.FirstOrDefault(e => 
            e.Colors.Contains(crossColor) && e.Colors.Contains(edgeColor));
    }
    
    
    /// <summary>
    /// Classifies the edge based on its position relative to target
    /// </summary>
    private static CrossEdgeCase ClassifyEdge(CubePiece edge, Position3D targetPosition, 
        CubeColor crossColor, CubeColor edgeColor)
    {
        var currentPosition = edge.Position;
        var layer = GetLayer(currentPosition);
        
        // Check if edge is correctly oriented (cross color facing correct direction)
        var isCorrectlyOriented = IsEdgeCorrectlyOriented(edge, currentPosition, crossColor);
        
        switch (layer)
        {
            case Layer.Bottom:
                return ClassifyBottomLayerEdge(currentPosition, targetPosition, isCorrectlyOriented);
            case Layer.Middle:
                return ClassifyMiddleLayerEdge(currentPosition, targetPosition, isCorrectlyOriented);
            case Layer.Top:
                return ClassifyTopLayerEdge(currentPosition, targetPosition, isCorrectlyOriented);
            default:
                throw new InvalidOperationException($"Unknown layer for position {currentPosition}");
        }
    }
    
    /// <summary>
    /// Determines which layer a position is in
    /// </summary>
    private static Layer GetLayer(Position3D position)
    {
        return position.Y switch
        {
            -1 => Layer.Bottom,
            0 => Layer.Middle,
            1 => Layer.Top,
            _ => throw new ArgumentException($"Invalid Y coordinate: {position.Y}")
        };
    }
    
    /// <summary>
    /// Checks if the cross color is facing the correct direction for its current position
    /// </summary>
    private static bool IsEdgeCorrectlyOriented(CubePiece edge, Position3D position, CubeColor crossColor)
    {
        // Colors array is [X, Y, Z] where each index corresponds to color on that axis
        var crossColorIndex = Array.IndexOf(edge.Colors, crossColor);
        if (crossColorIndex == -1)
        {
            throw new InvalidOperationException("Cross color not found in edge");
        }
        
        var layer = GetLayer(position);
        
        return layer switch
        {
            // Bottom layer: cross color should face down (Y axis, index 1)
            Layer.Bottom => crossColorIndex == 1,
            
            // Middle layer: cross color should face towards center (not on the outer face)
            // For edges in middle layer, the cross color should be on the axis that goes to center
            Layer.Middle => IsMiddleLayerCorrectlyOriented(position, crossColorIndex),
            
            // Top layer: cross color should face up (Y axis, index 1) 
            Layer.Top => crossColorIndex == 1,
            
            _ => false
        };
    }
    
    /// <summary>
    /// Checks orientation for middle layer edges
    /// </summary>
    private static bool IsMiddleLayerCorrectlyOriented(Position3D position, int crossColorIndex)
    {
        // For middle layer edges, cross color should face towards center
        // Position (1,0,1) = front-right edge, cross color should be on X axis (index 0) facing inward
        // Position (1,0,-1) = right-back edge, cross color should be on X axis (index 0) facing inward
        // Position (-1,0,-1) = back-left edge, cross color should be on X axis (index 0) facing inward  
        // Position (-1,0,1) = left-front edge, cross color should be on X axis (index 0) facing inward
        
        // All middle layer cross edges have X != 0, so cross color should be on X axis (index 0)
        return crossColorIndex == 0;
    }
    
    /// <summary>
    /// Classifies bottom layer edge cases
    /// </summary>
    private static CrossEdgeCase ClassifyBottomLayerEdge(Position3D currentPos, Position3D targetPos, bool isCorrect)
    {
        // Check if edge is in the correct position
        if (currentPos.Equals(targetPos))
        {
            return isCorrect ? CrossEdgeCase.BottomFrontAligned : CrossEdgeCase.BottomFrontFlipped;
        }
        
        // Determine relative position
        var relativePos = GetRelativeBottomPosition(currentPos, targetPos);
        
        return (relativePos, isCorrect) switch
        {
            (RelativePosition.Front, true) => CrossEdgeCase.BottomFrontAligned,
            (RelativePosition.Front, false) => CrossEdgeCase.BottomFrontFlipped,
            (RelativePosition.Right, true) => CrossEdgeCase.BottomRightAligned,
            (RelativePosition.Right, false) => CrossEdgeCase.BottomRightFlipped,
            (RelativePosition.Back, true) => CrossEdgeCase.BottomBackAligned,
            (RelativePosition.Back, false) => CrossEdgeCase.BottomBackFlipped,
            (RelativePosition.Left, true) => CrossEdgeCase.BottomLeftAligned,
            (RelativePosition.Left, false) => CrossEdgeCase.BottomLeftFlipped,
            _ => throw new InvalidOperationException($"Unexpected relative position: {relativePos}")
        };
    }
    
    /// <summary>
    /// Classifies middle layer edge cases
    /// </summary>
    private static CrossEdgeCase ClassifyMiddleLayerEdge(Position3D currentPos, Position3D targetPos, bool isCorrect)
    {
        // Middle layer positions: (1,0,1), (1,0,-1), (-1,0,-1), (-1,0,1)
        // Map to cases relative to front target
        
        return (currentPos.X, currentPos.Z, isCorrect) switch
        {
            (1, 1, true) => CrossEdgeCase.MiddleFrontRightAligned,   // (1,0,1)
            (1, 1, false) => CrossEdgeCase.MiddleFrontRightFlipped,
            (1, -1, true) => CrossEdgeCase.MiddleRightBackAligned,    // (1,0,-1)
            (1, -1, false) => CrossEdgeCase.MiddleRightBackFlipped,
            (-1, -1, true) => CrossEdgeCase.MiddleBackLeftAligned,    // (-1,0,-1)
            (-1, -1, false) => CrossEdgeCase.MiddleBackLeftFlipped,
            (-1, 1, true) => CrossEdgeCase.MiddleLeftFrontAligned,    // (-1,0,1)
            (-1, 1, false) => CrossEdgeCase.MiddleLeftFrontFlipped,
            _ => throw new InvalidOperationException($"Unexpected middle layer position: {currentPos}")
        };
    }
    
    /// <summary>
    /// Classifies top layer edge cases  
    /// </summary>
    private static CrossEdgeCase ClassifyTopLayerEdge(Position3D currentPos, Position3D targetPos, bool isCorrect)
    {
        // Top layer positions: (0,1,1), (1,1,0), (0,1,-1), (-1,1,0)
        // Map to cases relative to front target
        
        return (currentPos.X, currentPos.Z, isCorrect) switch
        {
            (0, 1, true) => CrossEdgeCase.TopFrontAligned,      // (0,1,1)
            (0, 1, false) => CrossEdgeCase.TopFrontFlipped,
            (1, 0, true) => CrossEdgeCase.TopRightAligned,      // (1,1,0)
            (1, 0, false) => CrossEdgeCase.TopRightFlipped,
            (0, -1, true) => CrossEdgeCase.TopBackAligned,      // (0,1,-1)
            (0, -1, false) => CrossEdgeCase.TopBackFlipped,
            (-1, 0, true) => CrossEdgeCase.TopLeftAligned,      // (-1,1,0)
            (-1, 0, false) => CrossEdgeCase.TopLeftFlipped,
            _ => throw new InvalidOperationException($"Unexpected top layer position: {currentPos}")
        };
    }
    
    /// <summary>
    /// Gets the relative position of current position to target in bottom layer
    /// </summary>
    private static RelativePosition GetRelativeBottomPosition(Position3D current, Position3D target)
    {
        // All bottom layer edge positions: (0,-1,1), (1,-1,0), (0,-1,-1), (-1,-1,0)
        // Calculate relative position based on X,Z coordinates
        
        // Map current position to relative position names
        return (current.X, current.Z) switch
        {
            (0, 1) => RelativePosition.Front,    // (0,-1,1) - front position
            (1, 0) => RelativePosition.Right,    // (1,-1,0) - right position
            (0, -1) => RelativePosition.Back,    // (0,-1,-1) - back position  
            (-1, 0) => RelativePosition.Left,    // (-1,-1,0) - left position
            _ => throw new InvalidOperationException($"Invalid bottom layer position: {current}")
        };
    }
    
    private enum Layer
    {
        Bottom,
        Middle,
        Top
    }
    
    private enum RelativePosition
    {
        Front,
        Right,
        Back,
        Left
    }
}