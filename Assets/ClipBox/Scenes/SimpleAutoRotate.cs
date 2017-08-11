using UnityEngine;

public class SimpleAutoRotate : MonoBehaviour
{
	public Vector3 rotateSpeed;

	void Update()
	{
		transform.Rotate(rotateSpeed * Time.deltaTime);
	}
}
