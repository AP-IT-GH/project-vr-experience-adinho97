using UnityEngine;
using UnityEngine.XR.Management;

public class DisableXR : MonoBehaviour
{
    void Start()
    {
        // This ensures the XR simulator doesn't persist when loading PC mode
        var simulator = GameObject.FindObjectOfType<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
        if (simulator != null)
        {
            Destroy(simulator.gameObject);
        }
    }
}
