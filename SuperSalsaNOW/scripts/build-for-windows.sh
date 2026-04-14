#!/bin/bash
# Build SuperSalsaNOW for Windows from macOS

set -euo pipefail

echo "========================================="
echo "SuperSalsaNOW - Build for Windows"
echo "========================================="
echo ""

# Colors
GREEN='\033[0;32m'
CYAN='\033[0;36m'
RED='\033[0;31m'
NC='\033[0m'

# Paths
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CLI_PROJECT="$PROJECT_ROOT/src/SuperSalsaNOW.Cli"
OUTPUT_DIR="$PROJECT_ROOT/dist/win-x64"

echo -e "${CYAN}Project root:${NC} $PROJECT_ROOT"
echo ""

# Clean previous build
if [ -d "$OUTPUT_DIR" ]; then
  echo "Cleaning previous build..."
  rm -rf "$OUTPUT_DIR"
fi

# Restore dependencies
echo -e "${CYAN}Restoring dependencies...${NC}"
dotnet restore "$PROJECT_ROOT/SuperSalsaNOW.sln"

# Build
echo -e "${CYAN}Building solution...${NC}"
dotnet build "$PROJECT_ROOT/SuperSalsaNOW.sln" -c Release

# Publish for Windows
echo -e "${CYAN}Publishing for Windows (win-x64)...${NC}"
dotnet publish "$CLI_PROJECT" \
  -c Release \
  -r win-x64 \
  --self-contained \
  -o "$OUTPUT_DIR" \
  /p:PublishSingleFile=true \
  /p:DebugType=None \
  /p:DebugSymbols=false

echo ""
echo -e "${GREEN}✓ Build complete!${NC}"
echo ""
echo "Output: $OUTPUT_DIR/"
echo ""
ls -lh "$OUTPUT_DIR" | grep -E "\\.exe$|\\.dll$|\\.json$"
echo ""
echo "To test on Windows:"
echo "  1. Transfer $OUTPUT_DIR to Windows machine"
echo "  2. Run SuperSalsaNOW.exe"
echo ""
