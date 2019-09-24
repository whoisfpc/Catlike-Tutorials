using UnityEngine;

namespace SelectBlur
{
    public class PostProcess : MonoBehaviour
    {
        [HideInInspector]
        public Shader combineShader;
        [System.NonSerialized]
        Material combine;
        private Camera cam;

        void Start()
        {
            cam = GetComponent<Camera>();
            cam.depthTextureMode = DepthTextureMode.Depth;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (combine == null) {
                combine = new Material(combineShader);
                combine.hideFlags = HideFlags.HideAndDontSave;
            }
            Graphics.Blit(source, destination, combine);
        }
    }
}
