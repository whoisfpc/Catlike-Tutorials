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
            {
                blurObj.ToAdd();
                blurObjSet.Add(blurObj);
            }
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
            blurObj.ToAdd();
            blurObjSet.Add(blurObj);
        }

        public Shader depthOnlyShader;
        private CommandBuffer blurBuffer;
        private Camera cam;
        private Camera depthCam;
        private RenderTexture rt, dt, wt;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            rt = new RenderTexture(Screen.width, Screen.height, 24);
            rt.filterMode = FilterMode.Bilinear;
            dt = new RenderTexture(rt);
            wt = new RenderTexture(rt);
        }

        private void OnDestroy()
        {
            rt?.Release();
            dt?.Release();
            wt?.Release();
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
                cam.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, blurBuffer);
            }
        }

        private void OnPreRender()
        {
            if (!cam)
            {
                return;
            }
            if (depthCam == null)
            {
                GameObject go = new GameObject("depthCam");
                depthCam = go.AddComponent<Camera>();
                depthCam.enabled = false;
                go.hideFlags = HideFlags.DontSave;
            }
            depthCam.CopyFrom(cam);
            depthCam.renderingPath = RenderingPath.Forward;
            depthCam.SetTargetBuffers(rt.colorBuffer, rt.depthBuffer);
            depthCam.RenderWithShader(depthOnlyShader, "");
            if (blurBuffer != null)
            {
                blurBuffer.Clear();
            }
            else
            {
                blurBuffer = new CommandBuffer();
                blurBuffer.name = "Blur map buffer";
                cam.AddCommandBuffer(CameraEvent.BeforeImageEffects, blurBuffer);
            }

            blurBuffer.SetRenderTarget(rt);
            blurBuffer.ClearRenderTarget(false, true, Color.clear);

            foreach (BlurObj o in blurObjSet)
            {
                Renderer r = o.GetComponent<Renderer>();
                if (r)
                {
                    blurBuffer.DrawRenderer(r, r.sharedMaterial);
                }
            }
        }

        const int BoxDownPass = 0;
        const int BoxUpPass = 1;

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

        public Shader combineShader;
        [System.NonSerialized]
        Material combine;

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
            BlurImage(rt, dt);
            // set render texture as globally accessable 'blur map' texture
            Shader.SetGlobalTexture("_BlurMap", dt);
            Graphics.Blit(source, destination, combine);
        }
    }
}
