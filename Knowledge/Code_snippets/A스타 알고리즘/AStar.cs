using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStar
{
    private const int MAX_ITERATIONS = 300;

    private readonly Tilemap _wallTilemap;
    private readonly NodeManager _nodeManager;

    private readonly PriorityQueue<Node, int> _openList;
    private readonly Dictionary<Vector2Int, Node> _openDict;
    private readonly HashSet<Vector2Int> _closedSet;
    private int _count;

    public AStar(Tilemap wallTilemap)
    {
        _wallTilemap = wallTilemap;
        _nodeManager = new NodeManager(wallTilemap);

        _openList = new PriorityQueue<Node, int>();
        _openDict = new Dictionary<Vector2Int, Node>();
        _closedSet = new HashSet<Vector2Int>();
    }

    /// <summary> A* 알고리즘으로 경로를 찾는다 </summary>
    public List<Vector2> FindPath(Vector3 start, Vector3 target)
    {
        Init();

        Vector2Int startCellPos = (Vector2Int)_wallTilemap.WorldToCell(start);
        Vector2Int targetCellPos = (Vector2Int)_wallTilemap.WorldToCell(target);

        if (startCellPos == targetCellPos)
            return null;

        Node startNode = _nodeManager.GetNode(startCellPos, 0, Heuristic(startCellPos, targetCellPos));
        
        _openList.Enqueue(startNode, startNode.F);
        _openDict.Add(startCellPos, startNode);

        while (_count < MAX_ITERATIONS && _openList.Count > 0)
        {
            _count++;
            Node current = _openList.Dequeue();

            if (_closedSet.Contains(current.Pos))
                continue;

            if (current.Pos == targetCellPos)
                return BuildPath(current);

            _closedSet.Add(current.Pos);
            _openDict.Remove(current.Pos);

            foreach (var neighborPos in _nodeManager.GetNeighbors(current.Pos))
            {
                TryUpdateNeighbor(current, neighborPos, targetCellPos);
            }
        }

        Debug.LogWarning("경로 탐색 실패");
        return null;
    }

    #region 내부 메서드

    private void TryUpdateNeighbor(Node current, Vector2Int neighborPos, Vector2Int targetCellPos)
    {
        if (_closedSet.Contains(neighborPos))
            return;

        int sumG = current.G + _nodeManager.GetMoveCost(current.Pos);

        if (!_openDict.TryGetValue(neighborPos, out Node neighborNode) || sumG < neighborNode.G)
        {
            neighborNode = _nodeManager.GetNode(neighborPos, sumG, Heuristic(neighborPos, targetCellPos), current);
            
            _openList.Enqueue(neighborNode, neighborNode.F);
            _openDict[neighborPos] = neighborNode;
        }
    }

    private List<Vector2> BuildPath(Node targetNode)
    {
        List<Vector2> path = new List<Vector2>();
        Node current = targetNode;

        while (current != null)
        {
            if (current.Parent != null)
                path.Add(GetCellPos(current.Pos));
            
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }

    private int Heuristic(Vector2Int current, Vector2Int target) =>
        (Mathf.Abs(target.x - current.x) + Mathf.Abs(target.y - current.y)) * 10;

    public Vector2 GetCellPos(Vector3 pos) => CellPos(pos);
    public Vector2 GetCellPos(Vector2 pos) => CellPos(pos);
    public Vector2 GetCellPos(Vector2Int pos) => CellPos(pos);

    private Vector2 CellPos(Vector2 pos) =>
        _wallTilemap.GetCellCenterWorld(Vector3Int.FloorToInt(pos));

    private void Init()
    {
        _count = 0;
        _openList.Clear();
        _openDict.Clear();
        _closedSet.Clear();
        _nodeManager.Init();
    }

    #endregion
}