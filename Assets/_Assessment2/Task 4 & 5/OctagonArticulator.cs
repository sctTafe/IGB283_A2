using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ScottBarley.IGB283.Assessment2.Task4
{
    /// <summary>
    /// Based on workshop7 provided learning materials
    /// </summary>
    public class OctagonArticulator : MonoBehaviour
    {
        // Reference the limb’s child and controller
        [SerializeField] private List<OctagonArticulator> childObjects;
        [SerializeField] private Slider _controlRInput; // Rotate Input
        [SerializeField] private Slider _controlSInput; // Scale Input
        // Keep the joint location from being altered after Start
        [SerializeField] private Vector2 initialJointLocation;
        // The corner positions of each limb
        [SerializeField] private Vector3[] _limbVertices;
        [SerializeField] private Color _colour = Color.white;
        [SerializeField] private Material material;

        [SerializeField] private VertexData _vertexDataSO; //SO to save and strore the vertex data on so i dont keep losing it and having to reenter it

        private Mesh _mesh;
        // JointLocation is the position at which the limb is joined to its parent. We will use this as a pivot for rotating.
        private Vector2 _jointLocation;
        // Store the previous angle to undo
        private float _lastAngle = 0;
        private float _lastScale;

        public float LastRotationAngle => _lastAngle;

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
            fn_Move(initialJointLocation);
        }


        private void OnEnable()
        {
            if (_controlRInput != null)
                _controlRInput.onValueChanged.AddListener(Handle_OnRotateControlChanged);
            if (_controlSInput != null)
                _controlSInput.onValueChanged.AddListener(Handle_OnScaleControlChanged);

        }
        private void OnDisable()
        {
            if (_controlRInput != null)          
                _controlRInput.onValueChanged.RemoveListener(Handle_OnRotateControlChanged);
            if (_controlSInput != null)
                _controlSInput.onValueChanged.AddListener(Handle_OnScaleControlChanged);

        }

        /// <summary>
        /// Rotate part around privot to the desired angle (rad)
        /// </summary>
        public void fn_RotatePartAroundPivot(float value) => fn_RotateAroundPoint(_jointLocation, value);



        private void Handle_OnRotateControlChanged(float value)
        {
            fn_RotateAroundPoint(_jointLocation, value);
        }
        private void Handle_OnScaleControlChanged(float value)
        {
            fn_ScalePoint(_jointLocation, value);
        }


        private void DrawLimb()
        {
            // octagon
            int[] triangles = new int[]
            {
                0, 1, 2,    // t0
                0, 2, 3,    // t1
                0, 3, 4,    // t2
                0, 4, 5,    // t3
                0, 5, 6,    // t4
                0, 6, 7,    // t5
            };

            Color[] colors = new Color[_limbVertices.Length];

            for (int i = 0; i < _limbVertices.Length; i++)
            {
                colors[i] = _colour;
            }

            _mesh.vertices = _limbVertices;
            _mesh.triangles = triangles;
            _mesh.colors = colors;
        }



        // Translate the limb
        public void fn_Move(Vector2 offset)
        {
            // Calculate the translation matrix
            IGB283Transform t = IGB283Transform.Translate(offset.x, offset.y);
            ApplyTransformation(t);
        }


        /// <summary>
        ///  Rotate the limb around a point
        /// </summary>
        private void fn_RotateAroundPoint(Vector2 point, float angle)
        {
            // Calculate the transformation matrices
            IGB283Transform tInv = IGB283Transform.Translate(-point.x, -point.y); //Move to origin
            IGB283Transform rLastAngle = IGB283Transform.Rotate(-_lastAngle); //Undo last rotation
            IGB283Transform rAngle = IGB283Transform.Rotate(angle); //Apply new rotation
            IGB283Transform t = IGB283Transform.Translate(point.x, point.y); //Move back

            // Apply the rotation around point
            ApplyTransformation(t * rAngle * rLastAngle * tInv);

            // Update the last angle
            _lastAngle = angle;
        }
        /// <summary>
        ///  Scale the limb around a point
        /// </summary>
        private void fn_ScalePoint(Vector2 point, float scale)
        {
            // Move mesh to origin → scale → move back
            IGB283Transform t = IGB283Transform.Translate(point.x, point.y);
            IGB283Transform s = IGB283Transform.Scale(scale, scale);
            IGB283Transform tInv = IGB283Transform.Translate(-point.x, -point.y);

            ApplyTransformation(t * s * tInv);

            _lastScale = scale;
        }
    


        /// <summary>
        /// Return to Zero Rotation about pivot point
        /// </summary>
        public void fn_RotateToZero() => fn_RotateTowardsoTargetAngleAtSpeed(0f, 1f);

        /// <summary>
        /// Rotate Smoothly towards a target angle, at input speed
        /// </summary>
        public void fn_RotateTowardsoTargetAngleAtSpeed(float targetAngle, float speed)
        {

            float diff = targetAngle - _lastAngle;

            // If we're already close enough, just snap to the target
            if (Mathf.Abs(diff) <= 0.001f)
            {
                fn_RotateAroundPoint(_jointLocation, targetAngle);
                return;
            }

            // Rotation distance
            float angleStep = speed * Time.deltaTime;

            // Check if change is grater then required
            if (Mathf.Abs(angleStep) > Mathf.Abs(diff))
            {
                // reached target
                angleStep = diff; 
            }
            else
            {
                // move towards target, set direction +ve/-ve
                angleStep = Mathf.Sign(diff) * angleStep; 
            }

            float newAngle = _lastAngle + angleStep;
            fn_RotateAroundPoint(_jointLocation, newAngle);
        }



        // Apply any input transformation to the limb -  apply any transformation to the mesh,
        // regardless of whether it is a translation, rotation, scale, or combination
        private void ApplyTransformation(IGB283Transform transformation)
        {
            // Apply the transformation to each vertex
            Vector3[] vertices = _mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = transformation.MultiplyPoint(vertices[i]);
            }
            // Update the mesh
            _mesh.vertices = vertices;
            _mesh.RecalculateBounds();


            // Update the joint location
            _jointLocation = transformation.MultiplyPoint(_jointLocation);

            // Apply the offset to the child, if not null
            if (childObjects != null)
            {
                if (childObjects.Count > 0)
                {
                    for (int i = 0; i < childObjects.Count; i++)
                    {
                        childObjects[i].ApplyTransformation(transformation);
                    }
                }
            }

        }

        private void InitialiseComponents()
        {
            // Add a MeshFilter and MeshRenderer to the GameObject If empty
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();

            var renderer = gameObject.GetComponent<MeshRenderer>();
            if (renderer == null)
                renderer = gameObject.AddComponent<MeshRenderer>();

            // create a fresh mesh & assign to mesh filter
            _mesh = new Mesh();
            meshFilter.mesh = _mesh;
            // Clear all vertex and index data from the mesh
            _mesh.Clear();

            // Set the material to the material we have selected
            if (material != null)
            {
                renderer.material = material;
            }
            else
            {
                Debug.LogWarning($"[{name}] No material assigned to OctagonArticulator!");
            }
        }


        [ContextMenu("Save Vertices To SO")]
        public void fn_Util_SaveLimbVerticesToSO()
        {
            _vertexDataSO.SaveVertices(_limbVertices);
            Debug.Log($"Saved {_limbVertices.Length} vertices from {name} into {_vertexDataSO.name}");
        }

        [ContextMenu("Save Current Mesh Vertices To SO")]
        public void fn_Util_SaveCurrentMeshVerticesToSO()
        {
            Vector3[] currentVertices = _mesh.vertices;
            _vertexDataSO.SaveVertices(currentVertices);
            Debug.Log($"[{name}] Saved {currentVertices.Length} CURRENT vertices into {_vertexDataSO.name}");
        }

        [ContextMenu("Load Vertices From SO")]
        public void fn_Util_LoadLimbVerticesFromSO()
        {
                // Clear existing vertices
                _limbVertices = null;
                // Copy the vertices from SO
                Vector3[] saved = _vertexDataSO.Vertices;
                _limbVertices = new Vector3[saved.Length];
                for (int i = 0; i < saved.Length; i++)
                {
                    _limbVertices[i] = saved[i];
                }
                Debug.Log($"Loaded {_limbVertices.Length} vertices from {_vertexDataSO.name} into {name}");
        }
    }
}
