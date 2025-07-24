using RubiksCube.Core.Models;
using RubiksCube.Core.Display;
using RubiksCube.Core.Storage;
using RubiksCube.Core.Algorithms;
using RubiksCube.Core.Scrambling;
using RubiksCube.Core.PatternRecognition;
using RubiksCube.Core.PatternRecognition.Models;
using RubiksCube.Core.Solving;
using RubiksCube.Core.Extensions;
using RubiksCube.Core.Utilities;
using RubiksCube.CLI.Configuration;
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
            bool shortest = true; // Default to shortest-first (aligns with solve cross behavior)
            bool fixedOrder = false; // Backward compatibility flag
            CubeColor? specificEdge = null;
            
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
                else if (args[i] == "--shortest")
                {
                    shortest = true;
                    fixedOrder = false;
                }
                else if (args[i] == "--fixed-order")
                {
                    fixedOrder = true;
                    shortest = false;
                }
                else if (args[i] == "--edge" || args[i] == "-e")
                {
                    if (i + 1 < args.Length)
                    {
                        var edgeColorStr = args[i + 1].ToLowerInvariant();
                        specificEdge = edgeColorStr switch
                        {
                            "green" or "g" => CubeColor.Green,
                            "orange" or "o" => CubeColor.Orange,
                            "blue" or "b" => CubeColor.Blue,
                            "red" or "r" => CubeColor.Red,
                            _ => null
                        };
                        if (specificEdge == null)
                        {
                            await Console.Error.WriteLineAsync($"Error: Invalid edge color '{args[i + 1]}'. Use green, orange, blue, or red.");
                            return 1;
                        }
                        i++; // Skip the edge color value
                    }
                    else
                    {
                        await Console.Error.WriteLineAsync("Error: --edge requires a color value (green, orange, blue, red)");
                        return 1;
                    }
                }
                else if (args[i].StartsWith("--edge="))
                {
                    var edgeColorStr = args[i].Substring("--edge=".Length).ToLowerInvariant();
                    specificEdge = edgeColorStr switch
                    {
                        "green" or "g" => CubeColor.Green,
                        "orange" or "o" => CubeColor.Orange,
                        "blue" or "b" => CubeColor.Blue,
                        "red" or "r" => CubeColor.Red,
                        _ => null
                    };
                    if (specificEdge == null)
                    {
                        await Console.Error.WriteLineAsync($"Error: Invalid edge color '{edgeColorStr}'. Use green, orange, blue, or red.");
                        return 1;
                    }
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

            // Normalize cube to canonical orientation (white bottom, green front) for analysis
            // Analysis methods require canonical format due to edge.IsSolved dependency chain
            var normalizationResult = CubeNormalizer.NormalizeToCanonical(cube);
            cube = normalizationResult.NormalizedCube;

            // Check if we should show all cross edges analysis
            if (allEdges)
            {
                var crossSolver = CrossConfiguration.CreateCrossSolver(args);
                var edgeAnalysis = crossSolver.AnalyzeAllCrossEdges(cube, verbose);
                Console.WriteLine("Cross edge analysis for white cross:");
                Console.WriteLine("=====================================");
                Console.WriteLine(edgeAnalysis);
                return 0;
            }

            // Analyze the cube
            AnalysisResult result;
            string verboseDetails = "";
            
            // Check if we're using custom edge selection
            if (shortest || specificEdge.HasValue)
            {
                // Use CrossSolver directly with custom parameters
                var crossSolver = CrossConfiguration.CreateCrossSolver(args);
                var crossAnalyzer = CrossConfiguration.CreateCrossAnalyzer(args);
                
                // Get recognition result first
                var recognition = crossAnalyzer.Analyze(cube);
                if (recognition != null && recognition.Stage == "cross")
                {
                    // Edge selection logic: specific edge overrides other modes, then check shortest vs fixed
                    var selectionMode = specificEdge.HasValue 
                        ? CrossSolver.EdgeSelectionMode.FixedOrder  // Use fixed order with specific edge
                        : fixedOrder 
                            ? CrossSolver.EdgeSelectionMode.FixedOrder 
                            : CrossSolver.EdgeSelectionMode.Shortest; // Default to shortest
                    var suggestion = crossSolver.SuggestAlgorithm(cube, recognition, selectionMode, specificEdge);
                    result = new AnalysisResult(recognition, suggestion);
                    
                    if (verbose)
                    {
                        verboseDetails = GenerateCustomVerboseDetails(cube, recognition, suggestion, !fixedOrder && !specificEdge.HasValue, specificEdge);
                    }
                }
                else
                {
                    // Fallback to normal analysis
                    var analyzer = new CubeStateAnalyzer();
                    if (verbose)
                    {
                        var (analysisResult, details) = analyzer.AnalyzeWithDetails(cube);
                        result = analysisResult;
                        verboseDetails = details;
                    }
                    else
                    {
                        result = analyzer.Analyze(cube);
                    }
                }
            }
            else
            {
                // Use standard analysis
                var analyzer = new CubeStateAnalyzer();
                if (verbose)
                {
                    var (analysisResult, details) = analyzer.AnalyzeWithDetails(cube);
                    result = analysisResult;
                    verboseDetails = details;
                }
                else
                {
                    result = analyzer.Analyze(cube);
                }
            }

            // Combine normalization moves with suggestion algorithms using centralized logic
            // Only return combined algorithm if there's a real solving algorithm
            if (result.Suggestion != null)
            {
                var originalAlgorithm = result.Suggestion.Algorithm ?? "";
                var completeAlgorithm = AlgorithmCombiner.CombineNormalizationWithAlgorithm(
                    normalizationResult.NormalizationMoves, 
                    originalAlgorithm);
                
                // Update the suggestion with the complete algorithm (may be empty if no real algorithm)
                result = new AnalysisResult(
                    result.Recognition,
                    new SuggestionResult(
                        completeAlgorithm,
                        result.Suggestion.Description,
                        result.Suggestion.NextStage,
                        result.Suggestion.Details
                    )
                );
            }
            // Note: Removed the else clause that would add normalization-only suggestions
            // If there's no real algorithm needed, we shouldn't suggest normalization moves

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
                if (verbose)
                {
                    // Verbose mode: show detailed analysis process
                    Console.WriteLine(verboseDetails);
                }
                else
                {
                    // Non-verbose mode: show standard output
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
            await Console.Error.WriteLineAsync("Usage: rubiks solve cross [cube-name] [--verbose] [--level=pattern|superhuman] [--shortest] [--fixed-order] [--edge=color]");
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
            string level = "pattern"; // Default to pattern-based solving
            bool shortest = true; // Default to shortest-first (new behavior)
            bool fixedOrder = false; // Backward compatibility flag
            CubeColor? specificEdge = null; // Specific edge targeting
            
            // Parse arguments
            for (int i = 2; i < args.Length; i++)
            {
                if (args[i] == "--verbose" || args[i] == "-v")
                {
                    verbose = true;
                }
                else if (args[i] == "--level" || args[i] == "-l")
                {
                    if (i + 1 < args.Length)
                    {
                        level = args[i + 1].ToLowerInvariant();
                        i++; // Skip the level value
                    }
                    else
                    {
                        await Console.Error.WriteLineAsync("Error: --level requires a value (pattern or superhuman)");
                        return 1;
                    }
                }
                else if (args[i].StartsWith("--level="))
                {
                    level = args[i].Substring("--level=".Length).ToLowerInvariant();
                }
                else if (args[i] == "--shortest")
                {
                    shortest = true;
                    fixedOrder = false;
                }
                else if (args[i] == "--fixed-order")
                {
                    fixedOrder = true;
                    shortest = false;
                }
                else if (args[i] == "--edge" || args[i] == "-e")
                {
                    if (i + 1 < args.Length)
                    {
                        var edgeColorStr = args[i + 1].ToLowerInvariant();
                        specificEdge = edgeColorStr switch
                        {
                            "green" or "g" => CubeColor.Green,
                            "orange" or "o" => CubeColor.Orange,
                            "blue" or "b" => CubeColor.Blue,
                            "red" or "r" => CubeColor.Red,
                            _ => null
                        };
                        if (specificEdge == null)
                        {
                            await Console.Error.WriteLineAsync($"Error: Invalid edge color '{args[i + 1]}'. Use green, orange, blue, or red.");
                            return 1;
                        }
                        i++; // Skip the edge color value
                    }
                    else
                    {
                        await Console.Error.WriteLineAsync("Error: --edge requires a color value (green, orange, blue, red)");
                        return 1;
                    }
                }
                else if (args[i].StartsWith("--edge="))
                {
                    var edgeColorStr = args[i].Substring("--edge=".Length).ToLowerInvariant();
                    specificEdge = edgeColorStr switch
                    {
                        "green" or "g" => CubeColor.Green,
                        "orange" or "o" => CubeColor.Orange,
                        "blue" or "b" => CubeColor.Blue,
                        "red" or "r" => CubeColor.Red,
                        _ => null
                    };
                    if (specificEdge == null)
                    {
                        await Console.Error.WriteLineAsync($"Error: Invalid edge color '{edgeColorStr}'. Use green, orange, blue, or red.");
                        return 1;
                    }
                }
                else if (!args[i].StartsWith("-"))
                {
                    // Assume it's a cube name
                    cubeName = args[i];
                }
            }

            // Validate level parameter
            if (level != "pattern" && level != "superhuman")
            {
                await Console.Error.WriteLineAsync($"Error: Unsupported level '{level}'. Use 'pattern' or 'superhuman'.");
                return 1;
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

            // Normalize cube to canonical orientation for solving
            // Both CrossAnalyzer and CrossSolver require canonical format (white bottom + green front)
            var normalizationResult = CubeNormalizer.NormalizeToCanonical(cube);
            cube = normalizationResult.NormalizedCube;

            // Choose solver based on level
            string algorithm;
            string verboseOutput;
            Cube finalCube;
            
            if (level == "superhuman")
            {
                var superhumanSolver = CrossConfiguration.CreateSuperhumanCrossSolver(args);
                superhumanSolver.Verbose = verbose;
                var normalizedSuperhumanSolver = new NormalizedSuperhumanCrossSolver(superhumanSolver);
                var crossColor = CrossConfiguration.GetCrossColor(args);
                
                // Use wrapper method that handles progress calculation internally
                // Pass the pre-normalized cube and the normalization moves from HandleSolve
                var suggestion = normalizedSuperhumanSolver.SuggestAlgorithmWithProgress(cube, normalizationResult.NormalizationMoves, crossColor, verbose);
                if (suggestion != null)
                {
                    algorithm = suggestion.Algorithm;
                    verboseOutput = suggestion.Description;
                    
                    // Apply algorithm to get final cube state
                    finalCube = cube.Clone();
                    if (!string.IsNullOrEmpty(algorithm))
                    {
                        var algorithmResult = Algorithm.Parse(algorithm);
                        if (algorithmResult.IsSuccess)
                        {
                            foreach (var move in algorithmResult.Value.Moves)
                            {
                                finalCube.ApplyMove(move);
                            }
                        }
                    }
                }
                else
                {
                    algorithm = "";
                    verboseOutput = "No superhuman solution found";
                    finalCube = cube;
                }
            }
            else
            {
                // Use standard pattern-based solver with new edge selection logic
                var crossSolver = CrossConfiguration.CreateCrossSolver(args);
                var crossAnalyzer = CrossConfiguration.CreateCrossAnalyzer(args);
                
                if (fixedOrder)
                {
                    // Use legacy fixed-order solving for backward compatibility
                    var solution = crossSolver.SolveCompleteCross(cube, verbose);
                    
                    // Combine normalization with solution algorithm using centralized logic
                    var solutionAlgorithm = solution.Algorithm;
                    algorithm = AlgorithmCombiner.CombineNormalizationWithAlgorithm(
                        normalizationResult.NormalizationMoves, 
                        solutionAlgorithm);
                    
                    verboseOutput = solution.VerboseOutput;
                    finalCube = solution.FinalCube;
                }
                else
                {
                    // Use new shortest-first or specific edge selection (matches analyze logic)
                    var recognition = crossAnalyzer.Analyze(cube);
                    if (recognition != null && recognition.Stage == "cross")
                    {
                        // Edge selection logic: specific edge overrides shortest mode
                        var selectionMode = specificEdge.HasValue 
                            ? CrossSolver.EdgeSelectionMode.FixedOrder  // Use fixed order with specific edge
                            : shortest 
                                ? CrossSolver.EdgeSelectionMode.Shortest 
                                : CrossSolver.EdgeSelectionMode.FixedOrder;
                        var suggestion = crossSolver.SuggestAlgorithm(cube, recognition, selectionMode, specificEdge);
                        
                        // Combine normalization with suggestion algorithm using centralized logic
                        var suggestionAlgorithm = suggestion?.Algorithm ?? "";
                        algorithm = AlgorithmCombiner.CombineNormalizationWithAlgorithm(
                            normalizationResult.NormalizationMoves, 
                            suggestionAlgorithm);
                        
                        if (verbose)
                        {
                            verboseOutput = GenerateCustomVerboseDetails(cube, recognition, suggestion, shortest, specificEdge);
                        }
                        else
                        {
                            verboseOutput = suggestion?.Description ?? "No solution found";
                        }
                        
                        // Apply algorithm to get final cube state
                        finalCube = cube.Clone();
                        if (!string.IsNullOrEmpty(suggestionAlgorithm))
                        {
                            var algorithmResult = Algorithm.Parse(suggestionAlgorithm);
                            if (algorithmResult.IsSuccess)
                            {
                                foreach (var move in algorithmResult.Value.Moves)
                                {
                                    finalCube.ApplyMove(move);
                                }
                            }
                        }
                    }
                    else
                    {
                        algorithm = normalizationResult.NormalizationMoves;
                        verboseOutput = "Cube is already solved or not in cross stage";
                        finalCube = cube;
                    }
                }
            }
            
            if (verbose)
            {
                Console.WriteLine(verboseOutput);
            }
            else
            {
                Console.WriteLine(algorithm);
            }

            // Note: We don't save the cube state - solve command only outputs the solution algorithm

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error solving cross: {ex.Message}");
            return 1;
        }
    }

    private static string GenerateCustomVerboseDetails(Cube cube, RecognitionResult recognition, SuggestionResult? suggestion, bool shortest, CubeColor? specificEdge)
    {
        var details = new List<string>();
        details.Add("Cube Analysis - Custom Edge Selection");
        details.Add("=====================================");
        details.Add("");
        
        if (specificEdge.HasValue)
        {
            details.Add($"Target edge: {specificEdge.Value.GetColorName()}");
        }
        else if (shortest)
        {
            details.Add("Selection mode: Shortest algorithm");
        }
        
        details.Add($"Stage: {recognition.Stage}");
        details.Add($"Progress: {recognition.Progress}/{recognition.Total}");
        details.Add($"Complete: {(recognition.IsComplete ? "Yes" : "No")}");
        details.Add("");
        
        if (suggestion != null)
        {
            details.Add($"Algorithm: {(string.IsNullOrEmpty(suggestion.Algorithm) ? "None needed" : suggestion.Algorithm)}");
            details.Add($"Description: {suggestion.Description}");
            details.Add($"Next stage: {suggestion.NextStage}");
            
            if (shortest && !specificEdge.HasValue)
            {
                details.Add("");
                details.Add("Note: This is the shortest available algorithm among all unsolved edges.");
            }
        }
        
        return string.Join("\n", details);
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
        Console.WriteLine("  rubiks solve cross [cube-name] [--verbose] [--level=pattern|superhuman] [--shortest] [--fixed-order] [--edge=color] Solve white cross");
        Console.WriteLine("  rubiks analyze [cube-name] [--json] [--verbose] [--shortest] [--fixed-order] [--edge=color] [--all-edges] Analyze cube state and suggest moves");
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
        Console.WriteLine("  rubiks analyze testcube --shortest");
        Console.WriteLine("  rubiks analyze testcube --edge=blue");
        Console.WriteLine("  rubiks export testcube | rubiks analyze --json");
        Console.WriteLine("  rubiks create | rubiks apply \"R U R'\" | rubiks analyze");
        Console.WriteLine();
        Console.WriteLine("  # Solving workflow");
        Console.WriteLine("  rubiks solve cross testcube --verbose");
        Console.WriteLine("  rubiks solve cross testcube --level=superhuman");
        Console.WriteLine("  rubiks create | rubiks apply \"$(rubiks scramble-gen)\" | rubiks solve cross");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --format, -f    Display format: ascii or unicode (default: unicode)");
        Console.WriteLine("  --moves, -m     Number of moves in scramble (default: 25)");
        Console.WriteLine("  --seed, -s      Random seed for reproducible scrambles");
        Console.WriteLine("  --json, -j      JSON output format for analysis");
        Console.WriteLine("  --verbose, -v   Verbose analysis output");
        Console.WriteLine("  --shortest      Use shortest available algorithm (default for both analyze and solve cross)");
        Console.WriteLine("  --fixed-order   Use fixed edge order (green→blue→orange→red) for backward compatibility");
        Console.WriteLine("  --edge, -e      Analyze specific edge color: green, orange, blue, red");
        Console.WriteLine("  --level, -l     Solving level: pattern (default) or superhuman");
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