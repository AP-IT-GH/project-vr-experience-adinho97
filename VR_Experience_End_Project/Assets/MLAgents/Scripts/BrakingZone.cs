using UnityEngine;

public class BrakingZone : MonoBehaviour
{
private void Start()
{
// Verify setup
Collider col = GetComponent<Collider>();
if (col == null)
{
Debug.LogError("BrakingZone: No Collider component found!");
}
else if (!col.isTrigger)
{
Debug.LogError("BrakingZone: Collider is not set to trigger!");
}

if (gameObject.tag != "Brakingpoint")
{
Debug.LogError("BrakingZone: GameObject is not tagged as 'Brakingpoint'!");
}

Debug.Log($"BrakingZone initialized at position: {transform.position}");
}

private void OnTriggerEnter(Collider other)
{
Debug.Log($"BrakingZone: Trigger entered by {other.gameObject.name} (Tag: {other.tag})");

// Check for AI Car
AiCarController aiCar = other.GetComponent<AiCarController>();
if (aiCar)
{
aiCar.isInsideBraking = true;
Debug.Log($"BrakingZone: AI Car {aiCar.name} entered braking zone");
// Force brake input
if (aiCar.carController != null)
{
aiCar.carController.SetInputs(0f, 0f, 1f);
Debug.Log("BrakingZone: Applied brakes to AI Car");
}
else
{
Debug.LogError($"BrakingZone: AI Car {aiCar.name} has no carController!");
}
}

// Check for ML-Agent Car
CarAgent agentCar = other.GetComponent<CarAgent>();
if (agentCar)
{
agentCar.isInBrakingZone = true;
Debug.Log($"BrakingZone: Agent Car {agentCar.name} entered braking zone");
// Force brake input
if (agentCar.carController != null)
{
agentCar.carController.SetInputs(0f, 0f, 1f);
Debug.Log("BrakingZone: Applied brakes to Agent Car");
}
else
{
Debug.LogError($"BrakingZone: Agent Car {agentCar.name} has no carController!");
}
}
}

private void OnTriggerExit(Collider other)
{
Debug.Log($"BrakingZone: Trigger exited by {other.gameObject.name} (Tag: {other.tag})");

// Check for AI Car
AiCarController aiCar = other.GetComponent<AiCarController>();
if (aiCar)
{
aiCar.isInsideBraking = false;
Debug.Log($"BrakingZone: AI Car {aiCar.name} exited braking zone");
// Reset brake input
if (aiCar.carController != null)
{
aiCar.carController.SetInputs(0f, 0f, 0f);
Debug.Log("BrakingZone: Released brakes on AI Car");
}
}

// Check for ML-Agent Car
CarAgent agentCar = other.GetComponent<CarAgent>();
if (agentCar)
{
agentCar.isInBrakingZone = false;
Debug.Log($"BrakingZone: Agent Car {agentCar.name} exited braking zone");
// Reset brake input
if (agentCar.carController != null)
{
agentCar.carController.SetInputs(0f, 0f, 0f);
Debug.Log("BrakingZone: Released brakes on Agent Car");
}
}
}
}