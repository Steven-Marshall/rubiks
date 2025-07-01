using RubiksCube.Core.Models;
using RubiksCube.Core.Algorithms;
using CSharpFunctionalExtensions;

namespace RubiksCube.Core.Scrambling;

public class ScrambleGenerator
{
    private readonly Random _random;
    private readonly char[] _faces = { 'U', 'F', 'R', 'L', 'D', 'B' };
    private readonly string[] _modifiers = { "", "'", "2" };

    public ScrambleGenerator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public Result<Algorithm> GenerateScramble(int moveCount = 25, bool strictWcaRules = true)
    {
        if (moveCount <= 0)
            return Result.Failure<Algorithm>("Move count must be positive");

        if (moveCount > 100)
            return Result.Failure<Algorithm>("Move count cannot exceed 100 moves");

        var moves = new List<string>();
        
        for (int i = 0; i < moveCount; i++)
        {
            var move = GenerateValidMove(moves, strictWcaRules);
            if (move == null)
            {
                return Result.Failure<Algorithm>($"Unable to generate valid move at position {i + 1}");
            }
            moves.Add(move);
        }

        var algorithmString = string.Join(" ", moves);
        return Algorithm.Parse(algorithmString);
    }

    private string? GenerateValidMove(List<string> previousMoves, bool strictWcaRules)
    {
        const int maxAttempts = 50;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var face = _faces[_random.Next(_faces.Length)];
            var modifier = _modifiers[_random.Next(_modifiers.Length)];
            var candidateMove = $"{face}{modifier}";

            if (IsValidMove(candidateMove, previousMoves, strictWcaRules))
            {
                return candidateMove;
            }
        }

        return null;
    }

    private bool IsValidMove(string candidateMove, List<string> previousMoves, bool strictWcaRules)
    {
        if (previousMoves.Count == 0)
            return true;

        var candidateFace = candidateMove[0];
        var lastMove = previousMoves[^1];
        var lastFace = lastMove[0];

        // Rule 1: No consecutive same-face moves
        if (candidateFace == lastFace)
            return false;

        if (strictWcaRules && previousMoves.Count >= 2)
        {
            var secondLastMove = previousMoves[^2];
            var secondLastFace = secondLastMove[0];
            
            // Rule 2: No opposite faces within 2 moves
            if (AreOppositeFaces(candidateFace, secondLastFace))
                return false;
        }

        return true;
    }

    private static bool AreOppositeFaces(char face1, char face2)
    {
        return (face1, face2) switch
        {
            ('U', 'D') or ('D', 'U') => true,
            ('F', 'B') or ('B', 'F') => true,
            ('R', 'L') or ('L', 'R') => true,
            _ => false
        };
    }
}