# A* 경로 탐색 (Tilemap 기반)

- **Date**: 2026-08-29
- **Tags**: #Code #AStar #Pathfinding #Tilemap #ObjectPooling
- **Related**: [[[Design Pattern] 오브젝트 풀링 패턴]]

> **TL;DR**
> Unity `Tilemap`의 벽 타일을 장애물로 보고 4방향 격자에서 최단 경로를 찾는 A* 구현이다.
> `AStar`(탐색) · `NodeManager`(노드 생성/풀링/이웃 판정) · `Node`(데이터) 세 파일로 구성되며, 매 탐색마다 노드를 새로 만들지 않고 풀에서 재사용해 GC 부담을 줄인다.
> 반환값은 시작 칸을 제외한 **셀 중심 월드 좌표 리스트**이고, 실패하거나 시작 = 목표면 `null`이다.

---

# 1. 파일 구성

---

| 파일 | 역할 | 핵심 멤버 |
|---|---|---|
| `AStar.cs` | 탐색 루프, 경로 복원 | `FindPath()`, `TryUpdateNeighbor()`, `BuildPath()`, `Heuristic()` |
| `NodeManager.cs` | 노드 캐시·풀링, 이동 가능 판정, 이웃 좌표 | `GetNode()`, `GetNeighbors()`, `IsWalkable()`, `GetMoveCost()`, `Init()` |
| `Node.cs` | 노드 데이터 (`G`, `H`, `F`, `Parent`) | `Reset()` |

```
AStar ──uses──> NodeManager ──creates/pools──> Node
  │                 │
  │                 └── Tilemap.HasTile()  (벽 판정)
  └── PriorityQueue<Node,int>  (open list, F 기준)
```

# 2. 사용법

---

### 1) 준비물
- 벽(이동 불가) 타일만 들어 있는 `Tilemap` 하나. 타일이 있는 칸 = 벽, 없는 칸 = 통행 가능
- 세 `.cs` 파일을 프로젝트에 복사

### 2) 호출 예시

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyMover : MonoBehaviour
{
    [Header("벽 타일맵")]
    [SerializeField] private Tilemap _wallTilemap;

    [Header("목표 지점")]
    [SerializeField] private Transform _target;

    private AStar _aStar;
    private List<Vector2> _path;

    private void Awake() => _aStar = new AStar(_wallTilemap);

    private void RequestPath()
    {
        _path = _aStar.FindPath(transform.position, _target.position);

        if (_path == null)
            return; // 도달 불가 또는 이미 같은 칸

        // _path[0]부터 순서대로 이동
    }
}
```

- `AStar` 인스턴스는 **한 번 만들어 재사용**한다. 생성자에서 `NodeManager`와 컬렉션을 만들고, 매 `FindPath()`마다 `Init()`으로 비우기만 하므로 반복 호출해도 힙 할당이 거의 없다
- 여러 유닛이 같은 타일맵을 쓰면 `AStar` 하나를 공유해도 된다 (단, 동시에 호출하지 않는 단일 스레드 전제)

# 3. 동작 흐름

---

```
FindPath(start, target)
  │
  ├─ Init()  ─── open/closed/nodeMap 비우고 노드는 풀로 반환
  ├─ 월드 좌표 → 셀 좌표 (Tilemap.WorldToCell)
  ├─ start == target ? → null
  ├─ 시작 노드 enqueue (F = 0 + H)
  │
  └─ while (반복 < 300 && open 비어있지 않음)
        ├─ F 최소 노드 dequeue
        ├─ closed에 이미 있으면 skip   ← 중복 enqueue된 낡은 항목 처리
        ├─ 목표 도달 → BuildPath()
        ├─ closed에 추가
        └─ 4방향 이웃마다 TryUpdateNeighbor()
              ├─ closed면 skip
              ├─ sumG = current.G + 이동비용(10)
              └─ open에 없거나 더 싸면 → 노드 갱신 후 enqueue
```

### 1) 비용과 휴리스틱
| 항목 | 값 | 비고 |
|---|---|---|
| 이동 비용 `G` | 칸당 10 | `NodeManager.GetMoveCost()`가 항상 10 반환. 지형별 비용을 주려면 여기만 수정 |
| 휴리스틱 `H` | 맨해튼 거리 × 10 | 4방향 이동이라 맨해튼 거리가 허용적(admissible) |
| 이웃 | 상·하·좌·우 4방향 | 대각선 없음. 대각선을 넣으면 휴리스틱도 옥타일 거리로 바꿔야 함 |

### 2) 노드 풀링
- `NodeManager._nodeMap`이 셀 좌표 → `Node` 캐시 역할. 같은 칸이 여러 번 방문돼도 노드는 하나
- `Init()` 시 모든 노드를 `_nodePool`(Stack)로 돌려보내고, 다음 탐색에서 `CreateOrGetNode()`가 꺼내 씀
- 자세한 원리는 [[[Design Pattern] 오브젝트 풀링 패턴]] 참고

### 3) 경로 복원
- 목표 노드에서 `Parent`를 따라 역추적한 뒤 `Reverse()`
- `Parent == null`인 시작 노드는 제외되므로 **결과 리스트에 시작 칸은 없다**
- 각 항목은 `Tilemap.GetCellCenterWorld()`로 변환한 셀 중심 월드 좌표

# 4. 주의점

---

| 항목 | 내용 |
|---|---|
| `PriorityQueue<TElement,TPriority>` | .NET 6+ API. Unity 프로젝트의 API 호환성 수준에 따라 컴파일되지 않을 수 있음. 그 경우 최소 힙을 직접 구현하거나 `SortedSet` 등으로 대체 |
| `MAX_ITERATIONS = 300` | 300개 노드까지만 확장. 맵이 크거나 목표가 멀면 실패(`null`)할 수 있으니 맵 크기에 맞게 조정 |
| 낡은 open 항목 | 더 싼 경로가 발견돼도 기존 enqueue 항목을 제거하지 않고, dequeue 시 closed 검사로 걸러냄(lazy deletion). 정상 동작이지만 open 리스트 크기가 실제보다 클 수 있음 |
| 이동 비용 기준 | `GetMoveCost(current.Pos)`로 **현재 칸을 떠나는 비용**을 쓴다. 지형별 비용을 "들어가는 칸" 기준으로 하려면 `neighborPos`를 넘기도록 수정 |
| 타일맵 경계 | `HasTile()`이 false인 곳은 전부 통행 가능으로 본다. 맵 바깥으로 탐색이 새지 않게 외곽을 벽 타일로 둘러야 함 |
| 스레드 | 내부 컬렉션을 재사용하므로 스레드 안전하지 않음 |

# 5. 확장 아이디어

---

- 지형별 비용: `GetMoveCost()`에서 별도 `Tilemap`이나 `Dictionary<Vector2Int,int>`를 조회
- 대각선 이동: `GetNeighbors()`에 8방향 추가 + 휴리스틱을 옥타일 거리로 교체 + 모서리 끼임(corner cutting) 방지
- 경로 스무딩: 결과 리스트에서 직선 시야가 확보되는 중간점 제거
- 비동기화: 프레임 분산이 필요하면 `while` 루프를 코루틴/Job으로 분할
