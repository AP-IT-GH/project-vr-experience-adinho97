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
		}

		CarAgent agent = other.GetComponent<CarAgent>();
		if (agent)
		{
			agent.isInsideBraking = true;
			agent.SetBrakingSteerDirection(direction == BrakingDirection.Left ? -1f : 1f);
		}
	}

	public void OnTriggerExit(Collider other)
	{
		AiCarController car = other.GetComponent<AiCarController>();
		if (car)
		{
			car.isInsideBraking = false;
		}

		CarAgent agent = other.GetComponent<CarAgent>();
		if (agent)
		{
			agent.isInsideBraking = false;
			agent.SetBrakingSteerDirection(0f);
		}
	}
}
