# Coordraw

<div align="center">

**Free, self-hosted diagram toolkit with human/AI-teachable DSL**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Node.js](https://img.shields.io/badge/Node.js-≥22.12-green.svg)](https://nodejs.org/)

[Features](#features) • [Quick Start](#quick-start) • [Architecture](#architecture) • [Korean README](./README.ko.md)

</div>

---

## Overview

Coordraw is a **free, open-source diagram authoring toolkit** that bridges human creativity and AI-assisted workflows. It provides:

1. **Thin custom DSL** - Human/AI-teachable, compact syntax for diagram authoring
2. **Compiler** - Transforms DSL to Eraser Diagrams JSON (coordinate-based format)
3. **Free-positioning GUI** - Pixel-perfect drag/resize canvas for visual editing
4. **Japanese (日本語) label support** - Full CJK rendering via Noto Sans CJK JP
5. **Dependency on Eraser Diagrams OSS** - Uses `@eraserlabs/diagrams` as a library (not a fork)

## Features

✅ **No SaaS lock-in** - Self-hosted, MIT licensed  
✅ **Japanese text support** - Proven with Noto Sans CJK JP (クライアント, 本番環境, etc.)  
✅ **Simple DSL** - Easy for humans to write, easy for AI to generate  
✅ **Free canvas** - Move boxes to exact pixels, resize with handles  
✅ **Compile & render** - CLI tools for DSL → JSON → PNG workflows  

## Non-Goals

❌ Real-time collaboration  
❌ Hosted AI chat integration  
❌ Eraser.io SaaS features  
❌ Mermaid as source of truth  

## Quick Start

### Prerequisites

- **Node.js** ≥ 22.12
- **Chrome/Chromium** (for rendering)
- **Noto Sans CJK JP** system font (for Japanese labels)

#### Install Noto Sans CJK JP

**Ubuntu/Debian:**
```bash
sudo apt-get install fonts-noto-cjk
```

**macOS:**
```bash
brew tap homebrew/cask-fonts
brew install font-noto-sans-cjk-jp
```

**Windows:**  
Download from [Google Fonts](https://fonts.google.com/noto/specimen/Noto+Sans+JP)

### Installation

```bash
git clone https://github.com/Samek86/coordraw.git
cd coordraw
pnpm install
pnpm build
```

### Try the Japanese Demo

```bash
# Compile DSL to JSON
pnpm coordraw compile examples/demo-japanese.crd -o out/demo.json

# Render to PNG with Japanese fonts
pnpm coordraw render examples/demo-japanese.crd -o out/demo.png --fonts examples/fonts.json
```

### Start the GUI Editor

```bash
pnpm dev
```

Open http://localhost:5173

- Load `examples/demo-japanese.crd` or `examples/simple.crd`
- Drag nodes, resize with handles
- Export back to JSON

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  Monorepo (pnpm workspaces)                                 │
├─────────────────────────────────────────────────────────────┤
│  packages/dsl          - Parser, types, grammar             │
│  packages/compiler     - DSL→Eraser JSON + CLI              │
│  apps/editor           - React Flow GUI (drag/resize)       │
│  examples/             - Japanese + simple demos            │
└─────────────────────────────────────────────────────────────┘

Flow:
  DSL (.crd)
    ↓ parse
  AST
    ↓ compile
  Eraser JSON
    ↓ render (@eraserlabs/diagrams-cli)
  PNG
```

### Dependency on Eraser Diagrams

Coordraw **uses** Eraser Diagrams as a **library dependency** (`@eraserlabs/diagrams`, `@eraserlabs/diagrams-cli`). It does **not fork** the Eraser codebase. Rendering is delegated to the official Eraser CLI.

## DSL Syntax

### Example: Simple Diagram

\`\`\`
diagram "My Architecture"

box frontend label="Frontend" x=50 y=100 w=120 h=60 color=blue
box backend label="Backend" x=250 y=100 w=140 h=60 color=green

edge frontend -> backend label="REST API"
\`\`\`

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

\`\`\`
diagram "本番環境アーキテクチャ"

box client label="クライアント" x=40 y=140 w=140 h=70 color=green

group prod title="本番環境" x=200 y=40 w=520 h=280 color=blue
  icon api icon=server label="注文API" x=280 y=130
  icon db icon=postgres label="Postgres" x=520 y=130
end

edge client -> api label="HTTPS"
edge api -> db label="書き込み"
\`\`\`

## CLI Reference

### Compile

Convert DSL to Eraser JSON:

\`\`\`bash
pnpm coordraw compile <input.crd> -o <output.json>
\`\`\`

### Render

Compile DSL and render to PNG:

\`\`\`bash
pnpm coordraw render <input.crd> -o <output.png> [--fonts <fonts.json>] [--chromium <path>]
\`\`\`

**Options:**
- `--fonts` - Custom font config (required for Japanese)
- `--chromium` - Path to Chromium binary

## Development

### Project Structure

```
coordraw/
├── packages/
│   ├── dsl/              # DSL parser
│   │   ├── src/
│   │   │   ├── types.ts
│   │   │   ├── parser.ts
│   │   │   └── index.ts
│   │   └── package.json
│   └── compiler/         # Compiler + CLI
│       ├── src/
│       │   ├── eraser-types.ts
│       │   ├── compiler.ts
│       │   ├── cli.ts
│       │   └── index.ts
│       └── package.json
├── apps/
│   └── editor/           # React Flow GUI
│       ├── src/
│       │   ├── components/
│       │   ├── App.tsx
│       │   └── main.tsx
│       └── package.json
├── examples/
│   ├── demo-japanese.crd
│   ├── simple.crd
│   └── fonts.json
├── package.json
├── pnpm-workspace.yaml
└── README.md
```

### Scripts

```bash
pnpm install       # Install dependencies
pnpm build         # Build all packages
pnpm test          # Run tests
pnpm dev           # Start editor dev server
```

### Testing

```bash
pnpm test          # Run all tests with vitest
```

Tests cover:
- DSL parser with various syntaxes
- Compiler DSL→Eraser JSON transformations
- Error handling

### Tech Stack

- **Language:** TypeScript
- **Build:** Vite
- **Tests:** Vitest
- **GUI:** React + React Flow (MIT license)
- **Rendering:** @eraserlabs/diagrams + @eraserlabs/diagrams-cli
- **Fonts:** Noto Sans CJK JP (system font)

## Font Configuration

For Japanese rendering, use this `fonts.json`:

\`\`\`json
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
\`\`\`

Pass it to the CLI:

```bash
pnpm coordraw render diagram.crd --fonts fonts.json -o output.png
```

## Roadmap

**v0.1 (Initial Release)** ✅
- DSL parser
- Compiler to Eraser JSON
- CLI compile/render
- Basic GUI editor
- Japanese example

**v0.2 (Planned)**
- Auto-layout fallback for omitted coordinates
- More icon types
- Improved GUI: better handles, snapping, alignment tools
- DSL→GUI→DSL round-trip (reverse compiler)

**v1.0 (Planned)**
- Stable DSL schema
- Plugin system for custom shapes
- Export to SVG/PDF
- WASM parser for browser-only usage

## Contributing

Contributions welcome! Please:

1. Fork the repo
2. Create a feature branch
3. Write tests for new features
4. Ensure `pnpm build && pnpm test` passes
5. Submit a PR

## License

MIT License - see [LICENSE](./LICENSE)

Copyright (c) 2026 Jun

## Acknowledgments

- [Eraser Diagrams](https://github.com/eraserlabs/diagrams) - Core rendering engine
- [React Flow](https://reactflow.dev/) - GUI canvas (MIT)
- [Noto Sans CJK JP](https://fonts.google.com/noto/specimen/Noto+Sans+JP) - CJK font support

---

**Note:** This project uses Eraser Diagrams as a dependency, not a fork. Rendering is handled by `@eraserlabs/diagrams-cli`. Coordraw focuses on DSL authoring and free-position editing on top of Eraser's coordinate-based JSON format.
