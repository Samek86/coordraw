# Coordraw

<div align="center">

**사람/AI가 쉽게 배울 수 있는 DSL을 갖춘 무료 셀프 호스팅 다이어그램 툴킷**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Node.js](https://img.shields.io/badge/Node.js-≥22.12-green.svg)](https://nodejs.org/)

[기능](#기능) • [빠른 시작](#빠른-시작) • [아키텍처](#아키텍처) • [English README](./README.md)

</div>

---

## 개요

Coordraw는 사람의 창의성과 AI 지원 워크플로우를 연결하는 **무료 오픈소스 다이어그램 작성 툴킷**입니다. 다음을 제공합니다:

1. **간결한 커스텀 DSL** - 사람/AI가 배우기 쉽고 간결한 다이어그램 작성 문법
2. **컴파일러** - DSL을 Eraser Diagrams JSON(좌표 기반 형식)으로 변환
3. **자유 배치 GUI** - 픽셀 단위로 정확한 드래그/크기 조정 캔버스
4. **일본어(日本語) 라벨 지원** - Noto Sans CJK JP를 통한 완전한 CJK 렌더링
5. **Eraser Diagrams OSS 의존성** - `@eraserlabs/diagrams`를 라이브러리로 사용(포크 아님)

## 기능

✅ **SaaS 종속성 없음** - 셀프 호스팅, MIT 라이선스  
✅ **일본어 텍스트 지원** - Noto Sans CJK JP로 검증됨 (クライアント, 本番環境 등)  
✅ **간단한 DSL** - 사람이 작성하기 쉽고, AI가 생성하기 쉬움  
✅ **자유 캔버스** - 박스를 정확한 픽셀로 이동, 핸들로 크기 조정  
✅ **컴파일 및 렌더** - DSL → JSON → PNG 워크플로우용 CLI 도구  

## 비목표

❌ 실시간 협업  
❌ 호스팅된 AI 채팅 통합  
❌ Eraser.io SaaS 기능  
❌ Mermaid를 진실의 원천으로 사용  

## 빠른 시작

### 사전 요구사항

- **Node.js** ≥ 22.12
- **Chrome/Chromium** (렌더링용)
- **Noto Sans CJK JP** 시스템 폰트 (일본어 라벨용)

#### Noto Sans CJK JP 설치

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
[Google Fonts](https://fonts.google.com/noto/specimen/Noto+Sans+JP)에서 다운로드

### 설치

```bash
git clone https://github.com/Samek86/coordraw.git
cd coordraw
pnpm install
pnpm build
```

### 일본어 데모 실행

```bash
# DSL을 JSON으로 컴파일
pnpm coordraw compile examples/demo-japanese.crd -o out/demo.json

# 일본어 폰트로 PNG 렌더링
pnpm coordraw render examples/demo-japanese.crd -o out/demo.png --fonts examples/fonts.json
```

### GUI 에디터 시작

```bash
pnpm dev
```

http://localhost:5173 열기

- `examples/demo-japanese.crd` 또는 `examples/simple.crd` 로드
- 노드 드래그, 핸들로 크기 조정
- JSON으로 다시 내보내기

## 아키텍처

```
┌─────────────────────────────────────────────────────────────┐
│  Monorepo (pnpm workspaces)                                 │
├─────────────────────────────────────────────────────────────┤
│  packages/dsl          - 파서, 타입, 문법                   │
│  packages/compiler     - DSL→Eraser JSON + CLI              │
│  apps/editor           - React Flow GUI (드래그/크기조정)    │
│  examples/             - 일본어 + 간단한 데모               │
└─────────────────────────────────────────────────────────────┘

흐름:
  DSL (.crd)
    ↓ 파싱
  AST
    ↓ 컴파일
  Eraser JSON
    ↓ 렌더링 (@eraserlabs/diagrams-cli)
  PNG
```

### Eraser Diagrams 의존성

Coordraw는 Eraser Diagrams를 **라이브러리 의존성**으로 **사용**합니다 (`@eraserlabs/diagrams`, `@eraserlabs/diagrams-cli`). Eraser 코드베이스를 **포크하지 않습니다**. 렌더링은 공식 Eraser CLI에 위임됩니다.

## DSL 문법

### 예제: 간단한 다이어그램

\`\`\`
diagram "My Architecture"

box frontend label="Frontend" x=50 y=100 w=120 h=60 color=blue
box backend label="Backend" x=250 y=100 w=140 h=60 color=green

edge frontend -> backend label="REST API"
\`\`\`

### 문법 참조

| 명령어 | 문법 | 설명 |
|---------|--------|-------------|
| `diagram` | `diagram "<title>"` | 다이어그램 메타데이터 |
| `box` | `box <id> label="..." x=<n> y=<n> w=<n> h=<n> [color=<color>]` | 사각형 도형 |
| `group` | `group <id> title="..." x=<n> y=<n> w=<n> h=<n> [color=<color>]`<br>`  ...자식...`<br>`end` | 컨테이너 그룹 |
| `icon` | `icon <id> icon=<type> label="..." x=<n> y=<n> [color=<color>]` | 아이콘/심볼 |
| `edge` | `edge <from> -> <to> [label="..."] [style=dashed\|solid\|dotted]` | 연결선 |

**색상:** `red`, `orange`, `yellow`, `green`, `blue`, `purple`, `pink`, `gray`, `black`, `white`  
**아이콘 타입:** `server`, `database`, `postgres`, `redis`, `cloud`, `user`, `lock`, `code`

### 일본어 예제

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

## CLI 참조

### 컴파일

DSL을 Eraser JSON으로 변환:

\`\`\`bash
pnpm coordraw compile <input.crd> -o <output.json>
\`\`\`

### 렌더

DSL을 컴파일하고 PNG로 렌더링:

\`\`\`bash
pnpm coordraw render <input.crd> -o <output.png> [--fonts <fonts.json>] [--chromium <path>]
\`\`\`

**옵션:**
- `--fonts` - 커스텀 폰트 설정 (일본어에 필요)
- `--chromium` - Chromium 바이너리 경로

## 개발

### 프로젝트 구조

```
coordraw/
├── packages/
│   ├── dsl/              # DSL 파서
│   │   ├── src/
│   │   │   ├── types.ts
│   │   │   ├── parser.ts
│   │   │   └── index.ts
│   │   └── package.json
│   └── compiler/         # 컴파일러 + CLI
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

### 스크립트

```bash
pnpm install       # 의존성 설치
pnpm build         # 모든 패키지 빌드
pnpm test          # 테스트 실행
pnpm dev           # 에디터 개발 서버 시작
```

### 테스트

```bash
pnpm test          # vitest로 모든 테스트 실행
```

테스트 커버리지:
- 다양한 문법의 DSL 파서
- 컴파일러 DSL→Eraser JSON 변환
- 오류 처리

### 기술 스택

- **언어:** TypeScript
- **빌드:** Vite
- **테스트:** Vitest
- **GUI:** React + React Flow (MIT 라이선스)
- **렌더링:** @eraserlabs/diagrams + @eraserlabs/diagrams-cli
- **폰트:** Noto Sans CJK JP (시스템 폰트)

## 폰트 구성

일본어 렌더링을 위해 이 `fonts.json`을 사용하세요:

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

CLI에 전달:

```bash
pnpm coordraw render diagram.crd --fonts fonts.json -o output.png
```

## 로드맵

**v0.1 (초기 릴리스)** ✅
- DSL 파서
- Eraser JSON 컴파일러
- CLI 컴파일/렌더
- 기본 GUI 에디터
- 일본어 예제

**v0.2 (계획됨)**
- 좌표 누락 시 자동 레이아웃 폴백
- 더 많은 아이콘 타입
- 개선된 GUI: 더 나은 핸들, 스내핑, 정렬 도구
- DSL→GUI→DSL 라운드 트립 (역컴파일러)

**v1.0 (계획됨)**
- 안정적인 DSL 스키마
- 커스텀 도형용 플러그인 시스템
- SVG/PDF 내보내기
- 브라우저 전용 사용을 위한 WASM 파서

## 기여

기여를 환영합니다! 다음을 수행하세요:

1. 저장소 포크
2. 기능 브랜치 생성
3. 새 기능에 대한 테스트 작성
4. `pnpm build && pnpm test` 통과 확인
5. PR 제출

## 라이선스

MIT 라이선스 - [LICENSE](./LICENSE) 참조

Copyright (c) 2026 Jun

## 감사의 말

- [Eraser Diagrams](https://github.com/eraserlabs/diagrams) - 핵심 렌더링 엔진
- [React Flow](https://reactflow.dev/) - GUI 캔버스 (MIT)
- [Noto Sans CJK JP](https://fonts.google.com/noto/specimen/Noto+Sans+JP) - CJK 폰트 지원

---

**참고:** 이 프로젝트는 Eraser Diagrams를 포크가 아닌 의존성으로 사용합니다. 렌더링은 `@eraserlabs/diagrams-cli`가 처리합니다. Coordraw는 Eraser의 좌표 기반 JSON 형식 위에 DSL 작성 및 자유 배치 편집에 중점을 둡니다.
