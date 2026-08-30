# [Web] 정적 사이트 생성기 (SSG)

- **Date**: 2026-08-30
- **Tags**: #Web #SSG #StaticSite #Quartz #Markdown #HTML
- **Related**: [[[Web] npm]], [[[Git] Git Flow]]

---

# TL;DR

---

- 브라우저는 **HTML**만 이해한다. 마크다운(`.md`)은 브라우저가 읽지 못하므로, 웹에 올리려면 HTML로 **변환**해야 한다.
- **정적 사이트 생성기(SSG)** 는 마크다운 같은 원본을 읽어 HTML/CSS/JS 파일 묶음을 미리 만들어 두는 도구다. 서버가 요청마다 계산하는 대신 완성된 파일을 그대로 내보내므로 서버 비용이 0에 가깝다.
- 이 위키는 Obsidian 문법(`[[링크]]`, `#태그`, 그래프 뷰)을 그대로 지원하는 **Quartz**를 쓴다.

# 1. 왜 변환이 필요한가

---

### 1) 브라우저가 이해하는 것

| 파일 | 브라우저가 읽나 | 역할 |
|---|---|---|
| `.html` | O | 문서의 **구조** (제목, 문단, 링크, 표) |
| `.css` | O | **모양** (색, 글꼴, 배치) |
| `.js` | O | **동작** (검색, 그래프 애니메이션) |
| `.md` | **X** | 사람이 쓰기 편한 텍스트 문법. 브라우저는 그냥 글자로만 표시 |

- Obsidian은 `.md`를 예쁘게 렌더링해서 보여주지만, 그건 Obsidian 앱이 내부에서 변환하기 때문이다. 웹 브라우저에는 그런 기능이 없다.

### 2) 변환 예시

```
마크다운                          HTML
──────────────────────            ──────────────────────────────
# 데드락                      →   <h1>데드락</h1>
- 상호 배제                   →   <ul><li>상호 배제</li></ul>
[[[CS] Process]]              →   <a href="/CS-Process">Process</a>
`code`                        →   <code>code</code>
```

# 2. 정적 vs 동적 사이트

---

### 1) 비교

| 항목 | 정적 사이트 | 동적 사이트 |
|---|---|---|
| 페이지 생성 시점 | **미리** (빌드할 때 한 번) | **요청할 때마다** (서버가 즉석에서 조립) |
| 서버 역할 | 완성된 파일을 그대로 전달 | DB 조회 + 로직 실행 + HTML 조립 |
| 필요한 인프라 | 파일 서버만 (GitHub Pages 등 무료) | 앱 서버 + DB (비용 발생) |
| 속도 | 매우 빠름 | 서버 처리 시간만큼 느림 |
| 보안 | 공격 표면이 거의 없음 | 서버 코드·DB 취약점 관리 필요 |
| 적합한 용도 | 블로그, 문서, 위키, 포트폴리오 | 로그인, 장바구니, 실시간 데이터 |
| 한계 | 사용자별로 다른 내용을 서버에서 만들 수 없음 | — |

### 2) 흐름 도식

```
[정적 사이트]
  원본(.md) ──[빌드: SSG]──> HTML/CSS/JS 파일 ──> 파일 서버 ──> 브라우저
              (한 번만)                          (그대로 전달)

[동적 사이트]
  브라우저 요청 ──> 앱 서버 ──> DB 조회 ──> HTML 조립 ──> 브라우저
                   (요청마다 반복)
```

- 개인 위키는 내용이 바뀔 때만 다시 빌드하면 되므로 정적 사이트가 딱 맞는다. "쓰기"나 "LLM 질문" 같은 동적 기능은 GitHub Actions나 Cloudflare Workers 같은 **외부 서비스에 위임**하고, 사이트 자체는 정적으로 유지한다.

# 3. 정적 사이트 생성기(SSG)

---

### 1) 하는 일

```
입력                    SSG 내부 처리                       출력
──────────              ────────────────────────────         ──────────────
Knowledge/*.md   ──>   1. 파일 읽기                   ──>   public/
(원본 문서)             2. 마크다운 파싱                       index.html
                        3. 링크·태그 해석                      CS-Deadlock.html
                        4. 템플릿에 끼워 HTML 생성              static/
                        5. 검색 인덱스·그래프 데이터 생성        search.json
                        6. CSS/JS 복사                          graph.json
```

- 원본은 사람이 쓰기 편한 형식(마크다운)으로 유지하고, 웹용 결과물(`public/`)은 빌드할 때마다 새로 만든다. **결과물은 git에 올리지 않는다** — 원본에서 언제든 재생성 가능하기 때문이다.

### 2) 주요 SSG 비교

| 도구 | 언어 | 특징 | Obsidian 문법 지원 |
|---|---|---|---|
| **Quartz** | TypeScript | Obsidian 볼트 전용. `[[링크]]`, `#태그`, 그래프 뷰, 백링크 기본 내장 | **O (기본)** |
| Jekyll | Ruby | GitHub Pages 공식 기본. 블로그 중심 | X (플러그인 필요) |
| Hugo | Go | 매우 빠른 빌드. 테마 많음 | △ (테마·설정 필요) |
| Docusaurus | React | 기술 문서 사이트. 버전 관리 기능 | X |
| MkDocs | Python | 파이썬 프로젝트 문서 정석 | △ (플러그인) |
| Astro / Next.js | JS | 범용 프레임워크. SSG 기능 포함 | X (직접 구현) |

### 3) Quartz를 고른 이유
- Obsidian 문법을 **추가 설정 없이** 지원한다. 다른 도구는 `[[링크]]`를 일반 링크로 바꾸는 플러그인을 붙여야 한다.
- 그래프 뷰, 백링크, 태그 페이지, 전문 검색이 기본 컴포넌트로 들어 있다.
- TypeScript 소스가 그대로 제공되어 필요한 부분을 직접 고칠 수 있다 (예: 파일명의 `[대괄호]` 처리).
- v4.5.2를 고정해서 쓴다. 특정 버전을 고정하면 도구가 업데이트되어도 사이트가 갑자기 깨지지 않는다.

# 4. Quartz 구조

---

### 1) 폴더 구성

```
quartz4/
  quartz/               ← 설계도 (Quartz 소스 코드)
    build.ts            ← 빌드 전체 흐름
    plugins/            ← 변환 규칙 (transformers / filters / emitters)
    components/         ← 화면 조각 (검색창, 그래프, 목차, 백링크)
    styles/             ← 디자인 (SCSS)
    util/               ← 보조 함수 (URL 변환 등)
  quartz.config.ts      ← 사이트 설정 (제목, 언어, 폰트, 플러그인 순서)
  quartz.layout.ts      ← 화면 배치 (어느 컴포넌트를 어디에)
  package.json          ← 부품 목록 ([[[Web] npm]] 참고)
  package-lock.json     ← 부품 상세 명세
  node_modules/         ← 실제 부품 (npm ci로 생성, git 제외)
  content/              ← 원본 문서 폴더 (기본값. -d 옵션으로 바꿀 수 있음)
  public/               ← 빌드 결과물 (git 제외)
```

### 2) 플러그인 파이프라인

| 종류 | 역할 | 예시 |
|---|---|---|
| **Transformer** | 마크다운 하나하나를 변환 | `FrontMatter`(메타데이터 읽기), `ObsidianFlavoredMarkdown`(`[[링크]]`·`#태그` 해석), `SyntaxHighlighting`(코드 색칠), `TableOfContents` |
| **Filter** | 어떤 문서를 빼거나 남길지 결정 | `RemoveDrafts`(`draft: true` 문서 제외) |
| **Emitter** | 최종 파일을 실제로 써냄 | `ContentPage`(문서 → HTML), `TagPage`(태그별 목록), `ContentIndex`(검색·그래프용 JSON), `Assets`(이미지·`.cs` 등 그대로 복사) |

```
.md 파일 ──> [Transformer 1] ──> [Transformer 2] ──> ... ──> [Filter] ──> [Emitter] ──> .html
             FrontMatter        OFM(링크·태그)          TOC      Drafts    ContentPage
```

- 순서가 중요하다. 예를 들어 링크 해석(OFM)은 메타데이터 읽기(FrontMatter) 뒤에 와야 한다.

### 3) 실행 명령

| 명령 | 역할 |
|---|---|
| `npx quartz build -d Knowledge` | `Knowledge/` 폴더를 원본으로 삼아 `public/`에 HTML 생성 |
| `npx quartz build --serve` | 빌드 후 로컬 서버(`localhost:8080`)로 미리보기. 파일 수정 시 자동 재빌드 |

# 5. 실전 사례 — 이 위키에서 손본 지점

---

### 1) 파일명의 대괄호
- 이 위키는 `[CS] Deadlock.md`처럼 파일명에 `[태그]`를 붙이고, 링크도 `[[[CS] Deadlock]]` 형태로 쓴다.
- Quartz의 링크 인식 정규식은 `[[...]]` 안에 `[`나 `]`가 있으면 링크로 보지 않는다 → 정규식을 고쳐야 한다.
- URL에 `[`, `]`가 들어가면 보기 흉하고 일부 환경에서 문제가 생긴다 → URL 변환 함수(`sluggify`)에서 대괄호를 제거한다 (`[CS] Deadlock` → `CS-Deadlock`).

### 2) 제목
- Quartz는 기본적으로 frontmatter의 `title`을 제목으로 쓰고, 없으면 파일명을 쓴다.
- 이 위키는 frontmatter 없이 첫 줄 `# 국문 제목`을 쓴다 → 첫 H1을 제목으로 가져오는 transformer를 추가한다.

### 3) `.cs` 파일
- Quartz는 `.md`만 페이지로 만든다. `.cs`는 `Assets` emitter가 그대로 복사만 한다.
- 그래서 `Code_snippets/` 폴더마다 `[Code] 이름.md` 설명 문서를 두어 페이지·검색·그래프에 잡히게 한다.

# 6. 용어 정리

---

| 용어 | 뜻 |
|---|---|
| HTML | 웹 페이지의 구조를 적는 언어. 브라우저가 읽는 최종 형식 |
| 정적 파일 | 서버에서 계산 없이 그대로 전달되는 파일 (HTML, CSS, JS, 이미지) |
| 빌드(build) | 원본을 결과물로 변환하는 과정. SSG에서는 `.md` → HTML |
| frontmatter | 마크다운 맨 위 `---` 사이에 적는 메타데이터 (제목, 날짜, 태그). 이 위키는 쓰지 않음 |
| slug | 제목이나 파일명을 URL용 문자열로 바꾼 것 (`[CS] Deadlock` → `CS-Deadlock`) |
| transformer / emitter | SSG 플러그인의 종류. 변환하는 것 / 파일로 써내는 것 |
| 호스팅 | 완성된 파일을 인터넷에 올려두는 것. 정적 파일은 GitHub Pages 등에서 무료 |
