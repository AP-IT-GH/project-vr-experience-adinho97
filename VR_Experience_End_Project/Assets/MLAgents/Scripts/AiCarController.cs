using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(SimpleCarController))]
public class AiCarController : MonoBehaviour {
    public WaypointContainer waypointContainer;
    public List<Transform> waypoints;
    public int currentWaypoint;
    public float waypointRange;
    private SimpleCarController carController;
    private float currentAngle;
    private float gasInput;
    public float gasDampen;
    public bool isInsideBraking;
    public float maximumAngle = 45f;
    public float maximumSpeed = 120f;

	[Range(0.01f, 0.04f)]
	public float turningConstant = 0.02f;


    public void Start() {
       
        carController = GetComponent<SimpleCarController>();
         waypoints = waypointContainer.waypoints;
        currentWaypoint = 0;
    }

    public void Update() {
        if (waypoints.Count > 0) {
            if (Vector3.Distance(transform.position, waypoints[currentWaypoint].position) < waypointRange) {
                currentWaypoint++;
                if (currentWaypoint >= waypoints.Count) currentWaypoint = 0;
            }
            Vector3 fwd = transform.TransformDirection(Vector3.forward);
            currentAngle = Vector3.SignedAngle(fwd, waypoints[currentWaypoint].position - transform.position, Vector3.up);
            gasInput = Mathf.Clamp01((1f - Mathf.Abs(carController.speed * 0.02f * currentAngle / maximumAngle)));

            float brakeInput = 0f;
            if (isInsideBraking) {
                float gewensteRemSnelheid = 20f; // pas aan naar wens
                brakeInput = Mathf.Clamp01(carController.speed / gewensteRemSnelheid);
                gasInput = 0f;
            }
            // Buiten braking zone: brakeInput blijft 0, gasInput wordt weer berekend

            gasDampen = Mathf.Lerp(gasDampen, gasInput, Time.deltaTime * 3f);
            carController.SetInputs(gasDampen, currentAngle, brakeInput);
            Debug.DrawRay(transform.position, waypoints[currentWaypoint].position - transform.position, Color.yellow);
        }
    }
}