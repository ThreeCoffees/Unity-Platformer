using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
	private Rigidbody2D playerInBounds = null;
	[SerializeField] private bool isRight = false;
	private float currentHandleAngle = 0;

	[SerializeField] private GameObject HandleAnchor;
	[SerializeField] private UnityEvent<bool> OnLeverPulled = new UnityEvent<bool>();

    void Update()
    {
		if (playerInBounds != null) {
			bool prevIsRight = isRight;
			Vector2 playerVelLocal = transform.InverseTransformDirection(playerInBounds.velocity);
			if(Mathf.Abs(playerVelLocal.x) > 3f) isRight = playerVelLocal.x > 0;
			if(prevIsRight != isRight) OnLeverPulled.Invoke(isRight);

			Debug.Log("Player velocity: " + playerVelLocal.x);
		}

		float targetAngle = isRight ? -90 : 0;
		currentHandleAngle = Mathf.Sign(targetAngle - currentHandleAngle) * 500 * Time.deltaTime + currentHandleAngle;
		currentHandleAngle = Mathf.Clamp(currentHandleAngle, -90, 0);

        HandleAnchor.transform.localRotation = Quaternion.Euler(0, 0, currentHandleAngle);
    }

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			playerInBounds = 
				other.gameObject
					.transform.parent.gameObject // needed because of colliders nesting in the player prefab
					.transform.parent.gameObject
					.GetComponent<Rigidbody2D>();
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			playerInBounds = null;
		}
	}
}
