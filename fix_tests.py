#!/usr/bin/env python3
"""
Script to fix CubeMoveTests.cs with correct color expectations
after our color scheme change (Red left, Orange right)
"""

# Color mapping updates needed
color_replacements = [
    # In Orange face tests (should now be Left face tests)
    ("CubeColor.Orange, CubeColor.Orange, CubeColor.Orange,\n            CubeColor.Orange, CubeColor.Orange, CubeColor.Orange,\n            CubeColor.Orange, CubeColor.Orange, CubeColor.Orange);", 
     "CubeColor.Red, CubeColor.Red, CubeColor.Red,\n            CubeColor.Red, CubeColor.Red, CubeColor.Red,\n            CubeColor.Red, CubeColor.Red, CubeColor.Red);"),
     
    # Any standalone Orange face that should be Red
    ("AssertStickersEqual(cube, CubeFace.Left,\n            CubeColor.Orange", 
     "AssertStickersEqual(cube, CubeFace.Left,\n            CubeColor.Red"),
]

# Read the test file
with open('/mnt/c/users/steve/onedrive/Documents/code/rubiks/src/RubiksCube.Tests/Models/CubeMoveTests.cs', 'r') as f:
    content = f.read()

# Apply replacements
for old, new in color_replacements:
    content = content.replace(old, new)

# Write back
with open('/mnt/c/users/steve/onedrive/Documents/code/rubiks/src/RubiksCube.Tests/Models/CubeMoveTests.cs', 'w') as f:
    f.write(content)

print("Test file updated with color scheme fixes")