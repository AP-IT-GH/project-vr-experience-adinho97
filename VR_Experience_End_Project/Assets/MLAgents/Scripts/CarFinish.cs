using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarFinish : MonoBehaviour
{
        private void OnTriggerEnter(Collider other)
    {
        // als de speler de finish raakt, laad het menu
        RaceManager raceManager = new RaceManager();
        AiCarController carController = other.GetComponent<AiCarController>();
        int lapCount = carController.lapCount;
        if (other.CompareTag("Player"))
        {   
            if(lapCount >= 2)
            {
                raceManager.LoadMenu();
            }
        }   
    }
}
