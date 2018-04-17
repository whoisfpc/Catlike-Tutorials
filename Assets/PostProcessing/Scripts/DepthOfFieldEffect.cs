﻿using System;
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
		[Range(1f, 10f)]
		public float bokehRadius = 4f;

		[NonSerialized]
		private Material dofMaterial;

		private const int circleOfConfusionPass = 0;
		private const int preFilterPass = 1;
		private const int bokehPass = 2;
		private const int postFilterPass = 3;
		private const int combinePass = 4;

		void OnRenderImage (RenderTexture src, RenderTexture dest)
		{
			if (dofMaterial == null)
			{
				dofMaterial = new Material(dofShader);
				dofMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			dofMaterial.SetFloat("_BokehRadius", bokehRadius);
			dofMaterial.SetFloat("_FocusDistance", focusDistance);
			dofMaterial.SetFloat("_FocusRange", focusRange);

			RenderTexture coc = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
			int width = src.width / 2;
			int height = src.height / 2;
			RenderTextureFormat format = src.format;
			RenderTexture dof0 = RenderTexture.GetTemporary(width, height, 0, format);
			RenderTexture dof1 = RenderTexture.GetTemporary(width, height, 0, format);
			dofMaterial.SetTexture("_CoCTex", coc);
			dofMaterial.SetTexture("_DoFTex", dof0);

			Graphics.Blit(src, coc, dofMaterial, circleOfConfusionPass);
			Graphics.Blit(src, dof0, dofMaterial, preFilterPass);
			Graphics.Blit(dof0, dof1, dofMaterial, bokehPass);
			Graphics.Blit(dof1, dof0, dofMaterial, postFilterPass);
			Graphics.Blit(src, dest, dofMaterial, combinePass);

			RenderTexture.ReleaseTemporary(coc);
			RenderTexture.ReleaseTemporary(dof0);
			RenderTexture.ReleaseTemporary(dof1);
		}
	}
}
