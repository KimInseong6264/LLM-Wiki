# 오브젝트 풀링 패턴 (Object Pooling Pattern)

- **Date**: 2026-06-06
- **Tags**: #Unity #DesignPattern #Optimization

---

## 1. 개요 (Overview)

**오브젝트 풀링(Object Pooling)**은 객체를 실시간으로 계속 생성(`Instantiate`)하고 파기(`Destroy`)하는 대신, 필요한 만큼 미리 생성하여 관리 풀(Pool)에 보관해 두고 **비활성화/활성화** 방식으로 반복 재사용하는 성능 최적화 패턴입니다. 

특히 런타임 중에 총알, 적 캐릭터, 폭발 이펙트 같이 아주 짧은 수명을 지닌 객체가 수백 개씩 생겨나고 사라지는 게임 구조에서 가비지 컬렉션(GC) 부하와 프레임 드랍(Stuttering)을 방지하는 필수 패턴입니다.

---

## 2. 왜 사용하는가? (Benefits & Trade-offs)

### 장점
- **GC(Garbage Collector) 부하 감소**: Unity/C# 환경에서 동적 생성/삭제는 대량의 가비지(Garbage)를 발생시켜 불규칙한 프레임 드랍(Stuttering)을 유발합니다. 풀링은 이를 예방합니다.
- **메모리 단편화(Memory Fragmentation) 방지**: 힙 메모리를 불규칙하게 할당/해제하는 것을 막고 메모리 레이아웃을 균일하게 보존합니다.
- **CPU 오버헤드 완화**: `Instantiate`와 `Destroy`는 엔진 내부적으로 하이어라키 등록, 컴포넌트 초기화 등 매우 무거운 연산입니다. 이미 생성된 상태에서 `SetActive`만 켜고 끄는 것이 훨씬 가볍습니다.

### 단점 및 주의점
- **초기 메모리 사용량 상승**: 시작 시에 대량의 객체를 미리 힙 메모리에 적재해 두어야 합니다.
- **상태 초기화 오버헤드**: 객체를 풀에서 꺼낼 때 체력, 속도, 위치, 이벤트 바인딩 등의 상태를 **반드시 완벽히 초기화**해 주어야 합니다. 그렇지 않으면 이전 생명주기의 쓰레기 값이 그대로 유지되는 논리 버그가 발생할 수 있습니다.

---

## 3. Unity 내장 `UnityEngine.Pool` (Unity 2021.1+)

과거에는 수동으로 List나 Queue를 활용해 풀러를 구현했으나, Unity 2021.1부터는 매우 강력하고 최적화된 내장 제네릭 풀 인터페이스인 `IObjectPool<T>` 및 구현체 `ObjectPool<T>`을 제공합니다.

### 주요 API 매개변수 구성
`ObjectPool<T>` 생성자는 아래의 콜백 함수들을 받아서 동작합니다.

| 콜백 / 옵션 | 자료형 / 형태 | 설명 |
|:---|:---|:---|
| **`createFunc`** | `Func<T>` | 풀 내에 재사용할 객체가 부족할 때 새로 객체를 생성하는 메서드 |
| **`actionOnGet`** | `Action<T>` | 풀에서 객체를 꺼내와 사용자에게 주기 직전 호출되는 메서드 (예: `SetActive(true)`) |
| **`actionOnRelease`** | `Action<T>` | 사용이 끝난 객체를 풀에 반환할 때 호출되는 메서드 (예: `SetActive(false)`) |
| **`actionOnDestroy`** | `Action<T>` | 풀이 설정된 `maxSize`를 초과하여 넘치는 객체를 영구 파괴할 때 호출되는 메서드 |
| **`collectionCheck`** | `bool` | 이미 풀에 들어있는 객체를 중복으로 반환하려 할 때 에러를 뿜을지 여부 (디버깅용, 배포 시엔 성능을 위해 끔) |
| **`defaultCapacity`** | `int` | 처음 확보해 둘 풀 컬렉션 메모리 내부 예약 용량 |
| **`maxSize`** | `int` | 풀이 유지할 수 있는 최대 재사용 객체 개수 (초과분은 힙에 머물지 않고 즉시 파괴) |

---

## 4. 완벽한 실무 구현 패턴 (Solid Code Example)

아래 예시는 **총알 발사기(Launcher)**가 Unity 내장 `ObjectPool`을 통해 총알을 통제하고, 각 **총알(PooledBullet)** 스스로가 지정된 조건(화면을 벗어나거나 지속 시간 초과 등)을 만족하면 자신을 소유한 풀로 **스스로를 안전하게 반납**하는 정석적인 구조입니다.

### 4-1. 풀러를 소유할 총알 컴포넌트 (`PooledBullet.cs`)
```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class PooledBullet : MonoBehaviour
{
    private IObjectPool<PooledBullet> _myPool;
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifeTime = 2f;
    private Coroutine _deactivateCoroutine;

    // 발사기가 자신을 생성할 때 풀에 대한 참조를 할당해 줍니다.
    public void SetManagedPool(IObjectPool<PooledBullet> pool)
    {
        _myPool = pool;
    }

    // 풀에서 Get() 되어 씬에 활성화될 때마다 초기화 로직을 수행합니다.
    public void Launch()
    {
        if (_deactivateCoroutine != null)
        {
            StopCoroutine(_deactivateCoroutine);
        }
        _deactivateCoroutine = StartCoroutine(DeactivateAfterDelay());
    }

    void Update()
    {
        // 전방으로 계속 이동
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(lifeTime);
        ReleaseSelf();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 무언가 부딪혔을 때도 반납
        if (other.CompareTag("Enemy"))
        {
            ReleaseSelf();
        }
    }

    private void ReleaseSelf()
    {
        if (_myPool != null)
        {
            _myPool.Release(this);
        }
        else
        {
            Destroy(gameObject); // 예외 처리용
        }
    }
}
```

### 4-2. 풀을 생성하고 관리하는 발사기 스크립트 (`BulletLauncher.cs`)
```csharp
using UnityEngine;
using UnityEngine.Pool;

public class BulletLauncher : MonoBehaviour
{
    [SerializeField] private PooledBullet bulletPrefab;
    [SerializeField] private Transform firePoint;
    
    private IObjectPool<PooledBullet> _pool;

    void Awake()
    {
        _pool = new ObjectPool<PooledBullet>(
            createFunc: CreateBullet,
            actionOnGet: OnGetBullet,
            actionOnRelease: OnReleaseBullet,
            actionOnDestroy: OnDestroyBullet,
            collectionCheck: true,
            defaultCapacity: 20,
            maxSize: 50
        );

        // Pre-warming (선택 사항: 시작 시 지정 개수를 미리 만들어 둠)
        Prewarm(15);
    }

    private PooledBullet CreateBullet()
    {
        PooledBullet bullet = Instantiate(bulletPrefab);
        bullet.SetManagedPool(_pool); // 풀 참조 넘겨주기
        return bullet;
    }

    private void OnGetBullet(PooledBullet bullet)
    {
        bullet.gameObject.SetActive(true);
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        bullet.Launch(); // 발사 초기화 시동
    }

    private void OnReleaseBullet(PooledBullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    private void OnDestroyBullet(PooledBullet bullet)
    {
        Destroy(bullet.gameObject);
    }

    private void Prewarm(int count)
    {
        // 가비지가 없는 깔끔한 방식의 프리웜 구현
        PooledBullet[] tempArray = new PooledBullet[count];
        for (int i = 0; i < count; i++)
        {
            tempArray[i] = _pool.Get();
        }
        for (int i = 0; i < count; i++)
        {
            _pool.Release(tempArray[i]);
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            // 필요할 때 풀에서 꺼내씀 (풀이 비어있으면 내부에서 알아서 CreateBullet 호출)
            _pool.Get();
        }
    }
}
```

---

## 5. 핵심 최적화 실무 팁 (Best Practices)

1. **Pre-warming (선택 아닌 필수)**: 
   - 오브젝트 풀링을 쓰는 가장 큰 목적은 프레임 드랍을 원천 차단하는 것입니다. 게임 실행 도중이나 급박한 씬 안에서 대량 생성 연산이 돌면 소용없습니다.
   - 따라서 **씬 로딩화면 연출** 도중이나 게임 준비(`Initialize`) 페이즈 단계에서 `Prewarm()`을 수행하여 목표 개수를 완전히 확보해 두어야 프레임 드랍을 0에 가깝게 통제할 수 있습니다.
2. **풀링 대상 선별**:
   - 게임 전체에서 한 두 번만 생성되는 큰 단위의 매니저나 플레이어 캐릭터는 굳이 풀링하지 않습니다.
   - **수명이 짧고, 발생 빈도가 매우 잦으며, 동시다발적으로 나타나는 엔티티**(총알, 먼지 이펙트, 스파크, 잔해 파편, 데미지 텍스트 UI)가 완벽한 풀링 대상입니다.
3. **Hierarchy 관리**:
   - 많은 오브젝트가 동적으로 활성화/비활성화되면 에디터의 Hierarchy 뷰가 몹시 지저분해져 성능 분석 및 개발 생산성이 하락합니다.
   - 풀링 매니저 스크립트 산하에 빈 GameObject 부모(`Transform parent`)를 생성해 두고, `Create` 시점에 자식(Child)으로 등록되도록 묶어주는 편이 보기에도 좋고 깔끔합니다.

---
- **이전 글**: [[Design Pattern] 싱글톤 패턴](./%5BDesign%20Pattern%5D%20%EC%8B%B1%EA%B8%80%ED%86%A4%20%ED%8C%A8%ED%84%B4.md)
---
**출처**: Unity Scripting API - UnityEngine.Pool (IObjectPool) · Unity Learn Optimization Course
