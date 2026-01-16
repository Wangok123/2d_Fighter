#!/bin/bash

echo "========================================"
echo "Generate Server Luban Configuration"
echo "========================================"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
PUBLIC_DIR="$ROOT_DIR/Public"

if [ ! -f "$PUBLIC_DIR/gen_server.bat" ] && [ ! -f "$PUBLIC_DIR/gen_server.sh" ]; then
    echo "Error: gen_server.bat or gen_server.sh not found at $PUBLIC_DIR"
    exit 1
fi

echo "Running server generation script in $PUBLIC_DIR..."
cd "$PUBLIC_DIR"

if [ -f "gen_server.sh" ]; then
    chmod +x gen_server.sh
    ./gen_server.sh
elif [ -f "gen_server.bat" ]; then
    ./gen_server.bat
else
    echo "Error: No server generation script found"
    exit 1
fi

if [ $? -ne 0 ]; then
    echo "Error: Failed to generate Server Luban configuration"
    exit 1
fi

echo ""
echo "========================================"
echo "Server Luban configuration generated successfully!"
echo "========================================"
