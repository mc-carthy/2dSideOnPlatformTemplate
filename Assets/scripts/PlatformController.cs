using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController {

	private List<PassengerMovement> passengerMovement;

	[SerializeField]
	private LayerMask passengerMask;
	public Vector3 move;

	protected override void Start () {
		base.Start();
	}

	private void Update () {
		UpdateRaycastOrigins ();
		Vector3 velocity = move * Time.deltaTime;
		CalculatePassengerMovement (velocity);

		MovePassengers (true);
		transform.Translate (velocity);
		MovePassengers (false);
	}

	private void CalculatePassengerMovement (Vector3 velocity) {
		HashSet<Transform> movedPassengers = new HashSet<Transform> ();
		passengerMovement = new List<PassengerMovement> ();

		float dirX = Mathf.Sign (velocity.x);
		float dirY = Mathf.Sign (velocity.y);

		// Player on top of vertically moving platform pushing player vertically
		if (velocity.y != 0) {
			float rayLength = Mathf.Abs (velocity.y) + skinWidth;
			for (int i = 0; i < verticalRayCount; i++) {
				Vector2 rayOrigin = (dirY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * dirY, rayLength, passengerMask);
				Debug.DrawRay (rayOrigin, Vector2.up * dirY * rayLength, Color.red);

				if (hit) {
					Debug.Log (hit.transform.name);
					if (!movedPassengers.Contains(hit.transform)) {
						movedPassengers.Add (hit.transform);
						float pushX = (dirY == 1) ? velocity.x : 0;
						float pushY = velocity.y - (hit.distance - skinWidth) * dirY;
						passengerMovement.Add(new PassengerMovement(
							hit.transform, 
							new Vector3(pushX, pushY),
							dirY == 1,
							true
						));
					}
				}
			}
		}

		// Player alongside horizontally moving platform pushing player horizontally
		if (velocity.x != 0) {
			float rayLength = Mathf.Abs (velocity.x) + skinWidth;
			for (int i = 0; i < horizontalRayCount; i++) {
				Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * dirX, rayLength, passengerMask);
				Debug.DrawRay (rayOrigin, Vector2.right * dirX * rayLength, Color.red);

				if (hit) {
					Debug.Log (hit.transform.name);
					if (!movedPassengers.Contains(hit.transform)) {
						movedPassengers.Add (hit.transform);
						float pushX = velocity.x - (hit.distance - skinWidth) * dirX;
						float pushY = 0;
						passengerMovement.Add(new PassengerMovement(
							hit.transform, 
							new Vector3(pushX, pushY),
							false,
							true
						));
					}
				}
			}
		}

		// Player on top of horizontally or downward moving platform
		if (dirY == -1 || (velocity.y == 0 && velocity.x != 0)) {
			float rayLength = skinWidth * 2;
			for (int i = 0; i < verticalRayCount; i++) {
				Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up, rayLength, passengerMask);
				Debug.DrawRay (rayOrigin, Vector2.up * rayLength, Color.red);

				if (hit) {
					Debug.Log (hit.transform.name);
					if (!movedPassengers.Contains(hit.transform)) {
						movedPassengers.Add (hit.transform);
						float pushX = velocity.x;
						float pushY = velocity.y;
						passengerMovement.Add(new PassengerMovement(
							hit.transform, 
							new Vector3(pushX, pushY),
							true,
							false
						));
					}
				}
			}
		}
	}

	private void MovePassengers (bool beforeMovePlatform) {
		foreach (PassengerMovement passenger in passengerMovement) {
			if (passenger.moveBeforePlatform == beforeMovePlatform) {
				passenger.transform.GetComponent<Controller2D> ().Move (passenger.velocity, passenger.isStandingOnPlatform);
			}
		}
	}

	private struct PassengerMovement {
		public Transform transform;
		public Vector3 velocity;
		public bool isStandingOnPlatform;
		public bool moveBeforePlatform;

		public PassengerMovement(Transform _transform, Vector3 _velocity, bool _isStandingOnPlatform, bool _moveBeforePlatform) {
			transform = _transform;
			velocity = _velocity;
			isStandingOnPlatform = _isStandingOnPlatform;
			moveBeforePlatform = _moveBeforePlatform;
		}
	}
}
