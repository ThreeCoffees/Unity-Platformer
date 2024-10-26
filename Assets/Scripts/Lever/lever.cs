using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lever : MonoBehaviour
{
	private GameObject playerInBounds = null;
	private bool isRight = false;
	private float currentHandleAngle = 0;

	[SerializeField] private GameObject HandleAnchor;

	private float TargetAngle() {
		if(playerInBounds == null) {
			return isRight ? -90 : 0;
		}
		else {
			// player position in local coords
			Vector3 playerLocal = transform.InverseTransformPoint(playerInBounds.transform.position);
			
			// where the player is exactly within the lever (0 - left, 1 - right)
			float t = playerLocal.x + 1;
			
			// lerp between -90 and 0
			return -90 * t;
		}
	}

    // Update is called once per frame
    void Update()
    {
		currentHandleAngle = Mathf.Lerp(currentHandleAngle, TargetAngle(), 1 - Mathf.Pow(0.01f, Time.deltaTime));

        HandleAnchor.transform.localRotation = Quaternion.Euler(0, 0, currentHandleAngle);
    }

	private void OnTriggerEnter2D(Collider2D other) {
		Debug.Log(other);
		if (other.CompareTag("Player")) {
			playerInBounds = other.gameObject;
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			Vector3 playerLocal = transform.InverseTransformPoint(playerInBounds.transform.position);
			float t = playerLocal.x + 1;

			isRight = t > 0.5f;

			playerInBounds = null;
		}
	}
}
