using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	private Player player;

	private void Start () {
		player = FindObjectOfType <Player> ();
	}

	private void Update () {
		Vector3 temp = transform.position;
		temp.x = player.transform.position.x;
		transform.position = temp;
	}
}
