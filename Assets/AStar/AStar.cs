using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AStar : MonoBehaviour
{
	enum NodeType
	{
		start,
		end,
		walkable,
		unwalkable,
		path
	}

	enum ActiveType
	{
		open,
		closed,
		none
	}

	class Node
	{
		public NodeType nodeType = NodeType.walkable;
		public Node parent = null;
		private float g, f, h;
		public float G { get{return g;} set{g = value; h = g + f;}}
		public float F { get{return f;} set{f = value; h = g + f;}}
		public float H { get{ return h;}}
		
		public int x, y;

		public ActiveType activeType = ActiveType.none;

		public Node(NodeType nodeType, int x, int y)
		{
			this.nodeType = nodeType;
			this.x = x;
			this.y = y;
		}
	}

	static int[,] steps = new int[,]{{-1, -1, 14}, {0, -1, 10}, {1, -1, 14},
									 {-1, 0, 10},               {1, 0, 10}, 
									 {-1, 1, 14},  {0, 1, 10},  {1, 1, 14}};

	public int rows = 10;
	public int columns = 10;

	public Vector2 startPoint;
	public Vector2 endPoint;

	Node[,] map = null;
	Node startNode, endNode;
	List<Vector2> path = null;

	void Start()
	{
		map = new Node[rows, columns];
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < columns; j++)
			{
				map[i, j] = new Node(NodeType.walkable, i, j);
				if (i == 4 && (j >= 2 && j <= 7))
				{
					map[i, j].nodeType = NodeType.unwalkable;
				}
			}
		}
		startNode = map[(int)startPoint.x, (int)startPoint.y];
		startNode.nodeType = NodeType.start;
		endNode = map[(int)endPoint.x, (int)endPoint.y];
		endNode.nodeType = NodeType.end;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			AStarPathFind();
		}
	}

	float ManhattanDistance(Node a, Node b)
	{
		return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
	}

	void InitMapState()
	{
		foreach (var node in map)
		{
			node.activeType = ActiveType.none;
			node.F = ManhattanDistance(node, endNode);
		}
	}

	static void AStarAdd(LinkedList<Node> list, Node node)
	{
		node.activeType = ActiveType.open;
		if (list.Count == 0)
		{
			list.AddLast(node);
			return;
		}
		if (node.H <= list.First.Value.H)
		{
			list.AddBefore(list.First, node);
			return;
		}
		if (node.H > list.Last.Value.H)
		{
			list.AddLast(node);
			return;
		}
		var linkedNode = list.First;
		while (linkedNode.Next != null)
		{
			if (node.H > linkedNode.Value.H && node.H <= linkedNode.Next.Value.H)
			{
				list.AddAfter(linkedNode, node);
				break;
			}
			linkedNode = linkedNode.Next;
		}
		if (linkedNode.Next == null)
		{
			Debug.LogError("unexpect error");
		}
	}

	static Node AStarRemove(LinkedList<Node> list)
	{
		var node = list.First.Value;
		node.activeType = ActiveType.closed;
		list.RemoveFirst();
		return node;
	}

	static void AStarAdjust(LinkedList<Node> list, Node node)
	{
		list.Remove(node);
		AStarAdd(list, node);
	}

	void AStarPathFind()
	{
		InitMapState();
		LinkedList<Node> open = new LinkedList<Node>();
		AStarAdd(open, startNode);
		while (open.Count > 0)
		{
			var node = AStarRemove(open);
			
			if (node.nodeType == NodeType.end)
			{
				UpdatePath(node);
				break;
			}
			for (int i = 0; i < steps.GetLength(0); i++)
			{
				var x = node.x + steps[i, 0];
				var y = node.y + steps[i, 1];
				if (x < 0 || x >= rows || y < 0 || y >= columns ||
					map[x, y].activeType == ActiveType.closed || map[x, y].nodeType == NodeType.unwalkable)
				{
					continue;
				}
				var dis = node.G + steps[i, 2];
				if (map[x, y].activeType == ActiveType.none)
				{
					map[x, y].G = dis;
					map[x, y].parent = node;
					AStarAdd(open, map[x, y]);
				}
				else if (dis < map[x, y].G)
				{
					map[x, y].G = dis;
					map[x, y].parent = node;
					AStarAdjust(open, map[x, y]);
				}
			}
		}
	}

	void UpdatePath(Node node)
	{
		path = new List<Vector2>();
		while (node.parent != null)
		{
			if (node.nodeType == NodeType.walkable)
			{
				node.nodeType = NodeType.path;
			}
			path.Add(new Vector2(node.x, node.y));
			node = node.parent;
		}
		path.Add(new Vector2(node.x, node.y));
	}

	void OnDrawGizmos()
	{
		if (map == null)
		{
			return;
		}
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < columns; j++)
			{
				if (map[i, j].nodeType == NodeType.unwalkable)
				{
					Gizmos.color = Color.black;
				}
				else if (map[i, j].nodeType == NodeType.walkable || map[i, j].nodeType == NodeType.path)
				{
					Gizmos.color = Color.white;
					if (map[i, j].activeType == ActiveType.open)
					{
						Gizmos.color = Color.green;
					}
					else if (map[i, j].activeType == ActiveType.closed)
					{
						Gizmos.color = Color.cyan;
					}
				}
				else if (map[i, j].nodeType == NodeType.start)
				{
					Gizmos.color = Color.red;
				}
				else if (map[i, j].nodeType == NodeType.end)
				{
					Gizmos.color = Color.blue;
				}
				else if (map[i, j].nodeType == NodeType.path)
				{
					//Gizmos.color = Color.green;
				}
				Gizmos.DrawCube(new Vector3(i, j), Vector3.one * 0.9f);
			}
		}
		if (path == null)
		{
			return;
		}
		Gizmos.color = Color.yellow;
		for (int i = 0; i < path.Count - 1; i++)
		{
			Gizmos.DrawLine(path[i], path[i+1]);
		}
	}
}