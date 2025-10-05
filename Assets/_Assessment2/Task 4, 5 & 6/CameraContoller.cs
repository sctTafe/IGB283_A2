using UnityEngine;
using ScottBarley.IGB283.Assessment2.Task4; //for the updated OctagonAnimator
namespace ScottBarley.IGB283.Assessment2
{


    /// <summary>
    /// Camera controller that tracks two characters and maintains visibility of both by calculating the required distance based on their separation and the camera's field of view
    /// Based on bot weapons controler from IGB383 A3 bots
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraContoller : MonoBehaviour
    {
        [Header("Chracter Controlers")]
        [SerializeField] OctagonAnimator _characterController_1;
        [SerializeField] OctagonAnimator _characterController_2;

        [Header("General Movement")]
        [SerializeField] float _smoothMoveXY = 0.20f;         // Smooth time for camera XY movement
        [SerializeField] float _camBoundaryPadding = 1.5f;    // World units of padding around players

        [Header("Cam Setup")]
        [SerializeField] float _minPerspectiveDistance = 10f;
        [SerializeField] float _maxPerspectiveDistance = 50f;
        [SerializeField] float _camMovementDamping = 0.12f;
        [SerializeField] float _yoffset = 4f;

        // Internals
        Camera _cammera;

        void Awake()
        {
            _cammera = GetComponent<Camera>();
        }

        void LateUpdate()
        {
            MoveCam();
        }

        void MoveCam()
        {
            float xDifference = Mathf.Abs(_characterController_1.CurrentPosition.x - _characterController_2.CurrentPosition.x);
            float yDifference = Mathf.Abs(_characterController_1.CurrentPosition.y - _characterController_2.CurrentPosition.y);

            float halfHeight = yDifference * 0.5f + _camBoundaryPadding;
            float halfWidth = xDifference * 0.5f + _camBoundaryPadding;

            // Vertical FOV 
            float fovRad = _cammera.fieldOfView * Mathf.Deg2Rad;
            float distanceForHeight = halfHeight / Mathf.Tan(fovRad * 0.5f);
            // Horizontal FOV 
            float horizontalFov = 2f * Mathf.Atan(Mathf.Tan(fovRad * 0.5f) * _cammera.aspect);
            float distanceForWidth = halfWidth / Mathf.Tan(horizontalFov * 0.5f);
            // Target Z distance (Zoom)
            float requiredDistance = Mathf.Max(distanceForHeight, distanceForWidth);
            requiredDistance = Mathf.Clamp(requiredDistance, _minPerspectiveDistance, _maxPerspectiveDistance);

            // Cam Target Pos
            IGB283Vector playersCenter = (_characterController_1.CurrentPosition + _characterController_2.CurrentPosition) * 0.5f;
            IGB283Vector camTargetPos = new IGB283Vector(playersCenter.x, playersCenter.y + _yoffset, -requiredDistance);

            // Smoothly move to camTargetPos
            transform.position = Vector3.SmoothDamp(transform.position, camTargetPos, ref _moveVelocity, _camMovementDamping);
        }

        Vector3 _moveVelocity = Vector3.zero; // Stores _moveVelocity for use in SmoothDamp, its not worth the marks rewriting my own version of a smoothDamp so using the built in version, it looks janky without it

        void OnValidate()
        {
            if (_minPerspectiveDistance < 0.01f) _minPerspectiveDistance = 0.01f;
            if (_maxPerspectiveDistance < _minPerspectiveDistance) _maxPerspectiveDistance = _minPerspectiveDistance;
        }
    }
}