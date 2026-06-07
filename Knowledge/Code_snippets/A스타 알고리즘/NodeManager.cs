using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NodeManager
{
    private readonly Tilemap _wallTilemap;
    private readonly List<Vector2Int> _neighborList;
    private readonly Dictionary<Vector2Int, Node> _nodeMap;
    private readonly Stack<Node> _nodePool;

    public NodeManager(Tilemap wall)
    {
        _wallTilemap = wall;
        _neighborList = new List<Vector2Int>();
        _nodeMap = new Dictionary<Vector2Int, Node>();
        _nodePool = new Stack<Node>();
    }

    /// <summary> 이동 가능 여부 검사 </summary>
    public bool IsWalkable(Vector2Int cellPos) => !_wallTilemap.HasTile((Vector3Int)cellPos);

    /// <summary> 해당 셀의 이동 비용 반환 </summary>
    public int GetMoveCost(Vector2Int cellPos) => 10;

    /// <summary> 이웃한 셀 좌표 배열 반환 </summary>
    public Vector2Int[] GetNeighbors(Vector2Int cellPos)
    {
        _neighborList.Clear();

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            Vector2Int next = cellPos + dir;

            if (!IsWalkable(next)) { continue; }

            _neighborList.Add(next);
        }

        return _neighborList.ToArray();
    }

    /// <summary> 셀 좌표에 해당하는 노드 획득 (풀링 적용) </summary>
    public Node GetNode(Vector2Int cellPos, int g, int h, Node parent = null)
    {
        if (!_nodeMap.TryGetValue(cellPos, out var node))
        {
            node = CreateOrGetNode();
            node.Reset(cellPos, g, h, parent);
            _nodeMap[cellPos] = node;
        }
        else if (g < node.G)
        {
            node.G = g;
            node.H = h;
            node.Parent = parent;
        }

        return node;
    }

    /// <summary> 모든 노드를 풀로 반환하고 초기화 </summary>
    public void Init()
    {
        foreach (var node in _nodeMap.Values)
        {
            _nodePool.Push(node);
        }

        _nodeMap.Clear();
        _neighborList.Clear();
    }

    #region 내부 메서드

    private Node CreateOrGetNode()
    {
        return _nodePool.Count > 0 ? _nodePool.Pop() : new Node();
    }

    #endregion
}