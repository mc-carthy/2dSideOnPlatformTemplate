using UnityEngine;
using System.Collections;

public class Controller2D : RaycastController {

	public CollisionInfo collisions;

	private float maxClimbAngle = 60;
	private float maxDescendAngle = 50;

	protected override void Start () {
		base.Start ();
		collisions.faceDir = 1;
	}

	public void Move (Vector3 velocity, bool isStandingOnPlatform = false) {
		UpdateRaycastOrigins ();
		collisions.Reset ();

		collisions.velocityOld = velocity;

		if (velocity.x != 0) {
			collisions.faceDir = (int)Mathf.Sign (velocity.x);
		}

		if (velocity.y < 0) {
			DescendSlope (ref velocity);
		}

		HorizontalCollisions (ref velocity);

		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}
		transform.Translate (velocity);

		if (isStandingOnPlatform) {
			collisions.below = true;
		}
	}

	private void VerticalCollisions (ref Vector3 velocity) {
		float dirY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + skinWidth;
		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = (dirY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * dirY, rayLength, collisionMask);
			Debug.DrawRay (rayOrigin, Vector2.up * dirY * rayLength, Color.red);

			if (hit) {
				if (hit.collider.tag == "through") {
					if (dirY == 1 || hit.distance == 0) {
						continue;
					}
				}
				velocity.y = (hit.distance - skinWidth) * dirY;
				rayLength = hit.distance;

				if (collisions.climbingSlope) {
					velocity.x = velocity.y / Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign (velocity.x);
				}

				collisions.below = dirY == -1;
				collisions.above = dirY == 1;			
			}
		}

		if (collisions.climbingSlope) {
			float dirX = Mathf.Sign (velocity.x);
			rayLength = Mathf.Abs (velocity.x) + skinWidth;
			Vector2 rayOrigin = ((dirX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * dirX, rayLength, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
				if (slopeAngle != collisions.slopeAngle) {
					velocity.x = (hit.distance - skinWidth) * dirX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}

	private void HorizontalCollisions (ref Vector3 velocity) {
		float dirX = collisions.faceDir;
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;

		if (Mathf.Abs(velocity.x) < skinWidth) {
			rayLength = 2 * skinWidth;
		}

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * dirX, rayLength, collisionMask);
			Debug.DrawRay (rayOrigin, Vector2.right * dirX * rayLength, Color.red);

			if (hit) {

				// If we're "inside the platform" (as if it has passed from above us), let the horiztonal movement continue as normal so as not to slow it down
				if (hit.distance == 0) {
					continue;
				}

				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);

				if (i == 0 && slopeAngle <= maxClimbAngle) {
					if (collisions.descendingSlope) {
						collisions.descendingSlope = false;
						velocity = collisions.velocityOld;
					}
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hit.distance - skinWidth;
						velocity.x -= distanceToSlopeStart * dirX;
					}
					ClimbSlope (ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * dirX;
				}

				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
					velocity.x = (hit.distance - skinWidth) * dirX;
					rayLength = hit.distance;

					if (collisions.climbingSlope) {
						velocity.y = Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x);
					}

					collisions.left = dirX == -1;
					collisions.right = dirX == 1;
				}
			}
		}
	}

	private void ClimbSlope (ref Vector3 velocity, float slopeAngle) {

		float moveDistance = Mathf.Abs (velocity.x);

		float climbVelY = moveDistance * Mathf.Sin (slopeAngle * Mathf.Deg2Rad);

		if (velocity.y <= climbVelY) {
			velocity.y = climbVelY;
			velocity.x = moveDistance * Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * Mathf.Sign (velocity.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}

	}

	private void DescendSlope (ref Vector3 velocity) {

		float dirX = Mathf.Sign (velocity.x);
		Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

		if (hit) {
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
				// Check if moving down slope
				if (Mathf.Sign (hit.normal.x) == dirX) {
					// Check if we're close enough to the slope to hit it
					if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) {
						float moveDistance = Mathf.Abs (velocity.x);
						float descendVelY = moveDistance * Mathf.Sin (slopeAngle * Mathf.Deg2Rad);
						velocity.x = moveDistance * Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * Mathf.Sign (velocity.x);
						velocity.y -= descendVelY;

						collisions.slopeAngle = slopeAngle;
						collisions.descendingSlope = true;
						collisions.below = true;
					}
				}
			}
		}

	}

	public struct CollisionInfo {
		public bool above;
		public bool below;
		public bool left;
		public bool right;

		public bool climbingSlope, descendingSlope;
		public float slopeAngle, slopeAngleOld;

		public int faceDir;

		public Vector3 velocityOld;

		public void Reset () {
			above = below = false;
			left = right = false;
			climbingSlope = descendingSlope = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}