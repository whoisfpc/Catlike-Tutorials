using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeEditor : EditorWindow
{
	Rect windowRect = new Rect(0, 0, 200, 200);
	Rect scaleRect;
	float zoomScale = 1f;
	Rect areaRect = new Rect(100, 100, 400, 400);


	[MenuItem("NodeEditor/NodeEditor")]
	static void CreateWindow()
	{
		var window = GetWindow<NodeEditor>();
		window.Show();
	}

	void OnGUI()
	{
		DrawPoint(Vector2.one * 400, Color.green);
		DrawPoint(Vector2.one * 300, Color.green);
		DrawPoint(Vector2.one * 200, Color.green);
		DrawPoint(Vector2.one * 100, Color.green);
		DrawPoint(Vector2.one * 90, Color.green);
		DrawPoint(Vector2.one * 80, Color.green);
		DrawPoint(Vector2.one * 70, Color.green);
		DrawPoint(Vector2.one * 60, Color.green);
		DrawPoint(Vector2.one * 50, Color.green);
		DrawPoint(Vector2.one * 30, Color.green);

		DrawRectangle(new Rect(0, 0, position.size.x, 50), Color.black, 2);

		GUILayout.BeginArea(new Rect(0, 0, position.size.x, 50));
		zoomScale = EditorGUI.Slider(new Rect(5, 5, 150, 20), zoomScale, 0.25f, 4f);
		GUILayout.EndArea();

		GUI.EndGroup(); // use to end implicit group,(21 pixels head offset)
		var mainRect = areaRect;
		var scaleRect = new Rect(mainRect.position / zoomScale, mainRect.size / zoomScale);
		DrawRectangle(mainRect, Color.red, 2);
		// -- start zoom --
		var oldMatrix = GUI.matrix;
		GUIUtility.ScaleAroundPivot(Vector2.one * zoomScale, Vector2.zero);
		DrawRectangle(mainRect, Color.blue, 2);
		DrawRectangle(scaleRect, Color.yellow, 2);
		Handles.DrawLine(Vector2.one, Vector2.one * 100);
		GUI.BeginGroup(scaleRect);
		BeginWindows();
		MainWindow();
		EndWindows();
		GUI.EndGroup();
		GUI.matrix = oldMatrix;
		// -- end zoom --
		GUI.BeginClip(new Rect(0, 21, Screen.width, Screen.height - 21));
	}

	void MainWindow()
	{
		windowRect = GUI.Window(0, windowRect, MainWindowFunc, "Main Window");
	}

	void MainWindowFunc(int windowId)
	{
		GUI.Button(new Rect(20, 20, 100, 20), "Hello");
		GUI.DragWindow();
	}

	static void DrawRectangle(Rect rect, Color color, float width = 1)
	{
		var colorBackup = Handles.color;
		Handles.color = color;
		var topLeft = new Vector2(rect.xMin, rect.yMin);
		var topRight = new Vector2(rect.xMax, rect.yMin);
		var bottomRight = new Vector2(rect.xMax, rect.yMax);
		var bottomLeft = new Vector2(rect.xMin, rect.yMax);
		if (width <= 1)
		{
			Handles.DrawPolyLine(topLeft, topRight, bottomRight, bottomLeft, topLeft);
		}
		else
		{
			Handles.DrawAAPolyLine(width, topLeft, topRight, bottomRight, bottomLeft, topLeft);
		}
		Handles.color = colorBackup;
	}

	static void DrawPoint(Vector2 point, Color color, float radius = 3)
	{
		var colorBackup = Handles.color;
		Handles.color = color;
		Handles.DrawSolidDisc(point, Vector3.back, radius);
		Handles.color = colorBackup;
	}
}
