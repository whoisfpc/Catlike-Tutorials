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

		/// <summary>
		/// Trims the algorithm from processing darks.
		///   0.0833 - upper limit (default, the start of visible unfiltered edges)
		///   0.0625 - high quality (faster)
		///   0.0312 - visible limit (slower)
		/// </summary>
		[Range(0.0312f, 0.0833f)]
		[Tooltip("Trims the algorithm from processing darks.")]
		public float contrastThreshold = 0.0312f;

		/// <summary>
		/// The minimum amount of local contrast required to apply algorithm.
		///   0.333 - too little (faster)
		///   0.250 - low quality
		///   0.166 - default
		///   0.125 - high quality 
		///   0.063 - overkill (slower)
		/// </summary>
		[Range(0.063f, 0.333f)]
		[Tooltip("The minimum amount of local contrast required to apply algorithm.")]
		public float relativeThreshold = 0.063f;

		/// <summary>
		/// Choose the amount of sub-pixel aliasing removal.
		/// This can effect sharpness.
		///   1.00 - upper limit (softer)
		///   0.75 - default amount of filtering
		///   0.50 - lower limit (sharper, less sub-pixel aliasing removal)
		///   0.25 - almost off
		///   0.00 - completely off
		/// </summary>
		[Range(0f, 1f)]
		public float subpixelBlending = 1f;

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

			fxaaMaterial.SetFloat("_ContrastThreshold", contrastThreshold);
			fxaaMaterial.SetFloat("_RelativeThreshold", relativeThreshold);
			fxaaMaterial.SetFloat("_SubpixelBlending", subpixelBlending);

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
