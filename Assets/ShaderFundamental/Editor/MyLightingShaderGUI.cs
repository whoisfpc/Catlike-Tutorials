using UnityEditor;
using UnityEngine;

public class MyLightingShaderGUI : ShaderGUI
{
	private enum SmoothnessSource
	{
		Uniform, Albedo, Metallic
	}
	private static GUIContent staticLabel = new GUIContent();
	private static ColorPickerHDRConfig emissionConfig = new ColorPickerHDRConfig(0f, 99f, 1f / 99f, 3f);
	private Material target;
	private MaterialEditor editor;
	private MaterialProperty[] properties;

	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		this.target = materialEditor.target as Material;
		this.editor = materialEditor;
		this.properties = properties;
		DoMain();
		DoSecondary();
	}

	private void DoMain()
	{
		GUILayout.Label("Main Maps", EditorStyles.boldLabel);
		MaterialProperty mainTex = FindProperty("_MainTex");
		editor.TexturePropertySingleLine(MakeLabel(mainTex, "Albedo (RGB)"), mainTex, FindProperty("_Tint"));
		DoMetallic();
		DoSmoothness();
		DoNormals();
		DoEmission();
		editor.TextureScaleOffsetProperty(mainTex);
	}

	private void DoMetallic()
	{
		MaterialProperty map = FindProperty("_MetallicMap");
		EditorGUI.BeginChangeCheck();
		editor.TexturePropertySingleLine(
			MakeLabel(map, "Metallic (R)"), map,
			map.textureValue ? null : FindProperty("_Metallic")
		);
		if (EditorGUI.EndChangeCheck())
		{
			SetKeyword("_METALLIC_MAP", map.textureValue);
		}
	}

	private void DoEmission()
	{
		MaterialProperty map = FindProperty("_EmissionMap");
		EditorGUI.BeginChangeCheck();
		editor.TexturePropertyWithHDRColor(
			MakeLabel(map, "Emission (RGB)"), map,
			FindProperty("_Emission"),
			emissionConfig,
			false
		);
		if (EditorGUI.EndChangeCheck())
		{
			SetKeyword("_EMISSION_MAP", map.textureValue);
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
		MaterialProperty scaleProperty = map.textureValue ? FindProperty("_BumpScale") : null;
		editor.TexturePropertySingleLine(MakeLabel(map), map, scaleProperty);
	}

	private void DoSecondary()
	{
		GUILayout.Label("Secondary Maps", EditorStyles.boldLabel);

		MaterialProperty detailTex = FindProperty("_DetailTex");
		editor.TexturePropertySingleLine(
			MakeLabel(detailTex, "Albedo (RGB) multiplied by 2"), detailTex
		);
		DoSecondaryNormals();
		editor.TextureScaleOffsetProperty(detailTex);
	}

	private void DoSecondaryNormals()
	{
		MaterialProperty map = FindProperty("_DetailNormalMap");
		MaterialProperty scaleProperty = map.textureValue ? FindProperty("_DetailBumpScale") : null;
		editor.TexturePropertySingleLine(MakeLabel(map), map, scaleProperty);
	}

	private void RecordAction(string label)
	{
		editor.RegisterPropertyChangeUndo(label);
	}

	private bool IsKeywordEnabled(string keyword)
	{
		return target.IsKeywordEnabled(keyword);
	}

	private void SetKeyword(string keyword, bool state)
	{
		if (state)
		{
			target.EnableKeyword(keyword);
		}
		else
		{
			target.DisableKeyword(keyword);
		}
	}

	private MaterialProperty FindProperty(string name)
	{
		return FindProperty(name, properties);
	}

	private static GUIContent MakeLabel(string name, string tooltip = null)
	{
		staticLabel.text = name;
		staticLabel.tooltip = tooltip;
		return staticLabel;
	}

	private static GUIContent MakeLabel(MaterialProperty property, string tooltip = null)
	{
		staticLabel.text = property.displayName;
		staticLabel.tooltip = tooltip;
		return staticLabel;
	}
}
