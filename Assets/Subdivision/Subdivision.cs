using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Subdivision : MonoBehaviour
{
	public bool isSingleStep = false;
	public bool isAuto = false;
	private Mesh mesh;
	private Vector3[] aroundVertices = null;
	private Vector3 adjustVertex;
	private Vector3 afterAdjustVertex;
	private WaitForSeconds waitTime = null;
	private WaitUntil waitUntil = null;
	private bool onAdjust = false;
	private bool onSubdivision = false;
	private bool continueNextStep = false;
	private bool skipCurrentSubdivision = false;

	void Start()
	{
		mesh = GetComponent<MeshFilter>().mesh;
		if (isSingleStep)
		{
			waitTime = new WaitForSeconds(1f);
			if (!isAuto)
			{
				waitUntil = new WaitUntil(() => continueNextStep || skipCurrentSubdivision);
			}
		}
	}

	int SetTriangle(int[] triangles, int i, int v0, int v1, int v2)
	{
		triangles[i] = v0;
		triangles[i+1] = v1;
		triangles[i+2] = v2;
		return i + 3;
	}
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) && !onSubdivision)
		{
			skipCurrentSubdivision = false;
			StartCoroutine(ExecuteSubdivision());
		}
		if (Input.GetKeyDown(KeyCode.J) && onSubdivision)
		{
			continueNextStep = true;
		}
		if (Input.GetKeyDown(KeyCode.K) && onSubdivision)
		{
			skipCurrentSubdivision = true;
		}
	}

	void OnDrawGizmos()
	{
		var r = 0.005f;
		if (onAdjust && aroundVertices != null)
		{
			Gizmos.color = Color.green;
			for (int i = 0; i < aroundVertices.Length; i++)
			{
				Gizmos.DrawSphere(transform.TransformPoint(aroundVertices[i]), r);
				if (i != aroundVertices.Length -1)
				{
					Gizmos.DrawLine(aroundVertices[i], aroundVertices[i+1]);
				}
				else
				{
					Gizmos.DrawLine(aroundVertices[i], aroundVertices[0]);
				}
			}
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(transform.TransformPoint(adjustVertex), r);
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(transform.TransformPoint(afterAdjustVertex), r);
		}
	}

	IEnumerator ExecuteSubdivision()
	{
		onSubdivision = true;
		var triangles = mesh.triangles;
		List<int>[] neighbors = new List<int>[mesh.vertexCount];
		Dictionary<Edge, Pair> edgeDict = new Dictionary<Edge, Pair>();
		Dictionary<Edge, int> edgeMidDict = new Dictionary<Edge, int>();

		for (int i = 0; i < neighbors.Length; i++)
		{
			neighbors[i] = new List<int>();
		}
		for (int i = 0; i < triangles.Length; i+=3)
		{
			var v0 = triangles[i];
			var v1 = triangles[i+1];
			var v2 = triangles[i+2];
			// Find vertex's neighbors(not duplicate)
			if (!neighbors[v0].Contains(v1)) neighbors[v0].Add(v1);
			if (!neighbors[v0].Contains(v2)) neighbors[v0].Add(v2);

			if (!neighbors[v1].Contains(v0)) neighbors[v1].Add(v0);
			if (!neighbors[v1].Contains(v2)) neighbors[v1].Add(v2);

			if (!neighbors[v2].Contains(v1)) neighbors[v2].Add(v1);
			if (!neighbors[v2].Contains(v0)) neighbors[v2].Add(v0);

			// Add edge and its relevant vertices
			var e0 = new Edge(v0, v1);
			if (edgeDict.ContainsKey(e0))
			{
				var p0 = edgeDict[e0];
				p0.right = v2;
				edgeDict[e0] = p0;
			}
			else
			{
				var p0 = new Pair(v2);
				edgeDict.Add(e0, p0);
			}

			var e1 = new Edge(v1, v2);
			if (edgeDict.ContainsKey(e1))
			{
				var p1 = edgeDict[e1];
				p1.right = v0;
				edgeDict[e1] = p1;
			}
			else
			{
				var p1 = new Pair(v0);
				edgeDict.Add(e1, p1);
			}

			var e2 = new Edge(v2, v0);
			if (edgeDict.ContainsKey(e2))
			{
				var p2 = edgeDict[e2];
				p2.right = v1;
				edgeDict[e2] = p2;
			}
			else
			{
				var p2 = new Pair(v1);
				edgeDict.Add(e2, p2);
			}
		}

		var vertices = mesh.vertices;
		var newVertices = new Vector3[vertices.Length + edgeDict.Count];
		System.Array.Copy(vertices, newVertices, vertices.Length);
		var newTriangles = new int[triangles.Length * 4];

		// Generate new vertices
		int idx = vertices.Length;
		foreach (var edge in edgeDict.Keys)
		{
			var mid = (vertices[edge.v0] + vertices[edge.v1]) / 2;
			newVertices[idx] = mid;
			edgeMidDict.Add(edge, idx);
			idx++;
		}

		// Generate new triangles
		for (int i = 0, j = 0; i < triangles.Length; i+=3)
		{
			var v0 = triangles[i];
			var v1 = triangles[i+1];
			var v2 = triangles[i+2];
			var m0 = edgeMidDict[new Edge(v0, v1)];
			var m1 = edgeMidDict[new Edge(v1, v2)];
			var m2 = edgeMidDict[new Edge(v2, v0)];
			
			j = SetTriangle(newTriangles, j, v0, m0, m2);
			j = SetTriangle(newTriangles, j, m0, v1, m1);
			j = SetTriangle(newTriangles, j, m2, m1, v2);
			j = SetTriangle(newTriangles, j, m0, m1, m2);
		}
		// Start draw gizmos
		onAdjust = true;
		// Adjust new vertices
		foreach (var edge in edgeMidDict.Keys)
		{
			var midIdx = edgeMidDict[edge];
			var left = vertices[edge.v0];
			var right = vertices[edge.v1];
			var pair = edgeDict[edge];
			var bottom = vertices[pair.left];
			adjustVertex = newVertices[midIdx];
			if (pair.right == -1)
			{
				aroundVertices = new Vector3[]{left, bottom, right};
				newVertices[midIdx] = (left * 13 + right * 13 + bottom * 6) / 32f;
			}
			else
			{
				var top = vertices[pair.right];
				aroundVertices = new Vector3[]{left, bottom, right, top};
				newVertices[midIdx] = (left * 3 + right * 3 + bottom + top) / 8f;
			}
			afterAdjustVertex = newVertices[midIdx];
			if (isAuto)
			{
				yield return waitTime;
			}
			else
			{
				yield return waitUntil;
				continueNextStep = false;
			}
		}

		// Adjust origin vertices
		for (int i = 0; i < neighbors.Length; i++)
		{
			float factor;
			int n = neighbors[i].Count;
			if (n > 3)
			{
				factor = 3f / (8f * n);
			}
			else if (n == 3)
			{
				factor = 3f / 16f;
			}
			else // n == 2
			{
				factor = 3f / 8f;
			}
			var point = vertices[i] * (1f - n * factor);
			neighbors[i].ForEach(p => {
				point += factor * vertices[p];
			});
			adjustVertex = newVertices[i];
			afterAdjustVertex = point;
			aroundVertices = new Vector3[n];
			for (int j = 0; j < n; j++)
			{
				aroundVertices[j] = vertices[neighbors[i][j]];
			}
			newVertices[i] = point;
			if (isAuto)
			{
				yield return waitTime;
			}
			else
			{
				yield return waitUntil;
				continueNextStep = false;
			}
		}
		// Stop draw gizmos
		onAdjust = false;

		// Apply new mesh
		mesh.vertices = newVertices;
		mesh.triangles = newTriangles;
		mesh.RecalculateNormals();

		onSubdivision = false;
	}

	void ShowLog(List<int>[] neighbors, Dictionary<Edge, Pair> edgeDict)
	{
		for (int i = 0; i < neighbors.Length; i++)
		{
			string msg = string.Format("{0} neighors: ", i);
			neighbors[i].ForEach(x => {
				msg += x + " ,";
			});
			Debug.Log(msg);
		}

		foreach (var edge in edgeDict.Keys)
		{
			string msg = edge.ToString() + " : " + edgeDict[edge].ToString();
			Debug.Log(msg);
		}
	}

	struct Edge
	{
		public int v0, v1;
		public Edge(int v0, int v1)
		{
			if (v0 > v1)
			{
				var t = v0;
				v0 = v1;
				v1 = t;
			}
			this.v0 = v0;
			this.v1 = v1;
		}

		public override string ToString()
		{
			return string.Format("edge ({0}, {1})", v0, v1);
		}
	}

	struct Pair
	{
		public int left, right;
		public Pair(int left)
		{
			this.left = left;
			this.right = -1;
		}
		
		public override string ToString()
		{
			return string.Format("pair ({0}, {1})", left, right);
		}
	}
}
