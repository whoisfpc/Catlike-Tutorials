using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Tetrahedron : MonoBehaviour
{
	public float scale = 1f;
	private Mesh mesh;
	private Vector3[] vertices;

	void Awake()
	{
		Generate();
	}

	void Generate()
	{
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Terahedron";
		vertices = new Vector3[12];
		var v0 = new Vector3(0, 0, 0);
		var v1 = new Vector3(1, 0, 0);
		var v2 = new Vector3(0.5f, 0, 0.5f * Mathf.Sqrt(3));
		var v3 = new Vector3(0.5f, Mathf.Sqrt(2f/3f), v2.z / 3);
		vertices[0] = v0;
		vertices[1] = v1;
		vertices[2] = v2;
		vertices[3] = v0;
		vertices[4] = v3;
		vertices[5] = v1;
		vertices[6] = v1;
		vertices[7] = v3;
		vertices[8] = v2;
		vertices[9] = v2;
		vertices[10] = v3;
		vertices[11] = v0;
		mesh.vertices = vertices;

		var triangles = new int[12];
		int i = 0;
		i = SetTriangle(triangles, i, 0, 1, 2);
		i = SetTriangle(triangles, i, 3, 4, 5);
		i = SetTriangle(triangles, i, 6, 7, 8);
		i = SetTriangle(triangles, i, 9, 10, 11);
		mesh.triangles = triangles;

		mesh.RecalculateNormals();
	}

	int SetTriangle(int[] triangles, int i, int v0, int v1, int v2)
	{
		triangles[i] = v0;
		triangles[i+1] = v1;
		triangles[i+2] = v2;
		return i + 3;
	}

	void OnDrawGizmos()
	{
		if (vertices != null)
		{
			foreach (var vertex in vertices)
			{
				Gizmos.DrawSphere(transform.TransformPoint(vertex), 0.1f);
			}
		}
	}
}
