using UnityEngine;

namespace PostProcessing
{
	[ExecuteInEditMode, ImageEffectAllowedInSceneView]
	public class BloomEffect : MonoBehaviour
	{
		[Range(1, 16)]
		public int iterations = 1;

		private RenderTexture[] textures = new RenderTexture[16];

		void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			var width = src.width;
			var height = src.height;
			var format = src.format;
			RenderTexture currentDest = textures[0] = RenderTexture.GetTemporary(width, height, 0, format);
			Graphics.Blit(src, currentDest);
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
				Graphics.Blit(currentSrc, currentDest);
				currentSrc = currentDest;
			}

			textures[i-1] = null;
			for (i -= 2; i >= 0; i--)
			{
				currentDest = textures[i];
				textures[i] = null;
				Graphics.Blit(currentSrc, currentDest);
				RenderTexture.ReleaseTemporary(currentSrc);
				currentSrc = currentDest;
			}

			Graphics.Blit(currentSrc, dest);
			RenderTexture.ReleaseTemporary(currentSrc);
		}
	}
}