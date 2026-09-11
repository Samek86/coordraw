# Coordraw Examples

This directory contains example diagrams demonstrating the Coordraw DSL.

## Files

- **demo-japanese.crd** - Japanese labels demo matching the PoC (クライアント, 本番環境, etc.)
- **simple.crd** - Basic microservices architecture
- **fonts.json** - Font configuration for Japanese rendering with Noto Sans CJK JP

## Usage

### Compile to JSON

```bash
pnpm coordraw compile examples/demo-japanese.crd -o out/demo.json
```

### Render to PNG

```bash
pnpm coordraw render examples/demo-japanese.crd -o out/demo.png --fonts examples/fonts.json
```

Make sure you have:
- Node.js 22.12+
- Chrome/Chromium installed
- Noto Sans CJK JP system font installed

## Installing Noto Sans CJK JP

### Ubuntu/Debian
```bash
sudo apt-get install fonts-noto-cjk
```

### macOS
```bash
brew tap homebrew/cask-fonts
brew install font-noto-sans-cjk-jp
```

### Windows
Download from [Google Fonts](https://fonts.google.com/noto/specimen/Noto+Sans+JP) and install.
