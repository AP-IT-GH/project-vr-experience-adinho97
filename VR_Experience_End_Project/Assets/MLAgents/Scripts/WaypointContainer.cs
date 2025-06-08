using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointContainer : MonoBehaviour {

    public List<Transform> waypoints;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        foreach (Transform tr in gameObject.GetComponentsInChildren<Transform>())
        {
          
                waypoints.Add(tr);
            Debug.Log("Waypoint toegevoegd: " + tr.name);

        }
        if (waypoints.Count > 0)
        {
            Debug.Log("Eerste element verwijderd: " + waypoints[0].name);
            waypoints.Remove(waypoints[0]);
        }

        Debug.Log("Totaal aantal waypoints: " + waypoints.Count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
