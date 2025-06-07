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

    [Header("Training Settings")]
    public Transform[] checkpoints;
    public float maxTimePerEpisode = 60f;
    public float checkpointTimeout = 10f;
    public float speedRewardMultiplier = 0.1f;
    public float checkpointReward = 1f;
    public float crashPenalty = 0.5f;

    public override void Initialize()
    {
        carController = GetComponent<SimpleCarController>();
        carRigidbody = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public override void OnEpisodeBegin()
    {
        // Reset car position and rotation
        transform.position = startPosition;
        transform.rotation = startRotation;
        carRigidbody.linearVelocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;
        
        // Reset training variables
        currentCheckpoint = 0;
        lastCheckpointTime = Time.time;
        episodeStartTime = Time.time;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add car's velocity
        sensor.AddObservation(carRigidbody.linearVelocity);
        
        // Add car's angular velocity
        sensor.AddObservation(carRigidbody.angularVelocity);
        
        // Add car's rotation
        sensor.AddObservation(transform.rotation);
        
        // Add distance to next checkpoint
        if (currentCheckpoint < checkpoints.Length)
        {
            sensor.AddObservation(Vector3.Distance(transform.position, checkpoints[currentCheckpoint].position));
            sensor.AddObservation(checkpoints[currentCheckpoint].position - transform.position);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(Vector3.zero);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float throttle = actions.ContinuousActions[0];
        float steering = actions.ContinuousActions[1];
        float brake = actions.ContinuousActions[2];

        carController.SetInputs(throttle, steering, brake);

        AddReward(carRigidbody.linearVelocity.magnitude * 0.01f);
        AddReward(-0.001f);

        // Episode timeout
        if (Time.time - episodeStartTime > maxTimePerEpisode)
        {
            EndEpisode();
            return;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        // Throttle: W/S
        continuousActionsOut[0] = Input.GetKey(KeyCode.W) ? 1f : (Input.GetKey(KeyCode.S) ? -1f : 0f);
        // Steering: A/D
        continuousActionsOut[1] = Input.GetKey(KeyCode.A) ? -1f : (Input.GetKey(KeyCode.D) ? 1f : 0f);
        // Brake: Spatie
        continuousActionsOut[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Penalize collisions
        if (collision.gameObject.CompareTag("Wall") )
        {
            AddReward(-crashPenalty);
            // EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if we hit a checkpoint
        if (other.CompareTag("Checkpoint"))
        {
            int checkpointIndex = System.Array.IndexOf(checkpoints, other.transform);
            if (checkpointIndex == currentCheckpoint)
            {
                // Correct checkpoint
                AddReward(checkpointReward);
                currentCheckpoint++;
                lastCheckpointTime = Time.time;

                // If we completed all checkpoints, end episode with success
                if (currentCheckpoint >= checkpoints.Length)
                {
                    AddReward(5f); // Big reward for completing the track
                    EndEpisode();
                }
            }
        }
    }
} 