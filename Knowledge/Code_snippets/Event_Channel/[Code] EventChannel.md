# ScriptableObject 이벤트 채널 (우선순위 지원)

- **Date**: 2026-08-29
- **Tags**: #Code #EventChannel #ScriptableObject #Observer #Decoupling
- **Related**: [[[Design Pattern] 싱글톤 패턴]], [[[UnityEngine] Unity Lifecycle]]

> **TL;DR**
> `ScriptableObject` 에셋을 "이벤트 통로"로 두고, 발신자와 수신자가 서로를 모른 채 에셋만 참조해 통신하는 구조다.
> 리스너는 `priority`(작을수록 먼저)로 정렬돼 호출되며, `Void / Int / Float` 세 타입이 준비돼 있고 `EventChannelBase<T>`를 상속하면 새 타입을 한 줄로 추가할 수 있다.
> 싱글톤 매니저 없이 씬·프리팹 간 결합도를 낮추는 것이 목적이다. 에셋은 플레이 세션을 넘어 살아남으므로 **리스너 해제를 반드시 챙겨야** 한다.

---

# 1. 파일 구성

---

| 파일 | 역할 |
|---|---|
| `EventChannelBase.cs` | 비제네릭 추상 베이스(`ClearListeners()`) + 제네릭 `EventChannelBase<T>` 구현 |
| `VoidEventChannelSO.cs` | 인자 없는 이벤트. `EventChannelBase<T>`를 쓸 수 없어 별도 구현 |
| `IntEventChannelSO.cs` | `EventChannelBase<int>` 상속, 본문 없음 |
| `FloatEventChannelSO.cs` | `EventChannelBase<float>` 상속, 본문 없음 |

```
                 EventChannelBase (abstract SO)
                   │   ClearListeners()
        ┌──────────┴───────────────┐
 EventChannelBase<T>         VoidEventChannelSO
   AddListener(Action<T>, p)     AddListener(Action, p)
   RemoveListener(Action<T>)     RemoveListener(Action)
   RaiseEvent(T)                 RaiseEvent()
        │
   ┌────┴────┐
 IntSO     FloatSO
```

# 2. 사용법

---

### 1) 에셋 생성
- Project 창 우클릭 → `Create → EventChannel → Void / Int / Float`
- 이벤트 하나당 에셋 하나 (예: `OnPlayerDied.asset`, `OnScoreChanged.asset`)

### 2) 발신자

```csharp
using UnityEngine;

public class ScoreCounter : MonoBehaviour
{
    [Header("점수 변경 이벤트 채널")]
    [SerializeField] private IntEventChannelSO _onScoreChanged;

    private int _score;

    public void AddScore(int amount)
    {
        _score += amount;
        _onScoreChanged.RaiseEvent(_score);
    }
}
```

### 3) 수신자

```csharp
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [Header("점수 변경 이벤트 채널")]
    [SerializeField] private IntEventChannelSO _onScoreChanged;

    private void OnEnable() => _onScoreChanged.AddListener(UpdateText, priority: 10);
    private void OnDisable() => _onScoreChanged.RemoveListener(UpdateText);

    private void UpdateText(int score) { /* ... */ }
}
```

- 발신자와 수신자는 서로를 참조하지 않는다. 둘 다 같은 에셋만 인스펙터에서 연결
- `OnEnable` / `OnDisable` 쌍으로 등록·해제하는 것이 기본. 생명주기는 [[[UnityEngine] Unity Lifecycle]] 참고

### 4) 새 타입 추가

```csharp
[CreateAssetMenu(fileName = "EventChannel", menuName = "EventChannel/Vector2")]
public class Vector2EventChannelSO : EventChannelBase<Vector2> { }
```

# 3. 동작 원리

---

### 1) 우선순위 정렬
- 내부 저장소는 `SortedList<int, List<Action<T>>>`. 키가 `priority`이며 **오름차순**으로 순회하므로 숫자가 작을수록 먼저 호출된다
- 같은 우선순위끼리는 등록 순서대로 호출
- 기본값 `priority = 0`

```
RaiseEvent(v)
  └─ for priority in [-10, 0, 10, ...]   (SortedList 순서)
       └─ for listener in list[priority]
            └─ listener?.Invoke(v)
```

### 2) 왜 ScriptableObject인가
| 비교 | 싱글톤 매니저 + C# event | SO 이벤트 채널 |
|---|---|---|
| 결합도 | 모두가 매니저 타입을 직접 참조 | 에셋만 참조, 타입 의존 없음 |
| 씬 전환 | 매니저 오브젝트 생존 관리 필요 | 에셋이라 씬과 무관 |
| 테스트 | 매니저를 띄워야 함 | 에셋 하나로 격리 테스트 가능 |
| 디자이너 친화 | 코드 수정 필요 | 인스펙터에서 연결만 바꾸면 됨 |

싱글톤과의 자세한 비교는 [[[Design Pattern] 싱글톤 패턴]] 참고.

# 4. 주의점

---

| 항목 | 내용 |
|---|---|
| **리스너 누수** | SO는 에디터에서 플레이를 멈춰도 메모리에 남는다. `OnDisable`/`OnDestroy`에서 반드시 `RemoveListener`. 안전장치로 씬 로드 시 `ClearListeners()`를 호출하는 초기화 지점을 두는 것도 방법 |
| **호출 중 등록/해제 금지** | `RaiseEvent` 순회 도중 리스너 안에서 `AddListener`/`RemoveListener`를 부르면 `InvalidOperationException`(컬렉션 변경). 필요하면 순회 전에 리스트를 복사하거나 지연 큐로 처리하도록 개선 |
| 중복 등록 | 같은 델리게이트를 두 번 `AddListener`하면 두 번 호출된다. `Contains` 검사가 없음 |
| `RemoveListener` 비용 | 모든 우선순위 리스트를 선형 탐색. 리스너 수가 많지 않은 전제 |
| 빈 우선순위 키 | 리스너를 모두 제거해도 `SortedList`에 빈 리스트가 남는다. 동작엔 문제없지만 순회 대상이 늘어남 |
| `Void` 채널 중복 코드 | `EventChannelBase<T>`와 로직이 같지만 `Action`(무인자)이라 제네릭으로 묶지 못해 별도 구현. 수정 시 두 파일 모두 고쳐야 함 |

# 5. 확장 아이디어

---

- `RaiseEvent` 전에 리스트 스냅샷을 떠서 호출 중 해제를 허용
- 인스펙터용 디버그: 마지막 발생 값, 현재 리스너 수 표시(`#if UNITY_EDITOR`)
- 두 인자 이벤트: `EventChannelBase<T1, T2>` 추가
- `UnityEvent` 기반 리스너 컴포넌트(`EventListenerSO`)를 만들어 코드 없이 인스펙터에서 반응 연결
