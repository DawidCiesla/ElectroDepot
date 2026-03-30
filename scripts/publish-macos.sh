#!/usr/bin/env bash
set -euo pipefail

# Usage: ./scripts/publish-macos.sh [osx-arm64|osx-x64|all]
RUNTIMES=${1:-all}
ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
OUTPUT_DIR="$ROOT_DIR/publish"
PROJECT="$ROOT_DIR/ElectroDepot/DesktopClient/DesktopClient.csproj"
APP_NAME="ElectroDepot"

mkdir -p "$OUTPUT_DIR"

publish_runtime() {
  local RUNTIME=$1
  echo "Publishing for $RUNTIME..."
  dotnet publish "$PROJECT" -c Release -r "$RUNTIME" --self-contained true -p:PublishSingleFile=false -o "$OUTPUT_DIR/desktop/$RUNTIME"

  local PUB_DIR="$OUTPUT_DIR/desktop/$RUNTIME"
  local APP_BUNDLE_DIR="$OUTPUT_DIR/${APP_NAME}-${RUNTIME}.app"

  echo "Creating .app bundle at $APP_BUNDLE_DIR"
  rm -rf "$APP_BUNDLE_DIR"
  mkdir -p "$APP_BUNDLE_DIR/Contents/MacOS"
  mkdir -p "$APP_BUNDLE_DIR/Contents/Resources"

  # Copy published files into MacOS folder
  cp -R "$PUB_DIR/"* "$APP_BUNDLE_DIR/Contents/MacOS/"

  # Make executable bits: prefer a binary named like the app, otherwise mark all files executable
  if [ -f "$APP_BUNDLE_DIR/Contents/MacOS/$APP_NAME" ]; then
    chmod +x "$APP_BUNDLE_DIR/Contents/MacOS/$APP_NAME"
  else
    chmod +x "$APP_BUNDLE_DIR/Contents/MacOS/"* || true
  fi

  # Write a minimal Info.plist
  cat > "$APP_BUNDLE_DIR/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key>
  <string>${APP_NAME}</string>
  <key>CFBundleDisplayName</key>
  <string>${APP_NAME}</string>
  <key>CFBundleIdentifier</key>
  <string>com.electrodepot.desktop</string>
  <key>CFBundleVersion</key>
  <string>1.0</string>
  <key>CFBundleExecutable</key>
  <string>${APP_NAME}</string>
  <key>CFBundlePackageType</key>
  <string>APPL</string>
  <key>LSMinimumSystemVersion</key>
  <string>10.13</string>
</dict>
</plist>
EOF

  echo "Built $APP_BUNDLE_DIR"
}

if [ "$RUNTIMES" = "all" ]; then
  publish_runtime osx-arm64
  publish_runtime osx-x64
else
  publish_runtime "$RUNTIMES"
fi

echo "macOS publish complete. See publish/desktop and *.app bundles in publish/" 
