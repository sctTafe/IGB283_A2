using UnityEngine;
using UnityEngine.UI;
using ScottBarley.IGB283.Assessment2.WS7;
/// <summary>
/// Created for workshop7 based on provided learning materials
/// </summary>
public class Limb : MonoBehaviour
{
    // Reference the limb’s child and controller
    [SerializeField] private Limb child;
    [SerializeField] private Slider control;
    // Keep the joint location from being altered after Start
    [SerializeField] private Vector2 initialJointLocation;
    // The corner positions of each limb
    [SerializeField] private Vector3[] _limbVertices;
    [SerializeField] private Color _colour = Color.white;
    [SerializeField] private Material material;
    


    private Mesh mesh;
    // JointLocation is the position at which the limb is joined to its parent. We will use this as a pivot for rotating.
    private Vector2 jointLocation;
    // Store the previous angle to undo
    private float lastAngle = 0;



    // Runs before start
    private void Awake()
    {
        InitialiseComponents();
        DrawLimb();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Move limb to starting position
        Move(initialJointLocation);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        if (control != null)
        {
            control.onValueChanged.AddListener(OnControlChanged);
        }
    }
    private void OnDisable()
    {
        if (control != null)
        {
            control.onValueChanged.RemoveListener(OnControlChanged);
        }
    }



    private void OnControlChanged(float value)
    {
        RotateAroundPoint(jointLocation, value);
    }



    private void DrawLimb()
    {
        
        int[] triangles = new int[]
        {
                0, 1, 2,    //t0
                0, 2, 3     //t1
        };

        Color[] colors = new Color[]
        {
                _colour, _colour, _colour, _colour
        };

        mesh.vertices = _limbVertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
    }



    // Translate the limb
    public void Move(Vector2 offset)
    {
        // Calculate the translation matrix
        Matrix3x3 t = Matrix3x3.Translate(offset.x,offset.y);
        ApplyTransformation(t);
    }

    // Rotate the limb around a point
    private void RotateAroundPoint(Vector2 point, float angle)
    {
        // Calculate the transformation matrices
        Matrix3x3 tInv = Matrix3x3.Translate(-point.x,-point.y); //Move to origin
        Matrix3x3 rLastAngle = Matrix3x3.Rotate(-lastAngle); //Undo last rotation
        Matrix3x3 rAngle = Matrix3x3.Rotate(angle); //Apply new rotation
        Matrix3x3 t = Matrix3x3.Translate(point.x, point.y); //Move back

        // Apply the rotation around point
        ApplyTransformation(t * rAngle * rLastAngle * tInv);

        // Update the last angle
        lastAngle = angle;
    }





    // Apply any input transformation to the limb -  apply any transformation to the mesh,
    // regardless of whether it is a translation, rotation, scale, or combination
    private void ApplyTransformation(Matrix3x3 transformation)
    {
        // Apply the transformation to each vertex
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = transformation.MultiplyPoint(vertices[i]);
        }
        // Update the mesh
        mesh.vertices = vertices;
        mesh.RecalculateBounds();


        // Update the joint location
        jointLocation = transformation.MultiplyPoint(jointLocation);

        // Apply the offset to the child, if not null
        if (child != null)
        {
            child.ApplyTransformation(transformation);
        }

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



}
