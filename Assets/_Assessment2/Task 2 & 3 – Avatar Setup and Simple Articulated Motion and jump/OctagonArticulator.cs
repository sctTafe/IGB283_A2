using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ScottBarley.IGB283.Assessment2
{
    /// <summary>
    /// Based on workshop7 provided learning materials
    /// </summary>
    [Serializable]
    public class OctagonArticulator : MonoBehaviour
    {
        // Reference the limb’s child and controller
        [SerializeField] private List<OctagonArticulator> childObjects;
        [SerializeField] private Slider control;
        // Keep the joint location from being altered after Start
        [SerializeField] private Vector2 initialJointLocation;
        // The corner positions of each limb
        [SerializeField] private Vector3[] _limbVertices;
        [SerializeField] private Color _colour = Color.white;
        [SerializeField] private Material material;


        private Mesh _mesh;
        // JointLocation is the position at which the limb is joined to its parent. We will use this as a pivot for rotating.
        private Vector2 _jointLocation;
        // Store the previous angle to undo
        private float _lastAngle = 0;
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

        /// <summary>
        /// Rotate part around privot to the desired angle (rad)
        /// </summary>
        public void fn_RotatePartAroundPivot(float value) => fn_RotateAroundPoint(_jointLocation, value);



        private void OnControlChanged(float value)
        {
            fn_RotateAroundPoint(_jointLocation, value);
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

        // Rotate the limb around a point
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
        /// Return to Zero Rotation about pivot point
        /// </summary>
        public void fn_RotateToZero() => fn_RotateAroundPoint(_jointLocation, -_lastAngle);




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


    }
}
