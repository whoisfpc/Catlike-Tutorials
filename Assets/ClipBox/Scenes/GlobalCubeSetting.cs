using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GlobalCubeSetting : MonoBehaviour {

    public Vector3 boxSize;

	// Use this for initialization
	void Start () {
        Shader.SetGlobalVector("_BoxSize", new Vector4(boxSize.x, boxSize.y, boxSize.z, 1));
        Shader.SetGlobalVector("_BoxRotation", new Vector4(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z, 1));
        Shader.SetGlobalVector("_Origin", new Vector4(transform.position.x, transform.position.y, transform.position.z, 1));
	}

    private void Update()
    {
        Shader.SetGlobalVector("_BoxSize", new Vector4(boxSize.x, boxSize.y, boxSize.z, 1));
        Shader.SetGlobalVector("_BoxRotation", new Vector4(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z, 1));
        Shader.SetGlobalVector("_Origin", new Vector4(transform.position.x, transform.position.y, transform.position.z, 1));
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale); 
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}
