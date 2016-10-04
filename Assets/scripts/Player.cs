﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {

	private Controller2D controller;

	private Vector3 velocity;

	[SerializeField]
	private float jumpHeight = 4; // In world units
	[SerializeField]
	private float timeToJumpApex = 0.4f; // In seconds
	private float moveSpeed = 6;

	private float gravity;
	private float jumpVelocity;
	private float velocityXSmoothing;
	private float accTimeAir = 0.2f;
	private float accTimeGround = 0.1f;

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


		float targetVelocityX = input.x * moveSpeed;

		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accTimeGround : accTimeAir);
		velocity.y += gravity * Time.deltaTime;

		controller.Move (velocity * Time.deltaTime);
	}
}
