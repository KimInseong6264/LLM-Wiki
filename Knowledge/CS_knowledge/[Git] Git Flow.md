# [Git] Git Flow 브랜치 전략

- **Date**: 2026-06-06
- **Tags**: #Git #GitFlow #BranchingStrategy

---

# 1. 개요

---

**Git Flow**는 2010년 빈센트 드리센(Vincent Driessen)의 블로그 글을 통해 널리 알려진 **Git 브랜치 관리 및 워크플로우 전략**입니다. 
소스코드 관리, 배포 주기 조율, 버그 수정 등 대규모 협업 과정에서 브랜치를 효율적으로 나누어 개발 프로세스를 고도로 구조화하고 병목 현상을 최소화하는 것을 목적으로 합니다.

# 2. 5가지 핵심 브랜치 구조

---

Git Flow는 역할을 기준으로 크게 **항시 유지되는 메인 브랜치(2개)**와 **필요할 때 만들고 삭제하는 보조 브랜치(3개)**로 나뉩니다.

### 1) 메인 브랜치 (Main Branches)
- **`main`**: 제품으로 배포 가능한 완성된 상태의 코드가 병합되는 브랜치입니다.
- **`develop`**: 다음 배포를 위해 새롭게 개발 중인 기능들이 모이는 핵심 통합 브랜치입니다.

### 2) 보조 브랜치 (Supporting Branches)
- **`feature/`**: 새로운 기능을 개발할 때 사용합니다. `develop`에서 갈라져 나와 다시 `develop`으로 병합됩니다.
- **`release/`**: 배포 전 버그 수정 및 최종 점검(QA)을 수행합니다. `develop`에서 생성되어, 완료 후 `main`과 `develop`으로 병합됩니다.
- **`hotfix/`**: 실서비스(`main`)의 치명적인 버그 수정 시 사용합니다. `main`에서 파생되어 해결 후 `main`과 `develop` 모두에 병합됩니다.

# 3. 워크플로우 시각화

---

```mermaid
gitGraph
    commit id: "Initial Commit"
    branch develop
    checkout develop
    commit id: "Setup project"
    branch feature/login
    checkout feature/login
    commit id: "Add login UI"
    checkout develop
    merge feature/login
    branch release/v1.0.0
    checkout release/v1.0.0
    checkout main
    merge release/v1.0.0 tag: "v1.0.0"
    checkout develop
    merge release/v1.0.0
    checkout main
    branch hotfix/v1.0.1
    checkout hotfix/v1.0.1
    checkout main
    merge hotfix/v1.0.1 tag: "v1.0.1"
    checkout develop
    merge hotfix/v1.0.1
```

# 4. 실무 권장 수칙

---

### 1) 워크플로우 팁
- **Pull Request 기반**: 모든 병합은 PR/MR을 거쳐 코드 리뷰를 수행합니다.
- **이름 규칙**: `feature/`, `release/`, `hotfix/` 접두사를 명확히 사용합니다.
- **잦은 동기화**: `develop`의 최신 내용을 지속적으로 받아와 충돌을 방지합니다.

### 2) 전략 비교
| 비교 항목 | Git Flow | GitHub Flow |
|:---|:---|:---|
| **복잡성** | 복잡함 | 단순함 |
| **적합성** | 정기적 릴리즈가 있는 앱 | 수시로 배포되는 웹 서비스 |

---
**출처**: Vincent Driessen — "A successful Git branching model" (2010) · Atlassian Git Tutorials
