using UnityEngine;

[RequireComponent(typeof(SimpleCarController))]
public class CarInputFromActions : MonoBehaviour
{
    private SimpleCarController car;
    private NewControls controls;  // <--- your generated input class

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

        car.SetInputs(move.y, move.x, brake);
    }
}
