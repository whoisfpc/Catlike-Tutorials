using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CubeShpere
{
    public class CircleGizmos : MonoBehaviour
    {
        public int resolution = 10;
        public bool badSolution;
        public bool drawSquare;
        public bool drawYelloLine;
        public bool drawGrayLine;

        private void OnDrawGizmosSelected()
        {
            float step = 2f / resolution;
            for (int y = 0; y <= resolution; y++)
            {
                for (int x = 0; x <= resolution; x++)
                {
                    ShowPoint(x * step - 1f, y * step - 1f, -1f);
                    ShowPoint(x * step - 1f, y * step - 1f, 1f);
                }
                for (int z = 1; z < resolution; z++)
                {
                    ShowPoint(-1f, y * step - 1f, z * step - 1f);
                    ShowPoint(1f, y * step - 1f, z * step - 1f);
                }
            }
            for (int z = 1; z < resolution; z++)
            {
                for (int x = 1; x < resolution; x++)
                {
                    ShowPoint(x * step - 1f, -1f, z * step -1f);
                    ShowPoint(x * step - 1f, 1f, z * step - 1f);
                }
            }
        }

        private void ShowPoint(float x, float y, float z)
        {
            var square = new Vector3(x, y, z);
            float x2 = square.x * square.x;
            float y2 = square.y * square.y;
            float z2 = square.z * square.z;
            Vector3 s = Vector3.zero;
            s.x = square.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
            s.y = square.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
            s.z = square.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
            Vector3 circle = badSolution ? CalcShperePoint(x, y, z) : s;

            Gizmos.color = Color.black;
            if (drawSquare)
            {
                Gizmos.DrawSphere(square, 0.025f);
            }

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(circle, 0.025f);

            Gizmos.color = Color.yellow;
            if (drawYelloLine)
            {
                Gizmos.DrawLine(square, circle);
            }

            Gizmos.color = Color.gray;
            if (drawGrayLine)
            {
                Gizmos.DrawLine(circle, Vector2.zero);
            }
        }

        private Vector3 CalcShperePoint(float x, float y, float z)
        {
            Vector3 circle = Vector3.zero;
            if (x == 1f || x == -1f)
            {
                float r_xz = CalcRadian(x, z);
                float r_xy = CalcRadian(x, y);
                circle.x = Mathf.Abs(Mathf.Cos(r_xy)) * Mathf.Cos(r_xz);
                circle.y = Mathf.Sin(r_xy);
                circle.z = Mathf.Abs(Mathf.Cos(r_xy)) * Mathf.Sin(r_xz);
            }
            else if (y == 1f || y == -1f)
            {
                float r_xy = CalcRadian(x, y);
                float r_zy = CalcRadian(z, y);
                circle.x = Mathf.Abs(Mathf.Sin(r_zy)) * Mathf.Cos(r_xy);
                circle.y = Mathf.Abs(Mathf.Sin(r_zy)) * Mathf.Sin(r_xy);
                circle.z = Mathf.Cos(r_zy);

            }
            else if (z == 1f || z == -1f)
            {
                float r_xz = CalcRadian(x, z);
                float r_zy = CalcRadian(z, y);
                circle.x = Mathf.Abs(Mathf.Cos(r_zy)) * Mathf.Cos(r_xz);
                circle.y = Mathf.Sin(r_zy);
                circle.z = Mathf.Abs(Mathf.Cos(r_zy)) * Mathf.Sin(r_xz);
            }
            return circle;
        }

        private float CalcRadian(float x, float y)
        {
            float r = 0;
            if (x == 1f)
            {
                r = Mathf.PI * -0.25f + Mathf.PI * 0.25f * (1f + y);
            }
            else if (y == 1f)
            {
                r = Mathf.PI * 0.25f + Mathf.PI * 0.25f * (1f - x);
            }
            else if (x == -1f)
            {
                r = Mathf.PI * 0.75f + Mathf.PI * 0.25f * (1f - y);
            }
            else if (y == -1f)
            {
                r = Mathf.PI * 1.25f + Mathf.PI * 0.25f * (1f + x);
            }
            return r;
        }

    }
}