using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	private const float skinWidth = 0.05f;

	public LayerMask collisionMask;

	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	private BoxCollider2D collider;

	public CollisionInfo collisions;
	private RaycastOrigins raycastOrigins;

	private float horizontalRaySpacing;
	private float verticalRaySpacing;

	private float maxClimbAngle = 60;

	private void Start () {
		collider = GetComponent<BoxCollider2D> ();
		CalculateRaySpacing ();
	}

	public void Move (Vector3 velocity) {
		UpdateRaycastOrigins ();
		collisions.Reset ();
		if (velocity.x != 0) {
			HorizontalCollisions (ref velocity);
		}
		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}
		transform.Translate (velocity);
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
		float dirX = Mathf.Sign (velocity.x);
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;
		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * dirX, rayLength, collisionMask);
			Debug.DrawRay (rayOrigin, Vector2.right * dirX * rayLength, Color.red);

			if (hit) {

				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);

				if (i == 0 && slopeAngle <= maxClimbAngle) {
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

	private void UpdateRaycastOrigins () {
		Bounds bounds = collider.bounds;
		// Reduce the bounds so that they are inside the object by a distance of skin width
		bounds.Expand (skinWidth * -2);
		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	private void CalculateRaySpacing () {
		Bounds bounds = collider.bounds;
		// Reduce the bounds so that they are inside the object by a distance of skin width
		bounds.Expand (skinWidth * -2);

		// Ensure there are at least 2 rays created both vertically and horizontally
		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	private struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}

	public struct CollisionInfo {
		public bool above;
		public bool below;
		public bool left;
		public bool right;

		public bool climbingSlope;
		public float slopeAngle, slopeAngleOld;

		public void Reset () {
			above = below = false;
			left = right = false;
			climbingSlope = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}