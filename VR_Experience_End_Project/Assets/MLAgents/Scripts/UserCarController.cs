using UnityEngine;

[RequireComponent(typeof(SimpleCarController))]
public class UserCarController : MonoBehaviour
{
    private SimpleCarController carController;

    void Start()
    {
        carController = GetComponent<SimpleCarController>();
    }

    void Update()
    {
        // WASD of pijltjes
        float throttle = Input.GetKey(KeyCode.W) ? 1f : (Input.GetKey(KeyCode.S) ? -1f : 0f);
        float steering = Input.GetKey(KeyCode.A) ? -1f : (Input.GetKey(KeyCode.D) ? 1f : 0f);
        float brake = Input.GetKey(KeyCode.Space) ? 1f : 0f;

        carController.SetInputs(throttle, steering, brake);
    }
}
