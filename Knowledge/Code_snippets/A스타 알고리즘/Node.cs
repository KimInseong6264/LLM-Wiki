using UnityEngine;

/// <summary> 경로 탐색 시 사용할 노드 정보 </summary>
public class Node
{
    public Vector2Int Pos { get; set; }
    public int G { get; set; }
    public int H { get; set; }
    public int F => G + H;
    public Node Parent { get; set; }

    /// <summary> 노드 데이터 초기화 (풀링용) </summary>
    public void Reset(Vector2Int pos, int g, int h, Node parent)
    {
        Pos = pos;
        G = g;
        H = h;
        Parent = parent;
    }
}