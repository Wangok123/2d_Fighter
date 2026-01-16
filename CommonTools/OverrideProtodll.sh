#!/bin/bash

echo "========================================"
echo "Override Protocol DLL Tool"
echo "========================================"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
DLL_NAME="LatProtocol.dll"
SOURCE="$ROOT_DIR/Server/LatServer/LatProtocol/bin/Release/netstandard2.1/$DLL_NAME"
TARGET="$ROOT_DIR/Client/Assets/Plugins/Tools/KCPNet/$DLL_NAME"

if [ ! -f "$SOURCE" ]; then
    echo "Error: Source DLL not found at $SOURCE"
    echo "Please build the LatProtocol project first."
    exit 1
fi

TARGET_DIR=$(dirname "$TARGET")
if [ ! -d "$TARGET_DIR" ]; then
    echo "Warning: Target directory does not exist: $TARGET_DIR"
    read -p "Create target directory? (y/n): " CREATE_DIR
    if [ "$CREATE_DIR" == "y" ] || [ "$CREATE_DIR" == "Y" ]; then
        mkdir -p "$TARGET_DIR"
    else
        echo "Operation cancelled."
        exit 1
    fi
fi

echo ""
echo "Copying DLL..."
echo "From: $SOURCE"
echo "To: $TARGET"
cp -f "$SOURCE" "$TARGET"

if [ $? -ne 0 ]; then
    echo "Error: Failed to copy DLL file"
    exit 1
fi

echo ""
echo "========================================"
echo "DLL override completed successfully!"
echo "========================================"
