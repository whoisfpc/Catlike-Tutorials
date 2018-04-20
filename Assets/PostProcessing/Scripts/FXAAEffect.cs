using System;
using UnityEngine;

namespace PostProcessing
{
	[ExecuteInEditMode, ImageEffectAllowedInSceneView]
	public class FXAAEffect : MonoBehaviour
	{
		[HideInInspector]
		public Shader fxaaShader;

		public enum LuminanceMode { Alpha, Green, Calculate }

		public LuminanceMode luminanceSource;

		[NonSerialized]
		private Material fxaaMaterial;

		private const int luminancePass = 0;
		private const int fxaaPass = 1;

		void OnRenderImage (RenderTexture src, RenderTexture dest)
		{
			if (fxaaMaterial == null)
			{
				fxaaMaterial = new Material(fxaaShader);
				fxaaMaterial.hideFlags = HideFlags.HideAndDontSave;
			}

			if (luminanceSource == LuminanceMode.Calculate)
			{
				fxaaMaterial.DisableKeyword("LUMINANCE_GREEN");
				var luminanceTex = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
				Graphics.Blit(src, luminanceTex, fxaaMaterial, luminancePass);
				Graphics.Blit(luminanceTex, dest, fxaaMaterial, fxaaPass);
				RenderTexture.ReleaseTemporary(luminanceTex);
			}
			else
			{
				if (luminanceSource == LuminanceMode.Green)
				{
					fxaaMaterial.EnableKeyword("LUMINANCE_GREEN");
				}
				else
				{
					fxaaMaterial.DisableKeyword("LUMINANCE_GREEN");
				}
				Graphics.Blit(src, dest, fxaaMaterial, fxaaPass);
			}
		}
	}
}
