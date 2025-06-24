using RubiksCube.Core.Models;
using CSharpFunctionalExtensions;
using System.Text.RegularExpressions;

namespace RubiksCube.Core.Storage;

public static class CubeStorageService
{
    private static readonly Regex ValidCubeNamePattern = new(@"^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);
    
    public static Result Save(Cube cube, string cubeName, string? storageDirectory = null)
    {
        var validationResult = ValidateCubeName(cubeName);
        if (validationResult.IsFailure)
            return validationResult;

        try
        {
            var directory = GetStorageDirectory(storageDirectory);
            EnsureDirectoryExists(directory);
            
            var filePath = GetCubePath(cubeName, directory);
            var cubeJson = cube.ToJson();
            
            File.WriteAllText(filePath, cubeJson);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to save cube '{cubeName}': {ex.Message}");
        }
    }

    public static Result<Cube> Load(string cubeName, string? storageDirectory = null)
    {
        var validationResult = ValidateCubeName(cubeName);
        if (validationResult.IsFailure)
            return Result.Failure<Cube>(validationResult.Error);

        try
        {
            var directory = GetStorageDirectory(storageDirectory);
            var filePath = GetCubePath(cubeName, directory);
            
            if (!File.Exists(filePath))
                return Result.Failure<Cube>($"Cube '{cubeName}' not found");

            var cubeJson = File.ReadAllText(filePath);
            var cube = Cube.FromJson(cubeJson);
            
            return Result.Success(cube);
        }
        catch (Exception ex)
        {
            return Result.Failure<Cube>($"Failed to load cube '{cubeName}': {ex.Message}");
        }
    }

    public static Result Delete(string cubeName, string? storageDirectory = null)
    {
        var validationResult = ValidateCubeName(cubeName);
        if (validationResult.IsFailure)
            return validationResult;

        try
        {
            var directory = GetStorageDirectory(storageDirectory);
            var filePath = GetCubePath(cubeName, directory);
            
            if (!File.Exists(filePath))
                return Result.Failure($"Cube '{cubeName}' not found");

            File.Delete(filePath);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete cube '{cubeName}': {ex.Message}");
        }
    }

    public static Result<string> Export(string cubeName, string? storageDirectory = null)
    {
        var loadResult = Load(cubeName, storageDirectory);
        if (loadResult.IsFailure)
            return Result.Failure<string>(loadResult.Error);

        return Result.Success(loadResult.Value.ToJson());
    }

    public static IEnumerable<string> List(string? storageDirectory = null)
    {
        try
        {
            var directory = GetStorageDirectory(storageDirectory);
            
            if (!Directory.Exists(directory))
                return Enumerable.Empty<string>();

            return Directory.GetFiles(directory, "*.cube")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => !string.IsNullOrEmpty(name))
                .OrderBy(name => name)
                .ToList()!;
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }

    public static bool Exists(string cubeName, string? storageDirectory = null)
    {
        var validationResult = ValidateCubeName(cubeName);
        if (validationResult.IsFailure)
            return false;

        try
        {
            var directory = GetStorageDirectory(storageDirectory);
            var filePath = GetCubePath(cubeName, directory);
            return File.Exists(filePath);
        }
        catch
        {
            return false;
        }
    }

    private static Result ValidateCubeName(string cubeName)
    {
        if (string.IsNullOrWhiteSpace(cubeName))
            return Result.Failure("Cube name cannot be null or empty");

        if (cubeName.Length > 50)
            return Result.Failure("Cube name cannot exceed 50 characters");

        if (!ValidCubeNamePattern.IsMatch(cubeName))
            return Result.Failure("Cube name can only contain letters, numbers, hyphens, and underscores");

        return Result.Success();
    }

    private static string GetStorageDirectory(string? storageDirectory)
    {
        return storageDirectory ?? Directory.GetCurrentDirectory();
    }

    private static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static string GetCubePath(string cubeName, string directory)
    {
        return Path.Combine(directory, $"{cubeName}.cube");
    }
}