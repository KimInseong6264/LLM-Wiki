# LLM Wiki - AGENTS.md

## 이 Wiki는 무엇인가?
Unity 게임 개발자의 개인 지식 베이스입니다.
공부한 내용, 개발 노하우, 재활용 코드를 마크다운으로 축적합니다.

## 폴더 구조
- `Knowledge/` : 지식 베이스 루트
  - `Unity/` : Unity 엔진 관련 노하우, 디자인 패턴, 팁
  - `CS_knowledge/` : 자료구조, 알고리즘, CS 이론, C# 언어 지식
  - `Code_snippets/` : 재활용 가능한 코드 모음 (C# 위주)
  - `Debug_log/` : 트러블슈팅 기록, 버그 해결 과정

## 파일 이름 규칙
파일명은 반드시 아래 형식을 따릅니다:
`[태그] 파일명.md`

### 태그 목록
**CS_knowledge/**
- `[C#]` : C# 언어 관련 (예: [C#] interface.md)
- `[Network]` : 네트워크 관련 (예: [Network] OSI 7계층.md)
- `[Algorithm]` : 알고리즘 관련
- `[DataStructure]` : 자료구조 관련
- `[OS]` : 운영체제 관련
- `[Git]` : Git 관련 (예: [Git] Git Flow.md)

**Unity/**
- `[Design Pattern]` : 디자인 패턴 (예: [Design Pattern] 상태 패턴.md)
- `[Graphics]` : 셰이더, 렌더링 관련
- `[Physics]` : 물리 엔진 관련
- `[UI]` : UI/UX 관련
- `[Optimization]` : 최적화 관련

**Code_snippets/**
- `[Code]` : 모든 재활용 코드 (예: [Code] ObjectPool.md)

**Debug_log/**
- `[Debug]` : 모든 트러블슈팅 기록 (예: [Debug] NullReference.md)

### 새로운 태그 필요시
- 기존 태그에 맞지 않는 내용은 `[Unknown]` 태그로 파일 생성
- 반드시 유저에게 새로운 태그가 필요함을 알리고 태그 규칙 추가를 요청할 것
- 예: `[Unknown] 파일명.md` 생성 후 "새로운 태그 규칙이 필요합니다: [제안 태그명]" 알림

## Obsidian 태그 규칙
- 파일 상단에 파일명 태그와 키워드 태그를 함께 명시
- 파일명 태그: `#[폴더태그]` (예: #Git, #CSharp, #Unity)
- 키워드 태그: 내용의 핵심 개념들을 검색 및 분류에 유용한 주요 주제 위주로 3~5개 내외로 명시 (예: #GitFlow, #BranchingStrategy)
- `#Unknown` 태그는 Obsidian 태그로 사용 금지

## 문서 작성 양식 및 규칙
문서 작성 시 반드시 `TEMPLATE.md`를 읽어 양식과 규칙을 준수할 것