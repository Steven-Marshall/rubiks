using RubiksCube.Core.Models;
using RubiksCube.Core.Display;
using RubiksCube.Core.Storage;
using RubiksCube.Core.Algorithms;
using RubiksCube.Core.Scrambling;
using RubiksCube.Core.PatternRecognition;
using RubiksCube.Core.Solving;
using System.Text.Json;

namespace RubiksCube.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return 0;
            }

            string command = args[0].ToLowerInvariant();
            
            return command switch
            {
                "create" => await HandleCreate(args),
                "apply" => await HandleApply(args),
                "display" => await HandleDisplay(args),
                "list" => HandleList(),
                "delete" => HandleDelete(args),
                "export" => HandleExport(args),
                "scramble-gen" => HandleScrambleGen(args),
                "analyze" => await HandleAnalyze(args),
                "solve" => await HandleSolve(args),
                "help" or "--help" or "-h" => HandleHelp(),
                _ => HandleUnknownCommand(command)
            };
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> HandleCreate(string[] args)
    {
        try
        {
            var cube = Cube.CreateSolved();
            
            // Check if cube name provided
            if (args.Length > 1)
            {
                string cubeName = args[1];
                var saveResult = CubeStorageService.Save(cube, cubeName);
                if (saveResult.IsFailure)
                {
                    await Console.Error.WriteLineAsync($"Error saving cube: {saveResult.Error}");
                    return 1;
                }
                // For named saves, don't output JSON to stdout
                return 0;
            }
            
            // No cube name - output JSON to stdout (for piping)
            var json = cube.ToJson();
            Console.WriteLine(json);
            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error creating cube: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> HandleApply(string[] args)
    {
        if (args.Length < 2)
        {
            await Console.Error.WriteLineAsync("Error: Algorithm required for 'apply' command");
            await Console.Error.WriteLineAsync("Usage: rubiks apply \"R U R' U'\" [cube-name]");
            await Console.Error.WriteLineAsync("       echo cube-json | rubiks apply \"R U R' U'\"");
            return 1;
        }

        try
        {
            string algorithmString = args[1];
            string? cubeName = args.Length > 2 ? args[2] : null;

            // Parse the algorithm
            var algorithmResult = Algorithm.Parse(algorithmString);
            if (algorithmResult.IsFailure)
            {
                await Console.Error.WriteLineAsync($"Error parsing algorithm: {algorithmResult.Error}");
                return 1;
            }

            var algorithm = algorithmResult.Value;

            // Load cube from name or stdin
            Cube cube;
            if (cubeName != null)
            {
                var loadResult = CubeStorageService.Load(cubeName);
                if (loadResult.IsFailure)
                {
                    await Console.Error.WriteLineAsync($"Error loading cube: {loadResult.Error}");
                    return 1;
                }
                cube = loadResult.Value;
            }
            else
            {
                // Read from stdin (for piping)
                var cubeJson = await Console.In.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(cubeJson))
                {
                    await Console.Error.WriteLineAsync("Error: No cube data provided via stdin");
                    return 1;
                }
                cube = Cube.FromJson(cubeJson.Trim());
            }

            // Apply each move in the algorithm
            foreach (var move in algorithm.Moves)
            {
                cube.ApplyMove(move);
            }

            // Output result
            if (cubeName != null)
            {
                // Save the modified cube back to the named file
                var saveResult = CubeStorageService.Save(cube, cubeName);
                if (saveResult.IsFailure)
                {
                    await Console.Error.WriteLineAsync($"Error saving cube: {saveResult.Error}");
                    return 1;
                }
                // For named operations, don't output JSON to stdout
                return 0;
            }
            else
            {
                // Output JSON to stdout (for piping)
                var json = cube.ToJson();
                Console.WriteLine(json);
                return 0;
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error applying algorithm: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> HandleDisplay(string[] args)
    {
        try
        {
            string format = "unicode";
            string? cubeName = null;
            
            // Parse arguments
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--format" || args[i] == "-f")
                {
                    if (i + 1 < args.Length)
                    {
                        format = args[i + 1];
                        i++; // Skip the format value
                    }
                    else
                    {
                        await Console.Error.WriteLineAsync("Error: --format requires a value (ascii or unicode)");
                        return 1;
                    }
                }
                else if (args[i].StartsWith("--format="))
                {
                    format = args[i].Substring("--format=".Length);
                }
                else if (!args[i].StartsWith("-"))
                {
                    // Assume it's a cube name
                    cubeName = args[i];
                }
            }

            Cube cube;
            
            // Load cube from name or stdin
            if (cubeName != null)
            {
                var loadResult = CubeStorageService.Load(cubeName);
                if (loadResult.IsFailure)
                {
                    await Console.Error.WriteLineAsync($"Error loading cube: {loadResult.Error}");
                    return 1;
                }
                cube = loadResult.Value;
            }
            else
            {
                // Read from stdin (for piping)
                var cubeJson = await Console.In.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(cubeJson))
                {
                    await Console.Error.WriteLineAsync("Error: No cube data provided via stdin");
                    return 1;
                }
                cube = Cube.FromJson(cubeJson.Trim());
            }

            // Use PythonRenderer with format support
            var cubeRenderer = new CubeRenderer(cube);
            
            string output = format.ToLowerInvariant() switch
            {
                "ascii" => cubeRenderer.RenderAscii(),
                "unicode" => cubeRenderer.RenderUnicode(),
                _ => throw new ArgumentException($"Unsupported format: {format}. Use 'ascii' or 'unicode'.")
            };
            
            Console.WriteLine(output);
            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error displaying cube: {ex.Message}");
            return 1;
        }
    }

    private static int HandleList()
    {
        try
        {
            var cubes = CubeStorageService.List();
            if (!cubes.Any())
            {
                Console.WriteLine("No saved cubes found.");
                return 0;
            }

            Console.WriteLine("Saved cubes:");
            foreach (var cubeName in cubes)
            {
                Console.WriteLine($"  {cubeName}");
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error listing cubes: {ex.Message}");
            return 1;
        }
    }

    private static int HandleDelete(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Error: Cube name required for 'delete' command");
            Console.Error.WriteLine("Usage: rubiks delete cube-name");
            return 1;
        }

        try
        {
            string cubeName = args[1];
            var result = CubeStorageService.Delete(cubeName);
            if (result.IsFailure)
            {
                Console.Error.WriteLine($"Error deleting cube: {result.Error}");
                return 1;
            }

            Console.WriteLine($"Cube '{cubeName}' deleted successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting cube: {ex.Message}");
            return 1;
        }
    }

    private static int HandleExport(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Error: Cube name required for 'export' command");
            Console.Error.WriteLine("Usage: rubiks export cube-name");
            return 1;
        }

        try
        {
            string cubeName = args[1];
            var result = CubeStorageService.Export(cubeName);
            if (result.IsFailure)
            {
                Console.Error.WriteLine($"Error exporting cube: {result.Error}");
                return 1;
            }

            Console.WriteLine(result.Value);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error exporting cube: {ex.Message}");
            return 1;
        }
    }

    private static int HandleScrambleGen(string[] args)
    {
        try
        {
            int moveCount = 25; // WCA default
            int? seed = null;
            
            // Parse arguments
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--moves" || args[i] == "-m")
                {
                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out var count))
                    {
                        moveCount = count;
                        i++; // Skip the value
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: --moves requires a numeric value");
                        return 1;
                    }
                }
                else if (args[i].StartsWith("--moves="))
                {
                    if (int.TryParse(args[i].Substring("--moves=".Length), out var count))
                    {
                        moveCount = count;
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: --moves requires a numeric value");
                        return 1;
                    }
                }
                else if (args[i] == "--seed" || args[i] == "-s")
                {
                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out var s))
                    {
                        seed = s;
                        i++; // Skip the value
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: --seed requires a numeric value");
                        return 1;
                    }
                }
                else if (args[i].StartsWith("--seed="))
                {
                    if (int.TryParse(args[i].Substring("--seed=".Length), out var s))
                    {
                        seed = s;
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: --seed requires a numeric value");
                        return 1;
                    }
                }
            }

            var generator = new ScrambleGenerator(seed);
            var result = generator.GenerateScramble(moveCount);

            if (result.IsFailure)
            {
                Console.Error.WriteLine($"Error generating scramble: {result.Error}");
                return 1;
            }

            Console.WriteLine(result.Value.ToString());
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error generating scramble: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> HandleAnalyze(string[] args)
    {
        try
        {
            string? cubeName = null;
            bool verbose = false;
            bool json = false;
            bool allEdges = false;
            
            // Parse arguments
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--verbose" || args[i] == "-v")
                {
                    verbose = true;
                }
                else if (args[i] == "--json" || args[i] == "-j")
                {
                    json = true;
                }
                else if (args[i] == "--all-edges" || args[i] == "-a")
                {
                    allEdges = true;
                }
                else if (!args[i].StartsWith("-"))
                {
                    // Assume it's a cube name
                    cubeName = args[i];
                }
            }

            Cube cube;
            
            // Load cube from name or stdin
            if (cubeName != null)
            {
                var loadResult = CubeStorageService.Load(cubeName);
                if (loadResult.IsFailure)
                {
                    await Console.Error.WriteLineAsync($"Error loading cube: {loadResult.Error}");
                    return 1;
                }
                cube = loadResult.Value;
            }
            else
            {
                // Read from stdin (for piping)
                var cubeJson = await Console.In.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(cubeJson))
                {
                    await Console.Error.WriteLineAsync("Error: No cube data provided via stdin");
                    return 1;
                }
                cube = Cube.FromJson(cubeJson.Trim());
            }

            // Check if we should show all cross edges analysis
            if (allEdges)
            {
                var crossSolver = new CrossSolver(CubeColor.White);
                var edgeAnalysis = crossSolver.AnalyzeAllCrossEdges(cube, verbose);
                Console.WriteLine("Cross edge analysis for white cross:");
                Console.WriteLine("=====================================");
                Console.WriteLine(edgeAnalysis);
                return 0;
            }

            // Analyze the cube
            var analyzer = new CubeStateAnalyzer();
            var result = analyzer.Analyze(cube);

            // Output result
            if (json)
            {
                var jsonOutput = new
                {
                    recognition = new
                    {
                        stage = result.Recognition.Stage,
                        complete = result.Recognition.IsComplete,
                        progress = result.Recognition.Progress,
                        total = result.Recognition.Total,
                        description = result.Recognition.Description,
                        details = result.Recognition.Details
                    },
                    suggestion = result.Suggestion != null ? new
                    {
                        algorithm = result.Suggestion.Algorithm,
                        description = result.Suggestion.Description,
                        next_stage = result.Suggestion.NextStage,
                        details = result.Suggestion.Details
                    } : null
                };
                
                Console.WriteLine(JsonSerializer.Serialize(jsonOutput, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                // Human-readable output
                Console.WriteLine(result.Recognition.Description);
                
                if (result.Suggestion != null)
                {
                    if (!string.IsNullOrEmpty(result.Suggestion.Algorithm))
                    {
                        Console.WriteLine($"Suggested: {result.Suggestion.Algorithm}");
                    }
                    else
                    {
                        Console.WriteLine("No moves needed");
                    }
                    
                    if (verbose && !string.IsNullOrEmpty(result.Suggestion.Description))
                    {
                        Console.WriteLine($"Description: {result.Suggestion.Description}");
                    }
                }
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error analyzing cube: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> HandleSolve(string[] args)
    {
        if (args.Length < 2)
        {
            await Console.Error.WriteLineAsync("Error: Solve stage required");
            await Console.Error.WriteLineAsync("Usage: rubiks solve cross [cube-name] [--verbose]");
            return 1;
        }

        string stage = args[1].ToLowerInvariant();
        if (stage != "cross")
        {
            await Console.Error.WriteLineAsync($"Error: Unsupported solve stage '{stage}'. Only 'cross' is currently supported.");
            return 1;
        }

        try
        {
            string? cubeName = null;
            bool verbose = false;
            
            // Parse arguments
            for (int i = 2; i < args.Length; i++)
            {
                if (args[i] == "--verbose" || args[i] == "-v")
                {
                    verbose = true;
                }
                else if (!args[i].StartsWith("-"))
                {
                    // Assume it's a cube name
                    cubeName = args[i];
                }
            }

            Cube cube;
            
            // Load cube from name or stdin
            if (cubeName != null)
            {
                var loadResult = CubeStorageService.Load(cubeName);
                if (loadResult.IsFailure)
                {
                    await Console.Error.WriteLineAsync($"Error loading cube: {loadResult.Error}");
                    return 1;
                }
                cube = loadResult.Value;
            }
            else
            {
                // Read from stdin (for piping)
                var cubeJson = await Console.In.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(cubeJson))
                {
                    await Console.Error.WriteLineAsync("Error: No cube data provided via stdin");
                    return 1;
                }
                cube = Cube.FromJson(cubeJson.Trim());
            }

            // Solve the white cross
            var crossSolver = new CrossSolver(CubeColor.White);
            var solution = SolveWhiteCross(cube, crossSolver, verbose);
            
            if (verbose)
            {
                Console.WriteLine(solution.VerboseOutput);
            }
            else
            {
                Console.WriteLine(solution.Algorithm);
            }

            // If named cube, save the solved state
            if (cubeName != null)
            {
                var saveResult = CubeStorageService.Save(solution.FinalCube, cubeName);
                if (saveResult.IsFailure)
                {
                    await Console.Error.WriteLineAsync($"Error saving cube: {saveResult.Error}");
                    return 1;
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error solving cross: {ex.Message}");
            return 1;
        }
    }

    private static (string Algorithm, string VerboseOutput, Cube FinalCube) SolveWhiteCross(Cube cube, CrossSolver crossSolver, bool verbose)
    {
        var workingCube = cube.Clone();
        var algorithmSteps = new List<string>();
        var verboseLines = new List<string>();
        
        if (verbose)
        {
            verboseLines.Add("Solving white cross step by step...");
            verboseLines.Add("");
        }

        int stepNumber = 1;
        while (true)
        {
            // Analyze current state
            var edgeColors = new[] { CubeColor.Green, CubeColor.Orange, CubeColor.Blue, CubeColor.Red };
            var edgeAnalysis = new List<(CubeColor Color, string Algorithm, int MoveCount)>();
            int solvedCount = 0;

            foreach (var edgeColor in edgeColors)
            {
                var edge = FindCrossEdge(workingCube, CubeColor.White, edgeColor);
                if (edge != null && edge.IsSolved)
                {
                    solvedCount++;
                    continue;
                }

                var caseType = CrossEdgeClassifier.ClassifyEdgePosition(workingCube, CubeColor.White, edgeColor);
                var preserveBottomLayer = CountSolvedCrossEdges(workingCube, CubeColor.White) > 0;
                var algorithm = CrossEdgeAlgorithms.GetAlgorithm(caseType, workingCube, CubeColor.White, preserveBottomLayer, edgeColor);
                
                if (!string.IsNullOrEmpty(algorithm))
                {
                    var parsedAlgo = Algorithm.Parse(algorithm);
                    int moveCount = parsedAlgo.IsSuccess ? parsedAlgo.Value.Length : 0;
                    edgeAnalysis.Add((edgeColor, algorithm, moveCount));
                }
            }

            // Check if cross is complete
            if (solvedCount == 4)
            {
                if (verbose)
                {
                    if (stepNumber == 1)
                    {
                        verboseLines.Add("Cross already solved!");
                    }
                    else
                    {
                        verboseLines.Add($"White cross complete! Total moves: {algorithmSteps.Sum(a => Algorithm.Parse(a).IsSuccess ? Algorithm.Parse(a).Value.Length : 0)}");
                        var fullAlgorithm = string.Join(" ", algorithmSteps);
                        var compressedAlgorithm = AlgorithmCompressor.Compress(fullAlgorithm);
                        verboseLines.Add($"Full algorithm: {compressedAlgorithm}");
                    }
                }
                break;
            }

            // Find shortest algorithm
            if (!edgeAnalysis.Any())
            {
                break; // No more edges to solve
            }

            var shortestEdge = edgeAnalysis.OrderBy(e => e.MoveCount).First();
            
            if (verbose)
            {
                var remainingCount = 4 - solvedCount;
                verboseLines.Add($"Step {stepNumber}: white-{shortestEdge.Color.ToString().ToLowerInvariant()} ({shortestEdge.MoveCount} move{(shortestEdge.MoveCount == 1 ? "" : "s")}) -> {shortestEdge.Algorithm}");
                verboseLines.Add($"Applied: {shortestEdge.Algorithm}");
            }

            // Apply the algorithm
            var algorithmResult = Algorithm.Parse(shortestEdge.Algorithm);
            if (algorithmResult.IsSuccess)
            {
                foreach (var move in algorithmResult.Value.Moves)
                {
                    workingCube.ApplyMove(move);
                }
                algorithmSteps.Add(shortestEdge.Algorithm);
            }

            if (verbose)
            {
                var newSolvedCount = CountSolvedCrossEdges(workingCube, CubeColor.White);
                var remainingCount = 4 - newSolvedCount;
                verboseLines.Add($"Status: {newSolvedCount}/4 edges solved{(remainingCount > 0 ? $" ({remainingCount} remaining)" : "")}");
                verboseLines.Add("");
            }

            stepNumber++;
        }

        // Prepare final algorithm
        var finalAlgorithm = "";
        if (algorithmSteps.Any())
        {
            var fullAlgorithm = string.Join(" ", algorithmSteps);
            finalAlgorithm = AlgorithmCompressor.Compress(fullAlgorithm);
        }

        return (finalAlgorithm, string.Join("\n", verboseLines), workingCube);
    }

    private static CubePiece? FindCrossEdge(Cube cube, CubeColor crossColor, CubeColor edgeColor)
    {
        return cube.Edges.FirstOrDefault(edge =>
            edge.Colors.Contains(crossColor) && edge.Colors.Contains(edgeColor));
    }

    private static int CountSolvedCrossEdges(Cube cube, CubeColor crossColor)
    {
        var crossEdgeColors = new[] { CubeColor.Green, CubeColor.Orange, CubeColor.Blue, CubeColor.Red };
        var solvedCount = 0;
        
        foreach (var edgeColor in crossEdgeColors)
        {
            var edge = FindCrossEdge(cube, crossColor, edgeColor);
            if (edge != null && edge.IsSolved)
            {
                solvedCount++;
            }
        }
        
        return solvedCount;
    }

    private static int HandleHelp()
    {
        ShowHelp();
        return 0;
    }

    private static int HandleUnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        ShowHelp();
        return 1;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("RubiksCube v3.0 - A CLI tool for 3x3x3 Rubik's cube operations");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  rubiks create [cube-name]                     Generate a solved cube state");
        Console.WriteLine("  rubiks apply \"algorithm\" [cube-name]           Apply moves to a cube");
        Console.WriteLine("  rubiks display [cube-name] [--format=fmt]     Display a cube in visual format");
        Console.WriteLine("  rubiks list                                   List all saved cubes");
        Console.WriteLine("  rubiks delete cube-name                       Delete a saved cube");
        Console.WriteLine("  rubiks export cube-name                       Export cube JSON to stdout");
        Console.WriteLine("  rubiks scramble-gen [--moves=n] [--seed=n]    Generate a WCA scramble algorithm");
        Console.WriteLine("  rubiks analyze [cube-name] [--json] [--verbose] Analyze cube state and suggest moves");
        Console.WriteLine("  rubiks help                                   Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  # Named cube operations");
        Console.WriteLine("  rubiks create testcube");
        Console.WriteLine("  rubiks apply \"x y z\" testcube");
        Console.WriteLine("  rubiks display testcube");
        Console.WriteLine("  rubiks list");
        Console.WriteLine("  rubiks export testcube | rubiks display");
        Console.WriteLine();
        Console.WriteLine("  # Traditional piping");
        Console.WriteLine("  rubiks create | rubiks apply \"x\" | rubiks display");
        Console.WriteLine("  rubiks create | rubiks apply \"y y'\" | rubiks display");
        Console.WriteLine("  rubiks create | rubiks display --format=ascii");
        Console.WriteLine();
        Console.WriteLine("  # Scrambling workflow");
        Console.WriteLine("  rubiks create | rubiks apply \"$(rubiks scramble-gen)\" | rubiks display");
        Console.WriteLine("  rubiks scramble-gen --moves=30 --seed=42");
        Console.WriteLine();
        Console.WriteLine("  # Analysis workflow");
        Console.WriteLine("  rubiks analyze testcube");
        Console.WriteLine("  rubiks export testcube | rubiks analyze --json");
        Console.WriteLine("  rubiks create | rubiks apply \"R U R'\" | rubiks analyze");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --format, -f    Display format: ascii or unicode (default: unicode)");
        Console.WriteLine("  --moves, -m     Number of moves in scramble (default: 25)");
        Console.WriteLine("  --seed, -s      Random seed for reproducible scrambles");
        Console.WriteLine("  --json, -j      JSON output format for analysis");
        Console.WriteLine("  --verbose, -v   Verbose analysis output");
        Console.WriteLine("  --help, -h      Show help information");
        Console.WriteLine();
        Console.WriteLine("Storage:");
        Console.WriteLine("  Cubes are saved as .cube files in the current directory");
        Console.WriteLine("  Color scheme: Western/BOY (Green front, Yellow up, Orange right)");
        Console.WriteLine();
        Console.WriteLine("Algorithms:");
        Console.WriteLine("  All moves support ', 2 modifiers (e.g., R', U2, x')");
        Console.WriteLine("  Face rotations: R, L, U, D, F, B");
        Console.WriteLine("  Slice rotations: M, E, S");
        Console.WriteLine("  Cube rotations: x, y, z");
    }
}