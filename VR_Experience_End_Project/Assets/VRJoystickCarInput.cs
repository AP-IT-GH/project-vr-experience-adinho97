using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SimpleCarController))]
public class VRJoystickCarInput : MonoBehaviour
{
    private SimpleCarController car;

    void Start()
    {
        car = GetComponent<SimpleCarController>();
    }

    void Update()
    {
        if (Gamepad.current == null) return;

        Vector2 move = Gamepad.current.leftStick.ReadValue();

        float throttle = move.y;   // Forward/backward
        float steering = move.x;   // Left/right
        float brake = Gamepad.current.buttonSouth.isPressed ? 1f : 0f; // A button as brake

        car.SetInputs(throttle, steering, brake);
    }
}
