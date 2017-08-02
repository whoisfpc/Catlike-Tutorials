using UnityEngine;
using System.Collections.Generic;

public class TransformationGrid : MonoBehaviour {

	public Transform prefab;

	public int gridResolution = 10;

	Transform[] grid;

	List<Transformation> transformations;

	Matrix4x4 transformation;

	void Awake () {
		grid = new Transform[gridResolution * gridResolution * gridResolution];
		for (int i = 0, z = 0; z < gridResolution; z++) {
			for (int y = 0; y < gridResolution; y++) {
				for (int x = 0; x < gridResolution; x++, i++) {
					grid[i] = CreateGridPoint(x, y, z);
				}
			}
		}
		transformations = new List<Transformation>();
	}

	Transform CreateGridPoint (int x, int y, int z) {
		Transform point = Instantiate(prefab, transform);
		point.localPosition = GetCoordinates(x, y, z);
		point.GetComponent<MeshRenderer>().material.color = new Color(
			(float)x / gridResolution,
			(float)y / gridResolution,
			(float)z / gridResolution
		);
		return point;
	}

	Vector3 GetCoordinates (int x, int y, int z) {
		return new Vector3(
			x - (gridResolution - 1) * 0.5f,
			y - (gridResolution - 1) * 0.5f,
			z - (gridResolution - 1) * 0.5f
		);
	}

	void Update () {
		UpdateTransformation();
		for (int i = 0, z = 0; z < gridResolution; z++)
		{
			for (int y = 0; y < gridResolution; y++)
			{
				for (int x = 0; x < gridResolution; x++, i++)
				{
					grid[i].localPosition = TransformPoint(x, y, z);
				}
			}
		}
	}

	void UpdateTransformation () {
		GetComponents(transformations);
		if (transformations.Count > 0) {
			transformation = transformations[0].Matrix;
			for (int i = 1; i < transformations.Count; i++) {
				transformation = transformations[i].Matrix * transformation;
			}
		}
	}

	Vector3 TransformPoint (int x, int y, int z) {
		Vector3 coordinates = GetCoordinates(x, y, z);
		//return transformation.MultiplyPoint3x4(coordinates);
		return mul(transformation, coordinates);
		//return transformation.MultiplyPoint(coordinates);
	}

	Vector3 mul(Matrix4x4 mat, Vector3 vec)
	{
		Vector4 v = new Vector4(vec.x, vec.y, vec.z, 1);
		Vector4 ret = new Vector4();
		for (int i = 0; i < 4; i++)
		{
			var r = mat.GetRow(i);
			if (i == 0) ret.x = Vector4.Dot(r, v);
			if (i == 1) ret.y = Vector4.Dot(r, v);
			if (i == 2) ret.z = Vector4.Dot(r, v);
			if (i == 3) ret.w = Vector4.Dot(r, v);
		}
		return ret / ret.w;
	}
}