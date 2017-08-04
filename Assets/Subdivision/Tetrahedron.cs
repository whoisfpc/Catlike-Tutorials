using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Tetrahedron : MonoBehaviour
{
	public float scale = 1f;
	private Mesh mesh;
	private Vector3[] vertices = null;

	void Awake()
	{
		Generate();
	}

	void Generate()
	{
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Terahedron";
		vertices = new Vector3[4];
		vertices[0] = new Vector3(0, 0, 0);
		vertices[1] = new Vector3(1, 0, 0);
		vertices[2] = new Vector3(0.5f, 0, 0.5f * Mathf.Sqrt(3));
		vertices[3] = new Vector3(0.5f, Mathf.Sqrt(2f/3f), Mathf.Sqrt(3) / 6);
		mesh.vertices = vertices;

		var triangles = new int[12];
		int i = 0;
		i = SetTriangle(triangles, i, 0, 1, 2);
		i = SetTriangle(triangles, i, 0, 3, 1);
		i = SetTriangle(triangles, i, 1, 3, 2);
		i = SetTriangle(triangles, i, 2, 3, 0);
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
}
