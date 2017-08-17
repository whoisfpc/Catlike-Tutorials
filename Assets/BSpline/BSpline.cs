using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSpline : MonoBehaviour
{
	public bool drawLine;
	private int m;
	private int n;
	private int p;
	public int N;
	public int P;
	public float[] ts;
	public Vector3[] points;

	void Start()
	{
		Reset();
	}

	public void Reset()
	{
		if (P > N)
		{
			print("B-Spline: degree > nodes");
			P = N;
		}
		p = P;
		m = P + N + 1;
		n = N;
		var step = 1f / (n + n - m + 2);
		points = new Vector3[n + 1];
		for (int i = 0; i <= n; i++)
		{
			points[i] = new Vector3(i, 0, 0);
		}
		ts = new float[m + 1];
		for (int i = 0; i <= m; i++)
		{
			if (i >= p + 1 && i <= m - p - 1)
			{
				ts[i] = step * (i - p);
			}
			else if (i < p + 1)
			{
				ts[i] = 0;
			}
			else
			{
				ts[i] = 1;
			}
		}
	}

	public void Update()
	{
		if (P > N)
		{
			print("B-Spline: degree > nodes");
			P = N;
		}
		if (p == P && n == N)
		{
			return;
		}
		m = P + N + 1;
		n = N;
		var step = 1f / (n + n - m + 2);
		var newPoints = new Vector3[n + 1];
		for (int i = 0; i <= n; i++)
		{
			if (i >= points.Length)
			{
				newPoints[i] = points[points.Length-1];
			}
			else
			{
				newPoints[i] = points[i];
			}
		}
		points = newPoints;
		ts = new float[m + 1];
		for (int i = 0; i <= m; i++)
		{
			if (i >= p + 1 && i <= m - p - 1)
			{
				ts[i] = step * (i - p);
			}
			else if (i < p + 1)
			{
				ts[i] = 0;
			}
			else
			{
				ts[i] = 1;
			}
		}
	}

	public Vector3 GetPoint(float t)
	{
		if (t == 1)
		{
			return transform.TransformPoint(points[n]);
		}
		var tmp = Vector3.zero;
		for (int i = 0; i <= n; i++)
		{
			tmp = tmp + points[i] * GetN(i, p, t);
		}
		return transform.TransformPoint(tmp);
	}

	private float GetN(int i, int j, float t)
	{
		if (j == 0)
		{
			if (t >= ts[i] && t < ts[i + 1])
			{
				return 1;
			}
			return 0;
		}

		float x1 = 0, x2 = 0;

		var under1 = (ts[i + j] - ts[i]);
		var under2 = (ts[i + j + 1] - ts[i + 1]);
		x1 = under1 == 0 ? 0 : GetN(i, j - 1, t) * (t - ts[i]) / under1;
		x2 = under2 == 0 ? 0 : GetN(i + 1, j - 1, t) * (ts[i + j + 1] - t) / under2;
		return x1 + x2;
	}
}
