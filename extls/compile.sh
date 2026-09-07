#!/bin/bash

CURRENT_DIR="$(cd "$(dirname "${BASH_SOURCE}")" && pwd)"
APP_NAME="extls"
TARGET_BIN="$HOME/.local/bin/$APP_NAME"

if [ -f "$CURRENT_DIR/$APP_NAME" ]; then
    rm "$CURRENT_DIR/$APP_NAME"
fi

dotnet publish "$CURRENT_DIR" -c Release -r linux-x64 --self-contained true \
  /p:PublishSingleFile=true /p:PublishTrimmed=false \
  --output "$CURRENT_DIR"

if [ -f "$CURRENT_DIR/$APP_NAME.pdb" ]; then
    rm "$CURRENT_DIR/$APP_NAME.pdb"
fi

if [ -f "$CURRENT_DIR/$APP_NAME" ]; then
    chmod +x "$CURRENT_DIR/$APP_NAME"
    
    mkdir -p "$HOME/.local/bin"
    
    cp "$CURRENT_DIR/$APP_NAME" "$TARGET_BIN"
    echo "Binary successfully copied to: $TARGET_BIN"
    
    rm "$CURRENT_DIR/$APP_NAME"
    echo "Cleaned up temporary artifact from source directory."
fi