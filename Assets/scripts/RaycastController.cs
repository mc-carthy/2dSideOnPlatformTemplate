using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {

	protected const float skinWidth = 0.05f;

	[SerializeField]
	protected LayerMask collisionMask;

	protected int horizontalRayCount = 4;
	protected int verticalRayCount = 4;

	protected RaycastOrigins raycastOrigins;
	public BoxCollider2D collider;

	protected float horizontalRaySpacing;
	protected float verticalRaySpacing;

	private void Awake () {
		collider = GetComponent<BoxCollider2D> ();
	}

	protected virtual void Start () {
		CalculateRaySpacing ();
	}

	protected void UpdateRaycastOrigins () {
		Bounds bounds = collider.bounds;
		// Reduce the bounds so that they are inside the object by a distance of skin width
		bounds.Expand (skinWidth * -2);
		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	protected void CalculateRaySpacing () {
		Bounds bounds = collider.bounds;
		// Reduce the bounds so that they are inside the object by a distance of skin width
		bounds.Expand (skinWidth * -2);

		// Ensure there are at least 2 rays created both vertically and horizontally
		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	protected struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
