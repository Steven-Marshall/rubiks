# RubiksCube

A cross-platform CLI application and library for solving 3x3x3 Rubik's cubes using CFOP (Cross, F2L, OLL, PLL) method, written in C# .NET 9.

## Features

- **Complete cube manipulation**: All 18 standard moves (R, L, U, D, F, B and their inverses/rotations)
- **ASCII/Unicode visualization**: Clear cube state display with color support
- **Unix-style piping**: Chain commands for complex operations
- **Named cube persistence**: Save and reload cube states
- **WCA-compliant scrambling**: Generate official competition scrambles
- **Pattern recognition**: Analyze cube state and suggest next moves
- **Cross solver**: Automated white cross solving with multiple strategies

## Installation

### Prerequisites
- .NET 9 SDK or later
- Terminal with Unicode support (for cube display)

### Build from Source
```bash
git clone https://github.com/Steven-Marshall/rubiks.git
cd rubiks
dotnet build --configuration Release
```

### Create System-wide Command (Linux/macOS)
```bash
sudo ln -s $(pwd)/src/RubiksCube.CLI/bin/Release/net9.0/rubiks /usr/local/bin/rubiks
```

## Usage

### Basic Commands

**Create a solved cube:**
```bash
rubiks create
```

**Apply moves:**
```bash
rubiks apply "R U R' U'"
```

**Display cube state:**
```bash
rubiks display
```

### Piping Commands

Chain operations together:
```bash
rubiks create | rubiks apply "F R U' R' F'" | rubiks display
```

### Named Cubes

Save cube states for later use:
```bash
rubiks create mycube
rubiks apply "R U R' U'" mycube
rubiks display mycube
```

### Scrambling

Generate WCA-compliant scrambles:
```bash
rubiks scramble-gen
rubiks create | rubiks apply "$(rubiks scramble-gen)" | rubiks display
```

### Pattern Analysis

Analyze cube state:
```bash
rubiks create | rubiks apply "R U R' F'" | rubiks analyze
```

With detailed output:
```bash
rubiks analyze mycube --verbose
```

### Solving

Currently supports white cross solving:
```bash
rubiks solve cross
```

## Notation

Uses standard Singmaster notation:
- **Face turns**: R (Right), L (Left), U (Up), D (Down), F (Front), B (Back)
- **Modifiers**: ' (counter-clockwise), 2 (180 degrees)
- **Rotations**: x (pitch), y (yaw), z (roll)
- **Middle slices**: M (middle), E (equatorial), S (standing)

## Architecture

- **RubiksCube.Core**: Business logic and cube operations
- **RubiksCube.CLI**: Command-line interface
- **RubiksCube.Tests**: Comprehensive test suite (361 tests)

## Development

### Build and Test
```bash
dotnet build
dotnet test
dotnet run --project src/RubiksCube.CLI
```

### Project Structure
```
rubiks/
├── src/
│   ├── RubiksCube.Core/      # Core library
│   ├── RubiksCube.CLI/       # CLI application
│   └── RubiksCube.Tests/     # Unit tests
├── docs/                     # Documentation
└── algorithms/               # Algorithm databases (planned)
```

## Roadmap

- [x] Core cube manipulation and display
- [x] Pattern recognition framework
- [x] White cross solver
- [ ] F2L (First Two Layers) solver
- [ ] OLL (Orient Last Layer) - 57 cases
- [ ] PLL (Permute Last Layer) - 21 cases
- [ ] Advanced solving methods (ROUX, ZZ)
- [ ] Performance analysis and metrics

## Contributing

Contributions welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Singmaster notation for standardized move notation
- WCA (World Cube Association) for scrambling standards
- CFOP method pioneers for solving strategies