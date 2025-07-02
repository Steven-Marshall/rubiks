namespace RubiksCube.Core.Solving;

/// <summary>
/// Represents all 24 possible cases for a cross edge piece
/// Named by Layer_Position_Orientation for clarity
/// "Aligned" = white sticker facing correct direction for that layer
/// "Flipped" = white sticker facing wrong direction for that layer
/// </summary>
public enum CrossEdgeCase
{
    // Bottom Layer Cases (Y = -1)
    BottomFrontAligned,    // Case 1a: Front position, white down
    BottomFrontFlipped,    // Case 1b: Front position, white on side
    BottomRightAligned,    // Case 1c: Right position, white down
    BottomRightFlipped,    // Case 1d: Right position, white on side
    BottomBackAligned,     // Case 1e: Back position, white down
    BottomBackFlipped,     // Case 1f: Back position, white on side
    BottomLeftAligned,     // Case 1g: Left position, white down
    BottomLeftFlipped,     // Case 1h: Left position, white on side
    
    // Middle Layer Cases (Y = 0)
    MiddleFrontRightAligned,  // Case 2a: Front-right, white facing inward
    MiddleFrontRightFlipped,  // Case 2b: Front-right, white facing outward
    MiddleRightBackAligned,   // Case 2c: Right-back, white facing inward
    MiddleRightBackFlipped,   // Case 2d: Right-back, white facing outward
    MiddleBackLeftAligned,    // Case 2e: Back-left, white facing inward
    MiddleBackLeftFlipped,    // Case 2f: Back-left, white facing outward
    MiddleLeftFrontAligned,   // Case 2g: Left-front, white facing inward
    MiddleLeftFrontFlipped,   // Case 2h: Left-front, white facing outward
    
    // Top Layer Cases (Y = 1)
    TopFrontAligned,       // Case 3a: Front position, white up
    TopFrontFlipped,       // Case 3b: Front position, white on side
    TopRightAligned,       // Case 3c: Right position, white up
    TopRightFlipped,       // Case 3d: Right position, white on side
    TopBackAligned,        // Case 3e: Back position, white up
    TopBackFlipped,        // Case 3f: Back position, white on side
    TopLeftAligned,        // Case 3g: Left position, white up
    TopLeftFlipped         // Case 3h: Left position, white on side
}