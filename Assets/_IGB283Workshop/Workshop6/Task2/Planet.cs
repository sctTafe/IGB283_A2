using UnityEngine;
namespace workshop6.Task2
{

    /// <summary>
    /// 
    /// A planet that:
    /// - Spins around its own axis
    /// - Orbits around its parent planet (e.g., Earth around Sun, Moon around Earth)
    /// - Uses only Matrix3x3 transformations on mesh vertices (not Transform.position)
    /// 
    /// Note:  
    /// - Cobind the transformation is the reverse order you want them combind
    /// - Rotating around an object’s centre takes the form 𝑀=𝑇𝑅𝑇^(−1)
    /// </summary>
    public class Planet : MonoBehaviour
    {

        [Header("Settings")]
        [SerializeField] private Vector2 _initialOffset = new Vector2(3f, 0f); 
        [SerializeField] private float _orbitSpeed = 1f;   // radians/s
        [SerializeField] private float _spinSpeed = 2f;    // radians/s
        [SerializeField] private float _scale = 1f;       

        private Mesh _mesh;
        private Vector3[] _originalVertices;  // initial vertices reference
        private Planet _parentPlanet;


        void Start()
        {
            InitialiseComponents();
        }

        void Update()
        {
            Matrix3x3 transformMatrix = PlanetMovementMatrix();
            ApplyUpdatedMeshToVertices(transformMatrix);
        }

        private Matrix3x3 PlanetMovementMatrix()
        {

            // IMPORTANT NOTE: Apply Matrix Order - Right is applied first 'right-to-left'

            // Local Scale (Planet Scale)
            Matrix3x3 scaleM = Matrix3x3.Scale(_scale, _scale);

            // Local Rotation (Planet Spin) (radians)
            float spinAngle = _spinSpeed * Time.time;
            Matrix3x3 rotationM = Matrix3x3.Rotate(spinAngle);

            if (_parentPlanet != null)
            {
                // Rotation Around Point (Orbit around parents) (radians)
                float orbitAngle = _orbitSpeed * Time.time;
                Vector3 offset = _initialOffset;
                Vector3 parentCentre = _parentPlanet.GetCurrentCentre();

                // Orbital Translation
                Matrix3x3 orbitalTranslateM = Matrix3x3.Translate(offset.x, offset.y);

                // Orbital Rotation
                Matrix3x3 orbitalRotationM = Matrix3x3.Rotate(orbitAngle);

                // Parent Translation
                Matrix3x3 translateParentM = Matrix3x3.Translate(parentCentre.x, parentCentre.y);

                Matrix3x3 orbitalMatrix = translateParentM * orbitalRotationM * orbitalTranslateM;

                // Matrix Out - With Parent
                return orbitalMatrix * (scaleM * rotationM);
            }
            else
            {
                // Matrix Out - Without Parent
                return scaleM * rotationM;
            }
        }

       
        private void ApplyUpdatedMeshToVertices(Matrix3x3 transformMatrix)
        {
            // Reset vertices to the original state 
            Vector3[] vertices = (Vector3[])_originalVertices.Clone();

            // Apply to vertices
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = transformMatrix.MultiplyPoint(vertices[i]);

            // display
            _mesh.vertices = vertices;
            _mesh.RecalculateBounds();
        }

        /// <summary>
        /// Gets the current centre of this planet in world space
        /// Note: for children for orbiting
        /// </summary>
        public Vector3 GetCurrentCentre()
        {
            return _mesh.bounds.center;
        }

        public bool fn_TryGetParentPlanetScript(out Planet parentPlanet)
        {
            parentPlanet = null;

            if (this.transform.parent.gameObject.TryGetComponent<Planet>(out Planet _planet))
            {
                parentPlanet = _planet;
                return true;
            }

            return false;           
        }

        private void InitialiseComponents()
        {
            _mesh = GetComponent<MeshFilter>().mesh;

            _originalVertices = _mesh.vertices;

            if (transform.parent != null)
                //fn_TryGetParentPlanetScript(out _parentPlanet);
                transform.parent.TryGetComponent(out _parentPlanet);
        }
    }
}