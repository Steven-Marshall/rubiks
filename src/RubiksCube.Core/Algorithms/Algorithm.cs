using CSharpFunctionalExtensions;

namespace RubiksCube.Core.Algorithms;

/// <summary>
/// Represents a sequence of moves that can be applied to a Rubik's cube
/// Supports both reorientations (x, y, z) and rotations (R, L, U, D, F, B)
/// </summary>
public class Algorithm : IEquatable<Algorithm>
{
    private readonly List<Move> _moves;
    
    /// <summary>
    /// The sequence of moves in this algorithm
    /// </summary>
    public IReadOnlyList<Move> Moves => _moves.AsReadOnly();
    
    /// <summary>
    /// True if this algorithm contains no moves
    /// </summary>
    public bool IsEmpty => _moves.Count == 0;
    
    /// <summary>
    /// The number of moves in this algorithm
    /// </summary>
    public int Length => _moves.Count;
    
    /// <summary>
    /// Creates an empty algorithm
    /// </summary>
    public Algorithm() : this(new List<Move>())
    {
    }
    
    /// <summary>
    /// Creates an algorithm from a sequence of moves
    /// </summary>
    public Algorithm(IEnumerable<Move> moves)
    {
        _moves = new List<Move>(moves ?? throw new ArgumentNullException(nameof(moves)));
    }
    
    /// <summary>
    /// Creates an algorithm from a single move
    /// </summary>
    public Algorithm(Move move)
    {
        _moves = new List<Move> { move ?? throw new ArgumentNullException(nameof(move)) };
    }
    
    /// <summary>
    /// Parses an algorithm from Singmaster notation
    /// Supports moves like: R, U, R', U2, x, y', z2
    /// Handles sequences like: "R U R' U'" or "x R U R' U' x'"
    /// </summary>
    public static Result<Algorithm> Parse(string algorithmString)
    {
        if (string.IsNullOrWhiteSpace(algorithmString))
            return Result.Success(new Algorithm()); // Empty algorithm
            
        try
        {
            var moves = new List<Move>();
            var tokens = TokenizeAlgorithm(algorithmString);
            
            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token))
                    continue;
                    
                var move = Move.Parse(token);
                moves.Add(move);
            }
            
            return Result.Success(new Algorithm(moves));
        }
        catch (Exception ex)
        {
            return Result.Failure<Algorithm>($"Failed to parse algorithm '{algorithmString}': {ex.Message}");
        }
    }
    
    /// <summary>
    /// Tokenizes an algorithm string into individual move strings
    /// Handles spaces, parentheses (ignored), and various notation styles
    /// </summary>
    private static IEnumerable<string> TokenizeAlgorithm(string algorithmString)
    {
        // Remove parentheses - they're just grouping aids
        var cleaned = algorithmString.Replace("(", "").Replace(")", "");
        
        // Split by whitespace and filter out empty tokens
        var tokens = cleaned.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        return tokens.Where(s => !string.IsNullOrWhiteSpace(s));
    }
    
    /// <summary>
    /// Gets the inverse of this algorithm (all moves in reverse order, each inverted)
    /// </summary>
    public Algorithm Inverse()
    {
        var inverseMoves = _moves.AsEnumerable().Reverse().Select(m => m.Inverse());
        return new Algorithm(inverseMoves);
    }
    
    /// <summary>
    /// Concatenates this algorithm with another
    /// </summary>
    public Algorithm Concat(Algorithm other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        
        var combinedMoves = _moves.Concat(other._moves);
        return new Algorithm(combinedMoves);
    }
    
    /// <summary>
    /// Repeats this algorithm the specified number of times
    /// </summary>
    public Algorithm Repeat(int times)
    {
        if (times < 0) throw new ArgumentException("Repeat count must be non-negative");
        if (times == 0) return new Algorithm();
        
        var repeatedMoves = new List<Move>();
        for (int i = 0; i < times; i++)
        {
            repeatedMoves.AddRange(_moves);
        }
        
        return new Algorithm(repeatedMoves);
    }
    
    /// <summary>
    /// Converts the algorithm back to Singmaster notation
    /// </summary>
    public override string ToString()
    {
        if (IsEmpty) return "";
        return string.Join(" ", _moves.Select(m => m.ToString()));
    }
    
    /// <summary>
    /// Gets a detailed string representation
    /// </summary>
    public string ToDetailedString()
    {
        if (IsEmpty) return "Empty algorithm";
        
        var moveDescriptions = _moves.Select(m => m.ToString());
        return string.Join(", ", moveDescriptions);
    }
    
    public bool Equals(Algorithm? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _moves.SequenceEqual(other._moves);
    }
    
    public override bool Equals(object? obj) => obj is Algorithm other && Equals(other);
    
    public override int GetHashCode()
    {
        return _moves.Aggregate(0, (acc, move) => HashCode.Combine(acc, move));
    }
    
    public static bool operator ==(Algorithm? left, Algorithm? right) =>
        ReferenceEquals(left, right) || (left?.Equals(right) ?? false);
        
    public static bool operator !=(Algorithm? left, Algorithm? right) => !(left == right);
    
    /// <summary>
    /// Implicit conversion from string to Algorithm
    /// </summary>
    public static implicit operator Algorithm(string algorithmString)
    {
        var result = Parse(algorithmString);
        if (result.IsFailure)
            throw new ArgumentException($"Invalid algorithm: {result.Error}");
        return result.Value;
    }
}