using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace ScottBarley.IGB283.Assessment2
{
    public class OctagonAnimator : MonoBehaviour
    {
        [Header("Body Parts")]
        [SerializeField] OctagonArticulator _HeadOctagon;
        [SerializeField] OctagonArticulator _UperBodyOctagon;
        [SerializeField] OctagonArticulator _LowerBodyOctagon;
        [SerializeField] OctagonArticulator _Root;




        [Header("Head Wobble")]        
        [SerializeField] float _maxWobbleRotation = 1f;
        [SerializeField] float _wobbleRotationSpeed = 0.1f;
        [SerializeField] bool isWobbling = true;
        bool _HeadWobbleRotatingClockwise;

        [Header("movement - Sideways")]
        [SerializeField] float _speed;
        [SerializeField] private bool _isMovingSideToSide;
        
        IGB283Vector _startingPosition;
        IGB283Vector _currentPosition;

        private void Start()
        {
            //Getstarting postion
            _startingPosition = transform.position;
            _currentPosition = _startingPosition;
        }

        private void Update()
        {
            if (isWobbling) {
                headBob();
                UppderBoddyBob();
            }

            if (_isMovingSideToSide)
            {
                
            }
        }


        #region Sideways Movement
        void Move()
        {

        }


        #endregion

        #region Wobbling
        void headBob()
        {
            float currentAngle = _HeadOctagon.LastRotationAngle;

            // Flip Rotation Direction if over max value
            if (_HeadWobbleRotatingClockwise)
            {
                if (currentAngle > _maxWobbleRotation)
                    _HeadWobbleRotatingClockwise = !_HeadWobbleRotatingClockwise;
            } else {

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

        void UppderBoddyBob()
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