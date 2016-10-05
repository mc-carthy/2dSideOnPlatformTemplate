using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {

	private Controller2D controller;
	private Vector2 directionalInput;

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

	private bool isWallSliding;
	private int wallDirX;

	private void Start () {
		controller = GetComponent<Controller2D> ();
		gravity = -(2 * maxJumpHeight) / (Mathf.Pow (timeToJumpApex, 2));
		maxJumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity)) * minJumpHeight;
	}

	private void Update () {
		CalculateVelocity ();
		HandleWallSliding ();

		controller.Move (velocity * Time.deltaTime, directionalInput);

		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}
	}

	public void SetDirectionalInput (Vector2 input) {
		directionalInput = input;
	}

	public void OnJumpInputDown () {
		if (isWallSliding) {
			if (wallDirX == Mathf.Sign(directionalInput.x)) {
				velocity.x = -wallDirX * wallJumpClimb.x;
				velocity.y = wallJumpClimb.y;
			} else if (directionalInput.x == 0) {
				velocity.x = -wallDirX * wallJumpOff.x;
				velocity.y = wallJumpOff.y;
			} else if (wallDirX == -Mathf.Sign(directionalInput.x)) {
				velocity.x = -wallDirX * wallJumpLeap.x;
				velocity.y = wallJumpLeap.y;
			}
		}
		if (controller.collisions.below) {
			velocity.y = maxJumpVelocity;
		}
	}

	public void OnJumpInputUp () {
		if (velocity.y > minJumpVelocity) {
			velocity.y = minJumpVelocity;
		}
	}

	private void HandleWallSliding () {

		wallDirX = (controller.collisions.left) ? -1 : 1;

		isWallSliding = false;

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
				if (Mathf.Sign (directionalInput.x) != wallDirX && directionalInput.x != 0) {
					timeToWallUnstick -= Time.deltaTime;
				} else {
					timeToWallUnstick = wallStickTime;
				}
			} else {
				timeToWallUnstick = wallStickTime;
			}
		}
	}

	private void CalculateVelocity () {
		float targetVelocityX = directionalInput.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accTimeGround : accTimeAir);
		velocity.y += gravity * Time.deltaTime;
	}
}