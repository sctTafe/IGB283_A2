using UnityEngine;
using ScottBarley.IGB283.Assessment2.WS7;
namespace ScottBarley.IGB283.Assessment2.WS9
{
    public class LineCubeIntersection : MonoBehaviour
    {
        [Header("Cube")]
        [SerializeField] private MeshFilter cube;
        private Mesh cubeMesh;

        [Header("Line")]
        [SerializeField] private Vector3[] lineEnds;
        [SerializeField] private float lineWidth;
        [SerializeField] private Material lineMaterial;
        private LineRenderer line;


        void Start()
        {
            SetupLine();
            SetupCube();
            FindIntersectionFaces();
        }


        // Find the line-cube intersections
        private int FindIntersectionFaces()
        {
            // Line ends
            Vector3 la = lineEnds[0];
            Vector3 lb = lineEnds[1];
            // Get the cube's position and scale
            float scale = cube.transform.lossyScale.x;
            Vector3 offset = cube.transform.position;


            // Test the intersection with every triangle in the mesh
            int intersectionCount = 0;
            for (int i = 0; i < cubeMesh.triangles.Length; i += 3)
            {
                // Find the transformed cube vertices
                Vector3 p0 = cubeMesh.vertices[cubeMesh.triangles[i + 1]] *
               scale + offset;
                Vector3 p1 = cubeMesh.vertices[cubeMesh.triangles[i + 0]] *
               scale + offset;
                Vector3 p2 = cubeMesh.vertices[cubeMesh.triangles[i + 2]] *
               scale + offset;

                // Check if the line and current face intersect
                if (FindIntersection(p0, p1, p2, la, lb))
                {
                    intersectionCount++;
                }
            }

            //log the number of intersections to the Console
            string pluralIsAre = intersectionCount == 1 ? "is" : "are";
            string pluralS = intersectionCount == 1 ? string.Empty : "s";
            Debug.Log(string.Format("There {0} {1} intersection{2}.",
            pluralIsAre, intersectionCount.ToString(), pluralS));
            return intersectionCount;
        }

        // Find the line-plane intersection
        private bool FindIntersection(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 la, Vector3 lb)

        {
            // Get the plane's Mesh data
            Vector3[] vertices = cubeMesh.vertices;
            int[] triangles = cubeMesh.triangles;


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
            m = m.inverse;

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


            // Set the location if on the line and plane
            if (onLine && onPlane && inTriangle)
            {
                // Create a new sphere to show the intersection
                GameObject intersection =
               GameObject.CreatePrimitive(PrimitiveType.Sphere);
                intersection.name = "Intersection Point";
                intersection.transform.localScale = new Vector3(0.5f, 0.5f,
               0.5f);
                // Move the sphere to the intersection point
                intersection.transform.position = la + lab * t;
                return true;
            }
            else
            {
                // The line and face do not intersect
                return false;
            }
        }

        #region Cube
        void SetupCube()
        {
            cubeMesh = cube.mesh;
        }
        #endregion

        #region Plane
        // Create the plane
        //private void SetupPlane()
        //{
        //    // Create a new empty GameObject for the plane
        //    GameObject plane = new GameObject("Plane");

        //    // Add a Mesh Filter and Mesh Renderer to the plane
        //    cubeMesh = plane.AddComponent<MeshFilter>().mesh;
        //    plane.AddComponent<MeshRenderer>().material = planeMaterial;
        //    cubeMesh.Clear();

        //    // set the vertices and colours as the Inspector inputs
        //    cubeMesh.vertices = planeCorners;
        //    cubeMesh.colors = new Color[] { planeColour, planeColour, planeColour, planeColour };
        //    cubeMesh.triangles = new int[]
        //    {
        //    0, 1, 2,
        //    0, 2, 3
        //    };

        //    // Recalculate the normals for correct lighting
        //    cubeMesh.RecalculateNormals();
        //}
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