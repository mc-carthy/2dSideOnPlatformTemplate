using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {

	private Controller2D controller;

	private Vector3 velocity;

	private float jumpHeight = 4; // In world units
	private float timeToJumpApex = 0.4f; // In seconds
	private float moveSpeed = 6;

	private float gravity;
	private float jumpVelocity;

	private void Start () {
		controller = GetComponent<Controller2D> ();
		gravity = -(2 * jumpHeight) / (Mathf.Pow (timeToJumpApex, 2));
		jumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
	}

	private void Update () {

		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}

		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

		if (Input.GetKeyDown (KeyCode.Space) && controller.collisions.below) {
			velocity.y = jumpVelocity;
		}

		velocity.x = input.x * moveSpeed;
		velocity.y += gravity * Time.deltaTime;

		controller.Move (velocity * Time.deltaTime);
	}
}
