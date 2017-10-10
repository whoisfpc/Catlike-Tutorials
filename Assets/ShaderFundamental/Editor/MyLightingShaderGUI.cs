using UnityEditor;
using UnityEngine;

public class MyLightingShaderGUI : ShaderGUI
{
	private static GUIContent staticLabel = new GUIContent();
	private MaterialEditor editor;
	private MaterialProperty[] properties;

	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
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
		editor.TextureScaleOffsetProperty(mainTex);
	}

	private void DoMetallic()
	{
		MaterialProperty slider = FindProperty("_Metallic");
		EditorGUI.indentLevel += 2;
		editor.ShaderProperty(slider, MakeLabel(slider));
		EditorGUI.indentLevel -= 2;
	}

	private void DoSmoothness()
	{
		MaterialProperty slider = FindProperty("_Smoothness");
		EditorGUI.indentLevel += 2;
		editor.ShaderProperty(slider, MakeLabel(slider));
		EditorGUI.indentLevel -= 2;
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

	private MaterialProperty FindProperty(string name)
	{
		return FindProperty(name, properties);
	}

	private static GUIContent MakeLabel (MaterialProperty property, string tooltip = null)
	{
		staticLabel.text = property.displayName;
		staticLabel.tooltip = tooltip;
		return staticLabel;
	}
}
