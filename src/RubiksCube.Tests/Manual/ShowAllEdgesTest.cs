using Xunit;
using RubiksCube.Core.Models;
using RubiksCube.Core.Solving;

namespace RubiksCube.Tests.Manual;

public class ShowAllEdgesTest
{
    [Fact]
    public void ShowAllCrossEdgeAnalysis()
    {
        // Create the same scrambled cube directly
        var cube = Cube.CreateSolved();
        
        // Apply the scramble we used: "U F R L F' L' F L' U D F' L2 U D U2 L2 F D2 R' F U L2 B2 L U2"
        var scrambleMoves = "U F R L F' L' F L' U D F' L2 U D U2 L2 F D2 R' F U L2 B2 L U2".Split(' ');
        foreach (var moveStr in scrambleMoves)
        {
            var move = RubiksCube.Core.Algorithms.Move.Parse(moveStr);
            cube.ApplyMove(move);
        }
        
        // Now analyze all edges
        var solver = new CrossSolver(CubeColor.White);
        var analysis = solver.AnalyzeAllCrossEdges(cube);
        
        // Force output by failing the test with the analysis
        Assert.True(false, $"Cross edge analysis:\n{analysis}");
    }
}