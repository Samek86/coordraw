# Coordraw

<div align="center">

**Windows desktop diagram toolkit with human/AI-teachable DSL**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Framework](https://img.shields.io/badge/.NET_Framework-4.8-blue.svg)](https://dotnet.microsoft.com/)

[Features](#features) • [Quick Start](#quick-start) • [Architecture](#architecture) • [Korean README](./README.ko.md)

</div>

---

## Overview

Coordraw is a **free, open-source Windows desktop diagram authoring application** that bridges human creativity and AI-assisted workflows. It provides:

1. **Thin custom DSL** - Human/AI-teachable, compact syntax for diagram authoring
2. **C# compiler** - Transforms DSL to Eraser Diagrams JSON (coordinate-based format)
3. **WPF + WebView2 GUI** - Pixel-perfect drag/resize canvas for visual editing
4. **Japanese (日本語) label support** - Full CJK rendering via Noto Sans CJK JP
5. **Dependency on Eraser Diagrams OSS** - Uses `@eraserlabs/diagrams-cli` as external process (not a fork)

## Features

✅ **No SaaS lock-in** - Self-hosted desktop app, MIT licensed  
✅ **Japanese text support** - Proven with Noto Sans CJK JP (クライアント, 本番環境, etc.)  
✅ **Simple DSL** - Easy for humans to write, easy for AI to generate  
✅ **Free canvas** - Move boxes to exact pixels, resize with handles  
✅ **Compile & render** - DSL → JSON → PNG workflows via Eraser CLI  
✅ **Windows desktop** - WPF .NET Framework 4.8 + WebView2  

## Non-Goals

❌ Real-time collaboration  
❌ Hosted AI chat integration  
❌ Eraser.io SaaS features  
❌ Mermaid as source of truth  
❌ Cross-platform (Windows-only desktop app)  

## Quick Start

### Prerequisites

- **Windows** 10/11
- **.NET Framework 4.8** (pre-installed on modern Windows)
- **Visual Studio 2019+** (for building from source)
- **WebView2 Runtime** ([download](https://developer.microsoft.com/en-us/microsoft-edge/webview2/))
- **Node.js** ≥ 22.12 (for Eraser CLI rendering)
- **Chrome/Chromium** (for rendering)
- **Noto Sans CJK JP** system font (for Japanese labels)

#### Install Noto Sans CJK JP

**Windows:**  
Download from [Google Fonts](https://fonts.google.com/noto/specimen/Noto+Sans+JP) and install

#### Install Eraser Diagrams CLI

```bash
cd coordraw
npm install
```

This installs `@eraserlabs/diagrams-cli` locally.

### Build and Run

```bash
git clone https://github.com/Samek86/coordraw.git
cd coordraw
npm install

# Open in Visual Studio
start Coordraw.sln

# Build and Run (F5)
```

Or build from command line:

```bash
msbuild Coordraw.sln /p:Configuration=Release
```

Executable: `src\Coordraw.App\bin\Release\Coordraw.exe`

### Try the Japanese Demo

1. Launch `Coordraw.exe`
2. **File → Open DSL...**
3. Load `examples\demo-japanese.crd`
4. View diagram in canvas (drag nodes, resize with handles)
5. **File → Render PNG...** to export with Japanese fonts

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  Windows Desktop Application                                │
├─────────────────────────────────────────────────────────────┤
│  Coordraw.App          - WPF .NET Framework 4.8 + WebView2  │
│  Coordraw.Core         - C# DSL parser + compiler           │
│  wwwroot/              - WebView2 canvas (HTML/JS)          │
│  examples/             - Japanese + simple demos            │
└─────────────────────────────────────────────────────────────┘

Flow:
  DSL (.crd)
    ↓ C# parser
  AST
    ↓ C# compiler
  Eraser JSON
    ↓ spawn eraser-diagrams-cli (Node.js)
  PNG
```

### Dependency on Eraser Diagrams

Coordraw **uses** Eraser Diagrams as an **external CLI tool** (`@eraserlabs/diagrams-cli`). It does **not fork** the Eraser codebase. Rendering is delegated to the official Eraser CLI via subprocess.

## DSL Syntax

### Example: Simple Diagram

```
diagram "My Architecture"

box frontend label="Frontend" x=50 y=100 w=120 h=60 color=blue
box backend label="Backend" x=250 y=100 w=140 h=60 color=green

edge frontend -> backend label="REST API"
```

### Grammar Reference

| Command | Syntax | Description |
|---------|--------|-------------|
| `diagram` | `diagram "<title>"` | Diagram metadata |
| `box` | `box <id> label="..." x=<n> y=<n> w=<n> h=<n> [color=<color>]` | Rectangle shape |
| `group` | `group <id> title="..." x=<n> y=<n> w=<n> h=<n> [color=<color>]`<br>`  ...children...`<br>`end` | Container group |
| `icon` | `icon <id> icon=<type> label="..." x=<n> y=<n> [color=<color>]` | Icon/symbol |
| `edge` | `edge <from> -> <to> [label="..."] [style=dashed\|solid\|dotted]` | Connection |

**Colors:** `red`, `orange`, `yellow`, `green`, `blue`, `purple`, `pink`, `gray`, `black`, `white`  
**Icon types:** `server`, `database`, `postgres`, `redis`, `cloud`, `user`, `lock`, `code`

### Japanese Example

```
diagram "本番環境アーキテクチャ"

box client label="クライアント" x=40 y=140 w=140 h=70 color=green

group prod title="本番環境" x=200 y=40 w=520 h=280 color=blue
  icon api icon=server label="注文API" x=280 y=130
  icon db icon=postgres label="Postgres" x=520 y=130
end

edge client -> api label="HTTPS"
edge api -> db label="書き込み"
```

## Development

### Project Structure

```
coordraw/
├── src/
│   ├── Coordraw.Core/         # C# DSL parser + compiler
│   │   ├── Models/
│   │   ├── Parser/
│   │   ├── Compiler/
│   │   └── Coordraw.Core.csproj
│   └── Coordraw.App/          # WPF application
│       ├── wwwroot/           # WebView2 canvas
│       │   ├── canvas.html
│       │   ├── canvas.css
│       │   └── canvas.js
│       ├── MainWindow.xaml
│       ├── MainWindow.xaml.cs
│       └── Coordraw.App.csproj
├── tests/
│   └── Coordraw.Core.Tests/   # Unit tests
├── examples/
│   ├── demo-japanese.crd
│   ├── simple.crd
│   └── fonts.json
├── Coordraw.sln
├── package.json               # Eraser CLI tooling
└── README.md
```

### Tech Stack

- **Language:** C# (.NET Framework 4.8)
- **GUI:** WPF (Windows Presentation Foundation)
- **Embedded Browser:** WebView2
- **Canvas:** HTML/CSS/JavaScript (drag/resize)
- **JSON:** Newtonsoft.Json
- **Rendering:** @eraserlabs/diagrams-cli (Node.js CLI)
- **Fonts:** Noto Sans CJK JP (system font)

### Building

Open `Coordraw.sln` in Visual Studio and build (F6). Or use MSBuild:

```bash
msbuild Coordraw.sln /p:Configuration=Release
```

### Testing

Unit tests for parser and compiler (NUnit/xUnit - to be added in future version).

## Font Configuration

For Japanese rendering, `examples/fonts.json` is used:

```json
{
  "roles": {
    "rough": "Noto Sans CJK JP",
    "clean": "Noto Sans CJK JP",
    "mono": "Noto Sans CJK JP"
  },
  "faces": [
    { "kind": "system", "family": "Noto Sans CJK JP" }
  ],
  "fallbacks": {
    "rough": "sans-serif",
    "clean": "sans-serif",
    "mono": "monospace"
  },
  "throwOnFontFail": false
}
```

The WPF app passes this to `eraser-diagrams-cli` when rendering.

## Roadmap

**v0.1 (Initial Release)** ✅
- C# DSL parser
- C# compiler to Eraser JSON
- WPF + WebView2 desktop app
- Pixel-perfect drag/resize canvas
- Japanese example

**v0.2 (Planned)**
- Auto-layout fallback for omitted coordinates
- More icon types
- Improved canvas: snapping, alignment tools, connection editing
- DSL→GUI→DSL round-trip (reverse compiler)
- Unit tests with NUnit

**v1.0 (Planned)**
- Stable DSL schema
- Plugin system for custom shapes
- Direct SVG/PDF export (bypass Eraser CLI)
- Command-line compiler tool

## Contributing

Contributions welcome! Please:

1. Fork the repo
2. Create a feature branch
3. Ensure solution builds successfully
4. Submit a PR

## License

MIT License - see [LICENSE](./LICENSE)

Copyright (c) 2026 Jun

## Acknowledgments

- [Eraser Diagrams](https://github.com/eraserlabs/diagrams) - External rendering CLI
- [WebView2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) - Embedded Chromium
- [Noto Sans CJK JP](https://fonts.google.com/noto/specimen/Noto+Sans+JP) - CJK font support

---

**Note:** This is a **Windows desktop application** built with WPF + WebView2. It uses Eraser Diagrams CLI as an external rendering tool (spawned via subprocess). Coordraw focuses on DSL authoring and free-position editing on top of Eraser's coordinate-based JSON format.
