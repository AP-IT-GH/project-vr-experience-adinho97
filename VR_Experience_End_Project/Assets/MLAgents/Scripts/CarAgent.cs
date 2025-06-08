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

    [Header("Waypoint Settings")]
    public List<Transform> waypoints;
    public int currentWaypoint = 0;
    public float waypointRange = 5f;
    private float previousDistanceToWaypoint;

    public float maxTimePerEpisode = 60f;
    public float speedRewardMultiplier = 0.15f;
    public float waypointReward = 2f;
    public float crashPenalty = 2f;

    public float brakingZoneSpeedLimit;
    private bool isInsideBraking;

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
        currentWaypoint = 0;
        episodeStartTime = Time.time;
        if (waypoints != null && waypoints.Count > 0)
            previousDistanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypoint].position);
        else
            previousDistanceToWaypoint = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add car's velocity (3)
        sensor.AddObservation(carRigidbody.linearVelocity);
        // Add car's angular velocity (3)
        sensor.AddObservation(carRigidbody.angularVelocity);
        // Add car's rotation (4)
        sensor.AddObservation(transform.rotation);
        // Add distance and direction to next waypoint (1+3)
        if (waypoints != null && waypoints.Count > 0)
        {
            sensor.AddObservation(Vector3.Distance(transform.position, waypoints[currentWaypoint].position));
            sensor.AddObservation(waypoints[currentWaypoint].position - transform.position);
            // Extra observatie: hoek naar waypoint (1)
            Vector3 toWaypoint = (waypoints[currentWaypoint].position - transform.position).normalized;
            float angleToWaypoint = Vector3.SignedAngle(transform.forward, toWaypoint, Vector3.up) / 180f; // tussen -1 en 1
            sensor.AddObservation(angleToWaypoint);
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
        float throttle = actions.ContinuousActions[0];
        float steering = actions.ContinuousActions[1];
        float brake = actions.ContinuousActions[2];
        carController.SetInputs(throttle, steering, brake);

        // Beloon elke beweging (niet achteruit, om te starten)
        if (throttle > 0)
        {
            AddReward(0.5f);
        }
        else
        {        // Straf stilstand

            AddReward(-0.0005f); // of zet hem tijdelijk uit
        }

        // Richting naar volgend waypoint
        if (waypoints != null && waypoints.Count > 0)
        {
            Vector3 toWaypoint = (waypoints[currentWaypoint].position - transform.position).normalized;
            float speedToWaypoint = Vector3.Dot(carRigidbody.linearVelocity, toWaypoint);
            AddReward(speedToWaypoint * 0.2f); // of zelfs 0.3f tijdelijk

            // Debug: DrawRay naar het volgende waypoint
            Debug.DrawRay(transform.position, waypoints[currentWaypoint].position - transform.position, Color.yellow, 0.1f);

            // Reward voor naderen en halen van waypoint
            float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypoint].position);
            AddReward((previousDistanceToWaypoint - distanceToWaypoint) * 0.2f);
            previousDistanceToWaypoint = distanceToWaypoint;

            if (distanceToWaypoint < waypointRange)
            {
                AddReward(waypointReward);
                currentWaypoint++;
                if (currentWaypoint >= waypoints.Count)
                {
                    AddReward(1.5f); // grote reward voor ronde afmaken
                    EndEpisode();
                }
                else
                {
                    // Straf voor het overslaan van een waypoint
                    previousDistanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypoint].position);
                    AddReward(-1f);
                }
            }

            float angleToWaypoint = Vector3.SignedAngle(transform.forward, toWaypoint, Vector3.up) / 180f;
            AddReward((1f - Mathf.Abs(angleToWaypoint)) * 0.02f); // Extra bonus voor goed insturen
        }

        // Episode timeout
        if (Time.time - episodeStartTime > maxTimePerEpisode)
        {
            EndEpisode();
            return;
        }

        AddReward(carRigidbody.linearVelocity.magnitude * 0.3f); // hogere multiplier

        if (isInsideBraking && carController.speed > brakingZoneSpeedLimit)
        {
            AddReward(-0.1f); // Straf voor te hard rijden in braking zone
        }
        else if (isInsideBraking && carController.speed <= brakingZoneSpeedLimit)
        {
            AddReward(0.05f); // Bonus voor netjes remmen
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
            AddReward(-5f);
          
         
        }
    }
} 