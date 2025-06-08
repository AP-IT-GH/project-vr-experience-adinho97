using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgent : Agent
{
    public SimpleCarController carController;
    public Rigidbody carRigidbody;
    public Vector3 startPosition;
    public Quaternion startRotation;
    public float lastCheckpointTime;
    public int currentCheckpoint = 0;
    public float episodeStartTime;
    public bool isInBrakingZone = false;
    public float waypointTimeout = 7f;

    [Header("Training Settings")]
    public Transform[] checkpoints;
    public float maxTimePerEpisode = 60f;
    public float checkpointTimeout = 10f;
    public float speedRewardMultiplier = 0.05f;
    public float checkpointReward = 2f;
    public float crashPenalty = 1f;
    public float brakingZoneReward = 0.2f;
    public float waypointTimeoutPenalty = 2f;
    public float progressReward = 0.05f;
    public float completionReward = 10f;

    private float lastDistanceToCheckpoint = float.MaxValue;
    private float lastCheckpointDistance = float.MaxValue;
    private int highestCheckpointReached = 0;

    private void Awake()
    {
        // Initialize components in Awake to ensure they're available
        if (carController == null)
            carController = GetComponent<SimpleCarController>();
        if (carRigidbody == null)
            carRigidbody = GetComponent<Rigidbody>();
    }

    public override void Initialize()
    {
        // Store initial position and rotation
        startPosition = transform.position;
        startRotation = transform.rotation;
        
        // Ensure we have all required components
        if (carController == null)
            carController = GetComponent<SimpleCarController>();
        if (carRigidbody == null)
            carRigidbody = GetComponent<Rigidbody>();

        // Verify checkpoints are set
        if (checkpoints == null || checkpoints.Length == 0)
        {
            Debug.LogError("No checkpoints assigned to CarAgent!");
        }
    }

    public override void OnEpisodeBegin()
    {
        // Reset car position and rotation
        transform.position = startPosition;
        transform.rotation = startRotation;
        
        // Reset physics
        if (carRigidbody != null)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            carRigidbody.angularVelocity = Vector3.zero;
            carRigidbody.Sleep();
            carRigidbody.WakeUp();
        }
        
        // Reset training variables
        currentCheckpoint = 0;
        lastCheckpointTime = Time.time;
        episodeStartTime = Time.time;
        isInBrakingZone = false;
        lastDistanceToCheckpoint = float.MaxValue;
        lastCheckpointDistance = float.MaxValue;

        // Reset car controller inputs
        if (carController != null)
        {
            carController.SetInputs(0f, 0f, 0f);
        }

        Debug.Log($"Episode {Academy.Instance.EpisodeCount} started - Position: {transform.position}");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (carRigidbody == null) return;

        // Add car's velocity (normalized)
        sensor.AddObservation(carRigidbody.linearVelocity.normalized);
        
        // Add car's angular velocity (normalized)
        sensor.AddObservation(carRigidbody.angularVelocity.normalized);
        
        // Add car's rotation (as euler angles, normalized)
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        sensor.AddObservation(new Vector3(
            eulerAngles.x / 360f,
            eulerAngles.y / 360f,
            eulerAngles.z / 360f
        ));
        
        // Add distance to next checkpoint (normalized)
        if (currentCheckpoint < checkpoints.Length && checkpoints[currentCheckpoint] != null)
        {
            float distanceToCheckpoint = Vector3.Distance(transform.position, checkpoints[currentCheckpoint].position);
            sensor.AddObservation(distanceToCheckpoint / 100f); // Normalize by dividing by max expected distance

            // Add direction to next checkpoint (normalized)
            Vector3 directionToCheckpoint = (checkpoints[currentCheckpoint].position - transform.position).normalized;
            sensor.AddObservation(directionToCheckpoint);

            // Add angle to next checkpoint (normalized)
            float angleToCheckpoint = Vector3.SignedAngle(transform.forward, directionToCheckpoint, Vector3.up);
            sensor.AddObservation(angleToCheckpoint / 180f); // Normalize to [-1, 1]
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }

        // Add whether we're in a braking zone
        sensor.AddObservation(isInBrakingZone ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (carController == null) return;

        // Get actions from the agent
        float throttle = actions.ContinuousActions[0];
        float steering = actions.ContinuousActions[1];
        float brake = actions.ContinuousActions[2];
      
        // Apply actions to car
        carController.SetInputs(throttle, steering, brake);

        // Check waypoint timeout
        if (Time.time - lastCheckpointTime > waypointTimeout)
        {
            Debug.Log($"Waypoint timeout at checkpoint {currentCheckpoint}");
            AddReward(-waypointTimeoutPenalty);
            EndEpisode();
            return;
        }

        // Episode timeout
        if (Time.time - episodeStartTime > maxTimePerEpisode)
        {
            Debug.Log("Episode timeout reached");
            EndEpisode();
            return;
        }

        // Calculate progress reward
        if (currentCheckpoint < checkpoints.Length && checkpoints[currentCheckpoint] != null)
        {
            float currentDistance = Vector3.Distance(transform.position, checkpoints[currentCheckpoint].position);
            if (currentDistance < lastDistanceToCheckpoint)
            {
                float progress = (lastDistanceToCheckpoint - currentDistance);
                AddReward(progressReward * progress);
                Debug.Log($"Progress reward: {progressReward * progress:F3} (Distance: {currentDistance:F1}m)");
            }
            lastDistanceToCheckpoint = currentDistance;
        }

        // Small negative reward for each step to encourage efficiency
        AddReward(-0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetKey(KeyCode.W) ? -1f : (Input.GetKey(KeyCode.S) ? 1f : 0f);
        continuousActionsOut[1] = Input.GetKey(KeyCode.A) ? -1f : (Input.GetKey(KeyCode.D) ? 1f : 0f);
        continuousActionsOut[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log($"Collision with wall at checkpoint {currentCheckpoint}");
            AddReward(-crashPenalty);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Brakingpoint"))
        {
            isInBrakingZone = true;
            Debug.Log($"Entered braking zone at checkpoint {currentCheckpoint}");
            AddReward(brakingZoneReward);
        }

        if (other.CompareTag("Waypoint"))
        {
            int checkpointIndex = System.Array.IndexOf(checkpoints, other.transform);
            if (checkpointIndex == currentCheckpoint)
            {
                Debug.Log($"Reached checkpoint {currentCheckpoint}");
                
                // Update highest checkpoint reached
                if (checkpointIndex > highestCheckpointReached)
                {
                    highestCheckpointReached = checkpointIndex;
                    // Bonus reward for reaching a new furthest checkpoint
                    AddReward(checkpointReward * 1.5f);
                }
                else
                {
                    AddReward(checkpointReward);
                }

                currentCheckpoint++;
                lastCheckpointTime = Time.time;
                lastDistanceToCheckpoint = float.MaxValue;

                if (currentCheckpoint >= checkpoints.Length)
                {
                    Debug.Log("Completed all checkpoints!");
                    AddReward(completionReward);
                    EndEpisode();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Brakingpoint"))
        {
            isInBrakingZone = false;
            Debug.Log($"Exited braking zone at checkpoint {currentCheckpoint}");
        }
    }
} 