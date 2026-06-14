# 코딩 스타일 규칙 (C# / Unity)

프로젝트에 관계없이 공통으로 적용하는 C# · Unity 코딩 컨벤션 및 본 프로젝트(`Event_Channel`, `Monster`, `System`, `DropItems` 폴더 등)에서 특화되어 사용하는 규칙을 정의합니다.

---

## 1. 네이밍

| 대상 | 규칙 | 예시 |
|------|------|------|
| 클래스 / 인터페이스 | PascalCase | `PlayerController`, `IDamageable` |
| 인터페이스 접두사 | 항상 `I` | `IPickable`, `IInteractable` |
| private 필드 | `_camelCase` | `_health`, `_moveSpeed` |
| public 프로퍼티 | PascalCase | `CurrentHealth`, `IsAlive` |
| 메서드 | PascalCase | `TakeDamage()`, `Move()` |
| 지역 변수 / 매개변수 | camelCase | `damageAmount`, `targetPos` |
| 상수 | ALL_CAPS | `MAX_HEALTH`, `DEFAULT_SPEED` |
| 열거형 멤버 | 항상 SCREAMING_SNAKE_CASE | `DROP_ITEM_TYPE`, `STATE_IDLE` |
| bool 필드 · 프로퍼티 | `Is` / `Has` / `Can` 접두사 | `IsAlive`, `HasWeapon`, `CanMove` |
| 이벤트 · 콜백 | `On` 접두사 | `OnHealthChanged`, `OnDeath` |
| ScriptableObject 이벤트 채널 | `SO` 접미사 | `DropRequestEventChannelSO` |

---

## 2. 주석

- **언어**: 인라인 · 블록 · XML doc 모두 **한국어만** 사용 (영어 주석 금지)
- **public API**: `public` 메서드 · 프로퍼티에는 `/// <summary>` 작성, 반드시 3줄 형식으로 작성
- **비활성 코드**: 주석 처리만 하고 별도 설명 없이 보존

```csharp
// 인라인 — 데미지 적용 메서드

// 블록 — 유효하지 않은 대상은 처리하지 않음

/// <summary>
/// 현재 체력
/// </summary>
public float CurrentHealth => _health;

/// <summary>
/// 피해를 적용한다
/// </summary>
public void TakeDamage(int amount) { ... }
```

---

## 3. 코드 구조

- Inspector 노출이 필요하면 `public` 필드 대신 **`[SerializeField] private`** 사용
- public 멤버 노출이 필요하면 **필드 대신 프로퍼티** 사용
- 복잡한 로직은 **메서드로 분리**해 한 메서드가 한 가지 일만 하도록 유지
- 본문이 한 줄로 끝나는 메서드는 **람다식(`=>`)**으로 표현

### 파일 · 클래스

- 파일명 = `public` 클래스명 (예: `PlayerController.cs`)
- 파일당 클래스 **하나**

### 멤버 작성 순서

클래스 안에서는 **위에서 아래**로 아래 순서를 따른다.

| 순서 | 구분 | 내용 |
|------|------|------|
| 1 | 필드 · 프로퍼티 | 클래스 **최상단** (하위 순서 참고) |
| 2 | Unity 생명주기 | `Awake`, `OnEnable`, `Start`, `Update`, `OnDisable` 등 |
| 3 | public API | 외부에서 호출하는 `public` 메서드 |
| 4 | 내부 메서드 | `#region 내부 메서드`에 `private` 메서드 묶기 |

**필드 · 프로퍼티** 구역 내부 순서:

1. `[SerializeField] private` 필드
2. 일반 `private` 필드
3. 프로퍼티 (`public` / `private` 포함)

- 클래스 내부에서만 쓰는 `private` 메서드는 **항상 맨 아래** `#region 내부 메서드`에 모아 **외부 API와 내부 구현을 한눈에 구분** (단, 내부 메서드가 없는 경우에는 해당 #region을 작성하지 않는다)

```csharp
[SerializeField] private float _maxHealth = 100f;
[SerializeField] private Transform _hitPoint;

private float _health;
private bool _isInvincible;

public float CurrentHealth => _health;
public bool IsAlive => _health > 0;

private void Awake()
{
    _health = _maxHealth;
}

/// <summary>
/// 피해를 적용한다
/// </summary>
public void TakeDamage(int amount)
{
    if (!CanTakeDamage())
        return;

    ApplyDamage(amount);
}

#region 내부 메서드

private bool CanTakeDamage() => _health > 0 && !_isInvincible;

private void ApplyDamage(int amount)
{
    _health -= amount;
}

#endregion
```

---

## 4. 포맷 · 스타일

- 조건 불충족 시 **early return**으로 중첩 줄이기
- early return은 중괄호 생략, if 조건문 다음 줄에 `return` 작성, return 이후 빈 줄 하나 추가 후 코드 이어서 작성
- 매직 넘버·문자열은 **상수 또는 명명된 변수**로 분리

```csharp
if (_health <= 0)
    return;

if (!IsValidTarget(target))
    return;

// 이후 로직
```

---

## 5. Unity 관례

| 항목 | 규칙 |
|------|------|
| 같은 오브젝트 컴포넌트 조회 | `Awake`에서 `GetComponent` 후 **캐시** · `Update` 등에서 반복 호출 금지 |
| 태그 비교 | `CompareTag()` 사용 (`tag == "..."` 지양) |
| 오브젝트 검색 | `Find` / `GameObject.Find`는 **초기화 시 1회만** 호출 후 캐시 |
| 로그 | 정보 `Debug.Log` · 경고 `LogWarning` · 오류 `LogError` |
| null 참조 방지 | 사용 전 참조 검사, `[SerializeField]` 필드는 Inspector 할당 확인 |
| Inspector 노출 | `[SerializeField]` 필드 사용 시 위에 `[Header("한글 필드명")]` 명시 |
| 에디터 편의성 메서드 | 인스펙터 우클릭 메뉴로 노출할 디버그용 메서드는 `[ContextMenu("메뉴명")]`을 명시 |
| 에디터 전용 드로잉 | 기즈모 드로잉 등 에디터 전용 로직은 `#if UNITY_EDITOR` 및 `#endif` 전처리기로 감싸 빌드 제외 처리 |
| 이벤트 채널 설계 | ScriptableObject 기반 이벤트를 사용하여 클래스 간의 느슨한 결합(Decoupling)을 권장 |


---

변경된 부분 요약:

1. **주석** — `<summary>` 전부 3줄 형식으로, 규칙 문구에 "반드시 3줄 형식" 명시
2. **early return** — 섹션 4에 규칙 명문화, 예시 코드 수정, `TakeDamage` 예시도 반영
3. **enum** — 표에서 "항상 SCREAMING_SNAKE_CASE"로 통일
4. **람다식** — 섹션 3에 규칙 한 줄 추가 (예시는 기존 코드에 이미 있었음)