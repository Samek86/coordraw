# Coordraw

<div align="center">

**사람/AI가 쉽게 배울 수 있는 DSL을 갖춘 Windows 데스크톱 다이어그램 툴킷**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Framework](https://img.shields.io/badge/.NET_Framework-4.8-blue.svg)](https://dotnet.microsoft.com/)

[기능](#기능) • [빠른 시작](#빠른-시작) • [아키텍처](#아키텍처) • [English README](./README.md)

</div>

---

## 개요

Coordraw는 사람의 창의성과 AI 지원 워크플로우를 연결하는 **무료 오픈소스 Windows 데스크톱 다이어그램 작성 애플리케이션**입니다. 다음을 제공합니다:

1. **간결한 커스텀 DSL** - 사람/AI가 배우기 쉽고 간결한 다이어그램 작성 문법
2. **C# 컴파일러** - DSL을 Eraser Diagrams JSON(좌표 기반 형식)으로 변환
3. **WPF + WebView2 GUI** - 픽셀 단위로 정확한 드래그/크기 조정 캔버스
4. **일본어(日本語) 라벨 지원** - Noto Sans CJK JP를 통한 완전한 CJK 렌더링
5. **Eraser Diagrams OSS 의존성** - `@eraserlabs/diagrams-cli`를 외부 프로세스로 사용(포크 아님)

## 기능

✅ **SaaS 종속성 없음** - 셀프 호스팅 데스크톱 앱, MIT 라이선스  
✅ **일본어 텍스트 지원** - Noto Sans CJK JP로 검증됨 (クライアント, 本番環境 등)  
✅ **간단한 DSL** - 사람이 작성하기 쉽고, AI가 생성하기 쉬움  
✅ **자유 캔버스** - 박스를 정확한 픽셀로 이동, 핸들로 크기 조정  
✅ **컴파일 및 렌더** - DSL → JSON → PNG 워크플로우 via Eraser CLI  
✅ **Windows 데스크톱** - WPF .NET Framework 4.8 + WebView2  

## 비목표

❌ 실시간 협업  
❌ 호스팅된 AI 채팅 통합  
❌ Eraser.io SaaS 기능  
❌ Mermaid를 진실의 원천으로 사용  
❌ 크로스 플랫폼 (Windows 전용 데스크톱 앱)  

## 빠른 시작

### 사전 요구사항

**CLI 전용 워크플로우:**
- **.NET Framework 4.8** (최신 Windows에 사전 설치, Linux는 Mono를 통해 가능)
- **Node.js** ≥ 22.12 (Eraser CLI 렌더링용)
- **Chrome/Chromium** (렌더링용)
- **Noto Sans CJK JP** 시스템 폰트 (일본어 라벨용)

**GUI (Windows 데스크톱 전용):**
- **Windows** 10/11
- **Visual Studio 2019+** (소스에서 빌드용)
- **WebView2 Runtime** ([다운로드](https://developer.microsoft.com/en-us/microsoft-edge/webview2/))

#### Noto Sans CJK JP 설치

**Windows:**  
[Google Fonts](https://fonts.google.com/noto/specimen/Noto+Sans+JP)에서 다운로드하여 우클릭 → 설치

**Linux:**  
```bash
sudo apt-get install fonts-noto-cjk
# 또는
sudo yum install google-noto-sans-cjk-jp-fonts
```

#### 의존성 설치

```bash
cd coordraw
npm install
```

로컬에 `@eraserlabs/diagrams-cli` 및 `@eraserlabs/diagrams`를 설치합니다.

### CLI 사용법 (비GUI 워크플로우)

**DSL을 JSON으로 컴파일:**
```bash
# dotnet 사용
dotnet run --project src/Coordraw.Cli/Coordraw.Cli.csproj -- compile examples/demo-japanese.crd -o output.json

# 또는 빌드 후
./src/Coordraw.Cli/bin/Release/coordraw.exe compile examples/demo-japanese.crd -o output.json

# 또는 npm 스크립트 사용
npm run compile:demo
```

**DSL 유효성 검사:**
```bash
dotnet run --project src/Coordraw.Cli/Coordraw.Cli.csproj -- validate examples/demo-japanese.crd
# 또는: npm run validate:demo
```

**PNG로 렌더링:**
```bash
dotnet run --project src/Coordraw.Cli/Coordraw.Cli.csproj -- render examples/demo-japanese.crd -o output.png --fonts examples/fonts.json
# 또는: npm run render:demo
```

**모든 CLI 명령:**
```bash
coordraw compile <file.crd> -o <output.json>    # Eraser JSON으로 컴파일
coordraw validate <file.crd>                     # 구문 유효성 검사
coordraw render <file.crd> -o <output.png>       # 컴파일 + 렌더링
coordraw help                                    # 도움말 표시
```

### 소스에서 빌드

```bash
git clone https://github.com/Samek86/coordraw.git
cd coordraw
npm install

# 솔루션 빌드
msbuild Coordraw.sln /p:Configuration=Release

# CLI 실행 파일: src\Coordraw.Cli\bin\Release\coordraw.exe
```

### GUI (Windows 데스크톱) - 선택사항

**참고:** GUI (`Coordraw.App`)는 데스크톱 개발자가 소유한 WPF 데스크톱 스캐폴드입니다. 위의 비GUI CLI 워크플로우는 프로덕션 준비가 완료되었으며 완전히 작동합니다.

**GUI 빌드 및 실행:**
```bash
# Visual Studio에서 열기
start Coordraw.sln

# 빌드 및 실행 (F5)
```

실행 파일: `src\Coordraw.App\bin\Release\Coordraw.exe`

### 일본어 데모 실행

**CLI:**
```bash
npm run validate:demo
npm run compile:demo
npm run render:demo
# 출력: examples/demo-japanese.png
```

**GUI (가능한 경우):**
1. `Coordraw.exe` 실행
2. **File → Open DSL...**
3. `examples\demo-japanese.crd` 로드
4. 캔버스에서 다이어그램 보기 (노드 드래그, 핸들로 크기 조정)
5. **File → Render PNG...**로 일본어 폰트로 내보내기

## 아키텍처

```
┌─────────────────────────────────────────────────────────────┐
│  Windows 데스크톱 애플리케이션                               │
├─────────────────────────────────────────────────────────────┤
│  Coordraw.App          - WPF .NET Framework 4.8 + WebView2  │
│  Coordraw.Core         - C# DSL 파서 + 컴파일러            │
│  wwwroot/              - WebView2 캔버스 (HTML/JS)          │
│  examples/             - 일본어 + 간단한 데모               │
└─────────────────────────────────────────────────────────────┘

흐름:
  DSL (.crd)
    ↓ C# 파서
  AST
    ↓ C# 컴파일러
  Eraser JSON
    ↓ eraser-diagrams-cli 실행 (Node.js)
  PNG
```

### Eraser Diagrams 의존성

Coordraw는 Eraser Diagrams를 **외부 CLI 도구**로 **사용**합니다 (`@eraserlabs/diagrams-cli`). Eraser 코드베이스를 **포크하지 않습니다**. 렌더링은 서브프로세스를 통해 공식 Eraser CLI에 위임됩니다.

## DSL 문법

### 예제: 간단한 다이어그램

```
diagram "My Architecture"

box frontend label="Frontend" x=50 y=100 w=120 h=60 color=blue
box backend label="Backend" x=250 y=100 w=140 h=60 color=green

edge frontend -> backend label="REST API"
```

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

## 개발

### 프로젝트 구조

```
coordraw/
├── src/
│   ├── Coordraw.Core/         # C# DSL 파서 + 컴파일러
│   │   ├── Models/
│   │   ├── Parser/
│   │   ├── Compiler/
│   │   └── Coordraw.Core.csproj
│   ├── Coordraw.Cli/          # 콘솔 CLI (compile/render/validate)
│   │   ├── Commands/
│   │   ├── Program.cs
│   │   └── Coordraw.Cli.csproj
│   └── Coordraw.App/          # WPF 애플리케이션 (GUI 스캐폴드)
│       ├── wwwroot/           # WebView2 캔버스
│       │   ├── canvas.html
│       │   ├── canvas.css
│       │   └── canvas.js
│       ├── MainWindow.xaml
│       ├── MainWindow.xaml.cs
│       └── Coordraw.App.csproj
├── tests/
│   └── Coordraw.Core.Tests/   # NUnit 단위 테스트
│       ├── ParserTests.cs
│       ├── CompilerTests.cs
│       └── Coordraw.Core.Tests.csproj
├── examples/
│   ├── demo-japanese.crd      # 일본어 예제
│   ├── simple.crd             # 간단한 예제
│   └── fonts.json             # Noto Sans CJK JP 설정
├── Coordraw.sln
├── package.json               # Eraser CLI 도구
└── README.md
```

### 기술 스택

- **언어:** C# (.NET Framework 4.8)
- **CLI:** 콘솔 앱 (Coordraw.Cli)
- **GUI (선택사항):** WPF (Windows Presentation Foundation)
- **임베디드 브라우저:** WebView2
- **캔버스:** HTML/CSS/JavaScript (드래그/크기 조정)
- **JSON:** Newtonsoft.Json
- **테스팅:** NUnit 3.14
- **렌더링:** @eraserlabs/diagrams-cli (Node.js CLI)
- **폰트:** Noto Sans CJK JP (시스템 폰트)

### 빌드

Visual Studio에서 `Coordraw.sln`을 열고 빌드(F6). 또는 MSBuild 사용:

```bash
msbuild Coordraw.sln /p:Configuration=Release
```

**빌드 출력:**
- CLI: `src/Coordraw.Cli/bin/Release/coordraw.exe`
- Core 라이브러리: `src/Coordraw.Core/bin/Release/Coordraw.Core.dll`
- GUI (선택사항): `src/Coordraw.App/bin/Release/Coordraw.exe`

### 테스트

NUnit으로 단위 테스트 실행:

```bash
# Visual Studio Test Explorer 사용, 또는:
dotnet test

# 또는 MSBuild + vstest.console.exe 사용
msbuild Coordraw.sln /t:Build /p:Configuration=Debug
vstest.console.exe tests\Coordraw.Core.Tests\bin\Debug\Coordraw.Core.Tests.dll
```

**테스트 커버리지:**
- Parser: 일본어 DSL, 그룹, 아이콘, 엣지, 주석, 유효성 검사
- Compiler: JSON 출력, containerId, 색상 매핑, 유효성 검사

## 폰트 구성

일본어 렌더링을 위해 `examples/fonts.json` 사용:

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

WPF 앱은 렌더링 시 이를 `eraser-diagrams-cli`에 전달합니다.

## 로드맵

**v0.1 (초기 릴리스)** ✅
- C# DSL 파서
- Eraser JSON C# 컴파일러
- WPF + WebView2 데스크톱 앱
- 픽셀 단위 드래그/크기 조정 캔버스
- 일본어 예제

**v0.2 (계획됨)**
- 좌표 누락 시 자동 레이아웃 폴백
- 더 많은 아이콘 타입
- 개선된 캔버스: 스내핑, 정렬 도구, 연결선 편집
- DSL→GUI→DSL 라운드 트립 (역컴파일러)
- NUnit 단위 테스트

**v1.0 (계획됨)**
- 안정적인 DSL 스키마
- 커스텀 도형용 플러그인 시스템
- 직접 SVG/PDF 내보내기 (Eraser CLI 우회)
- 명령줄 컴파일러 도구

## 기여

기여를 환영합니다! 다음을 수행하세요:

1. 저장소 포크
2. 기능 브랜치 생성
3. 솔루션 빌드 성공 확인
4. PR 제출

## 라이선스

MIT 라이선스 - [LICENSE](./LICENSE) 참조

Copyright (c) 2026 Jun

## 감사의 말

- [Eraser Diagrams](https://github.com/eraserlabs/diagrams) - 외부 렌더링 CLI
- [WebView2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) - 임베디드 Chromium
- [Noto Sans CJK JP](https://fonts.google.com/noto/specimen/Noto+Sans+JP) - CJK 폰트 지원

---

**참고:** 이것은 WPF + WebView2로 빌드된 **Windows 데스크톱 애플리케이션**입니다. Eraser Diagrams CLI를 외부 렌더링 도구로 사용합니다(서브프로세스를 통해 실행). Coordraw는 Eraser의 좌표 기반 JSON 형식 위에 DSL 작성 및 자유 배치 편집에 중점을 둡니다.
