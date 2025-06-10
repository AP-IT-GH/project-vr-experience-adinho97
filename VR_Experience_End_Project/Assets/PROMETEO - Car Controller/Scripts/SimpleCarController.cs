using UnityEngine;

public class SimpleCarController : MonoBehaviour
{
    [Header("Wheel Meshes")]
    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearLeftMesh;
    public Transform rearRightMesh;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    [Header("Effects (optioneel)")]
    public ParticleSystem leftTireSmoke;
    public ParticleSystem rightTireSmoke;
    public TrailRenderer leftTireSkid;
    public TrailRenderer rightTireSkid;

    [Header("Car Settings")]
    public float maxMotorTorque = 1500f;
    public float maxSteerAngle = 30f;
    public float brakeForce = 3000f;
    public float handbrakeDriftMultiplier = 2f;

    // Publieke instantievariabele voor snelheid (m/s)
    public float speed;

    private float motorInput;
    private float steerInput;
    private float brakeInput;
    private bool handbrake;
    private float clutch;
    private float originalMotorTorque;
   

    // Publieke functie voor AI/Agent input
    public void SetInputs(float throttle, float steering,  float brake)
    {
        motorInput = Mathf.Clamp(throttle, -1f, 1f);
        steerInput = Mathf.Clamp(steering, -1f, 1f);
        brakeInput = Mathf.Clamp01(brake);
        handbrake = brakeInput > 0.9f; // optioneel: handrem bij hoge remwaarde
    }

    void Awake()
    {
        originalMotorTorque = maxMotorTorque;
    }

    void FixedUpdate()
    {
        // Motor
        float motor = motorInput * maxMotorTorque;
        frontLeftCollider.motorTorque = motor;
        frontRightCollider.motorTorque = motor;

        // Sturen
        float steer = steerInput * maxSteerAngle;
        frontLeftCollider.steerAngle = steer;
        frontRightCollider.steerAngle = steer;

        // Remmen
        float brake = brakeInput * brakeForce;
        frontLeftCollider.brakeTorque = brake;
        frontRightCollider.brakeTorque = brake;
        rearLeftCollider.brakeTorque = brake;
        rearRightCollider.brakeTorque = brake;

        // Handrem/Drift
        if (handbrake)
        {
            var friction = rearLeftCollider.sidewaysFriction;
            friction.stiffness = 1f / handbrakeDriftMultiplier;
            rearLeftCollider.sidewaysFriction = friction;
            rearRightCollider.sidewaysFriction = friction;
            if (leftTireSmoke) leftTireSmoke.Play();
            if (rightTireSmoke) rightTireSmoke.Play();
            if (leftTireSkid) leftTireSkid.emitting = true;
            if (rightTireSkid) rightTireSkid.emitting = true;
        }
        else
        {
            var friction = rearLeftCollider.sidewaysFriction;
            friction.stiffness = 1f;
            rearLeftCollider.sidewaysFriction = friction;
            rearRightCollider.sidewaysFriction = friction;
            if (leftTireSmoke) leftTireSmoke.Stop();
            if (rightTireSmoke) rightTireSmoke.Stop();
            if (leftTireSkid) leftTireSkid.emitting = false;
            if (rightTireSkid) rightTireSkid.emitting = false;
        }

        // Snelheid berekenen (m/s)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            speed = rb.linearVelocity.magnitude;
        }
        else
        {
            // Fallback: schat snelheid via wheel rpm
            speed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000f;
        }
       
        // Wheel mesh animatie
        UpdateWheelMesh(frontLeftCollider, frontLeftMesh);
        UpdateWheelMesh(frontRightCollider, frontRightMesh);
        UpdateWheelMesh(rearLeftCollider, rearLeftMesh);
        UpdateWheelMesh(rearRightCollider, rearRightMesh);
    }

    void UpdateWheelMesh(WheelCollider col, Transform mesh)
    {
        Vector3 pos;
        Quaternion rot;
        col.GetWorldPose(out pos, out rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }

    public void SetBrakingZoneTorque(bool inBrakingZone)
    {
        if (inBrakingZone)
            maxMotorTorque = 100f;
        else
            maxMotorTorque = originalMotorTorque;
    }
} 