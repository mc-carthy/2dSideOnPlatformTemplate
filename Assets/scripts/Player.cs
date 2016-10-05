using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {

	private Controller2D controller;

	private Vector3 velocity;
	private Vector2 wallJumpClimb = new Vector2(7.5f, 16);
	private Vector2 wallJumpOff = new Vector2(8.5f, 7);
	private Vector2 wallJumpLeap = new Vector2(18, 17);

	[SerializeField]
	private float maxJumpHeight = 4; // In world units
	[SerializeField]
	private float minJumpHeight = 1; // In world units
	[SerializeField]
	private float timeToJumpApex = 0.4f; // In seconds
	private float moveSpeed = 6;

	private float gravity;
	private float maxJumpVelocity;
	private float minJumpVelocity;
	private float velocityXSmoothing;
	private float accTimeAir = 0.2f;
	private float accTimeGround = 0.1f;
	private float wallSlideSpeedMax = 3;
	private float wallStickTime = 0.25f;
	private float timeToWallUnstick;

	private void Start () {
		controller = GetComponent<Controller2D> ();
		gravity = -(2 * maxJumpHeight) / (Mathf.Pow (timeToJumpApex, 2));
		maxJumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity)) * minJumpHeight;
	}

	private void Update () {
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		int wallDirX = (controller.collisions.left) ? -1 : 1;

		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accTimeGround : accTimeAir);

		bool isWallSliding = false;

		if (
		(controller.collisions.left || controller.collisions.right) &&
		(!controller.collisions.below) &&
		(velocity.y < 0)) 
		{
			isWallSliding = true;

			if (velocity.y < -wallSlideSpeedMax) {
				velocity.y = -wallSlideSpeedMax;
			}

			if (timeToWallUnstick > 0) {
				velocityXSmoothing = 0;
				velocity.x = 0;
				if (Mathf.Sign (input.x) != wallDirX && input.x != 0) {
					timeToWallUnstick -= Time.deltaTime;
				} else {
					timeToWallUnstick = wallStickTime;
				}
			} else {
				timeToWallUnstick = wallStickTime;
			}
		}
			
		if (Input.GetKeyDown (KeyCode.Space)) {
			if (isWallSliding) {
				if (wallDirX == Mathf.Sign(input.x)) {
					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
				} else if (input.x == 0) {
					velocity.x = -wallDirX * wallJumpOff.x;
					velocity.y = wallJumpOff.y;
				} else if (wallDirX == -Mathf.Sign(input.x)) {
					velocity.x = -wallDirX * wallJumpLeap.x;
					velocity.y = wallJumpLeap.y;
				}
			}
			if (controller.collisions.below) {
				velocity.y = maxJumpVelocity;
			}
		}
		if (Input.GetKeyUp (KeyCode.Space)) {
			if (velocity.y > minJumpVelocity) {
				velocity.y = minJumpVelocity;
			}
		}

		velocity.y += gravity * Time.deltaTime;

		controller.Move (velocity * Time.deltaTime, input);

		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}
	}
}