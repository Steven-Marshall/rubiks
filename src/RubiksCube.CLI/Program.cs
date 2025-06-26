using RubiksCube.Core.Models;
using RubiksCube.Core.Display;
using RubiksCube.Core.Storage;
using RubiksCube.Core.Algorithms;
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
                if (move.Type == MoveType.Reorientation)
                {
                    cube.ApplyReorientation(move);
                }
                else if (move.Type == MoveType.Rotation)
                {
                    // Face rotations are now fully implemented
                    cube.ApplyMove(move);
                }
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
            var pythonRenderer = new PythonRenderer(cube);
            
            string output = format.ToLowerInvariant() switch
            {
                "ascii" => pythonRenderer.RenderAscii(),
                "unicode" => pythonRenderer.RenderUnicode(),
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
        Console.WriteLine("RubiksCube v2.0 - A CLI tool for 3x3x3 Rubik's cube operations");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  rubiks create [cube-name]                     Generate a solved cube state");
        Console.WriteLine("  rubiks apply \"algorithm\" [cube-name]           Apply moves to a cube");
        Console.WriteLine("  rubiks display [cube-name] [--format=fmt]     Display a cube in visual format");
        Console.WriteLine("  rubiks list                                   List all saved cubes");
        Console.WriteLine("  rubiks delete cube-name                       Delete a saved cube");
        Console.WriteLine("  rubiks export cube-name                       Export cube JSON to stdout");
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
        Console.WriteLine("Options:");
        Console.WriteLine("  --format, -f    Display format: ascii or unicode (default: unicode)");
        Console.WriteLine("  --help, -h      Show help information");
        Console.WriteLine();
        Console.WriteLine("Storage:");
        Console.WriteLine("  Cubes are saved as .cube files in the current directory");
        Console.WriteLine("  Color scheme: Western/BOY (Green front, Yellow up, Orange right)");
        Console.WriteLine();
        Console.WriteLine("Algorithms:");
        Console.WriteLine("  Reorientations: x, y, z (with ', 2 modifiers)");
        Console.WriteLine("  Face rotations: R, L, U, D, F, B (coming in Phase 4d)");
    }
}