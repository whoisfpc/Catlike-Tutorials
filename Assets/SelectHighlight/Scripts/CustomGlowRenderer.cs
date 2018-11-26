using UnityEngine;
using UnityEngine.Rendering;

namespace SelectHighlight
{
    public class CustomGlowRenderer : MonoBehaviour
    {
        private CommandBuffer glowBuffer;
        private Camera cam;
        private RenderTexture rt;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            rt = new RenderTexture(Screen.width, Screen.height, 24);
            rt.filterMode = FilterMode.Bilinear;
        }

        private void OnDisable()
        {
            Cleanup();
        }

        private void OnEnable()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (glowBuffer != null)
            {
                cam.RemoveCommandBuffer(CameraEvent.BeforeLighting, glowBuffer);
            }
        }

        private void OnPreRender()
        {
            if (!cam)
                return;
            if (glowBuffer != null)
            {
                glowBuffer.Clear();
            }
            else
            {
                glowBuffer = new CommandBuffer();
                glowBuffer.name = "Glow map buffer";
                cam.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, glowBuffer);
            }

            var glowSystem = CustomGlowSystem.Instance;


            // create render texture for glow map
            //int tempID = Shader.PropertyToID("_Temp1");
            //glowBuffer.GetTemporaryRT(tempID, -1, -1, 24, FilterMode.Bilinear);
            glowBuffer.SetRenderTarget(rt);
            glowBuffer.ClearRenderTarget(true, true, Color.black);

            foreach (CustomGlowObj o in glowSystem.glowObjSet)
            {
                Renderer r = o.GetComponent<Renderer>();
                Material glowMat = o.glowMaterial;
                if (r && glowMat)
                {
                    glowBuffer.DrawRenderer(r, glowMat);
                }
            }

            // set render texture as globally accessable 'glow map' texture
            glowBuffer.SetGlobalTexture("_GlowMap", rt);

        }
    }
}
