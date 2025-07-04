#!/bin/bash

echo "Running 20 white cross solves with different WCA scrambles..."
echo "============================================================="
echo

for i in {1..20}
do
    echo "=== Solve $i ==="
    
    # Create cube
    dotnet run --project src/RubiksCube.CLI create cube$i > /dev/null
    
    # Generate scramble
    scramble=$(dotnet run --project src/RubiksCube.CLI scramble-gen)
    echo "Scramble: $scramble"
    
    # Apply scramble
    dotnet run --project src/RubiksCube.CLI apply "$scramble" cube$i
    
    # Solve cross and get algorithm
    algorithm=$(dotnet run --project src/RubiksCube.CLI solve cross cube$i)
    
    # Count moves
    if [ -z "$algorithm" ]; then
        move_count=0
        echo "Algorithm: (already solved)"
    else
        # Count moves by splitting on spaces
        move_count=$(echo "$algorithm" | wc -w)
        echo "Algorithm: $algorithm"
    fi
    
    echo "Moves: $move_count"
    
    # Show final render
    echo "Final state:"
    dotnet run --project src/RubiksCube.CLI display cube$i
    
    echo
done

echo "All 20 solves completed!"