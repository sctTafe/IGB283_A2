using UnityEngine;
using UnityEngine.UI;
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

        [Header("Btn Inputs")]
        [SerializeField] Button button_Left;
        [SerializeField] Button button_Right;
        [SerializeField] Button button_Up;
        [SerializeField] Button button_Down;
        [SerializeField] Button button_Collapse;

        float _collapseTime = 2f;

        float _controlerDisabledTime;
        bool _isControlesDisabled;

        private void Start()
        {
            if (button_Left != null) button_Left.onClick.AddListener(DoLeft);
            if (button_Right != null) button_Right.onClick.AddListener(DoRight);
            if (button_Up != null) button_Up.onClick.AddListener(DoUpKey);
            if (button_Down != null) button_Down.onClick.AddListener(DoDownKey);
            if (button_Collapse != null) button_Collapse.onClick.AddListener(DoSpecial);
        }

        void Update()
        {
            // Controles Disabled
            Update_Timer();
            if (_isControlesDisabled)
                return;



            // Set Movement Left
            if (Input.GetKeyDown(Left))
            {
                DoLeft();
            }

            // Set Movement Right
            if (Input.GetKeyDown(Right))
            {
                DoRight();
            }

            // Jumping Up
            if (Input.GetKeyDown(Up))
            {
                DoUpKey();
            }

            // Jumping Forward
            if (Input.GetKeyDown(Down))
            {
                DoDownKey();
            }

            // Collapses 
            if (Input.GetKeyDown(Special))
            {
                DoSpecial();
            }
        }

        private void DoSpecial()
        {
            Debug.Log("Floppy Input");
            _controler.fn_Collapse(_collapseTime);
            _controlerDisabledTime = Time.time + _collapseTime;
        }

        private void DoDownKey()
        {
            Debug.Log("Down Input");
            _controler.fn_TryJump_Forward();
        }

        private void DoUpKey()
        {
            Debug.Log("Up Input");
            _controler.fn_TryJump_Up();
        }

        private void DoRight()
        {
            Debug.Log("Right Input");
            _controler.fn_SetMoveRight();
        }

        private void DoLeft()
        {
            Debug.Log("Left Input");
            _controler.fn_SetMoveLeft();
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