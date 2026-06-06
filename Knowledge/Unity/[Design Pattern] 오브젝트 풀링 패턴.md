# [Design Pattern] 오브젝트 풀링 패턴

- **Date**: 2026-06-06
- **Tags**: #Unity #DesignPattern #ObjectPooling

---

# 1. 개요

---

**오브젝트 풀링(Object Pooling)**은 객체를 실시간으로 계속 생성(`Instantiate`)하고 파기(`Destroy`)하는 대신, 필요한 만큼 미리 생성하여 관리 풀(Pool)에 보관해 두고 **비활성화/활성화** 방식으로 반복 재사용하는 성능 최적화 패턴입니다. 

특히 런타임 중에 총알, 적 캐릭터, 폭발 이펙트 같이 아주 짧은 수명을 지닌 객체가 수백 개씩 생겨나고 사라지는 게임 구조에서 가비지 컬렉션(GC) 부하와 프레임 드랍(Stuttering)을 방지하는 필수 패턴입니다.

# 2. 장점 및 단점

---

### 1) 장점
- **GC 부하 감소**: 동적 생성/삭제로 인한 가비지 발생을 막아 GC 호출 횟수를 줄입니다.
- **메모리 단편화 방지**: 메모리 할당/해제를 줄여 메모리 레이아웃을 균일하게 보존합니다.
- **CPU 오버헤드 완화**: `Instantiate`와 `Destroy`의 무거운 연산 대신 `SetActive`만 사용하여 성능을 확보합니다.

### 2) 단점 및 주의점
- **초기 메모리 사용량 상승**: 시작 시에 대량의 객체를 미리 힙 메모리에 적재해야 합니다.
- **상태 초기화 오버헤드**: 객체를 풀에서 꺼낼 때 이전 생명주기의 쓰레기 값이 남지 않도록 완벽히 초기화해야 합니다.

# 3. Unity 내장 `UnityEngine.Pool`

---

### 1) 주요 API 매개변수 구성
| 콜백 / 옵션 | 자료형 | 설명 |
|:---|:---|:---|
| **createFunc** | `Func<T>` | 객체가 부족할 때 생성 |
| **actionOnGet** | `Action<T>` | 풀에서 꺼낼 때 (`SetActive(true)`) |
| **actionOnRelease** | `Action<T>` | 풀에 반납할 때 (`SetActive(false)`) |
| **maxSize** | `int` | 풀의 최대 용량 |

# 4. 실무 구현 패턴

---

### 1) 총알 컴포넌트 (`PooledBullet.cs`)
```csharp
public class PooledBullet : MonoBehaviour
{
    private IObjectPool<PooledBullet> _myPool;
    public void SetManagedPool(IObjectPool<PooledBullet> pool) => _myPool = pool;
    public void ReleaseSelf() => _myPool?.Release(this);
}
```

### 2) 발사기 스크립트 (`BulletLauncher.cs`)
```csharp
public class BulletLauncher : MonoBehaviour
{
    private IObjectPool<PooledBullet> _pool;
    void Awake()
    {
        _pool = new ObjectPool<PooledBullet>(CreateBullet, OnGet, OnRelease);
    }
    // ... 구현 상세
}
```

# 5. 실무 최적화 팁

---

### 1) 필수 사항
- **Pre-warming**: 게임 로딩이나 초기화 페이즈에 미리 객체를 생성해 두어 게임 중 끊김을 방지합니다.
- **풀링 대상 선별**: 수명이 짧고, 발생 빈도가 잦으며, 동시다발적인 엔티티(총알, 이펙트 등)를 대상으로 합니다.
- **Hierarchy 관리**: 풀러 스크립트 하위에 부모(`Transform parent`)를 두어 관리하면 정리된 환경을 유지할 수 있습니다.

---
**출처**: Unity Scripting API - UnityEngine.Pool · Unity Learn Optimization Course
