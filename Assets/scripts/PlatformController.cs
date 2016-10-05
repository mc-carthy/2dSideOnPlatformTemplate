using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController {

	[SerializeField]
	private LayerMask passengerMask;
	public Vector3 move;

	protected override void Start () {
		base.Start();
	}

	private void Update () {
		UpdateRaycastOrigins ();
		Vector3 velocity = move * Time.deltaTime;
		MovePassengers (velocity);
		transform.Translate (velocity);
	}

	private void MovePassengers (Vector3 velocity) {
		HashSet<Transform> movedPassengers = new HashSet<Transform> ();

		float dirX = Mathf.Sign (velocity.x);
		float dirY = Mathf.Sign (velocity.y);

		// Vertically moving platform
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
						hit.transform.Translate (new Vector3 (pushX, pushY));
					}
				}
			}
		}

		// Horizontally moving platform
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
						hit.transform.Translate (new Vector3 (pushX, pushY));
					}
				}
			}
		}
	}
}
