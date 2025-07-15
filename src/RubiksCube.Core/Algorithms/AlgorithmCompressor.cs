using CSharpFunctionalExtensions;

namespace RubiksCube.Core.Algorithms;

/// <summary>
/// Utility for compressing algorithm strings by applying basic mathematical simplifications
/// </summary>
public static class AlgorithmCompressor
{
    /// <summary>
    /// Compresses an algorithm by removing redundant moves and combining adjacent moves
    /// </summary>
    /// <param name="algorithmString">The algorithm string to compress (e.g., "R U R' U'")</param>
    /// <returns>Compressed algorithm string</returns>
    public static string Compress(string algorithmString)
    {
        if (string.IsNullOrWhiteSpace(algorithmString))
            return "";
            
        var parseResult = Algorithm.Parse(algorithmString);
        if (parseResult.IsFailure)
            return algorithmString; // Return original if parsing fails
            
        var moves = parseResult.Value.Moves.ToList();
        var compressed = CompressMoves(moves);
        
        if (compressed.Count == 0)
            return "";
            
        return string.Join(" ", compressed.Select(m => m.ToString()));
    }
    
    /// <summary>
    /// Compresses a list of moves by applying basic mathematical simplifications
    /// </summary>
    private static List<Move> CompressMoves(List<Move> moves)
    {
        if (moves.Count == 0)
            return new List<Move>();
            
        var result = new List<Move>();
        
        for (int i = 0; i < moves.Count; i++)
        {
            var currentMove = moves[i];
            
            // Look ahead to see if we can combine with next moves
            var combinedMove = CombineWithFollowingMoves(moves, i, out int movesConsumed);
            
            if (combinedMove != null)
            {
                // We got a combined move, add it
                result.Add(combinedMove);
            }
            else if (movesConsumed == 1)
            {
                // No combination was possible, add the original move
                result.Add(currentMove);
            }
            // If movesConsumed > 1 but combinedMove is null, the moves canceled out - add nothing
            
            // Always skip the consumed moves (subtract 1 because loop will increment)
            i += movesConsumed - 1;
        }
        
        return result;
    }
    
    /// <summary>
    /// Attempts to combine a move with following moves of the same face
    /// </summary>
    private static Move? CombineWithFollowingMoves(List<Move> moves, int startIndex, out int movesConsumed)
    {
        movesConsumed = 1;
        
        if (startIndex >= moves.Count)
            return null;
            
        var baseMove = moves[startIndex];
        var totalRotation = GetRotationAmount(baseMove);
        
        // Look ahead for same-face moves
        for (int i = startIndex + 1; i < moves.Count; i++)
        {
            var nextMove = moves[i];
            
            // Stop if different face or different move type (wide vs regular)
            if (!IsSameFace(nextMove, baseMove))
                break;
                
            totalRotation += GetRotationAmount(nextMove);
            movesConsumed++;
        }
        
        // Normalize rotation to 0, 1, 2, or 3 quarter turns
        totalRotation = ((totalRotation % 4) + 4) % 4;
        
        // Return appropriate move or null if no move needed
        return totalRotation switch
        {
            0 => null, // No net rotation
            1 => new Move(baseMove.Face, MoveDirection.Clockwise),
            2 => new Move(baseMove.Face, MoveDirection.Double),
            3 => new Move(baseMove.Face, MoveDirection.CounterClockwise),
            _ => null
        };
    }
    
    /// <summary>
    /// Checks if two moves affect the same face
    /// </summary>
    private static bool IsSameFace(Move move1, Move move2)
    {
        return move1.Face == move2.Face && move1.IsWide == move2.IsWide;
    }
    
    /// <summary>
    /// Gets the rotation amount for a move in quarter turns (positive = clockwise)
    /// </summary>
    private static int GetRotationAmount(Move move)
    {
        return move.Direction switch
        {
            MoveDirection.Clockwise => 1,
            MoveDirection.Double => 2,
            MoveDirection.CounterClockwise => 3, // Same as -1, but we use +3 for easier modulo math
            _ => 0
        };
    }
}