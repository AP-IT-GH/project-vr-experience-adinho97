using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class CarAgent : Agent
{
    public SimpleCarController carController;
    public Rigidbody carRigidbody;
    public Vector3 startPosition;
    public Quaternion startRotation;
    public float episodeStartTime;

    [Header("Checkpoint Settings")]
    public Transform[] checkpoints; // Handmatig vullen in Inspector
    public Transform checkpointContainer; // (optioneel, niet meer gebruikt)
    public int currentCheckpoint = 0;
    public float checkpointRange = 5f;
    private float previousDistanceToCheckpoint;

    public float maxTimePerEpisode = 60f;
    public float speedRewardMultiplier = 0.15f;
    public float checkpointReward = 2f;
    public float crashPenalty = 2f;

    public float brakingZoneSpeedLimit;
    public bool isInsideBraking;
    public bool hitTheWall;
    public bool hitCheckpoint;
    private float brakingSteerDirection = 0f;

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
        currentCheckpoint = 0;
        episodeStartTime = Time.time;
        if (checkpoints != null && checkpoints.Length > 0)
            previousDistanceToCheckpoint = Vector3.Distance(transform.position, checkpoints[currentCheckpoint].position);
        else
            previousDistanceToCheckpoint = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add car's velocity (3)
        sensor.AddObservation(carRigidbody.linearVelocity);
        // Add car's angular velocity (3)
        sensor.AddObservation(carRigidbody.angularVelocity);
        // Add car's rotation (4)
        sensor.AddObservation(transform.rotation);
        // Add distance and direction to next checkpoint (1+3)
        if (checkpoints != null && checkpoints.Length > 0)
        {
            sensor.AddObservation(Vector3.Distance(transform.position, checkpoints[currentCheckpoint].position));
            sensor.AddObservation(checkpoints[currentCheckpoint].position - transform.position);
            // Extra observatie: hoek naar checkpoint (1)
            Vector3 toCheckpoint = (checkpoints[currentCheckpoint].position - transform.position).normalized;
            float angleToCheckpoint = Vector3.SignedAngle(transform.forward, toCheckpoint, Vector3.up) / 180f; // tussen -1 en 1
            sensor.AddObservation(angleToCheckpoint);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float throttle = 0f;
        float steering = 0f;
        float brake = 0f;

        // Discrete actions: 0 = niks, 1 = vooruit, 2 = achteruit
        switch (actions.DiscreteActions[0])
        {
            case 0: throttle = 0f; break;
            case 1: throttle = 1f; break;
            // case 2: throttle = -1f; break;
        }

        if (isInsideBraking)
        {

            switch (actions.DiscreteActions[1])
            {
                case 0:
                    steering = 0f;
                    AddReward(-0.001f);
                    break;
                case 1:
                    steering = 1f;
                    if (hitCheckpoint)
                    {
                        AddReward(0.2f);
                    }
                    else
                    {
                        AddReward(-0.2f);
                    }
                    break;
                case 2:
                    steering = -1f;
                    if (hitCheckpoint)
                    {
                        AddReward(0.2f);
                    }
                    else
                    {
                        AddReward(-0.2f);
                    }
                    break;
            }
            // Reset hitCheckpoint na gebruik
            hitCheckpoint = false;

            

        }
        else
        {
            steering = 0f;
        }

        carController.SetInputs(throttle, steering, brake);

        // Progress rewards buiten braking zone
        if (!isInsideBraking && checkpoints != null && checkpoints.Length > 0 && currentCheckpoint < checkpoints.Length)
        {
            float distanceToCheckpoint = Vector3.Distance(transform.position, checkpoints[currentCheckpoint].position);
            float progress = previousDistanceToCheckpoint - distanceToCheckpoint;
            AddReward(progress * 0.1f);
            previousDistanceToCheckpoint = distanceToCheckpoint;
        }

        // Episode timeout
        if (Time.time - episodeStartTime > maxTimePerEpisode)
        {
            EndEpisode();
            return;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        // Forward
        if (Input.GetKey(KeyCode.W)) discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.S)) discreteActionsOut[0] = 2;
        else discreteActionsOut[0] = 0;

        // Steering
        if (Input.GetKey(KeyCode.D)) discreteActionsOut[1] = 1;
        else if (Input.GetKey(KeyCode.A)) discreteActionsOut[1] = 2;
        else discreteActionsOut[1] = 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Penalize collisions
        if (collision.gameObject.CompareTag("Wall") )
        {
            AddReward(-5f);
            hitTheWall = true;
        } else {
            hitTheWall = false;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entered: {other.name} with tag: {other.tag}");
        
        // Check if we hit a checkpoint
        if (other.CompareTag("Checkpoint"))
        {
            if (Vector3.Distance(transform.position, other.transform.position) < 10f)
            {
                hitCheckpoint = true;
            }
            else
            {
                hitCheckpoint = false;
            }
          
            Debug.Log($"Checkpoint hit: {other.name}");
            int checkpointIndex = System.Array.IndexOf(checkpoints, other.transform);
            Debug.Log($"Checkpoint index: {checkpointIndex}, Current checkpoint: {currentCheckpoint}");
            
            if (checkpointIndex == currentCheckpoint)
            {
                // Correct checkpoint
                Debug.Log($"Correct checkpoint reached! Reward: {checkpointReward}");
                AddReward(checkpointReward);
                currentCheckpoint++;
                // Zet de distance voor de volgende checkpoint
                if (currentCheckpoint < checkpoints.Length)
                {
                    previousDistanceToCheckpoint = Vector3.Distance(transform.position, checkpoints[currentCheckpoint].position);
                }
                if (currentCheckpoint >= checkpoints.Length)
                {
                    Debug.Log("All checkpoints completed! Episode ending.");
                    AddReward(5f); // grote reward voor ronde afmaken
                    EndEpisode();
                }
            }
            else if (checkpointIndex >= 0)
            {
                // Straf voor het teruggaan naar een vorig of verkeerd checkpoint
                Debug.Log($"Wrong checkpoint! Expected: {currentCheckpoint}, Got: {checkpointIndex}. Penalty applied.");
                AddReward(-1f);
            }
        }
    }

    public void SetBrakingSteerDirection(float dir)
    {
        brakingSteerDirection = dir;
    }
    
} 