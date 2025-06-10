using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SimpleCarController))]
public class UniversalCarInput : MonoBehaviour
{
    private SimpleCarController car;
    private NewControls controls;

    void Awake()
    {
        controls = new NewControls();
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        car = GetComponent<SimpleCarController>();
    }

    void Update()
    {
        Vector2 move = controls.Car.Move.ReadValue<Vector2>();
        float brake = controls.Car.Brake.ReadValue<float>();

        float throttle = move.y;   // Up/Down from stick or W/S
        float steering = move.x;   // Left/Right from stick or A/D

        car.SetInputs(throttle, steering, brake);
    }
}
