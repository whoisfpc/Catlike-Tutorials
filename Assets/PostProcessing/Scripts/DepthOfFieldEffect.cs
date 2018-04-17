using System;
using UnityEngine;

namespace PostProcessing
{
	[ExecuteInEditMode, ImageEffectAllowedInSceneView]
	public class DepthOfFieldEffect : MonoBehaviour
	{
		[HideInInspector]
		public Shader dofShader;
		[Range(0.1f, 100f)]
		public float focusDistance = 10f;
		[Range(0.1f, 10f)]
		public float focusRange = 3f;

		[NonSerialized]
		private Material dofMaterial;

		private const int circleOfConfusionPass = 0;


		void OnRenderImage (RenderTexture src, RenderTexture dest)
		{
			if (dofMaterial == null)
			{
				dofMaterial = new Material(dofShader);
				dofMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			dofMaterial.SetFloat("_FocusDistance", focusDistance);
			dofMaterial.SetFloat("_FocusRange", focusRange);

			RenderTexture coc = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
			Graphics.Blit(src, coc, dofMaterial, circleOfConfusionPass);
			Graphics.Blit(coc, dest);
			RenderTexture.ReleaseTemporary(coc);
		}
	}
}
