using UnityEngine;
namespace ScottBarley.IGB283.Assessment2.WS8
{
    public class LinePlaneIntersection : MonoBehaviour
    {
        [Header("Plane")]
        [SerializeField] private Vector3[] planeCorners;
        [SerializeField] private Material planeMaterial;
        [SerializeField]
        private Color planeColour = new Color(0.8f, 0.8f, 0.8f, 1);
        private Mesh planeMesh;

        [Header("Line")]
        [SerializeField] private Vector3[] lineEnds;
        [SerializeField] private float lineWidth;
        [SerializeField] private Material lineMaterial;
        private LineRenderer line;


        void Start()
        {
            // Setup the line and plane
            SetupPlane();
            SetupLine();
            // Find the line-plane intersection
            FindIntersection();

        }

        // Find the line-plane intersection
        private void FindIntersection()
        {
            // Get the plane's Mesh data
            Vector3[] vertices = planeMesh.vertices;
            int[] triangles = planeMesh.triangles;


            // Get a triangle and the line ends
            Vector3 p0 = vertices[triangles[1]];
            Vector3 p1 = vertices[triangles[0]];
            Vector3 p2 = vertices[triangles[2]];
            Vector3 la = lineEnds[0];
            Vector3 lb = lineEnds[1];

            //https://en.wikipedia.org/wiki/Line%E2%80%93plane_intersection (Line–plane intersection)
            // Calculate the equation vectors
            Vector3 lab = lb - la;
            Vector3 p01 = p1 - p0;
            Vector3 p02 = p2 - p0;

            // Find the inverse coefficient matrix
            Matrix3x3 m = new Matrix3x3();
            m.SetColumn(0, -lab);
            m.SetColumn(1, p01);
            m.SetColumn(2, p02);
            m = m.Inverse;

            // Apply the matrix to solve the system
            Vector3 constantVec = la - p0;
            Vector3 tuv = m.MultiplyVector(constantVec);
            // Separate t, u, and v
            float t = tuv.x;
            float u = tuv.y;
            float v = tuv.z;

            // If 𝑡 is between 0 and 1 inclusive, then the point is on the line between the start and end points
            // if 𝑢 and 𝑣 are both between 0 and 1 inclusive, then the point lies on the plane(within the parallelogram defined by the triangle’s vertices)
            // If (𝑢 + 𝑣) ≤ 1, then the point is inside the triangle itself.  

            // Determine if the point is on the line and plane
            bool onLine = t <= 1 && t >= 0;
            bool onPlane = (u >= 0 && u <= 1) && (v >= 0 && v <= 1);
            bool inTriangle = (u + v) <= 1;


            // Set the location if on the line and plane, or log a message otherwise
            if (onLine && onPlane)
            {
                // Create a new sphere to show the intersection
                GameObject intersection = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                intersection.name = "Intersection Point";
                intersection.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);


                // Move the sphere to the intersection point - Using line equation 𝒍𝑎 + 𝒍𝑎𝑏�
                intersection.transform.position = la + lab * t;

                // Output whether or not the intersection occured in the selected triangle
                 Debug.Log(string.Format("Inside triangle {0}, {1}, {2}: {3}", p0, p1, p2, inTriangle));
            }
            else
            {
                Debug.Log("The line and plane do not intersect.");
            }



        }

        #region Plane
        // Create the plane
        private void SetupPlane()
        {
            // Create a new empty GameObject for the plane
            GameObject plane = new GameObject("Plane");

            // Add a Mesh Filter and Mesh Renderer to the plane
            planeMesh = plane.AddComponent<MeshFilter>().mesh;
            plane.AddComponent<MeshRenderer>().material = planeMaterial;
            planeMesh.Clear();

            // set the vertices and colours as the Inspector inputs
            planeMesh.vertices = planeCorners;
            planeMesh.colors = new Color[] { planeColour, planeColour, planeColour, planeColour };
            planeMesh.triangles = new int[]
            {
            0, 1, 2,
            0, 2, 3
            };

            // Recalculate the normals for correct lighting
            planeMesh.RecalculateNormals();
        }
        #endregion
        #region Line
        private void SetupLine()
        {
            // Create a new empty GameObject for the plane
            GameObject lineObj = new GameObject("Line");
            line = lineObj.AddComponent<LineRenderer>();

            // Set the line variables
            line.widthMultiplier = lineWidth;
            line.positionCount = 2;
            line.SetPositions(lineEnds); //  SetPositions adds all the Vector3 values in an array as the Line Renderer’s positions
            line.material = lineMaterial;

        }
        #endregion
    }
}