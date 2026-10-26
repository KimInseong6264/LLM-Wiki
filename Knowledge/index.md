# LLM Wiki

Unity 게임 개발자의 개인 지식 베이스입니다. 공부한 내용, 개발 노하우, 재활용 코드를 마크다운으로 축적합니다.

---

# 1. 분류

---

| 폴더 | 내용 | 파일 태그 |
|---|---|---|
| `CS_knowledge/` | 자료구조, 알고리즘, CS 이론, C# 언어, Git, 웹 도구 | `[CS]` `[CSharp]` `[Git]` `[Web]` |
| `Unity/` | 엔진 지식, 디자인 패턴, 그래픽스, 물리, UI, 최적화 | `[UnityEngine]` `[Design Pattern]` `[Graphics]` `[Physics]` `[UI]` `[Optimization]` |
| `Code_snippets/` | 재활용 코드 (`.cs` 원본 + `[Code]` 설명 문서) | `[Code]` |
| `Debug_log/` | 트러블슈팅 기록, 버그 해결 과정 | `[Debug]` |
| `Projects/` | 프로젝트 단위 기록 (설계, 아키텍처, 결정 이유, 진행 과정) | `[Project]` |

왼쪽 탐색기에서 폴더별 문서 목록을 볼 수 있고, 상단 검색으로 본문 검색이 됩니다.

# 2. 시작점

---

### 1) CS_knowledge
- [[[CS] Deadlock]] — 교착 상태의 조건과 해결 전략
- [[[CSharp] Struct Dictionary Boxing]] — struct 키 박싱 제거
- [[[Git] Git Flow]] — 브랜치 전략
- [[[Web] npm]] · [[[Web] Static Site Generator]] — 이 위키 사이트를 만드는 도구

### 2) Unity
- [[[UnityEngine] Unity Lifecycle]] — MonoBehaviour 이벤트 함수 순서
- [[[Design Pattern] 싱글톤 패턴]] · [[[Design Pattern] 오브젝트 풀링 패턴]]

### 3) Code_snippets
- [[[Code] AStar]] — Tilemap 기반 A* 경로 탐색
- [[[Code] EventChannel]] — ScriptableObject 이벤트 채널

### 4) Projects
- [[[Project] LLM Wiki Site]] — 이 위키 사이트 구축 프로젝트 (허브)

# 3. 작성 규칙

---

- 파일명은 `[태그] PascalCase.md`, 문서 첫 줄은 국문 제목(H1)
- 문서 상단에 `Date` / `Tags` / `Related`를 적고, 관련 문서는 `[[링크]]`로 상호 연결
- 자세한 규칙은 저장소의 `AGENTS.md`, 양식은 `TEMPLATE.md` 참고
