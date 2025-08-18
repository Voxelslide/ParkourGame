using System;
using UnityEngine;

public class LedgeDetection : MonoBehaviour
{
	[SerializeField] private bool onLedge;
	[SerializeField] private Vector3 grabLocation;

	[Header("Settings")]
	[Tooltip("Max angle from horizontal (up) to allow grabbing")]
	[SerializeField] private float maxGrabAngle = 40f;

	[Tooltip("Ray length for normal check")]
	[SerializeField] private float rayLength = 1f;

	[Header("Debug")]
	[Tooltip("Draw the downward ray in Scene view for debugging")]
	[SerializeField] private bool drawDebugRay = true;

	private int grabbableLayer;

	private void Awake()
	{
		grabbableLayer = LayerMask.NameToLayer("Grabbable");
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == grabbableLayer)
		{
			CheckLedgeSurface(other);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.layer == grabbableLayer)
		{
			CheckLedgeSurface(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == grabbableLayer)
		{
			onLedge = false;
		}
	}

	private void CheckLedgeSurface(Collider collider)
	{
		// Use the closest point on the other collider to the trigger to start the ray
		Vector3 closestPoint = collider.ClosestPoint(transform.position);
		Vector3 rayStart = closestPoint + Vector3.up * 0.01f; // small offset to avoid being exactly on surface

		Ray ray = new Ray(rayStart, Vector3.down);

		if (drawDebugRay)
		{
			Debug.DrawRay(rayStart, Vector3.down * rayLength, Color.green);
		}

		if (collider.Raycast(ray, out RaycastHit hit, rayLength))
		{
			float angle = Vector3.Angle(hit.normal, Vector3.up);

			if (drawDebugRay)
			{
				// Visualize the hit normal
				Debug.DrawRay(hit.point, hit.normal * 0.5f, Color.red);
			}

			if (angle <= maxGrabAngle)
			{
				onLedge = true;
				grabLocation = hit.point;
				return;
			}
		}

		onLedge = false;
	}

	public bool ReturnOnLedge() => onLedge;
	public Vector3 ReturnGrabLocation() => grabLocation;
}
