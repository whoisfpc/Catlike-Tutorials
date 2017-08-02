using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class Stuff : PooledObject
{
	public Rigidbody Body { get; private set; }
	MeshRenderer[] meshRenderers;

	void Awake()
	{
		Body = GetComponent<Rigidbody>();
		meshRenderers = GetComponentsInChildren<MeshRenderer>();
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Kill Zone"))
		{
			ReturnToPool();
		}
	}
	public void SetMaterial (Material m) {
		foreach (var meshRenderer in meshRenderers)
		{
			meshRenderer.material = m;
		}
	}

	void OnEnable()
	{
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnLevelFinishedLoading;
	}

	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		ReturnToPool();
	}
}
