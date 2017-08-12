using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace AStar
{
	public class AStar : MonoBehaviour
	{

		public enum DistanceType
		{
			Euclidean,
			Manhattan
		}

		static int[,] steps1 = new int[,]{{-1, -1, 14}, {0, -1, 10}, {1, -1, 14},
										{-1, 0, 10},               {1, 0, 10}, 
										{-1, 1, 14},  {0, 1, 10},  {1, 1, 14}};
		static int [,] steps2 = {{0, -1, 10}, {1, 0, 10}, {0, 1, 10}, {-1, 0, 10}};

		public int rows = 10;
		public int columns = 10;
		public Node nodePrefab;

		public Vector2 startPoint;
		public Vector2 endPoint;
		public DistanceType distanceType = DistanceType.Manhattan;
		public bool allowDiagonal;

		Node[,] map = null;
		Node startNode, endNode;
		List<Vector2> path = new List<Vector2>();
		NodeType leftClickNodeType = NodeType.start;
		GLDrawLines glDrawLines;

		void Start()
		{
			GenerateMap();
			glDrawLines = Camera.main.GetComponent<GLDrawLines>();
		}

		void GenerateMap()
		{
			map = new Node[rows, columns];
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < columns; j++)
				{
					var obj = Instantiate(nodePrefab, transform);
					obj.transform.localPosition = new Vector2(i, j);
					obj.transform.localScale *= 0.9f;
					obj.x = i;
					obj.y = j;
					obj.OnClick = OnClick;
					obj.NodeType = NodeType.walkable;
					obj.ActiveType = ActiveType.none;
					map[i, j] = obj;
				}
			}
			startNode = map[(int)startPoint.x, (int)startPoint.y];
			startNode.NodeType = NodeType.start;
			endNode = map[(int)endPoint.x, (int)endPoint.y];
			endNode.NodeType = NodeType.end;
		}

		void OnClick(int x, int y, int p)
		{
			if (p == 0)
			{
				if (map[x, y].NodeType != NodeType.unwalkable && map[x, y].NodeType != NodeType.start
					&& map[x, y].NodeType != NodeType.end)
				{
					map[x, y].NodeType = leftClickNodeType;
					if (leftClickNodeType == NodeType.start)
					{
						startNode.NodeType = NodeType.walkable;
						startNode = map[x, y];
						leftClickNodeType = NodeType.end;
					}
					else
					{
						endNode.NodeType = NodeType.walkable;
						endNode = map[x, y];
						leftClickNodeType = NodeType.start;
					}
					AStarPathFind();
				}
			}
			else if (p == 1)
			{
				if (map[x, y].NodeType != NodeType.start || map[x, y].NodeType != NodeType.end)
				{
					if (map[x, y].NodeType == NodeType.unwalkable)
					{
						map[x, y].NodeType = NodeType.walkable;
					}
					else
					{
						map[x, y].NodeType = NodeType.unwalkable;
					}
					AStarPathFind();
				}

			}
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
			return 10 * (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y));
		}

		float EuclideanDistance(Node a, Node b)
		{
			return 14 * (Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2f) + Mathf.Pow(a.y - b.y, 2f)));
		}

		void InitMapState()
		{
			foreach (var node in map)
			{
				node.parent = null;
				node.ActiveType = ActiveType.none;
				node.G = 0;
				if (distanceType == DistanceType.Manhattan)
				{
					node.H = ManhattanDistance(node, endNode);
				}
				else if (distanceType == DistanceType.Euclidean)
				{
					node.H = EuclideanDistance(node, endNode);
				}
			}
		}

		void AStarPathFind()
		{
			InitMapState();
			Node nearestNode = startNode;
			PriorityQueue<Node> open = new PriorityQueue<Node>();
			startNode.ActiveType = ActiveType.open;
			open.Add(startNode);
			int[,] steps;
			if (allowDiagonal)
			{
				steps = steps1;
			}
			else
			{
				steps = steps2;
			}
			while (open.Count > 0)
			{
				var node = open.Poll();
				node.ActiveType = ActiveType.closed;
				
				if (node.NodeType == NodeType.end)
				{
					UpdatePath(node);
					break;
				}
				for (int i = 0; i < steps.GetLength(0); i++)
				{
					var x = node.x + steps[i, 0];
					var y = node.y + steps[i, 1];
					if (x < 0 || x >= rows || y < 0 || y >= columns ||
						map[x, y].ActiveType == ActiveType.closed || map[x, y].NodeType == NodeType.unwalkable)
					{
						continue;
					}
					var dis = node.G + steps[i, 2];
					if (map[x, y].ActiveType == ActiveType.none)
					{
						map[x, y].G = dis;
						map[x, y].parent = node;
						map[x, y].ActiveType = ActiveType.open;
						open.Add(map[x, y]);
					}
					else if (dis < map[x, y].G)
					{
						map[x, y].G = dis;
						map[x, y].parent = node;
						open.Remove(map[x, y]);
						open.Add(map[x, y]);
					}
					if (map[x, y].H < nearestNode.H)
					{
						nearestNode = map[x, y];
					}
				}
				// not find path
				if (open.Count == 0)
				{
					UpdatePath(nearestNode);
				}
			}
		}

		void UpdatePath(Node node)
		{
			path.Clear();
			int c = 0;
			while (node.parent != null)
			{
				if (c > rows* columns)
				{
					Debug.LogError("foooooool!");
					return;
				}
				c++;
				if (node.NodeType == NodeType.walkable)
				{
					node.NodeType = NodeType.path;
				}
				path.Add(new Vector2(node.x, node.y));
				node = node.parent;
			}
			path.Add(new Vector2(node.x, node.y));
			glDrawLines.path = path.ToArray();
		}

		void OnDrawGizmos()
		{
			if (map == null)
			{
				return;
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
}