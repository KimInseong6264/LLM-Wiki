# [Project] LLM Wiki 사이트 구축

- **Date**: 2026-08-30
- **Tags**: #Project #LLMWiki #Quartz #GitHubPages #Gemini #Architecture
- **Related**: [[[Project] Build Log]], [[[Web] Static Site Generator]], [[[Web] npm]], [[[Git] Git Flow]]

---

# TL;DR

---

- 이 저장소의 Obsidian 볼트(`Knowledge/`)를 웹에서 **보고, 편집하고, LLM으로 작성·질문**할 수 있게 만드는 프로젝트다.
- 구조: **GitHub 저장소(단일 진실 원천) → GitHub Actions가 Quartz로 빌드 → GitHub Pages에 배포**. 쓰기는 Issue → Gemini → PR, 질문은 사이트 채팅창 → Cloudflare Workers → Gemini. 비용 0.
- 이 문서는 프로젝트 **허브**다. 목표·아키텍처·기술 선택 이유·진행 상태를 담고, 세부 내용은 하위 문서로 연결한다. 진행 과정은 [[[Project] Build Log]] 참고.

# 1. 목표

---

### 1) 사용자가 원하는 기능

| # | 기능 | 예시 | 구현 방식 |
|---|---|---|---|
| 1 | **정리해서 문서로 남기기** | "데드락 정리해줘", "이 문서들 합쳐줘" → 파일 생성/수정 | Issue → GitHub Action → Gemini → PR |
| 2 | **정리된 내용 기반으로 묻기** | "싱글톤과 오브젝트 풀 차이가 뭐였지?" → 즉시 답변 | 사이트 채팅창 → Cloudflare Workers → Gemini |
| 3 | 보기 / 검색 / 그래프 | 브라우저에서 위키 탐색 | Quartz + GitHub Pages |
| 4 | 웹에서 편집 | 페이지마다 "GitHub에서 편집" / "새 문서" 버튼 | GitHub 웹 에디터 → 커밋 → 자동 재빌드 |

### 2) 제약
- 비용 0 (무료 티어만 사용)
- 서버 직접 운영 없음
- Obsidian 볼트 구조와 문서 규칙(`AGENTS.md`)을 그대로 유지
- 데이터는 GitHub 저장소에만 존재 (외부 DB 없음)

# 2. 아키텍처

---

```
                ┌──────────────── GitHub 저장소 (public) ────────────────────────────┐
                │  Knowledge/*.md  (Obsidian 볼트, 단일 진실 원천)                       │
                └──────┬───────────────────────────▲──────────────────────▲──────────┘
                       │ push/merge                 │ PR                   │ commit
                       ▼                            │                      │
        [Action: Quartz 빌드 → Pages 배포]   [Action: Issue 감지 → Gemini → 문서 생성 → PR]   [GitHub 웹 에디터]
                       │                            ▲                      ▲
                       ▼                            │                      │
        ┌─── GitHub Pages (정적 사이트) ─────────────────┐                  │
        │  문서 보기 / 검색 / 그래프 / 태그                 │                  │
        │  [편집] [새 문서] 버튼 ──────────────────────────┼──────────────────┘
        │  [채팅창] ──→ Cloudflare Workers ──→ Gemini     │
        │        (contentIndex.json = 위키 전문 전달)      │
        └─────────────────────────────────────────────────┘
                                                    │
        [사용자: Issue 작성 "데드락 문서 써줘"] ─────┘
```

### 1) 구성 요소

| 구성 요소 | 역할 | 관련 개념 문서 |
|---|---|---|
| GitHub 저장소 | 문서 원본 저장. 모든 변경은 커밋으로 기록 | [[[Git] Git Flow]] |
| Quartz v4.5.2 | 마크다운 → HTML 변환 (정적 사이트 생성기) | [[[Web] Static Site Generator]], [[[Web] npm]] |
| GitHub Actions | push마다 서버에서 자동 빌드·배포. Issue 감지 후 Gemini 호출 | `[[[Web] GitHub Actions]]` (C7에서 작성 예정) |
| GitHub Pages | 빌드된 정적 파일 무료 호스팅 | `[[[Web] GitHub Pages]]` (C7에서 작성 예정) |
| Gemini API (무료 티어) | 문서 생성, 질문 답변 | — |
| Cloudflare Workers (무료) | 채팅창과 Gemini 사이에서 API 키를 숨기는 중계 서버 | `[[[Web] Cloudflare Workers]]` (F1에서 작성 예정) |

### 2) 데이터 흐름 요약
- **읽기**: 저장소 → (Actions 빌드) → Pages → 브라우저
- **쓰기(사람)**: 브라우저 → GitHub 웹 에디터 → 커밋 → 자동 재빌드
- **쓰기(LLM)**: Issue → Actions → Gemini → 브랜치 + PR → 사람이 검토 후 머지 → 자동 재빌드
- **질문**: 브라우저 채팅창 → Workers → Gemini → 답변 (저장소는 변경 없음)

# 3. 기술 선택과 이유

---

| 항목 | 선택 | 대안 | 이유 |
|---|---|---|---|
| 백엔드 | **GitHub** (저장소 + Actions + Pages) | 자체 서버, Notion | 비용 0, 서버 관리 없음, Obsidian 볼트 그대로 유지 |
| 사이트 생성기 | **Quartz v4.5.2** | Jekyll, Hugo, MkDocs, Quartz v5 | Obsidian `[[링크]]`·`#태그`·그래프 뷰 기본 지원. v5는 갓 출시 + 플러그인을 CI에서 git으로 받는 구조라 패치 번거로움 |
| LLM | **Gemini API 무료 티어** (Flash) | Claude, OpenAI | 유료 구독 없음. Flash 무료: 약 10~15 RPM, 500~1,500 RPD, 카드 불필요. 무료 티어 데이터는 학습에 쓰일 수 있음(인지함) |
| 쓰기 방식 | **Issue → Action → Gemini → PR** | 사이트에서 직접 생성 | 결과가 파일이므로 검토 후 머지하는 흐름이 맞음 |
| 질문 방식 | **채팅창 → Cloudflare Workers → Gemini** | Issue 방식 | Issue는 질문마다 몇 분 걸려 부적합. Workers가 API 키를 숨김 |
| 웹 편집 | **GitHub 웹 에디터로 연결하는 버튼** | 사이트 내 에디터 | 사이트 내 에디터는 인증·저장 서버가 필요. GitHub 에디터는 이미 있음 |
| 다기기 동기화 | **Obsidian Git 플러그인** | Obsidian Sync(유료) | 저장소가 이미 GitHub. 모바일 필요 시 Remotely Save 검토 |
| 저장소 공개 | **public** | private | 무료 계정에서 Pages는 public만 가능. 개인 정보는 넣지 않음 |
| `Code_snippets` 운영 | **`.cs` 원본 + 폴더당 `[Code] 이름.md` 1장** | `.md` 안에 코드 전부 포함 | `.cs`는 Unity에 바로 복사, `.md`는 설명·핵심 코드. Quartz는 `.cs`를 페이지로 만들지 못하므로 `.md`가 있어야 그래프·검색에 잡힘 |

# 4. Quartz에서 손봐야 하는 지점

---

이 위키의 문서 규칙이 Quartz 기본 동작과 맞지 않는 곳. 상세 패치 내용은 `[[[Project] Quartz Patches]]`(C2에서 작성 예정)에 기록한다.

| # | 문제 | 원인 | 대응 |
|---|---|---|---|
| 1 | `[[[CS] Deadlock]]` 링크가 인식 안 됨 | Quartz 링크 정규식이 `[[...]]` 안의 `[`, `]`를 허용하지 않음 | `quartz/plugins/transformers/ofm.ts`의 `wikilinkRegex` 수정 |
| 2 | URL에 `[`, `]`가 들어감 | 파일명을 그대로 URL로 씀 | `quartz/util/path.ts`의 `sluggify()`에서 대괄호 제거 → `[CS] Deadlock` → `CS-Deadlock` |
| 3 | 페이지 제목이 파일명으로 나옴 | Quartz는 frontmatter `title`을 쓰는데 이 위키는 첫 줄 `# 국문 제목`을 씀 | 첫 H1을 제목으로 가져오는 커스텀 transformer `FirstH1Title` 추가 |
| 4 | `.cs` 파일이 페이지가 안 됨 | Quartz는 `.md`만 페이지화 | `[Code] *.md` 설명 문서가 페이지 역할 (이미 적용) |

- 인라인 `#태그`는 Quartz OFM 플러그인이 자동 인식 (frontmatter 불필요)
- 한글 검색은 Quartz가 비ASCII를 글자 단위로 토큰화해서 동작함

# 5. 작업 단계와 상태

---

### 1) 의존 관계

```
A(볼트 정리) ──┬──> C(보기용 사이트) ──> D(편집 버튼)
               │                    └──> F(질문 채팅창)
               └──> E(LLM 문서 작성)          (C 없이도 가능)
B(다기기 동기화)  독립
G(규칙 자동 검사)  E 이후 권장
```

### 2) 상태 (2026-08-30 기준)

| 단계 | 내용 | 상태 | 하위 문서 |
|---|---|---|---|
| A | 볼트 정리 (`[Code]` 문서, `index.md`, 미커밋 정리) | **완료** | — |
| B | 다기기 동기화 (Obsidian Git 플러그인) | 미착수 | — |
| C | 보기용 사이트 (Quartz + Pages) | **진행 중** — C1 완료, C2부터 | `[Project] Quartz Patches`, `[Project] Deploy Workflow` |
| D | 웹 편집 버튼 | 미착수 | (허브에 절 추가 예정) |
| E | LLM 문서 작성 파이프라인 (Issue → PR) | 미착수 | `[Project] LLM Pipeline` |
| F | 질문용 채팅창 (Workers) | 미착수 | `[Project] Chat QA` |
| G | 규칙 자동 검사 Action | 미착수 | — |

### 3) C단계 세부

| # | 작업 | 상태 |
|---|---|---|
| C1 | Quartz v4.5.2 클론 + `npm ci` (임시 폴더) | 완료 |
| C2 | 패치 3개 적용 (4절) | — |
| C3 | `quartz.config.ts` / `quartz.layout.ts` 설정 | — |
| C4 | `Knowledge` 복사해서 로컬 빌드 검증 | — |
| C5 | 검증된 파일만 저장소 루트로 복사 | — |
| C6 | `.gitignore`에 빌드 산출물 추가 | — |
| C7 | 배포 워크플로 작성 → Pages 활성화 → 첫 배포 | — |

- 사용자 선행 작업(E단계 전까지): [AI Studio](https://aistudio.google.com/apikey)에서 Gemini 키 발급 → 저장소 `Settings → Secrets and variables → Actions`에 `GEMINI_API_KEY` 등록

# 6. 이 프로젝트의 문서 구성

---

### 1) 폴더

```
Knowledge/Projects/LLM_Wiki_Site/
  [Project] LLM Wiki Site.md      ← 이 문서 (허브)
  [Project] Build Log.md          ← 날짜순 진행 기록, 겪은 문제, 결정 변경
  [Project] Quartz Patches.md     ← 소스 수정 목록 (파일·내용·이유). 업그레이드 시 재적용 체크리스트  (C2)
  [Project] Deploy Workflow.md    ← Actions → Pages 배포 구조, 설정값, 운영 방법                    (C7)
  [Project] LLM Pipeline.md       ← Issue → Gemini → PR 구조, 프롬프트, 규칙 강제 방식               (E)
  [Project] Chat QA.md            ← 채팅창 ↔ Workers ↔ Gemini 구조, CORS, 컨텍스트 전달 방식        (F)
```

### 2) 역할 분담

| 폴더 | 답하는 질문 | 예 |
|---|---|---|
| `CS_knowledge/` `[Web]` | **이게 뭔가** | npm이 뭔지, SSG가 뭔지 |
| `Projects/` `[Project]` | **우리는 왜·어떻게 했나** | 왜 Quartz인지, 어디를 고쳤는지, 어떻게 배포되는지 |
| `Debug_log/` `[Debug]` | **뭐가 깨졌고 어떻게 고쳤나** | 빌드 실패 원인 분석 |

- 개념은 개념 문서에 쓰고 여기서는 링크만 한다. 개념 문서는 해당 개념을 처음 쓰는 단계에서 작성한다 (C7: GitHub Pages/Actions, F1: Cloudflare Workers)
- 이 문서들은 사이트에 공개된다. API 키·개인 정보는 적지 않는다

# 7. 저장소 현황

---

| 항목 | 값 |
|---|---|
| 저장소 | `KimInseong6264/LLM-Wiki` (public, 기본 브랜치 `main`) |
| 사이트 주소 (예정) | `https://kiminseong6264.github.io/LLM-Wiki` |
| 로컬 환경 | Node v24.15.0, npm 11.12.1, git 2.54 (Quartz 요구: Node ≥ 22, npm ≥ 10.9.2) |
| 문서 수 (2026-08-30) | 17개 (`CS_knowledge` 10, `Unity` 3, `Code_snippets` 2, `Projects` 2, `index.md`) — `.cs` 원본 7개 별도 |
| 알려진 문제 | `[CSharp] Struct Dictionary Boxing.md`의 Related가 `[[CSharp] Struct]]`처럼 `[` 하나 빠져 있고 대상 문서도 없음 (나중에 손보기로 함) |
