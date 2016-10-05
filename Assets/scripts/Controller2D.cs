using UnityEngine;
using System.Collections;

public class Controller2D : RaycastController {

	public CollisionInfo collisions;

	private float maxSlopeAngle = 60;

	[HideInInspector]
	public Vector2 playerInput;

	protected override void Start () {
		base.Start ();
		collisions.faceDir = 1;
	}

	public void Move (Vector2 moveDist, bool isStandingOnPlatform = false) {
		Move (moveDist, Vector2.zero, isStandingOnPlatform);
	}

	public void Move (Vector2 moveDist, Vector2 input, bool isStandingOnPlatform = false) {
		UpdateRaycastOrigins ();
		collisions.Reset ();

		collisions.moveDistOld = moveDist;

		playerInput = input;

		if (moveDist.y < 0) {
			DescendSlope (ref moveDist);
		}

		if (moveDist.x != 0) {
			collisions.faceDir = (int)Mathf.Sign (moveDist.x);
		}

		HorizontalCollisions (ref moveDist);

		if (moveDist.y != 0) {
			VerticalCollisions (ref moveDist);
		}
		transform.Translate (moveDist);

		if (isStandingOnPlatform) {
			collisions.below = true;
		}
	}

	private void VerticalCollisions (ref Vector2 moveDist) {
		float dirY = Mathf.Sign (moveDist.y);
		float rayLength = Mathf.Abs (moveDist.y) + skinWidth;
		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = (dirY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + moveDist.x);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * dirY, rayLength, collisionMask);
			Debug.DrawRay (rayOrigin, Vector2.up * dirY, Color.red);

			if (hit) {
				if (hit.collider.tag == "through") {
					if (dirY == 1 || hit.distance == 0) {
						continue;
					}
					if (collisions.fallingThroughPlatform) {
						continue;
					}
					if (Mathf.Sign (playerInput.y) == -1) {
						collisions.fallingThroughPlatform = true;
						Invoke ("ResetFallingThroughPlatform", 0.5f);
						continue;
					}
				}
				moveDist.y = (hit.distance - skinWidth) * dirY;
				rayLength = hit.distance;

				if (collisions.climbingSlope) {
					moveDist.x = moveDist.y / Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign (moveDist.x);
				}

				collisions.below = dirY == -1;
				collisions.above = dirY == 1;			
			}
		}

		if (collisions.climbingSlope) {
			float dirX = Mathf.Sign (moveDist.x);
			rayLength = Mathf.Abs (moveDist.x) + skinWidth;
			Vector2 rayOrigin = ((dirX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveDist.y;
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * dirX, rayLength, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
				if (slopeAngle != collisions.slopeAngle) {
					moveDist.x = (hit.distance - skinWidth) * dirX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}

	private void HorizontalCollisions (ref Vector2 moveDist) {
		float dirX = collisions.faceDir;
		float rayLength = Mathf.Abs (moveDist.x) + skinWidth;

		if (Mathf.Abs(moveDist.x) < skinWidth) {
			rayLength = 2 * skinWidth;
		}

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * dirX, rayLength, collisionMask);
			Debug.DrawRay (rayOrigin, Vector2.right * dirX, Color.red);

			if (hit) {

				// If we're "inside the platform" (as if it has passed from above us), let the horiztonal movement continue as normal so as not to slow it down
				if (hit.distance == 0) {
					continue;
				}

				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);

				if (i == 0 && slopeAngle <= maxSlopeAngle) {
					if (collisions.descendingSlope) {
						collisions.descendingSlope = false;
						moveDist = collisions.moveDistOld;
					}
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hit.distance - skinWidth;
						moveDist.x -= distanceToSlopeStart * dirX;
					}
					ClimbSlope (ref moveDist, slopeAngle);
					moveDist.x += distanceToSlopeStart * dirX;
				}

				if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle) {
					moveDist.x = (hit.distance - skinWidth) * dirX;
					rayLength = hit.distance;

					if (collisions.climbingSlope) {
						moveDist.y = Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (moveDist.x);
					}

					collisions.left = dirX == -1;
					collisions.right = dirX == 1;
				}
			}
		}
	}

	private void ClimbSlope (ref Vector2 moveDist, float slopeAngle) {

		float moveDistance = Mathf.Abs (moveDist.x);

		float climbVelY = moveDistance * Mathf.Sin (slopeAngle * Mathf.Deg2Rad);

		if (moveDist.y <= climbVelY) {
			moveDist.y = climbVelY;
			moveDist.x = moveDistance * Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * Mathf.Sign (moveDist.x);
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}

	}

	private void DescendSlope (ref Vector2 moveDist) {

		RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast (raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs (moveDist.y) + skinWidth, collisionMask);
		RaycastHit2D maxSlopeHitRight = Physics2D.Raycast (raycastOrigins.bottomRight, Vector2.down, Mathf.Abs (moveDist.y) + skinWidth, collisionMask);

		SlideDownMaxSlope (maxSlopeHitLeft, ref moveDist);
		SlideDownMaxSlope (maxSlopeHitRight, ref moveDist);

		if (!collisions.slidingDownMaxSlope) {

		float dirX = Mathf.Sign (moveDist.x);
		Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);

				if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle) {
					// Check if moving down slope
					if (Mathf.Sign (hit.normal.x) == dirX) {
						// Check if we're close enough to the slope to hit it
						if (hit.distance - skinWidth <= Mathf.Tan (slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (moveDist.x)) {
							float moveDistance = Mathf.Abs (moveDist.x);
							float descendVelY = moveDistance * Mathf.Sin (slopeAngle * Mathf.Deg2Rad);
							moveDist.x = moveDistance * Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * Mathf.Sign (moveDist.x);
							moveDist.y -= descendVelY;

							collisions.slopeAngle = slopeAngle;
							collisions.descendingSlope = true;
							collisions.below = true;
						}
					}
				}
			}
		}
	}

	private void SlideDownMaxSlope (RaycastHit2D hit, ref Vector2 moveDist) {

		if (hit) {
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle > maxSlopeAngle) {
				moveDist.x = hit.normal.x * (Mathf.Abs (moveDist.y) - hit.distance) / Mathf.Tan (slopeAngle * Mathf.Deg2Rad);

				collisions.slopeAngle = slopeAngle;
				collisions.slidingDownMaxSlope = true;
			}
		}
	}

	private void ResetFallingThroughPlatform () {
		collisions.fallingThroughPlatform = false;
	}

	public struct CollisionInfo {
		public bool above;
		public bool below;
		public bool left;
		public bool right;

		public bool climbingSlope, descendingSlope;
		public bool slidingDownMaxSlope;
		public float slopeAngle, slopeAngleOld;

		public int faceDir;

		public bool fallingThroughPlatform;

		public Vector2 moveDistOld;

		public void Reset () {
			above = below = false;
			left = right = false;
			climbingSlope = descendingSlope = false;
			slidingDownMaxSlope = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}