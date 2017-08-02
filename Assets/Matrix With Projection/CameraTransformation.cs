using UnityEngine;

public class CameraTransformation : Transformation {

	public float focalLength = 1f;
	[Range(0, 179)]
	public float fov = 60;
	public float aspect = 4f / 3f;
	public float near = 0.1f;
	public float far = 100f;

	public override Matrix4x4 Matrix {
		get {
			Matrix4x4 matrix = Matrix4x4.zero;
			matrix[0,0] = aspect / Mathf.Tan(Mathf.Deg2Rad * fov * 0.5f);
			matrix[1,1] = 1 / Mathf.Tan(Mathf.Deg2Rad * fov * 0.5f);
			matrix[2,2] = far / (far - near);
			matrix[2,3] = (near * far) / (near - far);
			matrix[3,2] = 1f;
			//matrix.SetRow(0, new Vector4(focalLength, 0f, 0f, 0f));
			//matrix.SetRow(1, new Vector4(0f, focalLength, 0f, 0f));
			//matrix.SetRow(2, new Vector4(0f, 0f, 0f, 0f));
			//matrix.SetRow(3, new Vector4(0f, 0f, 1f, 0f));
			return matrix;

		}
	}

	private void OnDrawGizmos()
	{
		var originMatrix = Gizmos.matrix;
		//Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawFrustum(transform.localPosition + Vector3.forward * near, fov, far, near, aspect);
		Gizmos.matrix = originMatrix;
		Gizmos.DrawWireCube(transform.position, Vector3.one * 2);
	}
}