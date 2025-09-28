using System;
using UnityEngine;
namespace ScottBarley.IGB283.Assessment2.Task4
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

        [Header("Movement - Hopping")]
        [SerializeField] bool _isHopping = true;
        [SerializeField] float _hopHeight = 0.0010f;
        [Header("Movement - Leaping")]
        [SerializeField] float _leapHeight = 0.0035f;
        private bool _triggerForwardLeap;
        [Header("Movement - Jumping")]
        [SerializeField] float _jumpHeight = 0.005f;
        [SerializeField] float _gravity = -0.25f;
        [SerializeField] bool _isGrounded;
        private bool _triggerVerticalJump;

        // -- Animations --

        [Header("Animation - Lean In Direction Of Travel")]
        [SerializeField] float _leanAngle = 0.25f; //radians
        [SerializeField] bool _isLeaningLeft;

        [Header("Animation - Head Wobble")]
        [SerializeField] float _maxWobbleRotation = 1f; //radians
        [SerializeField] float _wobbleRotationSpeed = 0.1f; //radians/s
        [SerializeField] bool _isHeadWobbling = true;
        bool _HeadWobbleRotatingClockwise;

        // -- Specail States --
        float _collapseStateTimer;
        bool _isCollapsed;
        float _collaspeLimbSpeed = 2f;

        IGB283Vector _startingPosition;
        IGB283Vector _currentPosition;
        IGB283Vector _velocity = new IGB283Vector();
        private bool _isDebugging;

        private void Start()
        {
            //Get starting postion of root
            _startingPosition = transform.position;
            _currentPosition = _startingPosition;
        }

        private void Update()
        {
            // -- Collapse State --
            Update_Collapse();

            // -- Character Movement --

            VelocityVertical();

            if (_isMovingSideToSide)
            {
                VelocityHorizontal();
                Update_CurrentPoisitonAndMove();
            }

            

            // -- Animation --
            if (_isHeadWobbling)
            {
                headBob();
                UpperBodyBob();
            }

            if (_isMovingSideToSide)
            {
                LeanInDirectionOfTravel();
            }

            // -- Debugging --
            if (_isDebugging)
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

            if (_triggerVerticalJump)
            {
                TryVerticalJump();
            } 
            else if (_triggerForwardLeap)
            {
                TryFowardLeap();
            }
            else if (_isHopping)//Handle Auto Jump
            {
                TryHop();
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
        void TryHop()
        {
            if (_isGrounded)
            {
                Debug.Log("TryHop Called Sucessfully");
                _velocity.y = Mathf.Sqrt(_hopHeight * -2f * _gravity);
            }
        }
        private void TryVerticalJump()
        {
            if (_isGrounded)
            {
                Debug.Log("TryVerticalJump Called Sucessfully");
                _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                _triggerVerticalJump = false;
            }
        }

        private void TryFowardLeap()
        {
            if (_isGrounded)
            {
                Debug.Log("TryVerticalJump Called Sucessfully");
                _velocity.y = Mathf.Sqrt(_leapHeight * -2f * _gravity);
                _triggerForwardLeap = false;
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

        #region Collapse State

        float _collapseStateRecoveryTimer;

        private void Handle_CollapseTrigger(float time)
        {
            _collapseStateTimer = Time.time + time;
            _isCollapsed = true;

            //disable other functions
            _isHopping = false;
            _triggerForwardLeap = false;
            _triggerVerticalJump = false;
            _isMovingSideToSide = false;
            _isHeadWobbling = false;
        }


        private void Update_Collapse()
        {
  
            if (_isCollapsed)
            {
                Debug.Log("Is Collapsed Currently");
                CheckForEndOfCollapseState();
                Animations_Collapse_KeyFrame();
            }
        }

        private void CheckForEndOfCollapseState()
        {
            if (_collapseStateTimer > Time.time)
                return;

            Debug.Log("Collapsed End");
            // Exit State
            _isCollapsed = false;
            _collapseStateRecoveryTimer = Time.time + 1f;
        }



        private void ReturnToStanding()
        {
            //_Root.fn_RotateTowardsoTargetAngleAtSpeed(3.61f, _collaspeLimbSpeed);
        }

        private void EndCollapseState()
        {
            _isHopping = false;
            _isMovingSideToSide = false;
        }


        void Animations_Collapse_KeyFrame()
        {
            _HeadOctagon.fn_RotateTowardsoTargetAngleAtSpeed(1.58f, _collaspeLimbSpeed - _collaspeLimbSpeed/8);
            _UperBodyOctagon.fn_RotateTowardsoTargetAngleAtSpeed(0.5f, _collaspeLimbSpeed);
            _LowerBodyOctagon.fn_RotateTowardsoTargetAngleAtSpeed(-0.3f, _collaspeLimbSpeed);
            _Root.fn_RotateTowardsoTargetAngleAtSpeed(1.57f, _collaspeLimbSpeed);
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




        internal void fn_SetMoveRight()
        {
            _MovingToTheRight = true;
        }

        internal void fn_SetMoveLeft()
        {
            _MovingToTheRight = false;
        }



        internal void fn_TryJump_Up()
        {
            // Trigger for next Update Pass where isGrounded
            _triggerVerticalJump = true;
        }

        internal void fn_TryJump_Forward()
        {
            // Trigger for next Update Pass where isGrounded
            _triggerForwardLeap = true;
        }


        internal void fn_Collapse(float time)
        {
            Handle_CollapseTrigger(time);

        }





        #endregion
    }
}