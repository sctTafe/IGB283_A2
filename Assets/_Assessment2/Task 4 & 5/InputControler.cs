using UnityEngine;
namespace ScottBarley.IGB283.Assessment2.Task4
{
    /// <summary>
    /// 
    /// Instructions:
    /// 1.	Add C# Script code to steer your object around the screen using the keyboard. 
    /// •	Key ’a’ for left, ’d’ for right
    /// •	Key ’w’ for jumping up, key ’s’ for jumping forward
    /// 2.	When no key is pressed, your object should automatically move at a constant speed in the direction of its motion (no moving backward) 
    /// 3.	Add functionality so that when the ’z’ key is hit, keyboard control stops working and the avatar collapses to the ground.
    /// 4.	After-landing, QUT Jr lies slumped momentarily before rising to stand again. After this process, standard avatar controls may resume.
    /// </summary>
    public class InputControler : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private OctagonAnimator _controler;

        [Header("Key Bindings")]
        [SerializeField] KeyCode Left = KeyCode.A;
        [SerializeField] KeyCode Right = KeyCode.D;
        [SerializeField] KeyCode Up = KeyCode.W;
        [SerializeField] KeyCode Down = KeyCode.S;
        [SerializeField] KeyCode Special = KeyCode.Z;

        float _collapseTime = 2f;

        float _controlerDisabledTime;
        bool _isControlesDisabled;


        void Update()
        {
            // Controles Disabled
            Update_Timer();
            if (_isControlesDisabled)
                return;



            // Set Movement Left
            if (Input.GetKeyDown(Left))
            {
                Debug.Log("Left pressed");
                _controler.fn_SetMoveLeft();
            }

            // Set Movement Right
            if (Input.GetKeyDown(Right))
            {
                Debug.Log("Right pressed");
                _controler.fn_SetMoveRight();
            }

            // Jumping Up
            if (Input.GetKeyDown(Up))
            {
                Debug.Log("Up pressed");
                _controler.fn_TryJump_Up();
            }

            // Jumping Forward
            if (Input.GetKeyDown(Down))
            {
                Debug.Log("Down pressed");
                _controler.fn_TryJump_Forward();
            }

            // Collapses 
            if (Input.GetKeyDown(Special))
            {
                Debug.Log("Floppy pressed");
                _controler.fn_Collapse(_collapseTime);
                _controlerDisabledTime = Time.time + _collapseTime;
            }
        }

        private void Update_Timer()
        {
            if (!_isControlesDisabled)
                return;

            if(_controlerDisabledTime <= Time.time)
                _isControlesDisabled = false;                  
        }
    }
}