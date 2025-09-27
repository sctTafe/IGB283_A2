using UnityEngine;
namespace ScottBarley.IGB283.Assessment2
{
    [CreateAssetMenu(fileName = "VertexData", menuName = "Scriptable Objects/VertexData")]
    public class VertexData : ScriptableObject
    {
        [SerializeField] private Vector3[] vertices;

        public Vector3[] Vertices => vertices;

        /// <summary>
        /// Copy vertices into this ScriptableObject.
        /// </summary>
        public void SaveVertices(Vector3[] source)
        {
            if (source == null)
            {
                Debug.LogWarning("Tried to save null vertices into LimbVerticesData.");
                return;
            }

            // Create a copy so it’s independent of the source array
            vertices = new Vector3[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                vertices[i] = source[i];
            }
        }
    }
}