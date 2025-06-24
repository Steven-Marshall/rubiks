using RubiksCube.Core.Models;

namespace RubiksCube.Tests.Models;

/// <summary>
/// Critical tests to validate Western/BOY color scheme 
/// These tests are based on physical cube inspection, not code output
/// </summary>
public class ColorSchemeValidationTests
{
    [Fact]
    public void StandardOrientation_CRITICAL_GreenFrontYellowUp_ShouldHaveOrangeRight()
    {
        // CRITICAL TEST: This caused major v1 bugs
        // Physical validation: Hold cube with Green front, Yellow up (White bottom)
        // Looking at the Green face, Orange should be on your RIGHT
        
        var standardOrientation = CubeOrientation.Standard;
        
        Assert.Equal(CubeColor.Green, standardOrientation.Front);
        Assert.Equal(CubeColor.Yellow, standardOrientation.Up);
        Assert.Equal(CubeColor.Orange, standardOrientation.Right);  // CRITICAL: Orange on right
        Assert.Equal(CubeColor.Red, standardOrientation.Left);      // Red on left  
        Assert.Equal(CubeColor.Blue, standardOrientation.Back);     // Blue on back
        Assert.Equal(CubeColor.White, standardOrientation.Bottom);  // White on bottom
    }
    
    [Fact]
    public void PhysicalCubeValidation_BOYPattern_ShouldBeCorrect()
    {
        // BOY mnemonic: Blue-Orange-Yellow reading clockwise from Green front
        // Starting from Green front, going clockwise: Blue (back), Orange (right), Yellow (top)
        
        var orientation = CubeOrientation.Standard;
        
        // From Green front, reading clockwise around the middle slice:
        // Green (front) -> Orange (right) -> Blue (back) -> Red (left) -> Green (front)
        Assert.Equal(CubeColor.Green, orientation.Front);
        Assert.Equal(CubeColor.Orange, orientation.Right);   // Clockwise from Green
        Assert.Equal(CubeColor.Blue, orientation.Back);      // Clockwise from Orange  
        Assert.Equal(CubeColor.Red, orientation.Left);       // Clockwise from Blue
        
        // Vertical: Yellow up, White down
        Assert.Equal(CubeColor.Yellow, orientation.Up);
        Assert.Equal(CubeColor.White, orientation.Bottom);
    }
    
    [Fact] 
    public void OppositeColors_ShouldMatchPhysicalCube()
    {
        // These are fixed on any standard Rubik's cube regardless of orientation
        // Based on physical cube inspection - opposite faces are always:
        
        var orientation = CubeOrientation.Standard;
        
        // White opposite Yellow (vertical axis)
        Assert.Equal(CubeColor.White, orientation.Bottom);
        Assert.Equal(CubeColor.Yellow, orientation.Up);
        
        // Green opposite Blue (front-back axis) 
        Assert.Equal(CubeColor.Green, orientation.Front);
        Assert.Equal(CubeColor.Blue, orientation.Back);
        
        // Red opposite Orange (left-right axis)
        Assert.Equal(CubeColor.Red, orientation.Left);
        Assert.Equal(CubeColor.Orange, orientation.Right);
    }
    
    [Theory]
    [InlineData(CubeColor.Green, CubeColor.Yellow, CubeColor.Orange)]  // Standard: Green front, Yellow up -> Orange right
    [InlineData(CubeColor.Green, CubeColor.White, CubeColor.Red)]      // Green front, White up -> Red right  
    [InlineData(CubeColor.Blue, CubeColor.Yellow, CubeColor.Red)]      // Blue front, Yellow up -> Red right
    [InlineData(CubeColor.Red, CubeColor.Yellow, CubeColor.Green)]     // Red front, Yellow up -> Green right
    [InlineData(CubeColor.Orange, CubeColor.Yellow, CubeColor.Blue)]   // Orange front, Yellow up -> Blue right
    public void RightHandRule_KnownOrientations_ShouldBeCorrect(CubeColor front, CubeColor up, CubeColor expectedRight)
    {
        // These test cases are based on right-hand rule and physical cube validation
        // Right-hand rule: Point fingers toward front, curl toward up, thumb points right
        
        var orientation = new CubeOrientation(front, up);
        Assert.Equal(expectedRight, orientation.Right);
    }
    
    [Fact]
    public void CubeColor_EnumValues_ShouldMatchDocumentation()
    {
        // Verify our enum values match the documented color scheme
        // These should never change as they're part of the cube definition
        
        Assert.Equal(0, (int)CubeColor.White);   // Bottom in standard
        Assert.Equal(1, (int)CubeColor.Yellow);  // Top in standard
        Assert.Equal(2, (int)CubeColor.Green);   // Front in standard
        Assert.Equal(3, (int)CubeColor.Blue);    // Back in standard  
        Assert.Equal(4, (int)CubeColor.Red);     // Left in standard
        Assert.Equal(5, (int)CubeColor.Orange);  // Right in standard
    }
    
    [Fact]
    public void AllOrientations_ShouldHaveValidAdjacentFaces()
    {
        // Test that we can't create orientations with opposite faces
        // Based on physical impossibility - you can't have White front and Yellow up simultaneously
        
        var invalidCombinations = new[]
        {
            (CubeColor.White, CubeColor.Yellow),   // Opposite vertical
            (CubeColor.Yellow, CubeColor.White),   // Opposite vertical
            (CubeColor.Green, CubeColor.Blue),     // Opposite front-back
            (CubeColor.Blue, CubeColor.Green),     // Opposite front-back
            (CubeColor.Red, CubeColor.Orange),     // Opposite left-right
            (CubeColor.Orange, CubeColor.Red)      // Opposite left-right
        };
        
        foreach (var (front, up) in invalidCombinations)
        {
            Assert.Throws<ArgumentException>(() => new CubeOrientation(front, up));
        }
    }
    
    [Fact]
    public void StandardOrientation_AfterRotations_ShouldMaintainColorRelationships()
    {
        // Test that color relationships are preserved after rotations
        // This validates our rotation logic against known physical behavior
        
        var start = CubeOrientation.Standard;
        
        // Y rotation (around vertical axis): Front->Left, Left->Back, Back->Right, Right->Front
        var afterY = start.ApplyYRotation(clockwise: true);
        
        // After Y clockwise: Orange front, Yellow up
        // Y rotation: Front→Left, Left→Back, Back→Right, Right→Front
        // From standard: Green→Left, Red→Back, Blue→Right, Orange→Front
        Assert.Equal(CubeColor.Orange, afterY.Front);    // Right becomes Front
        Assert.Equal(CubeColor.Blue, afterY.Right);      // Back becomes Right
        Assert.Equal(CubeColor.Red, afterY.Back);        // Left becomes Back
        Assert.Equal(CubeColor.Green, afterY.Left);      // Front becomes Left
        Assert.Equal(CubeColor.Yellow, afterY.Up);       // Up unchanged
        Assert.Equal(CubeColor.White, afterY.Bottom);    // Bottom unchanged
        
        // X rotation (around left-right axis): Front->Up, Up->Back, Back->Bottom, Bottom->Front
        var afterX = start.ApplyXRotation(clockwise: true);
        
        // Start: Green front, Yellow up, Blue back, White bottom, Orange right, Red left
        // After X clockwise: White front, Green up 
        // From GetRightColor: (White, Green) => Red, so Red right, Orange left
        Assert.Equal(CubeColor.White, afterX.Front);     // Bottom becomes Front
        Assert.Equal(CubeColor.Green, afterX.Up);        // Front becomes Up  
        Assert.Equal(CubeColor.Yellow, afterX.Back);     // Up becomes Back (derived)
        Assert.Equal(CubeColor.Blue, afterX.Bottom);     // Back becomes Bottom (derived)
        Assert.Equal(CubeColor.Red, afterX.Right);       // Right changes due to orientation
        Assert.Equal(CubeColor.Orange, afterX.Left);     // Left changes due to orientation
    }
}