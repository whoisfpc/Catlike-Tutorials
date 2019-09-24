using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SelectBlur
{
    public class BlurSystem : MonoBehaviour
    {

        public static HashSet<BlurObj> blurObjSet = new HashSet<BlurObj>();

        public static void Add(BlurObj blurObj)
        {
            if (!blurObjSet.Contains(blurObj))
                blurObjSet.Add(blurObj);
        }

        public static void Remove(BlurObj blurObj)
        {
            if (blurObjSet.Contains(blurObj))
            {
                blurObj.ToRemove();
                blurObjSet.Remove(blurObj);
            }
        }

        public static void PresentOnlyOne(BlurObj blurObj)
        {
            foreach (var obj in blurObjSet)
            {
                obj.ToRemove();
            }
            blurObjSet.Clear();
            blurObjSet.Add(blurObj);
        }

        private CommandBuffer blurBuffer;
        private Camera cam;
        private RenderTexture rt, dt;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            rt = new RenderTexture(Screen.width, Screen.height, 24);
            rt.filterMode = FilterMode.Bilinear;
            dt = new RenderTexture(Screen.width, Screen.height, 24);
            dt.filterMode = FilterMode.Bilinear;
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
            if (blurBuffer != null)
            {
                cam.RemoveCommandBuffer(CameraEvent.BeforeLighting, blurBuffer);
            }
        }

        private void OnPreRender()
        {
            if (!cam)
                return;
            if (blurBuffer != null)
            {
                blurBuffer.Clear();
            }
            else
            {
                blurBuffer = new CommandBuffer();
                blurBuffer.name = "Blur map buffer";
                cam.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, blurBuffer);
            }

            blurBuffer.SetRenderTarget(rt);
            blurBuffer.ClearRenderTarget(true, true, Color.black);

            foreach (BlurObj o in blurObjSet)
            {
                Renderer r = o.GetComponent<Renderer>();
                if (r)
                {
                    blurBuffer.DrawRenderer(r, r.sharedMaterial);
                }
            }

            // set render texture as globally accessable 'blur map' texture
            BlurImage(rt, dt);
            blurBuffer.SetGlobalTexture("_BlurMap", dt);

        }

        const int BoxDownPass = 0;
        const int BoxUpPass = 1;

        [HideInInspector]
        public Shader blurShader;

        [Range(1, 16)]
        public int iterations = 4;

        RenderTexture[] textures = new RenderTexture[16];

        [System.NonSerialized]
        Material blur;

        void BlurImage (RenderTexture source, RenderTexture destination) {
            if (blur == null) {
                blur = new Material(blurShader);
                blur.hideFlags = HideFlags.HideAndDontSave;
            }

            int width = source.width / 2;
            int height = source.height / 2;
            RenderTextureFormat format = source.format;

            RenderTexture currentDestination = textures[0] =
                RenderTexture.GetTemporary(width, height, 0, format);
            Graphics.Blit(source, currentDestination, blur, BoxDownPass);
            RenderTexture currentSource = currentDestination;

            int i = 1;
            for (; i < iterations; i++) {
                width /= 2;
                height /= 2;
                if (height < 2) {
                    break;
                }
                currentDestination = textures[i] =
                    RenderTexture.GetTemporary(width, height, 0, format);
                Graphics.Blit(currentSource, currentDestination, blur, BoxDownPass);
                currentSource = currentDestination;
            }

            for (i -= 2; i >= 0; i--) {
                currentDestination = textures[i];
                textures[i] = null;
                Graphics.Blit(currentSource, currentDestination, blur, BoxUpPass);
                RenderTexture.ReleaseTemporary(currentSource);
                currentSource = currentDestination;
            }

            Graphics.Blit(currentSource, destination, blur, BoxUpPass);
            RenderTexture.ReleaseTemporary(currentSource);
        }
    }
}
