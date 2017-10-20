﻿using UnityEngine;

public class EmissiveOscillator : MonoBehaviour
{
	private MeshRenderer emissiveRenderer;
	private Material emissiveMaterial;

	void Start()
	{
		emissiveRenderer = GetComponent<MeshRenderer>();
		emissiveMaterial = emissiveRenderer.material;
	}
	
	void Update()
	{
		Color c = Color.Lerp(
			Color.white, Color.black,
			Mathf.Sin(Time.time * Mathf.PI) * 0.5f + 0.5f
		);
		emissiveMaterial.SetColor("_Emission", c);
		DynamicGI.SetEmissive(emissiveRenderer, c);
	}
}
