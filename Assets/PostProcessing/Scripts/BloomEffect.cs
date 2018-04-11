using UnityEngine;
using System;

namespace PostProcessing
{
	[ExecuteInEditMode, ImageEffectAllowedInSceneView]
	public class BloomEffect : MonoBehaviour
	{
		public Shader bloomShader;
		[Range(1, 16)]
		public int iterations = 1;
		[Range(0, 10)]
		public float threshold = 1;
		public bool debug;

		private const int BoxDownPrefilterPass  = 0;
		private const int BoxDownPass = 1;
		private const int BoxUpPass = 2;
		private const int ApplyBloomPass = 3;
		private const int DebugBloomPass = 4;

		[NonSerialized]
		private Material bloom;
		private RenderTexture[] textures = new RenderTexture[16];

		void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			if (bloom == null)
			{
				bloom = new Material(bloomShader);
				bloom.hideFlags = HideFlags.HideAndDontSave;
			}
			bloom.SetFloat("_Threshold", threshold);
			var width = src.width;
			var height = src.height;
			var format = src.format;
			RenderTexture currentDest = textures[0] = RenderTexture.GetTemporary(width, height, 0, format);
			Graphics.Blit(src, currentDest, bloom, BoxDownPrefilterPass);
			RenderTexture currentSrc = currentDest;

			int i = 1;
			for (; i < iterations; i++) {
				width /= 2;
				height /= 2;
				if (height < 2)
				{
					break;
				}

				currentDest = textures[i] = RenderTexture.GetTemporary(width, height, 0, format);
				Graphics.Blit(currentSrc, currentDest, bloom, BoxDownPass);
				currentSrc = currentDest;
			}

			textures[i-1] = null;
			for (i -= 2; i >= 0; i--)
			{
				currentDest = textures[i];
				textures[i] = null;
				Graphics.Blit(currentSrc, currentDest, bloom, BoxUpPass);
				RenderTexture.ReleaseTemporary(currentSrc);
				currentSrc = currentDest;
			}

			if (debug)
			{
				Graphics.Blit(currentSrc, dest, bloom, DebugBloomPass);
			}
			else
			{
				bloom.SetTexture("_SourceTex", src);
				Graphics.Blit(currentSrc, dest, bloom, ApplyBloomPass);
			}
			RenderTexture.ReleaseTemporary(currentSrc);
		}
	}
}