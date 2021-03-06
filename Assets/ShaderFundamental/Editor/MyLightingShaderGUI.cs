﻿using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class MyLightingShaderGUI : MyBaseShaderGUI
{
	private enum SmoothnessSource
	{
		Uniform, Albedo, Metallic
	}

	private enum RenderingMode
	{
		Opaque, Cutout, Fade, Transparent
	}

	private enum TessellationMode
	{
		Uniform, Edge
	}

	private struct RenderingSettings
	{
		public RenderQueue queue;
		public string renderType;
		public BlendMode srcBlend, dstBlend;
		public bool zWrite;

		public static RenderingSettings[] modes =
		{
			new RenderingSettings()
			{
				queue = RenderQueue.Geometry,
				renderType = "",
				srcBlend = BlendMode.One,
				dstBlend = BlendMode.Zero,
				zWrite = true
			},
			new RenderingSettings()
			{
				queue = RenderQueue.AlphaTest,
				renderType = "TransparentCutout",
				srcBlend = BlendMode.One,
				dstBlend = BlendMode.Zero,
				zWrite = true
			},
			new RenderingSettings()
			{
				queue = RenderQueue.Transparent,
				renderType = "Transparent",
				srcBlend = BlendMode.SrcAlpha,
				dstBlend = BlendMode.OneMinusSrcAlpha,
				zWrite = false
			},
			new RenderingSettings()
			{
				queue = RenderQueue.Transparent,
				renderType = "Transparent",
				srcBlend = BlendMode.One,
				dstBlend = BlendMode.OneMinusSrcAlpha,
				zWrite = false
			}
		};
	}
	private bool shouldShowAlphaCutoff;

	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		base.OnGUI(materialEditor, properties);
		DoRenderingMode();
		if (target.HasProperty("_TessellationUniform"))
		{
			DoTessellation();
		}
		if (target.HasProperty("_WireframeColor"))
		{
			DoWireframe();
		}
		DoMain();
		DoSecondary();
		DoAdvanced();
	}

	private void DoRenderingMode()
	{
		RenderingMode mode = RenderingMode.Opaque;
		shouldShowAlphaCutoff = false;
		if (IsKeywordEnabled("_RENDERING_CUTOUT"))
		{
			mode = RenderingMode.Cutout;
			shouldShowAlphaCutoff = true;
		}
		else if (IsKeywordEnabled("_RENDERING_FADE"))
		{
			mode = RenderingMode.Fade;
		}
		else if (IsKeywordEnabled("_RENDERING_TRANSPARENT"))
		{
			mode = RenderingMode.Transparent;
		}

		EditorGUI.BeginChangeCheck();
		mode = (RenderingMode)EditorGUILayout.EnumPopup(MakeLabel("Rendering Mode"), mode);
		if (EditorGUI.EndChangeCheck()) {
			RecordAction("Rendering Mode");
			SetKeyword("_RENDERING_CUTOUT", mode == RenderingMode.Cutout);
			SetKeyword("_RENDERING_FADE", mode == RenderingMode.Fade);
			SetKeyword("_RENDERING_TRANSPARENT", mode == RenderingMode.Transparent);
			RenderingSettings settings = RenderingSettings.modes[(int)mode];
			foreach (Material mat in editor.targets)
			{
				mat.renderQueue = (int)settings.queue;
				mat.SetOverrideTag("RenderType", settings.renderType);
				mat.SetInt("_SrcBlend", (int)settings.srcBlend);
				mat.SetInt("_DstBlend", (int)settings.dstBlend);
				mat.SetInt("_ZWrite", settings.zWrite ? 1 : 0);
			}
		}
		if (mode == RenderingMode.Fade || mode == RenderingMode.Transparent)
		{
			DoSemitransparentShadows();
		}
	}

	private void DoSemitransparentShadows()
	{
		EditorGUI.BeginChangeCheck();
		bool semitransparentShadows = EditorGUILayout.Toggle
		(
			MakeLabel("Semitransp. Shadows", "Semitransparent Shadows"),
			IsKeywordEnabled("_SEMITRANSPARENT_SHADOWS")
		);
		if (EditorGUI.EndChangeCheck())
		{
			SetKeyword("_SEMITRANSPARENT_SHADOWS", semitransparentShadows);
		}
		if (!semitransparentShadows)
		{
			shouldShowAlphaCutoff = true;
		}
	}

	private void DoTessellation ()
	{
		GUILayout.Label("Tessellation", EditorStyles.boldLabel);
		EditorGUI.indentLevel += 2;
		TessellationMode mode = TessellationMode.Uniform;
		if (IsKeywordEnabled("_TESSELLATION_EDGE"))
		{
			mode = TessellationMode.Edge;
		}
		EditorGUI.BeginChangeCheck();
		mode = (TessellationMode)EditorGUILayout.EnumPopup(MakeLabel("Mode"), mode);
		if (EditorGUI.EndChangeCheck())
		{
			RecordAction("Tessellation Mode");
			SetKeyword("_TESSELLATION_EDGE", mode == TessellationMode.Edge);
		}
		if (mode == TessellationMode.Uniform)
		{
			editor.ShaderProperty(
				FindProperty("_TessellationUniform"),
				MakeLabel("Uniform")
			);
		}
		else
		{
			editor.ShaderProperty(
				FindProperty("_TessellationEdgeLength"),
				MakeLabel("Edge Length")
			);
		}
		EditorGUI.indentLevel -= 2;
	}

	private void DoWireframe()
	{
		GUILayout.Label("Wireframe", EditorStyles.boldLabel);
		EditorGUI.indentLevel += 2;
		editor.ShaderProperty(
			FindProperty("_WireframeColor"),
			MakeLabel("Color")
		);
		editor.ShaderProperty(
			FindProperty("_WireframeSmoothing"),
			MakeLabel("Smoothing", "In screen space.")
		);
		editor.ShaderProperty(
			FindProperty("_WireframeThickness"),
			MakeLabel("Thickness", "In screen space.")
		);
		EditorGUI.indentLevel -= 2;
	}

	private void DoMain()
	{
		GUILayout.Label("Main Maps", EditorStyles.boldLabel);
		MaterialProperty mainTex = FindProperty("_MainTex");
		editor.TexturePropertySingleLine(MakeLabel(mainTex, "Albedo (RGB)"), mainTex, FindProperty("_Color"));
		if (shouldShowAlphaCutoff)
		{
			DoAplha();
		}
		DoMetallic();
		DoSmoothness();
		DoNormals();
		DoParallax();
		DoOcclusion();
		DoEmission();
		DoDetailMask();
		editor.TextureScaleOffsetProperty(mainTex);
	}

	private void DoAplha()
	{
		MaterialProperty slider = FindProperty("_Cutoff");
		EditorGUI.indentLevel += 2;
		editor.ShaderProperty(slider, MakeLabel(slider));
		EditorGUI.indentLevel -= 2;
	}

	private void DoDetailMask()
	{
		MaterialProperty mask = FindProperty("_DetailMask");
		EditorGUI.BeginChangeCheck();
		editor.TexturePropertySingleLine(MakeLabel(mask, "Detail Mask (A)"), mask);
		if (EditorGUI.EndChangeCheck())
		{
			SetKeyword("_DETAIL_MASK", mask.textureValue);
		}
	}

	private void DoParallax()
	{
		MaterialProperty map = FindProperty("_ParallaxMap");
		Texture tex= map.textureValue;
		EditorGUI.BeginChangeCheck();
		editor.TexturePropertySingleLine(
			MakeLabel(map, "Parallax (G)"), map,
			tex ? FindProperty("_ParallaxStrength") : null
		);
		if (EditorGUI.EndChangeCheck() && tex != map.textureValue)
		{
			SetKeyword("_PARALLAX_MAP", map.textureValue);
		}
	}

	private void DoOcclusion()
	{
		MaterialProperty map = FindProperty("_OcclusionMap");
		Texture tex= map.textureValue;
		EditorGUI.BeginChangeCheck();
		editor.TexturePropertySingleLine(
			MakeLabel(map, "Occlusion (G)"), map,
			tex ? FindProperty("_OcclusionStrength") : null
		);
		if (EditorGUI.EndChangeCheck() && tex != map.textureValue)
		{
			SetKeyword("_OCCLUSION_MAP", map.textureValue);
		}
	}

	private void DoMetallic()
	{
		MaterialProperty map = FindProperty("_MetallicMap");
		Texture tex = map.textureValue;
		EditorGUI.BeginChangeCheck();
		editor.TexturePropertySingleLine(
			MakeLabel(map, "Metallic (R)"), map,
			tex ? null : FindProperty("_Metallic")
		);
		if (EditorGUI.EndChangeCheck() && tex != map.textureValue)
		{
			SetKeyword("_METALLIC_MAP", map.textureValue);
		}
	}

	private void DoEmission()
	{
		MaterialProperty map = FindProperty("_EmissionMap");
		Texture tex = map.textureValue;
		EditorGUI.BeginChangeCheck();
		editor.TexturePropertyWithHDRColor(
			MakeLabel(map, "Emission (RGB)"), map,
			FindProperty("_Emission"), false
		);
		editor.LightmapEmissionProperty(2);
		if (EditorGUI.EndChangeCheck())
		{
			if (tex != map.textureValue)
			{
				SetKeyword("_EMISSION_MAP", map.textureValue);
			}
			foreach (Material mat in editor.targets)
			{
				mat.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
			}
		}
	}

	private void DoSmoothness()
	{
		SmoothnessSource source = SmoothnessSource.Uniform;
		if (IsKeywordEnabled("_SMOOTHNESS_ALBEDO"))
		{
			source = SmoothnessSource.Albedo;
		}
		else if (IsKeywordEnabled("_SMOOTHNESS_METALLIC"))
		{
			source = SmoothnessSource.Metallic;
		}
		MaterialProperty slider = FindProperty("_Smoothness");
		EditorGUI.indentLevel += 2;
		editor.ShaderProperty(slider, MakeLabel(slider));
		EditorGUI.indentLevel += 1;
		EditorGUI.BeginChangeCheck();
		source = (SmoothnessSource)EditorGUILayout.EnumPopup(MakeLabel("Source"), source);
		if (EditorGUI.EndChangeCheck())
		{
			RecordAction("Smoothness Source");
			SetKeyword("_SMOOTHNESS_ALBEDO", source == SmoothnessSource.Albedo);
			SetKeyword("_SMOOTHNESS_METALLIC", source == SmoothnessSource.Metallic);
		}
		EditorGUI.indentLevel -= 3;
	}

	private void DoNormals()
	{
		MaterialProperty map = FindProperty("_NormalMap");
		Texture tex = map.textureValue;
		MaterialProperty scaleProperty = tex ? FindProperty("_BumpScale") : null;
		EditorGUI.BeginChangeCheck();
		editor.TexturePropertySingleLine(MakeLabel(map), map, scaleProperty);
		if (EditorGUI.EndChangeCheck() && tex != map.textureValue)
		{
			SetKeyword("_NORMAL_MAP", map.textureValue);
		}
	}

	private void DoSecondary()
	{
		GUILayout.Label("Secondary Maps", EditorStyles.boldLabel);

		MaterialProperty detailTex = FindProperty("_DetailTex");
		EditorGUI.BeginChangeCheck();
		editor.TexturePropertySingleLine(
			MakeLabel(detailTex, "Albedo (RGB) multiplied by 2"), detailTex
		);
		if (EditorGUI.EndChangeCheck())
		{
			SetKeyword("_DETAIL_ALBEDO_MAP", detailTex.textureValue);
		}
		DoSecondaryNormals();
		editor.TextureScaleOffsetProperty(detailTex);
	}

	private void DoSecondaryNormals()
	{
		MaterialProperty map = FindProperty("_DetailNormalMap");
		Texture tex = map.textureValue;
		MaterialProperty scaleProperty = tex ? FindProperty("_DetailBumpScale") : null;
		EditorGUI.BeginChangeCheck();
		editor.TexturePropertySingleLine(MakeLabel(map), map, scaleProperty);
		if (EditorGUI.EndChangeCheck() && tex != map.textureValue)
		{
			SetKeyword("_DETAIL_NORMAL_MAP", map.textureValue);
		}
	}

	private void DoAdvanced()
	{
		GUILayout.Label("Advanced Options", EditorStyles.boldLabel);
		editor.EnableInstancingField();
	}
}
