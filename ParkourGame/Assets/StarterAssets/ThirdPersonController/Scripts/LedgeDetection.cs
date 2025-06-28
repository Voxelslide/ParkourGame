using System;
using UnityEngine;

public class LedgeDetection : MonoBehaviour
{
	[SerializeField]
	private bool OnLedge;

	[SerializeField]
	private Vector3 GrabLocation;

	private void OnTriggerEnter(Collider other)
	{
		//Debug.Log("OnTriggerEnter");
		// This method is called when a trigger collider enters this trigger.
		if (other.CompareTag("Ledge"))
		{
			Debug.Log("Hit Ledge");
			OnLedge = true;
			GrabLocation = other.ClosestPointOnBounds(transform.position);
		}
		else OnLedge = false;
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Ledge"))
		{
			Debug.Log("Left Ledge");
			OnLedge = false;
		}
	}


	public bool ReturnOnLedge()
	{
		return OnLedge;
	}

	public Vector3 ReturnGrabLocation()
	{
		return GrabLocation;
	}


}
