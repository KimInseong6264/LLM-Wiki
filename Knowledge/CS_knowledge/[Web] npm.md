# [Web] npm — JavaScript 패키지 관리자

- **Date**: 2026-08-30
- **Tags**: #Web #npm #NodeJS #PackageManager #DependencyManagement
- **Related**: [[[Web] Static Site Generator]], [[[Git] Git Flow]]

---

# TL;DR

---

- **npm**은 JavaScript 세계의 패키지 관리자다. Unity Package Manager, C#의 NuGet과 같은 역할로, 남이 만든 라이브러리를 이름과 버전으로 내려받는다.
- 프로젝트는 `package.json`(필요한 부품 목록) + `package-lock.json`(정확한 버전 명세)만 git에 올리고, 실제 부품(`node_modules/`)은 각자 `npm ci`로 받는다.
- **`npm ci`** 는 lock 파일에 적힌 버전 그대로 깨끗하게 설치하고, **`npm install`** 은 필요하면 버전을 새로 계산해 lock 파일을 고쳐 쓴다. 남의 프로젝트를 받아 그대로 돌리거나 CI 서버에서 빌드할 때는 `npm ci`를 쓴다.

# 1. npm이란

---

### 1) 정의
- **npm**(Node Package Manager)은 **Node.js**(브라우저 밖에서 JavaScript를 실행하는 런타임)에 기본으로 딸려오는 패키지 관리자다.
- 전 세계 개발자가 올려둔 라이브러리(**패키지**)를 공개 저장소(npm registry)에서 이름으로 찾아 내려받고, 프로젝트 안에서 쓸 수 있게 해준다.

### 2) 다른 생태계와의 비교

| 생태계 | 패키지 관리자 | 부품 목록 파일 | 실제 부품 저장 위치 |
|---|---|---|---|
| JavaScript / Node.js | **npm** (또는 pnpm, yarn) | `package.json` | `node_modules/` |
| C# / .NET | NuGet | `.csproj` | `packages/` 또는 전역 캐시 |
| Unity | Package Manager | `Packages/manifest.json` | `Library/PackageCache/` |
| Python | pip | `requirements.txt` | `site-packages/` |

- 어느 생태계든 구조는 같다. **"목록만 저장소에 올리고, 실제 부품은 각자 받는다."** 부품 폴더는 수백 MB가 넘고 언제든 다시 받을 수 있으므로 git에 올리지 않는다.

# 2. 세 가지 파일의 관계

---

### 1) 구조

```
프로젝트/
  package.json        ← 설계도의 "부품 목록" (사람이 편집)
  package-lock.json   ← 부품 "상세 명세" (npm이 자동 생성, 사람이 편집하지 않음)
  node_modules/       ← 실제 부품 (npm ci / install이 생성, git에 올리지 않음)
```

### 2) 각 파일의 역할

| 파일 | 내용 | git 커밋 여부 |
|---|---|---|
| `package.json` | 프로젝트 이름, 실행 스크립트, 필요한 패키지와 **느슨한 버전 범위** (예: `"d3": "^7.9.0"` = 7.9.0 이상 8.0.0 미만) | O |
| `package-lock.json` | 실제로 설치된 **정확한 버전**을 전부 기록. 패키지가 의존하는 패키지(의존성의 의존성)까지 포함하므로 수백 개가 됨 | O |
| `node_modules/` | 내려받은 패키지 본체. 폴더 하나가 패키지 하나 | X (`.gitignore`에 추가) |

### 3) 왜 lock 파일이 따로 필요한가
- `package.json`의 `^7.9.0` 같은 범위 표기는 "이 범위 안이면 아무거나 괜찮다"는 뜻이다. 시간이 지나 7.9.5가 나오면 새로 설치하는 사람은 7.9.5를 받게 되고, 같은 프로젝트인데 사람마다 버전이 달라질 수 있다.
- `package-lock.json`은 "마지막으로 설치했을 때 정확히 이 버전이었다"를 못 박아 둔다. 이 파일이 있으면 **누가 언제 설치해도 같은 결과**가 나온다.

### 4) 의존성의 의존성

```
package.json에 적힌 것: 62개
        │
        ▼
d3 ──┬── d3-array
     ├── d3-scale ──┬── d3-interpolate ── d3-color
     │              └── d3-time
     └── ... (d3 하나가 30여 개를 끌고 옴)
        │
        ▼
node_modules/ 에 실제로 생기는 것: 수백 개
```

- 직접 적은 패키지 하나가 내부적으로 다른 패키지들을 필요로 하고, 그것들이 또 다른 패키지를 필요로 한다. npm은 이 트리를 전부 풀어서 설치한다.

# 3. `npm ci` vs `npm install`

---

### 1) 동작 비교

| 항목 | `npm ci` | `npm install` |
|---|---|---|
| 이름 뜻 | **c**lean **i**nstall | install |
| 기존 `node_modules/` | **통째로 삭제** 후 새로 설치 | 있으면 재사용, 부족한 것만 추가 |
| 설치 기준 | `package-lock.json`의 **정확한 버전** | `package.json`의 범위 (lock 파일은 참고만) |
| `package-lock.json` 수정 | **절대 안 함** | 필요하면 고쳐 씀 |
| `package.json`과 lock이 불일치할 때 | **에러로 중단** | lock을 갱신해서 맞춤 |
| lock 파일이 없을 때 | 에러 | lock 파일을 새로 생성 |
| 속도 | 빠름 (버전 계산 생략) | 상대적으로 느림 |

### 2) 언제 무엇을 쓰나

| 상황 | 명령 | 이유 |
|---|---|---|
| 남의 프로젝트를 받아서 **그대로 돌리기만** 할 때 | `npm ci` | 원작자가 테스트한 버전 조합을 그대로 재현 |
| CI 서버(GitHub Actions 등)에서 자동 빌드할 때 | `npm ci` | 매번 빈 환경에서 시작하므로 재현성이 최우선 |
| 내 프로젝트에 **새 패키지를 추가**할 때 | `npm install 패키지명` | lock 파일에 새 패키지를 기록해야 함 |
| lock 파일이 아예 없을 때 | `npm install` | lock 파일을 만들어야 함 |

### 3) 흐름 도식

```
[git clone] ──> package.json + package-lock.json 만 손에 있음
                  (설계도와 부품 목록은 있지만 부품은 없음)
                            │
                            ▼
                       [npm ci]  ──> 인터넷(npm registry)에서
                            │        lock에 적힌 버전 그대로 내려받음
                            ▼
                     node_modules/ 생성 (실제 부품 도착)
                            │
                            ▼
                  [npx quartz build 등 실행 가능]
```

# 4. 자주 쓰는 명령

---

| 명령 | 역할 |
|---|---|
| `npm ci` | lock 파일 기준으로 깨끗하게 전체 설치 |
| `npm install` | 전체 설치 (lock 갱신 가능) |
| `npm install <이름>` | 패키지 추가 + `package.json`/lock에 기록 |
| `npm uninstall <이름>` | 패키지 제거 |
| `npm run <스크립트>` | `package.json`의 `"scripts"`에 정의된 명령 실행 |
| `npx <명령>` | `node_modules/` 안에 설치된 실행 파일을 바로 실행 (전역 설치 없이) |
| `npm outdated` | 최신 버전이 나온 패키지 목록 확인 |

- `npx`는 "이 프로젝트에 설치된 도구를 실행해라"는 뜻이다. 예를 들어 `npx quartz build`는 `node_modules/` 안의 Quartz를 실행한다.

# 5. 실전 사례 — 이 위키 사이트 구축

---

### 1) 로컬에서
1. `git clone --depth 1 --branch v4.5.2 https://github.com/jackyzha0/quartz.git` → Quartz 소스와 `package.json`, `package-lock.json`만 받아짐
2. `npm ci` → `node_modules/`에 부품 수백 개 설치 (1~2분)
3. `npx quartz build -d Knowledge` → 마크다운을 HTML로 변환 ([[[Web] Static Site Generator]] 참고)

### 2) GitHub Actions에서
- 배포 워크플로도 같은 순서를 서버에서 반복한다: `checkout → setup-node → npm ci → npx quartz build → 배포`
- GitHub 서버는 매번 빈 상태에서 시작하므로 `npm ci`가 필수이고, `npm install`이 아닌 `npm ci`를 써야 로컬에서 검증한 것과 같은 버전으로 빌드된다.

### 3) 주의점
- `node_modules/`는 반드시 `.gitignore`에 넣는다. 실수로 커밋하면 저장소가 수백 MB 늘어난다.
- Obsidian 볼트 안에 `node_modules/`를 만들지 않는다. Obsidian이 수백 개 파일을 인덱싱하려 들어 느려진다.
- Node.js 버전 요구사항을 확인한다. Quartz v4.5.2는 Node ≥ 22, npm ≥ 10.9.2를 요구한다.

# 6. 용어 정리

---

| 용어 | 뜻 |
|---|---|
| Node.js | 브라우저 밖에서 JavaScript를 실행하는 런타임. npm은 여기 딸려옴 |
| 패키지(package) | npm으로 배포되는 라이브러리 단위. `package.json`을 가진 폴더 |
| 의존성(dependency) | 내 프로젝트가 동작하려면 필요한 다른 패키지 |
| registry | 패키지가 올라가 있는 공개 서버 (기본값 npmjs.com) |
| semver | `주.부.패치` 형식의 버전 규칙. `^`는 주 버전 고정, `~`는 부 버전 고정 |
| lock 파일 | 설치된 정확한 버전을 기록한 파일. 재현성 보장용 |
