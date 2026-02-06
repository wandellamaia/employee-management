#!/bin/bash
set -e

echo "Starting repository cleanup..."

# Ensure .gitignore exists
if [ ! -f .gitignore ]; then
    echo "Error: .gitignore not found!"
    exit 1
fi

echo "Removing all files from Git index (keeping local files)..."
git rm -r --cached . > /dev/null 2>&1

echo "Re-adding files (respecting .gitignore)..."
git add .

echo "Checking for remaining binary files..."
BINARIES=$(git ls-files | grep -E "\.(dll|pdb|exe|cache|suo|user)$" || true)

if [ -n "$BINARIES" ]; then
    echo "Warning: Some binary files are still tracked:"
    echo "$BINARIES"
else
    echo "Success: No binary files found in Git index."
fi

echo "Cleanup complete. status:"
git status
