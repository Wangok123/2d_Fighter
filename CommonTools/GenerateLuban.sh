#!/bin/bash

echo "========================================"
echo "Generate Luban Configuration"
echo "========================================"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
PUBLIC_DIR="$ROOT_DIR/Public"

if [ ! -f "$PUBLIC_DIR/gen.bat" ] && [ ! -f "$PUBLIC_DIR/gen.sh" ]; then
    echo "Error: gen.bat or gen.sh not found at $PUBLIC_DIR"
    exit 1
fi

echo "Running generation script in $PUBLIC_DIR..."
cd "$PUBLIC_DIR"

if [ -f "gen.sh" ]; then
    chmod +x gen.sh
    ./gen.sh
elif [ -f "gen.bat" ]; then
    ./gen.bat
else
    echo "Error: No generation script found"
    exit 1
fi

if [ $? -ne 0 ]; then
    echo "Error: Failed to generate Luban configuration"
    exit 1
fi

echo ""
echo "========================================"
echo "Luban configuration generated successfully!"
echo "========================================"
