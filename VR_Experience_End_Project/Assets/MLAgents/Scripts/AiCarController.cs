using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(SimpleCarController))]
public class AiCarController : MonoBehaviour {
    public WaypointContainer waypointContainer;
    public List<Transform> waypoints;
    public float waypointTimeout = 5f;
    public float maxSpeed = 20f;
    public float acceleration = 10f;
    public float deceleration = 15f;
    public float turnSpeed = 100f;
    public float waypointRadius = 5f;
    public float brakingDistance = 10f;
    public float maxBrakingForce = 1f;
    public float timeoutPerMeter = 0.5f;
    public float baseWaypointTimeout = 5f;
    public float maxCollisions = 3f;
    public float collisionResetTime = 5f;
    public SimpleCarController carController;
    private float currentAngle;
    private float gasInput;
    public float gasDampen;
    public bool isInsideBraking;
    public float maximumAngle = 45f;
    public float maximumSpeed = 120f;
    public float brakingZoneSpeedMultiplier = 0.5f;
    private float waypointTimer;
    private float currentTimeout;
    private bool isFirstWaypoint = true;
    private int currentWaypoint = 0;

    // Collision tracking for play mode
    private int collisionCount = 0;
    private float lastCollisionTime;

    // Store initial position and rotation
    private Vector3 startPosition;
    private Quaternion startRotation;

    [Range(0.01f, 0.04f)]
    public float turningConstant = 0.02f;

    private Rigidbody carRigidbody;
    private LineRenderer waypointLine;
    private float frameCounter = 0f;
    private const float LOG_INTERVAL = 60f; // Log every 60 frames

    public void Start() {
        carController = GetComponent<SimpleCarController>();
        if (carController == null)
        {
            Debug.LogError("No SimpleCarController found on car!");
            return;
        }

        carRigidbody = GetComponent<Rigidbody>();
        if (carRigidbody == null)
        {
            Debug.LogError("No Rigidbody found on car!");
            return;
        }

        waypoints = waypointContainer.waypoints;
        currentWaypoint = 0;
        waypointTimer = 0f;
        isFirstWaypoint = true;
        collisionCount = 0;
        lastCollisionTime = 0f;
        
        // Store initial position and rotation
        startPosition = transform.position;
        startRotation = transform.rotation;
        
        // Initialize waypoint line
        if (waypointLine == null)
        {
            waypointLine = gameObject.AddComponent<LineRenderer>();
            waypointLine.startWidth = 0.1f;
            waypointLine.endWidth = 0.1f;
            waypointLine.material = new Material(Shader.Find("Sprites/Default"));
            waypointLine.startColor = Color.yellow;
            waypointLine.endColor = Color.red;
        }
        
        // Set initial waypoint line
        if (waypoints.Count > 0)
        {
            waypointLine.SetPosition(0, transform.position);
            waypointLine.SetPosition(1, waypoints[0].position);
        }
        
        // Initialize timeout
        UpdateWaypointTimeout();
        
        Debug.Log($"AiCarController initialized at position: {startPosition}, rotation: {startRotation.eulerAngles}");
        if (waypoints.Count > 0)
        {
            Debug.Log($"First waypoint position: {waypoints[0].position}, Distance: {Vector3.Distance(transform.position, waypoints[0].position)}m");
            Debug.Log($"Initial timeout set to: {currentTimeout} seconds");
        }
    }

    private void UpdateWaypointTimeout()
    {
        if (waypoints.Count > 0 && currentWaypoint < waypoints.Count)
        {
            float distance = Vector3.Distance(transform.position, waypoints[currentWaypoint].position);
            currentTimeout = baseWaypointTimeout + (distance * timeoutPerMeter);
            Debug.Log($"Updated timeout for waypoint {currentWaypoint}: {currentTimeout} seconds (Distance: {distance}m)");
        }
    }

    public void ResetCar()
    {
        Debug.Log("Resetting car position and rotation");
        
        // Reset waypoint first
        currentWaypoint = 0;
        waypointTimer = 0f;
        isFirstWaypoint = true;
        
        // Reset car controller inputs
        if (carController != null)
        {
            carController.SetInputs(0f, 0f, 0f);
        }
        
        // Reset physics
        if (carRigidbody != null)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            carRigidbody.angularVelocity = Vector3.zero;
            carRigidbody.Sleep();
        }
        
        // Reset position and rotation
        transform.position = startPosition;
        transform.rotation = startRotation;
        
        // Wake up physics after position/rotation is set
        if (carRigidbody != null)
        {
            carRigidbody.WakeUp();
        }
        
        // Reset waypoint line after position is set
        if (waypointLine != null && waypoints.Count > 0)
        {
            waypointLine.SetPosition(0, transform.position);
            waypointLine.SetPosition(1, waypoints[0].position);
        }
        
        // Update timeout for new waypoint
        UpdateWaypointTimeout();
        
        Debug.Log($"Car reset complete - Position: {transform.position}, Rotation: {transform.rotation.eulerAngles}");
        if (waypoints.Count > 0)
        {
            Debug.Log($"Distance to first waypoint after reset: {Vector3.Distance(transform.position, waypoints[0].position)}m");
        }
    }

    public void Update() {
        if (waypoints.Count > 0) {
            // Update waypoint timer
            waypointTimer += Time.deltaTime;
            
            // Update waypoint line
            UpdateWaypointLine();
            
            // Calculate distance to current waypoint
            float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypoint].position);
            
            // Check if we've reached the waypoint
            if (distanceToWaypoint < waypointRadius) {
                Debug.Log($"Waypoint reached: {waypoints[currentWaypoint].name} (Index: {currentWaypoint})");
                currentWaypoint++;
                if (currentWaypoint >= waypoints.Count) currentWaypoint = 0;
                waypointTimer = 0f; // Reset timer when reaching waypoint
                isFirstWaypoint = false;
                UpdateWaypointTimeout(); // Update timeout for new waypoint
                Debug.Log($"New target waypoint: {waypoints[currentWaypoint].name} (Index: {currentWaypoint})");
            }
            // Check if we've exceeded the waypoint timeout
            else if (waypointTimer > currentTimeout && !isFirstWaypoint) {
                Debug.Log($"Waypoint timeout reached after {waypointTimer} seconds (Timeout was {currentTimeout}s) - resetting car and waypoint");
                ResetCar();
                return;
            }

            // Calculate steering angle
            Vector3 fwd = transform.TransformDirection(Vector3.forward);
            currentAngle = Vector3.SignedAngle(fwd, waypoints[currentWaypoint].position - transform.position, Vector3.up);
            
            // Calculate gas input based on angle and speed
            float targetSpeed = maximumSpeed;
            if (isInsideBraking) {
                targetSpeed *= brakingZoneSpeedMultiplier;
                Debug.Log($"In braking zone - Target speed reduced to: {targetSpeed}");
            }
            
            // More aggressive gas input for first waypoint
            if (isFirstWaypoint)
            {
                gasInput = 1f; // Full gas for first waypoint
                Debug.Log($"First waypoint - Full gas input");
            }
            else
            {
                gasInput = Mathf.Clamp01((1f - Mathf.Abs(carController.speed * 0.02f * currentAngle / maximumAngle)));
            }
            
            // Apply braking in braking zones
            if (isInsideBraking) {
                gasInput = -gasInput * ((Mathf.Clamp01((carController.speed) / targetSpeed) * 2 - 1f));
                Debug.Log($"Applying brake - Gas input: {gasInput}, Current speed: {carController.speed}");
            }
            
            // Smooth gas input
            gasDampen = Mathf.Lerp(gasDampen, gasInput, Time.deltaTime * 3f);
            
            // Set car inputs
            carController.SetInputs(gasDampen, currentAngle, isInsideBraking ? 1f : 0f);
            
            // Draw debug ray to waypoint
            Debug.DrawRay(transform.position, waypoints[currentWaypoint].position - transform.position, Color.yellow);
            
            // Log distance to waypoint periodically
            frameCounter++;
            if (frameCounter >= LOG_INTERVAL)
            {
                frameCounter = 0f;
                Debug.Log($"Distance to waypoint {currentWaypoint}: {distanceToWaypoint}, Speed: {carController.speed}, Time left: {currentTimeout - waypointTimer:F1}s");
            }
        }
    }

    private void UpdateWaypointLine()
    {
        if (waypointLine != null && currentWaypoint < waypoints.Count)
        {
            waypointLine.SetPosition(0, transform.position);
            waypointLine.SetPosition(1, waypoints[currentWaypoint].position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"AiCarController: Trigger entered with {other.gameObject.name} (Tag: {other.tag})");
        if (other.CompareTag("Brakingpoint"))
        {
            isInsideBraking = true;
            Debug.Log("AiCarController: Entered braking zone - Applying brakes");
            // Force brake input when entering braking zone
            carController.SetInputs(0f, 0f, 1f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"AiCarController: Trigger exited with {other.gameObject.name} (Tag: {other.tag})");
        if (other.CompareTag("Brakingpoint"))
        {
            isInsideBraking = false;
            Debug.Log("AiCarController: Exited braking zone - Releasing brakes");
            // Reset brake input when exiting braking zone
            carController.SetInputs(0f, 0f, 0f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Check if we're within the collision time window
            if (Time.time - lastCollisionTime < collisionResetTime)
            {
                collisionCount++;
                Debug.Log($"Collision with wall #{collisionCount} in {Time.time - lastCollisionTime:F1}s");
                
                if (collisionCount >= maxCollisions)
                {
                    Debug.Log("Too many collisions in short time - resetting car");
                    ResetCar();
                    collisionCount = 0;
                }
            }
            else
            {
                // Reset collision count if too much time has passed
                collisionCount = 1;
                Debug.Log("First collision in new time window");
            }
            
            lastCollisionTime = Time.time;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Check if we're stuck against the wall
            if (carController.speed < 1f && Time.time - lastCollisionTime > 2f)
            {
                Debug.Log("Car appears to be stuck against wall - resetting");
                ResetCar();
                collisionCount = 0;
                lastCollisionTime = Time.time;
            }
        }
    }
}