using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(BSpline))]
public class BSplineInspector : Editor
{
	private BSpline curve;
	private Transform handleTransform;
	private Quaternion handleRotation;

	private const int lineSteps = 50;

	private void OnSceneGUI()
	{
		curve = target as BSpline;
		handleTransform = curve.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

		Vector3[] transPoints = new Vector3[curve.points.Length];
		for (int i = 0; i < curve.points.Length; i++)
		{
			transPoints[i] = ShowPoint(i);
			
		}
		int[] segmentIndices = new int[curve.points.Length * 2 - 2];
		for (int i = 0, j = 1, k = 0; j < curve.points.Length; i++, j++)
		{
			segmentIndices[k++] = i;
			segmentIndices[k++] = j;
		}
		Handles.color = Color.gray;
		Handles.DrawLines(transPoints, segmentIndices);

		if (!curve.drawLine)
		{
			return;
		}
		Handles.color = Color.white;
		Vector3 lineStart = curve.GetPoint(0f);
		for (int i = 1; i <= lineSteps; i++)
		{
			Vector3 lineEnd = curve.GetPoint(i / (float)lineSteps);
			Handles.DrawLine(lineStart, lineEnd);
			lineStart = lineEnd;
		}
	}

	private Vector3 ShowPoint(int index)
	{
		Vector3 point = handleTransform.TransformPoint(curve.points[index]);
		EditorGUI.BeginChangeCheck();
		point = Handles.PositionHandle(point, handleRotation);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(curve, "Move Point");
			EditorUtility.SetDirty(curve);
			curve.points[index] = handleTransform.InverseTransformPoint(point);
		}
		return point;
	}

	public override void OnInspectorGUI()
	{
		curve = target as BSpline;
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();
		if (EditorGUI.EndChangeCheck())
		{
			curve.Update();
		}
		if (GUILayout.Button("reset"))
		{
			Undo.RecordObject(curve, "reset");
			curve.Reset();
			EditorUtility.SetDirty(curve);
		}
	}
}
