#!/bin/bash

CURRENT_DIR="$(cd "$(dirname "${BASH_SOURCE}")" && pwd)"
APP_NAME="extls"

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
fi
