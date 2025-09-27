using System;
using UnityEngine;
namespace ScottBarley.IGB283.Assessment2.Task2
{
    public class OctagonAnimator : MonoBehaviour
    {
        // -- Setup --

        [Header("Body Parts")]
        [SerializeField] OctagonArticulator _HeadOctagon;
        [SerializeField] OctagonArticulator _UperBodyOctagon;
        [SerializeField] OctagonArticulator _LowerBodyOctagon;
        [SerializeField] OctagonArticulator _Root;

        // -- Movment --

        [Header("Movement - Sideways")]
        [SerializeField] private bool _isMovingSideToSide;
        [SerializeField] private bool _isAutoDirectionChange;
        [SerializeField] float _speed;
        [SerializeField] bool _MovingToTheRight;

        [Header("Boundaries - Sides")]
        [SerializeField] float _xMaxBoundary = 15f;
        [SerializeField] float _xMinBoundary = -15f;

        [Header("Movement - Jumping")]
        [SerializeField] bool _isAutoJumping = true;
        [SerializeField] bool _isGrounded;
        [SerializeField] float _jumpHeight = 5f;
        [SerializeField] float _gravity = -9.81f;

        // -- Animations --

        [Header("Animation - Lean In Direction Of Travel")]
        [SerializeField] float _leanAngle = 0.25f; //radians
        [SerializeField] bool _isLeaningLeft;

        [Header("Animation - Head Wobble")]
        [SerializeField] float _maxWobbleRotation = 1f; //radians
        [SerializeField] float _wobbleRotationSpeed = 0.1f; //radians/s
        [SerializeField] bool isWobbling = true;
        bool _HeadWobbleRotatingClockwise;



        IGB283Vector _startingPosition;
        IGB283Vector _currentPosition;
        IGB283Vector _velocity = new IGB283Vector();


        private void Start()
        {
            //Get starting postion of root
            _startingPosition = transform.position;
            _currentPosition = _startingPosition;
        }

        private void Update()
        {

            // -- Character Movement --

            VelocityVertical();

            if (_isMovingSideToSide)
            {
                VelocityHorizontal();
            }

            Update_CurrentPoisitonAndMove();



            // -- Animation --
            if (isWobbling)
            {
                headBob();
                UpperBodyBob();
            }

            LeanInDirectionOfTravel();


            // -- Debugging --
            Debug.Log($"Velocity {_velocity}");
        }

        #region Movement
        void VelocityHorizontal()
        {
            //Boundary Interaction - Handles Auto change in driection
            if (_isAutoDirectionChange)
            {
                var currentX = _currentPosition.x;
                if (currentX < _xMinBoundary)
                    _MovingToTheRight = true;
                if (currentX > _xMaxBoundary)
                    _MovingToTheRight = false;
            }

            var displacementAmount = Time.deltaTime * _speed;

            if (_MovingToTheRight) //Moving along +ve X           
                _velocity.x = displacementAmount;
            else
                _velocity.x = -displacementAmount;
        }

        void VelocityVertical()
        {
            // Check if Grounded
            IsGrounded();

            // If Grounded
            if (_isGrounded)
            {
                UpdateVerticalVewlocity_ApplyGrounded();
            }

            // If not grounded
            if (!_isGrounded)
            {
                // Update Vertical Velocity
                UpdateVerticalVelocity_ApplyGravity();
            }

            //Handle Auto Jump
            if (_isAutoJumping)
            {
                TryJump();
            }
        }

        void Update_CurrentPoisitonAndMove()
        {
            // Update current position with the velocity
            _currentPosition += _velocity;
            _Root.fn_Move(_velocity);
        }


        void UpdateVerticalVelocity_ApplyGravity()
        {
            // Apply gravity
            _velocity.y += _gravity * Time.deltaTime;

            // Terminal velocity limit 
            _velocity.y = Mathf.Max(_velocity.y, -20f);
        }

        void UpdateVerticalVewlocity_ApplyGrounded()
        {
            _velocity.y = 0f;
        }


        /// <summary>
        /// Changes Velocity Value to handle Jump Call
        /// </summary>
        void TryJump()
        {
            if (_isGrounded)
            {
                Debug.Log("TryJump Called Sucessfully");
                _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }
        }

        bool IsGrounded()
        {
            if (_currentPosition.y <= 0)
            {
                _isGrounded = true;
                return true;
            }

            _isGrounded = false;
            return false;
        }
        #endregion

        #region Animations

        /// <summary>
        /// Lean in the direction of travel from the lower body pivot point
        /// </summary>
        private void LeanInDirectionOfTravel()
        {
            // Not Leaning
            if (_velocity.x == 0f)
            {
                _LowerBodyOctagon.fn_RotateToZero();
            }


            if (_velocity.x > 0.001f)
            {
                _LowerBodyOctagon.fn_RotateToZero();
                _LowerBodyOctagon.fn_RotatePartAroundPivot(-_leanAngle);
            }

            if (_velocity.x < -0.001f)
            {
                _LowerBodyOctagon.fn_RotateToZero();
                _LowerBodyOctagon.fn_RotatePartAroundPivot(_leanAngle);
            }
        }


        /// <summary>
        /// Rotates 'head' around piviot, ping ponging between a user specified angle 
        /// </summary>
        void headBob()
        {
            float currentAngle = _HeadOctagon.LastRotationAngle;

            // Flip Rotation Direction if over max value
            if (_HeadWobbleRotatingClockwise)
            {
                if (currentAngle > _maxWobbleRotation)
                    _HeadWobbleRotatingClockwise = !_HeadWobbleRotatingClockwise;
            }
            else
            {

                if (currentAngle < -_maxWobbleRotation)
                    _HeadWobbleRotatingClockwise = !_HeadWobbleRotatingClockwise;
            }

            //Rotate Part 
            if (_HeadWobbleRotatingClockwise)
            {
                _HeadOctagon.fn_RotatePartAroundPivot(currentAngle += Time.deltaTime * _wobbleRotationSpeed);
            }
            else
            {
                _HeadOctagon.fn_RotatePartAroundPivot(currentAngle -= Time.deltaTime * _wobbleRotationSpeed);
            }
        }
        /// <summary>
        /// Rotates 'UperBody' around piviot, ping ponging between a user specified angle 
        /// </summary>
        void UpperBodyBob()
        {
            float currentAngle = _UperBodyOctagon.LastRotationAngle;

            // Flip Rotation Direction if over max value
            if (_HeadWobbleRotatingClockwise)
            {
                if (currentAngle > _maxWobbleRotation)
                    _HeadWobbleRotatingClockwise = !_HeadWobbleRotatingClockwise;
            }
            else
            {

                if (currentAngle < -_maxWobbleRotation)
                    _HeadWobbleRotatingClockwise = !_HeadWobbleRotatingClockwise;
            }

            //Rotate Part 
            if (_HeadWobbleRotatingClockwise)
            {
                _UperBodyOctagon.fn_RotatePartAroundPivot(currentAngle += Time.deltaTime * _wobbleRotationSpeed);
            }
            else
            {
                _UperBodyOctagon.fn_RotatePartAroundPivot(currentAngle -= Time.deltaTime * _wobbleRotationSpeed);
            }
        }
        #endregion
    }
}