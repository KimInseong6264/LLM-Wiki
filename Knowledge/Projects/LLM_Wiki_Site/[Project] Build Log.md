# [Project] LLM Wiki 사이트 구축 — 진행 기록

- **Date**: 2026-08-30
- **Tags**: #Project #LLMWiki #BuildLog #Changelog
- **Related**: [[[Project] LLM Wiki Site]], [[[Web] npm]], [[[Web] Static Site Generator]]

---

# TL;DR

---

- [[[Project] LLM Wiki Site]] 프로젝트의 **날짜순 진행 기록**이다. 한 일, 겪은 문제, 바뀐 결정을 적는다.
- 큰 결정과 구조는 허브 문서에, 시간 흐름은 여기에. 규모가 큰 트러블슈팅은 `Debug_log/`로 분리하고 링크한다.
- 최신 항목이 아래에 온다 (시간순).

# 1. 기록 요약

---

| 날짜 | 단계 | 내용 | 커밋 |
|---|---|---|---|
| 2026-08-27 | 설계 | 목표·아키텍처·기술 선택 확정 | — |
| 2026-08-29 | 설계 | 작업을 목적 단위(A~G)로 재분류. `Code_snippets` 운영 방식, 동기화 방식 확정 | — |
| 2026-08-29 | A1 | 미커밋 변경 커밋 | `3fecd2f` |
| 2026-08-29 | A2 | `[Code] AStar.md`, `[Code] EventChannel.md` + Unity 문서에 역방향 링크 | `bc77f12` |
| 2026-08-29 | A3 | `Knowledge/index.md` 홈 문서 + AGENTS.md 예외 규칙 | `d5710d9` |
| 2026-08-30 | C1 | Quartz v4.5.2 클론 + `npm ci` (임시 폴더) | — |
| 2026-08-30 | 문서 | `[Web]` 태그 신설, `[Web] npm.md`, `[Web] Static Site Generator.md` | `e23b950` |
| 2026-08-30 | 구조 | `Projects/` 폴더·`[Project]` 태그 신설, 이 프로젝트 허브·진행 기록 문서 작성 | `42723fa` |

# 2. 상세 기록

---

### 2026-08-27 — 설계 확정
- 목표 두 가지 정의: (1) LLM으로 문서 작성 (2) 정리된 내용 기반 질문
- GitHub + Quartz + Gemini + Cloudflare Workers 조합 확정 (이유는 허브 3절)
- 검토 후 제외: Quartz v5(갓 출시, CI에서 플러그인을 git으로 받는 구조), Claude/OpenAI(유료), Obsidian Sync(유료), 사이트 내 에디터(인증 서버 필요)

### 2026-08-29 — 작업 재분류, 볼트 정리 (A단계)
- 작업을 볼트 정리(A) / 동기화(B) / 보기(C) / 편집(D) / LLM 쓰기(E) / LLM 질문(F) / 규칙 검사(G)로 재분류. 웹 작업과 무관한 A·B를 분리
- `Code_snippets` 운영 확정: `.cs` 원본 유지 + 폴더당 `[Code] 이름.md` 1장. Quartz가 `.cs`를 페이지로 만들지 못하기 때문
- A1~A3 완료. A4(`.obsidian/app.json`의 `userIgnoreFilters`)는 볼트가 `Knowledge/`라 효과가 적어 생략

### 2026-08-30 — C1, 개념 문서, 프로젝트 문서 구조

**C1: Quartz 준비**
- `git clone --depth 1 --branch v4.5.2 https://github.com/jackyzha0/quartz.git` → 임시 폴더(scratchpad)에 클론. 저장소 안에 두지 않는 이유: 불필요한 파일(`docs/`, `content/`, `.github/`) 제외, 패치 후 검증된 것만 옮기기 위해, 볼트 안에 `node_modules`가 생기면 Obsidian이 느려짐
- `npm ci` → 480개 패키지, 342MB, 23초. `package.json`에 직접 적힌 62개가 의존성의 의존성까지 풀리면서 480개가 됨
- npm 취약점 경고 12개(high 11)는 무시. Quartz는 빌드 시에만 실행되고 결과물은 정적 HTML이라 방문자에게 영향 없음. `npm audit fix --force`는 버전을 멋대로 올려 Quartz를 깨뜨릴 수 있어 실행하지 않음

**개념 문서화 방침**
- 웹 개념은 해당 개념을 처음 쓰는 단계에서 `[Web]` 문서로 남기기로 함. 지금: npm, SSG. 예정: GitHub Pages/Actions(C7), Cloudflare Workers(F1)
- `[Web]` 태그 신설. `[Git]`과의 경계: "GitHub 없이 Git만으로 성립하면 `[Git]`, 아니면 `[Web]`". GitHub Pages/Actions는 버전 관리가 아닌 호스팅·CI 서비스이므로 `[Web]`

**프로젝트 문서 구조 (위키의 의미적 확장)**
- 기존 위키는 "개념 학습 기록"이었으나, 이 사이트 구축 자체를 하나의 프로젝트로 보고 설계·결정·과정을 폴더 단위로 남기기로 함. 나중에 Unity 게임 프로젝트도 같은 방식으로 기록 가능
- `Knowledge/Projects/<프로젝트명>/` + `[Project]` 태그. 허브 문서 1개 + 주제별 하위 문서. 구성은 허브 6절 참고
- 역할 분담: `[Web]`은 "이게 뭔가", `[Project]`는 "우리는 왜·어떻게 했나", `[Debug]`는 "뭐가 깨졌나"
- 로컬 전용 계획 문서(`WEB_PLAN.md`, git 미추적)는 "앞으로 할 일" 용도로 유지하다가 완료 시 삭제. 결정 사항은 허브 문서로 이관됨

**다음**: C2 — Quartz 패치 3개 (허브 4절). 패치 내용은 `[Project] Quartz Patches`로 기록

# 3. 겪은 문제

---

| 날짜 | 문제 | 원인 | 해결 |
|---|---|---|---|
| — | (아직 없음) | | |
