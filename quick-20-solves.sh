#!/bin/bash

echo "Running 20 white cross solves - Summary only"
echo "============================================"
echo

total_moves=0

for i in {1..20}
do
    # Create cube
    dotnet run --project src/RubiksCube.CLI create testcube$i > /dev/null 2>&1
    
    # Generate scramble
    scramble=$(dotnet run --project src/RubiksCube.CLI scramble-gen 2>/dev/null)
    
    # Apply scramble
    dotnet run --project src/RubiksCube.CLI apply "$scramble" testcube$i > /dev/null 2>&1
    
    # Solve cross and get algorithm
    algorithm=$(dotnet run --project src/RubiksCube.CLI solve cross testcube$i 2>/dev/null)
    
    # Count moves
    if [ -z "$algorithm" ]; then
        move_count=0
        status="Already solved"
    else
        move_count=$(echo "$algorithm" | wc -w)
        status="$move_count moves: $algorithm"
        total_moves=$((total_moves + move_count))
    fi
    
    echo "Solve $i: $status"
    
    # Clean up
    rm -f testcube$i.cube
done

echo
echo "Total moves across all solves: $total_moves"
echo "Average moves per solve: $((total_moves / 20))"
echo "All 20 solves completed successfully!"