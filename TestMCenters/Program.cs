using RubiksCube.Core.Models;
using RubiksCube.Core.Algorithms;

// Test E move
var cube = Cube.CreateSolved();
cube.ApplyMove(new Move('E'));

var eFrontCenter = cube.Pieces.Single(p => p.SolvedPosition.Equals(new Position3D(0, 0, 1)));
var eRightCenter = cube.Pieces.Single(p => p.SolvedPosition.Equals(new Position3D(1, 0, 0)));
var eBackCenter = cube.Pieces.Single(p => p.SolvedPosition.Equals(new Position3D(0, 0, -1)));
var eLeftCenter = cube.Pieces.Single(p => p.SolvedPosition.Equals(new Position3D(-1, 0, 0)));

Console.WriteLine($"After E move:");
Console.WriteLine($"  Front center (0,0,1) is now at: {eFrontCenter.Position}");
Console.WriteLine($"  Right center (1,0,0) is now at: {eRightCenter.Position}");
Console.WriteLine($"  Back center (0,0,-1) is now at: {eBackCenter.Position}");
Console.WriteLine($"  Left center (-1,0,0) is now at: {eLeftCenter.Position}");