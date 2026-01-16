#!/bin/bash

echo "========================================"
echo "Proto2CS Server Tool"
echo "========================================"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
PROTO_DIR="$ROOT_DIR/Protocal"
PROTOC="$PROTO_DIR/protoc"
PROTOCOL_DIR="$PROTO_DIR/Proto"
OUTPUT_DIR="$ROOT_DIR/Server/LatServer/LatProtocol/Protocol"

if [ ! -f "$PROTOC" ]; then
    echo "Error: protoc not found at $PROTOC"
    exit 1
fi

chmod +x "$PROTOC"

if [ ! -d "$OUTPUT_DIR" ]; then
    mkdir -p "$OUTPUT_DIR"
else
    rm -f "$OUTPUT_DIR"/*
fi

echo "Compiling proto files..."
find "$PROTOCOL_DIR" -name "*.proto" | while read proto_file; do
    filename=$(basename "$proto_file")
    echo "Compiling $filename..."
    "$PROTOC" --csharp_out="$OUTPUT_DIR" --proto_path="$PROTOCOL_DIR" "$filename"
    if [ $? -ne 0 ]; then
        echo "Error compiling $filename"
        exit 1
    fi
done

echo ""
echo "Generating ProtocolID and ProtocolMapping..."
dotnet run --project "$SCRIPT_DIR/ProtoGenerator/ProtoGenerator.csproj" -- "$PROTOCOL_DIR" "$ROOT_DIR/Server/LatServer/LatProtocol"

if [ $? -ne 0 ]; then
    echo "Error generating protocol files"
    exit 1
fi

echo ""
echo "========================================"
echo "Proto2CS completed successfully!"
echo "========================================"
