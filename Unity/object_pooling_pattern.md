# Object Pooling Pattern

- **Date**: 2026-06-04
- **Tags**: #Unity #DesignPattern #Optimization

## 1. 개요 (Overview)
**오브젝트 풀링(Object Pooling)**은 객체를 매번 생성(`Instantiate`)하고 파괴(`Destroy`)하는 대신, 미리 생성해둔 객체들을 재사용하는 디자인 패턴입니다. 빈번한 가비지 컬렉션(GC) 발생을 막아 성능을 최적화하는 데 필수적입니다.

## 2. 왜 사용하는가? (Benefits)
- **메모리 단편화 방지**: 런타임에 메모리를 할당하고 해제하는 과정을 줄여 메모리 관리를 효율적으로 합니다.
- **GC 부하 감소**: Unity의 가비지 컬렉터는 CPU 성능에 영향을 줄 수 있는데, 풀링을 통해 객체 파괴를 피하면 GC 호출 횟수를 획기적으로 줄일 수 있습니다.
- **프레임 드랍(Stuttering) 방지**: 대량의 총알, 이펙트 등을 한꺼번에 생성할 때 발생하는 부하를 방지합니다.

## 3. Unity의 `UnityEngine.Pool` (2021.1+)
Unity 2021.1 버전부터 내장된 오브젝트 풀링 API를 제공합니다. `IObjectPool<T>` 인터페이스를 사용하여 직접 구현하는 수고를 덜 수 있습니다.

### 주요 인터페이스 및 클래스
- `ObjectPool<T>`: 기본적인 오브젝트 풀 클래스
- `LinkedPool<T>`: 연결 리스트 기반의 풀 (메모리 사용량이 적음)

## 4. 코드 예시 (Basic Implementation)

```csharp
using UnityEngine;
using UnityEngine.Pool;

public class BulletLauncher : MonoBehaviour
{
    public GameObject bulletPrefab;
    private IObjectPool<GameObject> _pool;

    void Awake()
    {
        _pool = new ObjectPool<GameObject>(
            createFunc: CreateBullet,        // 객체 생성 시 호출
            actionOnGet: OnGetBullet,        // 풀에서 꺼낼 때 호출
            actionOnRelease: OnReleaseBullet, // 풀에 반납할 때 호출
            actionOnDestroy: OnDestroyBullet, // 풀이 꽉 찼을 때 객체 파괴 시 호출
            collectionCheck: true,           // 이미 반납된 객체를 다시 반납하는지 체크
            defaultCapacity: 10,             // 기본 용량
            maxSize: 20                      // 최대 용량
        );
    }

    private GameObject CreateBullet() => Instantiate(bulletPrefab);
    private void OnGetBullet(GameObject bullet) => bullet.SetActive(true);
    private void OnReleaseBullet(GameObject bullet) => bullet.SetActive(false);
    private void OnDestroyBullet(GameObject bullet) => Destroy(bullet);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var bullet = _pool.Get();
            // ... 총알 위치 설정 등
        }
    }
}
```

## 5. 핵심 팁
- **Pre-warming**: 게임 시작 시나 로딩 중에 미리 필요한 만큼의 객체를 풀링해두면 게임 도중 끊김 현상을 더 확실히 방지할 수 있습니다.
- **Single vs Multi Pool**: 싱글톤으로 관리되는 범용 풀 매니저를 만들거나, 각 오브젝트 타입별로 개별 풀을 관리할 수 있습니다.

---
**출처**: Unity Documentation & General Design Patterns