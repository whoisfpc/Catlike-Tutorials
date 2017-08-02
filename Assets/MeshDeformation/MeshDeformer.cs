using UnityEngine;

namespace MeshDeformer
{
    [RequireComponent(typeof(MeshFilter))]
    public class MeshDeformer : MonoBehaviour
    {
        public float springForce = 20f;
        public float damping = 5f;
        private Mesh deformerMesh;
        private Vector3[] originalVertices, displayVertices;
        private Vector3[] vertexVelocities;
        private float uniformScale = 1f;

        private void Start()
        {
            deformerMesh = GetComponent<MeshFilter>().mesh;
            originalVertices = deformerMesh.vertices;
            displayVertices = new Vector3[originalVertices.Length];
            vertexVelocities = new Vector3[originalVertices.Length];
            for (int i = 0; i < originalVertices.Length; i++)
            {
                displayVertices[i] = originalVertices[i];
            }
        }

        private void Update()
        {
            uniformScale = transform.localScale.x;
            for (int i = 0; i < displayVertices.Length; i++)
            {
                UpdateVertex(i);
            }
            deformerMesh.vertices = displayVertices;
            deformerMesh.RecalculateNormals();
        }

        private void UpdateVertex(int i)
        {
            var velocity = vertexVelocities[i];
            var displacement = displayVertices[i] - originalVertices[i];
            displacement *= uniformScale;
            velocity -= displacement * springForce * Time.deltaTime;
            velocity *= 1f - damping * Time.deltaTime;
            vertexVelocities[i] = velocity;
            displayVertices[i] += velocity * (Time.deltaTime / uniformScale);
        }

        public void AddDeformingForce(Vector3 point, float force)
        {
            Debug.DrawLine(Camera.main.transform.position, point, Color.red);
            point = transform.InverseTransformPoint(point);
            for (int i = 0; i < displayVertices.Length; i++)
            {
                AddForceToVertex(i, point, force);
            }
        }

        private void AddForceToVertex(int i, Vector3 point, float force)
        {
            var pointToVertex = (displayVertices[i] - point) * uniformScale;
            var attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
            var velocity = attenuatedForce * Time.deltaTime;
            vertexVelocities[i] += pointToVertex.normalized * velocity;
        }
    }
}