using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {

	private Controller2D controller;

	private float gravity = -20;
	private Vector3 velocity;

	private void Start () {
		controller = GetComponent<Controller2D> ();
	}

	private void Update () {
		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime);
	}
}
