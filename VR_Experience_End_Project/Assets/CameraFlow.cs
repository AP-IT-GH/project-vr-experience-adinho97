using UnityEngine;

public class CameraFlow : MonoBehaviour
{
    public Transform carTransform; 
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Adjust the offset to match the driver's seat position

    void LateUpdate()
    {
        if (carTransform != null)
        {

            transform.position = carTransform.position + carTransform.TransformVector(offset);

            transform.rotation = carTransform.rotation;
        }
    }
}
