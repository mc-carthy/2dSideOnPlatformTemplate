using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	private Controller2D target;
	private Vector2 focusAreaSize = new Vector3 (3, 5);
	private float verticalOffset = 1;
	private float lookAheadDistX = 4;
	private float lookSmoothTimeX = 0.5f;
	private float lookSmoothTimeY = 0.2f;

	private FocusArea focusArea;

	private float currentLookAheadX;
	private float targetLookAheadX;
	private float lookAheadDirX;
	private float smoothLookVelocityX;
	private float smoothLookVelocityY;
	private bool lookAheadStopped;

	private void Start () {
		target = FindObjectOfType<Player> ().gameObject.GetComponent<Controller2D>();
		focusArea = new FocusArea (target.collider.bounds, focusAreaSize);
	}

	private void LateUpdate () {
		focusArea.Update (target.collider.bounds);
		Vector2 focusPosition = focusArea.centre + Vector2.up * verticalOffset;

		if (focusArea.velocity.x != 0) {
			lookAheadDirX = Mathf.Sign (focusArea.velocity.x);
			if (Mathf.Sign (target.playerInput.x) == Mathf.Sign (focusArea.velocity.x) && target.playerInput.x != 0) {
				lookAheadStopped = false;
				targetLookAheadX = lookAheadDirX * lookAheadDistX;
			} else {
				if (!lookAheadStopped) {
					targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDistX - currentLookAheadX) / 4;
					lookAheadStopped = true;
				}
			}
		}

		currentLookAheadX = Mathf.SmoothDamp (currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);

		focusPosition.y = Mathf.SmoothDamp (transform.position.y, focusPosition.y, ref smoothLookVelocityY, lookSmoothTimeY);
		focusPosition += Vector2.right * currentLookAheadX;

		transform.position = (Vector3)focusPosition + Vector3.forward * -10;
	}

	private void OnDrawGizmos () {
		Gizmos.color = new Color (1, 0, 0, 0.5f);
		Gizmos.DrawCube (focusArea.centre, focusAreaSize);
	}

	private struct FocusArea {
		public Vector2 centre;
		public Vector2 velocity;
		public float left, right;
		public float bottom, top;

		public FocusArea(Bounds targetsBounds, Vector2 size) {
			left = targetsBounds.center.x - size.x / 2;
			right = targetsBounds.center.x + size.x / 2;
			bottom = targetsBounds.min.y;
			top = targetsBounds.min.y + size.y;

			velocity = Vector2.zero;
			centre = new Vector2 ((left + right) / 2, (bottom + top) / 2);
		}

		public void Update (Bounds targetBounds) {
			float shiftX = 0;
			if (targetBounds.min.x < left) {
				shiftX = targetBounds.min.x - left;
			} else if (targetBounds.max.x > right) {
				shiftX = targetBounds.max.x - right;
			}
			left += shiftX;
			right += shiftX;

			float shiftY = 0;
			if (targetBounds.min.y < bottom) {
				shiftY = targetBounds.min.y - bottom;
			} else if (targetBounds.max.y > top) {
				shiftY = targetBounds.max.y - top;
			}
			bottom += shiftY;
			top += shiftY;

			centre = new Vector2 ((left + right) / 2, (bottom + top) / 2);

			velocity = new Vector2 (shiftX, shiftY);
		}
	}
}
