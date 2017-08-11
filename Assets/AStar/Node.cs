using UnityEngine;

namespace AStar
{
	public enum NodeType
	{
		start,
		end,
		walkable,
		unwalkable,
		path
	}

	public enum ActiveType
	{
		open,
		closed,
		none
	}

	[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
	public class Node : MonoBehaviour, System.IComparable<Node>
	{
		private NodeType nodeType = NodeType.walkable;
		public NodeType NodeType
		{
			get {return nodeType;}
			set
			{
				nodeType = value;
				SetColor();
			}
		}
		private ActiveType activeType = ActiveType.none;
		public ActiveType ActiveType
		{
			get {return activeType;}
			set
			{
				activeType = value;
				SetColor();
			}
		}
		
		public Node parent = null;
		private float f, g, h;
		public float G { get{return g;} set{g = value; f = g + h;}}
		public float H { get{return h;} set{h = value; f = g + h;}}
		public float F { get{return f;} }
		public int x, y;
		public System.Action<int, int, int> OnClick;

		public Node(NodeType nodeType, int x, int y)
		{
			this.nodeType = nodeType;
			this.x = x;
			this.y = y;
		}

		public int CompareTo(Node other)
		{
			if (other == null)
			{
				return 1;
			}
			return f.CompareTo(other.f);
		}

		public override bool Equals(object obj)
		{
			Node node = obj as Node;
			if (node == null)
			{
				return false;
			}
			return (x == node.x) && (y == node.y)
				&& (f == node.f) && (g == node.g) && (h == node.h);
		}

		public override int GetHashCode()
		{
			return ShiftAndWrap(x.GetHashCode(), 6) ^ ShiftAndWrap(y.GetHashCode(), 4)
				^ ShiftAndWrap(g.GetHashCode(), 2) ^ ShiftAndWrap(h.GetHashCode(), 0);
		}

		public int ShiftAndWrap(int value, int positions)
		{
			positions = positions & 0x1F;

			// Save the existing bit pattern, but interpret it as an unsigned integer.
			uint number = System.BitConverter.ToUInt32(System.BitConverter.GetBytes(value), 0);
			// Preserve the bits to be discarded.
			uint wrapped = number >> (32 - positions);
			// Shift and wrap the discarded bits.
			return System.BitConverter.ToInt32(System.BitConverter.GetBytes((number << positions) | wrapped), 0);
		}

		void Start()
		{
			SetColor();
		}

		void OnMouseDown()
		{
			OnClick(x, y, 0);
		}

		void OnMouseOver()
		{
			if (Input.GetMouseButtonDown(1))
			{
				OnClick(x, y, 1);
			}
		}

		void SetColor()
		{
			var material = GetComponent<SpriteRenderer>().material;
			switch (nodeType)
			{
				case NodeType.unwalkable:
					material.color = Color.black;
					return;
				case NodeType.walkable:
				case NodeType.path:
					if (activeType == ActiveType.open)
					{
						material.color = Color.green;
					}
					else if (activeType == ActiveType.closed)
					{
						material.color = Color.cyan;
					}
					else
					{
						material.color = Color.white;
					}
					return;
				case NodeType.start:
					material.color = Color.red;
					return;
				case NodeType.end:
					material.color = Color.blue;
					return;
			}
		}
	}
}