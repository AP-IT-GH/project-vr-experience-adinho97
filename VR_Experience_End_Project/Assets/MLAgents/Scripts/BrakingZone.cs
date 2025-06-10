using UnityEngine;

public enum BrakingDirection { Left, Right }

public class BrakingZone : MonoBehaviour
{
	public BrakingDirection direction = BrakingDirection.Left;

	public void OnTriggerEnter(Collider other)
	{
		AiCarController car = other.GetComponent<AiCarController>();
		if (car)
		{
			car.isInsideBraking = true;
			var carController = car.GetComponent<SimpleCarController>();
			if (carController != null)
				carController.SetBrakingZoneTorque(true);
		}

		CarAgent agent = other.GetComponent<CarAgent>();
		if (agent)
		{
			agent.isInsideBraking = true;
			agent.SetBrakingSteerDirection(direction == BrakingDirection.Left ? -1f : 1f);
		//	agent.SetBrakingZoneTorque(true);
		}
	}

	public void OnTriggerExit(Collider other)
	{
		AiCarController car = other.GetComponent<AiCarController>();
		if (car)
		{
			car.isInsideBraking = false;
			var carController = car.GetComponent<SimpleCarController>();
			if (carController != null)
				carController.SetBrakingZoneTorque(false);
		}

		CarAgent agent = other.GetComponent<CarAgent>();
		if (agent)
		{
			agent.isInsideBraking = false;
			agent.SetBrakingSteerDirection(0f);
			// agent.SetBrakingZoneTorque(false);
		}
	}
}
