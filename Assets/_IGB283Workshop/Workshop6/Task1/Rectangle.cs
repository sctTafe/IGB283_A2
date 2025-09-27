using UnityEngine;
namespace workshop6.task1
{
    public class Rectangle : MonoBehaviour
    {


        // Angle by which the triangle will rotate each frame (rad)
        public float angle = 10f;
        public Material material;
        
        private Color _colour = Color.white;
        private Mesh mesh;
        
        // Starting positon:
        private Vector3[] baseVertices;
        // Offset for the rectangle's centre
        private Vector2 offset;


        void Start()
        {
            InitialiseComponents();
            CreateRectangle();

            // Calculate the offset from the mesh size
            offset.x = mesh.bounds.size.x / 2;
            offset.y = mesh.bounds.size.y / 2;
        }

        private void Update()
        {
            RotateAroundMeshCenter();
        }

        /// <summary>
        /// Constantly Rotates the mesh center
        /// </summary>
        private void RotateAroundMeshCenter()
        {
            // Get the vertices from the matrix
            Vector3[] vertices = mesh.vertices;

            Matrix3x3 t = Matrix3x3.Translate(offset.x,offset.y);
            Matrix3x3 r = Matrix3x3.Rotate(angle * Time.deltaTime);
            Matrix3x3 tInv = Matrix3x3.Translate(-offset.x,-offset.y);
            Matrix3x3 m = t * r * tInv;

            //Apply Transformation Matrix to mesh
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = m.MultiplyPoint(vertices[i]);
            }

            // Set the vertices in the mesh to their new position
            mesh.vertices = vertices;
            // Recalculate the bounding volume
            mesh.RecalculateBounds();
        }
        
        
        
        /// <summary>
        /// Constantly Rotates the mesh
        /// </summary>
        private void ConstantRotation()
        {
            // Get the vertices from the matrix
            Vector3[] vertices = mesh.vertices;

            // Get the rotation matrix
            Matrix3x3 matrix3x3_Rotated = Matrix3x3.Rotate(angle * Time.deltaTime);


            // Rotate each point in the mesh to its new position
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = matrix3x3_Rotated.MultiplyPoint(vertices[i]);
            }

            // Set the vertices in the mesh to their new position
            mesh.vertices = vertices;
            // Recalculate the bounding volume
            mesh.RecalculateBounds();
        }




        #region Start

        private void CreateRectangle()
        {

            // Create a simple quad
            baseVertices = new Vector3[]
            {
                 new Vector3(0, 0, 0),
                 new Vector3(0, 1, 0),
                 new Vector3(1, 1, 0),
                 new Vector3(1, 0, 0)

            };

            int[] triangles = new int[]
            {
                0, 1, 2,    //t0
                0, 2, 3     //t1
            };

            _colour = new Color(0.8f, 0.3f, 0.3f, 1f);

            Color[] colors = new Color[]
            {
                _colour, _colour, _colour, _colour
            };

            mesh.vertices = baseVertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
        }


        private void InitialiseComponents()
        {
            // Add a MeshFilter and MeshRenderer to the Empty GameObject
            gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>();

            // Get the Mesh from the MeshFilter
            mesh = GetComponent<MeshFilter>().mesh;
            // Clear all vertex and index data from the mesh
            mesh.Clear();

            // Set the material to the material we have selected
            GetComponent<MeshRenderer>().material = material;
        }
        #endregion
    }
}
