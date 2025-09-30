using System;
using System.Collections;
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
        [SerializeField] OctagonArticulator _LArm;
        [SerializeField] OctagonArticulator _RArm;

        [Header("Debugging")]
        [SerializeField] bool _isDebugging;

        // -- Movment --

        [Header("Movement - Sideways")]
        [SerializeField] private float _horizontalDragCoefficient = 0.5f;   // strength of drag
        [SerializeField] private float _horizontalDragPower = 2f;           // Growth of drag strength to speed; 1 = linear, 2 = quadratic, 3 = cubic
        [SerializeField] private bool _isMovingSideToSide;
        [SerializeField] float _speed = 2f;
        [SerializeField] bool _MovingToTheRight;

        [Header("Boundaries - Sides")]
        [SerializeField] private bool _isAutoDirectionChange;
        [SerializeField] float _xMaxBoundary = 15f;
        [SerializeField] float _xMinBoundary = -15f;

        [Header("Movement - Hopping")]
        [SerializeField] bool _isHopping = true;
        [SerializeField] float _hopHeight = 0.0010f;
        [Header("Movement - Leaping")]
        [SerializeField] float _leapHeight = 0.0035f;
        [SerializeField] float _leapForwardSpeed = 6f;
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
        private collapseStateStage _currentCollapseState;
        float _collaspeLimbSpeed = 2f;

        IGB283Vector _startingPosition;
        IGB283Vector _currentPosition;
        IGB283Vector _velocity = new IGB283Vector();


        public enum AnimationsState
        {
            Hopping,
            Jumping,
            FowardLeaping,
            Collapsed
        }

        AnimationsState _currentAimationState = AnimationsState.Hopping;

        #region Unity Native
        private void Start()
        {
            //Get starting postion of root
            _startingPosition = transform.position;
            _currentPosition = _startingPosition;
        }

        private void Update()
        {
            // -- Character Movement --
            Update_VelocityVertical();
            Update_HorizontalVelocity();
            Update_SpecialVelocity(); //Leap & Hop Changes
            Update_CurrentPoisitonAndMove();


            // -- Animation --
            Update_CollapseStateAnimations();

            switch (_currentAimationState)
            {
                case AnimationsState.Hopping:
                    Update_AnimationState_Hopping();
                    break;
                case AnimationsState.Jumping:
                    Update_AnimationState_Jumping();
                    break;
                case AnimationsState.FowardLeaping:
                    break;
                case AnimationsState.Collapsed:
                    break;

                default:
                    break;
            }

           
            // -- Debugging --
            if (_isDebugging)
            {
                Debug.Log($"Velocity {_velocity}");
                Debug.Log($"Current Pos {_currentPosition}");
            }

        }




        #endregion

        #region Public
        public void fn_SetMoveRight()
        {
            if (_isDebugging) Debug.Log("fn_SetMoveRight Called");
            _MovingToTheRight = true;
            _velocity.x = 0;
        }

        public void fn_SetMoveLeft()
        {
            if (_isDebugging) Debug.Log("fn_SetMoveLeft Called");
            _MovingToTheRight = false;
            _velocity.x = 0;
        }

        public void fn_TryJump_Up()
        {
            if (_isDebugging) Debug.Log("fn_TryJump_Up Called");
            // Trigger for next Update Pass where isGrounded
            _triggerVerticalJump = true;
        }

        public void fn_TryJump_Forward()
        {
            if (_isDebugging) Debug.Log("fn_TryJump_Forward Called");
            // Trigger for next Update Pass where isGrounded
            _triggerForwardLeap = true;
        }

        public void fn_Collapse(float time)
        {
            Handle_CollapseTrigger(time);

        }
        #endregion

        #region Movement
        void Update_HorizontalVelocity()
        {

            if (!_isMovingSideToSide)
            {
                _velocity.x = 0;
                return;
            }

            //Boundary Interaction - Handles Auto change in driection
            if (_isAutoDirectionChange)
            {
                var currentX = _currentPosition.x;
                if (currentX < _xMinBoundary)
                {
                    _MovingToTheRight = true;
                    _velocity.x = 0;
                }

                if (currentX > _xMaxBoundary)
                {
                    _MovingToTheRight = false;
                    _velocity.x = 0;
                }
            }
            
            var absVel = Mathf.Abs(_velocity.x);
            //// Drag
            if (absVel > _speed)
            {
                float drag = _horizontalDragCoefficient * Mathf.Pow(Mathf.Abs(_velocity.x), _horizontalDragPower);
                
                // Reduce velocity toward 0, preserving sign
                if (_velocity.x > 0f)
                    _velocity.x = Mathf.Max(0f, _velocity.x - drag);
                else
                    _velocity.x = Mathf.Min(0f, _velocity.x + drag);
            }

            if(absVel < _speed)
            {
                // Base hopping velocity
                _velocity.x = (_MovingToTheRight ? 1f : -1f) * _speed;
            }
        }


        void Update_VelocityVertical()
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
        }

        void Update_SpecialVelocity()
        {
            // If in Collapse state, stop taking input from jumping
            if (_isCollapsed)
                return;

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
            var stepVelocity = _velocity * Time.deltaTime;

            _currentPosition += stepVelocity;
            _Root.fn_Move(stepVelocity);
        }


        void UpdateVerticalVelocity_ApplyGravity()
        {
            // Apply gravity
            _velocity.y += _gravity;

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
                if(_isDebugging) Debug.Log("TryHop Called Sucessfully");
                _velocity.y = Mathf.Sqrt(_hopHeight * -2f * _gravity);
            }
        }
        private void TryVerticalJump()
        {
            if (_isGrounded)
            {
                if (_isDebugging) Debug.Log("TryVerticalJump Called Sucessfully");
                _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                _triggerVerticalJump = false;

                Trigger_JumpAnimationState();
            }
        }

        private void TryFowardLeap()
        {
            if (_isGrounded)
            {
                if (_isDebugging) Debug.Log("TryVerticalJump Called Sucessfully");
                _velocity.y = Mathf.Sqrt(_leapHeight * -2f * _gravity);
                _velocity.x = (_MovingToTheRight ? 1f : -1f) * _leapForwardSpeed;
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
        enum collapseStateStage
        {
            onFloor,
            gettingUp
        }
        private void Handle_CollapseTrigger(float time)
        {          
            _isCollapsed = true;
            _currentCollapseState = collapseStateStage.onFloor;
            StartCoroutine(WaitAndDo(time, Handle_EndOnFloorStage));

            //disable other functions
            _isHopping = false;
            _triggerForwardLeap = false;
            _triggerVerticalJump = false;
            _isMovingSideToSide = false;
            _isHeadWobbling = false;
        }
        void Handle_EndOnFloorStage()
        {
            _currentCollapseState = collapseStateStage.gettingUp;
            StartCoroutine(WaitAndDo(1.2f, Handle_EndCollapseState));
        }

        private void Handle_EndCollapseState()
        {       
            _isCollapsed = false;

            _velocity = IGB283Vector.Zero;
            //reenable other functions
            _isHopping = true;
            _isMovingSideToSide = true;
            _isHeadWobbling = true;
        }


        // Main Collapse Handling Loop
        private void Update_CollapseStateAnimations()
        {
            if (!_isCollapsed)
                return;
            
            Debug.Log("Is Collapsed Currently");

            //Collapsed Animation State
            if(_currentCollapseState == collapseStateStage.onFloor)
                Animations_Collapse_KeyFrame();
            else
                Animations_StoodUp_KeyFrame();                     
        }


        public IEnumerator WaitAndDo(float delay, Action action)
        { 
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
        #endregion


        private float DegToRad(float deg) => deg * (Mathf.PI / 180f);




        #region Animations


        private void Update_AnimationState_Hopping()
        {
            if (_isHeadWobbling)
            {
                headBob();
                UpperBodyBob();
            }

            if (_isMovingSideToSide)
            {
                LeanInDirectionOfTravel();
            }
        }



        #region Sun - Jumping
        public enum jumpingStateStage
        {
            jumpPrep,
            JumpMain
        }
        jumpingStateStage _currentJumpingStageState;
        void Update_AnimationState_Jumping()
        {
            if (_currentJumpingStageState == jumpingStateStage.jumpPrep)
                Animations_Jump_Prep();
            else
                Animation_Jump_ArmsUp();
        }

        private void Trigger_JumpAnimationState()
        {
            _currentAimationState = AnimationsState.Jumping;
            _currentJumpingStageState = jumpingStateStage.jumpPrep;
            StartCoroutine(WaitAndDo(0.1f, Handle_EndOfJumpPrepStage));
        }

        void Handle_EndOfJumpPrepStage()
        {
            _currentJumpingStageState = jumpingStateStage.JumpMain;
            StartCoroutine(WaitAndDo(0.5f, Handle_EndOfJump));
        }

        void Handle_EndOfJump()
        {
            _currentAimationState = AnimationsState.Hopping;
        }


        void Animations_Jump_Prep()
        {
            //Debug.Log("Animations_Jump_Prep Called");
            _Root.fn_RotateToTargetAngle_DownChain(0f, _collaspeLimbSpeed * 5);
            //_RArm?.fn_RotateDownChain(DegToRad(30), 0.5f);
            //_RArm?.fn_RotateDownChain(DegToRad(-30), 0.5f);

            _RArm?.fn_RotateTowardsoTargetAngleAtSpeed(DegToRad(-45), _collaspeLimbSpeed * 10 );
            _LArm?.fn_RotateTowardsoTargetAngleAtSpeed(DegToRad(-45), _collaspeLimbSpeed * 10);
        }
        void Animation_Jump_ArmsUp()
        {
            _Root.fn_RotateToTargetAngle_DownChain(0f, _collaspeLimbSpeed * 5);

            //_RArm?.fn_RotateDownChain(DegToRad(-50), _collaspeLimbSpeed * 10);
            //_RArm?.fn_RotateDownChain(DegToRad(+50), _collaspeLimbSpeed * 10);


            _RArm?.fn_RotateTowardsoTargetAngleAtSpeed(DegToRad(45), _collaspeLimbSpeed * 10);
            _LArm?.fn_RotateTowardsoTargetAngleAtSpeed(DegToRad(-45), _collaspeLimbSpeed * 10);
        }
        #endregion

        #region Sub - Collapse
        void Animations_Collapse_KeyFrame()
        {
            _HeadOctagon.fn_RotateTowardsoTargetAngleAtSpeed(1.58f, _collaspeLimbSpeed - _collaspeLimbSpeed / 8);
            _UperBodyOctagon.fn_RotateTowardsoTargetAngleAtSpeed(0.5f, _collaspeLimbSpeed);
            _LowerBodyOctagon.fn_RotateTowardsoTargetAngleAtSpeed(-0.3f, _collaspeLimbSpeed);
            _Root.fn_RotateTowardsoTargetAngleAtSpeed(1.57f, _collaspeLimbSpeed);

            _LArm?.fn_RotateTowardsoTargetAngleAtSpeed(DegToRad(90), _collaspeLimbSpeed);
            _RArm?.fn_RotateTowardsoTargetAngleAtSpeed(DegToRad(-90), _collaspeLimbSpeed);
        }

        void Animations_StoodUp_KeyFrame()
        {
            _HeadOctagon.fn_RotateTowardsoTargetAngleAtSpeed(0f, _collaspeLimbSpeed);
            _UperBodyOctagon.fn_RotateTowardsoTargetAngleAtSpeed(0f, _collaspeLimbSpeed);
            _LowerBodyOctagon.fn_RotateTowardsoTargetAngleAtSpeed(0f, _collaspeLimbSpeed);
            _Root.fn_RotateTowardsoTargetAngleAtSpeed(0f, _collaspeLimbSpeed);
                     
            _LArm?.fn_RotateTowardsoTargetAngleAtSpeed(DegToRad(0), _collaspeLimbSpeed);
            _RArm?.fn_RotateTowardsoTargetAngleAtSpeed(DegToRad(0), _collaspeLimbSpeed);
        }
        #endregion

        #region Sub - Hopping
        /// <summary>
        /// Lean in the direction of travel from the lower body pivot point
        /// </summary>
        private void LeanInDirectionOfTravel()
        {
            // Not Leaning
            if (_velocity.x == 0f)
            {
                _LowerBodyOctagon.fn_RotateTowardsoTargetAngleAtSpeed(0f, 0.5f);
            }

            // moving right
            if (_velocity.x > 0.001f)
            {
                //_LowerBodyOctagon.fn_RotateToZero();
                //_LowerBodyOctagon.fn_RotatePartAroundPivot(-_leanAngle);
                _LowerBodyOctagon.fn_RotateTowardsoTargetAngleAtSpeed(-_leanAngle, 0.5f);

                _LArm?.fn_RotateToTargetAngle_DownChain(DegToRad(0), _collaspeLimbSpeed/2f);
                _LArm?.fn_RotateTowardsoTargetAngleAtSpeed(DegToRad(50), _collaspeLimbSpeed);
                _RArm?.fn_RotateToTargetAngle_DownChain(DegToRad(10), _collaspeLimbSpeed);
            }

            // moving left
            if (_velocity.x < -0.001f)
            {
                //_LowerBodyOctagon.fn_RotateToZero();
                //_LowerBodyOctagon.fn_RotatePartAroundPivot(_leanAngle);
                _LowerBodyOctagon.fn_RotateTowardsoTargetAngleAtSpeed(_leanAngle, 0.5f);

                _LArm?.fn_RotateToTargetAngle_DownChain(DegToRad(-10), _collaspeLimbSpeed);
                _RArm?.fn_RotateToTargetAngle_DownChain(DegToRad(0), _collaspeLimbSpeed / 2f);
                _RArm?.fn_RotateTowardsoTargetAngleAtSpeed(DegToRad(-50), _collaspeLimbSpeed);
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

        #endregion
    }
}