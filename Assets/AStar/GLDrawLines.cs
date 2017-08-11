using UnityEngine;

namespace AStar
{
	[RequireComponent(typeof(Camera))]
	public class GLDrawLines : MonoBehaviour
	{
		private Material mat;
		public Vector2[] path = new Vector2[0];
		new Camera camera;

		void Start()
		{
			camera = GetComponent<Camera>();
		}

		// Will be called from camera after regular rendering is done.
		public void OnPostRender()
		{
			if (path == null)
			{
				return;
			}
			if (!mat)
			{
				// Unity has a built-in shader that is useful for drawing
				// simple colored things. In this case, we just want to use
				// a blend mode that ignore destination colors.
				var shader = Shader.Find("Hidden/Internal-Colored");
				mat = new Material(shader);
				mat.hideFlags = HideFlags.HideAndDontSave;
				// Set blend mode to ignore destination colors.
				mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				// Turn off backface culling, depth writes, depth test.
				mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
				mat.SetInt("_ZWrite", 0);
				mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
			}
			Vector2[] points = new Vector2[path.Length];
			for (int i = 0; i < path.Length; i++)
			{
				points[i] = camera.WorldToViewportPoint(path[i]);
			}
			GL.PushMatrix();
			mat.SetPass(0);
			GL.LoadOrtho();
			GL.Begin(GL.LINE_STRIP);
			GL.Color(Color.red);
			foreach (var point in points)
			{
				GL.Vertex(point);
			}
			GL.End();
			GL.PopMatrix();
		}
	}
}