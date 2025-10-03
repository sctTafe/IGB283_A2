using UnityEngine;
using UnityEngine.InputSystem;

public class ControlPoint : MonoBehaviour
{
    [HideInInspector]
    public bool IsMoving = false;

    private Vector3 GetMousePosition()
    {
        // Find the mouse position in world space
        Vector3 mouseInput = Mouse.current.position.ReadValue();
        mouseInput.z = transform.position.z -
       Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mouseInput);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsMoving)
        {
            // Set the control point's position to the mouse position
            transform.position = GetMousePosition();
        }

    }
}
