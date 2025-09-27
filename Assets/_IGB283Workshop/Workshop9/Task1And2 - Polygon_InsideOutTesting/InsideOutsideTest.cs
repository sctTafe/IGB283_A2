using TMPro;
using UnityEngine;

namespace ScottBarley.IGB283.Assessment2.WS7
{
    public class InsideOutsideTest : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI outputTMP;

        [Header("Object References")]
        [SerializeField] private Transform testPoint;
        [SerializeField] private Polygon polygon;

        [Header("Line Renderer")]
        [SerializeField] private float lineWidth = 0.05f;
        [SerializeField] private Material lineMaterial;
        [SerializeField] private Gradient lineColour = new Gradient();
        private LineRenderer lineRenderer;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            // Set up the line renderer
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.widthMultiplier = lineWidth;
            lineRenderer.material = lineMaterial;
            lineRenderer.colorGradient = lineColour;
            lineRenderer.positionCount = 2;
        }

        // Update is called once per frame
        void Update()
        {
            TestPoint();
        }

        // Perform the inside-outside test
        private void TestPoint()
        {
            // Get the polygon's edges
            Edge[] edges = polygon.EdgesFromVertices();


            // Create a ray from outside the polygon to the test point
            Vector2 rayStart = new Vector2(polygon.MinX - 0.5f, 0f);
            Vector2 rayEnd = testPoint.position;

            // create an Edge to represent this ray and update the Line Renderer to display it on screen
            Edge ray = new Edge(rayStart, rayEnd);
            lineRenderer.SetPosition(0, rayStart);
            lineRenderer.SetPosition(1, rayEnd);

            var numCrossings = 0;
            // count the number of intersections between the ray and the polygon’s edges
            for (int i = 0; i < edges.Length; i++)
            {
                if (ray.CollidesWith(edges[i]))
                    numCrossings++;
            }

            // Output the result
            if (numCrossings % 2 == 0) // Even
            {
                Debug.Log("Outside");
                outputTMP.text = "Outside";
            }
            else // Odd
            {
                Debug.Log("Inside");
                outputTMP.text = "Inside";
            }

        }

    }
}