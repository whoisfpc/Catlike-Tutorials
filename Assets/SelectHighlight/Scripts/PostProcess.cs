﻿using UnityEngine;

namespace SelectHighlight
{
    public class PostProcess : MonoBehaviour
    {
        public Material material;
        private Camera cam;

        void Start()
        {
            cam = GetComponent<Camera>();
            cam.depthTextureMode = DepthTextureMode.Depth;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, material);
        }
    }
}
