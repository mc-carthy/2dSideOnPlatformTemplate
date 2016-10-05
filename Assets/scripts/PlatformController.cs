using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController {

	public float speed;
	public bool isCyclic;
	public float waitTime;
	[Range(0, 2)]
	public float easeAmount;

	private int fromWayPointIndex;
	private float percentBetweenWaypoints;
	private float nextMoveTime;

	private List<PassengerMovement> passengerMovement;
	private Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

	[SerializeField]
	private LayerMask passengerMask;

	public Vector3[] localWaypoints;
	private Vector3[] globalWaypoints;

	protected override void Start () {
		base.Start();

		globalWaypoints = new Vector3[localWaypoints.Length];

		for (int i = 0; i < globalWaypoints.Length; i++) {
			globalWaypoints [i] = localWaypoints [i] + transform.position;
		}

	}

	private void Update () {
		UpdateRaycastOrigins ();
		Vector3 velocity = CalculatePlatformMovement();
		CalculatePassengerMovement (velocity);

		MovePassengers (true);
		transform.Translate (velocity);
		MovePassengers (false);
	}

	private float Ease (float x) {
		float a = easeAmount + 1;
		return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
	}

	private Vector3 CalculatePlatformMovement () {
		if (Time.time < nextMoveTime) {
			return Vector3.zero;
		}
		fromWayPointIndex %= globalWaypoints.Length;
		int toWaypointIndex = ((fromWayPointIndex + 1) % globalWaypoints.Length);
		float distBetweenPoints = Vector3.Distance (globalWaypoints [fromWayPointIndex], globalWaypoints [toWaypointIndex]);
		percentBetweenWaypoints += (speed / distBetweenPoints) * Time.deltaTime;
		percentBetweenWaypoints = Mathf.Clamp01 (percentBetweenWaypoints);
		float easedPercentBetweenWaypoints = Ease (percentBetweenWaypoints);

		Vector3 newPos = Vector3.Lerp (globalWaypoints[fromWayPointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

		if (percentBetweenWaypoints >= 1) {
			percentBetweenWaypoints = 0;
			fromWayPointIndex++;
			if (!isCyclic) {
				if (fromWayPointIndex >= globalWaypoints.Length - 1) {
					fromWayPointIndex = 0;
					System.Array.Reverse (globalWaypoints);
				}
			}
			nextMoveTime = Time.time + waitTime;
		}

		return newPos - transform.position;
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
				Vector2 rayOrigin = (dirX == 1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
				rayOrigin += Vector2.right * (verticalRaySpacing * i * dirX);
				RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * dirX, rayLength, passengerMask);
				Debug.DrawRay (rayOrigin, Vector2.right * dirX * rayLength, Color.red);

				if (hit) {
					Debug.Log (hit.transform.name);
					if (!movedPassengers.Contains(hit.transform)) {
						movedPassengers.Add (hit.transform);
						float pushX = velocity.x - (hit.distance - skinWidth) * dirX;
						float pushY = -skinWidth;
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
			if (!passengerDictionary.ContainsKey (passenger.transform)) {
				passengerDictionary.Add (passenger.transform, passenger.transform.GetComponent<Controller2D> ());
			}
			if (passenger.moveBeforePlatform == beforeMovePlatform) {
				passengerDictionary[passenger.transform].Move (passenger.velocity, passenger.isStandingOnPlatform);
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

	private void OnDrawGizmos () {
		if (localWaypoints != null) {
			Gizmos.color = Color.red;
			float size = 0.3f;

			for (int i = 0; i < localWaypoints.Length; i++) {
				Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints [i] : localWaypoints [i] + transform.position;
				Gizmos.DrawLine (globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
				Gizmos.DrawLine (globalWaypointPos - Vector3.right * size, globalWaypointPos + Vector3.right * size);
			}
		}
	}
}
