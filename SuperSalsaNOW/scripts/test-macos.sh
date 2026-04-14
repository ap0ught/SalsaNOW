#!/bin/bash
# Run tests that work on macOS (cross-platform only)

set -euo pipefail

echo "========================================="
echo "SuperSalsaNOW - macOS Test Suite"
echo "========================================="
echo ""

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# Run unit tests
echo "Running unit tests..."
dotnet test "$PROJECT_ROOT/tests/SuperSalsaNOW.Core.Tests" \
  --logger "console;verbosity=normal"

echo ""
echo "✓ All cross-platform tests passed!"
