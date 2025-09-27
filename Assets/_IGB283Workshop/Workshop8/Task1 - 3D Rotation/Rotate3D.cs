using UnityEngine;

public class Rotate3D : MonoBehaviour
{
    [Header("Rotation Speed")]
    [SerializeField] private Vector3 angle = new Vector3(1f, 1f, 1f);


    private Mesh mesh;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mesh = gameObject.GetComponent<MeshFilter>().mesh;
    }

    void Update()
    {
        RotateCube();
    }

    /// <summary>
    /// Rotate the cube by 'angle' radians per second
    /// </summary>
    private void RotateCube()
    {
        // Get the rotation matrix
        Matrix4x4 r = Rotate(angle * Time.deltaTime);
        // Apply the rotation to all vertices
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = r.MultiplyPoint(vertices[i]);
        }
        // Update the mesh
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }


    #region 3DRotation Matrix
    /// <summary>
    /// Rotation
    /// </summary>
    public static Matrix4x4 Rotate(Vector3 vec3) => Rotate(vec3.x, vec3.y, vec3.z);
    public static Matrix4x4 Rotate(float xRad, float yRad, float zRad)
    {
        Matrix4x4 rx = RotateX(xRad);
        Matrix4x4 ry = RotateY(yRad);
        Matrix4x4 rz = RotateZ(zRad);

        // Combine the rotations => use the same as Unity: Z → X → Y
        return ry * rx * rz;
    }
    
    // Rotate Around X
    public static Matrix4x4 RotateX(float angleRad)
    {
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        Matrix4x4 m = Matrix4x4.identity;
        m.SetRow(0, new Vector4(1f, 0f, 0f, 0f));
        m.SetRow(1, new Vector4(0f, cos, -sin, 0f));
        m.SetRow(2, new Vector4(0f, sin, cos, 0f));
        m.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

        return m;
    }

    // Rotate Around Y
    public static Matrix4x4 RotateY(float angleRad)
    {
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        Matrix4x4 m = Matrix4x4.identity;
        m.SetRow(0, new Vector4(cos, 0f, sin, 0f));
        m.SetRow(1, new Vector4(0f, 1f, 0f, 0f));
        m.SetRow(2, new Vector4(-sin, 0f, cos, 0f));
        m.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

        return m;
    }

    // Rotate Around Z
    public static Matrix4x4 RotateZ(float angleRad)
    {
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        Matrix4x4 m = Matrix4x4.identity;
        m.SetRow(0, new Vector4(cos, -sin, 0f, 0f));
        m.SetRow(1, new Vector4(sin, cos, 0f, 0f));
        m.SetRow(2, new Vector4(0f, 0f, 1f, 0f));
        m.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

        return m;
    }
    #endregion

}
